using SR1PlayaCustomizer.Source;
using System;
using System.Linq;
using System.Windows.Forms;

namespace SR1PlayaCustomizer.Source {

    public partial class FormMain : Form {

        private FormVariant FormVariant;

        public FormMain() {
            FormVariant = new FormVariant(this);
            InitializeComponent();
        }

        private bool IsItem(TreeNode node) {
            return node.Tag != null && node.Tag.GetType() == typeof(Item);
        }

        private TreeNode GetSelectedItem(TreeNode category) {
            foreach (TreeNode node in category.Nodes) {
                if (!IsItem(node)) continue;
                if (((Item)node.Tag).Selected) return node;
            }
            return null;
        }

        public void RemoveSelection(TreeNode category) {
            Item item;
            TreeNode SelectedNode = GetSelectedItem(category);
            if (SelectedNode != null) {
                item = (Item)SelectedNode.Tag;
                item.SetSelected(false);
            }
        }

        private void Categories_AfterSelect(object sender, TreeViewEventArgs e) {
            TreeNode node = e.Node;

            if (!IsItem(node)) {
                if (node.Text == Globals.ITEM_DISPLAY_NAME_NONE) {
                    RemoveSelection(node.Parent);
                }
                return;
            }

            Item item = (Item)node.Tag;

            if (item.IsEmpty()) {
                FormVariant.Apply();
            } else {
                FormVariant.LoadItem((Item)item);
                FormVariant.Node = node;
                FormVariant.Text = item.DisplayName;
                FormVariant.ShowDialog();
            }
        }

        private void Save(object sender, EventArgs e) {
            Globals.SaveSelectedItems();
            Globals.SavePresets();
            Console.WriteLine("Saved items and morphs");
        }
    }

}
