namespace XIVComboPlugin.JobActions
{
    public static class DNC
    {
        public const uint
            Bladeshower = 15994,
            Bloodshower = 15996,
            Windmill = 15993,
            RisingWindmill = 15995,
            FanDance1 = 16007,
            FanDance2 = 16008,
            FanDance3 = 16009,
            FanDance4 = 25791,
            Flourish = 16013,
            Devilment = 16011,
            StarfallDance = 25792,
            Cascade = 15989,
            Fountain = 15990,
            ReverseCascade = 15991,
            FountainFall = 15992,
            Pirouette = 16002,
            Jete = 16001;
        public const ushort
            BuffFlourishingSymmetry = 3017,
            BuffFlourishingFlow = 3018,
            BuffThreefoldFanDance = 1820,
            BuffFourfoldFanDance = 2699,
            BuffStarfallDanceReady = 2700,
            BuffSilkenSymmetry = 2693,
            BuffSilkenFlow = 2694,
            BuffStandardStep = 1818,
            BuffTechnicalStep = 1819,
            BuffImprovisation = 1827;
        public static readonly short[]
            DancingBuffs = { BuffImprovisation, BuffStandardStep, BuffTechnicalStep };
    }
}
