# Roll67 FFXIV Plugin

A Dalamud plugin that plays a custom sound whenever someone rolls a 67 in Final Fantasy XIV.

## Features

- Automatically detects when anyone rolls a 67 in chat
- Plays a custom sound file of your choice
- Simple configuration window to set your sound file
- Enable/disable toggle

## Installation

### Prerequisites

1. You need XIVLauncher with Dalamud installed
2. Visual Studio 2022 or later (with .NET 8.0 SDK)
3. A .wav sound file you want to play

### Building the Plugin

1. Clone or download this repository
2. Open `Roll67Plugin.csproj` in Visual Studio
3. Make sure the Dalamud references are correctly pointing to your Dalamud installation:
   - Default path: `%appdata%\XIVLauncher\addon\Hooks\dev\`
   - If different, update the `DalamudLibPath` in the .csproj file
4. Build the solution (Build > Build Solution or Ctrl+Shift+B)
5. The compiled DLL will be in `bin/Debug/net8.0-windows/` or `bin/Release/net8.0-windows/`

### Installing the Plugin

1. Copy the following files to your Dalamud devPlugins folder:
   - `Roll67Plugin.dll`
   - `Roll67Plugin.json`
   
   Default devPlugins location: `%appdata%\XIVLauncher\devPlugins\Roll67Plugin\`

2. Launch FFXIV through XIVLauncher
3. Type `/xlplugins` in game to open the plugin installer
4. Enable the "Roll67 Sound Player" plugin

## Usage

1. Type `/roll67config` in game to open the configuration window
2. Enter the full path to your .wav sound file (e.g., `C:\Users\YourName\Music\roll67.wav`)
3. Click "Save"
4. Make sure "Enable Roll67 Sound" is checked
5. Now whenever anyone rolls a 67, your sound will play!

## Supported Audio Formats

Currently supports: **WAV files only**

For best results:
- Use uncompressed WAV files
- Keep the file size reasonable (under 5MB)
- Test your sound file to make sure it plays correctly

## Configuration

- `/roll67config` - Opens the configuration window
- Toggle the plugin on/off with the checkbox
- Change the sound file path at any time

## Troubleshooting

**Sound not playing?**
- Make sure the file path is correct and the file exists
- Check that the file is a .wav format
- Verify the plugin is enabled in the configuration
- Make sure your game sound isn't muted

**Plugin not loading?**
- Check that all required DLLs are in the plugin folder
- Verify you're using the correct Dalamud API level
- Check the Dalamud log for errors

## Development Notes

This plugin uses:
- Dalamud API Level 14
- .NET 8.0
- Regex pattern matching to detect roll messages
- SoundPlayer for audio playback

The plugin listens to chat messages and uses a regex pattern to detect dice rolls. When a roll of 67 is detected, it plays the configured sound file.

## License

Feel free to modify and distribute as needed.

## Credits

Created for the FFXIV community. Have fun with your 67 rolls!
