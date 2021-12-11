namespace XIVComboPlugin.JobActions
{
    public static class RPR
    {
        public const byte JobID = 39;

        public const uint
            // Single Target
            Slice = 24373,
            WaxingSlice = 24374,
            InfernalSlice = 24375,
            // AoE
            SpinningScythe = 24376,
            NightmareScythe = 24377,
            PlentifulHarvest = 24385,
            // Shroud
            Enshroud = 24394,
            Communio = 24398,
            // Buffs
            ArcaneCircle = 24405;

        public static class Buffs
        {
            public const short
                Enshrouded = 2593,
                ImmortalSacrifice = 2592,
                CircleOfSacrifice = 2600;
        }

        public static class Debuffs
        {
            public const short
                Placeholder = 0;
        }

        public static class Levels
        {
            public const byte
                Slice = 1,
                WaxingSlice = 5,
                SpinningScythe = 25,
                InfernalSlice = 30,
                NightmareScythe = 45,
                Enshroud = 80,
                PlentifulHarvest = 88,
                Communio = 90;
        }
    }
}