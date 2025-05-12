using System;
using Eto.Forms;
using Eto.Drawing;
using CTR;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace CTR
{
    public class CTRIFChar
    {
        public char c;
        public string filePath;
        public int Width;
        public int Height;
        
        public CTRIFChar(char c, string filePath)
        {
            this.c = c;
            this.filePath = filePath;
        }
    }
    public class CTRIF
    {
        public List<CTRIFChar> Chars;
        public CTRIFChar defaultChar;

        public CTRIF()
        {
            Chars = new List<CTRIFChar>();
        }
    }
}

public class AssetMaker : Form
{
    private StackLayout buttonPanel;
    public int x;
    public int y;

    public AssetMaker()
    {
        Title = "Asset Maker";
        var screen = Screen.PrimaryScreen;
        var workingArea = screen.WorkingArea;
        ClientSize = new Size((int)workingArea.Width, (int)workingArea.Height);
        Resizable = false;
        this.Icon = new Icon("gfx/Icon.ico");
        x = (int)workingArea.Width;
        y = (int)workingArea.Height;
        Padding = 10;

        buttonPanel = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Visible = false // Hidden by default
        };

        var layout = new StackLayout
        {
            Items =
            {
                buttonPanel
            },
            Orientation = Orientation.Vertical,
            Spacing = 10
        };

        var menu = new MenuBar();

        var fileMenu = new ButtonMenuItem { Text = "File" };

        fileMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Open File", "New Asset..." }, this);};

        menu.Items.Add(fileMenu);

        Menu = menu;
        Content = layout;
    }

    bool showingPButtons = false;

    private void ShowButtons(List<string> platformNames, Panel callFrom)
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
        }
    }

    void Actions(string name, Panel callFrom)
    {
        if (name == "Open File")
        {
            string SF = SelectFile();
            if (Path.GetExtension(SF) == ".ctrif")
            {
                CTRIFWindow ctrWindow = new CTRIFWindow(y);
                var content = ctrWindow.CreateContent(SelectFile, SF);

                // Insert the content below the button panel
                if (Content is StackLayout layout)
                {
                    if (layout.Items.Count == 1)
                        layout.Items.Add(content); // Adds after button panel
                    else
                        layout.Items[1] = content; // Replace old content
                }
            }
            // Open File
        }
        else if (name == "New Asset...")
        {
            List<string> ns = Enum.GetNames(typeof(AssetType)).ToList();
            ns.Add("VSE File");
            DropdownDialog D = new DropdownDialog("New Asset...", ns);
            D.ShowModal(this);

            if (D.yes && D.selectedIndex+1 < ns.Count-1)
            {
                AssetType AP = (AssetType)D.selectedIndex;
                if (AP == AssetType.ImageFont)
                {
                    CTRIFWindow ctrWindow = new CTRIFWindow(y);
                    var content = ctrWindow.CreateContent(SelectFile);

                    // Insert the content below the button panel
                    if (Content is StackLayout layout)
                    {
                        if (layout.Items.Count == 1)
                            layout.Items.Add(content); // Adds after button panel
                        else
                            layout.Items[1] = content; // Replace old content
                    }
                }
            }
            else if (D.yes && D.selectedIndex+1 == ns.Count)
            {
                CTRVSEWindow ctrWindow = new CTRVSEWindow(y);

                var content = ctrWindow.CreateContent(SelectFile);

                    if (Content is StackLayout layout)
                    {
                        if (layout.Items.Count == 1)
                            layout.Items.Add(content);
                        else
                            layout.Items[1] = content;
                    }
            }
        }
    }
    public string SelectFile()
    {
        string originalDir = Directory.GetCurrentDirectory();
        var dialog = new OpenFileDialog
        {
            Title = "Select a file"
        };
        dialog.Filters.Add(new FileFilter("All Files", "*"));

        var result = dialog.ShowDialog(this);
        Directory.SetCurrentDirectory(originalDir);
        if (result == DialogResult.Ok)
        {
            return dialog.FileName;
        }

        return null;
    }
}

public class CTRVSEWindow
{
    private int y;
    private StackLayout layout;

    public CTRVSEWindow(int availableHeight)
    {
        y = availableHeight;
    }

    public StackLayout CreateContent(Func<string> selectFileCallback)
    {
        //VSEFile currentVSE;
        var button1 = new Button
        {
            Text = "Select Header Host Folder"
        };

        var button2 = new Button
        {
            Text = "Export VSE File"
        };

        var stack = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Height = y - 225, // Assuming 'y' is your screen height or container height
            Items =
            {
                button1,
                button2
            }
        };

        List<string> GetHeaderFiles(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Directory not found: {directory}");

            var files = Directory.GetFiles(directory, "*.h", SearchOption.AllDirectories);
            return new List<string>(files);
        }

        // Optional: Wire up click handlers
        button1.Click += (s, e) =>
        {
            string result = selectFileCallback?.Invoke();
            if (result == null) return;
            Console.WriteLine("Selected: " + result);
            foreach (string hfile in GetHeaderFiles(result))
            {
                
            }
        };

        button2.Click += (s, e) =>
        {
            /*string path = CTR.FileManager.GetSaveFilePath(
            "Save Visual Scripting Editor File",
            new[] { "VSE File|vse" }
            );

            if (path != null)
            {
                //File.WriteAllText(path, JsonConvert.SerializeObject(currentVSE));
            }*/
        };

        return stack;
    }
}

public class CTRIFWindow
{
    private int y;
    private StackLayout layout;

    public CTRIFWindow(int availableHeight)
    {
        y = availableHeight;
    }

    public StackLayout CreateContent(Func<string> selectFileCallback, string filePath = null)
    {
        var charList = new ListBox { Width = 300, Height = y - 225 };
        var charName = new Label { Text = "Character: None" };
        var charBox = new TextBox();
        var charWL = new Label { Text = "Char Width" };
        var charWidth = new TextBox();
        var charHL = new Label { Text = "Char Height" };
        var charHeight = new TextBox();
        var imagePath = new Label { Text = "Image Path: None" };
        var selectIPButton = new Button { Text = "Select Image" };
        var NewButton = new Button { Text = "New Character...", Width = 300, Height = 45 };
        var RemoveButton = new Button { Text = "Remove Character" };
        var SaveButton = new Button { Text = "Save CTR Image Font as File..." };

        CTRIF currentIF = new CTRIF();
        int currentCharacter = -1;

        SaveButton.Click += (sender,e) => {

            string path = GetSaveFilePath(
            "Save CTR Image Font",
            new[] { "CTRIF File|ctrif" }
            );

            if (path != null)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(currentIF));
            }
        };

        // Default char initialization
        currentIF.defaultChar = new CTRIFChar('0', "No Path for Default Char");
        charList.Items.Add("Default Char");

        string GetSaveFilePath(string title = "Save File", string[] filters = null)
        {
            var dialog = new SaveFileDialog
            {
                Title = title
            };

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var parts = filter.Split('|');
                    if (parts.Length == 2)
                    {
                        var f = new FileDialogFilter
                        {
                            Name = parts[0],
                            Extensions = new[] { parts[1] } // This avoids calling Add
                        };
                        dialog.Filters.Add(f);
                    }
                }
            }
            else
            {
                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = "All Files",
                    Extensions = new[] { "*" }
                });
            }

            var result = dialog.ShowDialog(null);
            return result == DialogResult.Ok ? dialog.FileName : null;
        }

        // Refresh the list of characters in the charList
        void RefreshList()
        {
            charList.Items.Clear();
            charList.Items.Add("Default Char");
            foreach (var ch in currentIF.Chars)
                charList.Items.Add($"Char: {ch.c}  ({System.IO.Path.GetFileName(ch.filePath)})");
        }

        // Update the details shown for the selected character
        void UpdateDetails(int index)
        {
            if (index == 0) // Default Char selected
            {
                var dc = currentIF.defaultChar;
                charName.Text = "Character: Default";
                charBox.Text = dc.c.ToString();
                charWidth.Text = dc.Width.ToString();
                charHeight.Text = dc.Height.ToString();
                imagePath.Text = $"Image Path: {dc.filePath}";
            }
            else if (index > 0 && index - 1 < currentIF.Chars.Count) // Character selected
            {
                var selectedChar = currentIF.Chars[index - 1];
                charName.Text = $"Character: {selectedChar.c}";
                charBox.Text = selectedChar.c.ToString();
                charWidth.Text = selectedChar.Width.ToString();
                charHeight.Text = selectedChar.Height.ToString();
                imagePath.Text = $"Image Path: {selectedChar.filePath}";
            }
            else // No character selected
            {
                charName.Text = "Character: None";
                charBox.Text = "";
                imagePath.Text = "Image Path: None";
            }
        }

        // Update the character's 'c' when charBox text changes
        charBox.TextChanged += (sender, e) =>
        {
            if (charBox.Text.Length > 0)
            {
                char newChar = charBox.Text[0];
                if (currentCharacter == 0)
                    currentIF.defaultChar.c = newChar;
                else if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count)
                    currentIF.Chars[currentCharacter - 1].c = newChar;

                CTRIFChar ch = null;
                if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count) {
                    ch = currentIF.Chars[currentCharacter-1]; charList.Items[currentCharacter].Text = $"Char: {ch.c}  ({System.IO.Path.GetFileName(ch.filePath)})";}
                //UpdateDetails(currentCharacter);
                //charList.SelectedIndex = currentCharacter; // Keep the selected index
            }
        };

        charWidth.TextChanged += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(charBox.Text) && int.TryParse(charWidth.Text, out int newWidth))
            {
                if (currentCharacter == 0)
                    currentIF.defaultChar.Width = newWidth;
                else if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count)
                    currentIF.Chars[currentCharacter - 1].Width = newWidth;
            }
            else
            {
                // Optional: You can show an error message or highlight the textbox
                Console.WriteLine("Invalid input for width.");
            }
        };

        charHeight.TextChanged += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(charBox.Text) && int.TryParse(charHeight.Text, out int newHeight))
            {
                if (currentCharacter == 0)
                    currentIF.defaultChar.Height = newHeight;
                else if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count)
                    currentIF.Chars[currentCharacter - 1].Height = newHeight;
            }
            else
            {
                Console.WriteLine("Invalid input for height.");
            }
        };

        // Handle character selection from charList
        charList.SelectedIndexChanged += (sender, e) =>
        {
            currentCharacter = charList.SelectedIndex;
            UpdateDetails(currentCharacter);
        };

        // Handle selecting the image path for the selected character
        selectIPButton.Click += (sender, e) =>
        {
            string path = selectFileCallback(); // Get the path from the callback
            if (path != null)
            {
                if (currentCharacter == 0)
                    currentIF.defaultChar.filePath = path;
                else if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count)
                    currentIF.Chars[currentCharacter - 1].filePath = path;

                //UpdateDetails(currentCharacter);
                imagePath.Text = $"Image Path: {path}";
            }
        };

        // Handle adding a new character
        NewButton.Click += (sender, e) =>
        {
            currentIF.Chars.Add(new CTRIFChar('0', "No Path Yet"));
            RefreshList();
            //charList.SelectedIndex = currentIF.Chars.Count; // Select newly added character
        };

        // Handle removing a character
        RemoveButton.Click += (sender, e) =>
        {
            if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count)
            {
                currentIF.Chars.RemoveAt(currentCharacter - 1);
                RefreshList();
                currentCharacter = -1;
                charList.SelectedIndex = 0;
            }
        };

        // Layout for the window
        layout = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Items =
            {
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Items =
                    {
                        new StackLayout
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 10,
                            Items = { NewButton, RemoveButton, charList }
                        },
                        new StackLayout
                        {
                            Orientation = Orientation.Vertical,
                            Spacing = 10,
                            Items = { charName, charBox,charWL,charWidth,charHL,charHeight, imagePath, selectIPButton, SaveButton }
                        }
                    }
                }
            }
        };

        if (filePath != null)
        {
            currentIF = JsonConvert.DeserializeObject<CTRIF>(File.ReadAllText(filePath));
            RefreshList();
        }

        return layout;
    }
}
