using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("WAR", "MRD")]
	public class WAR : Job
	{
		private static JobAction
			HeavySwing		= new JobAction(31		),
			Maim			= new JobAction(37,		4),
			StormsPath		= new JobAction(42,		26),
			StormsEye		= new JobAction(45,		50),
			Overpower		= new JobAction(41		),
			MythrilTempest	= new JobAction(16462,	40),
			InnerRelease	= new JobAction(7389	),
			PrimalRend		= new JobAction(25753	),
			Berserk			= new JobAction(38		);

		private const ushort
			BuffPrimalRendReady = 2624;

		public WAR() : base()
		{
			Maim.SetCondition(() => LastMoveWasInCombo(HeavySwing));
			StormsPath.SetCondition(() => LastMoveWasInCombo(Maim));
			StormsEye.SetCondition(() => LastMoveWasInCombo(Maim));
			MythrilTempest.SetCondition(() => LastMoveWasInCombo(Overpower));

			// Replace Storm's Path with Storm's Path combo
			ForFlag(CustomComboPreset.WarriorStormsPathCombo)
				.ForComboActions(HeavySwing, Maim, StormsPath);
			// Replace Storm's Eye with Storm's Eye combo
			ForFlag(CustomComboPreset.WarriorStormsEyeCombo)
				.ForComboActions(HeavySwing, Maim, StormsEye);
			// Replace Mythril Tempest with Mythril Tempest combo
			ForFlag(CustomComboPreset.WarriorMythrilTempestCombo)
				.ForComboActions(Overpower, MythrilTempest);
			ForFlag(CustomComboPreset.WarriorIRCombo)
				.ForAction(
					() => HasBuff(BuffPrimalRendReady) ? PrimalRend : currentAction,
					InnerRelease, Berserk
				);
		}
	}
}
