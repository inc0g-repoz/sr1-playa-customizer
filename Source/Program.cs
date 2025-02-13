using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;

namespace SR1PlayaCustomizer.Source {

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
                nextItem.NodeTree = (treeNodeItem = new TreeNode(nextItem.DisplayName));
                treeNodeItem.Name = nextItem.Name;
                treeNodeItem.Tag = nextItem;
                treeNodeCat.Nodes.Add(treeNodeItem);
                itemCategory.Add(nextItem);
            }

            foreach (TreeNode node in formMain.Categories.Nodes) {
                nextItem = new Item(Globals.ITEM_DISPLAY_NAME_NONE, node.Text);
                nextItem.NodeTree = (treeNodeItem = new TreeNode(nextItem.DisplayName));
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
                    morphSet.List.Add(morphInfo);

                    label = new Label();
                    label.Text = morphInfo.DisplayName;

                    upDown = new NumericUpDown();
                    upDown.Maximum = 100;
                    upDown.Minimum = 0;
                    upDown.Width = 50;
                    upDown.Value = 50;
                    upDown.Tag = morphInfo;
                    upDown.ValueChanged += formMain.UpDown_ValueChange;
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

            Globals.XTBL_CUSTOMIZATION_DEFAULT_ITEMS = new XmlDocument();
            Globals.XTBL_CUSTOMIZATION_DEFAULT_ITEMS.Load(pathDocument);
            XmlNode defaultsList = Globals.XTBL_CUSTOMIZATION_DEFAULT_ITEMS
                    .ChildNodes.Item(0).ChildNodes.Item(0).ChildNodes.Item(0)
                    .SelectSingleNode("Defaults_List");
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

        public void LoadDefaultMorphs() {
            string pathLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pathDocument = Path.Combine(pathLocation, "player_presets.xtbl");
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
