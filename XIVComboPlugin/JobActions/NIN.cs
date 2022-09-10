using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("NIN")]
	public class NIN : Job
	{
		private static JobAction
			ArmorCrush		= new JobAction(3563,	54),
			SpinningEdge	= new JobAction(2240),
			GustSlash		= new JobAction(2242,	4),
			AeolianEdge		= new JobAction(2255,	26),
			HakkeM			= new JobAction(16488,	52),
			DeathBlossom	= new JobAction(2254),
			DWAD			= new JobAction(3566),
			Assassinate		= new JobAction(2246),
			Bunshin		    = new JobAction(16493),
			PhantomK		= new JobAction(25774);

		private const ushort
			BuffPhantomKReady = 2723;
	
		public NIN() : base()
		{
			GustSlash.SetCondition(() => LastMoveWasInCombo(SpinningEdge));
			ArmorCrush.SetCondition(() => LastMoveWasInCombo(GustSlash));
			AeolianEdge.SetCondition(() => LastMoveWasInCombo(GustSlash));
			HakkeM.SetCondition(() => LastMoveWasInCombo(DeathBlossom));

			// Replace Armor Crush with Armor Crush combo
			ForFlag(CustomComboPreset.NinjaArmorCrushCombo)
				.ForComboActions(SpinningEdge, GustSlash, ArmorCrush);

			// Replace Aeolian Edge with Aeolian Edge combo
			ForFlag(CustomComboPreset.NinjaAeolianEdgeCombo)
				.ForComboActions(SpinningEdge, GustSlash, AeolianEdge);

			// Replace Hakke Mujinsatsu with Hakke Mujinsatsu combo
			ForFlag(CustomComboPreset.NinjaHakkeMujinsatsuCombo)
				.ForComboActions(DeathBlossom, HakkeM);
		}
	}
}
