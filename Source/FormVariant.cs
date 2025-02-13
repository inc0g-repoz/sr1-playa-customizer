using SR1PlayaCustomizer.Source;
using System;
using System.Windows.Forms;

namespace SR1PlayaCustomizer.Source {

    public partial class FormVariant : Form {

        public FormMain FormMain { get; }

        public Item Item { get; set; }
        public TreeNode Node { get; set; }

        public FormVariant(FormMain formMain) {
            FormMain = formMain;
            InitializeComponent();
        }

        public void LoadItem(Item item) {
            Item = item;
            Node = item.NodeTree;
            Text = item.DisplayName;

            if (DomainVariant.Enabled = (item.Variants.Length != 0)) {
                DomainVariant.Items.Clear();
                foreach (Variant v in item.Variants) {
                    DomainVariant.Items.Add(v.Name);
                }
                DomainVariant.SelectedIndex = 0;
            } else {
                DomainVariant.Text = string.Empty;
            }

            if (DomainWearOption.Enabled = (item.WearOptions.Length != 0)) {
                DomainWearOption.Items.Clear();
                foreach (WearOption wo in item.WearOptions) {
                    DomainWearOption.Items.Add(wo.Name);
                }
                DomainWearOption.SelectedIndex = 0;
            } else {
                DomainWearOption.Text = string.Empty;
            }
        }

        public void Apply() {
            FormMain.RemoveSelection(Node.Parent);

            Item.SelV = DomainVariant.SelectedIndex;
            Item.SelWo = DomainWearOption.SelectedIndex;
            Item.SetSelected(true);

            Console.WriteLine("Applied " + Item);
        }

        private void ButtonApply_Click(object sender, EventArgs e) {
            Apply();
            Hide();
        }

        private void ButtonCancel_Click(object sender, EventArgs e) {
            Console.WriteLine("Cancelled");
            Hide();
        }

    }

}
