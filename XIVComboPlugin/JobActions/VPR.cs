namespace XIVComboPlugin.JobActions
{
    public static class VPR
    {
        public const uint
            SteelFangs = 34606,
            DreadFangs = 34607,
            HuntersSting = 34608,
            SwiftskinsSting = 34609,
            FlankstingStrike = 34610,
            FlanksbaneFang = 34611,
            HindstingStrike = 34612,
            HindsbaneFang = 34613,

            SteelMaw = 34614,
            DreadMaw = 34615,
            HuntersBite = 34616,
            SwiftskinsBite = 34617,
            JaggedMaw = 34618,
            BloodiedMaw = 34619,

            Dreadwinder = 34620,
            HuntersCoil = 34621,
            SwiftskinsCoil = 34622,
            PitOfDread = 34623,
            HuntersDen = 34624,
            SwiftskinsDen = 34625,

            SerpentsTail = 35920,
            DeathRattle = 34634,
            LastLash = 34635,
            Twinfang = 35921,
            Twinblood = 35922,
            TwinfangBite = 34636,
            TwinfangThresh = 34638,
            TwinbloodBite = 34637,
            TwinbloodThresh = 34639,

            UncoiledFury = 34633,
            UncoiledTwinfang = 34644,
            UncoiledTwinblood = 34645,

            SerpentsIre = 34647,
            Reawaken = 34626,
            FirstGeneration = 34627,
            SecondGeneration = 34628,
            ThirdGeneration = 34629,
            FourthGeneration = 34630,
            Ouroboros = 34631,
            FirstLegacy = 34640,
            SecondLegacy = 34641,
            ThirdLegacy = 34642,
            FourthLegacy = 34643,

            WrithingSnap = 34632,
            Slither = 34646;

        public static class Buffs
        {
            public const ushort
                FlankstungVenom = 3645,
                FlanksbaneVenom = 3646,
                HindstungVenom = 3647,
                HindsbaneVenom = 3648,
                GrimhuntersVenom = 3649,
                GrimskinsVenom = 3650,
                HuntersVenom = 3657,
                SwiftskinsVenom = 3658,
                FellhuntersVenom = 3659,
                FellskinsVenom = 3660,
                PoisedForTwinfang = 3665,
                PoisedForTwinblood = 3666,
                HuntersInstinct = 3668, // Double check, might also be 4120
                Swiftscaled = 3669, // Might also be 4121
                Reawakened = 3670,
                ReadyToReawaken = 3671;
        }

        public static class Debuffs
        {
            public const ushort
                NoxiousGash = 3667;
        }
        
        public static class Levels
        {
            public const byte
                SteelFangs = 1,
                HuntersSting = 5,
                DreadFangs = 10,
                WrithingSnap = 15,
                SwiftskinsSting = 20,
                SteelMaw = 25,
                Single3rdCombo = 30, // Includes Flanksting, Flanksbane, Hindsting, and Hindsbane
                DreadMaw = 35,
                Slither = 40,
                HuntersBite = 40,
                SwiftskinsBike = 45,
                AoE3rdCombo = 50,    // Jagged Maw and Bloodied Maw
                DeathRattle = 55,
                LastLash = 60,
                Dreadwinder = 65,    // Also includes Hunter's Coil and Swiftskin's Coil
                PitOfDread = 70,     // Also includes Hunter's Den and Swiftskin's Den
                TwinsSingle = 75,    // Twinfang Bite and Twinblood Bite
                TwinsAoE = 80,       // Twinfang Thresh and Twinblood Thresh
                UncoiledFury = 82,
                UncoiledTwins = 92,  // Uncoiled Twinfang and Uncoiled Twinblood
                SerpentsIre = 86,
                EnhancedRattle = 88, // Third stack of Rattling Coil can be accumulated
                Reawaken = 90,       // Also includes First Generation through Fourth Generation
                Ouroboros = 96,
                Legacies = 100;      // First through Fourth Legacy
        }
    }
}