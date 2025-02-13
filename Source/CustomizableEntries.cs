using SR1PlayaCustomizer.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SR1PlayaCustomizer.Source {

    public class MorphInfo {

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public float Value;
        public NumericUpDown UpDown;

        public MorphInfo(XmlNode node) {
            Name = node.SelectSingleNode("Name").InnerText;
            DisplayName = Globals.US_STRINGS[node.SelectSingleNode("DisplayName").InnerText];
        }

        public override string ToString() {
            return $"<Preset_Element><Morph_Name>{Name}</Morph_Name><Value>{Value}</Value></Preset_Element>";
        }

    }

    public class MorphSet {

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public readonly List<MorphInfo> List = new List<MorphInfo>();

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

        public bool Selected { get; set; }
        public string Name { get; }
        public string CategoryInternal { get; }
        public string Category { get; }
        public string DisplayName { get; }
        public WearOption[] WearOptions { get; }
        public Variant[] Variants { get; }

        public int SelWo { get; set; }
        public int SelV { get; set; }

        public TreeNode NodeTree { get; set; }
        public XmlNode NodeXml { get; set; }

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

        public void Apply() {
            if (NodeXml == null) {

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
                NodeTree.Text = "\u2713 " + DisplayName;
                NodeTree.Parent.Text = "\u2713 " + Category;
            } else {
                NodeTree.Text = DisplayName;
                NodeTree.Parent.Text = Category;
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

}
