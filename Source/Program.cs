using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;

namespace SR1PlayaCustomizer.Source {

    class DefaultItemNodeSorter : IComparer {

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

        static void Main(string[] args) {
            HideConsoleWindow();

            Globals.LoadXml_CustomizationDefaultItems();
            Globals.LoadXml_PlayerPresets();
            Globals.LoadStrings();

            FormMain formMain = new FormMain();

            Globals.LoadItems(formMain);
            Globals.LoadMorphs(formMain);
            Globals.LoadDefaultItems();
            Globals.LoadDefaultMorphs();

            formMain.ShowDialog();
        }

    }

}
