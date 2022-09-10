using Dalamud.Game.ClientState.JobGauge.Types;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("BLM")]
	public class BLM : Job
	{
		private static JobAction
			Blizzard4	= new JobAction(3576,	58),
			Fire4		= new JobAction(3577,	60),
			Transpose	= new JobAction(149),
			UmbralSoul	= new JobAction(16506),
			LeyLines	= new JobAction(3573),
			BTL			= new JobAction(7419,	62),
			Flare		= new JobAction(162,	50),
			Freeze		= new JobAction(159);

		private const ushort
			BuffLeyLines = 737;
	
		public BLM() : base()
		{
			BLMGauge gauge = XIVComboPlugin.JobGauges.Get<BLMGauge>();

			Blizzard4.SetCondition(() => gauge.InUmbralIce);
			Flare.SetCondition(() => gauge.InAstralFire);
			BTL.SetCondition(() => HasBuff(BuffLeyLines));

			// B4 and F4 change to each other depending on stance, as do Flare and Freeze.
			ForFlag(CustomComboPreset.BlackEnochianFeature)
				.ForAction(() => GetBestAvailableAction(Fire4, Blizzard4), Fire4, Blizzard4)
				.ForAction(() => GetBestAvailableAction(Freeze, Flare), Freeze, Flare);

			// Ley Lines and BTL
			ForFlag(CustomComboPreset.BlackLeyLines)
				.UpgradeAction(LeyLines, BTL);
		}
	}
}
