using Dalamud.Game.ClientState.JobGauge.Types;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("WHM", "CNJ")]
	public class WHM : Job
	{
		private static JobAction
			Solace	= new JobAction(16531),
			Rapture	= new JobAction(16534),
			Misery	= new JobAction(16535);

		public WHM() : base()
		{
			WHMGauge gauge = XIVComboPlugin.JobGauges.Get<WHMGauge>();
			Misery.SetCondition(() => gauge.BloodLily >= 3);
			// Replace Solace with Misery when full blood lily
			ForFlag(CustomComboPreset.WhiteMageSolaceMiseryFeature)
				.UpgradeAction(Solace, Misery);

			// Replace Rapture with Misery when full blood lily
			ForFlag(CustomComboPreset.WhiteMageRaptureMiseryFeature)
				.UpgradeAction(Rapture, Misery);
		}
	}
}
