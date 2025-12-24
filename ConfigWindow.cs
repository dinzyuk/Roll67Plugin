using System;
using System.Numerics;
using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System.IO;
using System.Windows.Forms;

namespace Roll67Plugin
{
    public class ConfigWindow : Window, IDisposable
    {
        private Roll67Plugin plugin;
        private Configuration configuration;
        
        // For adding new mappings
        private int newRollNumber = 1;
        private string newSoundPath = string.Empty;
        
        // For tracking which mapping to delete
        private int? indexToDelete = null;

        public ConfigWindow(Roll67Plugin plugin, Configuration configuration) : base(
            "Roll Sound Configuration",
            ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.plugin = plugin;
            this.configuration = configuration;
            
            // Migrate legacy configuration if needed
            if (configuration.RollSoundMappings.Count == 0 && !string.IsNullOrEmpty(configuration.SoundFilePath))
            {
                configuration.RollSoundMappings.Add(new RollSoundMapping 
                { 
                    RollNumber = 67, 
                    SoundFilePath = configuration.SoundFilePath 
                });
                plugin.SaveConfiguration();
            }
        }

        public void Dispose()
        {
        }

        public override void Draw()
        {
            // Enable/Disable checkbox
            var enabled = this.configuration.Enabled;
            if (ImGui.Checkbox("Enable Roll Sounds", ref enabled))
            {
                this.configuration.Enabled = enabled;
                this.plugin.SaveConfiguration();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Display existing mappings
            ImGui.Text("Configured Roll Numbers:");
            ImGui.Spacing();

            if (configuration.RollSoundMappings.Count == 0)
            {
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "No roll sounds configured yet.");
            }
            else
            {
                // Table header
                ImGui.Columns(3, "mappings", true);
                ImGui.Text("Roll #");
                ImGui.NextColumn();
                ImGui.Text("Sound File");
                ImGui.NextColumn();
                ImGui.Text("Actions");
                ImGui.NextColumn();
                ImGui.Separator();

                // Display each mapping
                for (int i = 0; i < configuration.RollSoundMappings.Count; i++)
                {
                    var mapping = configuration.RollSoundMappings[i];
                    
                    // Roll number
                    ImGui.Text(mapping.RollNumber.ToString());
                    ImGui.NextColumn();
                    
                    // Sound file with status
                    bool fileExists = File.Exists(mapping.SoundFilePath);
                    if (fileExists)
                    {
                        ImGui.TextColored(new Vector4(0, 1, 0, 1), "✓");
                        ImGui.SameLine();
                    }
                    else
                    {
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), "✗");
                        ImGui.SameLine();
                    }
                    
                    string fileName = string.IsNullOrEmpty(mapping.SoundFilePath) 
                        ? "(no file)" 
                        : Path.GetFileName(mapping.SoundFilePath);
                    ImGui.Text(fileName);
                    
                    if (ImGui.IsItemHovered() && !string.IsNullOrEmpty(mapping.SoundFilePath))
                    {
                        ImGui.SetTooltip(mapping.SoundFilePath);
                    }
                    
                    ImGui.NextColumn();
                    
                    // Delete button
                    if (ImGui.Button($"Delete##delete{i}"))
                    {
                        indexToDelete = i;
                    }
                    ImGui.NextColumn();
                }
                
                ImGui.Columns(1);
                
                // Handle deletion outside the loop
                if (indexToDelete.HasValue)
                {
                    configuration.RollSoundMappings.RemoveAt(indexToDelete.Value);
                    plugin.SaveConfiguration();
                    indexToDelete = null;
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Add new mapping section
            ImGui.Text("Add New Roll Sound:");
            ImGui.Spacing();

            // Roll number input
            ImGui.Text("Roll Number (1-999):");
            ImGui.SetNextItemWidth(150);
            ImGui.InputInt("##newrollnumber", ref newRollNumber);
            if (newRollNumber < 1) newRollNumber = 1;
            if (newRollNumber > 999) newRollNumber = 999;

            ImGui.Spacing();

            // Sound file path input
            ImGui.Text("Sound File Path:");
            ImGui.SetNextItemWidth(350);
            ImGui.InputText("##newsoundpath", ref newSoundPath, 500);
            
            ImGui.SameLine();
            if (ImGui.Button("Browse..."))
            {
                OpenFileDialog();
            }
            
            ImGui.Spacing();

            // Add button
            if (ImGui.Button("Add Roll Sound"))
            {
                if (string.IsNullOrEmpty(newSoundPath))
                {
                    ImGui.OpenPopup("ErrorEmpty");
                }
                else if (!File.Exists(newSoundPath))
                {
                    ImGui.OpenPopup("ErrorNotFound");
                }
                else if (configuration.RollSoundMappings.Any(m => m.RollNumber == newRollNumber))
                {
                    ImGui.OpenPopup("ErrorDuplicate");
                }
                else
                {
                    configuration.RollSoundMappings.Add(new RollSoundMapping
                    {
                        RollNumber = newRollNumber,
                        SoundFilePath = newSoundPath
                    });
                    plugin.SaveConfiguration();
                    
                    // Reset inputs
                    newRollNumber = 1;
                    newSoundPath = string.Empty;
                    
                    ImGui.OpenPopup("Success");
                }
            }

            ImGui.Spacing();
            ImGui.TextWrapped("Supported formats: .wav files");
            ImGui.TextWrapped("Example: C:\\Users\\YourName\\Music\\roll67.wav");

            // Popups
            bool dummyOpen = true;
            
            if (ImGui.BeginPopupModal("Success", ref dummyOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Roll sound added successfully!");
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopupModal("ErrorEmpty", ref dummyOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Please enter a sound file path.");
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopupModal("ErrorNotFound", ref dummyOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Error: Sound file not found at the specified path.");
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopupModal("ErrorDuplicate", ref dummyOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text($"A sound is already configured for roll number {newRollNumber}.");
                ImGui.Text("Please delete the existing one first or choose a different number.");
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }

        private void OpenFileDialog()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "WAV files (*.wav)|*.wav|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.Title = "Select Sound File";
                    openFileDialog.Multiselect = false;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        newSoundPath = openFileDialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                plugin.SaveConfiguration(); // This logs errors through Dalamud
                Roll67Plugin.Log.Error($"Error opening file dialog: {ex.Message}");
            }
        }
    }
}
