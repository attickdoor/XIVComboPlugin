using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Reflection.Emit;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("SAM")]
	public class SAM : Job
	{
		private static JobAction
			Yukikaze		= new JobAction(7480,	50),
			Hakaze			= new JobAction(7477),
			Gekko			= new JobAction(7481,	30),
			Jinpu			= new JobAction(7478,	4),
			Kasha			= new JobAction(7482,	40),
			Shifu			= new JobAction(7479,	18),
			Mangetsu		= new JobAction(7484,	35),
			Fuga			= new JobAction(7483),
			Oka			    = new JobAction(7485,	45),
			ThirdEye		= new JobAction(7498),
			Iaijutsu		= new JobAction(7867),
			Tsubame			= new JobAction(16483),
			OgiNamikiri		= new JobAction(25781),
			Ikishoten		= new JobAction(16482),
			KaeshiNamikiri	= new JobAction(25782),
			Fuko			= new JobAction(25780,	86);

		private const ushort
			BuffOgiNamikiriReady = 2959,
			BuffMeikyoShisui = 1233;
	
		public SAM() : base()
		{
			SAMGauge gauge = XIVComboPlugin.JobGauges.Get<SAMGauge>();

			Yukikaze.SetCondition(() => HasBuff(BuffMeikyoShisui) || LastMoveWasInCombo(Hakaze));
			Jinpu.SetCondition(() => LastMoveWasInCombo(Hakaze));
			Gekko.SetCondition(() => HasBuff(BuffMeikyoShisui) || LastMoveWasInCombo(Jinpu));
			Shifu.SetCondition(() => LastMoveWasInCombo(Hakaze));
			Kasha.SetCondition(() => HasBuff(BuffMeikyoShisui) || LastMoveWasInCombo(Shifu));
			Mangetsu.SetCondition(() => HasBuff(BuffMeikyoShisui) || LastMoveWasInCombo(Fuga, Fuko));
			Oka.SetCondition(() => HasBuff(BuffMeikyoShisui) || LastMoveWasInCombo(Fuga, Fuko));
			OgiNamikiri.SetCondition(() => HasBuff(BuffOgiNamikiriReady));
			KaeshiNamikiri.SetCondition(() => gauge.Kaeshi == Kaeshi.NAMIKIRI);

			ForFlag(CustomComboPreset.SamuraiTsubameCombo)
				.ForAction(() =>
			{
				JobAction resolvedTsubame = Resolve(Tsubame);
				return resolvedTsubame != Tsubame ? resolvedTsubame : currentAction;
			}, Iaijutsu);

			// Replace Yukikaze with Yukikaze combo
			ForFlag(CustomComboPreset.SamuraiYukikazeCombo)
				.ForComboActions(Hakaze, Yukikaze);

			// Replace Gekko with Gekko combo
			ForFlag(CustomComboPreset.SamuraiGekkoCombo)
				.ForComboActions(Hakaze, Jinpu, Gekko);

			// Replace Kasha with Kasha combo
			ForFlag(CustomComboPreset.SamuraiKashaCombo)
				.ForComboActions(Hakaze, Shifu, Kasha);

			// Replace Mangetsu with Mangetsu combo
			ForFlag(CustomComboPreset.SamuraiMangetsuCombo)
				.ForComboActions(Fuga, Fuko, Mangetsu);

			// Replace Oka with Oka combo
			ForFlag(CustomComboPreset.SamuraiOkaCombo)
				.ForComboActions(Fuga, Fuko, Oka);

			ForFlag(CustomComboPreset.SamuraiOgiCombo)
				.ForComboActions(OgiNamikiri, KaeshiNamikiri, Ikishoten);
		}
	}
}
