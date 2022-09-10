using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	[Job("AST")]
	public class AST : Job
	{
		private static JobAction
			Play		= new JobAction(17055),
			Draw		= new JobAction(3590),
			Balance		= new JobAction(4401),
			Bole		= new JobAction(4404),
			Arrow		= new JobAction(4402),
			Spear		= new JobAction(4403),
			Ewer		= new JobAction(4405),
			Spire		= new JobAction(4406),
			MinorArcana	= new JobAction(7443),
			CrownPlay	= new JobAction(25869);
		private const ushort
			BuffLordOfCrownsDrawn = 2054,
			BuffLadyOfCrownsDrawn = 2055;
	
		public AST() : base()
		{
			ASTGauge gauge = XIVComboPlugin.JobGauges.Get<ASTGauge>();

			Draw.SetCondition(() => gauge.DrawnCard == CardType.NONE);

			// Make cards on the same button as play
			ForFlag(CustomComboPreset.AstrologianCardsOnDrawFeature)
				.UpgradeAction(Play, Draw);
		}
	}
}
