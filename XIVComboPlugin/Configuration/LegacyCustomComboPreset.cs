using System;

namespace XIVComboPlugin
{
    [Flags]
    public enum LegacyCustomComboPreset : long
    {
        None = 0,

        // DRAGOON
        DragoonJumpFeature = 1L << 44,
        DragoonCoerthanTormentCombo = 1L << 0,
        DragoonChaosThrustCombo = 1L << 1,
        DragoonFullThrustCombo = 1L << 2,

        // DARK KNIGHT
        DarkSouleaterCombo = 1L << 3,
        DarkStalwartSoulCombo = 1L << 4,

        // PALADIN
        PaladinRoyalAuthorityCombo = 1L << 6,
        PaladinProminenceCombo = 1L << 7,
        PaladinRequiescatCombo = 1L << 55,

        // WARRIOR
        WarriorStormsPathCombo = 1L << 8,
        WarriorStormsEyeCombo = 1L << 9,

        WarriorMythrilTempestCombo = 1L << 10,
        WarriorIRCombo = 1L << 63,

        // SAMURAI
        SamuraiYukikazeCombo = 1L << 11,
        SamuraiGekkoCombo = 1L << 12,
        SamuraiKashaCombo = 1L << 13,
        SamuraiMangetsuCombo = 1L << 14,
        SamuraiOkaCombo = 1L << 15,
        SamuraiTsubameCombo = 1L << 56,
        SamuraiOgiCombo = 1L << 62,

        // NINJA
        NinjaArmorCrushCombo = 1L << 17,
        NinjaAeolianEdgeCombo = 1L << 18,
        NinjaHakkeMujinsatsuCombo = 1L << 19,

        // GUNBREAKER
        GunbreakerSolidBarrelCombo = 1L << 20,
        GunbreakerGnashingFangCombo = 1L << 21,
        GunbreakerGnashingFangCont = 1L << 52,
        GunbreakerBurstStrikeCont = 1L << 45,
        GunbreakerDemonSlaughterCombo = 1L << 22,

        // MACHINIST
        MachinistMainCombo = 1L << 23,
        MachinistSpreadShotFeature = 1L << 24,
        MachinistOverheatFeature = 1L << 47,

        // BLACK MAGE
        BlackEnochianFeature = 1L << 25,
        BlackLeyLines = 1L << 28,

        // ASTROLOGIAN
        AstrologianCardsOnDrawFeature = 1L << 27,

        // SUMMONER
        SummonerEDFesterCombo = 1L << 39,
        SummonerESPainflareCombo = 1L << 40,

        // SCHOLAR
        ScholarSeraphConsolationFeature = 1L << 29,
        ScholarEnergyDrainFeature = 1L << 37,

        // DANCER
        DancerAoeGcdFeature = 1L << 32,
        DancerFanDanceCombo = 1L << 33,
        DancerFanDance4Combo = 1L << 60,
        DancerDevilmentCombo = 1L << 61,

        // WHITE MAGE
        WhiteMageSolaceMiseryFeature = 1L << 35,
        WhiteMageRaptureMiseryFeature = 1L << 36,

        // BARD
        BardStraightShotUpgradeFeature = 1L << 42,
        BardAoEUpgradeFeature = 1L << 59,

        // MONK
        // you get nothing, you lose, have a nice day etc

        // RED MAGE
        RedMageAoECombo = 1L << 48,
        RedMageMeleeCombo = 1L << 49,
        RedMageVerprocCombo = 1L << 53,

        // REAPER
        ReaperSliceCombo = 1L << 16,
        ReaperScytheCombo = 1L << 57,
        ReaperRegressFeature = 1L << 58,
        ReaperEnshroudCombo = 1L << 26,
        ReaperArcaneFeature = 1L << 30,
    }
}
