using org.matheval;
using System;
using System.Collections.Generic;

namespace CTR.Initialization
{
	public static class InitManager
	{
		public static void Init()
		{
            CTR.FileManager.Paths.MakeProjectList();
			CTR.PlatformManager.InitPlatforms();
			CMS.Commands.Internal.RegisterCommands();
			//SampleCSCMSFunc.Commence();
			//CTR.Compiler.MakeDotnetProject("/home/deck/Documents/CTREngine/Engine/CartridgeTiltRetro/CTREngine/PlatformCompiled/", "platformCompiled").Build();
			//CMS.Interpreter.InterpretCMS(File.ReadAllText("/home/deck/Documents/CTREngine/CMS/HumanMade.cms")).Run();
		}
	}
}