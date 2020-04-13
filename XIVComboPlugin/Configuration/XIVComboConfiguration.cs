using System;
using System.Collections.Generic;
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
        public int Version { get; set; }

        public List<bool> HiddenActions;

    }
}
