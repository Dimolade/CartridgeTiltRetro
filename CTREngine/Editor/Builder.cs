using CTR;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using CTR.Projects;
using CMS;

public static class CMSWatcher
{
    public static EditorWindow editorWindow;
    private static List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

    public static void Setup(Project p)
    {
        Check();
        string basePath = p.path;
        string[] cmsFiles = Directory.GetFiles(basePath, "*.cms", SearchOption.AllDirectories);
        
        foreach (string cmsFile in cmsFiles)
        {
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(cmsFile),
                Filter = Path.GetFileName(cmsFile),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            watcher.Changed += (object sender, FileSystemEventArgs e) =>
            {
                Application.Instance.Invoke(() => Check());
            };

            watchers.Add(watcher);
        }
    }

    public static bool Check()
    {
        bool returnFalse = false;
        Builder.failList = "";
        List<string> tc = new List<string>();
        List<string> ps = new List<string>();
        List<string> anames = new List<string>();
        foreach (SceneObject a in EditorTools.currentScene)
        {
            if (a.assetType == AssetType.Script)
            {
                /*CMS.CMSV2ConversionResult res = CMS.CMSV2ToCpp.Convert(File.ReadAllText(a.asset.path), CTR.FileManager.Paths.GetCTRPath() + "/DefaultBuild/");
                if (res.failed)
                {
                    Builder.canBuild = false;
                    returnFalse = true;
                    res.whyFailed.Tokens = res.conversion.Tokens;
                    Builder.failList += a.name + " : " + res.whyFailed.fullError() + "\n";
                    Console.WriteLine("Failed converting!");
                }
                else
                {
                    Builder.failList += a.name + " : " + "Success!\n";
                }*/ // Non Batching ^^
                a.asset.TryFixIfPossible();
                tc.Add(File.ReadAllText(a.asset.path));
                ps.Add(a.asset.path);
                anames.Add(a.name);
            }
        }
        CMSV2BatchCR br = CMSV2ToCpp.ConvertBatch(tc, ps, CTR.FileManager.Paths.GetCTRPath() + "/DefaultBuild/");
        int i = 0;
        foreach (CMSV2ConversionResult res in br.CMSV2CR)
        {
            if (res.failed)
            {
                Builder.canBuild = false;
                returnFalse = true;
                res.whyFailed.Tokens = res.conversion.Tokens;
                Builder.failList += anames[i] + " : " + res.whyFailed.fullError() + "\n";
                Console.WriteLine("Failed converting!");
            }
            else
            {
                Builder.failList += anames[i] + " : " + "Success!\n";
            }
            i++;
        }
        Builder.canBuild = !returnFalse;
        editorWindow.UpdateConsole();
        //if (returnFalse == true) MessageBox.Show("Scripts had errors while Converting:\n"+Builder.failList, "Fix Script Errors!");
        return !returnFalse;
    }
}

public static class Builder
{
    public static bool canBuild = false;
    public static string failList = "";

    public static void GenerateEntryPoints(Project p, List<CMSV2ConversionResult> results, List<SceneObject> soL)
    {
        string directory = Path.GetDirectoryName(Path.Combine(p.path, "Build/"));
        string targetPath = Path.Combine(p.path, "Build/");
        List<string> entryPoints = new List<string>();
        foreach (CMSV2ConversionResult ress in results)
        {
            foreach (CMSV2Class cl in ress.classes)
            {
                if (cl.inheritants.Contains("CTREntry"))
                {
                    entryPoints.Add(cl.name);
                }
            }
        }

        string cppSnippetFile = "";
        for (int i = 0; i < results.Count; i++)
        {
            string ep = entryPoints[i];
            string fName = soL[i].Namespace + soL[i].name;
            cppSnippetFile += (ep+"* "+fName + " = new " + ep + "();\n");
        }
        File.WriteAllText(targetPath+"EntryPoints",cppSnippetFile);
    }

    public static void Build(Form f, Project p)
    {
        if (!canBuild)
        {
            MessageBox.Show("Cannot build because these scripts had errors:\n" + failList, "Fix Script Errors!");
            return;
        }
        SelectPlatformDialog SPL = new SelectPlatformDialog("Choose Platform to Build to:");
        SPL.ShowModal(f);
        if (SPL.choseYes)
        {
            // Make Build Folder
            Platform p2 = CTR.FileManager.Platforms.GetPlatforms()[SPL.platform.SelectedIndex];
            string directory = Path.GetDirectoryName(Path.Combine(p.path, "Build/"));

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (string file in Directory.GetFiles(Path.Combine(p.path, "Build/")))
            {
                File.Delete(file);
            }

            // Delete all subdirectories
            foreach (string subDirectory in Directory.GetDirectories(Path.Combine(p.path, "Build/")))
            {
                if (Path.GetFileName(subDirectory) == "CTR")
                {
                    continue;
                }
                Directory.Delete(subDirectory, true); // true: delete subdirectories recursively
            }

            string targetPath = Path.Combine(p.path, "Build/");
            File.WriteAllText(targetPath + "GeneratedSceneObjects.cpp", EditorTools.GenerateSceneObjectsCPP(
                (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                "assetStoragePath"
                ), (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                "defaultImageType"
                ), (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                "imagePrefix"
                ), (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                "soundPrefix"
                ), (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                "otherPrefix"
                ), (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                "defaultSoundType"
                )
            ));

            File.WriteAllText(targetPath + "GeneratedIncludes.cpp", EditorTools.GenerateIncludeList());
            string bpath = CTR.FileManager.Paths.GetCTRPath() + (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                "sourceDirectory"
                );

            /*List<CMSV2ConversionResult> ress = new List<CMSV2ConversionResult>();
            List<SceneObject> sos = new List<SceneObject>();
            foreach (SceneObject so in EditorTools.currentScene)
            {
                Asset a = so.asset;
                if (a.GetAssetType() == AssetType.Script)
                {
                    CMS.CMSV2ConversionResult res = CMS.CMSV2ToCpp.Convert(File.ReadAllText(a.path), bpath);
                    File.WriteAllText(targetPath + a.name + ".hpp", res.Cpp);
                    ress.Add(res);
                    sos.Add(so);
                }
            }*/ // non batch

            List<string> tc = new List<string>();
            List<string> anames = new List<string>();
            List<string> ps = new List<string>();
            List<SceneObject> so = new List<SceneObject>();
            foreach (SceneObject a in EditorTools.currentScene)
            {
                if (a.assetType == AssetType.Script)
                {
                    tc.Add(File.ReadAllText(a.asset.path));
                    anames.Add(a.name);
                    ps.Add(a.asset.path);
                    so.Add(a);
                }
            }
            CMSV2BatchCR br = CMSV2ToCpp.ConvertBatch(tc, ps, CTR.FileManager.Paths.GetCTRPath() + "/DefaultBuild/");
            int i = 0;

            GenerateEntryPoints(p, br.CMSV2CR, so);
            
            //
            PlatformDLLLoader.CallBuildPlatform(
                Path.Combine(p2.installPath, "MainAssembly.dll"),
                p.ctrProjPath
            );

            Console.WriteLine("Ran with ctrproj file: " + CTR.FileManager.Paths.GetProjectFilePaths()[SPL.platform.SelectedIndex]);
        }
    }
}