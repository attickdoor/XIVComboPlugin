using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("PLD", "GLA")]
	public class PLD : Job
	{
		private static JobAction
			FastBlade			= new JobAction(9),
			RiotBlade			= new JobAction(15,		4),
			GoringBlade			= new JobAction(3538,	54),
			RageOfHalone		= new JobAction(21,		26),
			RoyalAuthority		= new JobAction(3539,	60),
			Prominence			= new JobAction(16457,	40),
			TotalEclipse		= new JobAction(7381),
			Requiescat			= new JobAction(7383),
			Confiteor			= new JobAction(16459,	80),
			BladeOfFaith		= new JobAction(25748),
			BladeOfTruth		= new JobAction(25749),
			BladeOfValor		= new JobAction(25750);

		private const ushort
			BuffRequiescat = 1368,
			BuffBladeOfFaithReady = 3019;

		public PLD() : base()
		{
			RiotBlade.SetCondition(() => LastMoveWasInCombo(FastBlade));
			GoringBlade.SetCondition(() => LastMoveWasInCombo(RiotBlade));
			RageOfHalone.SetCondition(() => LastMoveWasInCombo(RiotBlade));
			RoyalAuthority.SetCondition(() => CanUse(RageOfHalone));
			Prominence.SetCondition(() => LastMoveWasInCombo(TotalEclipse));
			BladeOfFaith.SetCondition(() => HasBuff(BuffBladeOfFaithReady));
			BladeOfTruth.SetCondition(() => LastMoveWasInCombo(BladeOfFaith));
			BladeOfValor.SetCondition(() => LastMoveWasInCombo(BladeOfTruth));
			Confiteor.SetCondition(() => HasBuff(BuffRequiescat));

			// Replace Goring Blade with Goring Blade combo
			ForFlag(CustomComboPreset.PaladinGoringBladeCombo)
				.ForComboActions(FastBlade, RiotBlade, GoringBlade);

			// Replace Royal Authority with Royal Authority combo
			ForFlag(CustomComboPreset.PaladinRoyalAuthorityCombo)
				.ForComboActions(FastBlade, RiotBlade, RageOfHalone)
				.ForComboActions(FastBlade, RiotBlade, RoyalAuthority);
			// Could also be written as:
			//.ForAction(
			//    () => GetBestAvailableAction(FastBlade, RiotBlade, currentAction),
			//    RageOfHalone, RoyalAuthority
			//);

			// Replace Prominence with Prominence combo
			ForFlag(CustomComboPreset.PaladinProminenceCombo)
				.ForComboActions(TotalEclipse, Prominence);

			// Replace Requiescat with Confiteor when under the effect of Requiescat
			ForFlag(CustomComboPreset.PaladinRequiescatCombo)
				.UpgradeAction(Requiescat, Confiteor, BladeOfFaith, BladeOfValor, BladeOfTruth);
		}
	}
}
