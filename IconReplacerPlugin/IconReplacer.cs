using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Hooking;
using Serilog;

namespace IconReplacerPlugin
{
    public class IconReplacer
    {
        public delegate ulong OnCheckIsIconReplaceableDelegate(int actionID);

        public delegate ulong OnGetIconDelegate(byte param1, uint param2);

        private IntPtr activeBuffArray = IntPtr.Zero;

        private readonly IconReplacerAddressResolver Address;
        private readonly IntPtr byteBase;
        private readonly Hook<OnCheckIsIconReplaceableDelegate> checkerHook;
        private readonly ClientState clientState;

        private readonly IntPtr comboTimer;

        private readonly IconReplacerConfiguration Configuration;

        private readonly HashSet<uint> customIds;
        private readonly HashSet<uint> vanillaIds;

        private readonly Hook<OnGetIconDelegate> iconHook;
        private IntPtr jobInfo;
        private readonly IntPtr lastComboMove;
        private readonly IntPtr playerLevel;


        public IconReplacer(SigScanner scanner, ClientState clientState, IconReplacerConfiguration configuration)
        {
            Configuration = configuration;
            this.clientState = clientState;

            Address = new IconReplacerAddressResolver();
            Address.Setup(scanner);

            byteBase = scanner.Module.BaseAddress;
            comboTimer = byteBase + 0x1BB8B50;
            //this.comboTimer = scanner.ScanText("E8 ?? ?? ?? ?? 80 7E 21 00") + 0x178;
            lastComboMove = comboTimer + 0x4;

            playerLevel = byteBase + 0x1C30FA8 + 0x78;
            //this.playerLevel = scanner.ScanText("E8 ?? ?? ?? ?? 88 45 EF") + 0x4D;

            customIds = new HashSet<uint>();
            vanillaIds = new HashSet<uint>();

            PopulateDict();

            Log.Verbose("===== H O T B A R S =====");
            Log.Verbose("IsIconReplaceable address {IsIconReplaceable}", Address.IsIconReplaceable);
            Log.Verbose("GetIcon address {GetIcon}", Address.GetIcon);
            Log.Verbose("ComboTimer address {ComboTimer}", comboTimer);
            Log.Verbose("LastComboMove address {LastComboMove}", lastComboMove);
            Log.Verbose("PlayerLevel address {PlayerLevel}", playerLevel);

            iconHook = new Hook<OnGetIconDelegate>(Address.GetIcon, new OnGetIconDelegate(GetIconDetour), this);
            checkerHook = new Hook<OnCheckIsIconReplaceableDelegate>(Address.IsIconReplaceable,
                new OnCheckIsIconReplaceableDelegate(CheckIsIconReplaceableDetour), this);
        }

        public void Enable()
        {
            iconHook.Enable();
            checkerHook.Enable();
        }

        public void Dispose()
        {
            iconHook.Dispose();
            checkerHook.Dispose();
        }

        // I hate this function. This is the dumbest function to exist in the game. Just return 1.
        // Determines which abilities are allowed to have their icons updated.
        private ulong CheckIsIconReplaceableDetour(int actionID)
        {
            return 1;
        }

        /// <summary>
        ///     Replace an ability with another ability
        ///     actionID is the original ability to be "used"
        ///     Return either actionID (itself) or a new Action table ID as the
        ///     ability to take its place.
        ///     I tend to make the "combo chain" button be the last move in the combo
        ///     For example, Souleater combo on DRK happens by dragging Souleater
        ///     onto your bar and mashing it.
        /// </summary>
        private ulong GetIconDetour(byte self, uint actionID)
        {
            // TODO: More jobs, level checking for everything.

            if (vanillaIds.Contains(actionID)) return iconHook.Original(self, actionID);
            if (!customIds.Contains(actionID)) return actionID;
            if (activeBuffArray == IntPtr.Zero)
                try
                {
                    activeBuffArray = FindBuffAddress();
                }
                catch (Exception e)
                {
                    //Before you're loaded in
                    activeBuffArray = IntPtr.Zero;
                    return iconHook.Original(self, actionID);
                }

            // Don't clutter the spaghetti any worse than it already is.
            var lastMove = Marshal.ReadInt32(lastComboMove);
            var comboTime = Marshal.ReadInt32(comboTimer);
            var level = Marshal.ReadByte(playerLevel);

            // DRAGOON

            // Change Jump/High Jump into Mirage Dive when Dive Ready
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonJumpFeature))
                if (actionID == 92)
                {
                    if (SearchBuffArray(1243)) return 7399;
                    if (level >= 74) return 16478;
                    return 92;
                }

            // Change Blood of the Dragon into Stardiver when in Life of the Dragon
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonBOTDFeature))
                if (actionID == 3553)
                {
                    if (level >= 80)
                        if (clientState.JobGauges.Get<DRGGauge>().BOTDState == BOTDState.LOTD)
                            return 16480;
                    return 3553;
                }

            // Replace Coerthan Torment with Coerthan Torment combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonCoerthanTormentCombo))
                if (actionID == 16477)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 86 && level >= 62) return 7397;
                        if (lastMove == 7397 && level >= 72) return 16477;
                    }

                    return 86;
                }


            // Replace Chaos Thrust with the Chaos Thrust combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonChaosThrustCombo))
                if (actionID == 88)
                {
                    if (comboTime > 0)
                    {
                        if ((lastMove == 75 || lastMove == 16479) && level >= 18) return 87;
                        if (lastMove == 87 && level >= 50) return 88;
                    }

                    if (SearchBuffArray(802) && level >= 56) return 3554;
                    if (SearchBuffArray(803) && level >= 58) return 3556;
                    if (SearchBuffArray(1863) && level >= 76) return 16479;

                    return 75;
                }


            // Replace Full Thrust with the Full Thrust combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonFullThrustCombo))
                if (actionID == 84)
                {
                    if (comboTime > 0)
                    {
                        if ((lastMove == 75 || lastMove == 16479) && level >= 4) return 78;
                        if (lastMove == 78 && level >= 26) return 84;
                    }

                    if (SearchBuffArray(802) && level >= 56) return 3554;
                    if (SearchBuffArray(803) && level >= 58) return 3556;
                    if (SearchBuffArray(1863) && level >= 76) return 16479;

                    return 75;
                }

            // DARK KNIGHT

            // Replace Souleater with Souleater combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DarkSouleaterCombo))
                if (actionID == 3632)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 3617 && level >= 2) return 3623;
                        if (lastMove == 3623 && level >= 26) return 3632;
                    }

                    return 3617;
                }

            // Replace Stalwart Soul with Stalwart Soul combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DarkStalwartSoulCombo))
                if (actionID == 16468)
                {
                    if (comboTime > 0)
                        if (lastMove == 3621 && level >= 72)
                            return 16468;

                    return 3621;
                }

            // PALADIN

            // Replace Goring Blade with Goring Blade combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinGoringBladeCombo))
                if (actionID == 3538)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 9 && level >= 4) return 15;
                        if (lastMove == 15 && level >= 54) return 3538;
                    }

                    return 9;
                }

            // Replace Royal Authority with Royal Authority combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinRoyalAuthorityCombo))
                if (actionID == 3539)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 9 && level >= 4) return 15;
                        if (lastMove == 15)
                        {
                            if (level >= 60) return 3539;
                            if (level >= 26) return 21;
                        }
                    }

                    return 9;
                }

            // Replace Prominence with Prominence combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinProminenceCombo))
                if (actionID == 16457)
                {
                    if (comboTime > 0)
                        if (lastMove == 7381 && level >= 40)
                            return 16457;

                    return 7381;
                }

            // WARRIOR

            // Replace Storm's Path with Storm's Path combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorStormsPathCombo))
                if (actionID == 42)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 31 && level >= 4) return 37;
                        if (lastMove == 37 && level >= 26) return 42;
                    }

                    return 31;
                }

            // Replace Storm's Eye with Storm's Eye combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorStormsEyeCombo))
                if (actionID == 45)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 31 && level >= 4) return 37;
                        if (lastMove == 37 && level >= 50) return 45;
                    }

                    return 31;
                }

            // Replace Mythril Tempest with Mythril Tempest combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorMythrilTempestCombo))
                if (actionID == 16462)
                {
                    if (comboTime > 0)
                        if (lastMove == 41 && level >= 40)
                            return 16462;
                    return 41;
                }

            // SAMURAI

            // Replace Yukikaze with Yukikaze combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiYukikazeCombo))
                if (actionID == 7480)
                {
                    if (SearchBuffArray(1233)) return 7480;
                    if (comboTime > 0)
                        if (lastMove == 7477 && level >= 50)
                            return 7480;
                    return 7477;
                }

            // Replace Gekko with Gekko combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiGekkoCombo))
                if (actionID == 7481)
                {
                    if (SearchBuffArray(1233)) return 7481;
                    if (comboTime > 0)
                    {
                        if (lastMove == 7477 && level >= 4) return 7478;
                        if (lastMove == 7478 && level >= 30) return 7481;
                    }

                    return 7477;
                }

            // Replace Kasha with Kasha combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiKashaCombo))
                if (actionID == 7482)
                {
                    if (SearchBuffArray(1233)) return 7482;
                    if (comboTime > 0)
                    {
                        if (lastMove == 7477 && level >= 18) return 7479;
                        if (lastMove == 7479 && level >= 40) return 7482;
                    }

                    return 7477;
                }

            // Replace Mangetsu with Mangetsu combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiMangetsuCombo))
                if (actionID == 7484)
                {
                    if (SearchBuffArray(1233)) return 7484;
                    if (comboTime > 0)
                        if (lastMove == 7483 && level >= 35)
                            return 7484;
                    return 7483;
                }

            // Replace Oka with Oka combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiOkaCombo))
                if (actionID == 7485)
                {
                    if (SearchBuffArray(1233)) return 7485;
                    if (comboTime > 0)
                        if (lastMove == 7483 && level >= 45)
                            return 7485;
                    return 7483;
                }

            // NINJA

            // Replace Armor Crush with Armor Crush combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaArmorCrushCombo))
                if (actionID == 3563)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 2240 && level >= 4) return 2242;
                        if (lastMove == 2242 && level >= 54) return 3563;
                    }

                    return 2240;
                }

            // Replace Aeolian Edge with Aeolian Edge combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaAeolianEdgeCombo))
                if (actionID == 2255)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 2240 && level >= 4) return 2242;
                        if (lastMove == 2242 && level >= 26) return 2255;
                    }

                    return 2240;
                }

            // Replace Hakke Mujinsatsu with Hakke Mujinsatsu combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaHakkeMujinsatsuCombo))
                if (actionID == 16488)
                {
                    if (comboTime > 0)
                        if (lastMove == 2254 && level >= 52)
                            return 16488;
                    return 2254;
                }

            //Replace Dream Within a Dream with Assassinate when Assassinate Ready
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaAssassinateFeature))
                if (actionID == 3566)
                {
                    if (SearchBuffArray(1955)) return 2246;
                    return 3566;
                }

            // GUNBREAKER

            // Replace Solid Barrel with Solid Barrel combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerSolidBarrelCombo))
                if (actionID == 16145)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == 16137 && level >= 4) return 16139;
                        if (lastMove == 16139 && level >= 26) return 16145;
                    }

                    return 16137;
                }

            // Replace Wicked Talon with Gnashing Fang combo
            // TODO: Potentially add Contuation moves as well?
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerGnashingFangCombo))
                if (actionID == 16150)
                {
                    var ammoComboState = clientState.JobGauges.Get<GNBGauge>().AmmoComboStepNumber;
                    if (ammoComboState == 1) return 16147;
                    if (ammoComboState == 2) return 16150;
                    return 16146;
                }

            // Replace Demon Slaughter with Demon Slaughter combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerDemonSlaughterCombo))
                if (actionID == 16149)
                {
                    if (comboTime > 0)
                        if (lastMove == 16141 && level >= 40)
                            return 16149;
                    return 16141;
                }

            // MACHINIST

            // Replace Clean Shot with Heated Clean Shot combo
            // Or with Heat Blast when overheated.
            // For some reason the shots use their unheated IDs as combo moves
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistMainCombo))
                if (actionID == 2873)
                {
                    var gauge = clientState.JobGauges.Get<MCHGauge>();
                    // End overheat slightly early to prevent eager button mashing clipping your gcd with a fake 6th HB.
                    if (gauge.IsOverheated() && level >= 35 && gauge.OverheatTimeRemaining > 30) return 7410;
                    if (comboTime > 0)
                    {
                        if (lastMove == 2866)
                        {
                            if (level >= 60) return 7412;
                            if (level >= 2) return 2868;
                        }

                        if (lastMove == 2868)
                        {
                            if (level >= 64) return 7413;
                            if (level >= 26) return 2873;
                        }
                    }

                    if (level >= 54) return 7411;
                    return 2866;
                }

            // Replace Spread Shot with Auto Crossbow when overheated.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistSpreadShotFeature))
                if (actionID == 2870)
                {
                    if (clientState.JobGauges.Get<MCHGauge>().IsOverheated() && level >= 52) return 16497;
                    return 2870;
                }

            // BLACK MAGE

            // Enochian changes to B4 or F4 depending on stance.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackEnochianFeature))
                if (actionID == 3575)
                {
                    var jobInfo = clientState.JobGauges.Get<BLMGauge>();
                    if (jobInfo.IsEnoActive())
                    {
                        if (jobInfo.InUmbralIce() && level >= 58) return 3576;
                        if (level >= 60) return 3577;
                    }

                    return 3575;
                }

            // Umbral Soul and Transpose
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackManaFeature))
                if (actionID == 149)
                {
                    var gauge = clientState.JobGauges.Get<BLMGauge>();
                    if (gauge.InUmbralIce() && gauge.IsEnoActive() && level >= 76) return 16506;
                    return 149;
                }

            // ASTROLOGIAN

            // Make cards on the same button as play
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.AstrologianCardsOnDrawFeature))
                if (actionID == 17055)
                {
                    var gauge = clientState.JobGauges.Get<ASTGauge>();
                    switch (gauge.DrawnCard())
                    {
                        case CardType.BALANCE:
                            return 4401;
                        case CardType.BOLE:
                            return 4404;
                        case CardType.ARROW:
                            return 4402;
                        case CardType.SPEAR:
                            return 4403;
                        case CardType.EWER:
                            return 4405;
                        case CardType.SPIRE:
                            return 4406;
                        /*
                        case CardType.LORD:
                            return 7444;
                        case CardType.LADY:
                            return 7445;
                        */
                        default:
                            return 3590;
                    }
                }

            // SUMMONER

            // DWT changes. 
            // Now contains DWT, Deathflare, Summon Bahamut, Enkindle Bahamut, FBT, and Enkindle Phoenix.
            // What a monster of a button.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerDwtCombo))
                if (actionID == 3581)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.TimerRemaining > 0)
                    {
                        if (gauge.ReturnSummon > 0)
                        {
                            if (gauge.IsPhoenixReady()) return 16516;
                            return 7429;
                        }

                        if (level >= 60) return 3582;
                    }
                    else
                    {
                        if (gauge.IsBahamutReady()) return 7427;
                        if (gauge.IsPhoenixReady())
                        {
                            if (level == 80) return 16549;
                            return 16513;
                        }

                        return 3581;
                    }
                }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerDemiCombo))
            {
                // Replace Deathflare with demi enkindles
                if (actionID == 3582)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.IsPhoenixReady()) return 16516;
                    if (gauge.TimerRemaining > 0 && gauge.ReturnSummon != SummonPet.NONE) return 7429;
                    return 3582;
                }

                //Replace DWT with demi summons
                if (actionID == 3581)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.IsBahamutReady()) return 7427;
                    if (gauge.IsPhoenixReady() ||
                        gauge.TimerRemaining > 0 && gauge.ReturnSummon != SummonPet.NONE) return 16513;
                    return 3581;
                }
            }

            // Ruin 1 now upgrades to Brand of Purgatory in addition to Ruin 3 and Fountain of Fire
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerBoPCombo))
                if (actionID == 163)
                {
                    var gauge = clientState.JobGauges.Get<SMNGauge>();
                    if (gauge.TimerRemaining > 0)
                        if (gauge.IsPhoenixReady())
                        {
                            if (SearchBuffArray(1867)) return 16515;
                            return 16514;
                        }

                    if (level >= 54) return 3579;
                    return 163;
                }

            // Change Fester into Energy Drain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerEDFesterCombo))
                if (actionID == 181)
                {
                    if (!clientState.JobGauges.Get<SMNGauge>().HasAetherflowStacks())
                        return 16508;
                    return 181;
                }

            //Change Painflare into Energy Syphon
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerESPainflareCombo))
                if (actionID == 3578)
                {
                    if (!clientState.JobGauges.Get<SMNGauge>().HasAetherflowStacks())
                        return 16510;
                    if (level >= 52) return 3578;
                    return 16510;
                }

            // SCHOLAR

            // Change Fey Blessing into Consolation when Seraph is out.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarSeraphConsolationFeature))
                if (actionID == 16543)
                {
                    if (clientState.JobGauges.Get<SCHGauge>().SeraphTimer > 0) return 16546;
                    return 16543;
                }

            // Change Energy Drain into Aetherflow when you have no more Aetherflow stacks.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarEnergyDrainFeature))
                if (actionID == 167)
                {
                    if (clientState.JobGauges.Get<SCHGauge>().NumAetherflowStacks == 0) return 166;
                    return 167;
                }

            // DANCER

            /*
    
            // Standard Step is one button.
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerStandardStepCombo)) {
                if (actionID == 15997) {
                    DNCGauge gauge = this.clientState.JobGauges.Get<DNCGauge>();
                    if (gauge.IsDancing()) {
                        if (gauge.NumCompleteSteps == 2) {
                            return 16192;
                        }
                        else {
                            // C# can't implicitly cast from int to ulong.
                            return gauge.NextStep();
                        }
                    }
                    return 15997;
                }
            }
    
            // Technical Step is one button.
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerTechnicalStepCombo)) {
                if (actionID == 15998) {
                    DNCGauge gauge = this.clientState.JobGauges.Get<DNCGauge>();
                    if (gauge.IsDancing()) {
                        if (gauge.NumCompleteSteps == 4) {
                            return 16196;
                        }
                        else {
                            // C# can't implicitly cast from int to ulong.
                            return gauge.NextStep();
                        }
                    }
                    return 15998;
                }
            }
    
            // Fountain changes into Fountain combo, prioritizing procs over combo,
            // and Fountainfall over Reverse Cascade.
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerFountainCombo)) {
                if (actionID == 15990) {
                    if (this.clientState.JobGauges.Get<DNCGauge>().IsDancing()) return 15999;
                    if (SearchBuffArray(1815)) return 15992;
                    if (SearchBuffArray(1814)) return 15991;
                    if (comboTime > 0) {
                        if (lastMove == 15989 && level >= 2) return 15990;
                    }
                    return 15989;
                }
            }
    
            */

            // AoE GCDs are split into two buttons, because priority matters
            // differently in different single-target moments. Thanks yoship.
            // Replaces each GCD with its procced version.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerAoeGcdFeature))
            {
                if (actionID == 15994)
                {
                    if (SearchBuffArray(1817)) return 15996;
                    return 15994;
                }

                if (actionID == 15993)
                {
                    if (SearchBuffArray(1816)) return 15995;
                    return 15993;
                }
            }

            // Fan Dance changes into Fan Dance 3 while flourishing.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerFanDanceCombo))
            {
                if (actionID == 16007)
                {
                    if (SearchBuffArray(1820)) return 16009;

                    return 16007;
                }

                // Fan Dance 2 changes into Fan Dance 3 while flourishing.
                if (actionID == 16008)
                {
                    if (SearchBuffArray(1820)) return 16009;
                    return 16008;
                }
            }

            // WHM

            // Replace Solace with Misery when full blood lily
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageSolaceMiseryFeature))
                if (actionID == 16531)
                {
                    if (clientState.JobGauges.Get<WHMGauge>().NumBloodLily == 3)
                        return 16535;
                    return 16531;
                }

            // Replace Solace with Misery when full blood lily
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageRaptureMiseryFeature))
                if (actionID == 16534)
                {
                    if (clientState.JobGauges.Get<WHMGauge>().NumBloodLily == 3)
                        return 16535;
                    return 16534;
                }

            // BARD

            // Replace Wanderer's Minuet with PP when in WM.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardWandererPPFeature))
                if (actionID == 3559)
                {
                    if (clientState.JobGauges.Get<BRDGauge>().ActiveSong == CurrentSong.WANDERER) return 7404;
                    return 3559;
                }

            // Replace HS/BS with SS/RA when procced.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardStraightShotUpgradeFeature))
                if (actionID == 97)
                {
                    if (SearchBuffArray(122))
                    {
                        if (level >= 70) return 7409;
                        return 98;
                    }

                    if (level >= 76) return 16495;
                    return 97;
                }

            // MONK

            /*
    
            // Replace Snap Punch with flank positional combo.
            // During PB, Snap (with sub-max stacks) > Twin (with no active Twin) > DK
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.MonkFlankCombo)) {
                if (actionID == 56) {
                    if (SearchBuffArray(110)) {
                        MNKGauge gauge = this.clientState.JobGauges.Get<MNKGauge>();
                        if ((gauge.NumGLStacks < 3 && level < 76) || SearchBuffArray(103)) {
                            return 56;
                        }
                        else if (gauge.NumGLStacks < 4 && level >= 76 && SearchBuffArray(105)) {
                            return 56;
                        }
                        else if (!SearchBuffArray(101)) return 61;
                        else return 74;
                    }
                    else {
                        if (SearchBuffArray(107) && level >= 50) return 74;
                        if (SearchBuffArray(108) && level >= 18) return 61;
                        if (SearchBuffArray(109) && level >= 6) return 56;
                        return 74;
                    }
                }
            }
    
            // Replace Demolish with rear positional combo.
            // During PB, Demo (with sub-max stacks) > Bootshine.
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.MonkRearCombo)) {
                if (actionID == 66) {
                    if (SearchBuffArray(110)) {
                        MNKGauge gauge = this.clientState.JobGauges.Get<MNKGauge>();
                        if ((gauge.NumGLStacks < 3 && level < 76) || SearchBuffArray(103)) {
                            return 66;
                        }
                        else if (gauge.NumGLStacks < 4 && level >= 76 && SearchBuffArray(105)) {
                            return 66;
                        }
                        else return 53;
                    }
                    else {
                        if (SearchBuffArray(107)) return 53;
                        if (SearchBuffArray(108) && level >= 4) return 54;
                        if (SearchBuffArray(109) && level >= 30) return 66;
                        return 53;
                    }
                }
            }
    
            // Replace Rockbreaker with AoE combo.
            // During PB, RB (with sub-max stacks) > Twin Snakes (if not applied) > RB.
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.MonkAoECombo)) {
                if (actionID == 70) {
                    if (SearchBuffArray(110)) {
                        MNKGauge gauge = this.clientState.JobGauges.Get<MNKGauge>();
                        if ((gauge.NumGLStacks < 3 && level < 76) || SearchBuffArray(103)) {
                            return 70;
                        }
                        else if (gauge.NumGLStacks < 4 && level >= 76 && SearchBuffArray(105)) {
                            return 70;
                        }
                        else if (!SearchBuffArray(101)) return 61;
                        else return 70;
                    }
                    else {
                        if (SearchBuffArray(107)) return 62;
                        if (SearchBuffArray(108)) {
                            if (!SearchBuffArray(101)) return 61;
                            if (level >= 45) return 16473;
                        }
                        if (SearchBuffArray(109) && level >= 30) return 70;
                        return 62;
                    }
                }
            }
    
            */

            // RED MAGE

            /*
    
            // Replace Verstone with White Magic spells. Priority order:
            // Scorch > Verholy > Verstone = Veraero (with Dualcast active) > opener Veraero > Jolt
            // Impact is not the first available spell to allow for precast openers.
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageWhiteMagicFeature)) {
                if (actionID == 7511) {
                    if ((lastMove == 7526 || lastMove == 7525) && level >= 80) return 16530;
                    if (lastMove == 7529 && level >= 70) return 7526;
                    if ((SearchBuffArray(1249) || SearchBuffArray(167)) && level >= 10) return 7507;
                    if (SearchBuffArray(1235) && level >= 30) return 7511;
                    RDMGauge gauge = this.clientState.JobGauges.Get<RDMGauge>();
                    if ((gauge.BlackGauge == 0 && gauge.WhiteGauge == 0) && level >= 10) return 7507;
                    if (level >= 62) return 7524;
                    return 7503;
                }
            }
    
            // Replace Verfire with Black Magic spells. Priority order:
            // Scorch > Verflare> Verfire = Verthunder (with Dualcast active) > opener Verthunder > Jolt
            // Impact is not the first available spell to allow for precast openers.
            if (this.Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageBlackMagicFeature)) {
                if (actionID == 7510) {
                    if ((lastMove == 7526 || lastMove == 7525) && level >= 80) return 16530;
                    if (lastMove == 7529 && level >= 68) return 7525;
                    if ((SearchBuffArray(1249) || SearchBuffArray(167)) && level >= 4) return 7505;
                    if (SearchBuffArray(1234) && level >= 26) return 7510;
                    RDMGauge gauge = this.clientState.JobGauges.Get<RDMGauge>();
                    if ((gauge.BlackGauge == 0 && gauge.WhiteGauge == 0) && level >= 4) return 7505;
                    if (level >= 62) return 7524;
                    return 7503;
                }
            }
            */
            // Replace Veraero/thunder 2 with Impact when Dualcast is active
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageAoECombo))
            {
                if (actionID == 16525)
                {
                    if (level >= 66 && (SearchBuffArray(1249) || SearchBuffArray(167))) return 16526;
                    return 16525;
                }

                if (actionID == 16524)
                {
                    if (level >= 66 && (SearchBuffArray(1249) || SearchBuffArray(167))) return 16526;
                    return 16524;
                }
            }


            // Replace Redoublement with Redoublement combo, Enchanted if possible.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageMeleeCombo))
                if (actionID == 7516)
                {
                    var gauge = clientState.JobGauges.Get<RDMGauge>();
                    if ((lastMove == 7504 || lastMove == 7527) && level >= 35)
                    {
                        if (gauge.BlackGauge >= 25 && gauge.WhiteGauge >= 25) return 7528;
                        return 7512;
                    }

                    if (lastMove == 7512 && level >= 50)
                    {
                        if (gauge.BlackGauge >= 25 && gauge.WhiteGauge >= 25) return 7529;
                        return 7516;
                    }

                    if (gauge.BlackGauge >= 30 && gauge.WhiteGauge >= 30) return 7527;
                    return 7516;
                }

            return iconHook.Original(self, actionID);
        }

        private bool SearchBuffArray(short needle)
        {
            for (var i = 0; i < 60; i++)
                if (Marshal.ReadInt16(activeBuffArray + 4 * i) == needle)
                    return true;
            return false;
        }

        private unsafe IntPtr FindBuffAddress()
        {
            var randomAddress = byteBase + 0x1C02BE0;
            var num = Marshal.ReadIntPtr(randomAddress);
            var step2 = (IntPtr) (Marshal.ReadInt64(num) + 0x248);
            var step3 = Marshal.ReadIntPtr(step2);
            var callback = Marshal.GetDelegateForFunctionPointer<getArray>(step3);
            return (IntPtr) callback((long*) num);
        }

        private void PopulateDict()
        {
            customIds.Add(16477);
            customIds.Add(88);
            customIds.Add(84);
            customIds.Add(3632);
            customIds.Add(16468);
            customIds.Add(3538);
            customIds.Add(3539);
            customIds.Add(16457);
            customIds.Add(42);
            customIds.Add(45);
            customIds.Add(16462);
            customIds.Add(7480);
            customIds.Add(7481);
            customIds.Add(7482);
            customIds.Add(7484);
            customIds.Add(7485);
            customIds.Add(3563);
            customIds.Add(2255);
            customIds.Add(16488);
            customIds.Add(16145);
            customIds.Add(16150);
            customIds.Add(16149);
            customIds.Add(7413);
            customIds.Add(2870);
            customIds.Add(3575);
            customIds.Add(149);
            customIds.Add(17055);
            customIds.Add(3582);
            customIds.Add(3581);
            customIds.Add(163);
            customIds.Add(181);
            customIds.Add(3578);
            customIds.Add(16543);
            customIds.Add(167);
            customIds.Add(15994);
            customIds.Add(15993);
            customIds.Add(16007);
            customIds.Add(16008);
            customIds.Add(16531);
            customIds.Add(16534);
            customIds.Add(3559);
            customIds.Add(97);
            customIds.Add(16525);
            customIds.Add(16524);
            customIds.Add(7516);
            customIds.Add(3566);
            customIds.Add(92);
            customIds.Add(3553);
            customIds.Add(2873);
            vanillaIds.Add(0x3e75);
            vanillaIds.Add(0x3e76);
            vanillaIds.Add(0x3e77);
            vanillaIds.Add(0x3e78);
            vanillaIds.Add(0x3e7d);
            vanillaIds.Add(0x3e7e);
            vanillaIds.Add(0x3e86);
            vanillaIds.Add(0x3f10);
            vanillaIds.Add(0x3f25);
            vanillaIds.Add(0x3f1b);
            vanillaIds.Add(0x3f1c);
            vanillaIds.Add(0x3f1d);
            vanillaIds.Add(0x3f1e);
            vanillaIds.Add(0x451f);
            vanillaIds.Add(0x42ff);
            vanillaIds.Add(0x4300);
            vanillaIds.Add(0x49d4);
            vanillaIds.Add(0x49d5);
            vanillaIds.Add(0x49e9);
            vanillaIds.Add(0x49ea);
            vanillaIds.Add(0x49f4);
            vanillaIds.Add(0x49f7);
            vanillaIds.Add(0x49f9);
            vanillaIds.Add(0x4a06);
            vanillaIds.Add(0x4a31);
            vanillaIds.Add(0x4a32);
            vanillaIds.Add(0x4a35);
            vanillaIds.Add(0x4792);
            vanillaIds.Add(0x452f);
            vanillaIds.Add(0x453f);
            vanillaIds.Add(0x454c);
            vanillaIds.Add(0x455c);
            vanillaIds.Add(0x455d);
            vanillaIds.Add(0x4561);
            vanillaIds.Add(0x4565);
            vanillaIds.Add(0x4566);
            vanillaIds.Add(0x45a0);
            vanillaIds.Add(0x45c8);
            vanillaIds.Add(0x45c9);
            vanillaIds.Add(0x45cd);
            vanillaIds.Add(0x4197);
            vanillaIds.Add(0x4199);
            vanillaIds.Add(0x419b);
            vanillaIds.Add(0x419d);
            vanillaIds.Add(0x419f);
            vanillaIds.Add(0x4198);
            vanillaIds.Add(0x419a);
            vanillaIds.Add(0x419c);
            vanillaIds.Add(0x419e);
            vanillaIds.Add(0x41a0);
            vanillaIds.Add(0x41a1);
            vanillaIds.Add(0x41a2);
            vanillaIds.Add(0x41a3);
            vanillaIds.Add(0x417e);
            vanillaIds.Add(0x404f);
            vanillaIds.Add(0x4051);
            vanillaIds.Add(0x4052);
            vanillaIds.Add(0x4055);
            vanillaIds.Add(0x4053);
            vanillaIds.Add(0x4056);
            vanillaIds.Add(0x405e);
            vanillaIds.Add(0x405f);
            vanillaIds.Add(0x4063);
            vanillaIds.Add(0x406f);
            vanillaIds.Add(0x4074);
            vanillaIds.Add(0x4075);
            vanillaIds.Add(0x4076);
            vanillaIds.Add(0x407d);
            vanillaIds.Add(0x407f);
            vanillaIds.Add(0x4083);
            vanillaIds.Add(0x4080);
            vanillaIds.Add(0x4081);
            vanillaIds.Add(0x4082);
            vanillaIds.Add(0x4084);
            vanillaIds.Add(0x408e);
            vanillaIds.Add(0x4091);
            vanillaIds.Add(0x4092);
            vanillaIds.Add(0x4094);
            vanillaIds.Add(0x4095);
            vanillaIds.Add(0x409c);
            vanillaIds.Add(0x409d);
            vanillaIds.Add(0x40aa);
            vanillaIds.Add(0x40ab);
            vanillaIds.Add(0x40ad);
            vanillaIds.Add(0x40ae);
            vanillaIds.Add(0x272b);
            vanillaIds.Add(0x222a);
            vanillaIds.Add(0x222d);
            vanillaIds.Add(0x222e);
            vanillaIds.Add(0x223b);
            vanillaIds.Add(0x2265);
            vanillaIds.Add(0x2267);
            vanillaIds.Add(0x2268);
            vanillaIds.Add(0x2269);
            vanillaIds.Add(0x2274);
            vanillaIds.Add(0x2290);
            vanillaIds.Add(0x2291);
            vanillaIds.Add(0x2292);
            vanillaIds.Add(0x229c);
            vanillaIds.Add(0x229e);
            vanillaIds.Add(0x22a8);
            vanillaIds.Add(0x22b3);
            vanillaIds.Add(0x22b5);
            vanillaIds.Add(0x22b7);
            vanillaIds.Add(0x22d1);
            vanillaIds.Add(0x4575);
            vanillaIds.Add(0x2335);
            vanillaIds.Add(0x1ebb);
            vanillaIds.Add(0x1cdd);
            vanillaIds.Add(0x1cee);
            vanillaIds.Add(0x1cef);
            vanillaIds.Add(0x1cf1);
            vanillaIds.Add(0x1cf3);
            vanillaIds.Add(0x1cf4);
            vanillaIds.Add(0x1cf7);
            vanillaIds.Add(0x1cfc);
            vanillaIds.Add(0x1d17);
            vanillaIds.Add(0x1d00);
            vanillaIds.Add(0x1d01);
            vanillaIds.Add(0x1d05);
            vanillaIds.Add(0x1d07);
            vanillaIds.Add(0x1d0b);
            vanillaIds.Add(0x1d0d);
            vanillaIds.Add(0x1d0f);
            vanillaIds.Add(0x1d12);
            vanillaIds.Add(0x1d13);
            vanillaIds.Add(0x1d4f);
            vanillaIds.Add(0x1d64);
            vanillaIds.Add(0x1d50);
            vanillaIds.Add(0x1d58);
            vanillaIds.Add(0x1d59);
            vanillaIds.Add(0x1d51);
            vanillaIds.Add(0x1d53);
            vanillaIds.Add(0x1d66);
            vanillaIds.Add(0x1d55);
            vanillaIds.Add(0xdda);
            vanillaIds.Add(0xddd);
            vanillaIds.Add(0xdde);
            vanillaIds.Add(0xde3);
            vanillaIds.Add(0xdf0);
            vanillaIds.Add(0xdfb);
            vanillaIds.Add(0xe00);
            vanillaIds.Add(0xe0b);
            vanillaIds.Add(0xe0c);
            vanillaIds.Add(0xe0e);
            vanillaIds.Add(0xe0f);
            vanillaIds.Add(0xe11);
            vanillaIds.Add(0xfed);
            vanillaIds.Add(0xff7);
            vanillaIds.Add(0xffb);
            vanillaIds.Add(0xfe9);
            vanillaIds.Add(0xb30);
            vanillaIds.Add(0x12e);
            vanillaIds.Add(0x8d3);
            vanillaIds.Add(0x8d4);
            vanillaIds.Add(0x8d5);
            vanillaIds.Add(0x8d7);
            vanillaIds.Add(0xb32);
            vanillaIds.Add(0xb34);
            vanillaIds.Add(0xb38);
            vanillaIds.Add(0xb3e);
            vanillaIds.Add(0x12d);
            vanillaIds.Add(0x15);
            vanillaIds.Add(0x26);
            vanillaIds.Add(0x31);
            vanillaIds.Add(0x33);
            vanillaIds.Add(0x4b);
            vanillaIds.Add(0x62);
            vanillaIds.Add(0x64);
            vanillaIds.Add(0x71);
            vanillaIds.Add(0x77);
            vanillaIds.Add(0x7f);
            vanillaIds.Add(0x79);
            vanillaIds.Add(0x84);
            vanillaIds.Add(0x90);
            vanillaIds.Add(0x99);
            vanillaIds.Add(0xa4);
            vanillaIds.Add(0xb2);
            vanillaIds.Add(0xa8);
            vanillaIds.Add(0xac);
            vanillaIds.Add(0xb8);
            vanillaIds.Add(0xe2);
            vanillaIds.Add(0x10f);
            vanillaIds.Add(0xf3);
            vanillaIds.Add(0x10e);
            vanillaIds.Add(0x110);
            vanillaIds.Add(0x111);
        }

        private unsafe delegate int* getArray(long* address);
    }
}