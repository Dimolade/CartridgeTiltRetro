using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;

namespace CTR.Window
{
    public static class WManager
    {
        public static void Start()
        {
            new Application(Eto.Platforms.Gtk).Run(new MWindow());
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
            ClientSize = new Size((int)workingArea.Width, (int)workingArea.Height);
            Resizable = false;

            // ListBox for the left side
            listBox = new ListBox
            {
                Width = 1440, // 75% of the width
                Height = 1080
            };

            WManager.ProjectsHolder = listBox;

            // Dummy data for projects (Replace with actual logic)
            foreach (var project in CTR.FileManager.Projects.GetProjects())
            {
                listBox.Items.Add(project.name);
            }

            // ✅ Initialize buttonPanel before usage
            buttonPanel = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 20,
                Visible = false // Initially hidden
            };

            // Right-side panel with fixed width and height
            var rightPanel = new Panel
            {
                Size = new Size(480, 1080), // Set width and height explicitly
                Content = new StackLayout
                {
                    Orientation = Orientation.Vertical,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Items = { buttonPanel } // Now buttonPanel is initialized
                }
            };

            // Main layout with horizontal split
            var layout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items =
                {
                    listBox,  // Left side - List
                    rightPanel
                }
            };

            // Menu bar with options
            var menu = new MenuBar();
            var fileMenu = new ButtonMenuItem { Text = "File" };
            var testMenu = new ButtonMenuItem { Text = "Edit" };
            var cmsMenu = new ButtonMenuItem { Text = "CMS" };

            fileMenu.Click += (sender, e) => {ShowButtons(new List<string> { "New Project", "Add Project" }, this);};
            testMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Preferences", "Platform Manager" }, this);};
            cmsMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Open CMS IDE" }, this);};

            menu.Items.Add(fileMenu);
            menu.Items.Add(testMenu);
            menu.Items.Add(cmsMenu);

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
            WManager.ProjectSelected = true; WManager.selectedProjectIndex = listBox.SelectedIndex; 
            ClearList();
            ShowLabel($"\"{FileManager.Projects.GetProjects()[listBox.SelectedIndex].name}\"", 30);
            ShowLabel($"CTR Version {FileManager.Projects.GetProjects()[listBox.SelectedIndex].ctrVersion}", 20);
            var button = new Button { Text = "Open Editor", Width = 240, Height = 100 };

            button.Click += (sender, e) => CTR.Projects.Handler.OpenProjectEditor(CTR.FileManager.Projects.GetProjects()[listBox.SelectedIndex]);

            buttonPanel.Items.Add(button);

            buttonPanel.Visible = true;
        }

        private void ShowButtons(List<string> buttonLabels, Panel callFrom)
        {
            buttonPanel.Items.Clear(); // Clear previous buttons
            
            foreach (var label in buttonLabels)
            {
                var button = new Button { Text = label, Width = 240, Height = 100 };

                // Attach an event to handle the button click
                button.Click += (sender, e) => ButtonClicked(label, callFrom);

                buttonPanel.Items.Add(button);
            }

            buttonPanel.Visible = true; // Show buttons when an option is selected
        }

        private void ButtonClicked(string buttonText, Panel callFrom)
        {
            CTR.UIButtons.OnButtonPress(buttonText, callFrom);
        }
    }
}
