using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SR1PlayaCustomizer.Source {

    public class Globals {

        public const string ITEM_DISPLAY_NAME_NONE = "None";
        public static readonly Font FONR_REGULAR = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular);
        public static readonly Font FONT_SELECTED = new Font(FONR_REGULAR, FontStyle.Bold);
        public static readonly TextInfo TEXT_INFO = (new CultureInfo("en-US", false)).TextInfo;
        public static readonly Dictionary<string, string> US_STRINGS = new Dictionary<string, string>();
        public static readonly Dictionary<string, Item> ITEMS = new Dictionary<string, Item>();
        public static readonly Dictionary<string, MorphSet> MORPH_SETS = new Dictionary<string, MorphSet>();
        public static XmlDocument XTBL_CUSTOMIZATION_DEFAULT_ITEMS, XTBL_PLAYER_PRESETS;
        public static XmlNode NODE_DEFAULTS_LIST, NODE_PRESET_GRID;

        public static void LoadStrings() {
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

        public static XmlDocument LoadXml(String filename, Lazy<byte[]> def) {
            string pathLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pathDocument = Path.Combine(pathLocation, filename);
            XmlDocument xmlDoc = new XmlDocument();

            if (File.Exists(pathDocument)) {
                xmlDoc.Load(pathDocument);
            } else {
                xmlDoc.LoadXml(Encoding.UTF8.GetString(def.Value));
            }

            return xmlDoc;
        }

        public static void LoadXml_CustomizationDefaultItems() {
            XTBL_CUSTOMIZATION_DEFAULT_ITEMS = LoadXml("customization_default_items.xtbl",
                    new Lazy<byte[]>(() => ResourceGameFiles.customization_default_items));
            NODE_DEFAULTS_LIST = XTBL_CUSTOMIZATION_DEFAULT_ITEMS
                    .SelectSingleNode("//root/Table/Defaults/Defaults_List");
        }

        public static void LoadXml_PlayerPresets() {
            XTBL_PLAYER_PRESETS = LoadXml("player_presets.xtbl",
                    new Lazy<byte[]>(() => ResourceGameFiles.player_presets));

            // Adding a node for a custom preset, if doesn't exist
            XmlNode table = XTBL_PLAYER_PRESETS
                    .SelectSingleNode("//root/Table");
            XmlNode preset = table.ChildNodes.Item(0);
            if (table.ChildNodes.Count == 4) {
                XmlNode presetOld = preset;
                preset = table.PrependChild(preset.CloneNode(true));
                preset.SelectSingleNode("DisplayName").InnerText = "MAINMENU_MULTI";
                presetOld.SelectSingleNode("Name").InnerText = "white2";
            }

            NODE_PRESET_GRID = table.SelectSingleNode("//Preset[1]/Preset_Grid");
        }

        public static void LoadItems(FormMain formMain) {
            XmlDocument xtbl = new XmlDocument();
            xtbl.LoadXml(Encoding.UTF8.GetString(ResourceGameFiles.customization_items));

            XmlNode table = xtbl.ChildNodes.Item(0).ChildNodes.Item(0);

            Item nextItem;
            List<Item> itemCategory;
            TreeNode treeNodeCat, treeNodeItem;
            TreeNode[] nodes;

            foreach (XmlNode node in table.ChildNodes) {
                nextItem = new Item(node);
                ITEMS[nextItem.Name] = nextItem;

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
                nextItem.NodeTree = (treeNodeItem = new TreeNode(nextItem.DisplayName));
                treeNodeItem.Name = nextItem.Name;
                treeNodeItem.Tag = nextItem;
                treeNodeCat.Nodes.Add(treeNodeItem);
                itemCategory.Add(nextItem);
            }

            // Adding "None" entries
            foreach (TreeNode node in formMain.Categories.Nodes) {
                treeNodeItem = new TreeNode(Globals.ITEM_DISPLAY_NAME_NONE);
                node.Nodes.Add(treeNodeItem);
            }

            // Sorting the categories with items
            formMain.Categories.TreeViewNodeSorter = new DefaultItemNodeSorter();
            formMain.Categories.Sort();

            Console.WriteLine("Loaded " + table.ChildNodes.Count + " items in " + formMain.Categories.Nodes.Count + " categories");
        }

        public static void LoadMorphs(FormMain formMain) {
            XmlDocument xtbl = new XmlDocument();
            xtbl.LoadXml(Encoding.UTF8.GetString(ResourceGameFiles.player_creation));

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
                MORPH_SETS[morphSet.Name] = morphSet;

                xmlMorphInfos = xmlMorphSet.SelectSingleNode("Morph_Infos");

                foreach (XmlNode node in xmlMorphInfos) {
                    morphInfo = new MorphInfo(node);
                    morphInfo.Value = 0.5f;
                    morphSet.List.Add(morphInfo);

                    label = new Label();
                    label.Text = morphInfo.DisplayName;

                    upDown = new NumericUpDown();
                    upDown.Maximum = 100;
                    upDown.Minimum = 0;
                    upDown.Width = 50;
                    upDown.Value = 50;
                    upDown.Tag = morphInfo;
                    upDown.ValueChanged += morphInfo.UpDown_ValueChange;
                    morphInfo.UpDown = upDown;

                    container = new FlowLayoutPanel();
                    container.FlowDirection = FlowDirection.LeftToRight;
                    container.AutoSize = true;
                    container.Height = 20;
                    container.Controls.Add(label);
                    container.Controls.Add(upDown);

                    formMain.flowLayoutMorph.Controls.Add(container);
                }

                Console.WriteLine("Loaded " + xmlMorphInfos.ChildNodes.Count + " entries of morph set " + morphSet.Name);
            }
        }

        public static void LoadDefaultItems() {
            Item item;
            List<Item> selected = new List<Item>();

            foreach (XmlNode node in NODE_DEFAULTS_LIST) {
                item = ITEMS[node.SelectSingleNode("Item").InnerText];
                item.SelectWearOption(node.SelectSingleNode("Wear_Option").InnerText);
                item.SelectVariant(node.SelectSingleNode("Variant").InnerText);
//              item.SetSelected(true); // concurrent modification
                selected.Add(item);
            }

            foreach (Item nextItem in selected) {
                nextItem.SetSelected(true); // no concurrent modification
            }

            Console.WriteLine("Loaded " + NODE_DEFAULTS_LIST.ChildNodes.Count + " items from the existing files");
        }

        public static void LoadDefaultMorphs() {
            XmlNode node;

            foreach (MorphSet set in MORPH_SETS.Values) {
                foreach (MorphInfo info in set.List) {
                    node = NODE_PRESET_GRID.SelectSingleNode($"//Preset_Element[Morph_Name='{info.Name}']");
                    
                    if (node != null) {
                        info.UpDown.Value = (int)(float.Parse(node.SelectSingleNode("Value").InnerText) * 100);
                    }
                }
             }
        }

        public static void SaveSelectedItems() {
            Globals.XTBL_CUSTOMIZATION_DEFAULT_ITEMS.Save("customization_default_items.xtbl");
            Console.WriteLine("Saved items");
        }

        public static void SavePresets() {
            Globals.XTBL_PLAYER_PRESETS.Save("player_presets.xtbl");
            Console.WriteLine("Saved morphs");
        }

    }

}
