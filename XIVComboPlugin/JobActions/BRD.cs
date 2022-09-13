using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("BRD", "ARC")]
	public class BRD : Job
	{
		private static JobAction
			WanderersMinuet = new JobAction(3559),
			PitchPerfect	= new JobAction(7404),
			HeavyShot		= new JobAction(97),
			BurstShot		= new JobAction(16495,	76),
			StraightShot	= new JobAction(98),
			RefulgentArrow	= new JobAction(7409,	70),
			QuickNock		= new JobAction(106),
			Ladonsbite		= new JobAction(25783),
			Shadowbite		= new JobAction(16494);

		private const ushort
			BuffStraightShotReady = 122,
			BuffShadowbiteReady = 3002;

		public BRD() : base()
		{
			StraightShot.SetCondition(() => HasBuff(BuffStraightShotReady));
			RefulgentArrow.SetCondition(() => CanUse(StraightShot));
			Shadowbite.SetCondition(() => HasBuff(BuffShadowbiteReady));
			// Replace HS/BS with SS/RA when procced.
			ForFlag(CustomComboPreset.BardStraightShotUpgradeFeature)
				.ForAction(
					() => GetBestAvailableAction(HeavyShot, BurstShot, StraightShot, RefulgentArrow),
					HeavyShot, BurstShot
				);

			ForFlag(CustomComboPreset.BardAoEUpgradeFeature)
				.ForAction(
					() => GetBestAvailableAction(QuickNock, Shadowbite),
					QuickNock, Ladonsbite
				);
		}
	}
}
