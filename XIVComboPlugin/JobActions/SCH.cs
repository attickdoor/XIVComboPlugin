using Dalamud.Game.ClientState.JobGauge.Types;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("SCH")]
	public class SCH : Job
	{
		private static JobAction
			FeyBless	= new JobAction(16543),
			Consolation	= new JobAction(16546),
			EnergyDrain	= new JobAction(167),
			Aetherflow	= new JobAction(166);
	
		public SCH() : base()
		{
			SCHGauge gauge = XIVComboPlugin.JobGauges.Get<SCHGauge>();
			Consolation.SetCondition(() => gauge.SeraphTimer > 0);
			Aetherflow.SetCondition(() => gauge.Aetherflow == 0);

			// Change Fey Blessing into Consolation when Seraph is out.
			ForFlag(CustomComboPreset.ScholarSeraphConsolationFeature)
				.UpgradeAction(FeyBless, Consolation);

			// Change Energy Drain into Aetherflow when you have no more Aetherflow stacks.
			ForFlag(CustomComboPreset.ScholarEnergyDrainFeature)
				.UpgradeAction(EnergyDrain, Aetherflow);
		}
	}
}
