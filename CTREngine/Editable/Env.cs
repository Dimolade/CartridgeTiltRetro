using System.Collections;
using System.Collections.Generic;

namespace CTR
{
    public enum Architecture
{
    n6502,      // MOS Technology 6502
    n6800,      // Motorola 6800
    n6809,      // Motorola 6809
    n6812,      // Motorola 6812
    n6811,      // Motorola 6811
    n8080,      // Intel 8080
    n8085,      // Intel 8085
    n8096,      // Intel 8096
    n8500,      // RCA 8500
    n8600,      // RCA 8600
    nZ80,       // Zilog Z80
    nMIPS,      // MIPS architecture (generic, used by many processors)
    nRISC,      // General RISC architecture
    nARM,       // ARM architecture (generic)
    nARM7,      // ARM7 (used in Game Boy Advance)
    nARM9,      // ARM9 (used in Nintendo DS, early 3DS)
    nARM11,     // ARM11 (used in Nintendo 3DS)
    nCortexA8,  // ARM Cortex-A8 (used in Nintendo 3DS)
    nCortexA9,  // ARM Cortex-A9 (used in Nintendo Wii U)
    nCortexA53, // ARM Cortex-A53 (used in Nintendo Switch)
    nCortexA57, // ARM Cortex-A57 (used in Nintendo Switch)
    nPPC,       // PowerPC architecture
    nPPC64,     // PowerPC 64-bit architecture
    nSPARC,     // SPARC architecture (from Sun Microsystems)
    nx86,       // x86 architecture (Intel/AMD)
    nARM64,     // ARM 64-bit architecture (ARMv8)
    nM68k,      // Motorola 68000 series (common in early Mac computers)
    nRaspberryPi, // Broadcom BCM2835 (used in Raspberry Pi - ARM1176JZF-S)
    nM1,        // Apple M1 (ARM-based architecture)
    nM2,        // Apple M2 (ARM-based architecture)
}
    public class Platform
    {
        public string id;
        public string name;
        public string author;
        public string version;
        public string ctrVersion;
        public string installPath;
        public string iconName;
        public string architecture;
        public CTR.DotnetProject DN;
        
        public Platform(string id, string name, string author, string version, string installPath, string iconName, string architecture, CTR.DotnetProject DN, string ctrVersion)
        {
            this.id = id;
            this.name = name;
            this.author = author;
            this.version = version;
            this.installPath = installPath;
            this.iconName = iconName;
            this.architecture = architecture;
            this.DN = DN;
        }
    }

    public static class PlatformHandler
    {
        public static void HandleNewProject(Platform project, string path, string realIconPath)
        {
            FileManager.Paths.AddPlatform(path);
            System.IO.File.Copy(realIconPath, System.IO.Path.Combine(project.installPath, project.iconName));
            System.IO.File.Delete(System.IO.Path.Combine(project.installPath,"dotnet/Program.cs"));
            System.IO.File.WriteAllText(System.IO.Path.Combine(project.installPath,"dotnet/Main.cs"), Env.defaultPlatformScript);
            CTR.PlatformManager.Events.Update();
        }
    }
    public static class Env
    {
        public static string ctrVersion = "0.0.1";
        public static string defaultPlatformID = "Dimolade.Windows";
        public static string defaultCSMVersion = "0.0.1";
        public static string defaultPlatformScript = @"
public static class PlatformFunctions
{
    // Basic Info
    public static string id; // Unique identifier for the platform
    public static string name; // Display name of the platform (e.g., Nintendo DS, Android)

    // Compatibility & Features
    public static bool buildSupported = true; // If the platform supports building games
    public static bool buildRomSupported; // If the platform supports ROM building
    public static bool hasTouchscreen; // Whether the platform has a touchscreen
    public static bool supportsAudio; // Whether the platform supports audio playback
    public static bool supportsNetworking; // Whether the platform supports online/networking features

    // Display Info
    public static int numOfScreens; // Number of screens the platform has
    public static int primaryScreenIndex; // Index of the primary screen
    public static int screenWithTouchscreen; // Index of the screen with touchscreen if applicable

    // Input Info
    public static bool supportsGamepad; // Whether the platform supports gamepad input
    public static bool supportsKeyboard; // Whether the platform supports keyboard input
    public static bool supportsMouse;

    // Storage Info
    public static bool supportsSaveData; // Whether the platform supports saving/loading data
    public static long maxSaveSize; // Maximum size of save data in bytes

    // Platform Limitations
    public static int maxTextureSize; // Maximum texture resolution allowed
    public static int maxAudioChannels; // Maximum audio channels supported

    // Functions
    public static void BuildPlatform(string pathToCTRProj)
    {
        // Build the Game (Yes thats what you do.)
    }

    public static void OnBuiltROM(string pathToBuiltROM)
    {
        // Do something when the rom finishes building.
    }
}
";
    }
}