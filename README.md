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

Heres the source code to my N3DS Platform, its not fully complete yet but it'll be done soon.
https://github.com/Dimolade/CTREngine-N3DS

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

You might have already noticed the space between every Token, this is intended as part of Syntax. <br>
`);` <- Errors <br>
`) ;` <- Correct <br>

#### Function Parameters
Parameters are structured like this:
```csharp
public void MyFunction ( parameter /* <- indicates parameters */ int /* <- type */ myInt /* <- name */ , /* <- Parameter seperator */ string myString )
```

And to call MyFunction:
```csharp
MyFunction ( 15 , "A String" ) ;
```

#### Batch Conversion
in CTREngine, CMSV2 is Batch Converted when: Building, Viewing a Script.

#### How Batch Conversion works
It First does a "Symbol Run", where it collects names of Returners (Functions, Vars). It also collects classes. <br>
Then comes the actual conversion, itll look in the Symbols if the Returner hasnt been defined yet. <br>

#### How to make scripts interact with each other

Use the `Use` operator at the top of CMSV2 Scripts to include other Scripts by forward declaring its Classes. <br>
Usage: `Use MyOtherCMSV2Script ; ` <br>

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
