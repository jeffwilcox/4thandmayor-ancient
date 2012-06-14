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

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    using Controls;
    using System.Collections.Generic;

    public class MenuSection : List<MenuItem>, ISpecializedComparisonString, IName
    {
        public MenuSection()
        {
        }

        public string SectionId { get; set; }

        public string Name { get; set; }

        //public List<MenuItem> Items { get; set; }

        public static MenuSection ParseJson(JToken ejson)
        {
            MenuSection e = new MenuSection();

            e.SectionId = Json.TryGetJsonProperty(ejson, "sectionId");
            e.Name = Json.TryGetJsonProperty(ejson, "name");

            var entries = ejson["entries"];
            if (entries != null)
            {
                var items = entries["items"];
                if (items != null)
                {
                    //var li = new List<MenuItem>();
                    foreach (var item in items)
                    {
                        var es = MenuItem.ParseJson(item);
                        if (es != null)
                        {
                            /*li.*/
                            e.Add(es);
                        }
                    }
                    //e.Items = li;
                }
            }

            return e;
        }

        public string SpecializedComparisonString
        {
            get
            {
                return SectionId;
            }
        }
    }
}
