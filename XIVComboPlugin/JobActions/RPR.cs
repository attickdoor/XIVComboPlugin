using Lumina.Excel.GeneratedSheets;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("RPR")]
	public class RPR : Job
	{
		private static JobAction
			// Single Target
			Slice               = new JobAction(24373),
			WaxingSlice         = new JobAction(24374,	5),
			InfernalSlice       = new JobAction(24375,	30),
			// AoE
			SpinningScythe      = new JobAction(24376,	25),
			NightmareScythe     = new JobAction(24377,	45),
			// Shroud
			Enshroud            = new JobAction(24394,	80),
			Communio            = new JobAction(24398,	90),

			Egress              = new JobAction(24402),
			Ingress             = new JobAction(24401),
			Regress             = new JobAction(24403),

			ArcaneCircle        = new JobAction(24405),
			PlentifulHarvest    = new JobAction(24385);

		private const ushort
			BuffEnshrouded = 2593,
			BuffThreshold = 2595,
			BuffImSac1 = 2592,
			BuffImSac2 = 3204;
	
		public RPR() : base()
		{
			WaxingSlice.SetCondition(() => LastMoveWasInCombo(Slice));
			InfernalSlice.SetCondition(() => LastMoveWasInCombo(WaxingSlice));
			NightmareScythe.SetCondition(() => LastMoveWasInCombo(SpinningScythe));
			Regress.SetCondition(() => HasBuff(BuffThreshold));
			Communio.SetCondition(() => HasBuff(BuffEnshrouded));
			PlentifulHarvest.SetCondition(() => HasBuff(BuffImSac1, BuffImSac2));

			ForFlag(CustomComboPreset.ReaperSliceCombo)
				.ForComboActions(Slice, WaxingSlice, InfernalSlice);

			ForFlag(CustomComboPreset.ReaperScytheCombo)
				.UpgradeAction(SpinningScythe, NightmareScythe);

			ForFlag(CustomComboPreset.ReaperRegressFeature)
				.UpgradeAction(Egress, Regress)
				.UpgradeAction(Ingress, Regress);

			ForFlag(CustomComboPreset.ReaperEnshroudCombo)
				.UpgradeAction(Enshroud, Communio);

			ForFlag(CustomComboPreset.ReaperArcaneFeature)
				.UpgradeAction(ArcaneCircle, PlentifulHarvest);
		}
	}
}