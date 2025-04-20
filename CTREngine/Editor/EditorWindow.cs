using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using CTR.Projects;
using System.IO;
using CTR;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace CTR
{
    public enum AssetType
    {
        Image,
        Script,
        Font,
        Sound,
        Other,
        ImageFont
    }
    public enum AssetSortType
    {
        Name,
        AssetType,
        Both
    }
    public class ObjectPropertie
    {
        public object defaultValue;
        public object currentValue;
        public string name;
        public bool isPublic;
        public string typeName;
        [JsonIgnore]
        public Func<object, object> onUpdate;

        public bool Equals(ObjectPropertie OP)
        {
            return (OP.defaultValue == defaultValue && OP.currentValue == currentValue && name == OP.name && isPublic == OP.isPublic);
        }
        
        public ObjectPropertie(object defaultValue, string name, bool isPublic, Func<object, object> onUpdate)
        {
            this.defaultValue = defaultValue;
            this.name = name;
            this.isPublic = isPublic;
            this.currentValue = this.defaultValue;
            this.onUpdate = onUpdate;

            typeName = defaultValue.GetType().Name;
        }
    }
    public class Vector3
    {
        public double x;
        public double y;
        public double z;
        
        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    public class Color
    {
        public int r;
        public int g;
        public int b;
        public int a;
        public int blend;
        
        public Color(int x, int y, int z, int w, int v)
        {
            r = x;
            g = y;
            b = z;
            a = w;
            blend = v;
        }
    }
    public class Vector4
    {
        public double x;
        public double y;
        public double z;
        public double w;
        
        public Vector4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }
    public class Vector2
    {
        public double x;
        public double y;
        
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public class ObjectProperties
    {
        public List<ObjectPropertie> Props;
        [JsonIgnore]
        public bool initted;

        public bool created = false;

        public void TryAddOP(ObjectPropertie OP)
        {
            int pIndex = Props.FindIndex(p => p.name == OP.name);

            if (pIndex == -1)
            {
                Props.Add(OP);
            }
            else
            {
                Props[pIndex].onUpdate = OP.onUpdate;
            }
        }

        public void InitProps(AssetType at,SceneObject Owner)
        {
            initted = true;
            Console.WriteLine("Props Length before initting: "+Props.Count);
            //Props = new List<ObjectPropertie>();
            /* if (created)
                return;*/

            created = true;

            foreach (var prop in Props)
            {
                Console.WriteLine($"Name: {prop.name}, Type: {prop.defaultValue?.GetType().Name}, Default Value: {prop.defaultValue}");
            }

            foreach (var prop in Props)
            {
                if (prop.currentValue is JObject jObject)
                {
                    if (prop.typeName == "Vector3")
                    {
                        prop.currentValue = jObject.ToObject<Vector3>();
                        prop.defaultValue = jObject.ToObject<Vector3>();
                    }
                    else if (prop.typeName == "Vector2")
                    {
                        prop.currentValue = jObject.ToObject<Vector2>();
                        prop.defaultValue = jObject.ToObject<Vector2>();
                    }
                    else if (prop.typeName == "Color")
                    {
                        prop.currentValue = jObject.ToObject<Color>();
                        prop.defaultValue = jObject.ToObject<Color>();
                    }
                }
            }

            TryAddOP(new ObjectPropertie(Owner.name, "Name", true, (input) => {Owner.name = (string)input; return 1;}));
            TryAddOP(new ObjectPropertie(Owner.Namespace, "Namespace", true, (input) => {Owner.Namespace = (string)input; return 1;}));
            TryAddOP(new ObjectPropertie(0, "Screen", true, (input) => {return 1;}));
            TryAddOP(new ObjectPropertie(true, "Instant Load", true, (input) => {return 1;}));
            if (at == AssetType.Image)
            {
                ObjectPropertie LoadOnShow = new ObjectPropertie(true, "Load on Show", true, (input) => {return 1;});
                AddTransformProperties();
                TryAddOP(LoadOnShow);
                TryAddOP(new ObjectPropertie(true, "Display", true, (input) => {return 1;}));
                TryAddOP(new ObjectPropertie(new Color(255,255,255,255,0), "Color", true, (input) => {return 1;}));
            }
            if (at == AssetType.Font || at == AssetType.ImageFont)
            {
                ObjectPropertie LoadOnShow = new ObjectPropertie(true, "Load on Show", true, (input) => {return 1;});
                AddTransformProperties();
                TryAddOP(LoadOnShow);
                TryAddOP(new ObjectPropertie(true, "Display", true, (input) => {return 1;}));
                TryAddOP(new ObjectPropertie("Hello, World!", "Text", true, (input) => {return 1;}));
                TryAddOP(new ObjectPropertie(new Color(255,255,255,255,0), "Color", true, (input) => {return 1;}));
            }
            if (at == AssetType.Sound)
            {
                TryAddOP(new ObjectPropertie(false, "Play on Load", true, (input) => {return 1;}));
                TryAddOP(new ObjectPropertie(100.00, "Volume", true, (input) => {return 1;}));
                TryAddOP(new ObjectPropertie(1.00, "Pitch", true, (input) => {return 1;}));
            }
        }

        public void AddTransformProperties()
        {
            TryAddOP(new ObjectPropertie(new Vector3(0,0,0), "Position", true, (input) => {return 1;}));
            TryAddOP(new ObjectPropertie(new Vector3(0,0,0), "Rotation", true, (input) => {return 1;}));
            TryAddOP(new ObjectPropertie(new Vector3(1,1,1), "Size", true, (input) => {return 1;}));
        }

        public ObjectProperties(AssetType at, SceneObject Owner)
        {
            if (Props == null)  // Only create a new list if it's not already set (e.g., from JSON)
                Props = new List<ObjectPropertie>();
            initted = false;
        }
    }
    public class SceneObject
    {
        public Asset asset;
        public string name;
        public string Namespace;
        public AssetType assetType;
        public ObjectProperties OP;

        AssetType GetAssetType()
        {
            return asset.GetAssetType();
        }
        
        public SceneObject(Asset asset, string name, string Namespace, AssetType assetType = AssetType.Script)
        {
            this.asset = asset;
            this.name = name;
            this.Namespace = Namespace;
            this.assetType = GetAssetType();
            this.OP = new ObjectProperties(this.assetType, this);
        }
    }
    public class Asset
    {
        public string path;
        public string name;
        public string extension;
        public string localPath;

        public AssetType GetAssetType()
        {
            if (extension == "png" || extension == "jpg" || extension == "bmp")
                {
                    return AssetType.Image;
                }
                if (extension == "cms")
                {
                    return AssetType.Script;
                }
                if (extension == "ttf")
                {
                    return AssetType.Font;
                }
                if (extension == "ctrif")
                {
                    return AssetType.ImageFont;
                }
                if (extension == "wav" || extension == "mp3" || extension == "ogg")
                {
                    return AssetType.Sound;
                }
            return AssetType.Other;
        }
        
        public Asset(string path)
        {
            this.path = path;

            string[] both = Path.GetFileName(path).Split('.');

            name = both[0];
            extension = both[1];

            var assetsIndex = path.IndexOf("Assets", StringComparison.Ordinal);
    
            if (assetsIndex >= 0)
            {
                localPath = path.Substring(assetsIndex + "Assets/".Length);
            }
            else
            {
                localPath = name+"."+extension;
            }
        }
    }
    public static class EditorTools
    {
        public static BindingList<SceneObject> currentScene = new BindingList<SceneObject>();
        public static void RepositionMember<T>(BindingList<T> list, int fromIndex, int toIndex)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (fromIndex < 0 || fromIndex >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            if (toIndex < 0 || toIndex >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex));
            if (fromIndex == toIndex)
                return;

            // Suspend list change notifications if needed (optional depending on context)
            T item = list[fromIndex];
            list.RemoveAt(fromIndex);

            // Adjust target index if needed
            if (fromIndex < toIndex)
                toIndex--;

            list.Insert(toIndex, item);
        }
        public static void RepositionMember<T>(List<T> list, int fromIndex, int toIndex)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (fromIndex < 0 || fromIndex >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            if (toIndex < 0 || toIndex >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex));
            if (fromIndex == toIndex)
                return;

            T item = list[fromIndex];
            list.RemoveAt(fromIndex);

            // Adjust target index if removing earlier shifts it
            if (fromIndex < toIndex)
                toIndex--;

            list.Insert(toIndex, item);
        }
        public static string GenerateScrSceneDef()
        {
            string ch = "";
            int i = 0;
            ch += @"
#ifndef SCRSCRIPTSCENE_H
#define SCRSCRIPTSCENE_H

#include <vector>

            ";
            foreach (SceneObject SO1 in currentScene)
            {
                if (SO1.assetType == AssetType.Script)
                {
                    ch += "#include \"../CMS/"+SO1.asset.name+".h\"\n";
                }
            }
            string vectorsToAdd = "";
            foreach (SceneObject SO in currentScene)
            {
                if (SO.assetType == AssetType.Script)
                {
                    string script = File.ReadAllText(SO.asset.path);
                    string pattern = @".setvoid\s*""Start""\s*\(\s*\)";
                    bool containsPattern = Regex.IsMatch(script, pattern);

                    if (containsPattern)
                    {
                        string realName = SO.asset.name+"Ref"+i.ToString();
                        string sn = SO.asset.name;

                        ch += @$"
class {sn}Class {{
public:
std::string name;
std::string Namespace;
{sn}* script;

{sn}Class({sn}* scr, std::string n, std::string ns) :
script(scr), name(n), Namespace(ns) {{}}
}};";
                        vectorsToAdd += "\n"+$"static std::vector<{sn}Class*> {sn}classes;";
                    }
                }
            }
            ch += "class SceneRefs {\n"+
            "public:\n";
            ch += vectorsToAdd;
            ch += "\n};";
            ch += "\n#endif";
            return ch;
        }
        public static string GenerateRefDefs()
        {
            string ccpp = "";
            ccpp = "#include \"ScrSceneDef.h\"";

            string vectorsToAdd = "";
            int i = 0;
            foreach (SceneObject SO in currentScene)
            {
                if (SO.assetType == AssetType.Script)
                {
                    string script = File.ReadAllText(SO.asset.path);
                    string pattern = @".setvoid\s*""Start""\s*\(\s*\)";
                    bool containsPattern = Regex.IsMatch(script, pattern);

                    if (containsPattern)
                    {
                        string sn = SO.asset.name;

                        vectorsToAdd += "\n"+$"std::vector<{sn}Class*> {sn}classes;";
                    }
                }
                i++;
            }
            ccpp += vectorsToAdd;
            return ccpp;
        }
        public static string GenerateRefCpp()
        {
            string ccpp = "";
            ccpp += @"
#ifndef SCRIPTSCENE_H
#define SCRIPTSCENE_H

#include ""../../CTR/Classes.h""
#include ""../../CTR/Enums.h""
#include ""../../CTR/CTRScene.h""
#include ""../../CTR/ScrSceneDef.h""

class ScriptScene {
public:
            ";
            int i = 0;
            foreach (SceneObject SO in currentScene)
            {
                if (SO.assetType == AssetType.Script)
                {
                    string script = File.ReadAllText(SO.asset.path);
                    string pattern = @".setvoid\s*""Start""\s*\(\s*\)";
                    bool containsPattern = Regex.IsMatch(script, pattern);

                    if (containsPattern)
                    {
                        string realName = SO.asset.name+"Ref"+i.ToString();
                        string sn = SO.asset.name;
                        //currentList += SO.asset.name+" "+realName+";\n";
                        string toAdd = "";
                        toAdd += "static "+sn+"*"+" Get"+sn+"(const std::string& name, const std::string& Namespace) {\n";
                        toAdd += $@"
for (auto* asset : SceneRefs::{sn}classes) {{
        if (asset->name == name && asset->Namespace == Namespace) {{
            return asset->script;
        }}
    }}
    return nullptr;
}}
                        ";
                        toAdd += "\n";
                        ccpp += toAdd;
                    }
                }
                i++;
            }
            ccpp += "\n};\n#endif";
            return ccpp;
        }
        public static string GenerateStartList()
        {
            string currentList = "";
            int i = 0;
            foreach (SceneObject SO in currentScene)
            {
                if (SO.assetType == AssetType.Script)
                {
                    string script = File.ReadAllText(SO.asset.path);
                    string pattern = @".setvoid\s*""Start""\s*\(\s*\)";
                    bool containsPattern = Regex.IsMatch(script, pattern);

                    if (containsPattern)
                    {
                        string realName = SO.asset.name+"Ref"+i.ToString();
                        currentList += SO.asset.name+" "+realName+";\n";
                        currentList += (realName+".Start();\n");
                        string classInit = $"{SO.asset.name}Class(&{realName}, \"{SO.name}\", \"{SO.Namespace}\")";
                        currentList += "SceneRefs::"+SO.asset.name+$"classes.push_back(new {classInit});\n";
                    }
                }
                i++;
            }
            return currentList;
        }
        public static string GenerateUpdateList()
        {
            string currentList = "";
            int i = 0;
            foreach (SceneObject SO in currentScene)
            {
                if (SO.assetType == AssetType.Script)
                {
                    string script = File.ReadAllText(SO.asset.path);
                    string pattern = @".setvoid\s*""Update""\s*\(\s*\)";
                    bool containsPattern = Regex.IsMatch(script, pattern);

                    if (containsPattern)
                    {
                        string realName = SO.asset.name+"Ref"+i.ToString();
                        currentList += (realName+".Update();\n");
                    }
                }
                i++;
            }
            return currentList;
        }
        public static string GenerateIncludeList()
        {
            string cr = "";
            foreach (SceneObject so in currentScene)
            {
                if (so.assetType == AssetType.Script)
                {
                    cr += "#include \"CMS/"+so.asset.name+".h\"\n";
                }
            }
            return cr;
        }
        public static string GenerateSceneObjectsCPP(string storageprefix, string imageExtension, string imageprefix, string soundprefix, string otherprefix, string defaultSoundType)
        {
            string currentc = "";
            currentc += "std::vector<AssetProps*> AssetProps::assets;\nvoid CTRScene::InitScene() {\n";
            currentc += $"AssetProps::assets.reserve({currentScene.Count});\n";
            currentc += "AssetProps::assets = {\n";
            for (int i = 0; i < currentScene.Count; i++)
            {
                SceneObject current = currentScene[i];
                if (currentScene[i].assetType == AssetType.Image)
                {
                    bool show = (bool)current.OP.Props[8].currentValue;
                    bool los = (bool)current.OP.Props[7].currentValue;
                    Vector3 p = (Vector3)current.OP.Props[4].currentValue;
                    Vector3 r = (Vector3)current.OP.Props[5].currentValue;
                    Vector3 s = (Vector3)current.OP.Props[6].currentValue;
                    Color c = (Color)current.OP.Props[9].currentValue;
                    bool instaload = (bool)current.OP.Props[3].currentValue;
                    currentc += "new CTRImage(\""+storageprefix+imageprefix+current.asset.name+"."+imageExtension+"\","+show.ToString().ToLower()+","+los.ToString().ToLower()+","+
                    "{"+p.x.ToString()+","+p.y.ToString()+","+p.z.ToString()+"}"+
                    ","+"{"+s.x+","+s.y+","+s.z+"}"+
                    ","+"{"+r.x+","+r.y+","+r.z+"}"+
                    ",\""+current.name+"\",\""+current.Namespace+"\","+instaload.ToString().ToLower()+
                    ","+"{"+c.r+","+c.g+","+c.b+","+c.a+","+c.blend+"}"+","+Convert.ToInt32(current.OP.Props[2].currentValue)+
                    ")";
                }
                else if (currentScene[i].assetType == AssetType.ImageFont)
                {
                    bool show = (bool)current.OP.Props[8].currentValue;
                    bool los = (bool)current.OP.Props[7].currentValue;
                    Vector3 p = (Vector3)current.OP.Props[4].currentValue;
                    Vector3 r = (Vector3)current.OP.Props[5].currentValue;
                    Vector3 s = (Vector3)current.OP.Props[6].currentValue;
                    string t = (string)current.OP.Props[9].currentValue;
                    Color c = (Color)current.OP.Props[10].currentValue;
                    bool instaload = (bool)current.OP.Props[3].currentValue;
                    currentc += "new CTRImageFont(\""+storageprefix+imageprefix+current.asset.name+"."+imageExtension+"\","+show.ToString().ToLower()+","+los.ToString().ToLower()+","+
                    "{"+p.x.ToString()+","+p.y.ToString()+","+p.z.ToString()+"}"+
                    ","+"{"+s.x+","+s.y+","+s.z+"}"+
                    ","+"{"+r.x+","+r.y+","+r.z+"}"+
                    ",\""+current.name+"\",\""+current.Namespace+"\","+instaload.ToString().ToLower()+
                    ","+"{"+c.r+","+c.g+","+c.b+","+c.a+","+c.blend+"}"+","+Convert.ToInt32(current.OP.Props[2].currentValue)+
                    ",\""+t+"\""+
                    ")";
                }
                else if (currentScene[i].assetType == AssetType.Script)
                {
                    bool instaload = (bool)current.OP.Props[3].currentValue;
                    currentc += "new AssetProps(Enums::AssetType::Script, \""+current.asset.name+"\","+instaload.ToString().ToLower()+",\""+current.name+"\",\""+current.Namespace+"\","+
                    Convert.ToInt32(current.OP.Props[2].currentValue)+
                    ")";
                }
                else if (currentScene[i].assetType == AssetType.Sound)
                {
                    bool pOL = (bool)current.OP.Props[4].currentValue;
                    bool instaload = (bool)current.OP.Props[3].currentValue;
                    currentc += "new CTRSound(\""+
                    current.name+"\",\""+
                    current.Namespace+"\",\""+storageprefix+soundprefix+current.asset.name+"."+defaultSoundType+"\","+Convert.ToInt32(current.OP.Props[2].currentValue)+","+
                    instaload.ToString().ToLower()+","+
                    pOL.ToString().ToLower()+")";
                }

                if (i+1 != currentScene.Count)
                {
                    currentc+=",\n";
                }
            }
            currentc+="\n};\n};";
            return currentc;
        }
        public static List<Asset> GetAllAssets(Project p, AssetSortType ast = AssetSortType.Name)
        {
            List<Asset> currentAssetList = new List<Asset>();
            foreach (string file in GetAllFiles(Path.Combine(p.path, "Assets/")))
            {
                currentAssetList.Add(new Asset(file));
            }

            // Sorting the list based on the specified sorting type
            switch (ast)
            {
                case AssetSortType.Name:
                    currentAssetList = currentAssetList.OrderBy(asset => asset.name).ToList();
                    break;

                case AssetSortType.AssetType:
                    currentAssetList = currentAssetList.OrderBy(asset => asset.GetAssetType()).ToList();
                    break;

                case AssetSortType.Both:
                    currentAssetList = currentAssetList
                        .OrderBy(asset => asset.GetAssetType())
                        .ThenBy(asset => asset.name)
                        .ToList();
                    break;
            }

            return currentAssetList;
        }

        public static string[] GetAllFiles(string directoryPath)
        {
            return Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
        }
    }
    public class GridLayout : Panel
    {
        public TableLayout tableLayout;
        private int columns;
        private int rows;
        private List<Control> items;
        private Scrollable scrollable;

        public GridLayout(int columns, int width, int height, Size spacing, Padding padding)
        {
            this.columns = columns;
            this.rows = 0;
            this.items = new List<Control>();

            tableLayout = new TableLayout
            {
                Spacing = spacing,
                Padding = padding
            };

            scrollable = new Scrollable
            {
                Content = tableLayout,
                ExpandContentWidth = false,
                ExpandContentHeight = false,
                Size = new Size(width, height)
            };

            Content = scrollable;
        }

        public void SetItems(List<Control> items)
        {
            this.items = items;
            RebuildGrid();
        }

        public void AddItem(Control item)
        {
            items.Add(item);
            Console.WriteLine($"Added item. Total items: {items.Count}");
            RebuildGrid();
        }

        public void ClearItems()
        {
            items.Clear();
            tableLayout.Rows.Clear();
            rows = 0;
        }

        private void RebuildGrid()
        {
            var preservedItems = new List<Control>(items); // Clone the list

            // Create a new TableLayout and replace the existing one
            var newTableLayout = new TableLayout();
            rows = (preservedItems.Count + columns - 1) / columns;

            for (int row = 0; row < rows; row++)
            {
                var rowItems = new List<TableCell>();

                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;

                    if (index < preservedItems.Count)
                    {
                        // Wrap the control in a Panel and set the HorizontalContentAlignment
                        var panel = new StackLayout
                        {
                            Items = {preservedItems[index]},
                            Width = 100,
                            Height = 100,
                            HorizontalContentAlignment = HorizontalAlignment.Center
                        };

                        rowItems.Add(new TableCell(panel));
                    }
                    else
                    {
                        rowItems.Add(new TableCell(null));
                    }
                }

                newTableLayout.Rows.Add(new TableRow(rowItems.ToArray()));
            }

            // Replace the old tableLayout with the new one in the UI
            var parentContainer = tableLayout.Parent as Panel; // Assuming it's inside a Panel
            if (parentContainer != null)
            {
                parentContainer.Content = newTableLayout;
            }
            tableLayout = newTableLayout; // Update the reference to the new table layout
        }
    }
    public class NewObjectDialog : Dialog<string>
    {
        public DropDown dropdown;
        public DropDown TypeDropdown;
        public TextBox inputBox;
        public TextBox namespaceBox;
        public Button okButton;
        public Button cancelButton;
        public Asset ass;
        public bool aceepted = false;

        public NewObjectDialog(string title, Project p)
        {
            Title = title;
            ClientSize = new Size(300, 200);

            List<string> names = new List<string>();

            foreach (Asset a in EditorTools.GetAllAssets(p))
            {
                names.Add(a.name);
            }

            dropdown = new DropDown { DataStore = names, SelectedIndex = 0 };
            TypeDropdown = new DropDown { DataStore = Enum.GetNames(typeof(AssetType)), SelectedIndex = 0 };
            inputBox = new TextBox();
            namespaceBox = new TextBox();

            okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) =>
            {
                Result = $"{dropdown.SelectedValue} - {inputBox.Text}";
                ass = EditorTools.GetAllAssets(p)[dropdown.SelectedIndex];
                aceepted = true;
                Close();
            };

            cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (sender, e) => {Close();};

            var layout = new DynamicLayout { Padding = 10, Spacing = new Size(5, 5) };
            layout.Add(new Label { Text = "Select Asset Type:" });
            layout.Add(TypeDropdown);
            layout.Add(new Label { Text = "Select Asset:" });
            layout.Add(dropdown);
            layout.Add(new Label { Text = "Enter Object Name:" });
            layout.Add(inputBox);
            layout.Add(new Label { Text = "Enter Object Namespace:" });
            layout.Add(namespaceBox);
            layout.AddSeparateRow(null, okButton, cancelButton);

            Content = layout;
        }

        public static NewObjectDialog ShowDialog(Control parent, string title, Project p)
        {
            var dialog = new NewObjectDialog(title, p);
            dialog.ShowModal(parent);
            return dialog;
        }
    }
    public class EditorWindow : Form
    {
        private StackLayout buttonPanel;
        // Hierarchy
        public ListBox Hierarchy;
        public StackLayout HierarchyParent;
        public StackLayout HierarchyPanel;
        public Button PlusHierarchyButton;
        public Button RefreshHierarchyButton;
        public void RemoveSceneObjectFromScene(SceneObject so)
        {
            EditorTools.currentScene.Remove(so);
            RefreshHierarchy();
        }
        // Files
        public GridLayout FileExplorer;
        public StackLayout FileExplorerParent;
        public DropDown SortType;
        public AssetSortType currentAST = AssetSortType.Name;
        // Properties
        public StackLayout PropertyHolder;
        public StackLayout ObjectPropertiesContainer;
        public void RefreshProperties()
        {
            ObjectPropertiesContainer.Items.Clear();
            int index = Hierarchy.SelectedIndex;
            if (index < 0 || index >= EditorTools.currentScene.Count)
            {
                return;
            }
            SceneObject SO = EditorTools.currentScene[index];
            Console.WriteLine("Refreshing Properties");
            if (SO.OP.initted == false)
            {
                SO.OP.InitProps(SO.assetType, SO);
            }
            foreach (ObjectPropertie OP in SO.OP.Props)
            {
                Console.WriteLine("Making a new propertie");
                if (OP.defaultValue is string)
                {
                    Console.WriteLine("Making a new string propertie");
                    CreateStringPropertie(OP);
                }
                if (OP.defaultValue is bool)
                {
                    Console.WriteLine("Making a new bool propertie");
                    CreateBoolPropertie(OP);
                }
                if (OP.defaultValue is Vector3)
                {
                    CreateVector3Propertie(OP);
                }
                if (OP.defaultValue is Color)
                {
                    CreateColorPropertie(OP);
                }
                if (OP.defaultValue is int || OP.defaultValue is long)
                {
                    CreateIntPropertie(OP);
                }
            }
            Button removeButton = new Button {Text = "Remove -"};
            removeButton.Click += (sender,e) => {RemoveSceneObjectFromScene(SO);};
            ObjectPropertiesContainer.Items.Add(removeButton);
        }
        void CreateStringPropertie(ObjectPropertie OP)
        {
            TextBox valueO = new TextBox() {Width = 400, Text = (string)OP.currentValue};
            StackLayout input = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Label {Text = OP.name},
                    valueO
                }
            };

            valueO.TextChanged += (sender,e) => {OP.currentValue = valueO.Text; OP.onUpdate?.Invoke(valueO.Text);};

            ObjectPropertiesContainer.Items.Add(input);
        }
        void CreateBoolPropertie(ObjectPropertie OP)
        {
            CheckBox value = new CheckBox {Text = OP.name, Checked = (bool)OP.currentValue};

            value.CheckedChanged += (sender,e) => {OP.currentValue = value.Checked; OP.onUpdate?.Invoke(value.Checked); Console.WriteLine("CheckBox changed!");};

            ObjectPropertiesContainer.Items.Add(value);
        }
        void CreateDoublePropertie(ObjectPropertie OP)
        {
            TextBox valueO = new TextBox() {Width = 400, Text = (string)OP.currentValue};
            StackLayout input = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Label {Text = OP.name},
                    valueO
                }
            };

            valueO.TextChanged += (sender, e) =>
            {
                if (double.TryParse(valueO.Text, out double x))
                {
                    OP.currentValue = x;
                    OP.onUpdate?.Invoke(OP.currentValue);
                }
            };

            ObjectPropertiesContainer.Items.Add(input);
        }
        void CreateIntPropertie(ObjectPropertie OP)
        {
            TextBox valueO = new TextBox() {Width = 400, Text = OP.currentValue.ToString()};
            StackLayout input = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Label {Text = OP.name},
                    valueO
                }
            };

            valueO.TextChanged += (sender, e) =>
            {
                if (int.TryParse(valueO.Text, out int x))
                {
                    OP.currentValue = x;
                    OP.onUpdate?.Invoke(OP.currentValue);
                }
            };

            ObjectPropertiesContainer.Items.Add(input);
        }
        void CreateVector3Propertie(ObjectPropertie OP)
        {
            Vector3 currentVec = (Vector3)OP.currentValue;

            TextBox xValue = new TextBox() { Width = 100, Text = currentVec.x.ToString() };
            TextBox yValue = new TextBox() { Width = 100, Text = currentVec.y.ToString() };
            TextBox zValue = new TextBox() { Width = 100, Text = currentVec.z.ToString() };

            StackLayout input = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Label {Text = OP.name + " (X)"},
                    xValue,
                    new Label {Text = "(Y)"},
                    yValue,
                    new Label {Text = "(Z)"},
                    zValue
                }
            };

            xValue.TextChanged += (sender, e) =>
            {
                if (double.TryParse(xValue.Text, out double x))
                {
                    OP.currentValue = new Vector3(x, currentVec.y, currentVec.z);
                    OP.onUpdate?.Invoke(OP.currentValue);
                }
            };

            yValue.TextChanged += (sender, e) =>
            {
                if (double.TryParse(yValue.Text, out double y))
                {
                    OP.currentValue = new Vector3(currentVec.x, y, currentVec.z);
                    OP.onUpdate?.Invoke(OP.currentValue);
                }
            };

            zValue.TextChanged += (sender, e) =>
            {
                if (double.TryParse(zValue.Text, out double z))
                {
                    OP.currentValue = new Vector3(currentVec.x, currentVec.y, z);
                    OP.onUpdate?.Invoke(OP.currentValue);
                }
            };

            ObjectPropertiesContainer.Items.Add(input);
        }
        void CreateColorPropertie(ObjectPropertie OP)
        {
            Color currentColor = (Color)OP.currentValue;

            TextBox rValue = new TextBox() { Width = 50, Text = (currentColor.r).ToString() };
            TextBox gValue = new TextBox() { Width = 50, Text = (currentColor.g).ToString() };
            TextBox bValue = new TextBox() { Width = 50, Text = (currentColor.b).ToString() };
            TextBox aValue = new TextBox() { Width = 50, Text = (currentColor.a).ToString() };
            TextBox blValue = new TextBox() { Width = 50, Text = (currentColor.blend).ToString() };

            StackLayout input = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Label {Text = OP.name + " (R)"},
                    rValue,
                    new Label {Text = "(G)"},
                    gValue,
                    new Label {Text = "(B)"},
                    bValue,
                    new Label {Text = "(A)"},
                    aValue,
                    new Label {Text = "(Blend)"},
                    blValue
                }
            };

            void UpdateColor()
            {
                if (int.TryParse(rValue.Text, out int r) &&
                    int.TryParse(gValue.Text, out int g) &&
                    int.TryParse(bValue.Text, out int b) &&
                    int.TryParse(aValue.Text, out int a) &&
                    int.TryParse(blValue.Text, out int bl))
                {
                    r = Math.Clamp(r, 0, 255);
                    g = Math.Clamp(g, 0, 255);
                    b = Math.Clamp(b, 0, 255);
                    a = Math.Clamp(a, 0, 255);
                    bl = Math.Clamp(bl,0, 255);

                    OP.currentValue = new Color(r, g, b, a, bl);
                    OP.onUpdate?.Invoke(OP.currentValue);
                }
            }

            rValue.TextChanged += (sender, e) => UpdateColor();
            gValue.TextChanged += (sender, e) => UpdateColor();
            bValue.TextChanged += (sender, e) => UpdateColor();
            aValue.TextChanged += (sender, e) => UpdateColor();
            blValue.TextChanged += (sender, e) => UpdateColor();

            ObjectPropertiesContainer.Items.Add(input);
        }

        // Others
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
        public string GetPathForPreview(Asset a)
        {
            if (a.extension == "png" || a.extension == "jpg" || a.extension == "bmp")
                {
                    return a.path;
                }
                if (a.extension == "cms")
                {
                    return "gfx/CMS.png";
                }
                if (a.extension == "ttf" || a.extension == "ctrif")
                {
                    return "gfx/FontIcon.png";
                }
                if (a.extension == "wav" || a.extension == "mp3" || a.extension == "ogg")
                {
                    return "gfx/SoundIcon.png";
                }

            return "gfx/Text.png";
        }
        public Bitmap GetPreviewForAsset(Asset a)
        {
            if (GetPathForPreview(a) != null)
            {
                return new Bitmap(GetPathForPreview(a));
            } 
            return null;
        }
        public void RefreshHierarchy()
        {
            Hierarchy.Items.Clear();
            foreach (SceneObject SO in EditorTools.currentScene)
            {
                Hierarchy.Items.Add(new ListItem {Text = SO.Namespace+"/"+SO.name+$" ({SO.asset.localPath})", Key = GetPathForPreview(SO.asset)});
            }
            Hierarchy.ItemImageBinding = CreateImageBinding();
        }
        public void RefreshFileExplorer(Project p)
        {
            FileExplorer.ClearItems();
            AssetSortType ast = (AssetSortType)SortType.SelectedIndex;
            List<Asset> assetList = EditorTools.GetAllAssets(p, ast);
            List<Control> currentControls = new List<Control>();
            //assetList.Reverse();
            foreach (Asset a in assetList)
            {
                var imageView = new ImageView() {Size = new Size(50,50), Visible = false};
                if (GetPreviewForAsset(a) != null)
                {
                    imageView.Image = GetPreviewForAsset(a);
                    imageView.Visible = true;
                }

                StackLayout Holder = new StackLayout
                {
                    Width = 85,
                    Height = 85,
                    Orientation = Orientation.Vertical,
                    Items =
                    {
                        imageView,
                        new Label() {Text = a.name+$" ({a.extension})", Font = new Font(SystemFont.Default, 7)}
                    },
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                currentControls.Add(Holder);
            }
            FileExplorer.SetItems(currentControls);
        }
        void NewObjectVoid(Project p)
        {
            var result = NewObjectDialog.ShowDialog(this, "Create New Object", p);
            if (result.aceepted == false)
                return;
            
            EditorTools.currentScene.Add(new SceneObject(result.ass, result.inputBox.Text, result.namespaceBox.Text, (AssetType)result.TypeDropdown.SelectedIndex));
            RefreshHierarchy();
        }
        public EditorWindow(CTR.Projects.Project p)
        {
            Title = $"Cartridge Tilt Retro Editor ({CTR.Env.ctrVersion})";
            var screen = Screen.PrimaryScreen;
            var workingArea = screen.WorkingArea;
            ClientSize = new Size((int)workingArea.Width, (int)workingArea.Height);
            Resizable = false;
            this.Icon = new Icon("gfx/Icon.ico");
            int x = (int)workingArea.Width;
            int y = (int)workingArea.Height-65;

            buttonPanel = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Visible = false // Hidden by default
            };

            // Property Holder

            PropertyHolder = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Width = 600,
                Height = y-30,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            ObjectPropertiesContainer = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Width = 600,
                Height = y-30,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            PropertyHolder.Items.Add(new Label {Text = "Object Properties", Font = new Font(FontFamilies.Sans, 18, FontStyle.Bold)});
            PropertyHolder.Items.Add(ObjectPropertiesContainer);

            // FileExplorer

            FileExplorer = new GridLayout(3, 300, y-50, new Size(15, 15), new Padding(15)); // 3 columns

            Label explorerTitle = new Label {Text = "Asset Explorer", Font = new Font(FontFamilies.Sans, 18, FontStyle.Bold)};

            SortType = new DropDown 
            { 
                DataStore = Enum.GetNames(typeof(AssetSortType)), 
                SelectedIndex = 0 
            };

            SortType.SelectedIndexChanged += (sender, e) => 
            {
                RefreshFileExplorer(p);
            };

            RefreshFileExplorer(p);

            FileExplorer.MouseDown += (sender, e) => {RefreshFileExplorer(p);};

            FileExplorerParent = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Width = 300,
                Height = y-30,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items = {
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Items =
                        {
                            explorerTitle,
                            SortType
                        }
                    },
                    FileExplorer
                }
            };

            // Hierarchy

            Hierarchy = new ListBox
            {
                Width = 300,
                Height = y-50
            };

            ListBoxUtils.MakeListBoxReorderable(Hierarchy);
            // Propertie
            Hierarchy.SelectedIndexChanged += (sender,e) => {RefreshProperties();};
            Hierarchy.MouseDown += (sender,e) => {RefreshProperties();};
            EditorTools.currentScene.ListChanged += (sender,e) => {RefreshHierarchy();};
            //Hierarchy

            PlusHierarchyButton = new Button() {Text = "New +"};

            PlusHierarchyButton.Click += (sender,e ) => NewObjectVoid(p);

            RefreshHierarchyButton = new Button() {Text = "⟳", Size = new Size(12, 12)};

            RefreshHierarchyButton.Click += (sender,e ) => RefreshHierarchy();
            //⟳

            HierarchyPanel = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Spacing = 5,
                Visible = true,
                Items =
                {
                    new Label() {Text = "Hierarchy", Font = new Font(FontFamilies.Sans, 18, FontStyle.Bold)},
                    PlusHierarchyButton,RefreshHierarchyButton
                }
            };

            HierarchyParent = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Spacing = 5,
                Visible = true,
                Items =
                {
                    HierarchyPanel,
                    Hierarchy
                }
            };

            Hierarchy.KeyDown += HierarchyOnKeyDown;

            // other

            var menu = new MenuBar();
            var fileMenu = new ButtonMenuItem { Text = "File" };
            var sceneMenu = new ButtonMenuItem { Text = "Scene" };
            var assetsMenu = new ButtonMenuItem { Text = "Assets" };
            var testMenu = new ButtonMenuItem { Text = "Edit" };
            var cmsMenu = new ButtonMenuItem { Text = "CMS" };

            fileMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Build" }, this, p);};
            sceneMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Save Scene", "Load Scene", "Preview Scene" }, this, p);};
            assetsMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Open Asset Maker" }, this, p);};
            testMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Preferences", "Platform Manager" }, this, p);};
            cmsMenu.Click += (sender, e) => {ShowButtons(new List<string> { "Open CMS IDE" }, this, p);};

            menu.Items.Add(fileMenu);
            menu.Items.Add(sceneMenu);
            menu.Items.Add(assetsMenu);
            menu.Items.Add(testMenu);
            menu.Items.Add(cmsMenu);


            var layout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items =
                {
                    buttonPanel,
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Items =
                        {
                            HierarchyParent,
                            FileExplorerParent,
                            PropertyHolder
                        }
                    }
                }
            };

            Content = layout;

            Menu = menu;
        }

        private void HierarchyOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Up)
            {
                if (EditorTools.currentScene.Count > 1)
                {
                    int csi = Hierarchy.SelectedIndex;
                    EditorTools.RepositionMember(EditorTools.currentScene, csi,csi-1);
                    RefreshHierarchy();
                    Hierarchy.SelectedIndex = csi;
                }
            }
            else if (e.Key == Keys.Down)
            {
                if (EditorTools.currentScene.Count > 1)
                {
                    int csi = Hierarchy.SelectedIndex;
                    EditorTools.RepositionMember(EditorTools.currentScene, csi,csi+1);
                    RefreshHierarchy();
                    Hierarchy.SelectedIndex = csi;
                }
            }
        }

        bool showingPButtons = false;

        private void ShowButtons(List<string> platformNames, Panel callFrom, Project p)
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

                button.Click += (sender, e) => {Actions(name, callFrom, p);};
                buttonPanel.Items.Add(button);
            }

            buttonPanel.Visible = true; // Make the button panel visible
            }
            else
            {
                buttonPanel.Items.Clear();
            }
        }

        void Actions(string name, Panel callFrom, Project p)
        {
            if (name == "Open CMS IDE")
            {
                CMS.CMSIDEWindow cmsIDE;
                cmsIDE = new CMS.CMSIDEWindow();
            
                cmsIDE.Show();
            }
            else if (name == "Open Asset Maker")
            {
                AssetMaker AM = new AssetMaker();
                AM.Show();
            }
            else if (name == "Build")
            {
                SelectPlatformDialog SPL = new SelectPlatformDialog("Choose Platform to Build to:");
                SPL.ShowModal(this);
                if (SPL.choseYes)
                {
                    // Make Build Folder
                    Platform p2 = CTR.FileManager.Platforms.GetPlatforms()[SPL.platform.SelectedIndex];
                    string directory = Path.GetDirectoryName(Path.Combine(p.path,"Build/"));

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    foreach (string file in Directory.GetFiles(Path.Combine(p.path,"Build/")))
                    {
                        File.Delete(file);
                    }

                    // Delete all subdirectories
                    foreach (string subDirectory in Directory.GetDirectories(Path.Combine(p.path,"Build/")))
                    {
                        if (Path.GetFileName(subDirectory) == "CTR")
                        {
                            continue;
                        }
                        Directory.Delete(subDirectory, true); // true: delete subdirectories recursively
                    }

                    string targetPath = Path.Combine(p.path,"Build/");
                    File.WriteAllText(targetPath+"GeneratedSceneObjects.cpp", EditorTools.GenerateSceneObjectsCPP(
                        (string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                        "assetStoragePath"
                        ),(string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                        "defaultImageType"
                        ),(string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                        "imagePrefix"
                        ),(string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                        "soundPrefix"
                        ),(string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                        "otherPrefix"
                        ),(string)PlatformDLLLoader.GetValueFromDll(Path.Combine(p2.installPath, "MainAssembly.dll"),
                        "defaultSoundType"
                        )
                    ));

                    File.WriteAllText(targetPath+"GeneratedStarts.cpp", EditorTools.GenerateStartList());
                    File.WriteAllText(targetPath+"GeneratedSceneRefs.h", EditorTools.GenerateScrSceneDef()); //GenerateRefDefs
                    File.WriteAllText(targetPath+"GeneratedSceneRefs.cpp", EditorTools.GenerateRefDefs());
                    File.WriteAllText(targetPath+"GeneratedSceneRefsCMS.h", EditorTools.GenerateRefCpp());
                    File.WriteAllText(targetPath+"GeneratedUpdates.cpp", EditorTools.GenerateUpdateList());
                    File.WriteAllText(targetPath+"GeneratedIncludes.cpp", EditorTools.GenerateIncludeList());
                    foreach (SceneObject so in EditorTools.currentScene)
                    {
                        Asset a = so.asset;
                        if (a.GetAssetType() == AssetType.Script)
                        {
                            CMS.ConvertResult convertResult = CMS.Cpp.ConvertToCpp(File.ReadAllText(a.path), a.name);
                            File.WriteAllText(targetPath+a.name+".cpp", convertResult.OutputScript);
                            File.WriteAllText(targetPath+a.name+".h", convertResult.GenerateHFile());
                        }
                    }
                    //
                    PlatformDLLLoader.CallBuildPlatform(
                        Path.Combine(p2.installPath, "MainAssembly.dll"),
                        p.ctrProjPath
                    );

                    Console.WriteLine("Ran with ctrproj file: "+FileManager.Paths.GetProjectFilePaths()[SPL.platform.SelectedIndex]);
                }
                //PlatformDLLLoader.CallBuildPlatform();
            }
            else if (name == "Save Scene")
            {
                string directoryPath = Path.Combine(p.path, "MainScene");
                Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, "mainScene.json");
                File.WriteAllText(filePath, JsonConvert.SerializeObject(EditorTools.currentScene));
            }
            else if (name == "Load Scene")
            {
                string directoryPath = Path.Combine(p.path, "MainScene");

                string filePath = Path.Combine(directoryPath, "mainScene.json");
                if (File.Exists(filePath))
                {
                    EditorTools.currentScene = JsonConvert.DeserializeObject<BindingList<SceneObject>>(File.ReadAllText(filePath));
                    foreach (SceneObject SO in EditorTools.currentScene)
                    {
                        SO.OP.InitProps(SO.assetType, SO);
                    }
                    RefreshHierarchy();
                }
            }
            else if (name == "Preview Scene")
            {
                SelectPlatformDialog SPL = new SelectPlatformDialog("Choose Platform to Display Scene on:");
                SPL.ShowModal(this);
                if (SPL.choseYes)
                {
                    // Make Build Folder
                    Platform p2 = CTR.FileManager.Platforms.GetPlatforms()[SPL.platform.SelectedIndex];
                    string assemblyPath = Path.Combine(p2.installPath, "MainAssembly.dll");
                    int[][] screens = (int[][])PlatformDLLLoader.GetValueFromDll(assemblyPath, "ScreenSizes");

                    List<string> Screens = new List<string>();
                    int i = 1;
                    foreach (int[] screen in screens)
                    {
                        Screens.Add("Screen "+i+$" ({screen[0]+"x"+screen[1]})");
                        i++;
                    }

                    DropdownDialog D = new DropdownDialog("Choose Screen", Screens);
                    D.ShowModal(this);

                    if (D.yes)
                    {
                        DisplayPreview dp = new DisplayPreview(screens[D.selectedIndex][0], screens[D.selectedIndex][1], new Eto.Drawing.Color(0f, 0f, 0f, 1f), p2);
                        dp.Show();

                        foreach (SceneObject SO in EditorTools.currentScene)
                        {
                            if (Convert.ToInt32(SO.OP.Props[2].currentValue) != D.selectedIndex)
                                continue;
                            
                            if (SO.assetType == AssetType.Image)
                            {
                                Image img = dp.LoadImage(SO.asset.path);
                                Vector3 po = (Vector3)SO.OP.Props[4].currentValue;
                                Vector3 r = (Vector3)SO.OP.Props[5].currentValue;
                                Vector3 s = (Vector3)SO.OP.Props[6].currentValue;
                                Color c = (Color)SO.OP.Props[9].currentValue;
                                dp.DrawImage(img, (int)po.x, (int)po.y, c,s,r);
                            }
                        }
                    }
                }
            }
        }
    }

    public class DropdownDialog : Dialog<string>
    {
        public int selectedIndex;
        public bool yes = false;
        private DropDown comboBox;
        private Button okButton;

        public DropdownDialog(string title, List<string> options)
        {
            Title = title;
            ClientSize = new Size(300, 150);

            // Create the dropdown (ComboBox)
            comboBox = new DropDown { DataStore = options, SelectedIndex = 0 };

            // Create OK button
            okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) =>
            {
                Result = comboBox.SelectedValue as string;
                selectedIndex = comboBox.SelectedIndex;
                yes = true;
                Close();
            };

            // Layout: Vertical Stack
            Content = new StackLayout
            {
                Padding = 10,
                Items =
                {
                    new Label { Text = "Select an option:" },
                    comboBox,
                    okButton
                }
            };
        }
    }

    public class SelectPlatformDialog : Dialog<string>
    {
        public DropDown platform;
        public Button OkButton;
        public bool choseYes = false;

        public SelectPlatformDialog(string title)
        {
            Title = title;
            ClientSize = new Size(300, 200);

            List<string> names = new List<string>();
            foreach (CTR.Platform p in CTR.FileManager.Platforms.GetPlatforms())
            {
                names.Add(p.name);
            }

            platform = new DropDown { DataStore = names, SelectedIndex = 0, Width = 300 };
            OkButton = new Button {Text = "OK"};
            var cancelButton = new Button {Text = "Cancel"};
            cancelButton.Click += (sender,e) => {Close();};
            OkButton.Click += (sender,e) => {choseYes = true;Close();};

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items = {
                    platform,
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        Width = 300,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Items =
                        {
                            OkButton, cancelButton
                        }
                    }
                }
            };
        }
    }
}