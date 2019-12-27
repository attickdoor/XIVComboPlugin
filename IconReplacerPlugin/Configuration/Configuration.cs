using System;
using System.IO;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace IconReplacerPlugin
{
    [Serializable]
    public class IconReplacerConfiguration : PluginConfiguration
    {
        public CustomComboPreset ComboPresets { get; set; }
    }
}
