using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pr0MinerSharp.Utils
{
    public class JsonValidator
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
    }
}