using Eto.Forms;
using Eto.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace CTR
{
    public static class UIButtons
    {
        public static void OnButtonPress(string btn, Panel callFrom, int buttonID = -1)
        {
            //MessageBox.Show($"You clicked: {btn}");
            if (btn == "New Project")
            {
                var dialog = new NewProjWindow("New Project", "Project Name");
                string result = dialog.ShowModal();
            }
            if (btn == "Add Project")
            {
                string proj = FileSelect("Select Project", new List<string>() {".ctrproj"} ,callFrom);
                if (proj == null) return;
                CTR.FileManager.Paths.AddProject(proj);
            }
            if (btn == "Platform Manager")
            {
                CTR.PlatformManager.PMW = new PlatformManagerWindow();
            
                CTR.PlatformManager.PMW.Show();
            }
            if (btn == "Open CMS IDE")
            {
                CMS.CMSIDEWindow cmsIDE;
                cmsIDE = new CMS.CMSIDEWindow();
            
                cmsIDE.Show();
            }
        }

        public static string FileSelect(string title, List<string> filters, Panel thing)
        {
            // Create a list of FileFilter objects
            var filterList = new List<FileFilter>();
            foreach (var filter in filters)
            {
                filterList.Add(new FileFilter($"*{filter} files", filter));
            }

            string originalDir = Directory.GetCurrentDirectory();

            var dialog = new OpenFileDialog
            {
                Title = title/*,
                Filters = filterList*/
            };

            if (dialog.ShowDialog(thing) == DialogResult.Ok)
            {
                Directory.SetCurrentDirectory(originalDir);
                return dialog.FileName;
            }

            Directory.SetCurrentDirectory(originalDir);
            return null;
        }

        public static string FolderSelect(string title, Panel thing)
        {
            var dialog = new SelectFolderDialog
            {
                Title = title
            };

            if (dialog.ShowDialog(thing) == DialogResult.Ok)
            {
                string selectedPath = dialog.Directory;
                return selectedPath;
            }
            return null;
        }

        public class NewProjWindow : Dialog<string>
        {
            private TextBox inputBox;

            public string ShowFolderDialog(string title, NewProjWindow thing)
            {
                var dialog = new SelectFolderDialog
                {
                    Title = title
                };

                if (dialog.ShowDialog(thing) == DialogResult.Ok)
                {
                    string selectedPath = dialog.Directory;
                    return selectedPath;
                }
                return CTR.FileManager.Paths.GetCTRPath();
            }

            public NewProjWindow(string title, string message, int x = 300, int y = 200)
            {
                Title = title;
                ClientSize = new Size(x, y);
                Resizable = false;

                // Label at the top
                var label = new Label
                {
                    Text = message,
                    TextAlignment = TextAlignment.Center
                };

                string path = CTR.FileManager.Paths.GetCTRPath();

                var label2 = new Label
                {
                    Text = path,
                    TextAlignment = TextAlignment.Center
                };

                // Text input field
                inputBox = new TextBox();

                // OK and Cancel buttons
                var ChooseFolderbtn = new Button { Text = "Project Location" };
                var okButton = new Button { Text = "OK" };
                var cancelButton = new Button { Text = "Cancel" };
                string lastPath = CTR.FileManager.Paths.GetCTRPath();

                ChooseFolderbtn.Click += (sender, e) => { lastPath = ShowFolderDialog("Choose Project Location", this); path = System.IO.Path.Combine(lastPath,inputBox.Text); label2.Text = path; };
                okButton.Click += (sender, e) => { CTR.FileManager.Projects.WriteProject(new CTR.Projects.Project(inputBox.Text, path, CTR.Env.ctrVersion, CTR.Env.defaultPlatformID, CTR.Env.defaultCSMVersion, CTR.FileManager.Paths.GetCTRPath()+"/projects/"+inputBox.Text+".ctrproj"));Result = inputBox.Text; Close(); };
                cancelButton.Click += (sender, e) => { Result = null; Close(); };
                inputBox.TextChanged += (sender, e) => {path = System.IO.Path.Combine(lastPath,inputBox.Text); label2.Text = path;};

                var Boxlayout = new StackLayout
                {
                    Orientation = Orientation.Vertical,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 10,
                    Items =
                    {
                        label,inputBox
                    },
                    Width = x,
                    Height = 75
                };

                // Stack buttons vertically
                var buttonLayout = new StackLayout
                {
                    Orientation = Orientation.Vertical,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Spacing = 10,
                    Items =
                    {
                        label2,
                        ChooseFolderbtn,
                        okButton,
                        cancelButton
                    },
                    Width = x,
                    Height = y
                };

                // Main layout
                Content = new StackLayout
                {
                    Orientation = Orientation.Vertical,
                    Padding = 10,
                    Spacing = 10,
                    Items =
                    {
                        Boxlayout,
                        buttonLayout
                    }
                };
            }
        }
    }
}