using Eto.Forms;
using Eto.Drawing;
using System.Collections;
using System.Collections.Generic;
using CTR;

namespace CTR
{
    public class NewPlatWindow : Dialog<string>
        {
            private TextBox inputBox;
            private TextBox idIB;
            private TextBox architecture;
            private TextBox author;
            private TextBox pversion;
            private string IconName;

            public string ShowFolderDialog(string title, NewPlatWindow thing)
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

            public DropDown CreateEnumDropDown<T>() where T : Enum
            {
                var dropdown = new DropDown();

                foreach (var value in Enum.GetValues(typeof(T)))
                {
                    dropdown.Items.Add(new ListItem { Text = value.ToString(), Key = value.ToString() });
                }

                dropdown.SelectedIndex = 0; // Optionally set a default selection
                return dropdown;
            }

            public NewPlatWindow(string title, string message, int x = 300, int y = 200)
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
                idIB = new TextBox();
                author = new TextBox();
                pversion = new TextBox()
                {
                    Text = "0.0.1"
                };
                architecture = new TextBox()
                {
                    Text = "Undefined"
                };

                var label3 = new Label
                {
                    Text = "Please make an ID:",
                    TextAlignment = TextAlignment.Center
                };

                var label4 = new Label
                {
                    Text = "Please set the Architecture (Optional):",
                    TextAlignment = TextAlignment.Center
                };

                var label6 = new Label
                {
                    Text = "Platform Version:",
                    TextAlignment = TextAlignment.Center
                };

                var label5 = new Label
                {
                    Text = "Author:",
                    TextAlignment = TextAlignment.Center
                };

                // OK and Cancel buttons
                var ChooseFolderbtn = new Button { Text = "Platform Location" };
                var okButton = new Button { Text = "OK" };
                var cancelButton = new Button { Text = "Cancel" };
                var IconButton = new Button { Text = "Choose Icon"};
                string lastPath = CTR.FileManager.Paths.GetCTRPath();

                ChooseFolderbtn.Click += (sender, e) => { lastPath = ShowFolderDialog("Choose Platform Location", this); path = System.IO.Path.Combine(lastPath,inputBox.Text); label2.Text = path; };
                
                okButton.Click += (sender, e) => { 
                    CTR.FileManager.Platforms.WritePlatform(
                        new CTR.Platform(idIB.Text, inputBox.Text, author.Text, pversion.Text, path, System.IO.Path.GetFileName(IconName), architecture.Text,
                        new DotnetProject(System.IO.Path.Combine(path, "dotnet/"), inputBox.Text), Env.ctrVersion), IconName);
                    Result = inputBox.Text; Close(); };

                cancelButton.Click += (sender, e) => { Result = "fuck you"; Close(); };

                inputBox.TextChanged += (sender, e) => {path = System.IO.Path.Combine(lastPath,inputBox.Text); label2.Text = path;};

                IconButton.Click += (sender, e) => {IconName = CTR.UIButtons.FileSelect("Choose the Icon for the Platform.", new List<string>
                {".png", ".jpg", ".ico"}, this);};

                var Boxlayout = new StackLayout
                {
                    Orientation = Orientation.Vertical,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 10,
                    Items =
                    {
                        label3, idIB,
                        label,  inputBox,
                        label5, author,
                        label6, pversion,
                        label4, architecture,
                        IconButton
                    },
                    Width = x,
                    Height = 400
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
    public class PlatformManagerWindow : Form
    {
        private StackLayout buttonPanel;
        private StackLayout rightPanel;
        protected Form windoh;
        public ListBox itemList;
        private List<Platform> lastPlatforms = new List<Platform>();

        public void RefreshProjects()
        {
            itemList.Items.Clear();
            lastPlatforms.Clear();
            foreach (CTR.Platform p in CTR.FileManager.Platforms.GetPlatforms())
            {
                itemList.Items.Add(new ListItem { Text = p.name + " V"+p.version, Key =  System.IO.Path.Combine(p.installPath, p.iconName)});
                lastPlatforms.Add(p);
            }

            itemList.ItemImageBinding = CreateImageBinding();
        }

        public IIndirectBinding<Image> CreateImageBinding(int x = 32, int y = 32)
        {
            return Binding.Delegate<ListItem, Image>(item =>
            {
                try
                {
                    var path = item?.Key;
                    if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                    {
                        using (var original = new Bitmap(path))
                        {
                            // Resize to 32x32 in RAM
                            var resized = new Bitmap(original, x, y);
                            return resized;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                }

                return null;
            });
        }

        public PlatformManagerWindow()
        {
            Title = "Platform Manager";
            ClientSize = new Size(800, 600);
            Resizable = false;
            windoh = this;

            // Create the ItemList (ListBox)
            itemList = new ListBox
            {
                Width = 350,
                Height = 600
            };

            RefreshProjects();

            this.Icon = new Icon("gfx/Icon.ico");

            rightPanel = new StackLayout
            {
                Width = 450,
                Height = 600,
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 10
            };

            buttonPanel = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Visible = false // Hidden by default
            };

            // Create the menu bar
            var menu = new MenuBar();
            var platformMenu = new ButtonMenuItem { Text = "Platforms" };
            platformMenu.Click += (sender, e) => ShowPlatformButtons(new List<string> { "Create Platform","Add Platform" }, this);
            itemList.SelectedIndexChanged += (sender, e) => {OnPlatformSelect(itemList.SelectedIndex);};

            menu.Items.Add(platformMenu);
            Menu = menu;

            // Layout with vertical stacking (Menu + Buttons + Main Content)
            var layout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items =
                {
                    buttonPanel, // Where the buttons will appear
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Items =
                        {
                            itemList,
                            rightPanel
                        }
                    }
                }
            };

            Content = layout;
        }

        bool showingPButtons = false;

        private void Actions(string name, Panel callFrom)
        {
            if (name == "Create Platform")
            {
                var dialog = new NewPlatWindow("Create New Platform", "Platform Name");
                dialog.ShowModal();
            }

            if (name == "Add Platform")
            {
                string plat = CTR.UIButtons.FileSelect("Select Platform", new List<string>() {".ctrplat"} ,callFrom);
                if (plat == null) return;
                CTR.FileManager.Paths.AddProject(plat);
            }
        }

        private void OnPlatformSelect(int platformIndex)
        {
            if (platformIndex < 0 || platformIndex >= lastPlatforms.Count)
            {
                Console.WriteLine("Invalid platform index selected.");
                return;
            }

            rightPanel.Items.Clear();
            rightPanel.Items.Add(new Label {Text = lastPlatforms[platformIndex].name});

            var buildPlatButton = new Button
            {
                Text = "Build Platform",
                Width = 200,
                Height = 30
            };

            buildPlatButton.Click += (sender, e) => {OnBuildPlatform(platformIndex);};
            rightPanel.Items.Add(buildPlatButton);

            if (System.IO.File.Exists(System.IO.Path.Combine(lastPlatforms[platformIndex].installPath, "MainAssembly.dll")))
            {
               var getPlatInfoButton = new Button
                {
                    Text = "Get Platform Info",
                    Width = 200,
                    Height = 30
                };

                getPlatInfoButton.Click += (sender, e) => {GetPlatformInfo(platformIndex);};
                rightPanel.Items.Add(getPlatInfoButton); 
            }
        }

        private void OnBuildPlatform(int ind)
        {
            string asmblyPath = lastPlatforms[ind].DN.Build();
            System.IO.File.Copy(asmblyPath, System.IO.Path.Combine(lastPlatforms[ind].installPath, "MainAssembly.dll"), true);
            OnPlatformSelect(ind);
        }

        private void GetPlatformInfo(int ind)
        {
            string assemblyPath = System.IO.Path.Combine(lastPlatforms[ind].installPath, "MainAssembly.dll");
            rightPanel.Items.Add(new Label {Text = "Name: "+
            ((string)PlatformDLLLoader.GetValueFromDll(assemblyPath, "name"))});
            rightPanel.Items.Add(new Label {Text = "Info: "+
            ((string)PlatformDLLLoader.GetValueFromDll(assemblyPath, "info"))});
        }

        private void ShowPlatformButtons(List<string> platformNames, Panel callFrom)
        {
            showingPButtons = !showingPButtons;
            if (showingPButtons)
            {
            buttonPanel.Items.Clear(); // Clear previous buttons

            foreach (var name in platformNames)
            {
                var button = new Button
                {
                    Text = name,
                    Width = 200,
                    Height = 30
                };

                button.Click += (sender, e) => {Actions(name, callFrom);};
                buttonPanel.Items.Add(button);
            }

            buttonPanel.Visible = true; // Make the button panel visible
            }
            else
            {
                buttonPanel.Items.Clear();
                windoh.ClientSize = new Size(800,600);
            }
        }
    }
    public static class PlatformManager
    {
        public static List<Platform> availablePlatforms;
        public static PlatformManagerWindow PMW;

        public static void InitPlatforms()
        {
            CTR.FileManager.Paths.MakePlatformsList();
            availablePlatforms = CTR.FileManager.Platforms.GetPlatforms();
        }

        public static class Events
        {
            public static void Update()
            {
                if (PMW != null)
                {
                    PMW.RefreshProjects();
                }
            }
        }
    }
}