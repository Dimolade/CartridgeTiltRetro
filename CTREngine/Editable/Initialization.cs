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
vector<GameAsset*> Scene.GameAssets
vector<GameAsset*> GameAssets
GameAsset* CTRENGINEEXTRADONTUSE
GameAsset* CTRENGINEEXTRADONTUSE
CTRImage* Scene.GetCTRImage
CTRImage* Scene::GetCTRImage
CTRSound* Scene.GetCTRSound
CTRSound* Scene::GetCTRSound
void Scene.AddSceneObject
void Scene::AddSceneObject
void Scene.AddCamera
void Scene::AddCamera
CTRImage* Scene.CTRImage
CTRImage* Scene::ConstCTRImage
CTRSound* Scene.CTRSound
CTRSound* Scene::ConstCTRSound
CTRImageFont* Scene.CTRImageFont
CTRImageFont* Scene::ConstCTRImageFont
CTRCamera* Scene.CTRCamera
CTRCamera* Scene::ConstCTRCamera
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
string CTRGame::Name
string Game.Author
string CTRGame::Author
string Game.Description
string CTRGame::Description
string Game.Version
string CTRGame::Version
ScreenInfo Platform.GetScreen
ScreenInfo Platform::GetScreen
float Time.deltaTime
float Time::deltaTime
Vector2 Input.GamePad.GetRightJoy
Vector2 GamePad::GetRightJoy
Vector2 Input.GamePad.GetLeftJoy
Vector2 GamePad::GetLeftJoy
bool Input.Actions.Get
bool Actions::Get
bool Input.Actions.GetDown
bool Actions::GetDown
bool Input.Actions.GetUp
bool Actions::GetUp
void Input.Mouse.Init
void Mouse::InitMouse
void Input.Mouse.Update
void Mouse::UpdateMouse
Vector2 Input.Mouse.position
Vector2 Mouse::position
Vector2 Input.Mouse.screen
Vector2 Mouse::screen
CTRScissor CTRScissor
CTRScissor CTRScissor";
		public static void Init()
		{
			string path = Path.Combine(Paths.GetCTRPath(), "DefaultBuild/");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			File.WriteAllText(Path.Combine(path, "TRANSLATOR.cmsv2t"), DefaultTranslator);
		}
	}
}