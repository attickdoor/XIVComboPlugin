using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Hooking;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using XIVCombo.JobActions;

namespace XIVComboPlugin
{
	public class IconReplacer
	{
		public delegate ulong OnCheckIsIconReplaceableDelegate(uint actionID);

		public delegate ulong OnGetIconDelegate(byte param1, uint param2);

		private readonly IconReplacerAddressResolver Address;
		private readonly Hook<OnCheckIsIconReplaceableDelegate> checkerHook;
		private readonly ClientState clientState;

		private readonly IntPtr comboTimer;

		private readonly XIVComboConfiguration Configuration;

		private readonly Hook<OnGetIconDelegate> iconHook;
		private readonly IntPtr lastComboMove;

		private unsafe delegate int* getArray(long* address);

		private Dictionary<string, Job> jobsMap;

		public IconReplacer(SigScanner scanner, ClientState clientState, DataManager manager, XIVComboConfiguration configuration)
		{

			Configuration = configuration;
			this.clientState = clientState;

			Address = new IconReplacerAddressResolver();
			Address.Setup(scanner);

			comboTimer = Address.ComboTimer;
			lastComboMove = comboTimer + 0x4;

			PluginLog.Verbose("===== X I V C O M B O =====");
			PluginLog.Verbose("IsIconReplaceable address {IsIconReplaceable}", Address.IsIconReplaceable);
			PluginLog.Verbose("GetIcon address {GetIcon}", Address.GetIcon);
			PluginLog.Verbose("ComboTimer address {ComboTimer}", comboTimer);
			PluginLog.Verbose("LastComboMove address {LastComboMove}", lastComboMove);

			iconHook = Hook<OnGetIconDelegate>.FromAddress(Address.GetIcon, GetIconDetour);
			checkerHook = Hook<OnCheckIsIconReplaceableDelegate>.FromAddress(Address.IsIconReplaceable, CheckIsIconReplaceableDetour);

			jobsMap = Job.Initialize(clientState, Configuration, iconHook);
		}

		public void Enable()
		{
			iconHook.Enable();
			checkerHook.Enable();
		}

		public void Dispose()
		{
			iconHook.Dispose();
			checkerHook.Dispose();

		}

		// I hate this function. This is the dumbest function to exist in the game. Just return 1.
		// Determines which abilities are allowed to have their icons updated.
		private ulong CheckIsIconReplaceableDetour(uint actionID)
		{
			return 1;
		}

		/// <summary>
		///     Replace an ability with another ability
		///     actionID is the original ability to be "used"
		///     Return either actionID (itself) or a new Action table ID as the
		///     ability to take its place.
		///     I tend to make the "combo chain" button be the last move in the combo
		///     For example, Souleater combo on DRK happens by dragging Souleater
		///     onto your bar and mashing it.
		/// </summary>
		private ulong GetIconDetour(byte self, uint actionID)
		{
			if (clientState.LocalPlayer != null)
			{
				var lastMove = Marshal.ReadInt32(lastComboMove);
				var comboTime = Marshal.PtrToStructure<float>(comboTimer);
				var level = clientState.LocalPlayer.Level;
				var classAbbreviation = clientState.LocalPlayer.ClassJob.GameData.Abbreviation;

				if (jobsMap.ContainsKey(classAbbreviation) && jobsMap[classAbbreviation].ReplaceIcon(self, level, lastMove, comboTime, actionID, out uint newActionID))
					return iconHook.Original(self, newActionID);
			}
			
			return iconHook.Original(self, actionID);
		}
	}
}
