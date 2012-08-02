//
// Copyright (c) Jeff Wilcox
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

using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    using System.Collections.Generic;
    using Controls;

    public class MenuItem : NotifyPropertyChangedBase, ISpecializedComparisonString
    {
        public MenuItem()
        {
        }

        public string EntryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Prices { get; set; }

        public string PrimaryPrice
        {
            get
            {
                if (Prices != null && Prices.Count > 0)
                {
                    return Prices[0];
                }

                return null;
            }
        }

        public string SecondaryPrices
        {
            get
            {
                if (Prices != null && Prices.Count > 1)
                {
                    string s = string.Empty;
                    int c = Prices.Count;
                    for (int i = 1; i < c; i++)
                    {
                        s += Prices[i];
                        if (i < c - 1)
                        {
                            s += ", ";
                        }
                    }

                    return s;
                }

                return null;
            }
        }

        public static MenuItem ParseJson(JToken ejson)
        {
            MenuItem e = new MenuItem();

            e.EntryId = Json.TryGetJsonProperty(ejson, "entryId");
            e.Name = Json.TryGetJsonProperty(ejson, "name");
            e.Description = Json.TryGetJsonProperty(ejson, "description");

            var entries = ejson["prices"];
            if (entries != null)
            {
                List<string> prices = new List<string>();
                foreach (string s in entries)
                {
                    if (s != null)
                    {
                        prices.Add(s);
                    }
                }
                e.Prices = prices;
            }

            return e;
        }

        public string SpecializedComparisonString
        {
            get
            {
                return EntryId;
            }
        }
    }
}
