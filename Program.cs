using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SR1PlayaCustomizer {

    public class Globals {

        public const string ITEM_DISPLAY_NAME_NONE = "None";
        public static readonly TextInfo TEXT_INFO = (new CultureInfo("en-US", false)).TextInfo;
        public static readonly Dictionary<string, string> US_STRINGS = new Dictionary<string, string>();
        public static readonly Dictionary<string, Item> ITEMS = new Dictionary<string, Item>();
        public static readonly Dictionary<string, Item> SELECTED_ITEMS = new Dictionary<string, Item>();
        public static readonly Dictionary<string, MorphSet> MORPH_SETS = new Dictionary<string, MorphSet>();

        public static void SaveSelectedItems() {
            StringBuilder builder = new StringBuilder();
            builder.Append(ResourceGameFiles.CUSTOMIZATION_DEFAULT_ITEMS_PREFIX);
            foreach (Item Item in SELECTED_ITEMS.Values) {
                builder.Append(Item);
            }
            builder.Append(ResourceGameFiles.CUSTOMIZATION_DEFAULT_ITEMS_SUFFIX);

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, "customization_default_items.xtbl"), false)) {
                outputFile.Write(builder);
            }
            Console.WriteLine("Saved in " + path + Path.DirectorySeparatorChar + "customization_default_items.xtbl");
        }

    }

    public class MorphInfo {

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public float Value;
        public NumericUpDown UpDown;

        public MorphInfo(XmlNode node) {
            Name = node.SelectSingleNode("Name").InnerText;
            DisplayName = Globals.US_STRINGS[node.SelectSingleNode("DisplayName").InnerText];
        }

    }

    public class MorphSet {

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public readonly List<MorphInfo> list = new List<MorphInfo>();

        public MorphSet(XmlNode node) {
            Name = node.SelectSingleNode("Name").InnerText;
            DisplayName = Globals.US_STRINGS[node.SelectSingleNode("DisplayName").InnerText];
        }

    }

    public class WearOption {

        public string Name { get; }
        public string MeshName { get; }

        public WearOption(XmlNode node) {
            Name = Globals.US_STRINGS[node.SelectSingleNode("Name").InnerText];
            MeshName = node.SelectSingleNode("Mesh_Name").InnerText;
        }

        public override string ToString() {
            return MeshName;
        }

    }

    public class Variant {

        public string Name { get; }
        public string VariantName { get; }

        public Variant(XmlNode node) {
            Name = Globals.US_STRINGS[node.SelectSingleNode("Name").InnerText];
            VariantName = node.SelectSingleNode("Variant_Name").InnerText;
        }

        public override string ToString() {
            return VariantName;
        }

    }

    public class Item {

        public bool Selected { get; set; }
        public string Name { get; }
        public string CategoryInternal { get; }
        public string Category { get; }
        public string DisplayName { get; }
        public int SelWo { get; set; }
        public int SelV { get; set; }
        public WearOption[] WearOptions { get; }
        public Variant[] Variants { get; }
        public TreeNode Node { get; set; }

        // Creates an empty item
        public Item(string displayName, string category) {
            DisplayName = displayName;
            Category = CategoryInternal = category;
            WearOptions = new WearOption[0];
            Variants = new Variant[0];
        }

        // Creates an item from a document node
        public Item(XmlNode node) {
            Name = node.SelectSingleNode("Name").InnerText;
            Category = ("Tattoo").Equals(CategoryInternal = node.SelectSingleNode("Category").InnerText)
                ? Globals.TEXT_INFO.ToTitleCase(String.Concat(Name.TakeWhile(c => !char.IsNumber(c))))
                : CategoryInternal;
            DisplayName = Globals.US_STRINGS[node.SelectSingleNode("DisplayName").InnerText];

            XmlNodeList listWo = node.SelectSingleNode("Wear_Options").ChildNodes;
            WearOptions = new WearOption[listWo.Count];
            for (int i = 0; i < listWo.Count; i++) {
                WearOptions[i] = new WearOption(listWo.Item(i));
            }

            XmlNodeList listV = node.SelectSingleNode("Variants").ChildNodes;
            Variants = new Variant[listV.Count];
            for (int i = 0; i < listV.Count; i++) {
                Variants[i] = new Variant(listV.Item(i));
            }

            if (Variants.Length == 0 || WearOptions.Length == 0) {
                Console.WriteLine("Invalid item " + Name);
            }
        }

        public override string ToString() {
            return IsEmpty() ? String.Empty
                    : $"<Default><Item>{Name}</Item><Wear_Option>{WearOptions[SelWo]}</Wear_Option><Variant>{Variants[SelV]}</Variant></Default>";
        }

        public bool IsEmpty() {
            return WearOptions.Length == 0 || Variants.Length == 0;
        }

        public void SetSelected(bool selected) {
            if (Selected = selected && !IsEmpty()) {
                Node.Text = "\u2713 " + DisplayName;
                Node.Parent.Text = "\u2713 " + Category;
            } else {
                Node.Text = DisplayName;
                Node.Parent.Text = Category;
            }
        }

        public void SelectWearOption(string meshName) {
            for (int i = 0; i < WearOptions.Length; i++) {
                if (meshName.Equals(WearOptions[i].MeshName)) {
                    SelWo = i;
                    break;
                }
            }
        }

        public void SelectVariant(string variantName) {
            for (int i = 0; i < Variants.Length; i++) {
                if (variantName.Equals(Variants[i].VariantName)) {
                    SelV = i;
                    break;
                }
            }
        }

    }

    class NodeSorter : IComparer {

        public int Compare(object x, object y) {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;

            return tx.Text == Globals.ITEM_DISPLAY_NAME_NONE ? Int32.MinValue
                 : ty.Text == Globals.ITEM_DISPLAY_NAME_NONE ? Int32.MaxValue
                 : string.Compare(tx.Text, ty.Text);
        }

    }

    class Program {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void HideConsoleWindow() {
#if !DEBUG
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
#endif
        }

        static void LoadStrings() {
            int indexOf;
            string key, value;
            string[] lines = ResourceGameFiles.US_Strings.Split('\n');
            for (int i = 2; i < lines.Length - 2; i++) {
                indexOf = lines[i].IndexOf('=');
                key = lines[i].Substring(0, indexOf);
                value = lines[i].Substring(indexOf + 1).Trim(); // \r\n
                Globals.US_STRINGS[key] = value;
            }
        }

        static void LoadItems(FormMain formMain) {
            string strVal = System.Text.Encoding.UTF8.GetString(ResourceGameFiles.customization_items);
            XmlDocument xtbl = new XmlDocument();
            xtbl.LoadXml(strVal);
            XmlNode table = xtbl.ChildNodes.Item(0).ChildNodes.Item(0);

            Item nextItem;
            List<Item> itemCategory;
            TreeNode treeNodeCat, treeNodeItem;
            TreeNode[] nodes;

            foreach (XmlNode node in table.ChildNodes) {
                nextItem = new Item(node);
                Globals.ITEMS[nextItem.Name] = nextItem;

                if (("Hair").Equals(nextItem.Category)
                        || ("Eyes").Equals(nextItem.Category)) {
                    continue;
                }

                // Looking for the item category in the tree view
                nodes = formMain.Categories.Nodes.Find(nextItem.Category, false);
                if (nodes.Length == 0) {
                    treeNodeCat = new TreeNode(nextItem.Category);
                    treeNodeCat.Name = nextItem.Category;
                    treeNodeCat.Tag = (itemCategory = new List<Item>());
                    formMain.Categories.Nodes.Add(treeNodeCat);
                } else {
                    treeNodeCat = nodes[0];
                    itemCategory = (List<Item>)treeNodeCat.Tag;
                }

                // Adding the current item into it's category
                nextItem.Node = (treeNodeItem = new TreeNode(nextItem.DisplayName));
                treeNodeItem.Name = nextItem.Name;
                treeNodeItem.Tag = nextItem;
                treeNodeCat.Nodes.Add(treeNodeItem);
                itemCategory.Add(nextItem);
            }

            foreach (TreeNode node in formMain.Categories.Nodes) {
                nextItem = new Item(Globals.ITEM_DISPLAY_NAME_NONE, node.Text);
                nextItem.Node = (treeNodeItem = new TreeNode(nextItem.DisplayName));
                treeNodeItem.Tag = nextItem;
                node.Nodes.Add(treeNodeItem);
            }

            formMain.Categories.TreeViewNodeSorter = new NodeSorter();
            formMain.Categories.Sort();

            Console.WriteLine("Loaded " + table.ChildNodes.Count + " items in " + formMain.Categories.Nodes.Count + " categories");
        }

        static void LoadMorphs(FormMain formMain) {
            string strVal = System.Text.Encoding.UTF8.GetString(ResourceGameFiles.player_creation);
            XmlDocument xtbl = new XmlDocument();
            xtbl.LoadXml(strVal);
            XmlNode table = xtbl.ChildNodes.Item(0).ChildNodes.Item(0);

            // Data
            MorphSet morphSet;
            MorphInfo morphInfo;
            XmlNode xmlMorphInfos;

            // UI
            FlowLayoutPanel container;
            Label label;
            NumericUpDown upDown;

            foreach (XmlNode xmlMorphSet in table.ChildNodes) {
                morphSet = new MorphSet(xmlMorphSet);
                Globals.MORPH_SETS[morphSet.Name] = morphSet;

                xmlMorphInfos = xmlMorphSet.SelectSingleNode("Morph_Infos");

                foreach (XmlNode xmlMorphInfo in xmlMorphInfos) {
                    morphInfo = new MorphInfo(xmlMorphInfo);
                    morphInfo.Value = 0.5f;

                    label = new Label();
                    label.Text = morphInfo.DisplayName;

                    upDown = new NumericUpDown();
                    upDown.Maximum = 100;
                    upDown.Minimum = 0;
                    upDown.Width = 50;
                    upDown.Value = 50;
                    upDown.Tag = morphInfo;
                    morphInfo.UpDown = upDown;

                    container = new FlowLayoutPanel();
                    container.FlowDirection = FlowDirection.LeftToRight;
                    container.AutoSize = true;
                    container.Height = 20;
                    container.Controls.Add(label);
                    container.Controls.Add(upDown);

                    formMain.flowLayoutPanel1.Controls.Add(container);
                }

                Console.WriteLine("Loaded " + xmlMorphInfos.ChildNodes.Count + " entries of morph set " + morphSet.Name);
            }
        }

        static void LoadDefaultItems() {
            string pathLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pathDocument = Path.Combine(pathLocation, "customization_default_items.xtbl");

            if (!File.Exists(pathDocument)) {
                return;
            }

            XmlDocument xml = new XmlDocument();
            xml.Load(pathDocument);
            XmlNode defaultsList = xml.ChildNodes.Item(0).ChildNodes.Item(0)
                    .ChildNodes.Item(0).SelectSingleNode("Defaults_List");
            Item item;

            foreach (XmlNode def in defaultsList) {
                item = Globals.ITEMS[def.SelectSingleNode("Item").InnerText];
                item.SelectWearOption(def.SelectSingleNode("Wear_Option").InnerText);
                item.SelectVariant(def.SelectSingleNode("Variant").InnerText);
                item.SetSelected(true);

                Globals.SELECTED_ITEMS[item.Category] = item;
            }

            Console.WriteLine("Loaded " + defaultsList.ChildNodes.Count + " items from the existing files");
        }

        static void Main(string[] args) {
            HideConsoleWindow();

            FormMain formMain = new FormMain();
            LoadStrings();

            LoadItems(formMain);
            LoadMorphs(formMain);
            LoadDefaultItems();

            formMain.ShowDialog();
        }

    }

}
