using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace SR1PlayaCustomizer.Source {

    public class Globals {

        public const string ITEM_DISPLAY_NAME_NONE = "None";
        public static readonly TextInfo TEXT_INFO = (new CultureInfo("en-US", false)).TextInfo;
        public static readonly Dictionary<string, string> US_STRINGS = new Dictionary<string, string>();
        public static readonly Dictionary<string, Item> ITEMS = new Dictionary<string, Item>();
        public static readonly Dictionary<string, Item> SELECTED_ITEMS = new Dictionary<string, Item>();
        public static readonly Dictionary<string, MorphSet> MORPH_SETS = new Dictionary<string, MorphSet>();
        public static XmlDocument XTBL_CUSTOMIZATION_DEFAULT_ITEMS, XTBL_PLAYER_PRESETS;

        public static void SaveSelectedItems() {
            StringBuilder builder = new StringBuilder();
            builder.Append(ResourceGameFiles.CUSTOMIZATION_DEFAULT_ITEMS_PREFIX);
            foreach (Item Item in SELECTED_ITEMS.Values) {
                builder.Append(Item);
            }
            builder.Append(ResourceGameFiles.CUSTOMIZATION_DEFAULT_ITEMS_SUFFIX);

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "customization_default_items.xtbl");
            using (StreamWriter outputFile = new StreamWriter(path, false)) {
                outputFile.Write(builder);
            }

            Console.WriteLine("Saved items in " + path);
        }

        public static void SavePresets() {
            StringBuilder builder = new StringBuilder();
            builder.Append(ResourceGameFiles.PLAYER_PRESETS_PREFIX);
            builder.Append("<Preset_Grid>");
            foreach (MorphSet set in MORPH_SETS.Values) {
                builder.Append(set.ToString());
            }
            builder.Append("</Preset_Grid>");
            builder.Append("<Hair>combed forward</Hair><Hair_Length>0.5</Hair_Length><Skin><Trans_Grid><Trans_Elem><Min>.25</Min><Start>.4</Start><Max>.75</Max></Trans_Elem><Trans_Elem><Min>0.1</Min><Start>.55</Start><Max>0.75</Max></Trans_Elem></Trans_Grid><MeshID>570</MeshID><VariantID>36</VariantID></Skin><Facial_Hair><Facial_Hair_Element><Hair>sideburns 3</Hair><Hair_Color>Light Brown</Hair_Color></Facial_Hair_Element><Facial_Hair_Element><Hair>eyebrows 1</Hair><Hair_Color>Light Brown</Hair_Color></Facial_Hair_Element></Facial_Hair><Eye_Mesh>pc_eyeballs.cmeshx</Eye_Mesh><Eye_Variant>eyes_blue</Eye_Variant><Hair_Color>Light Brown</Hair_Color>");
            builder.Append(ResourceGameFiles.PLAYER_PRESETS_SUFFIX);

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "player_presets.xtbl");
            using (StreamWriter outputFile = new StreamWriter(path, false)) {
                outputFile.Write(builder);
            }

            Console.WriteLine("Saved morphs in " + path);
        }

    }

}
