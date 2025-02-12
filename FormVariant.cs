using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SR1PlayaCustomizer {

    public partial class FormVariant : Form {

        public Item Item { get; set; }
        public TreeNode Node { get; set; }

        public FormVariant() {
            InitializeComponent();
        }

        public void Apply() {
            Item.SelV = DomainVariant.SelectedIndex;
            Item.SelWo = DomainWearOption.SelectedIndex;
            Item.SetSelected(true);

            Console.WriteLine("Accepted " + Item);
            Globals.SELECTED_ITEMS[Item.Category] = Item;
            Globals.SaveSelectedItems();
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
