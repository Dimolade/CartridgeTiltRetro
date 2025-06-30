using org.matheval;
using System;
using System.Collections.Generic;
using CTR.FileManager;
using System.IO;

namespace CTR.Initialization
{
	public static class InitManager
	{
		public static void Init()
		{
			CTR.FileManager.Paths.MakeProjectList();
			CTR.PlatformManager.InitPlatforms();
			CMS.Commands.Internal.RegisterCommands();
			DefaultBuildMaker.Init();
			//SampleCSCMSFunc.Commence();
			//CTR.Compiler.MakeDotnetProject("/home/deck/Documents/CTREngine/Engine/CartridgeTiltRetro/CTREngine/PlatformCompiled/", "platformCompiled").Build();
			//CMS.Interpreter.InterpretCMS(File.ReadAllText("/home/deck/Documents/CTREngine/CMS/HumanMade.cms")).Run();
			//CMS.CMSV2ConversionResult res = CMS.CMSV2ToCpp.Convert(File.ReadAllText("CMS/CMSToCpp/CMSV2.cms"), "/home/deck/CartridgeTiltRetro/N3DS/engine/source/");
			//Console.WriteLine(res.SumUp());
		}
	}

	public static class DefaultBuildMaker
	{
		public static string DefaultTranslator =
@"void MyNamespace.DoAnything
void MyNamespace::DoAnything
int MyNamespace.MyInt
int MyNamespace::MyInt
CTRImage* Scene.GetCTRImage
CTRImage* Scene::GetCTRImage
CTRSound* Scene.GetCTRSound
CTRSound* Scene::GetCTRSound
void Scene.AddSceneObject
void Scene::AddSceneObject
CTRImage* Scene.CTRImage
CTRImage* Scene::ConstCTRImage
CTRSound* Scene.CTRSound
CTRSound* Scene::ConstCTRSound
CTRImageFont* Scene.CTRImageFont
CTRImageFont* Scene::ConstCTRImageFont
RNG Random
RNG RNG
Vector3 Vector3
Vector3 Vector3
Vector2 Vector2
Vector2 Vector2
bool Actions.Get
bool Actions::Get
bool Actions.GetDown
bool Actions::Down
bool Actions.GetUp
bool Actions::Up
SAM CTRENGINEEXTRADONTUSE
SAM CTRENGINEEXTRADONTUSE
void Log.Append
void Log::Append
void Log.Subtract
void Log::Subtract
void Log.Save
void Log::Save
void Log.Clear
void Log::Clear
string Log.Get
string Log::Get
string Log.GetLocation
string Log::GetLocation
string Game.Name
string Game::Name
string Game.Author
string Game::Author
string Game.Description
string Game::Description
string Game.Version
string Game::Version";
		public static void Init()
		{
			string path = Path.Combine(Paths.GetCTRPath(), "DefaultBuild/");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			File.WriteAllText(Path.Combine(path, "TRANSLATION.cmsv2t"), DefaultTranslator);
		}
	}
}