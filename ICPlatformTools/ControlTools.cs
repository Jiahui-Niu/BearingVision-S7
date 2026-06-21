using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace ICPlatformTools
{
    public static class ControlTools
    {
        public class ComboxReflectData
        {
            public ComboxReflectData()
            {
            }

            public ComboxReflectData(Type type)
            {
                this.Type = type;
                this.Value = type.Name;
            }

            /// <summary>
            /// 列表项的值, 实际是类型的名称
            /// </summary>
            public string Value { get; set; }            

            /// <summary>
            /// 列表项需要创建的对象类型
            /// </summary>
            public Type Type { get; set; }

            public bool Equals(ComboxReflectData other)
            {
                return this.Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                if (obj.GetType() != this.GetType())
                    return false;

                return Equals((ComboxReflectData)obj);
            }

            public override string ToString()
            {
                return this.Value;
            }
        }

        public static void ComboBoxSetCurrentText(ComboBox cb, string text)
        {
            int index = cb.FindStringExact(text);
            if (index >= 0)
            {
                cb.SelectedIndex = index;
            }
        }

        public static void ComboBoxSetCurrentData<T>(ComboBox cb, T data)
        {
            for (int i = 0; i < cb.Items.Count; ++i)
            {
                var pair = (KeyValuePair<T, string>)cb.Items[i];
                if (pair.Key.Equals(data))
                {
                    cb.SelectedIndex = i;
                    return;
                }
            }
            cb.SelectedIndex = -1;
        }

        public static void ComboBoxSetDataSource<T>(ComboBox cb, Dictionary<T, string> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                cb.Items.Clear();
                return;
            }

            cb.DataSource = new BindingSource(dict, null);
            cb.DisplayMember = "Value";
            cb.ValueMember = "Key";
        }

        public static IEnumerable<Control> FindChild(this Control root, Func<Control, bool> predict, bool recursive = true)
        {
            List<Control> cons = new List<Control>();
            foreach (Control child in root.Controls)
            {
                if (predict(child))
                {
                    cons.Add(child);
                }

                if (recursive)
                {
                    var cc = child.FindChild(predict);
                    if (cc.Count() > 0)
                    {
                        cons.AddRange(cc);
                    }
                }
            }
            return cons;
        }

        public static T FindChild<T>(Control root, bool recursive = true) where T : Control
        {
            foreach (Control child in root.Controls)
            {
                if (child is T)
                {
                    return child as T;
                }

                if (recursive)
                {
                    var innerChild = FindChild<T>(child, true);
                    if (innerChild != null)
                    {
                        return innerChild;
                    }
                }
            }
            return null;
        }

        public static List<T> FindChildren<T>(Control root, bool recursive = true) where T : Control
        {
            var list = new List<T>();
            foreach (Control child in root.Controls)
            {
                if (child is T)
                {
                    list.Add(child as T);
                }
                list.AddRange(FindChildren<T>(child));
            }
            return list;
        }
        

        public static string GetSkinDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Skin");
        }

        public static Image GetImageFromFile(string name)
        {
            var path = Path.Combine(GetSkinDirectory(), name);
            try
            {
                return Image.FromFile(path);
            }
            catch
            {
            }
            return null;
        }

        public static void SetNumEditRange(this NumericUpDown ctrl, int min, int max)
        {
            ctrl.Minimum = min;
            ctrl.Maximum = max;
        }
    }
}
