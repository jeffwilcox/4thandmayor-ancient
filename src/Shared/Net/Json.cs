//
// Copyright (c) 2010-2011 Jeff Wilcox
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Globalization;
using Newtonsoft.Json.Linq;
using System;

namespace JeffWilcox
{
    public static class Json
    {
        public static string GetPrettyInt(int i)
        {
            return string.Format(CultureInfo.CurrentUICulture, "{0:N0}", i);
        }

        // Defaults to false!
        public static bool TryGetJsonBool(JToken token, string key)
        {
            bool v = false;
            string sv = TryGetJsonProperty(token, key);
            if (sv != null)
            {
                if (sv == "true" || sv == "True" || sv == "TRUE")
                {
                    v = true;
                }
            }
            return v;
        }

        public static Uri TryGetUriProperty(JToken token, string key)
        {
            string str = TryGetJsonProperty(token, key);
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    var uri = new Uri(str, UriKind.Absolute);
                    return uri;
                }
                catch 
                { }
            }

            return null;
        }

        public static string TryGetJsonProperty(JToken token, string key)
        {
            System.Diagnostics.Debug.Assert(token != null, "The token is null, looking for " + key);
            if (token != null)
            {
                object o = token[key];
                if (o != null)
                {
                    JValue jv = (JValue)o;
                    string s = jv.Value.ToString();
                    if (s != null && s.Length == 0)
                    {
                        return null;
                    }
                    return s;
                }
            }
            return null;
        }
    }
}
