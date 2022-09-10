using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XIVCombo.JobActions;

namespace XIVComboPlugin.JobActions
{
	public class MNK : Job
	{
		private static JobAction
			AOTD			= new JobAction(62),
			FourPointFury	= new JobAction(16473),
			Rockbreaker		= new JobAction(70);
	
		public MNK() : base()
		{
		}
	}
}
