using System;
using System.IO;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace XIVComboPlugin
{
    [Serializable]
    public class XIVComboConfiguration : IPluginConfiguration
    {
        public CustomComboPreset ComboPresets { get; set; }
        int IPluginConfiguration.Version { get; set; }
    }
}
