using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("DNC")]
	public class DNC : Job
	{
		private static JobAction
			Bladeshower		= new JobAction(15994),
			Bloodshower		= new JobAction(15996),
			Windmill		= new JobAction(15993),
			RisingWindmill	= new JobAction(15995),
			FanDance1		= new JobAction(16007),
			FanDance2		= new JobAction(16008),
			FanDance3		= new JobAction(16009),
			FanDance4		= new JobAction(25791),
			Flourish		= new JobAction(16013),
			Devilment		= new JobAction(16011),
			StarfallDance	= new JobAction(25792);
		private const ushort
			BuffFlourishingSymmetry = 3017,
			BuffFlourishingFlow = 3018,
			BuffThreefoldFanDance = 1820,
			BuffFourfoldFanDance = 2699,
			BuffStarfallDanceReady = 2700,
			BuffSilkenSymmetry = 2693,
			BuffSilkenFlow = 2694;
	
		public DNC() : base()
		{
			Bloodshower.SetCondition(() => HasBuff(BuffFlourishingFlow, BuffSilkenFlow));
			RisingWindmill.SetCondition(() => HasBuff(BuffFlourishingSymmetry, BuffSilkenSymmetry));
			FanDance3.SetCondition(() => HasBuff(BuffThreefoldFanDance));
			FanDance4.SetCondition(() => HasBuff(BuffFourfoldFanDance));
			StarfallDance.SetCondition(() => HasBuff(BuffStarfallDanceReady));

			// AoE GCDs are split into two buttons, because priority matters
			// differently in different single-target moments. Thanks yoship.
			// Replaces each GCD with its procced version.
			ForFlag(CustomComboPreset.DancerAoeGcdFeature)
				.ForComboActions(Bladeshower, Bloodshower)
				.ForComboActions(Windmill, RisingWindmill);

			// Fan Dance changes into Fan Dance 3 while flourishing.
			ForFlag(CustomComboPreset.DancerFanDanceCombo)
				.UpgradeAction(FanDance1, FanDance3)
				.UpgradeAction(FanDance2, FanDance3);

			ForFlag(CustomComboPreset.DancerFanDance4Combo)
				.UpgradeAction(Flourish, FanDance4);

			ForFlag(CustomComboPreset.DancerDevilmentCombo)
				.UpgradeAction(Devilment, StarfallDance);
		}
	}
}
