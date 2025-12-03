using System;
using Eto.Forms;
using Eto.Drawing;
using CTR;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace CTR
{
    public class CTRIFChar
    {
        public char c;
        public string filePath;
        public int Width;
        public int Height;

        public CTRIFChar(char c, string filePath, int w = 0, int h = 0)
        {
            this.c = c;
            this.filePath = filePath;
            Width = w;
            Height = h;
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

enum CreateableAT
{
    ImageFont,
    GameData,
    BuildDefinitions
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

        fileMenu.Click += (sender, e) => { ShowButtons(new List<string> { "Open File", "New Asset..." }, this); };

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

                button.Click += (sender, e) => { Actions(name, callFrom); };
                buttonPanel.Items.Add(button);
            }

            buttonPanel.Visible = true; // Make the button panel Visible
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
            else if (Path.GetExtension(SF) == ".ctrgd")
            {
                CTRGDWindow ctrWindow = new CTRGDWindow(y);
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
            else if (Path.GetExtension(SF) == ".ctrbd")
            {
                CTRBDWindow ctrWindow = new CTRBDWindow(y);
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
            List<string> ns = Enum.GetNames(typeof(CreateableAT)).ToList();
            DropdownDialog D = new DropdownDialog("New Asset...", ns);
            D.ShowModal(this);

            if (D.yes)
            {
                CreateableAT AP = (CreateableAT)D.selectedIndex;
                if (AP == CreateableAT.ImageFont)
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
                else if (AP == CreateableAT.GameData)
                {
                    CTRGDWindow ctrWindow = new CTRGDWindow(y);
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
                else if (AP == CreateableAT.BuildDefinitions)
                {
                    CTRBDWindow ctrWindow = new CTRBDWindow(y);
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

public class CTRIFWindow
{
    private int y;
    private StackLayout layout;

    public CTRIFWindow(int availableHeight)
    {
        y = availableHeight;
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
        var AutoButton = new Button { Text = "Auto Generate Image Font..." };
        var imageDisp = new ImageView { Width = 350, Height = 350 };

        CTRIF currentIF = new CTRIF();
        int currentCharacter = -1;

        SaveButton.Click += (sender, e) =>
        {

            string path = GetSaveFilePath(
            "Save CTR Image Font",
            new[] { "CTRIF File|ctrif" }
            );

            if (path != null)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(currentIF));
            }
        };

        AutoButton.Click += (sender, e) =>
        {
            string path = CTR.UIButtons.FolderSelect("Select Folder with Letters", null);
            if (path != null)
            {
                foreach (string f in Directory.GetFiles(path))
                {
                    if (f.EndsWith(".png"))
                    {
                        Bitmap b = new Bitmap(f);
                        string woE = Path.GetFileNameWithoutExtension(f);
                        char name = woE[woE.Length-1];
                        currentIF.Chars.Add(new CTRIFChar(name, f, b.Width, b.Height));
                        b.Dispose();
                    }
                }
                RefreshList();
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

        void RefreshList()
        {
            charList.Items.Clear();
            charList.Items.Add(new ListItem { Text = "Default Char", Key = currentIF.defaultChar.filePath });
            charList.ItemImageBinding = CreateImageBinding();
            foreach (var ch in currentIF.Chars)
                charList.Items.Add(new ListItem { Text = $"Char: {ch.c}  ({System.IO.Path.GetFileName(ch.filePath)})", Key = ch.filePath });
        }

        void UpdateDetails(int index)
        {
            if (index == 0)
            {
                var dc = currentIF.defaultChar;
                UpdDetails(dc);
            }
            else if (index > 0 && index - 1 < currentIF.Chars.Count) // Character selected
            {
                var selectedChar = currentIF.Chars[index - 1];
                UpdDetails(selectedChar);
            }
            else
            {
                charName.Text = "Character: None";
                charBox.Text = "";
                imagePath.Text = "Image Path: None";
                UpdDetails(null);
            }
        }

        void UpdDetails(CTRIFChar dc)
        {
            if (dc != null)
            {
                charName.Text = "Character: Default";
                charBox.Text = dc.c.ToString();
                charWidth.Text = dc.Width.ToString();
                charHeight.Text = dc.Height.ToString();
                if (File.Exists(dc.filePath))
                {
                    imageDisp.Image = new Bitmap(dc.filePath);
                    imageDisp.Visible = true;
                }
                else
                {
                    imageDisp.Visible = false;
                }
                imagePath.Text = $"Image Path: {dc.filePath}";
            }
            else
            {
                charName.Text = "Character: None";
                charBox.Text = "";
                imagePath.Text = "Image Path: None";
                imageDisp.Visible = false;
            }
        }

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
                if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count)
                {
                    ch = currentIF.Chars[currentCharacter - 1]; charList.Items[currentCharacter].Text = $"Char: {ch.c}  ({System.IO.Path.GetFileName(ch.filePath)})";
                }
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

        charList.SelectedIndexChanged += (sender, e) =>
        {
            currentCharacter = charList.SelectedIndex;
            UpdateDetails(currentCharacter);
        };

        selectIPButton.Click += (sender, e) =>
        {
            string path = selectFileCallback();
            if (path != null)
            {
                if (currentCharacter == 0)
                    currentIF.defaultChar.filePath = path;
                else if (currentCharacter > 0 && currentCharacter - 1 < currentIF.Chars.Count)
                    currentIF.Chars[currentCharacter - 1].filePath = path;

                imagePath.Text = $"Image Path: {path}";
                RefreshList();
            }
        };

        NewButton.Click += (sender, e) =>
        {
            currentIF.Chars.Add(new CTRIFChar('0', "No Path Yet"));
            RefreshList();
        };

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
                            Items = { charName, charBox,charWL,charWidth,charHL,charHeight,imageDisp, imagePath, selectIPButton, SaveButton, AutoButton }
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

public class CTRGD
{
    public string Name;
    public string Author;
    public string Description;
    public string Version;
    public string IconPath;
}

public class CTRGDWindow
{
    private int y;
    private StackLayout layout;

    public CTRGDWindow(int availableHeight)
    {
        y = availableHeight;
    }

    public StackLayout CreateContent(Func<string> selectFileCallback, string filePath = null)
    {
        var currentGD = new CTRGD();
        var SaveButton = new Button { Text = "Save CTR Game Data as File..." };
        var l1 = new Label { Text = "Name" };
        var l5 = new Label { Text = "Author" };
        var l2 = new Label { Text = "Description" };
        var l3 = new Label { Text = "Version" };
        var l4 = new Label { Text = "Icon" };
        var t1 = new TextBox { Width = 450 };
        var t2 = new TextArea { Height = 500, Width = 450, ReadOnly = false, Wrap = true };
        var t3 = new TextBox { Width = 450, Text = "0.0.1" };
        var t4 = new TextBox { Width = 450, Text = "Assets/Icon.png" };
        var t5 = new TextBox { Width = 450, Text = "" };
        SaveButton.Click += (sender, e) =>
        {
            currentGD.Name = t1.Text;
            currentGD.Author = t5.Text;
            currentGD.Description = t2.Text;
            currentGD.Version = t3.Text;
            currentGD.IconPath = t4.Text;
            string path = GetSaveFilePath(
            "Save CTR Game Data",
            new[] { "CTRGD File|ctrgd" }
            );

            if (path != null)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(currentGD));
            }
        };

        

        void RefreshAll()
        {
            t1.Text = currentGD.Name;
            t5.Text = currentGD.Author;
            t2.Text = currentGD.Description;
            t3.Text = currentGD.Version;
            t4.Text = currentGD.IconPath;
        }

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

        StackLayout QSL(Control l, Control r)
        {
            return new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items = {
                    l,r
                }
            };
        }

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
                            Items = { QSL(l1, t1), QSL(l5,t5), QSL(l2,t2),QSL(l3,t3),QSL(l4,t4),
                                new Label {Text = "Remember, CTRGD's should be named GameData.ctrgd and be in Project/Assets/"},
                                SaveButton }
                        }
                    }
                }
            }
        };

        if (filePath != null)
        {
            currentGD = JsonConvert.DeserializeObject<CTRGD>(File.ReadAllText(filePath));
            RefreshAll();
        }

        return layout;
    }
}

public class CTRBD // Build Definitions
{
    const string commentBase = "// Definitions for ";
    const string defBase = "#define ";

    public List<CTRBDSet> Definitions = new List<CTRBDSet>(); // 0 is Global, after that is platforms.
    public string GenerateHeader(string platId)
    {
        int length = calcLength(platId);
        StringBuilder sb = new StringBuilder(length);
        List<string> nameList = new List<string>();
        nameList.Add("Global");
        foreach (CTR.Platform p in CTR.FileManager.Platforms.GetPlatforms())
        {
            nameList.Add(p.name);
        }
        int i = 0;
        foreach (CTRBDSet s in Definitions)
        {
            sb.Append(commentBase + nameList[i] + "\n");
            foreach (CTRBDMember mem in s.toDefine)
            {
                if (mem.enabled ?? false)
                {
                    sb.Append(defBase + mem.defname + " " + mem.value + "\n");
                }
            }
            i++;
        }
        return sb.ToString();
    }

    int calcLength(string platId)
    {
        int cur = 0;
        List<string> nameList = new List<string>();
        nameList.Add("Global");
        foreach (CTR.Platform p in CTR.FileManager.Platforms.GetPlatforms())
        {
            nameList.Add(p.name);
        }
        int i = 0;
        foreach (CTRBDSet s in Definitions)
        {
            if (s.platformId != "Global" && s.platformId != platId) continue;
            cur += commentBase.Length + nameList[i].Length + 1; // +1 for newline
            foreach (CTRBDMember mem in s.toDefine)
            {
                if (mem.enabled ?? false)
                {
                    cur += mem.defname.Length + mem.value.Length + defBase.Length + 2; // +1 for newline, +1 for space between value
                }
            }
            i++;
        }
        return cur;
    }
}

public class CTRBDSet
{
    public string platformId = "";
    public List<CTRBDMember> toDefine = new List<CTRBDMember>();
}

public class CTRBDMember
{
    public string defname { get; set; } = "Name";
    public string value { get; set; } = "0";
    public bool? enabled { get; set; } = true;
}

public class CTRBDWindow
{
    private int y;
    private StackLayout layout;

    public CTRBDWindow(int availableHeight)
    {
        y = availableHeight;
    }

    public StackLayout CreateContent(Func<string> selectFileCallback, string filePath = null)
    {
        var currentBD = new CTRBD();
        var SaveButton = new Button { Text = "Save CTR Build Definitions as File..." };
        var AddButton = new Button { Text = "+" };
        List<string> nameList = new List<string>();
        nameList.Add("Global");
        foreach (CTR.Platform p in CTR.FileManager.Platforms.GetPlatforms())
        {
            nameList.Add(p.name);
        }
        var typeDropDown = new DropDown { DataStore = nameList, SelectedIndex = 0, Width = 500 };
        var defList = new GridView();
        defList.Height = y - 400;
        defList.ShowHeader = false;
        defList.AllowMultipleSelection = true;

        defList.Columns.Add(new GridColumn
        {
            DataCell = new TextBoxCell { Binding = Binding.Property<CTRBDMember, string>(r => r.defname) },
            Editable = true,
            Width = 300
        });

        defList.Columns.Add(new GridColumn
        {
            DataCell = new TextBoxCell { Binding = Binding.Delegate<CTRBDMember, string>(r => "=") },
            Editable = false,
            Width = 30
        });

        defList.Columns.Add(new GridColumn
        {
            DataCell = new TextBoxCell { Binding = Binding.Property<CTRBDMember, string>(r => r.value) },
            Editable = true,
            Width = 150
        });

        defList.Columns.Add(new GridColumn
        {
            DataCell = new CheckBoxCell { Binding = Binding.Property<CTRBDMember, bool?>(r => r.enabled) },
            Editable = true,
            Width = 50
        });

        Button removeButton = new Button { Text = "Remove Selected" };

        removeButton.Click += (sender, e) =>
        {
            if (defList.SelectedItems.Count() < 0) return;
            foreach (var mem in defList.SelectedItems.Cast<CTRBDMember>())
            {
                currentBD.Definitions[typeDropDown.SelectedIndex].toDefine.Remove(mem);
            }
            RefreshAll();
        };
        
        SaveButton.Click += (sender, e) =>
        {
            string path = GetSaveFilePath(
            "Save CTR Build Definitions",
            new[] { "CTRBD File|ctrbd" }
            );

            if (path != null)
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(currentBD));
            }
        };

        int lastIndex = typeDropDown.SelectedIndex;

        string FromNameList(int index)
        {
            return nameList[index];
        }

        typeDropDown.SelectedIndexChanged += (sender, e) =>
        {
            idkwhyimdoingthislikethis:
            if (currentBD.Definitions.Count <= typeDropDown.SelectedIndex)
            {
                CTRBDSet set = new CTRBDSet();
                set.platformId = FromNameList(currentBD.Definitions.Count);
                currentBD.Definitions.Add(set); // Make new
                if (currentBD.Definitions.Count <= typeDropDown.SelectedIndex) goto idkwhyimdoingthislikethis;
            }
            currentBD.Definitions[lastIndex].toDefine = defList.DataStore.Cast<CTRBDMember>().ToList();
            lastIndex = typeDropDown.SelectedIndex;
            RefreshAll();
        };

        AddButton.Click += (sender, e) =>
        {
            currentBD.Definitions[typeDropDown.SelectedIndex].toDefine.Add(new CTRBDMember());
            RefreshAll();
        };

        void RefreshAll()
        {
            List<CTRBDMember> bdmembers = new List<CTRBDMember>();
            idkwhyimdoingthislikethis:
            if (currentBD.Definitions.Count <= typeDropDown.SelectedIndex)
            {
                CTRBDSet set = new CTRBDSet();
                set.platformId = FromNameList(currentBD.Definitions.Count);
                currentBD.Definitions.Add(set); // Make new
                if (currentBD.Definitions.Count <= typeDropDown.SelectedIndex) goto idkwhyimdoingthislikethis;
            }
            foreach (CTRBDMember bdmem in currentBD.Definitions[typeDropDown.SelectedIndex].toDefine)
            {
                bdmembers.Add(bdmem);
            }
            defList.DataStore = new BindingList<CTRBDMember>(bdmembers);
        }

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
                            Items = { new StackLayout {Orientation = Orientation.Horizontal, Items = {typeDropDown, AddButton}},
                                removeButton,
                                defList,
                                new Label {Text = "Remember, CTRBD's should be named BuildDefs.ctrbd and be in Project/Assets/"},
                                SaveButton
                            }
                        }
                    }
                }
            }
        };

        if (filePath != null)
        {
            currentBD = JsonConvert.DeserializeObject<CTRBD>(File.ReadAllText(filePath));
        }
        RefreshAll();

        return layout;
    }
}