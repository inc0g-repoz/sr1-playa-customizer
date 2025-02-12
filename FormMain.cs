﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SR1PlayaCustomizer {

    public partial class FormMain : Form {

        private FormVariant FormVariant = new FormVariant();

        public FormMain() {
            InitializeComponent();
        }

        private bool IsItem(TreeNode node) {
            return node.Tag != null && node.Tag.GetType() == typeof(Item);
        }

        private void Categories_AfterSelect(object sender, TreeViewEventArgs e) {
            TreeNode node = e.Node;

            if (!IsItem(node)) {
                return;
            }

            Item item;
            TreeNode lastSelected = GetSelectedItem(node.Parent);
            if (lastSelected != null) {
                item = (Item) lastSelected.Tag;
                item.Selected = false;
                item.SetSelected(false);

                Globals.SELECTED_ITEMS.Remove(item.Category);
            }

            item = (Item) node.Tag;

            if (FormVariant.DomainVariant.Enabled = (item.Variants.Count() != 0)) {
                FormVariant.DomainVariant.Items.Clear();
                foreach (Variant v in item.Variants) {
                    FormVariant.DomainVariant.Items.Add(v.Name);
                }
                FormVariant.DomainVariant.SelectedIndex = 0;
            } else {
                FormVariant.DomainVariant.Text = string.Empty;
            }

            if (FormVariant.DomainWearOption.Enabled = (item.WearOptions.Count() != 0)) {
                FormVariant.DomainWearOption.Items.Clear();
                foreach (WearOption wo in item.WearOptions) {
                    FormVariant.DomainWearOption.Items.Add(wo.Name);
                }
                FormVariant.DomainWearOption.SelectedIndex = 0;
            } else {
                FormVariant.DomainWearOption.Text = string.Empty;
            }

            FormVariant.Item = item;
            FormVariant.Node = node;
            FormVariant.Text = item.DisplayName;

            if (FormVariant.Item.IsEmpty()) {
                FormVariant.Apply();
            } else {
                FormVariant.ShowDialog();
            }
        }

        private TreeNode GetSelectedItem(TreeNode category) {
            foreach (TreeNode node in category.Nodes) {
                if (!IsItem(node)) continue;
                if (((Item)node.Tag).Selected) return node;
            }
            return null;
        }

    }

}
