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
            this.pluginInterface.CommandManager.AddHandler("/pcombo", new CommandInfo(OnCommandDebugCombo)
            {
                HelpMessage = "Edit custom combo settings. Run without any parameters to learn more.",
                ShowInHelp = true
            });

            this.Configuration = pluginInterface.GetPluginConfig() as IconReplacerConfiguration;

            this.iconReplacer = new IconReplacer(pluginInterface.TargetModuleScanner, pluginInterface.ClientState, this.Configuration);

            this.iconReplacer.Enable();
        }

        public void Dispose()
        {
            this.iconReplacer.Dispose();

            this.pluginInterface.CommandManager.RemoveHandler("/pcombo");
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

            this.pluginInterface.SavePluginConfig(this.Configuration);
        }
    }
}
