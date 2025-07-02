using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;

namespace CTR.Window
{
    public static class WManager
    {
        public static void Start()
        {
            new Application(Eto.Platform.Detect).Run(new MWindow());
        }

        public static ListBox ProjectsHolder;
        public static bool ProjectSelected = false;
        public static int selectedProjectIndex = -1;
    }

    public static class WindowTools
    {
        public static void AddItemWithImage(StackLayout itemList, string text, string imagePath, int fontSize = 14, FontDecoration fs = FontDecoration.None, bool isButton = false, int buttonID = -1, Panel callFrom = null)
        {
            var image = new Bitmap(imagePath);  // Load the image from the path

            // Create an ImageView to show the image
            var imageView = new ImageView { Image = image, Width = 24, Height = 24 };

            // Create a Label for text with customized font properties
            var textLabel = new Label
            {
                Text = text,
                Font = new Font(SystemFont.Default, fontSize, fs), // Customize font size, style, etc.
                VerticalAlignment = VerticalAlignment.Center,
                TextColor = Colors.White
            };

            // Combine image and text using a horizontal layout
            var itemLayout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Items = { imageView, textLabel }
            };

            // Render the layout to a drawable control
            var drawableItem = new Panel { Content = itemLayout };

            // Add the drawable control to the ListBox
            itemList.Items.Add(itemLayout); // Simple ListBox doesn't support custom content rendering
        }
    }

    class MWindow : Form
    {
        StackLayout buttonPanel; // Panel to show buttons dynamically
        ListBox listBox;

        private void ShowLabel(string buttonLabels, int FontSize = 20)
        {
            var button = new Label { Text = buttonLabels, Width = 240, Height = 100, Font = new Font(SystemFont.Default, FontSize) };

            buttonPanel.Items.Add(button);

            buttonPanel.Visible = true; // Show buttons when an option is selected
        }

        private void ShowButton(string buttonName, Panel callFrom)
        {
            var button = new Button { Text = buttonName, Width = 240, Height = 100 };

            button.Click += (sender, e) => ButtonClicked(buttonName, callFrom);

            buttonPanel.Items.Add(button);

            buttonPanel.Visible = true;
        }

        private void ClearList()
        {
            buttonPanel.Items.Clear();
        }

        public MWindow()
        {
            Title = "Cartridge Tilt Retro";
            var screen = Screen.PrimaryScreen;
            var workingArea = screen.WorkingArea;
            ClientSize = new Size(750+480, 420);
            Resizable = false;

            // ListBox for the left side
            listBox = new ListBox
            {
                Width = 750, // 75% of the width
                Height = 400
            };

            WManager.ProjectsHolder = listBox;

            foreach (var project in CTR.FileManager.Projects.GetProjects())
            {
                listBox.Items.Add(project.name);
            }

            buttonPanel = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 20,
                Visible = false
            };

            var rightPanel = new Panel
            {
                Size = new Size(480, 400),
                Content = new StackLayout
                {
                    Orientation = Orientation.Vertical,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Items = { buttonPanel }
                }
            };

            var layout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items =
                {
                    listBox,
                    rightPanel
                }
            };

            var menu = new MenuBar();
            var fileMenu = new ButtonMenuItem { Text = "File" };
            var testMenu = new ButtonMenuItem { Text = "Edit" };

            fileMenu.Click += (sender, e) => {ShowButtons(new List<string> { "New Project", "Add Project" }, this);};
            testMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Preferences", "Platform Manager" }, this);};

            menu.Items.Add(fileMenu);
            menu.Items.Add(testMenu);

            listBox.MouseDown += (sender, e) => {
                OnprojectSelected();
            };

            listBox.SelectedIndexChanged += (sender, e) => {
                OnprojectSelected();
            };

            this.Icon = new Icon("gfx/Icon.ico");

            Menu = menu;
            Content = layout;
        }

        void OnprojectSelected()
        {
            if (listBox.SelectedIndex < 0 || listBox.SelectedIndex >= FileManager.Projects.GetProjects().Count)
                return;
            
            WManager.ProjectSelected = true; WManager.selectedProjectIndex = listBox.SelectedIndex; 
            ClearList();
            ShowLabel($"\"{FileManager.Projects.GetProjects()[listBox.SelectedIndex].name}\"", 30);
            ShowLabel($"CTR Version {FileManager.Projects.GetProjects()[listBox.SelectedIndex].ctrVersion}", 20);
            var button = new Button { Text = "Open Editor", Width = 240, Height = 100 };

            button.Click += (sender, e) => CTR.Projects.Handler.OpenProjectEditor(
            CTR.FileManager.Projects.GetProjects()[listBox.SelectedIndex],
            CTR.FileManager.Paths.GetProjectFilePaths()[listBox.SelectedIndex]);

            buttonPanel.Items.Add(button);

            Button removeButton = new Button { Text = "Remove -" };
            removeButton.Click += (sender, e) =>
            {
                var projects = CTR.FileManager.Projects.GetProjects();
                Console.WriteLine(projects.Count);
                int selectedIndex = listBox.SelectedIndex;

                if (selectedIndex >= 0 && selectedIndex < projects.Count)
                {
                    Console.WriteLine(projects[selectedIndex].ctrProjPath);
                    CTR.FileManager.Projects.RemoveProject(projects[selectedIndex].ctrProjPath);
                    Console.WriteLine("Removing Project!");
                }

                listBox.Items.Clear();
                projects = CTR.FileManager.Projects.GetProjects();

                foreach (var project in projects)
                {
                    listBox.Items.Add(project.name);
                }

                if (listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }
            };
            buttonPanel.Items.Add(removeButton);

            buttonPanel.Visible = true;
        }

        private void ShowButtons(List<string> buttonLabels, Panel callFrom)
        {
            buttonPanel.Items.Clear();
            
            foreach (var label in buttonLabels)
            {
                var button = new Button { Text = label, Width = 240, Height = 100 };

                button.Click += (sender, e) => ButtonClicked(label, callFrom);

                buttonPanel.Items.Add(button);
            }

            buttonPanel.Visible = true;
        }

        private void ButtonClicked(string buttonText, Panel callFrom)
        {
            CTR.UIButtons.OnButtonPress(buttonText, callFrom);
        }
    }
}
