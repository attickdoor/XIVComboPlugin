using Lumina.Excel.GeneratedSheets;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("DRK")]
	public class DRK : Job
	{
		private static JobAction
			HardSlash		= new JobAction(3617),
			SyphonStrike	= new JobAction(3623,	2),
			Souleater		= new JobAction(3632,	26),
			Unleash			= new JobAction(3621),
			StalwartSoul	= new JobAction(16468,	40);

		public DRK() : base()
		{
			SyphonStrike.SetCondition(() => LastMoveWasInCombo(HardSlash));
			Souleater.SetCondition(() => LastMoveWasInCombo(SyphonStrike));
			StalwartSoul.SetCondition(() => LastMoveWasInCombo(Unleash));

			// Replace Souleater with Souleater combo chain
			ForFlag(CustomComboPreset.DarkSouleaterCombo)
				.ForComboActions(HardSlash, SyphonStrike, Souleater);

			// Replace Stalwart Soul with Stalwart Soul combo chain
			ForFlag(CustomComboPreset.DarkStalwartSoulCombo)
				.ForComboActions(Unleash, StalwartSoul);
		}
	}
}
