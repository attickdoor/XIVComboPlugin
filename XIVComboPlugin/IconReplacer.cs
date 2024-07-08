using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;
using XIVComboPlugin.JobActions;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Plugin.Services;

namespace XIVComboPlugin
{
    public class IconReplacer
    {
        public delegate ulong OnCheckIsIconReplaceableDelegate(uint actionID);

        public delegate ulong OnGetIconDelegate(byte param1, uint param2);

        private readonly IconReplacerAddressResolver Address;
        private readonly Hook<OnCheckIsIconReplaceableDelegate> checkerHook;
        private readonly IClientState clientState;

        private IntPtr comboTimer = IntPtr.Zero;
        private IntPtr lastComboMove = IntPtr.Zero;

        private readonly XIVComboConfiguration Configuration;

        private readonly Hook<OnGetIconDelegate> iconHook;

        private IGameInteropProvider HookProvider;
        private IJobGauges JobGauges;
        private IPluginLog PluginLog;

        private unsafe delegate int* getArray(long* address);

        public IconReplacer(ISigScanner scanner, IClientState clientState, IDataManager manager, XIVComboConfiguration configuration, IGameInteropProvider hookProvider, IJobGauges jobGauges, IPluginLog pluginLog)
        {
            HookProvider = hookProvider;
            Configuration = configuration;
            this.clientState = clientState;
            JobGauges = jobGauges;
            PluginLog = pluginLog;

            Address = new IconReplacerAddressResolver(scanner);

            if (!clientState.IsLoggedIn)
                clientState.Login += SetupComboData;
            else
                SetupComboData();

            PluginLog.Verbose("===== X I V C O M B O =====");
            PluginLog.Verbose("IsIconReplaceable address {IsIconReplaceable}", Address.IsIconReplaceable);
            PluginLog.Verbose("ComboTimer address {ComboTimer}", comboTimer);
            PluginLog.Verbose("LastComboMove address {LastComboMove}", lastComboMove);

            iconHook = HookProvider.HookFromAddress<OnGetIconDelegate>((nint)ActionManager.Addresses.GetAdjustedActionId.Value, GetIconDetour);
            checkerHook = HookProvider.HookFromAddress<OnCheckIsIconReplaceableDelegate>(Address.IsIconReplaceable, CheckIsIconReplaceableDetour);
            HookProvider = hookProvider;
        }

        public unsafe void SetupComboData()
        {
            var actionmanager = (byte*)ActionManager.Instance();
            comboTimer = (IntPtr)(actionmanager + 0x60);
            lastComboMove = comboTimer + 0x4;
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
            // Last resort. For some reason GetIcon fires after leaving the lobby but before ClientState.Login
            if (lastComboMove == IntPtr.Zero)
            {
                SetupComboData();
                return iconHook.Original(self, actionID);
            }
            if (comboTimer == IntPtr.Zero)
            {
                SetupComboData();
                return iconHook.Original(self, actionID);
            }

            var lastMove = Marshal.ReadInt32(lastComboMove);
            var comboTime = Marshal.PtrToStructure<float>(comboTimer);
            var level = clientState.LocalPlayer.Level;

            // DRAGOON

            // Change Jump/High Jump into Mirage Dive when Dive Ready
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonJumpFeature))
                if (actionID == DRG.Jump || actionID == DRG.HighJump)
                {
                    if (SearchBuffArray(1243))
                        return DRG.MirageDive;
                    return iconHook.Original(self, DRG.Jump);
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
                    
                    return iconHook.Original(self, DRG.DoomSpike);
                }

            // Replace Chaos Thrust with the Chaos Thrust combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonChaosThrustCombo))
                if (actionID == DRG.ChaosThrust || actionID == DRG.ChaoticSpring)
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
                    if (SearchBuffArray(DRG.BuffFangAndClawReady) && level >= 56)
                        return DRG.FangAndClaw;
                    if (SearchBuffArray(DRG.BuffWheelingThrustReady) && level >= 58)
                        return DRG.WheelingThrust;
                    if (SearchBuffArray(DRG.BuffDraconianFire) && level >= 76)
                        return DRG.RaidenThrust;

                    return DRG.TrueThrust;
                }

            // Replace Full Thrust with the Full Thrust combo chain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DragoonFullThrustCombo))
                if (actionID == DRG.FullThrust || actionID == DRG.HeavensThrust)
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
                    if (SearchBuffArray(DRG.BuffFangAndClawReady) && level >= 56)
                        return DRG.FangAndClaw;
                    if (SearchBuffArray(DRG.BuffWheelingThrustReady) && level >= 58)
                        return DRG.WheelingThrust;
                    if (SearchBuffArray(DRG.BuffDraconianFire) && level >= 76)
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
                        if (lastMove == DRK.Unleash && level >= 40)
                            return DRK.StalwartSoul;

                    return DRK.Unleash;
                }

            // PALADIN

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
                    if (SearchBuffArray(PLD.BuffRequiescat) && level >= 80)
                        return iconHook.Original(self, PLD.Confiteor);

                    if (SearchBuffArray(PLD.BuffBladeOfHonor)) 
                        return PLD.BladeOfHonor;

                    if (level >= 96)
                        return PLD.Imperator;

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

                    return WAR.HeavySwing;
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

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WarriorIRCombo))
                if (actionID == WAR.InnerRelease || actionID == WAR.Berserk)
                {
                    if (SearchBuffArray(WAR.BuffPrimalRendReady))
                        return WAR.PrimalRend;
                    return iconHook.Original(self, actionID);
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
                    if (SearchBuffArray(SAM.BuffMeikyoShisui))
                        return SAM.Yukikaze;
                    if (comboTime > 0)
                        if (lastMove == SAM.Hakaze && level >= 50)
                            return SAM.Yukikaze;

                    if (level >= 92)
                        return SAM.Gyuofu;
                    return SAM.Hakaze;
                }

            // Replace Gekko with Gekko combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiGekkoCombo))
                if (actionID == SAM.Gekko)
                {
                    if (SearchBuffArray(SAM.BuffMeikyoShisui))
                        return SAM.Gekko;
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 4)
                            return SAM.Jinpu;
                        if (lastMove == SAM.Jinpu && level >= 30)
                            return SAM.Gekko;
                    }

                    if (level >= 92)
                        return SAM.Gyuofu;
                    return SAM.Hakaze;
                }

            // Replace Kasha with Kasha combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiKashaCombo))
                if (actionID == SAM.Kasha)
                {
                    if (SearchBuffArray(SAM.BuffMeikyoShisui))
                        return SAM.Kasha;
                    if (comboTime > 0)
                    {
                        if (lastMove == SAM.Hakaze && level >= 18)
                            return SAM.Shifu;
                        if (lastMove == SAM.Shifu && level >= 40)
                            return SAM.Kasha;
                    }

                    if (level >= 92) 
                        return SAM.Gyuofu;
                    return SAM.Hakaze;
                }

            // Replace Mangetsu with Mangetsu combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiMangetsuCombo))
                if (actionID == SAM.Mangetsu)
                {
                    if (SearchBuffArray(SAM.BuffMeikyoShisui))
                        return SAM.Mangetsu;
                    if (comboTime > 0)
                        if ((lastMove == SAM.Fuga || lastMove == SAM.Fuko) && level >= 35)
                            return SAM.Mangetsu;
                    if (level >= 86)
                        return SAM.Fuko;
                    return SAM.Fuga;
                }

            // Replace Oka with Oka combo
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiOkaCombo))
                if (actionID == SAM.Oka)
                {
                    if (SearchBuffArray(SAM.BuffMeikyoShisui))
                        return SAM.Oka;
                    if (comboTime > 0)
                        if ((lastMove == SAM.Fuga || lastMove == SAM.Fuko) && level >= 45)
                            return SAM.Oka;
                    if (level >= 86)
                        return SAM.Fuko;
                    return SAM.Fuga;
                }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SamuraiOgiCombo))
                if (actionID == SAM.Ikishoten)
                {
                    if (SearchBuffArray(SAM.BuffOgiNamikiriReady))
                        return SAM.OgiNamikiri;
                    if (JobGauges.Get<SAMGauge>().Kaeshi == Kaeshi.NAMIKIRI)
                        return SAM.KaeshiNamikiri;

                    if (SearchBuffArray(SAM.BuffZanshinReady)) return SAM.Zanshin;

                    return SAM.Ikishoten;
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
                    {
                        if (lastMove == NIN.DeathBlossom && level >= 52)
                            return NIN.HakkeM;
                    }
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
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerGnashingFangCont))
                if (actionID == GNB.GnashingFang)
                {
                    if (level >= GNB.LevelContinuation)
                    {
                        if (SearchBuffArray(GNB.BuffReadyToRip))
                            return GNB.JugularRip;
                        if (SearchBuffArray(GNB.BuffReadyToTear))
                            return GNB.AbdomenTear;
                        if (SearchBuffArray(GNB.BuffReadyToGouge))
                            return GNB.EyeGouge;
                    }
                    return iconHook.Original(self, GNB.GnashingFang);
                }

            // Replace Burst Strike with Continuation
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerBurstStrikeCont))
                if (actionID == GNB.BurstStrike)
                {
                    if (level >= GNB.LevelEnhancedContinuation)
                    {
                        if (SearchBuffArray(GNB.BuffReadyToBlast))
                            return GNB.Hypervelocity;
                    }
                    return GNB.BurstStrike;
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

            // Replace Fated Brand with Continuation
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.GunbreakerFatedCircleCont))
                if (actionID == GNB.FatedCircle)
                {
                    if (level >= GNB.LevelEnhancedContinuation2)
                    {
                        if (SearchBuffArray(GNB.BuffReadyToRaze))
                            return GNB.FatedBrand;
                    }
                    return GNB.FatedCircle;
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
                    var gauge = JobGauges.Get<MCHGauge>();
                    if (gauge.IsOverheated && level >= 35)
                        if (level >= 68)
                            return MCH.BlazingShot;
                        else
                            return MCH.HeatBlast;
                    return MCH.Hypercharge;
                }

            // Replace Spread Shot with Auto Crossbow when overheated.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.MachinistSpreadShotFeature))
                if (actionID == MCH.SpreadShot || actionID == MCH.Scattergun)
                {
                    if (JobGauges.Get<MCHGauge>().IsOverheated && level >= 52)
                        return MCH.AutoCrossbow;
                    if (level >= 82)
                        return MCH.Scattergun;
                    return MCH.SpreadShot;
                }

            // BLACK MAGE

            // B4 and F4 change to each other depending on stance, as do Flare and Freeze.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackEnochianFeature))
            {
                if (actionID == BLM.Fire4 || actionID == BLM.Blizzard4)
                {
                    var gauge = JobGauges.Get<BLMGauge>();
                    if (gauge.InUmbralIce && level >= 58)
                        return BLM.Blizzard4;
                    if (level >= 60)
                        return BLM.Fire4;
                }

                if (actionID == BLM.Flare || actionID == BLM.Freeze)
                {
                    var gauge = JobGauges.Get<BLMGauge>();
                    if (gauge.InAstralFire && level >= 50)
                        return BLM.Flare;
                    return BLM.Freeze;
                }
            }

            // Ley Lines and BTL
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BlackLeyLines))
                if (actionID == BLM.LeyLines)
                {
                    if (SearchBuffArray(BLM.BuffLeyLines) && level >= 62)
                        return BLM.BTL;
                    return BLM.LeyLines;
                }

            // ASTROLOGIAN

            /* Old Astro
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.AstrologianCardsOnDrawFeature))
                if (actionID == AST.Play)
                {
                    var gauge = JobGauges.Get<ASTGauge>();
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
            */

            // SUMMONER
            // Change Fester into Energy Drain
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerEDFesterCombo))
                if (actionID == SMN.Fester)
                {
                    if (!JobGauges.Get<SMNGauge>().HasAetherflowStacks)
                        return SMN.EnergyDrain;
                    return SMN.Fester;
                }

            //Change Painflare into Energy Syphon
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.SummonerESPainflareCombo))
                if (actionID == SMN.Painflare)
                {
                    if (!JobGauges.Get<SMNGauge>().HasAetherflowStacks)
                        return SMN.EnergySyphon;
                    return SMN.Painflare;
                }

            // SCHOLAR

            // Change Fey Blessing into Consolation when Seraph is out.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarSeraphConsolationFeature))
                if (actionID == SCH.FeyBless)
                {
                    if (JobGauges.Get<SCHGauge>().SeraphTimer > 0) return SCH.Consolation;
                    return SCH.FeyBless;
                }

            // Change Energy Drain into Aetherflow when you have no more Aetherflow stacks.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ScholarEnergyDrainFeature))
                if (actionID == SCH.EnergyDrain)
                {
                    if (JobGauges.Get<SCHGauge>().Aetherflow == 0) return SCH.Aetherflow;
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
                    if (SearchBuffArray(DNC.BuffFlourishingFlow) || SearchBuffArray(DNC.BuffSilkenFlow))
                        return DNC.Bloodshower;
                    return DNC.Bladeshower;
                }

                if (actionID == DNC.RisingWindmill)
                {
                    if (SearchBuffArray(DNC.BuffFlourishingSymmetry) || SearchBuffArray(DNC.BuffSilkenSymmetry))
                        return DNC.RisingWindmill;
                    return DNC.Windmill;
                }
            }

            // Fan Dance changes into Fan Dance 3 while flourishing.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerFanDanceCombo))
            {
                if (actionID == DNC.FanDance1)
                {
                    if (SearchBuffArray(DNC.BuffThreefoldFanDance))
                        return DNC.FanDance3;
                    return DNC.FanDance1;
                }

                // Fan Dance 2 changes into Fan Dance 3 while flourishing.
                if (actionID == DNC.FanDance2)
                {
                    if (SearchBuffArray(DNC.BuffThreefoldFanDance))
                        return DNC.FanDance3;
                    return DNC.FanDance2;
                }
            }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerFanDance4Combo))
            {
                if (actionID == DNC.Flourish)
                {
                    if (SearchBuffArray(DNC.BuffFourfoldFanDance))
                        return DNC.FanDance4;
                    return DNC.Flourish;
                }
            }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.DancerDevilmentCombo))
            {
                if (actionID == DNC.Devilment)
                {
                    if (SearchBuffArray(DNC.BuffStarfallDanceReady))
                        return DNC.StarfallDance;
                    return DNC.Devilment;
                }
            }

            // WHM

            // Replace Solace with Misery when full blood lily
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageSolaceMiseryFeature))
                if (actionID == WHM.Solace)
                {
                    if (JobGauges.Get<WHMGauge>().BloodLily == 3)
                        return WHM.Misery;
                    return WHM.Solace;
                }

            // Replace Solace with Misery when full blood lily
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.WhiteMageRaptureMiseryFeature))
                if (actionID == WHM.Rapture)
                {
                    if (JobGauges.Get<WHMGauge>().BloodLily == 3)
                        return WHM.Misery;
                    return WHM.Rapture;
                }

            // BARD

            // Replace HS/BS with SS/RA when procced.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardStraightShotUpgradeFeature))
                if (actionID == BRD.HeavyShot || actionID == BRD.BurstShot)
                {
                    if (SearchBuffArray(BRD.BuffStraightShotReady))
                    {
                        if (level >= 70) return BRD.RefulgentArrow;
                        return BRD.StraightShot;
                    }

                    if (level >= 76) return BRD.BurstShot;
                    return BRD.HeavyShot;
                }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.BardAoEUpgradeFeature))
                if (actionID == BRD.QuickNock || actionID == BRD.Ladonsbite)
                {
                    if (SearchBuffArray(BRD.BuffShadowbiteReady))
                    {
                        return BRD.Shadowbite;
                    }

                    return iconHook.Original(self, BRD.QuickNock);
                }

            // MONK
            // haha you get nothing now

            // RED MAGE

            // Replace Veraero/thunder 2 with Impact when Dualcast is active
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageAoECombo))
            {
                if (actionID == RDM.Veraero2)
                {
                    if (SearchBuffArray(RDM.BuffSwiftcast) || SearchBuffArray(RDM.BuffDualcast) || 
                        SearchBuffArray(RDM.BuffAcceleration) || SearchBuffArray(RDM.BuffChainspell))
                    {
                        if (level >= 66) return RDM.Impact;
                        return RDM.Scatter;
                    }
                    return iconHook.Original(self, RDM.Veraero2);
                }

                if (actionID == RDM.Verthunder2)
                {
                    if (SearchBuffArray(RDM.BuffSwiftcast) || SearchBuffArray(RDM.BuffDualcast) ||
                        SearchBuffArray(RDM.BuffAcceleration) || SearchBuffArray(RDM.BuffChainspell))
                    {
                        if (level >= 66) return RDM.Impact;
                        return RDM.Scatter;
                    }
                    return iconHook.Original(self, RDM.Verthunder2);
                }
            }

            // Replace Redoublement with Redoublement combo, Enchanted if possible.
            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageMeleeCombo))
                if (actionID == RDM.Redoublement)
                {
                    var gauge = JobGauges.Get<RDMGauge>();
                    if ((lastMove == RDM.Riposte || lastMove == RDM.ERiposte) && level >= 35)
                    {
                        if (gauge.BlackMana >= 15 && gauge.WhiteMana >= 15 || SearchBuffArray(RDM.BuffMagickedSwordplay))
                            return RDM.EZwerchhau;
                        return RDM.Zwerchhau;
                    }

                    if (lastMove == RDM.Zwerchhau && level >= 50)
                    {
                        if (gauge.BlackMana >= 15 && gauge.WhiteMana >= 15 || SearchBuffArray(RDM.BuffMagickedSwordplay))
                            return RDM.ERedoublement;
                        return RDM.Redoublement;
                    }

                    if (gauge.BlackMana >= 20 && gauge.WhiteMana >= 20 || SearchBuffArray(RDM.BuffMagickedSwordplay))
                        return RDM.ERiposte;
                    return RDM.Riposte;
                }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.RedMageVerprocCombo))
            {
                if (actionID == RDM.Verstone)
                {
                    if (level >= 80 && (lastMove == RDM.Verflare || lastMove == RDM.Verholy)) return RDM.Scorch;
                    if (level >= 90 && lastMove == RDM.Scorch) return RDM.Resolution;

                    if (SearchBuffArray(RDM.BuffVerstoneReady)) return RDM.Verstone;
                    if (level < 62) return RDM.Jolt;
                    if (level < 84) return RDM.Jolt2;
                    return RDM.Jolt3;
                }
                if (actionID == RDM.Verfire)
                {
                    if (level >= 80 && (lastMove == RDM.Verflare || lastMove == RDM.Verholy)) return RDM.Scorch;
                    if (level >= 90 && lastMove == RDM.Scorch) return RDM.Resolution;

                    if (SearchBuffArray(RDM.BuffVerfireReady)) return RDM.Verfire;
                    if (level < 62) return RDM.Jolt;
                    if (level < 84) return RDM.Jolt2;
                    return RDM.Jolt3;
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
            }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ReaperRegressFeature))
            {
                if (actionID == RPR.Egress || actionID == RPR.Ingress)
                {
                    if (SearchBuffArray(RPR.Buffs.Threshold)) return RPR.Regress;
                    return actionID;
                }
            }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ReaperEnshroudCombo))
            {
                if (actionID == RPR.Enshroud)
                {
                    if (SearchBuffArray(RPR.Buffs.Enshrouded)) return RPR.Communio;
                    return actionID;
                }
            }

            if (Configuration.ComboPresets.HasFlag(CustomComboPreset.ReaperArcaneFeature))
            {
                if (actionID == RPR.ArcaneCircle)
                {
                    if (SearchBuffArray(RPR.Buffs.ImSac1) ||
                        SearchBuffArray(RPR.Buffs.ImSac2))
                        return RPR.PlentifulHarvest;
                    return actionID;
                }
            }

            return iconHook.Original(self, actionID);
        }

        private bool SearchBuffArray(ushort needle)
        {
            if (needle == 0) return false;
            var buffs = clientState.LocalPlayer.StatusList;
            for (var i = 0; i < buffs.Length; i++)
                if (buffs[i].StatusId == needle)
                    return true;
            return false;
        }        
    }
}
