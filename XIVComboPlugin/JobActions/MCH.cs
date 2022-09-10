using Dalamud.Game.ClientState.JobGauge.Types;
using Lumina.Excel.GeneratedSheets;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("MCH")]
	public class MCH : Job
	{
		private static JobAction
			CleanShot		= new JobAction(2873,	26),
			HeatedCleanShot	= new JobAction(7413,	64),
			SplitShot		= new JobAction(2866),
			HeatedSplitShot	= new JobAction(7411,	54),
			SlugShot		= new JobAction(2868,	2),
			HeatedSlugshot	= new JobAction(7412,	60),
			Hypercharge		= new JobAction(17209),
			HeatBlast		= new JobAction(7410,	35),
			SpreadShot		= new JobAction(2870),
			AutoCrossbow	= new JobAction(16497,	52),
			Scattergun		= new JobAction(25786,	82);
	
		public MCH() : base()
		{
			MCHGauge gauge = XIVComboPlugin.JobGauges.Get<MCHGauge>();

			SlugShot.SetCondition(() => LastMoveWasInCombo(SplitShot));
			HeatedSlugshot.SetCondition(() => CanUse(SlugShot));
			CleanShot.SetCondition(() => LastMoveWasInCombo(SlugShot));
			HeatedCleanShot.SetCondition(() => CanUse(CleanShot));
			HeatBlast.SetCondition(() => gauge.IsOverheated);
			AutoCrossbow.SetCondition(() => gauge.IsOverheated);

			// Replace Clean Shot with Heated Clean Shot combo
			// Or with Heat Blast when overheated.
			// For some reason the shots use their unheated IDs as combo moves
			ForFlag(CustomComboPreset.MachinistMainCombo)
				.ForComboActions(HeatedSplitShot, HeatedSlugshot, HeatedCleanShot)
				.ForComboActions(SplitShot, SlugShot, CleanShot);

			// Replace Hypercharge with Heat Blast when overheated
			ForFlag(CustomComboPreset.MachinistOverheatFeature)
				.UpgradeAction(Hypercharge, HeatBlast);

			// Replace Spread Shot with Auto Crossbow when overheated.
			ForFlag(CustomComboPreset.MachinistSpreadShotFeature)
				.ForAction(() => GetBestAvailableAction(SpreadShot, Scattergun, AutoCrossbow), SpreadShot, Scattergun);
		}
	}
}
