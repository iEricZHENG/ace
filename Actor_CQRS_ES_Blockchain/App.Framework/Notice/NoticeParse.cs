using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Framework.Notice
{
    public class NoticeParse
    {
        static Regex tplRegex = new Regex(@"(?<pre>.*?)\{\{(?<key>.+?)\}\}(?<next>[^\{]*)", RegexOptions.Compiled);
        static ConcurrentDictionary<int, List<ParseInfo>> tplDict = new ConcurrentDictionary<int, List<ParseInfo>>();
        public static List<ParseInfo> ParseTpl(string tpl)
        {
            var hashCode = tpl.GetHashCode();
            List<ParseInfo> result;
            if (!tplDict.TryGetValue(hashCode, out result))
            {
                result = new List<ParseInfo>();
                if (!string.IsNullOrEmpty(tpl))
                {
                    var match = tplRegex.Matches(tpl);
                    if (match.Count > 0)
                    {
                        foreach (Match m in match)
                        {
                            var parseInfo = new ParseInfo();
                            parseInfo.PreText = m.Groups["pre"].Value;
                            parseInfo.Key = m.Groups["key"].Value;
                            parseInfo.NextText = m.Groups["next"].Value;
                            result.Add(parseInfo);
                        }
                    }
                    else
                    {
                        result.Add(new ParseInfo() { PreText = tpl, Key = string.Empty });
                    }
                }
            }
            return result;
        }
        public static string ParseTpl(string tpl, Dictionary<string, string> values)
        {
            var parseResult = ParseTpl(tpl);
            var build = new StringBuilder();
            foreach (var parse in parseResult)
            {
                if (!string.IsNullOrEmpty(parse.PreText))
                    build.Append(parse.PreText);
                string value;
                if (values.TryGetValue(parse.Key, out value))
                {
                    build.Append(value);
                }
                if (!string.IsNullOrEmpty(parse.NextText))
                    build.Append(parse.NextText);
            }
            return build.ToString();
        }
    }
    public class ParseInfo
    {
        public string PreText { get; set; }
        public string Key { get; set; }
        public string NextText { get; set; }
    }
}
