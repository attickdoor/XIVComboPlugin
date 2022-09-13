using Dalamud.Game.ClientState;
using Dalamud.Hooking;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XIVComboPlugin;
using static XIVComboPlugin.IconReplacer;

namespace XIVCombo.JobActions
{
	public class JobAttribute : Attribute
	{
		internal JobAttribute(params string[] abbreviations)
		{
			Abbreviations = abbreviations;
		}

		public string[] Abbreviations { get; }

	}
	public abstract class Job
	{
		protected static ClientState clientState;
		protected static XIVComboConfiguration Configuration;
		protected static Hook<OnGetIconDelegate> iconHook;
		private readonly JobConfig jobConfig;
		public Job()
		{
			jobConfig = new JobConfig(this);
		}

		public static Dictionary<string, Job> Initialize(ClientState clientState, XIVComboConfiguration Configuration, Hook<OnGetIconDelegate> iconHook)
		{
			Job.clientState = clientState;
			Job.Configuration = Configuration;
			Job.iconHook = iconHook;

			Type[] jobTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsDefined(typeof(JobAttribute))).ToArray();
			Dictionary<string, Job> jobs = new Dictionary<string, Job>();
			foreach (Type jobType in jobTypes)
			{
				PluginLog.Log($"{jobType.Name} Loaded");
				string[] abbreviations = jobType.GetCustomAttribute<JobAttribute>().Abbreviations;
				Job job = (Job)Activator.CreateInstance(jobType);
				foreach(string abbreviation in abbreviations)
					jobs[abbreviation] = job;
			}
			return jobs;
		}

		private byte self;
		protected byte currentLevel { get; private set; }
		protected JobAction currentAction { get; private set; }
		private int lastMove;
		private float comboTime;
		public bool ReplaceIcon(byte self, byte currentLevel, int lastMove, float comboTime, uint currentActionID, out uint newActionID)
		{
			newActionID = currentAction = JobAction.FromId(currentActionID);
			if (currentAction != null)
			{
				this.self = self;
				this.currentLevel = currentLevel;
				this.lastMove = lastMove;
				this.comboTime = comboTime;

				return ProcessActionLogic(out newActionID);
			}

			return false;
		}

		// The default logic using jobConfig is probably good enough for most classes, but it can be
		// overridden if you don't want to use jobConfig and would rather write the logic manually
		protected virtual bool ProcessActionLogic(out uint newActionID)
		{
			var actionLogic = jobConfig.GetActionLogic(Configuration);
			if (actionLogic.ContainsKey(currentAction))
			{
				newActionID = actionLogic[currentAction]();
				if (newActionID == 0)
					newActionID = currentAction;
				return true;
			}
			newActionID = currentAction;
			return false;
		}

		// Pick the last option which is still usable.
		// The actions should be listed in order of combo, level, and/or from most accessible to least accessible
		protected JobAction GetBestAvailableAction(params JobAction[] actions)
		{
			return actions.Where(CanUse).LastOrDefault();
		}
		protected bool IsCurrentAction(params JobAction[] actions) => actions.Any(action => action.Id == currentAction);
		protected bool CanUse(JobAction action) => action.Level <= currentLevel && action.ConditionPasses();
		protected bool LastMoveWas(params JobAction[] actions) => actions.Any(action => action.Id == lastMove);
		protected bool InCombo() => comboTime > 0;
		protected bool LastMoveWasInCombo(params JobAction[] actions) => InCombo() && actions.Any(action => action.Id == lastMove);
		protected bool HasBuff(params uint[] buffs)
		{
			buffs = buffs.Where(buff => buff > 0).ToArray();
			if (buffs.Length > 0)
				return clientState.LocalPlayer.StatusList.Any(status => buffs.Contains(status.StatusId));

			return false;
		}
		protected JobAction Resolve(JobAction action) => JobAction.FromId((uint)iconHook.Original(self, action));
		protected JobConfig.JobConfigData ForFlag(CustomComboPreset flag) => jobConfig.ForFlag(flag);

		protected class JobConfig
		{
			private Job job;
			internal JobConfig(Job job) => this.job = job;

			private readonly Dictionary<CustomComboPreset, JobConfigData> configData = new Dictionary<CustomComboPreset, JobConfigData>();
			public JobConfigData ForFlag(CustomComboPreset flag)
			{
				configData[flag] = new JobConfigData(job);
				return configData[flag];
			}

			public delegate uint ActionLogic();
			internal Dictionary<JobAction, ActionLogic> GetActionLogic(XIVComboConfiguration Configuration)
			{
				Dictionary<JobAction, ActionLogic> actionLogicMap = new Dictionary<JobAction, ActionLogic>();
				foreach (CustomComboPreset flag in configData.Keys)
				{
					if (Configuration.ComboPresets.HasFlag(flag))
						configData[flag].actionLogicMap.ToList().ForEach(x => actionLogicMap[x.Key] = x.Value);
				}
				return actionLogicMap;
			}

			public class JobConfigData
			{
				private Job job;
				internal JobConfigData(Job job) => this.job = job;
				internal readonly Dictionary<JobAction, ActionLogic> actionLogicMap = new Dictionary<JobAction, ActionLogic>();
				public JobConfigData ForAction(ActionLogic logic, params JobAction[] actions)
				{
					foreach (JobAction action in actions)
						actionLogicMap[action] = logic;
					return this;
				}
				public JobConfigData ForComboActions(params JobAction[] actions)
				{
					if (actions.Length > 0)
						ForAction(() => job.GetBestAvailableAction(actions), actions[actions.Length - 1]);
					return this;
				}

				public JobConfigData UpgradeAction(params JobAction[] actions)
				{
					if (actions.Length > 0)
						ForAction(() => job.GetBestAvailableAction(actions), actions[0]);
					return this;
				}
			}
		}
	}
}
