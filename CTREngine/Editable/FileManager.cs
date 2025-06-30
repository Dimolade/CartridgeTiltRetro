using System.IO;
using CTR.Projects;
using Newtonsoft.Json;
using System.Collections.Generic;
using CTR.FileManager;
using Eto.Forms;

namespace CTR.FileManager
{
    public static class Paths
    {
        public static string GetCTRPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "CartridgeTiltRetro/");
        }
        public static List<string> GetProjectFilePaths()
        {
            string plistp = Path.Combine(GetCTRPath()+"projectlist.txt");
            string[] lines = File.ReadAllText(plistp).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            List<string> projectsLists = new List<string>();
            foreach (string l in lines)
            {
                if (l != "")
                {
                    projectsLists.Add(l);
                }
            }
            return projectsLists;
        }
        public static List<string> GetPlatformFilePaths()
        {
            string plistp = Path.Combine(GetCTRPath()+"platformlist.txt");
            string[] lines = File.ReadAllText(plistp).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string l in lines)
            {
                Console.WriteLine(l);
            }
            List<string> projectsLists = new List<string>(lines);
            return projectsLists;
        }
        public static void AddProject(string path)
        {
            string plistp = Path.Combine(GetCTRPath()+"projectlist.txt");
            string contents = File.ReadAllText(plistp);
            contents += "\n"+path;
            File.WriteAllText(plistp, contents);
            CTR.Projects.Events.Update();
        }
        public static void AddPlatform(string path, Form f)
        {
            string plistp = Path.Combine(GetCTRPath() + "platformlist.txt");
            string contents = File.ReadAllText(plistp);
            contents += "\n" + path;
            File.WriteAllText(plistp, contents);
            CTR.Platform p = JsonConvert.DeserializeObject<CTR.Platform>(File.ReadAllText(path));
            if (!Directory.Exists(p.installPath))
            {
                MessageBox.Show("This Platform file is corrupted, repairing.");
            corrupted:
                MessageBox.Show("Please select the Platform directory.");
                string dir = CTR.UIButtons.FolderSelect("Select Platform Directory", f);
                if (dir != null && dir != "")
                {
                    if (!Directory.Exists(Path.Combine(dir, "dotnet/")))
                    {
                        MessageBox.Show("This Platform isnt a Platform, are you sure this Platform is a Platform for a CTREngine Platform?");
                        goto corrupted;
                    }
                    p.installPath = dir;
                    File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(p));
                }
            }
            CTR.PlatformManager.Events.Update();
        }
        public static void MakeProjectList()
        {
            string plistp = Path.Combine(GetCTRPath()+"projectlist.txt");
            string path = Path.Combine(CTR.FileManager.Paths.GetCTRPath(), "projectlist.txt");
			string directory = Path.GetDirectoryName(path);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

            if (!File.Exists(plistp))
            {
                File.WriteAllText(plistp, "");
            }
        }
        public static void MakePlatformsList()
        {
            string plistp = Path.Combine(GetCTRPath()+"platformlist.txt");
            string path = Path.Combine(CTR.FileManager.Paths.GetCTRPath(), "platformlist.txt");
			string directory = Path.GetDirectoryName(path);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

            if (!File.Exists(plistp))
            {
                File.WriteAllText(plistp, "");
            }
        }
    }

	public static class ConfigFiles
	{
		public static void WriteTestConfig()
		{
			string path = Path.Combine(CTR.FileManager.Paths.GetCTRPath(), "config/projects");
			string directory = Path.GetDirectoryName(path);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(path, "Test");
			Console.WriteLine($"File written to: {path}");
		}
	}

    public static class Platforms
    {
        public static List<Platform> GetPlatforms()
        {
            List<Platform> plist = new List<Platform>();
            List<string> files = Paths.GetPlatformFilePaths();
            foreach (string f in files)
            {
                if (f.EndsWith(".ctrplat"))
                {
                    plist.Add(JsonConvert.DeserializeObject<Platform>(File.ReadAllText(f)));
                }
            }
            return plist;
        }

        public static void WritePlatform(Platform project, string riconpath, Eto.Forms.Form f)
        {
            string path = Path.Combine(project.installPath, project.name + ".ctrplat");
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(project));
            Console.WriteLine($"File written to: {path}");
            CTR.PlatformHandler.HandleNewProject(project, path, riconpath, f);
        }

        public static void RemovePlatform(string p)
        {
            string folderName = new DirectoryInfo(p).Name;
            p = Path.Combine(p+"/", folderName + ".ctrplat");
            Console.WriteLine(p);
            string plistp = Path.Combine(Paths.GetCTRPath(), "platformlist.txt");
            string[] lines = File.ReadAllText(plistp).Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            List<string> projectsLists = new List<string>();
            string newList = "";

            foreach (string l in lines)
            {
                if (p != l)
                {
                    Console.WriteLine("P:" + p + "\nL:" + l);
                    newList += l + "\n";
                }
            }

            File.WriteAllText(plistp, newList);
            CTR.PlatformManager.Events.Update();
        }
    }

    public static class Projects
    {
        public static void WriteProject(Project project)
        {
            string path = Path.Combine(CTR.FileManager.Paths.GetCTRPath(), "projects/" + project.name + ".ctrproj");
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(project));
            Console.WriteLine($"File written to: {path}");
            CTR.Projects.Handler.HandleNewProject(project, path);
        }

        public static List<Project> GetProjects()
        {
            List<Project> plist = new List<Project>();
            List<string> files = Paths.GetProjectFilePaths();
            foreach (string f in files)
            {
                if (f.EndsWith(".ctrproj"))
                {
                    plist.Add(JsonConvert.DeserializeObject<Project>(File.ReadAllText(f)));
                }
            }
            return plist;
        }

        public static void RemoveProject(string p)
        {
            string plistp = Path.Combine(Paths.GetCTRPath(), "projectlist.txt");
            string[] lines = File.ReadAllText(plistp).Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            List<string> projectsLists = new List<string>();
            string newList = "";

            string fullP = Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (string l in lines)
            {
                if (!string.IsNullOrWhiteSpace(l))
                {
                    string fullL = Path.GetFullPath(l).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    bool samePath = string.Equals(fullP, fullL,
                        Environment.OSVersion.Platform == PlatformID.Win32NT
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal);

                    if (!samePath)
                    {
                        newList += l + "\n";
                    }
                }
            }

            File.WriteAllText(plistp, newList);
        }
    }
}