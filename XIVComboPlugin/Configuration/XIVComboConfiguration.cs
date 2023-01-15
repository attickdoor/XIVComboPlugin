using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Utility;
using Newtonsoft.Json;

namespace XIVComboPlugin
{
    [Serializable]
    public class XIVComboConfiguration : IPluginConfiguration
    {

        public LegacyCustomComboPreset ComboPresets { get; set; }

        public Dictionary<string, bool> CombosEnabled { get; set; }

        public int Version { get; set; }

        public List<bool> HiddenActions;

        private Dictionary<CustomComboPreset, bool> CachedCombosEnabled { get; set; }

        public void Initialize()
        {
            if (Version < 4) Version = 4;

            if (CombosEnabled == null)
                MigrateCombosEnabledFromLegacy();

            // TODO delete legacy ComboPresets from config file
        }

        public void SetComboEnabled(CustomComboPreset combo, bool enabled = true)
        {
            if (IsComboEnabled(combo) == enabled)
                return;

            CombosEnabled[combo.ToString()] = enabled;
            CachedCombosEnabled = null;
        }

        public bool IsComboEnabled(CustomComboPreset combo)
        {
            if (CachedCombosEnabled == null)
                CachedCombosEnabled = GetComboEnabledStates();
            return CachedCombosEnabled[combo];
        }

        public Dictionary<CustomComboPreset, bool> GetComboEnabledStates()
        {
            var states = new Dictionary<CustomComboPreset, bool>();
            var combos = Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>();
            foreach (var combo in combos)
            {
                if (CombosEnabled.ContainsKey(combo.ToString()))
                    states[combo] = CombosEnabled[combo.ToString()];
                else
                    states[combo] = false;
            }
            return states;
        }

        private void MigrateCombosEnabledFromLegacy()
        {
            CombosEnabled = new Dictionary<string, bool>();

            var combos = Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>()
                .Where(x => x.GetAttribute<CustomComboInfoAttribute>() != null);

            foreach (var combo in combos)
            {
                var legacyCombo = FindLegacyComboByName(combo.ToString());
                if (legacyCombo.HasValue)
                {
                    var legacyVal = ComboPresets.HasFlag(legacyCombo);
                    SetComboEnabled(combo, legacyVal);
                }
                else
                    SetComboEnabled(combo, false);
            }
        }

        private LegacyCustomComboPreset? FindLegacyComboByName(string name)
        {
            var list = Enum.GetValues(typeof(LegacyCustomComboPreset)).Cast<LegacyCustomComboPreset>()
                .Where(legacyCombo =>  name.Equals(legacyCombo.ToString()));

            if (list.Any())
                return list.First();
            else
                return null;
        }
    }
}
