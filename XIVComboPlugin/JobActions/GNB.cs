using Lumina.Excel.GeneratedSheets;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("GNB")]
	public class GNB : Job
	{
		private static JobAction
			SolidBarrel		= new JobAction(16145,	26),
			KeenEdge		= new JobAction(16137),
			BrutalShell		= new JobAction(16139,	4),
			WickedTalon		= new JobAction(16150),
			GnashingFang	= new JobAction(16146),
			SavageClaw		= new JobAction(16147),
			DemonSlaughter	= new JobAction(16149,	40),
			DemonSlice		= new JobAction(16141),
			Continuation	= new JobAction(16155,	70),
			JugularRip		= new JobAction(16156,	70),
			AbdomenTear		= new JobAction(16157,	70),
			EyeGouge		= new JobAction(16158,	70),
			BurstStrike		= new JobAction(16162),
			Hypervelocity	= new JobAction(25759,	86);
		private const ushort
			BuffReadyToRip = 1842,
			BuffReadyToTear = 1843,
			BuffReadyToGouge = 1844,
			BuffReadyToBlast = 2686;
	
		public GNB() : base()
		{
			BrutalShell.SetCondition(() => LastMoveWasInCombo(KeenEdge));
			SolidBarrel.SetCondition(() => LastMoveWasInCombo(BrutalShell));
			JugularRip.SetCondition(() => HasBuff(BuffReadyToRip));
			AbdomenTear.SetCondition(() => HasBuff(BuffReadyToTear));
			EyeGouge.SetCondition(() => HasBuff(BuffReadyToGouge));
			Hypervelocity.SetCondition(() => HasBuff(BuffReadyToBlast));
			DemonSlaughter.SetCondition(() => LastMoveWasInCombo(DemonSlice));

			// Replace Solid Barrel with Solid Barrel combo
			ForFlag(CustomComboPreset.GunbreakerSolidBarrelCombo)
				.ForComboActions(KeenEdge, BrutalShell, SolidBarrel);

			// Replace Wicked Talon with Gnashing Fang combo
			ForFlag(CustomComboPreset.GunbreakerGnashingFangCont)
				.ForComboActions(JugularRip, AbdomenTear, EyeGouge, GnashingFang);

			// Replace Burst Strike with Continuation
			ForFlag(CustomComboPreset.GunbreakerBurstStrikeCont)
				.UpgradeAction(BurstStrike, Hypervelocity);

			// Replace Demon Slaughter with Demon Slaughter combo
			ForFlag(CustomComboPreset.GunbreakerDemonSlaughterCombo)
				.ForComboActions(DemonSlice, DemonSlaughter);
		}
	}
}
