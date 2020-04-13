using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using Dalamud.Game.Chat;
using ImGuiNET;
using Serilog;
using System.Collections.Generic;

namespace XIVComboPlugin
{
    class XIVComboPlugin : IDalamudPlugin
    {
        public string Name => "XIV Combo Plugin";

        public XIVComboConfiguration Configuration;

        private DalamudPluginInterface pluginInterface;
        private IconReplacer iconReplacer;
        private readonly int CURRENT_CONFIG_VERSION = 1;
        private CustomComboPreset[] orderedByClassJob;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.pluginInterface.CommandManager.AddHandler("/pcombo", new CommandInfo(OnCommandDebugCombo)
            {
                HelpMessage = "Open a window to edit custom combo settings.",
                ShowInHelp = true
            });

            this.Configuration = pluginInterface.GetPluginConfig() as XIVComboConfiguration ?? new XIVComboConfiguration();
            if (Configuration.Version < CURRENT_CONFIG_VERSION)
            {
                Configuration.HiddenActions = new List<bool>();
                for (var i = 0; i < Enum.GetValues(typeof(CustomComboPreset)).Length; i++)
                    Configuration.HiddenActions.Add(false);
                Configuration.Version = CURRENT_CONFIG_VERSION;
            }
            this.iconReplacer = new IconReplacer(pluginInterface.TargetModuleScanner, pluginInterface.ClientState, this.Configuration);

            this.iconReplacer.Enable();

            this.pluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => isImguiComboSetupOpen = true;
            this.pluginInterface.UiBuilder.OnBuildUi += UiBuilder_OnBuildUi;

            var values = Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>();
            orderedByClassJob = values.Where(x => x != CustomComboPreset.None && x.GetAttribute<CustomComboInfoAttribute>() != null).OrderBy(x => x.GetAttribute<CustomComboInfoAttribute>().ClassJob).ToArray();
            UpdateConfig();
        }

        private bool isImguiComboSetupOpen = false;

        private string ClassJobToName(byte key)
        {
            switch (key)
            {
                default: return "Unknown";
                case 1: return "Gladiator";
                case 2: return "Pugilist";
                case 3: return "Marauder";
                case 4: return "Lancer";
                case 5: return "Archer";
                case 6: return "Conjurer";
                case 7: return "Thaumaturge";
                case 8: return "Carpenter";
                case 9: return "Blacksmith";
                case 10: return "Armorer";
                case 11: return "Goldsmith";
                case 12: return "Leatherworker";
                case 13: return "Weaver";
                case 14: return "Alchemist";
                case 15: return "Culinarian";
                case 16: return "Miner";
                case 17: return "Botanist";
                case 18: return "Fisher";
                case 19: return "Paladin";
                case 20: return "Monk";
                case 21: return "Warrior";
                case 22: return "Dragoon";
                case 23: return "Bard";
                case 24: return "White Mage";
                case 25: return "Black Mage";
                case 26: return "Arcanist";
                case 27: return "Summoner";
                case 28: return "Scholar";
                case 29: return "Rogue";
                case 30: return "Ninja";
                case 31: return "Machinist";
                case 32: return "Dark Knight";
                case 33: return "Astrologian";
                case 34: return "Samurai";
                case 35: return "Red Mage";
                case 36: return "Blue Mage";
                case 37: return "Gunbreaker";
                case 38: return "Dancer";
            }
        }

        private void UpdateConfig()
        {
            for (var i = 0; i < orderedByClassJob.Length; i++)
            {
                if (Configuration.HiddenActions[i])
                    iconReplacer.AddNoUpdate(orderedByClassJob[i].GetAttribute<CustomComboInfoAttribute>().Abilities);
                else
                    iconReplacer.RemoveNoUpdate(orderedByClassJob[i].GetAttribute<CustomComboInfoAttribute>().Abilities);
            }
        }

        private void UiBuilder_OnBuildUi()
        {
            if (!isImguiComboSetupOpen)
                return;

            var flagsSelected = new bool[orderedByClassJob.Length];
            var hiddenFlags = new bool[orderedByClassJob.Length];
            for (var i = 0; i < orderedByClassJob.Length; i++)
            {
                flagsSelected[i] = Configuration.ComboPresets.HasFlag(orderedByClassJob[i]);
                hiddenFlags[i] = Configuration.HiddenActions[i];
            }

            ImGui.SetNextWindowSize(new Vector2(740, 490));

            ImGui.Begin("Custom Combo Setup", ref isImguiComboSetupOpen, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);

            ImGui.Text("This window allows you to enable and disable custom combos to your liking.");
            ImGui.Separator();

            ImGui.BeginChild("scrolling", new Vector2(0, 400), true, ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 5));

            var lastClassJob = 0;

            for (var i = 0; i < orderedByClassJob.Length; i++)
            {
                var flag = orderedByClassJob[i];
                var flagInfo = flag.GetAttribute<CustomComboInfoAttribute>();
                if (lastClassJob != flagInfo.ClassJob)
                {
                    lastClassJob = flagInfo.ClassJob;
                    if (ImGui.CollapsingHeader(ClassJobToName((byte)lastClassJob)))
                    {
                        for (int j = i; j < orderedByClassJob.Length; j++)
                        {
                            flag = orderedByClassJob[j];
                            flagInfo = flag.GetAttribute<CustomComboInfoAttribute>();
                            if (lastClassJob != flagInfo.ClassJob)
                            {
                                break;
                            }
                            ImGui.Checkbox(flagInfo.FancyName, ref flagsSelected[j]);
                            ImGui.TextColored(new Vector4(0.68f, 0.68f, 0.68f, 1.0f), $"#{j+1}:" + flagInfo.Description);
                            ImGui.Indent();
                            ImGui.Checkbox("Prevent this chain from updating its icon", ref hiddenFlags[j]);
                            ImGui.Unindent();
                            ImGui.Spacing();
                        }
                        
                    }
                    
                }
            }

            for (var i = 0; i < orderedByClassJob.Length; i++)
            {
                if (flagsSelected[i])
                {
                    Configuration.ComboPresets |= orderedByClassJob[i];
                }
                else
                {
                    Configuration.ComboPresets &= ~orderedByClassJob[i];
                }
                Configuration.HiddenActions[i] = hiddenFlags[i];
            }

            ImGui.PopStyleVar();

            ImGui.EndChild();

            ImGui.Separator();

            if (ImGui.Button("Save and Close"))
            {
                this.pluginInterface.SavePluginConfig(Configuration);
                this.isImguiComboSetupOpen = false;
                UpdateConfig();
            }

            ImGui.End();
        }

        public void Dispose()
        {
            this.iconReplacer.Dispose();

            this.pluginInterface.CommandManager.RemoveHandler("/pcombo");

            this.pluginInterface.Dispose();
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
                        foreach (var value in Enum.GetValues(typeof(CustomComboPreset)).Cast<CustomComboPreset>().Where(x => x != CustomComboPreset.None))
                        {
                            if (argumentsParts[1].ToLower() == "set")
                            {
                                if (this.Configuration.ComboPresets.HasFlag(value))
                                    this.pluginInterface.Framework.Gui.Chat.Print(value.ToString());
                            }
                            else if (argumentsParts[1].ToLower() == "all")
                                this.pluginInterface.Framework.Gui.Chat.Print(value.ToString());
                        }
                    }
                    break;

                default:
                    this.isImguiComboSetupOpen = true;
                    break;
            }

            this.pluginInterface.SavePluginConfig(this.Configuration);
        }
    }
}
