# CartridgeTiltRetro
Game Engine made in C#, made for C++ Devices.
Uses Eto.Forms for its GUI.

# Editor

CTREngine features a small Editor.

#### Featuring:
- Images
- Image Fonts
- Sounds
- Scripts
- Building

#### Keybinds:
- Up/Down Arrow: Move Object Up/Down

#### Building:

When building, you are Prompted to Select a Platform.
C# will call a specific function on that Platforms DLL.
How the Game Looks and what works, entirely depends on the Platform.

# The Platform Manager

CTREngine features a simple Platform Manager, where you can add and make Platforms.
Platforms use Dotnet to Build Games.

###### More Platform Info coming soon.

# CMS
CMS, standing for C++ made Sharp is the Programming Language of CTREngine.
Its syntax aims to be very similar to C#.

#### Hello, World!

```csharp
public static class HelloWorld inherits CTREntry
{
  public override void EntryPoint ( )
  {
    Log.Append ( "Hello, World!" ) ; /* Add to the Log */
    Log.Save ( ) ; /* Save the Log */
  }
}
```

You might have already noticed the space between every Token, this is intended as part of Syntax.
`);` <- Errors
`) ;` <- Correct

In Version 0.0.3, Batch Converting isnt supported yet, meaning that you cannot interact with other classes yet.
## Translation Files
CMSV2 uses Translation Files to correctly Translate Things.
Each platform has its own Translation file.

A Translation file is structured like this, comments are not supported and are there for helpfulness.
```
CTRImage* <- Type  Scene.CTRImage <- CMS Usage
CTRImage* <- Type (doesnt matter) Scene::ConstCTRImage <- C++ Version
```