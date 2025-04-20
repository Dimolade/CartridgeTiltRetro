using System;
using System.IO;
using Eto.Forms;
using Eto.Drawing;
using CMS;
using System.Collections;
using System.Collections.Generic;

namespace CMS
{
    public class CMSIDEWindow : Form
    {
        protected Form windoh;
        private StackLayout topButtons;
        public ListBox VariableList;
        public CMSScript lastRunCMSScript;
        public ListBox InstrucList;

        public CMSIDEWindow()
        {
            Title = "CMS IDE";
            var screen = Screen.PrimaryScreen;
            var workingArea = screen.WorkingArea;
            ClientSize = new Size((int)workingArea.Width, (int)workingArea.Height);
            Resizable = false;
            windoh = this;
            int y = (int)workingArea.Height;
            int x = (int)workingArea.Width;
            Console.WriteLine("X Area: "+x+" Y Area: "+y);

            this.Icon = new Icon("gfx/Icon.ico");

            VariableList = new ListBox
            {
                Width = 250,
                Height = y-30
            };

            InstrucList = new ListBox
            {
                Width = 250,
                Height = y-30
            };

            VariableList.Items.Add("Variable List (Empty)");

            topButtons = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Visible = false // Hidden by default
            };

            var ideText = new WebView
            {
                Size = new Size(1670-250, y-30)
            };

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MonacoEditor/index.html");
            ideText.Url = new Uri($"file://{filePath}");

            var menu = new MenuBar();
            var cmsMenu = new ButtonMenuItem { Text = "CMS" };
            var ctrMenu = new ButtonMenuItem { Text = "CTR" };
            var fileMenu = new ButtonMenuItem { Text = "File" };
            cmsMenu.Click += (sender, e) => ShowButtons(new List<string> { "Run Script" }, this, x, y, ideText);
            ctrMenu.Click += (sender, e) => ShowButtons(new List<string> { "Export Script to Project" }, this, x, y, ideText);
            fileMenu.Click += (sender, e) => ShowButtons(new List<string> { "Open Script" }, this, x, y, ideText);

            menu.Items.Add(cmsMenu);
            menu.Items.Add(ctrMenu);
            menu.Items.Add(fileMenu);
            Menu = menu;

            var layout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items =
                {
                    topButtons,
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Items =
                        {
                            InstrucList,
                            VariableList,
                            ideText
                        }
                    }
                }
            };

            Content = layout;
        }

        bool showingPButtons = false;

        private void UpdateVariableList(CMSScript cms)
        {
            VariableList.Items.Clear();
            VariableList.Items.Add("/// Variable List \\\\\\");
            VariableList.Items.Add("--- .int ---");
            List<string> Namespaces = new List<string>();
            foreach (CMSInt Int in cms.intVars)
            {
                if (Namespaces.Contains(Int.Namespace) == false)
                {
                    VariableList.Items.Add("--- Namespace \""+Int.Namespace+"\" ---");
                    Namespaces.Add(Int.Namespace);
                }

                VariableList.Items.Add(Int.name +" : "+Int.value);
            }

            VariableList.Items.Add("--- .void ---");
            Namespaces.Clear();
            foreach (CMSVoid Void in cms.voidVars)
            {
                if (Namespaces.Contains(Void.Namespace) == false)
                {
                    VariableList.Items.Add("--- Namespace \""+Void.Namespace+"\" ---");
                    Namespaces.Add(Void.Namespace);
                }

                VariableList.Items.Add(Void.name +" : "+Void.instructionStart);
            }


            VariableList.Items.Add("--- .get Namespaces ---");
            Namespaces.Clear();
            foreach (CMSInt Int in cms.namespaceIntVars)
            {
                if (Namespaces.Contains(Int.Namespace) == false)
                {
                    VariableList.Items.Add("--- Namespace \""+Int.Namespace+"\" ---");
                    Namespaces.Add(Int.Namespace);
                }

                VariableList.Items.Add(Int.Namespace+"."+Int.name +" : "+Int.value);
            }

            VariableList.Items.Add("--- .setClass classes ---");
            Namespaces.Clear();
            foreach (CMSClass Int in cms.classVars)
            {
                if (Namespaces.Contains(Int.Namespace) == false)
                {
                    VariableList.Items.Add("--- Namespace \""+Int.Namespace+"\" ---");
                    Namespaces.Add(Int.Namespace);
                }

                VariableList.Items.Add(Int.Namespace+"."+Int.name);
                VariableList.Items.Add("--- .int ---");
                foreach (CMSInt CInt in Int.intVars)
                {
                    VariableList.Items.Add("⏎ "+CInt.name+" : "+CInt.value);
                }
                VariableList.Items.Add("--- .void ---");
                foreach (CMSVoid CVoid in Int.voidVars)
                {
                    VariableList.Items.Add("⏎ "+CVoid.name+" : "+CVoid.instructionStart);
                }
            }
            VariableList.Items.Add("/// Finish \\\\\\");
        }

        private void UpdateInstructionList(CMSScript cms)
        {
            InstrucList.Items.Clear();
            int i = 0;
            foreach (string ins in cms.instructions)
            {
                InstrucList.Items.Add(cms.GetInstruction(i)+" : "+i);
                i++;
            }
        }

        private void DoButton(string name, Panel callFrom, WebView wv)
        {
            if (name == "Run Script")
            {
                CMSScript cms = CMS.Interpreter.InterpretCMS(GetTextFromEditor(wv));
                cms.Run();
                lastRunCMSScript = cms;
                UpdateVariableList(cms);
                UpdateInstructionList(cms);
            }
            if (name == "Open Script")
            {
                string path = CTR.UIButtons.FileSelect("Choose CMS Script", new List<string>() {".cms"}, callFrom);
                if (path == null)
                    return;

                if (ShowConfirmationDialog("Are you sure?", "Are you sure you wanna open this script? (Unsaved progress will be lost.)", callFrom))
                {
                    SetTextInEditor(wv, File.ReadAllText(path));
                }
            }
            if (name == "Export Script to Project")
            {
                CTR.Projects.Project p = ProjectPopUp("Choose Target Project", "Choose the Project to export this script to:", callFrom);
                string scriptName = ShowInputDialog("Choose File Name", "Choose the name of the Script.", callFrom);
                File.WriteAllText(Path.Combine(p.path, "Assets/"+scriptName+".cms"), GetTextFromEditor(wv));
            }
        }

        private void ShowButtons(List<string> platformNames, Panel callFrom, int x,int y, WebView wv)
        {
            showingPButtons = !showingPButtons;
            if (showingPButtons)
            {
            topButtons.Items.Clear(); // Clear previous buttons

            foreach (var name in platformNames)
            {
                var button = new Button
                {
                    Text = name,
                    Width = 200,
                    Height = 30
                };

                button.Click += (sender, e) => {DoButton(name, callFrom, wv);};
                topButtons.Items.Add(button);
            }

            topButtons.Visible = true; // Make the button panel visible
            }
            else
            {
                topButtons.Items.Clear();
                windoh.ClientSize = new Size(x,y);
            }
        }

        public string GetTextFromEditor(WebView webView)
        {
            string script = @"
                var editor = monaco.editor.getModels()[0];  // Get the first editor model
                return editor.getValue();  // Get the content of the editor
            ";

            // Execute the JavaScript inside the WebView and handle the result
            var result = webView.ExecuteScript(script);
            string editorText = result.ToString();

            return editorText;
        }

        public void SetTextInEditor(WebView webView, string newText)
        {
            string escapedText = newText.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
            
            string script = $@"
                var editor = monaco.editor.getModels()[0];  // Get the first editor model
                editor.setValue(""{escapedText}"");  // Set the content of the editor
            ";

            // Execute the JavaScript inside the WebView
            webView.ExecuteScript(script);
        }
    
        public bool ShowConfirmationDialog(string title, string message, Panel callFrom)
        {
            var dialog = new Dialog<bool>
            {
                Title = title,
                ClientSize = new Size(300, 150),
                Resizable = false,
                Padding = 10
            };

            var layout = new StackLayout
            {
                Spacing = 10,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    new Label { Text = message, TextAlignment = TextAlignment.Center },
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Items =
                        {
                            new Button { Text = "Yes", Command = new Command((sender, e) => { dialog.Result = true; dialog.Close(); }) },
                            new Button { Text = "No", Command = new Command((sender, e) => { dialog.Result = false; dialog.Close(); }) }
                        }
                    }
                }
            };

            dialog.Content = layout;
            dialog.ShowModal(callFrom);

            return dialog.Result;
        }
    
        public CTR.Projects.Project ProjectPopUp(string title, string message, Panel callFrom)
        {
            var dialog = new Dialog<CTR.Projects.Project>
            {
                Title = title,
                ClientSize = new Size(300, 180),
                Resizable = false,
                Padding = 10
            };

            var projects = CTR.FileManager.Projects.GetProjects(); // Fetch the list of projects

            var projectDropdown = new DropDown();
            var projectDict = new Dictionary<string, CTR.Projects.Project>();

            foreach (var project in projects)
            {
                projectDropdown.Items.Add(project.name);  // Populate dropdown
                projectDict[project.name] = project;      // Map project name to actual project
            }

            projectDropdown.SelectedIndex = 0; // Default to the first item

            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) => dialog.Close();

            var layout = new StackLayout
            {
                Spacing = 10,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    new Label { Text = message, TextAlignment = TextAlignment.Center },
                    projectDropdown,
                    okButton
                }
            };

            dialog.Content = layout;
            dialog.ShowModal(callFrom);

            var selectedProjectName = projectDropdown.SelectedValue?.ToString();
            return selectedProjectName != null && projectDict.ContainsKey(selectedProjectName) 
                ? projectDict[selectedProjectName] 
                : null;
        }
    
        public string ShowInputDialog(string title, string message, Panel callFrom)
        {
            var dialog = new Dialog<string>
            {
                Title = title,
                ClientSize = new Size(300, 150),
                Resizable = false,
                Padding = 10
            };

            var inputBox = new TextBox { Width = 250 };

            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) => dialog.Close();

            var layout = new StackLayout
            {
                Spacing = 10,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    new Label { Text = message, TextAlignment = TextAlignment.Center },
                    inputBox,
                    okButton
                }
            };

            dialog.Content = layout;
            dialog.ShowModal(callFrom);

            return inputBox.Text;
        }
    }
    public static class CMSTester
    {

    }
}