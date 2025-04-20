using System.IO;

namespace CTR.Projects
{
    public class Project
    {
        public string name;
        public string path;
        public string ctrVersion;
        public string platformID;
        public string csmVersion;
        public string ctrProjPath;
        
        public Project(string name, string path, string ctrVersion, string platformID, string csmVersion, string ctrProjPath)
        {
            this.name = name;
            this.path = path;
            this.ctrVersion = ctrVersion;
            this.platformID = platformID;
            this.csmVersion = csmVersion;
            this.ctrProjPath = ctrProjPath;
        }
    }

    public static class Events
    {
        public static void Update()
        {
            CTR.Window.WManager.ProjectsHolder.Items.Clear();
            foreach (var project in CTR.FileManager.Projects.GetProjects())
            {
                CTR.Window.WManager.ProjectsHolder.Items.Add(project.name);
            }
        }
    }

    public static class Handler
    {
        public static void HandleNewProject(Project p, string ctrprojpath)
        {
            string path = p.path;
            Directory.CreateDirectory(Path.Combine(path, "Assets"));
            FileManager.Paths.AddProject(ctrprojpath);
            CTR.Projects.Events.Update();
        }

        public static void OpenProjectEditor(Project p)
        {
            CTR.EditorWindow EW = new CTR.EditorWindow(p);
            EW.Show();
        }
    }
}