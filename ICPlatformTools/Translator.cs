using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ICPlatformTools
{
    public interface IMultiLanguage
    {      
        void TranslateUI();
    }

    public static class LanguageManager
    {
        private static Translator m_translator = new Translator();

        public static string Translate(string key)
        {
            return m_translator.GetValue(key);
        }

        public static void SetLanguage(string name)
        {
            m_translator.Load(name);
        }
    }

    public class Translator
    {
        private Dictionary<string, string> m_resources = new Dictionary<string, string>();

        public Translator()
        {
            this.InvalidValue = "???";
        }

        public string InvalidValue { get; set; }

        public bool Load(string langName)
        {
            if (string.IsNullOrEmpty(langName))
            {
                langName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            }

            int count = 0;
            var files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language"), langName + "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file, Encoding.UTF8);
                    if (!string.IsNullOrEmpty(content))
                    {
                        var dic = JsonHelper.DeSerialize<Dictionary<string, string>>(content);
                        count += dic.Count(s =>
                        {
                            if (!m_resources.ContainsKey(s.Key))
                            {
                                m_resources.Add(s.Key, s.Value);
                                return true;
                            }
                            return false;
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error("load language file error.", ex);
                }
            }
            if (count > 0)
            {
                return true;
            }
            return false;

            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language", langName);
            //try
            //{
            //    var content = File.ReadAllText(filePath, Encoding.UTF8);
            //    if (!string.IsNullOrEmpty(content))
            //    {
            //        m_resources = JsonHelper.DeSerialize<Dictionary<string, string>>(content);
            //        return true;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Log.Error("load language file error.", ex);
            //}
            //return false;
        }

        public string GetValue(string key)
        {
            string value = null;
            if (m_resources.TryGetValue(key, out value))
            {
                return value;
            }
            return InvalidValue;
        }
    }
}
