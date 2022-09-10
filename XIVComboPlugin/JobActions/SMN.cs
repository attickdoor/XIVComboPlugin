using Dalamud.Game.ClientState.JobGauge.Types;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("SMN")]
	public class SMN : Job
	{
		private static JobAction
			Deathflare			= new JobAction(3582),
			EnkindlePhoenix		= new JobAction(16516),
			EnkindleBahamut		= new JobAction(7429),
			DWT			        = new JobAction(3581),
			SummonBahamut		= new JobAction(7427),
			FBTLow			    = new JobAction(16513),
			FBTHigh			    = new JobAction(16549),
			Ruin1			    = new JobAction(163),
			Ruin3			    = new JobAction(3579),
			BrandOfPurgatory	= new JobAction(16515),
			FountainOfFire		= new JobAction(16514),
			Fester			    = new JobAction(181),
			EnergyDrain			= new JobAction(16508),
			Painflare			= new JobAction(3578),
			EnergySyphon		= new JobAction(16510,	52);
	
		public SMN() : base()
		{
			SMNGauge gauge = XIVComboPlugin.JobGauges.Get<SMNGauge>();
			EnergyDrain.SetCondition(() => !gauge.HasAetherflowStacks);
			EnergySyphon.SetCondition(() => CanUse(EnergyDrain));

			// Change Fester into Energy Drain
			ForFlag(CustomComboPreset.SummonerEDFesterCombo)
				.UpgradeAction(Fester, EnergyDrain);

			//Change Painflare into Energy Syphon
			ForFlag(CustomComboPreset.SummonerESPainflareCombo)
				.UpgradeAction(Painflare, EnergySyphon);
		}
	}
}
