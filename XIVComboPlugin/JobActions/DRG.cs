using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("DRG")]
	public class DRG : Job
	{
		private static JobAction
			Jump            = new JobAction(92),
			HighJump        = new JobAction(16478),
			MirageDive      = new JobAction(7399),
			BOTD            = new JobAction(3553),
			Stardiver       = new JobAction(16480),
			CTorment        = new JobAction(16477,	72),
			DoomSpike       = new JobAction(86),
			SonicThrust     = new JobAction(7397,	62),
			ChaosThrust     = new JobAction(88,		50),
			RaidenThrust    = new JobAction(16479,	76),
			HeavensThrust   = new JobAction(25771,	86),
			ChaoticSpring   = new JobAction(25772,	86),
			DraconianFury   = new JobAction(25770),
			TrueThrust      = new JobAction(75),
			Disembowel      = new JobAction(87,		18),
			FangAndClaw     = new JobAction(3554,	56),
			WheelingThrust  = new JobAction(3556,	58),
			FullThrust      = new JobAction(84,		26),
			VorpalThrust    = new JobAction(78,		4);

		private const ushort
			BuffFangAndClawReady = 802,
			BuffWheelingThrustReady = 803,
			BuffDraconianFire = 1863,
			BuffDiveReady = 1243;

		public DRG() : base()
		{
			MirageDive.SetCondition(() => HasBuff(BuffDiveReady));
			SonicThrust.SetCondition(() => LastMoveWasInCombo(DoomSpike, DraconianFury));
			CTorment.SetCondition(() => LastMoveWasInCombo(SonicThrust));
			Disembowel.SetCondition(() => LastMoveWasInCombo(TrueThrust, RaidenThrust));
			ChaosThrust.SetCondition(() => LastMoveWasInCombo(Disembowel));
			ChaoticSpring.SetCondition(() => CanUse(ChaosThrust));
			FangAndClaw.SetCondition(() => HasBuff(BuffFangAndClawReady));
			WheelingThrust.SetCondition(() => HasBuff(BuffWheelingThrustReady));
			RaidenThrust.SetCondition(() => HasBuff(BuffDraconianFire));
			FullThrust.SetCondition(() => LastMoveWasInCombo(VorpalThrust));
			HeavensThrust.SetCondition(() => CanUse(FullThrust));
			VorpalThrust.SetCondition(() => LastMoveWasInCombo(TrueThrust, RaidenThrust));

			// Change Jump/High Jump into Mirage Dive when Dive Ready
			ForFlag(CustomComboPreset.DragoonJumpFeature)
				.ForAction(
					() => GetBestAvailableAction(currentAction, MirageDive),
					Jump, HighJump
				);

			// Replace Coerthan Torment with Coerthan Torment combo chain
			ForFlag(CustomComboPreset.DragoonCoerthanTormentCombo)
				.ForComboActions(DoomSpike, SonicThrust, CTorment);

			// Replace Chaos Thrust with the Chaos Thrust combo chain
			ForFlag(CustomComboPreset.DragoonChaosThrustCombo)
				.ForAction(
					() => GetBestAvailableAction(TrueThrust, FangAndClaw, WheelingThrust, RaidenThrust, Disembowel, currentAction),
					ChaosThrust, ChaoticSpring
				);

			// Replace Full Thrust with the Full Thrust combo chain
			ForFlag(CustomComboPreset.DragoonFullThrustCombo)
				.ForAction(
					() => GetBestAvailableAction(TrueThrust, FangAndClaw, WheelingThrust, RaidenThrust, VorpalThrust, currentAction),
					FullThrust, HeavensThrust
				);
		}
	}
}
