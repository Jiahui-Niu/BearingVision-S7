using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public static class JsonHelper
    {
        public static string SerializeObject(object entity, bool beautified = false)
        {
            if (beautified)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(entity, Newtonsoft.Json.Formatting.Indented);
            }
            else
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(entity);
            }
        }

        public static T DeSerialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static string BeautifyJson(string json)
        {
            Newtonsoft.Json.Linq.JToken token = json;
            return token.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}
