using Dalamud.Game.ClientState.JobGauge.Types;
using Lumina.Excel.GeneratedSheets;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("RDM")]
	public class RDM : Job
	{
		private static JobAction
			Veraero2		= new JobAction(16525	),
			Verthunder2		= new JobAction(16524	),
			Impact			= new JobAction(16526,	66),
			Redoublement	= new JobAction(7516,	50),
			ERedoublement	= new JobAction(7529,	50),
			Zwerchhau		= new JobAction(7512,	35),
			EZwerchhau		= new JobAction(7528,	35),
			Riposte			= new JobAction(7504	),
			ERiposte		= new JobAction(7527	),
			Scatter			= new JobAction(7509	),
			Verstone		= new JobAction(7511	),
			Verfire			= new JobAction(7510	),
			Jolt			= new JobAction(7503	),
			Jolt2			= new JobAction(7524,	62),
			Verholy			= new JobAction(7526	),
			Verflare		= new JobAction(7525	),
			Scorch			= new JobAction(16530,	80),
			Resolution		= new JobAction(25858,	90);

		private const ushort
			BuffSwiftcast = 167,
			BuffDualcast = 1249,
			BuffAcceleration = 1238,
			BuffChainspell = 2560,
			BuffVerstoneReady = 1235,
			BuffVerfireReady = 1234;

		private static RDMGauge gauge;
		private static bool HasMana(byte manaMin) => gauge.BlackMana >= manaMin && gauge.WhiteMana >= manaMin;

		public RDM() : base()
		{
			gauge = XIVComboPlugin.JobGauges.Get<RDMGauge>();
			Verstone.SetCondition(() => IsCurrentAction(Verstone) && HasBuff(BuffVerstoneReady));
			Verfire.SetCondition(() => IsCurrentAction(Verfire) && HasBuff(BuffVerfireReady));
			ERiposte.SetCondition(() => HasMana(20));
			Zwerchhau.SetCondition(() => LastMoveWas(Riposte, ERiposte));
			EZwerchhau.SetCondition(() => CanUse(Zwerchhau) && HasMana(15));
			Redoublement.SetCondition(() => LastMoveWas(Zwerchhau, EZwerchhau));
			ERedoublement.SetCondition(() => CanUse(Redoublement) && HasMana(15));
			Scorch.SetCondition(() => LastMoveWas(Verflare, Verholy));
			Resolution.SetCondition(() => LastMoveWas(Scorch));
			Scatter.SetCondition(() => HasBuff(BuffSwiftcast, BuffDualcast, BuffAcceleration, BuffChainspell));
			Impact.SetCondition(() => CanUse(Scatter));

			ForFlag(CustomComboPreset.RedMageAoECombo)
				.ForAction(
					() => GetBestAvailableAction(currentAction, Scatter, Impact),
					Veraero2, Verthunder2
				);

			ForFlag(CustomComboPreset.RedMageMeleeCombo)
				.ForAction(
					() => GetBestAvailableAction(Riposte, ERiposte, Zwerchhau, EZwerchhau, Redoublement, ERedoublement),
					Redoublement
				);

			ForFlag(CustomComboPreset.RedMageVerprocCombo)
				.ForAction(
					() => GetBestAvailableAction(Jolt, Jolt2, Verstone, Verfire, Scorch, Resolution),
					Verstone, Verfire
				);
		}
	}
}
