﻿using System;
using XIVComboPlugin.JobActions;

namespace XIVComboPlugin
{
    //CURRENT HIGHEST FLAG IS 57
    [Flags]
    public enum CustomComboPreset : long
    {
        None = 0,

        // DRAGOON
        [CustomComboInfo("Jump + Mirage Dive", "Replace (High) Jump with Mirage Dive when Dive Ready", 22, new uint[]{ DRG.Jump, DRG.HighJump })]
        DragoonJumpFeature = 1L << 44,

        [CustomComboInfo("BOTD Into Stardiver", "Replace Blood of the Dragon with Stardiver when in Life of the Dragon", 22, new uint[] { DRG.BOTD })]
        DragoonBOTDFeature = 1L << 46,

        [CustomComboInfo("Coerthan Torment Combo", "Replace Coerthan Torment with its combo chain", 22, new uint[] { DRG.CTorment })]
        DragoonCoerthanTormentCombo = 1L << 0,

        [CustomComboInfo("Chaos Thrust Combo", "Replace Chaos Thrust with its combo chain", 22, new uint[] { DRG.ChaosThrust })]
        DragoonChaosThrustCombo = 1L << 1,

        [CustomComboInfo("Full Thrust Combo", "Replace Full Thrust with its combo chain", 22, new uint[] { DRG.FullThrust })]
        DragoonFullThrustCombo = 1L << 2,

        // DARK KNIGHT
        [CustomComboInfo("Souleater Combo", "Replace Souleater with its combo chain", 32, new uint[] { DRK.Souleater })]
        DarkSouleaterCombo = 1L << 3,

        [CustomComboInfo("Stalwart Soul Combo", "Replace Stalwart Soul with its combo chain", 32, new uint[] { DRK.StalwartSoul })]
        DarkStalwartSoulCombo = 1L << 4,

        // PALADIN
        [CustomComboInfo("Goring Blade Combo", "Replace Goring Blade with its combo chain", 19, new uint[] { PLD.GoringBlade })]
        PaladinGoringBladeCombo = 1L << 5,

        [CustomComboInfo("Royal Authority Combo", "Replace Royal Authority/Rage of Halone with its combo chain", 19, new uint[] { PLD.RoyalAuthority, PLD.RageOfHalone })]
        PaladinRoyalAuthorityCombo = 1L << 6,

        [CustomComboInfo("Prominence Combo", "Replace Prominence with its combo chain", 19, new uint[] { PLD.Prominence })]
        PaladinProminenceCombo = 1L << 7,

        [CustomComboInfo("Requiescat Confiteor", "Replace Requiescat with Confiter while under the effect of Requiescat", 19, new uint[] { PLD.Requiescat })]
        PaladinRequiescatCombo = 1L << 55,

        // WARRIOR
        [CustomComboInfo("Storms Path Combo", "Replace Storms Path with its combo chain", 21, new uint[] { WAR.StormsPath })]
        WarriorStormsPathCombo = 1L << 8,

        [CustomComboInfo("Storms Eye Combo", "Replace Storms Eye with its combo chain", 21, new uint[] { WAR.StormsEye })]
        WarriorStormsEyeCombo = 1L << 9,

        [CustomComboInfo("Mythril Tempest Combo", "Replace Mythril Tempest with its combo chain", 21, new uint[] { WAR.MythrilTempest })]
        WarriorMythrilTempestCombo = 1L << 10,

        // SAMURAI
        [CustomComboInfo("Yukikaze Combo", "Replace Yukikaze with its combo chain", 34, new uint[] { SAM.Yukikaze })]
        SamuraiYukikazeCombo = 1L << 11,

        [CustomComboInfo("Gekko Combo", "Replace Gekko with its combo chain", 34, new uint[] { SAM.Gekko })]
        SamuraiGekkoCombo = 1L << 12,

        [CustomComboInfo("Kasha Combo", "Replace Kasha with its combo chain", 34, new uint[] { SAM.Kasha })]
        SamuraiKashaCombo = 1L << 13,

        [CustomComboInfo("Mangetsu Combo", "Replace Mangetsu with its combo chain", 34, new uint[] { SAM.Mangetsu })]
        SamuraiMangetsuCombo = 1L << 14,

        [CustomComboInfo("Oka Combo", "Replace Oka with its combo chain", 34, new uint[] { SAM.Oka })]
        SamuraiOkaCombo = 1L << 15,

        [CustomComboInfo("Seigan to Third Eye", "Replace Seigan with Third Eye when not procced", 34, new uint[] { SAM.Seigan })]
        SamuraiThirdEyeFeature = 1L << 51,


        // NINJA
        [CustomComboInfo("Armor Crush Combo", "Replace Armor Crush with its combo chain", 30, new uint[] { NIN.ArmorCrush })]
        NinjaArmorCrushCombo = 1L << 17,

        [CustomComboInfo("Aeolian Edge Combo", "Replace Aeolian Edge with its combo chain", 30, new uint[] { NIN.AeolianEdge })]
        NinjaAeolianEdgeCombo = 1L << 18,

        [CustomComboInfo("Hakke Mujinsatsu Combo", "Replace Hakke Mujinsatsu with its combo chain", 30, new uint[] { NIN.HakkeM })]
        NinjaHakkeMujinsatsuCombo = 1L << 19,

        [CustomComboInfo("Dream to Assassinate", "Replace Dream Within a Dream with Assassinate when Assassinate Ready", 30, new uint[] { NIN.DWAD })]
        NinjaAssassinateFeature = 1L << 45,

        // GUNBREAKER
        [CustomComboInfo("Solid Barrel Combo", "Replace Solid Barrel with its combo chain", 37, new uint[] { GNB.SolidBarrel })]
        GunbreakerSolidBarrelCombo = 1L << 20,

        [CustomComboInfo("Wicked Talon Combo", "Replace Wicked Talon with its combo chain", 37, new uint[] { GNB.WickedTalon })]
        GunbreakerGnashingFangCombo = 1L << 21,

        [CustomComboInfo("Wicked Talon Continuation", "In addition to the Wicked Talon combo chain, put Continuation moves on Wicked Talon when appropriate", 37, new uint[] { GNB.WickedTalon })]
        GunbreakerGnashingFangCont = 1L << 52,

        [CustomComboInfo("Demon Slaughter Combo", "Replace Demon Slaughter with its combo chain", 37, new uint[] { GNB.DemonSlaughter })]
        GunbreakerDemonSlaughterCombo = 1L << 22,

        // MACHINIST
        [CustomComboInfo("(Heated) Shot Combo", "Replace either form of Clean Shot with its combo chain", 31, new uint[] { MCH.HeatedCleanShot, MCH.CleanShot })]
        MachinistMainCombo = 1L << 23,

        [CustomComboInfo("Spread Shot Heat", "Replace Spread Shot with Auto Crossbow when overheated", 31, new uint[] { MCH.SpreadShot })]
        MachinistSpreadShotFeature = 1L << 24,

        [CustomComboInfo("Heat Blast when overheated", "Replace Hypercharge with Heat Blast when overheated", 31, new uint[] { MCH.Hypercharge })]
        MachinistOverheatFeature = 1L << 47,

        // BLACK MAGE
        [CustomComboInfo("Enochian Stance Switcher", "Change Enochian to Fire 4 or Blizzard 4 depending on stance", 25, new uint[] { BLM.Enochian })]
        BlackEnochianFeature = 1L << 25,

        [CustomComboInfo("Umbral Soul/Transpose Switcher", "Change Transpose into Umbral Soul when Umbral Soul is usable", 25, new uint[] { BLM.Transpose })]
        BlackManaFeature = 1L << 26,

        [CustomComboInfo("(Between the) Ley Lines", "Change Ley Lines into BTL when Ley Lines is active", 25, new uint[] { BLM.LeyLines })]
        BlackLeyLines = 1L << 56,

        // ASTROLOGIAN
        [CustomComboInfo("Draw on Play", "Play turns into Draw when no card is drawn, as well as the usual Play behavior", 33, new uint[] { AST.Play })]
        AstrologianCardsOnDrawFeature = 1L << 27,

        // SUMMONER
        [CustomComboInfo("Demi-summon combiners", "Dreadwyrm Trance, Summon Bahamut, and Firebird Trance are now one button.\nDeathflare, Enkindle Bahamut, and Enkindle Phoenix are now one button", 27, new uint[] { SMN.DWT, SMN.Deathflare })]
        SummonerDemiCombo = 1L << 28,

        [CustomComboInfo("Brand of Purgatory Combo", "Replaces Fountain of Fire with Brand of Purgatory when under the affect of Hellish Conduit", 27, new uint[] { SMN.Ruin1, SMN.Ruin3 })]
        SummonerBoPCombo = 1L << 38,

        [CustomComboInfo("ED Fester", "Change Fester into Energy Drain when out of Aetherflow stacks", 27, new uint[] { SMN.Fester })]
        SummonerEDFesterCombo = 1L << 39,

        [CustomComboInfo("ES Painflare", "Change Painflare into Energy Syphon when out of Aetherflow stacks", 27, new uint[] { SMN.Painflare })]
        SummonerESPainflareCombo = 1L << 40,

        // SCHOLAR
        [CustomComboInfo("Seraph Fey Blessing/Consolation", "Change Fey Blessing into Consolation when Seraph is out", 28, new uint[] { SCH.FeyBless })]
        ScholarSeraphConsolationFeature = 1L << 29,

        [CustomComboInfo("ED Aetherflow", "Change Energy Drain into Aetherflow when you have no more Aetherflow stacks", 28, new uint[] { SCH.EnergyDrain })]
        ScholarEnergyDrainFeature = 1L << 37,

        [CustomComboInfo("Summon Fairy", "Change Summon Eos/Serene into Whispering Dawn/Fey Illumination when either fairy is summoned", 28, new uint[] { SCH.WhisperingDawn, SCH.FeyIllumination })]
        ScholarSummonFeature = 1L << 57,

        // DANCER
        [CustomComboInfo("AoE GCD procs", "DNC AoE procs turn into their normal abilities when not procced", 38, new uint[] { DNC.Bloodshower, DNC.RisingWindmill })]
        DancerAoeGcdFeature = 1L << 32,

        [CustomComboInfo("Fan Dance Combos", "Change Fan Dance and Fan Dance 2 into Fan Dance 3 while flourishing", 38, new uint[] { DNC.FanDance1, DNC.FanDance2 })]
        DancerFanDanceCombo = 1L << 33,

        // WHITE MAGE
        [CustomComboInfo("Solace into Misery", "Replaces Afflatus Solace with Afflatus Misery when Misery is ready to be used", 24, new uint[] { WHM.Solace })]
        WhiteMageSolaceMiseryFeature = 1L << 35,

        [CustomComboInfo("Rapture into Misery", "Replaces Afflatus Rapture with Afflatus Misery when Misery is ready to be used", 24, new uint[] { WHM.Misery })]
        WhiteMageRaptureMiseryFeature = 1L << 36,

        // BARD
        [CustomComboInfo("Wanderer's into Pitch Perfect", "Replaces Wanderer's Minuet with Pitch Perfect while in WM", 23, new uint[] { BRD.WanderersMinuet })]
        BardWandererPPFeature = 1L << 41,

        [CustomComboInfo("Heavy Shot into Straight Shot", "Replaces Heavy Shot/Burst Shot with Straight Shot/Refulgent Arrow when procced", 23, new uint[] { BRD.HeavyShot, BRD.BurstShot })]
        BardStraightShotUpgradeFeature = 1L << 42,

        // MONK
        [CustomComboInfo("Monk AoE Combo", "Replaces Rockbreaker with the AoE combo chain, or Rockbreaker when Perfect Balance is active", 20, new uint[] { MNK.Rockbreaker })]
        MnkAoECombo = 1L << 54,

        // RED MAGE
        [CustomComboInfo("Red Mage AoE Combo", "Replaces Veraero/thunder 2 with Impact when Dualcast or Swiftcast are active", 35, new uint[] { RDM.Veraero2, RDM.Verthunder2 })]
        RedMageAoECombo = 1L << 48,

        [CustomComboInfo("Redoublement combo", "Replaces Redoublement with its combo chain, following enchantment rules", 35, new uint[] { RDM.Redoublement })]
        RedMageMeleeCombo = 1L << 49,

        [CustomComboInfo("Verproc into Jolt", "Replaces Verstone/Verfire with Jolt/Scorch when no proc is available.", 35, new uint[] { RDM.Verstone, RDM.Verfire })]
        RedMageVerprocCombo = 1L << 53
    }

    public class CustomComboInfoAttribute : Attribute
    {
        internal CustomComboInfoAttribute(string fancyName, string description, byte classJob, uint[] abilities)
        {
            FancyName = fancyName;
            Description = description;
            ClassJob = classJob;
            Abilities = abilities;
        }

        public string FancyName { get; }
        public string Description { get; }
        public byte ClassJob { get; }
        public uint[] Abilities { get; }
    }
}
