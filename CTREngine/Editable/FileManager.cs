using System.IO;
using CTR.Projects;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        public static void AddPlatform(string path)
        {
            string plistp = Path.Combine(GetCTRPath()+"platformlist.txt");
            string contents = File.ReadAllText(plistp);
            contents += "\n"+path;
            File.WriteAllText(plistp, contents);
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

        public static void WritePlatform(Platform project, string riconpath)
		{
			string path = Path.Combine(project.installPath, project.name+".ctrplat");
			string directory = Path.GetDirectoryName(path);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(path, JsonConvert.SerializeObject(project));
			Console.WriteLine($"File written to: {path}");
            CTR.PlatformHandler.HandleNewProject(project, path, riconpath);
		}
    }

    public static class Projects
    {
        public static void WriteProject(Project project)
		{
			string path = Path.Combine(CTR.FileManager.Paths.GetCTRPath(), "projects/"+project.name+".ctrproj");
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
    }
}