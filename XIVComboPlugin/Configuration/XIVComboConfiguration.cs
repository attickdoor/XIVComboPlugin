using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace XIVComboPlugin
{
    [Serializable]
    public class XIVComboConfiguration : IPluginConfiguration
    {

        public CustomComboPreset ComboPresets { get; set; }
        public int Version { get; set; }

        public List<bool> HiddenActions;

    }
}
