using SR1PlayaCustomizer.Source;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SR1PlayaCustomizer.Source {

    public class MorphInfo {

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public float Value;

        public XmlNode NodeXml;
        public NumericUpDown UpDown;

        public MorphInfo(XmlNode node) {
            Name = node.SelectSingleNode("Name").InnerText;
            DisplayName = Globals.US_STRINGS[node.SelectSingleNode("DisplayName").InnerText];
        }

        public override string ToString() {
            return $"<Preset_Element><Morph_Name>{Name}</Morph_Name><Value>{Value}</Value></Preset_Element>";
        }

        public void UpDown_ValueChange(object sender, EventArgs e) {
            NumericUpDown upDown = sender as NumericUpDown;
            Value = (float)upDown.Value / 100;

            ValidateXmlNode();
            UpdateXmlNode();

            Console.WriteLine($"Set morph property \"{Name}\" to {Value}");
        }

        private XmlNode CreateXmlNode() {
            XmlDocument xtbl = Globals.XTBL_PLAYER_PRESETS;
            XmlNode node = xtbl.CreateElement("Preset_Element");
            node.AppendChild(xtbl.CreateElement("Morph_Name")).InnerText = Name;
            node.AppendChild(xtbl.CreateElement("Value")).InnerText = Value.ToString();
            return node;
        }

        private XmlNode FindXmlNode() {
            return Globals.NODE_PRESET_GRID.SelectSingleNode($"//Preset_Element[Morph_Name='{Name}']");
        }

        private void ValidateXmlNode() {
            if (NodeXml != null) {
                return;
            }

            NodeXml = FindXmlNode();

            if (NodeXml == null) {
                NodeXml = CreateXmlNode();
                Globals.NODE_PRESET_GRID.AppendChild(NodeXml);
            }
        }

        private void UpdateXmlNode() {
            NodeXml.SelectSingleNode("Value").InnerText = Value.ToString();
        }

    }

    public class MorphSet {

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public readonly List<MorphInfo> List = new List<MorphInfo>();

        public XmlNode NodeXml;

        public MorphSet(XmlNode node) {
            Name = node.SelectSingleNode("Name").InnerText;
            DisplayName = Globals.US_STRINGS[node.SelectSingleNode("DisplayName").InnerText];
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            foreach (MorphInfo info in List) {
                builder.Append(info);
            }
            return builder.ToString();
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

        public string Name { get; }
        public string CategoryInternal { get; }
        public string Category { get; }
        public string DisplayName { get; }

        public WearOption[] WearOptions { get; }
        public Variant[] Variants { get; }

        public bool Selected { get; set; }
        public int SelWo { get; set; }
        public int SelV { get; set; }

        public TreeNode NodeTree { get; set; }
        public XmlNode NodeXml { get; set; }

        // Creates an empty item
        public Item(XmlNode node) {
            Name = node.SelectSingleNode("Name").InnerText;
            DisplayName = Globals.US_STRINGS[node.SelectSingleNode("DisplayName").InnerText];
            Category = ("Tattoo").Equals(CategoryInternal = node.SelectSingleNode("Category").InnerText)
                ? Globals.TEXT_INFO.ToTitleCase(String.Concat(Name.TakeWhile(c => !char.IsNumber(c))))
                : CategoryInternal;

            // Saving wear options
            XmlNodeList listWo = node.SelectSingleNode("Wear_Options").ChildNodes;
            WearOptions = new WearOption[listWo.Count];
            for (int i = 0; i < listWo.Count; i++) {
                WearOptions[i] = new WearOption(listWo.Item(i));
            }

            // Saving variants
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
            if (Selected == selected) return;

            ValidateXmlNode();

            if (Selected = selected && !IsEmpty()) {
//              NodeTree.NodeFont = Globals.FONT_SELECTED;
                NodeTree.Text = "\u2713 " + DisplayName;
//              NodeTree.Parent.NodeFont = Globals.FONT_SELECTED;
                NodeTree.Parent.Text = "\u2713 " + Category;
                UpdateXmlNode();
                Globals.NODE_DEFAULTS_LIST.AppendChild(NodeXml);
            } else {
//              NodeTree.NodeFont = Globals.FONR_REGULAR;
                NodeTree.Text = DisplayName;
//              NodeTree.Parent.NodeFont = Globals.FONR_REGULAR;
                NodeTree.Parent.Text = Category;
                Globals.NODE_DEFAULTS_LIST.RemoveChild(NodeXml);
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

        private XmlNode CreateXmlNode() {
            XmlDocument xtbl = Globals.XTBL_CUSTOMIZATION_DEFAULT_ITEMS;
            XmlNode node = xtbl.CreateElement("Default");
            node.AppendChild(xtbl.CreateElement("Item")).InnerText = Name;
            node.AppendChild(xtbl.CreateElement("Wear_Option")).InnerText = WearOptions[SelWo].MeshName;
            node.AppendChild(xtbl.CreateElement("Variant")).InnerText = Variants[SelV].VariantName;
            return node;
        }

        private XmlNode FindXmlNode() {
            return Globals.NODE_DEFAULTS_LIST.SelectSingleNode($"//Default[Item='{Name}']");
        }

        private void ValidateXmlNode() {
            if (NodeXml != null) {
                return;
            }

            NodeXml = FindXmlNode();

            if (NodeXml == null) {
                NodeXml = CreateXmlNode();
            }
        }

        private void UpdateXmlNode() {
            NodeXml.SelectSingleNode("Item").InnerText = Name;
            NodeXml.SelectSingleNode("Wear_Option").InnerText = WearOptions[SelWo].MeshName;
            NodeXml.SelectSingleNode("Variant").InnerText = Variants[SelV].VariantName;
        }

    }

}
