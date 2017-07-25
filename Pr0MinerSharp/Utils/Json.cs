using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pr0MinerSharp.Utils
{
    public class XJson
    {
        public static bool TryParseJson(string strInput, out JObject value)
        {
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) ||
                (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    value = JsonConvert.DeserializeObject(strInput) as JObject;
                    return value != null;
                }
                catch
                {
                    // ignored
                }
            }

            value = default(JObject);
            return false;
        }

        /*public static bool TryParseJson<T>(string strInput, out T value)
        {
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) ||
                (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    value = JsonConvert.DeserializeObject<T>(strInput);
                    return true;
                }
                catch
                {
                    // ignored
                }
            }

            value = default(T);
            return false;
        }*/
    }
}