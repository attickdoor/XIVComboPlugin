﻿using System;

namespace XIVComboPlugin
{
    //TODO: reorganize the numbers lol lmao
    [Flags]
    public enum CustomComboPreset : long
    {
        None = 0,

        // DRAGOON
        //[CustomComboInfo("Jump + Mirage Dive", "Replace (High) Jump with Mirage Dive when Dive Ready", 22)]
        //DragoonJumpFeature = 1L << 44,

        [CustomComboInfo("Coerthan Torment Combo", "Replace Coerthan Torment with its combo chain", 22)]
        DragoonCoerthanTormentCombo = 1L << 0,

        [CustomComboInfo("Chaos Thrust Combo", "Replace Chaos Thrust with its combo chain", 22)]
        DragoonChaosThrustCombo = 1L << 1,

        [CustomComboInfo("Full Thrust Combo", "Replace Full Thrust with its combo chain", 22)]
        DragoonFullThrustCombo = 1L << 2,

        // DARK KNIGHT
        [CustomComboInfo("Souleater Combo", "Replace Souleater with its combo chain", 32)]
        DarkSouleaterCombo = 1L << 3,

        [CustomComboInfo("Stalwart Soul Combo", "Replace Stalwart Soul with its combo chain", 32)]
        DarkStalwartSoulCombo = 1L << 4,

        // PALADIN
        [CustomComboInfo("Royal Authority Combo", "Replace Royal Authority/Rage of Halone with its combo chain", 19)]
        PaladinRoyalAuthorityCombo = 1L << 5,

        [CustomComboInfo("Prominence Combo", "Replace Prominence with its combo chain", 19)]
        PaladinProminenceCombo = 1L << 6,

        [CustomComboInfo("Requiescat Confiteor", "Replace Requiescat with Confiteor while under the effect of Requiescat", 19)]
        PaladinRequiescatCombo = 1L << 7,

        // WARRIOR
        [CustomComboInfo("Storms Path Combo", "Replace Storms Path with its combo chain", 21)]
        WarriorStormsPathCombo = 1L << 8,

        [CustomComboInfo("Storms Eye Combo", "Replace Storms Eye with its combo chain", 21)]
        WarriorStormsEyeCombo = 1L << 9,

        [CustomComboInfo("Mythril Tempest Combo", "Replace Mythril Tempest with its combo chain", 21)]
        WarriorMythrilTempestCombo = 1L << 10,

        [CustomComboInfo("IR to Primal Rend", "Replace Inner Release with Primal Rend when Primal Rend Ready", 21)]
        WarriorIRCombo = 1L << 11,

        // SAMURAI
        [CustomComboInfo("Yukikaze Combo", "Replace Yukikaze with its combo chain", 34)]
        SamuraiYukikazeCombo = 1L << 12,

        [CustomComboInfo("Gekko Combo", "Replace Gekko with its combo chain", 34)]
        SamuraiGekkoCombo = 1L << 13,

        [CustomComboInfo("Kasha Combo", "Replace Kasha with its combo chain", 34)]
        SamuraiKashaCombo = 1L << 14,

        [CustomComboInfo("Mangetsu Combo", "Replace Mangetsu with its combo chain", 34)]
        SamuraiMangetsuCombo = 1L << 15,

        [CustomComboInfo("Oka Combo", "Replace Oka with its combo chain", 34)]
        SamuraiOkaCombo = 1L << 16,

        [CustomComboInfo("Iaijutsu into Tsubame", "Replace Iaijutsu with Tsubame after using an Iaijutsu", 34)]
        SamuraiTsubameCombo = 1L << 17,

        [CustomComboInfo("Ogi Namikiri Combo", "Replace Ikishoten with Ogi Namiki and Kaeshi Namikiri when appropriate", 34)]
        SamuraiOgiCombo = 1L << 18,


        // NINJA
        [CustomComboInfo("Armor Crush Combo", "Replace Armor Crush with its combo chain", 30)]
        NinjaArmorCrushCombo = 1L << 19,

        [CustomComboInfo("Aeolian Edge Combo", "Replace Aeolian Edge with its combo chain", 30)]
        NinjaAeolianEdgeCombo = 1L << 20,

        [CustomComboInfo("Hakke Mujinsatsu Combo", "Replace Hakke Mujinsatsu with its combo chain", 30)]
        NinjaHakkeMujinsatsuCombo = 1L << 21,

        // GUNBREAKER
        [CustomComboInfo("Solid Barrel Combo", "Replace Solid Barrel with its combo chain", 37)]
        GunbreakerSolidBarrelCombo = 1L << 22,

        [CustomComboInfo("Gnashing Fang Continuation", "Put Continuation moves on Gnashing Fang when appropriate", 37)]
        GunbreakerGnashingFangCont = 1L << 23,

        [CustomComboInfo("Burst Strike Continuation", "Put Continuation moves on Burst Strike when appropriate", 37)]
        GunbreakerBurstStrikeCont = 1L << 24,

        [CustomComboInfo("Demon Slaughter Combo", "Replace Demon Slaughter with its combo chain", 37)]
        GunbreakerDemonSlaughterCombo = 1L << 25,

        // MACHINIST
        [CustomComboInfo("(Heated) Shot Combo", "Replace either form of Clean Shot with its combo chain", 31)]
        MachinistMainCombo = 1L << 26,

        [CustomComboInfo("Spread Shot Heat", "Replace Spread Shot or Scattergun with Auto Crossbow when overheated", 31)]
        MachinistSpreadShotFeature = 1L << 27,

        [CustomComboInfo("Heat Blast when overheated", "Replace Hypercharge with Heat Blast when overheated", 31)]
        MachinistOverheatFeature = 1L << 28,

        // BLACK MAGE
        [CustomComboInfo("Enochian Stance Switcher", "Change Fire 4 and Blizzard 4 to the appropriate element depending on stance, as well as Flare and Freeze", 25)]
        BlackEnochianFeature = 1L << 29,

        //[CustomComboInfo("(Between the) Ley Lines", "Change Ley Lines into BTL when Ley Lines is active", 25)]
        //BlackLeyLines = 1L << 28,

        // ASTROLOGIAN
        [CustomComboInfo("Draw on Play", "Play turns into Draw when no card is drawn, as well as the usual Play behavior", 33)]
        AstrologianCardsOnDrawFeature = 1L << 30,

        [CustomComboInfo("Benefic? My beloved.", "Replace Benefic 2 with Benefic 1 when sync'd below 26.",33)]
        AstrologianHealLevelSyncFeature = 1L << 31,

        // SUMMONER

        [CustomComboInfo("ED Fester", "Change Fester into Energy Drain when out of Aetherflow stacks", 27)]
        SummonerEDFesterCombo = 1L << 32,

        [CustomComboInfo("ES Painflare", "Change Painflare into Energy Syphon when out of Aetherflow stacks", 27)]
        SummonerESPainflareCombo = 1L << 33,

        // SCHOLAR
        //[CustomComboInfo("Seraph Fey Blessing/Consolation", "Change Fey Blessing into Consolation when Seraph is out", 28)]
        //ScholarSeraphConsolationFeature = 1L << 29,
        [CustomComboInfo("ED Aetherflow", "Change Energy Drain into Aetherflow when you have no more Aetherflow stacks", 28)]
        ScholarEnergyDrainFeature = 1L << 34,

        [CustomComboInfo("Physick has a use?", "Replace Adlo with Physick when sync'd below 30.", 28)]
        ScholarHealLevelSyncFeature = 1L << 35,

        // DANCER
        [CustomComboInfo("AoE GCD procs", "DNC AoE procs turn into their normal abilities when not procced", 38)]
        DancerAoeGcdFeature = 1L << 36,

        [CustomComboInfo("Fan Dance Combos", "Change Fan Dance and Fan Dance 2 into Fan Dance 3 while flourishing", 38)]
        DancerFanDanceCombo = 1L << 37,

        [CustomComboInfo("Fan Dance IV", "Change Flourish into Fan Dance IV while flourishing", 38)]
        DancerFanDance4Combo = 1L << 38,

        [CustomComboInfo("Devilment into Starfall", "Change Devilment into Starfall Dance while under the effect of Flourishing Starfall", 38)]
        DancerDevilmentCombo = 1L << 39,

        // WHITE MAGE
        [CustomComboInfo("Solace into Misery", "Replaces Afflatus Solace with Afflatus Misery when Misery is ready to be used", 24)]
        WhiteMageSolaceMiseryFeature = 1L << 40,

        [CustomComboInfo("Rapture into Misery", "Replaces Afflatus Rapture with Afflatus Misery when Misery is ready to be used", 24)]
        WhiteMageRaptureMiseryFeature = 1L << 41,

        [CustomComboInfo("Cure 1 is back!", "Replace Cure 2 with Cure 1 when sync'd below 30.", 24)]
        WhiteMageHealLevelSyncFeature = 1L << 42,

        // BARD
        [CustomComboInfo("Heavy Shot into Straight Shot", "Replaces Heavy Shot/Burst Shot with Straight Shot/Refulgent Arrow when procced", 23)]
        BardStraightShotUpgradeFeature = 1L << 43,

        [CustomComboInfo("Quick Nock into Shadowbite", "Replaces Quick Nock/Ladonsbite with Shadowbite when procced", 23)]
        BardAoEUpgradeFeature = 1L << 44,

        // MONK
        // you get nothing, you lose, have a nice day etc

        // RED MAGE
        [CustomComboInfo("Red Mage AoE Combo", "Replaces Veraero/thunder 2 with Impact when Dualcast or Swiftcast are active", 35)]
        RedMageAoECombo = 1L << 45,

        [CustomComboInfo("Redoublement combo", "Replaces Redoublement with its combo chain, following enchantment rules", 35)]
        RedMageMeleeCombo = 1L << 46,

        [CustomComboInfo("Verproc into Jolt", "Replaces Verstone/Verfire with Jolt/Scorch when no proc is available.", 35)]
        RedMageVerprocCombo = 1L << 47,

        // REAPER
        [CustomComboInfo("Slice Combo", "Replace Slice with its combo chain.", 39)]
        ReaperSliceCombo = 1L << 48,

        [CustomComboInfo("Scythe Combo", "Replace Spinning Scythe with its combo chain.", 39)]
        ReaperScytheCombo = 1L << 49,

        [CustomComboInfo("Double Regress", "Regress always replaces both Hell's Egress and Hell's Ingress.", 39)]
        ReaperRegressFeature = 1L << 50,

        [CustomComboInfo("Enshroud Combo", "Replace Enshroud with Communio while you are Enshrouded.", 39)]
        ReaperEnshroudCombo = 1L << 51,

        [CustomComboInfo("Arcane Circle Combo", "Replace Arcane Circle with Plentiful Harvest while you have Immortal Sacrifice.", 39)]
        ReaperArcaneFeature = 1L << 52,

        //PICTOMANCER
        [CustomComboInfo("Red to Blue combo","Replace Fire combo with Blizzard combo when Subtractive Pallet is active.",42)]
        PictoSubtractivePallet = 1L << 53,

        [CustomComboInfo("Motifs and Muses", "Replace Motifs with their relevant Muses.", 42)]
        PictoMotifMuseFeature = 1L << 54,

        [CustomComboInfo("Starry Sky to Star Prism", "Replace Starry Sky with Star Prism.", 42)]
        PictoStarrySkyCombo = 1L << 55,

        [CustomComboInfo("Holy white to Comet Black", "Replace Holy in White with Comet in black when Monochrome Tones is active.", 42)]
        PictoHolyWhiteCombo = 1L << 56,
    }

    public class CustomComboInfoAttribute : Attribute
    {
        internal CustomComboInfoAttribute(string fancyName, string description, byte classJob)
        {
            FancyName = fancyName;
            Description = description;
            ClassJob = classJob;
        }

        public string FancyName { get; }
        public string Description { get; }
        public byte ClassJob { get; }

    }
}
