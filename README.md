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

#### Function Parameters
Parameters are structured like this:
```csharp
public void MyFunction ( parameter /* <- indicates parameters */ int /* <- type */ myInt /* <- name */ & /* <- Parameter seperator */ string myString )
```

And to call MyFunction:
```csharp
MyFunction ( 15 & "A String" ) ;
```

#### Batch Conversion
in CTREngine, CMSV2 is Batch Converted when: Building, Viewing a Script.
#### How Batch Conversion works
It scans through the Scene and picks up SceneObject's which are Scripts.
Then it will convert them 1 by 1, saving classes in a buffer shared accross Scripts, however be aware of:
##### How to properly set up Batch Conversion with *Multiple Scripts*
Make sure you set up the following:
- Library Scripts should be at the Top, since CMSV2 Parses from Top to Bottom.
- GamePlay Scripts should be at the Bottom, so they can interact with the Libraries.

Plans:
- Buffering all Errors until the Batch Conversion is complete so that the order of function's dont matter.
## Translation Files
CMSV2 uses Translation Files to correctly Translate Things.
Each platform has its own Translation file.

A Translation file is structured like this, comments are not supported and are there for helpfulness.
```
CTRImage* <- Type  Scene.CTRImage <- CMS Usage
CTRImage* <- Type (doesnt matter) Scene::ConstCTRImage <- C++ Version
```

Actual Translation File:
```
CTRImage* Scene.CTRImage
CTRImage* Scene::ConstCTRImage ```
