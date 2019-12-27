using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.IO;
using System.Linq;

namespace IconReplacerPlugin
{
    class IconReplacerPlugin : IDalamudPlugin
    {
        public string Name => "Icon Replacer Plugin";

        public IconReplacerConfiguration Configuration;

        private DalamudPluginInterface pluginInterface;
        private IconReplacer iconReplacer;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            this.pluginInterface.CommandManager.AddHandler("/xldcombo", new CommandInfo(OnCommandDebugCombo)
            {
                HelpMessage = "COMBO debug",
                ShowInHelp = false
            });

            string configPath = Path.Combine(pluginInterface.WorkingDirectory, "iconReplacerConfig.json");
            this.Configuration = IconReplacerConfiguration.Load<IconReplacerConfiguration>(configPath);

            this.iconReplacer = new IconReplacer(pluginInterface.SigScanner, pluginInterface.ClientState, this.Configuration);

            if (this.Configuration.ComboPresets != CustomComboPreset.None)
                this.iconReplacer.Enable();
        }

        public void Dispose()
        {
            if (this.Configuration.ComboPresets != CustomComboPreset.None)
                this.iconReplacer.Dispose();

            this.pluginInterface.CommandManager.RemoveHandler("/xldcombo");
        }

        private void OnCommandDebugCombo(string command, string arguments)
        {
            var argumentsParts = arguments.Split();

            switch (argumentsParts[0])
            {
                case "setall":
                    {
                        foreach (var value in Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>())
                        {
                            if (value == CustomComboPreset.None)
                                continue;

                            this.Configuration.ComboPresets |= value;
                        }

                        this.pluginInterface.Framework.Gui.Chat.Print("all SET");
                    }
                    break;
                case "unsetall":
                    {
                        foreach (var value in Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>())
                        {
                            this.Configuration.ComboPresets &= value;
                        }

                        this.pluginInterface.Framework.Gui.Chat.Print("all UNSET");
                    }
                    break;
                case "set":
                    {
                        foreach (var value in Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>())
                        {
                            if (value.ToString().ToLower() != argumentsParts[1].ToLower())
                                continue;

                            this.Configuration.ComboPresets |= value;
                        }
                    }
                    break;
                case "toggle":
                    {
                        foreach (var value in Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>())
                        {
                            if (value.ToString().ToLower() != argumentsParts[1].ToLower())
                                continue;

                            this.Configuration.ComboPresets ^= value;
                        }
                    }
                    break;

                case "unset":
                    {
                        foreach (var value in Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>())
                        {
                            if (value.ToString().ToLower() != argumentsParts[1].ToLower())
                                continue;

                            this.Configuration.ComboPresets &= ~value;
                        }
                    }
                    break;

                case "list":
                    {
                        foreach (var value in Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>())
                        {
                            if (this.Configuration.ComboPresets.HasFlag(value))
                                this.pluginInterface.Framework.Gui.Chat.Print(value.ToString());
                        }
                    }
                    break;

                default:
                    this.pluginInterface.Framework.Gui.Chat.Print("Unknown");
                    break;
            }

            this.Configuration.Save();
        }
    }
}
