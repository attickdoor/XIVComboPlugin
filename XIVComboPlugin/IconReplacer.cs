using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Logging;

namespace XIVComboPlugin
{
    public class IconReplacer
    {
        public delegate ulong OnCheckIsIconReplaceableDelegate(uint actionID);

        public delegate ulong OnGetIconDelegate(byte param1, uint param2);

        private IntPtr activeBuffArray = IntPtr.Zero;

        private readonly IconReplacerAddressResolver Address;
        private readonly Hook<OnCheckIsIconReplaceableDelegate> checkerHook;
        private readonly ClientState clientState;

        private readonly IntPtr comboTimer;

        private readonly XIVComboConfiguration Configuration;

        //private readonly HashSet<uint> customIds;
        //private readonly HashSet<uint> vanillaIds;
        private HashSet<uint> noUpdateIcons;
        private HashSet<uint> seenNoUpdate;

        private readonly Hook<OnGetIconDelegate> iconHook;
        private readonly IntPtr lastComboMove;
        private readonly IntPtr playerLevel;
        private readonly IntPtr playerJob;
        private uint lastJob = 0;

        private readonly IntPtr BuffVTableAddr;
        private float ping;

        private unsafe delegate int* getArray(long* address);

        private bool shutdown;

        public IconReplacer(SigScanner scanner, ClientState clientState, XIVComboConfiguration configuration)
        {
            ping = 0;
            shutdown = false;
            Configuration = configuration;
            this.clientState = clientState;

            Address = new IconReplacerAddressResolver();
            Address.Setup(scanner);

            comboTimer = scanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 80 7E 21 00", 0x178);
            lastComboMove = comboTimer + 0x4;
            /*
            playerLevel = scanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 88 45 EF", 0x4d) + 0x78;
            playerJob = playerLevel - 0xE;
            */
            BuffVTableAddr = scanner.GetStaticAddressFromSig("48 89 05 ?? ?? ?? ?? 88 05 ?? ?? ?? ?? 88 05 ?? ?? ?? ??", 0);

            //customIds = new HashSet<uint>();
            //vanillaIds = new HashSet<uint>();
            noUpdateIcons = new HashSet<uint>();
            seenNoUpdate = new HashSet<uint>();

            //PopulateDict();

            PluginLog.Verbose("===== H O T B A R S =====");
            PluginLog.Verbose("IsIconReplaceable address {IsIconReplaceable}", Address.IsIconReplaceable);
            PluginLog.Verbose("GetIcon address {GetIcon}", Address.GetIcon);
            PluginLog.Verbose("ComboTimer address {ComboTimer}", comboTimer);
            PluginLog.Verbose("LastComboMove address {LastComboMove}", lastComboMove);
            PluginLog.Verbose("PlayerLevel address {PlayerLevel}", playerLevel);

            iconHook = new Hook<OnGetIconDelegate>(Address.GetIcon, GetIconDetour);
            checkerHook = new Hook<OnCheckIsIconReplaceableDelegate>(Address.IsIconReplaceable, CheckIsIconReplaceableDetour);

            Task.Run(() =>
            {
                BuffTask();
            });
        }

        public void Enable()
        {
            iconHook.Enable();
            checkerHook.Enable();
        }

        public void Dispose()
        {
            shutdown = true;
            iconHook.Dispose();
            checkerHook.Dispose();
        }


        private async void BuffTask()
        {
            while (!shutdown)
            {
                UpdateBuffAddress();
                await Task.Delay(1000);
            }
        }

        // I hate this function. This is the dumbest function to exist in the game. Just return 1.
        // Determines which abilities are allowed to have their icons updated.
        private ulong CheckIsIconReplaceableDetour(uint actionID)
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

            if (clientState.LocalPlayer == null) return iconHook.Original(self, actionID);

            //if (vanillaIds.Contains(actionID)) return iconHook.Original(self, actionID);
            //if (!customIds.Contains(actionID)) return actionID;
            if (activeBuffArray == IntPtr.Zero) return iconHook.Original(self, actionID);

            // Don't clutter the spaghetti any worse than it already is.
            var lastMove = Marshal.ReadInt32(lastComboMove);
            var comboTime = Marshal.PtrToStructure<float>(comboTimer);
            //var level = Marshal.ReadByte(playerLevel);
            var level = clientState.LocalPlayer.Level;
            // DRAGOON

            // Change Jump/High Jump into Mirage Dive when Dive Ready
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonJumpFeature))
                if (actionID == DRG.Jump)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(1243))
                        return DRG.MirageDive;
                    if (level >= 74)
                        return DRG.HighJump;
                    return DRG.Jump;
                }

            // Replace Coerthan Torment with Coerthan Torment combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonCoerthanTormentCombo))
                if (actionID == DRG.CTorment)
                {
                    if (comboTime > 0)
                    {
                        if ((lastMove == DRG.DoomSpike || lastMove == DRG.DraconianFury) && level >= 62)
                            return DRG.SonicThrust;
                        if (lastMove == DRG.SonicThrust && level >= 72)
                            return DRG.CTorment;
                    }

                    return DRG.DoomSpike;
                }

            // Replace Chaos Thrust with the Chaos Thrust combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonChaosThrustCombo))
                if (actionID == DRG.ChaosThrust)
                {
                    if (comboTime > 0)
                    {
                        if ((lastMove == DRG.TrueThrust || lastMove == DRG.RaidenThrust) && level >= 18)
                            return DRG.Disembowel;
                        if (lastMove == DRG.Disembowel)
                        {
                            if (level >= 86)
                                return DRG.ChaoticSpring;
                            if (level >= 50)
                                return DRG.ChaosThrust;
                        }
                    }
                    UpdateBuffAddress();
                    if (SearchBuffArray(802) && level >= 56)
                        return DRG.FangAndClaw;
                    if (SearchBuffArray(803) && level >= 58)
                        return DRG.WheelingThrust;
                    if (SearchBuffArray(1863) && level >= 76)
                        return DRG.RaidenThrust;

                    return DRG.TrueThrust;
                }

            // Replace Full Thrust with the Full Thrust combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonFullThrustCombo))
                if (actionID == 84)
                {
                    if (comboTime > 0)
                    {
                        if ((lastMove == DRG.TrueThrust || lastMove == DRG.RaidenThrust) && level >= 4)
                            return DRG.VorpalThrust;
                        if (lastMove == DRG.VorpalThrust)
                        {
                            if (level >= 86)
                                return DRG.HeavensThrust;
                            if (level >= 26)
                                return DRG.FullThrust;
                        }
                    }
                    UpdateBuffAddress();
                    if (SearchBuffArray(802) && level >= 56)
                        return DRG.FangAndClaw;
                    if (SearchBuffArray(803) && level >= 58)
                        return DRG.WheelingThrust;
                    if (SearchBuffArray(1863) && level >= 76)
                        return DRG.RaidenThrust;

                    return DRG.TrueThrust;
                }

            // DARK KNIGHT

            // Replace Souleater with Souleater combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DarkSouleaterCombo))
                if (actionID == DRK.Souleater)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == DRK.HardSlash && level >= 2)
                            return DRK.SyphonStrike;
                        if (lastMove == DRK.SyphonStrike && level >= 26)
                            return DRK.Souleater;
                    }

                    return DRK.HardSlash;
                }

            // Replace Stalwart Soul with Stalwart Soul combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DarkStalwartSoulCombo))
                if (actionID == DRK.StalwartSoul)
                {
                    if (comboTime > 0)
                        if (lastMove == DRK.Unleash && level >= 72)
                            return DRK.StalwartSoul;

                    return DRK.Unleash;
                }

            // PALADIN

            // Replace Goring Blade with Goring Blade combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinGoringBladeCombo))
                if (actionID == PLD.GoringBlade)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == PLD.FastBlade && level >= 4)
                            return PLD.RiotBlade;
                        if (lastMove == PLD.RiotBlade && level >= 54)
                            return PLD.GoringBlade;
                    }

                    return PLD.FastBlade;
                }

            // Replace Royal Authority with Royal Authority combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinRoyalAuthorityCombo))
                if (actionID == PLD.RoyalAuthority || actionID == PLD.RageOfHalone)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == PLD.FastBlade && level >= 4)
                            return PLD.RiotBlade;
                        if (lastMove == PLD.RiotBlade)
                        {
                            if (level >= 60)
                                return PLD.RoyalAuthority;
                            if (level >= 26)
                                return PLD.RageOfHalone;
                        }
                    }

                    return PLD.FastBlade;
                }

            // Replace Prominence with Prominence combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinProminenceCombo))
                if (actionID == PLD.Prominence)
                {
                    if (comboTime > 0)
                        if (lastMove == PLD.TotalEclipse && level >= 40)
                            return PLD.Prominence;

                    return PLD.TotalEclipse;
                }

            // Replace Requiescat with Confiteor when under the effect of Requiescat
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.PaladinRequiescatCombo))
                if (actionID == PLD.Requiescat)
                {
                    if (SearchBuffArray(1368) && level >= 80)
                        return PLD.Confiteor;

                    if (lastMove == PLD.Confiteor)
                        return PLD.BladeOfFaith;

                    if (lastMove == PLD.BladeOfFaith)
                        return PLD.BladeOfTruth;

                    if (lastMove == PLD.BladeOfTruth)
                        return PLD.BladeOfValor;

                    return PLD.Requiescat;
                }

            // WARRIOR

            // Replace Storm's Path with Storm's Path combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorStormsPathCombo))
                if (actionID == WAR.StormsPath)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == WAR.HeavySwing && level >= 4)
                            return WAR.Maim;
                        if (lastMove == WAR.Maim && level >= 26)
                            return WAR.StormsPath;
                    }

                    return 31;
                }

            // Replace Storm's Eye with Storm's Eye combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorStormsEyeCombo))
                if (actionID == WAR.StormsEye)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == WAR.HeavySwing && level >= 4)
                            return WAR.Maim;
                        if (lastMove == WAR.Maim && level >= 50)
                            return WAR.StormsEye;
                    }

                    return WAR.HeavySwing;
                }

            // Replace Mythril Tempest with Mythril Tempest combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorMythrilTempestCombo))
                if (actionID == WAR.MythrilTempest)
                {
                    if (comboTime > 0)
                        if (lastMove == WAR.Overpower && level >= 40)
                            return WAR.MythrilTempest;
                    return WAR.Overpower;
                }

            // SAMURAI

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiTsubameCombo))
                if (actionID == SAM.Iaijutsu)
                {
                    var x = iconHook.Original(self, SAM.Tsubame);
                    if (x != SAM.Tsubame) return x;
                    return iconHook.Original(self, actionID);
                }

            // Replace Yukikaze with Yukikaze combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiYukikazeCombo))
                if (actionID == SAM.Yukikaze)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(1233))
                        return SAM.Yukikaze;
                    if (comboTime > 0)
                        if (lastMove == SAM.Hakaze && level >= 50)
                            return SAM.Yukikaze;
                    return SAM.Hakaze;
                }

            // Replace Gekko with Gekko combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiGekkoCombo))
                if (actionID == SAM.Gekko)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(1233))
                        return SAM.Gekko;
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 4)
                            return SAM.Jinpu;
                        if (lastMove == SAM.Jinpu && level >= 30)
                            return SAM.Gekko;
                    }

                    return SAM.Hakaze;
                }

            // Replace Kasha with Kasha combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiKashaCombo))
                if (actionID == SAM.Kasha)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(1233))
                        return SAM.Kasha;
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 18)
                            return SAM.Shifu;
                        if (lastMove == SAM.Shifu && level >= 40)
                            return SAM.Kasha;
                    }

                    return SAM.Hakaze;
                }

            // Replace Mangetsu with Mangetsu combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiMangetsuCombo))
                if (actionID == SAM.Mangetsu)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(1233))
                        return SAM.Mangetsu;
                    if (comboTime > 0)
                        if (lastMove == SAM.Fuga && level >= 35)
                            return SAM.Mangetsu;
                    return SAM.Fuga;
                }

            // Replace Oka with Oka combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiOkaCombo))
                if (actionID == SAM.Oka)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(1233))
                        return SAM.Oka;
                    if (comboTime > 0)
                        if (lastMove == SAM.Fuga && level >= 45)
                            return SAM.Oka;
                    return SAM.Fuga;
                }

            // NINJA

            // Replace Armor Crush with Armor Crush combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaArmorCrushCombo))
                if (actionID == NIN.ArmorCrush)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == NIN.SpinningEdge && level >= 4)
                            return NIN.GustSlash;
                        if (lastMove == NIN.GustSlash && level >= 54)
                            return NIN.ArmorCrush;
                    }

                    return NIN.SpinningEdge;
                }

            // Replace Aeolian Edge with Aeolian Edge combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaAeolianEdgeCombo))
                if (actionID == NIN.AeolianEdge)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == NIN.SpinningEdge && level >= 4)
                            return NIN.GustSlash;
                        if (lastMove == NIN.GustSlash && level >= 26)
                            return NIN.AeolianEdge;
                    }

                    return NIN.SpinningEdge;
                }

            // Replace Hakke Mujinsatsu with Hakke Mujinsatsu combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.NinjaHakkeMujinsatsuCombo))
                if (actionID == NIN.HakkeM)
                {
                    if (comboTime > 0)
                        if (lastMove == NIN.DeathBlossom && level >= 52)
                            return NIN.HakkeM;
                    return NIN.DeathBlossom;
                }

            // GUNBREAKER

            // Replace Solid Barrel with Solid Barrel combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerSolidBarrelCombo))
                if (actionID == GNB.SolidBarrel)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == GNB.KeenEdge && level >= 4)
                            return GNB.BrutalShell;
                        if (lastMove == GNB.BrutalShell && level >= 26)
                            return GNB.SolidBarrel;
                    }

                    return GNB.KeenEdge;
                }

            // Replace Wicked Talon with Gnashing Fang combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerGnashingFangCombo))
                if (actionID == GNB.WickedTalon)
                {
                    if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerGnashingFangCont))
                    {
                        if (level >= GNB.LevelContinuation)
                        {
                            UpdateBuffAddress();
                            if (SearchBuffArray(GNB.BuffReadyToRip))
                                return GNB.JugularRip;
                            if (SearchBuffArray(GNB.BuffReadyToTear))
                                return GNB.AbdomenTear;
                            if (SearchBuffArray(GNB.BuffReadyToGouge))
                                return GNB.EyeGouge;
                        }
                    }
                    var ammoComboState = XIVComboPlugin.JobGauges.Get<GNBGauge>().AmmoComboStep;
                    switch (ammoComboState)
                    {
                        case 1:
                            return GNB.SavageClaw;
                        case 2:
                            return GNB.WickedTalon;
                        default:
                            return GNB.GnashingFang;
                    }
                }

            // Replace Burst Strike with Continuation
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerBurstStrikeCont))
                if (actionID == GNB.BurstStrike)
                {
                    if (level >= GNB.LevelEnhancedContinuation)
                    {
                        UpdateBuffAddress();
                        if (SearchBuffArray(GNB.BuffReadyToBlast))
                            return GNB.Hypervelocity;
                    }
                }

            // Replace Demon Slaughter with Demon Slaughter combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerDemonSlaughterCombo))
                if (actionID == GNB.DemonSlaughter)
                {
                    if (comboTime > 0)
                        if (lastMove == GNB.DemonSlice && level >= 40)
                            return GNB.DemonSlaughter;
                    return GNB.DemonSlice;
                }

            // MACHINIST

            // Replace Clean Shot with Heated Clean Shot combo
            // Or with Heat Blast when overheated.
            // For some reason the shots use their unheated IDs as combo moves
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistMainCombo))
                if (actionID == MCH.CleanShot || actionID == MCH.HeatedCleanShot)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == MCH.SplitShot)
                        {
                            if (level >= 60)
                                return MCH.HeatedSlugshot;
                            if (level >= 2)
                                return MCH.SlugShot;
                        }

                        if (lastMove == MCH.SlugShot)
                        {
                            if (level >= 64)
                                return MCH.HeatedCleanShot;
                            if (level >= 26)
                                return MCH.CleanShot;
                        }
                    }

                    if (level >= 54)
                        return MCH.HeatedSplitShot;
                    return MCH.SplitShot;
                }


            // Replace Hypercharge with Heat Blast when overheated
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistOverheatFeature))
                if (actionID == MCH.Hypercharge)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<MCHGauge>();
                    if (gauge.IsOverheated && level >= 35)
                        return MCH.HeatBlast;
                    return MCH.Hypercharge;
                }

            // Replace Spread Shot with Auto Crossbow when overheated.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistSpreadShotFeature))
                if (actionID == MCH.SpreadShot)
                {
                    if (XIVComboPlugin.JobGauges.Get<MCHGauge>().IsOverheated && level >= 52)
                        return MCH.AutoCrossbow;
                    return MCH.SpreadShot;
                }

            // BLACK MAGE

            // B4 and F4 change to each other depending on stance.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackEnochianFeature))
                if (actionID == BLM.Fire4 || actionID == BLM.Blizzard4)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<BLMGauge>();
                    if (gauge.InUmbralIce && level >= 58)
                        return BLM.Blizzard4;
                    if (level >= 60)
                        return BLM.Fire4;
                }

            // Umbral Soul and Transpose
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackManaFeature))
                if (actionID == BLM.Transpose)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<BLMGauge>();
                    if (gauge.InUmbralIce && gauge.IsEnochianActive && level >= 76)
                        return BLM.UmbralSoul;
                    return BLM.Transpose;
                }

            // Ley Lines and BTL
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackLeyLines))
                if (actionID == BLM.LeyLines)
                {
                    if (SearchBuffArray(737) && level >= 62)
                        return BLM.BTL;
                    return BLM.LeyLines;
                }

            // ASTROLOGIAN

            // Make cards on the same button as play
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.AstrologianCardsOnDrawFeature))
                if (actionID == AST.Play)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<ASTGauge>();
                    switch (gauge.DrawnCard)
                    {
                        case CardType.BALANCE:
                            return AST.Balance;
                        case CardType.BOLE:
                            return AST.Bole;
                        case CardType.ARROW:
                            return AST.Arrow;
                        case CardType.SPEAR:
                            return AST.Spear;
                        case CardType.EWER:
                            return AST.Ewer;
                        case CardType.SPIRE:
                            return AST.Spire;
                        default:
                            return AST.Draw;
                    }
                }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.AstrologianMinorArcanaPlayFeature))
                if (actionID == AST.MinorArcana)
                {
                    if (level >= 70)
                    {
                        UpdateBuffAddress();
                        if (SearchBuffArray(AST.BuffLadyOfCrownsDrawn) || SearchBuffArray(AST.BuffLadyOfCrownsDrawn))
                            return AST.CrownPlay;
                    }
                }

            // SUMMONER

            /*
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerDwtCombo))
                if (actionID == 3581)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<SMNGauge>();
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
                if (actionID == SMN.Deathflare)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<SMNGauge>();
                    if (gauge.IsPhoenixReady)
                        return SMN.EnkindlePhoenix;
                    if (gauge.TimerRemaining > 0 && gauge.ReturnSummon != SummonPet.NONE)
                        return SMN.EnkindleBahamut;
                    return SMN.Deathflare;
                }

                //Replace DWT with demi summons
                if (actionID == SMN.DWT)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<SMNGauge>();
                    if (gauge.IsBahamutReady)
                        return SMN.SummonBahamut;
                    if (gauge.IsPhoenixReady ||
                        gauge.TimerRemaining > 0 && gauge.ReturnSummon != SummonPet.NONE)
                    {
                        if (level >= 80)
                            return SMN.FBTHigh;
                        return SMN.FBTLow;
                    }
                    return SMN.DWT;
                }
            }

            // Ruin 1 now upgrades to Brand of Purgatory in addition to Ruin 3 and Fountain of Fire
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerBoPCombo))
                if (actionID == SMN.Ruin1 || actionID == SMN.Ruin3)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<SMNGauge>();
                    if (gauge.TimerRemaining > 0)
                        if (gauge.IsPhoenixReady)
                        {
                            UpdateBuffAddress();
                            if (SearchBuffArray(1867))
                                return SMN.BrandOfPurgatory;
                            return SMN.FountainOfFire;
                        }

                    if (level >= 54)
                        return SMN.Ruin3;
                    return SMN.Ruin1;
                }

            // Change Fester into Energy Drain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerEDFesterCombo))
                if (actionID == SMN.Fester)
                {
                    if (!XIVComboPlugin.JobGauges.Get<SMNGauge>().HasAetherflowStacks)
                        return SMN.EnergyDrain;
                    return SMN.Fester;
                }

            //Change Painflare into Energy Syphon
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerESPainflareCombo))
                if (actionID == SMN.Painflare)
                {
                    if (!XIVComboPlugin.JobGauges.Get<SMNGauge>().HasAetherflowStacks)
                        return SMN.EnergySyphon;
                    if (level >= 52)
                        return SMN.Painflare;
                    return SMN.EnergySyphon;
                }
            */

            // SCHOLAR

            // Change Fey Blessing into Consolation when Seraph is out.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarSeraphConsolationFeature))
                if (actionID == SCH.FeyBless)
                {
                    if (XIVComboPlugin.JobGauges.Get<SCHGauge>().SeraphTimer > 0) return SCH.Consolation;
                    return SCH.FeyBless;
                }

            // Change Energy Drain into Aetherflow when you have no more Aetherflow stacks.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarEnergyDrainFeature))
                if (actionID == SCH.EnergyDrain)
                {
                    if (XIVComboPlugin.JobGauges.Get<SCHGauge>().Aetherflow == 0) return SCH.Aetherflow;
                    return SCH.EnergyDrain;
                }

            // DANCER

            // AoE GCDs are split into two buttons, because priority matters
            // differently in different single-target moments. Thanks yoship.
            // Replaces each GCD with its procced version.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerAoeGcdFeature))
            {
                if (actionID == DNC.Bloodshower)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(DNC.BuffFlourishingFlow))
                        return DNC.Bloodshower;
                    return DNC.Bladeshower;
                }

                if (actionID == DNC.RisingWindmill)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(DNC.BuffFlourishingSymmetry))
                        return DNC.RisingWindmill;
                    return DNC.Windmill;
                }
            }

            // Fan Dance changes into Fan Dance 3 while flourishing.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerFanDanceCombo))
            {
                if (actionID == DNC.FanDance1)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(DNC.BuffFourfoldFanDance))
                        return DNC.FanDance4;
                    if (SearchBuffArray(DNC.BuffThreefoldFanDance))
                        return DNC.FanDance3;
                    return DNC.FanDance1;
                }

                // Fan Dance 2 changes into Fan Dance 3 while flourishing.
                if (actionID == DNC.FanDance2)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(DNC.BuffFourfoldFanDance))
                        return DNC.FanDance4;
                    if (SearchBuffArray(DNC.BuffThreefoldFanDance))
                        return DNC.FanDance3;
                    return DNC.FanDance2;
                }
            }

            // WHM

            // Replace Solace with Misery when full blood lily
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageSolaceMiseryFeature))
                if (actionID == WHM.Solace)
                {
                    if (XIVComboPlugin.JobGauges.Get<WHMGauge>().BloodLily == 3)
                        return WHM.Misery;
                    return WHM.Solace;
                }

            // Replace Solace with Misery when full blood lily
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageRaptureMiseryFeature))
                if (actionID == WHM.Rapture)
                {
                    if (XIVComboPlugin.JobGauges.Get<WHMGauge>().BloodLily == 3)
                        return WHM.Misery;
                    return WHM.Rapture;
                }

            // BARD

            // Replace Wanderer's Minuet with PP when in WM.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardWandererPPFeature))
                if (actionID == BRD.WanderersMinuet)
                {
                    if (XIVComboPlugin.JobGauges.Get<BRDGauge>().Song == Song.WANDERER)
                        return BRD.PitchPerfect;
                    return BRD.WanderersMinuet;
                }

            // Replace HS/BS with SS/RA when procced.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardStraightShotUpgradeFeature))
                if (actionID == BRD.HeavyShot || actionID == BRD.BurstShot)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(122))
                    {
                        if (level >= 70) return BRD.RefulgentArrow;
                        return BRD.StraightShot;
                    }

                    if (level >= 76) return BRD.BurstShot;
                    return BRD.HeavyShot;
                }

            // MONK

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MnkAoECombo))
                if (actionID == MNK.Rockbreaker)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(110) || SearchBuffArray(2513)) return MNK.Rockbreaker;
                    if (SearchBuffArray(107)) return MNK.AOTD;
                    if (SearchBuffArray(108) && level >= 45) return MNK.FourPointFury;
                    if (SearchBuffArray(109) && level >= 30) return MNK.Rockbreaker;
                    return MNK.AOTD;
                }

            // RED MAGE

            // Replace Veraero/thunder 2 with Impact when Dualcast is active
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageAoECombo))
            {
                if (actionID == RDM.Veraero2)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(167) || SearchBuffArray(1249))
                    {
                        if (level >= 66) return RDM.Impact;
                        return RDM.Scatter;
                    }
                    return RDM.Veraero2;
                }

                if (actionID == RDM.Verthunder2)
                {
                    UpdateBuffAddress();
                    if (SearchBuffArray(167) || SearchBuffArray(1249))
                    {
                        if (level >= 66) return RDM.Impact;
                        return RDM.Scatter;
                    }
                    return RDM.Verthunder2;
                }
            }

            // Replace Redoublement with Redoublement combo, Enchanted if possible.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageMeleeCombo))
                if (actionID == RDM.Redoublement)
                {
                    var gauge = XIVComboPlugin.JobGauges.Get<RDMGauge>();
                    if ((lastMove == RDM.Riposte || lastMove == RDM.ERiposte) && level >= 35)
                    {
                        if (gauge.BlackMana >= 25 && gauge.WhiteMana >= 25)
                            return RDM.EZwerchhau;
                        return RDM.Zwerchhau;
                    }

                    if (lastMove == RDM.Zwerchhau && level >= 50)
                    {
                        if (gauge.BlackMana >= 25 && gauge.WhiteMana >= 25)
                            return RDM.ERedoublement;
                        return RDM.Redoublement;
                    }

                    if (gauge.BlackMana >= 30 && gauge.WhiteMana >= 30)
                        return RDM.ERiposte;
                    return RDM.Riposte;
                }
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageVerprocCombo))
            {
                if (actionID == RDM.Verstone)
                {
                    if (level >= 80 && (lastMove == RDM.Verflare || lastMove == RDM.Verholy)) return RDM.Scorch;
                    UpdateBuffAddress();
                    if (SearchBuffArray(1235)) return RDM.Verstone;
                    if (level < 62) return RDM.Jolt;
                    return RDM.Jolt2;
                }
                if (actionID == RDM.Verfire)
                {
                    if (level >= 80 && (lastMove == RDM.Verflare || lastMove == RDM.Verholy)) return RDM.Scorch;
                    UpdateBuffAddress();
                    if (SearchBuffArray(1234)) return RDM.Verfire;
                    if (level < 62) return RDM.Jolt;
                    return RDM.Jolt2;
                }
            }

            // REAPER 

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ReaperSliceCombo))
            {
                if (actionID == RPR.Slice)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == RPR.Slice && level >= RPR.Levels.WaxingSlice)
                            return RPR.WaxingSlice;

                        if (lastMove == RPR.WaxingSlice && level >= RPR.Levels.InfernalSlice)
                            return RPR.InfernalSlice;
                    }

                    return RPR.Slice;
                }
            }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ReaperScytheCombo))
            {
                if (actionID == RPR.SpinningScythe)
                {
                    if (comboTime > 0)
                    {
                        if (lastMove == RPR.SpinningScythe && level >= RPR.Levels.NightmareScythe)
                            return RPR.NightmareScythe;
                    }

                    return RPR.SpinningScythe;
                }

                return actionID;
            }

            return iconHook.Original(self, actionID);
        }
        /*
        public void UpdatePing(ulong value)
        {
            ping = (float)(value)/1000;
        }
        */
        private bool SearchBuffArray(short needle)
        {
            if (activeBuffArray == IntPtr.Zero) return false;
            for (var i = 0; i < 60; i++)
                if (Marshal.ReadInt16(activeBuffArray + (12 * i)) == needle)
                    return true;
            return false;
        }

        private void UpdateBuffAddress()
        {
            try
            {
                activeBuffArray = FindBuffAddress();
            }
            catch (Exception)
            {
                //Before you're loaded in
                activeBuffArray = IntPtr.Zero;
            }
        }

        private unsafe IntPtr FindBuffAddress()
        {
            var num = Marshal.ReadIntPtr(BuffVTableAddr);
            var step2 = (IntPtr)(Marshal.ReadInt64(num) + 0x278);
            var step3 = Marshal.ReadIntPtr(step2);
            var callback = Marshal.GetDelegateForFunctionPointer<getArray>(step3);
            return (IntPtr)callback((long*)num) + 8;
        }
        /*
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
            customIds.Add(3579);
            customIds.Add(17209);
            customIds.Add(7501);
            customIds.Add(21);
            customIds.Add(DNC.Bloodshower);
            customIds.Add(DNC.RisingWindmill);
            customIds.Add(RDM.Verstone);
            customIds.Add(RDM.Verfire);
            customIds.Add(MNK.Rockbreaker);
            customIds.Add(BLM.LeyLines);
            customIds.Add(PLD.Requiescat);
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
            vanillaIds.Add(0xe00);
            vanillaIds.Add(0xe0b);
            vanillaIds.Add(0xe0c);
            vanillaIds.Add(0xe0e);
            vanillaIds.Add(0xe0f);
            vanillaIds.Add(0xe11);
            vanillaIds.Add(0xe18);
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
            vanillaIds.
        }
        */
    }
}
