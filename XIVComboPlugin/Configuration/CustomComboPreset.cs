using System;

namespace XIVComboPlugin
{
    public enum CustomComboPreset : long
    {
        // DRAGOON
        [CustomComboInfo("Jump + Mirage Dive", "Replace (High) Jump with Mirage Dive when Dive Ready", 22)]
        DragoonJumpFeature,

        [CustomComboInfo("Coerthan Torment Combo", "Replace Coerthan Torment with its combo chain", 22)]
        DragoonCoerthanTormentCombo,

        [CustomComboInfo("Chaos Thrust Combo", "Replace Chaos Thrust with its combo chain", 22)]
        DragoonChaosThrustCombo,

        [CustomComboInfo("Full Thrust Combo", "Replace Full Thrust with its combo chain", 22)]
        DragoonFullThrustCombo,

        // DARK KNIGHT
        [CustomComboInfo("Souleater Combo", "Replace Souleater with its combo chain", 32)]
        DarkSouleaterCombo,

        [CustomComboInfo("Stalwart Soul Combo", "Replace Stalwart Soul with its combo chain", 32)]
        DarkStalwartSoulCombo,

        // PALADIN
        [CustomComboInfo("Royal Authority Combo", "Replace Royal Authority/Rage of Halone with its combo chain", 19)]
        PaladinRoyalAuthorityCombo,

        [CustomComboInfo("Prominence Combo", "Replace Prominence with its combo chain", 19)]
        PaladinProminenceCombo,

        [CustomComboInfo("Requiescat Confiteor", "Replace Requiescat with Confiteor while under the effect of Requiescat", 19)]
        PaladinRequiescatCombo,

        // WARRIOR
        [CustomComboInfo("Storms Path Combo", "Replace Storms Path with its combo chain", 21)]
        WarriorStormsPathCombo,

        [CustomComboInfo("Storms Eye Combo", "Replace Storms Eye with its combo chain", 21)]
        WarriorStormsEyeCombo,

        [CustomComboInfo("Mythril Tempest Combo", "Replace Mythril Tempest with its combo chain", 21)]
        WarriorMythrilTempestCombo,

        [CustomComboInfo("IR to Primal Rend", "Replace Inner Release with Primal Rend when Primal Rend Ready", 21)]
        WarriorIRCombo,

        // SAMURAI
        [CustomComboInfo("Yukikaze Combo", "Replace Yukikaze with its combo chain", 34)]
        SamuraiYukikazeCombo,

        [CustomComboInfo("Gekko Combo", "Replace Gekko with its combo chain", 34)]
        SamuraiGekkoCombo,

        [CustomComboInfo("Kasha Combo", "Replace Kasha with its combo chain", 34)]
        SamuraiKashaCombo,

        [CustomComboInfo("Mangetsu Combo", "Replace Mangetsu with its combo chain", 34)]
        SamuraiMangetsuCombo,

        [CustomComboInfo("Oka Combo", "Replace Oka with its combo chain", 34)]
        SamuraiOkaCombo,

        [CustomComboInfo("Iaijutsu into Tsubame", "Replace Iaijutsu with Tsubame after using an Iaijutsu", 34)]
        SamuraiTsubameCombo,

        [CustomComboInfo("Ogi Namikiri Combo", "Replace Ikishoten with Ogi Namiki and Kaeshi Namikiri when appropriate", 34)]
        SamuraiOgiCombo,


        // NINJA
        [CustomComboInfo("Armor Crush Combo", "Replace Armor Crush with its combo chain", 30)]
        NinjaArmorCrushCombo,

        [CustomComboInfo("Aeolian Edge Combo", "Replace Aeolian Edge with its combo chain", 30)]
        NinjaAeolianEdgeCombo,

        [CustomComboInfo("Hakke Mujinsatsu Combo", "Replace Hakke Mujinsatsu with its combo chain", 30)]
        NinjaHakkeMujinsatsuCombo,

        // GUNBREAKER
        [CustomComboInfo("Solid Barrel Combo", "Replace Solid Barrel with its combo chain", 37)]
        GunbreakerSolidBarrelCombo,

        [CustomComboInfo("Wicked Talon Combo", "Replace Wicked Talon with its combo chain", 37)]
        GunbreakerGnashingFangCombo,

        [CustomComboInfo("Wicked Talon Continuation", "In addition to the Wicked Talon combo chain, put Continuation moves on Wicked Talon when appropriate", 37)]
        GunbreakerGnashingFangCont,

        [CustomComboInfo("Burst Strike Continuation", "Put Continuation moves on Burst Strike when appropriate", 37)]
        GunbreakerBurstStrikeCont,

        [CustomComboInfo("Demon Slaughter Combo", "Replace Demon Slaughter with its combo chain", 37)]
        GunbreakerDemonSlaughterCombo,

        // MACHINIST
        [CustomComboInfo("(Heated) Shot Combo", "Replace either form of Clean Shot with its combo chain", 31)]
        MachinistMainCombo,

        [CustomComboInfo("Spread Shot Heat", "Replace Spread Shot or Scattergun with Auto Crossbow when overheated", 31)]
        MachinistSpreadShotFeature,

        [CustomComboInfo("Heat Blast when overheated", "Replace Hypercharge with Heat Blast when overheated", 31)]
        MachinistOverheatFeature,

        // BLACK MAGE
        [CustomComboInfo("Enochian Stance Switcher", "Change Fire 4 and Blizzard 4 to the appropriate element depending on stance, as well as Flare and Freeze", 25)]
        BlackEnochianFeature,

        [CustomComboInfo("(Between the) Ley Lines", "Change Ley Lines into BTL when Ley Lines is active", 25)]
        BlackLeyLines,

        // ASTROLOGIAN
        [CustomComboInfo("Draw on Play", "Play turns into Draw when no card is drawn, as well as the usual Play behavior", 33)]
        AstrologianCardsOnDrawFeature,

        // SUMMONER

        [CustomComboInfo("ED Fester", "Change Fester into Energy Drain when out of Aetherflow stacks", 27)]
        SummonerEDFesterCombo,

        [CustomComboInfo("ES Painflare", "Change Painflare into Energy Syphon when out of Aetherflow stacks", 27)]
        SummonerESPainflareCombo,
        
        // SCHOLAR
        [CustomComboInfo("Seraph Fey Blessing/Consolation", "Change Fey Blessing into Consolation when Seraph is out", 28)]
        ScholarSeraphConsolationFeature,

        [CustomComboInfo("ED Aetherflow", "Change Energy Drain into Aetherflow when you have no more Aetherflow stacks", 28)]
        ScholarEnergyDrainFeature,

        // DANCER
        [CustomComboInfo("AoE GCD procs", "DNC AoE procs turn into their normal abilities when not procced", 38)]
        DancerAoeGcdFeature,

        [CustomComboInfo("Fan Dance Combos", "Change Fan Dance and Fan Dance 2 into Fan Dance 3 while flourishing", 38)]
        DancerFanDanceCombo,

        [CustomComboInfo("Fan Dance IV", "Change Flourish into Fan Dance IV while flourishing", 38)]
        DancerFanDance4Combo,

        [CustomComboInfo("Devilment into Starfall", "Change Devilment into Starfall Dance while under the effect of Flourishing Starfall", 38)]
        DancerDevilmentCombo,

        // WHITE MAGE
        [CustomComboInfo("Solace into Misery", "Replaces Afflatus Solace with Afflatus Misery when Misery is ready to be used", 24)]
        WhiteMageSolaceMiseryFeature,

        [CustomComboInfo("Rapture into Misery", "Replaces Afflatus Rapture with Afflatus Misery when Misery is ready to be used", 24)]
        WhiteMageRaptureMiseryFeature,

        // BARD
        [CustomComboInfo("Heavy Shot into Straight Shot", "Replaces Heavy Shot/Burst Shot with Straight Shot/Refulgent Arrow when procced", 23)]
        BardStraightShotUpgradeFeature,

        [CustomComboInfo("Quick Nock into Shadowbite", "Replaces Quick Nock/Ladonsbite with Shadowbite when procced", 23)]
        BardAoEUpgradeFeature,

        // MONK
        // you get nothing, you lose, have a nice day etc

        // RED MAGE
        [CustomComboInfo("Red Mage AoE Combo", "Replaces Veraero/thunder 2 with Impact when Dualcast or Swiftcast are active", 35)]
        RedMageAoECombo,

        [CustomComboInfo("Redoublement combo", "Replaces Redoublement with its combo chain, following enchantment rules", 35)]
        RedMageMeleeCombo,

        [CustomComboInfo("Verproc into Jolt", "Replaces Verstone/Verfire with Jolt/Scorch when no proc is available.", 35)]
        RedMageVerprocCombo,

        // REAPER
        [CustomComboInfo("Slice Combo", "Replace Slice with its combo chain.", 39)]
        ReaperSliceCombo,

        [CustomComboInfo("Scythe Combo", "Replace Spinning Scythe with its combo chain.", 39)]
        ReaperScytheCombo,

        [CustomComboInfo("Double Regress", "Regress always replaces both Hell's Egress and Hell's Ingress.", 39)]
        ReaperRegressFeature,

        [CustomComboInfo("Enshroud Combo", "Replace Enshroud with Communio while you are Enshrouded.", 39)]
        ReaperEnshroudCombo,

        [CustomComboInfo("Arcane Circle Combo", "Replace Arcane Circle with Plentiful Harvest while you have Immortal Sacrifice.", 39)]
        ReaperArcaneFeature,
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
