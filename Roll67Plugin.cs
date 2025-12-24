using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Linq;

namespace Roll67Plugin
{
    public sealed class Roll67Plugin : IDalamudPlugin
    {
        public string Name => "Roll Sound Player";
        private const string CommandName = "/rollsounds";

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] internal static IPluginLog Log { get; private set; } = null!;

        private Configuration Configuration { get; init; }
        private WindowSystem WindowSystem = new("Roll67Plugin");
        private ConfigWindow ConfigWindow { get; init; }
        private SoundPlayer? soundPlayer;
        // Matches both "/random" style: "rolls 67" or "rolls a 67"
        // AND loot style: "rolls Need on the item. 67!"
        private readonly Regex rollPattern = new Regex(@"rolls? (?:(?:a )?(\d+)|(?:Need|Greed|Pass) on .+?\.\s*(\d+)!)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Roll67Plugin()
        {
            this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            ConfigWindow = new ConfigWindow(this, this.Configuration);
            WindowSystem.AddWindow(ConfigWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the Roll Sounds configuration window"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            ChatGui.ChatMessage += OnChatMessage;

            Log.Info("Roll Sound Player loaded.");
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            ConfigWindow.Dispose();
            CommandManager.RemoveHandler(CommandName);
            ChatGui.ChatMessage -= OnChatMessage;
            soundPlayer?.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        private void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Configuration.Enabled)
                return;

            var text = message.TextValue;
            var match = rollPattern.Match(text);
            
            if (!match.Success)
                return;

            // The regex has two capture groups: (1) for /random style, (2) for loot style
            // One will be empty, the other will have the number
            string rollString = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            
            if (!int.TryParse(rollString, out int rollValue))
                return;

            // Check if this roll number has a configured sound
            var mapping = Configuration.RollSoundMappings.FirstOrDefault(m => m.RollNumber == rollValue);
            
            if (mapping != null)
            {
                Log.Info($"[RollSounds] Detected roll {rollValue}, playing sound.");
                PlaySound(mapping.SoundFilePath);
            }
        }

        private void PlaySound(string soundFilePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(soundFilePath) && File.Exists(soundFilePath))
                {
                    soundPlayer?.Dispose();
                    soundPlayer = new SoundPlayer(soundFilePath);
                    soundPlayer.Play();
                }
                else
                {
                    Log.Error($"[RollSounds] Sound file missing or invalid: {soundFilePath}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RollSounds] Error playing sound: {ex.Message}");
            }
        }

        public void SaveConfiguration()
        {
            PluginInterface.SavePluginConfig(this.Configuration);
        }
    }
}
