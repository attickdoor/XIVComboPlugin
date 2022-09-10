using System;
using System.Collections.Generic;

namespace XIVCombo.JobActions
{
	public class JobAction
	{
		private static Dictionary<uint, JobAction> map = new Dictionary<uint, JobAction>();
		public static JobAction FromId(uint Id) => map.ContainsKey(Id) ? map[Id] : null;

		public readonly uint Id;
		public readonly byte Level;
		private Func<bool> condition; 

		public JobAction(uint Id, byte Level = 1)
		{
			this.Id = Id;
			this.Level = Level;
			map[Id] = this;
		}
		public void SetCondition(Func<bool> predicate) => (this.condition) = (predicate);
		public bool ConditionPasses() => condition?.Invoke() ?? true;

		public static implicit operator uint(JobAction action) => action?.Id ?? 0;
		public static implicit operator byte(JobAction action) => action?.Level ?? 0;
	}
}
