using System;
using System.IO;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace IconReplacerPlugin
{
    [Serializable]
    public class IconReplacerConfiguration : IPluginConfiguration
    {
        public CustomComboPreset ComboPresets { get; set; }
        int IPluginConfiguration.Version { get; set; }
    }
}
