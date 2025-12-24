using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace Roll67Plugin
{
    [Serializable]
    public class RollSoundMapping
    {
        public int RollNumber { get; set; }
        public string SoundFilePath { get; set; } = string.Empty;
    }

    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool Enabled { get; set; } = true;
        
        // Legacy single sound support (for backwards compatibility)
        public string SoundFilePath { get; set; } = string.Empty;
        
        // New multi-sound support
        public List<RollSoundMapping> RollSoundMappings { get; set; } = new List<RollSoundMapping>();

        public Configuration()
        {
            // Initialize with the classic 67 mapping if nothing exists
            if (RollSoundMappings.Count == 0 && !string.IsNullOrEmpty(SoundFilePath))
            {
                RollSoundMappings.Add(new RollSoundMapping 
                { 
                    RollNumber = 67, 
                    SoundFilePath = SoundFilePath 
                });
            }
        }

        public void Save()
        {
            // Save is now handled by DalamudPluginInterface.SavePluginConfig in the main plugin class
        }
    }
}
