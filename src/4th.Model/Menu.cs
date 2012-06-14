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

    public class Menu : NotifyPropertyChangedBase, ISpecializedComparisonString
    {
        public Menu()
        {
        }

        public string MenuId { get; set; }

        public string Name { get; set; }

        public Uri LocalUri { get; set; }

        public string Description { get; set; }

        public string ParentVenueId { get; set; }

        private List<MenuSection> _sections;
        public List<MenuSection> Sections
        {
            get
            {
                return _sections;
            }
            set
            {
                _sections = value;
                RaisePropertyChanged("Sections");
            }
        }

        public static Menu ParseJson(JToken ejson, string venueId)
        {
            Menu e = new Menu();

            e.ParentVenueId = venueId;

            e.MenuId = Json.TryGetJsonProperty(ejson, "menuId");
            e.Name = Json.TryGetJsonProperty(ejson, "name");
            e.Description = Json.TryGetJsonProperty(ejson, "description");
            if (e.Description != null && e.Description.Length == 0)
            {
                e.Description = null;
            }

            e.LocalUri = new Uri(string.Format(CultureInfo.InvariantCulture,
                "/JeffWilcox.FourthAndMayor.Place;component/VenueMenu.xaml?venueid={0}&menuid={1}",
                System.Net.HttpUtility.UrlEncode(e.ParentVenueId),
                System.Net.HttpUtility.UrlEncode(e.MenuId)
                ), UriKind.Relative);

            var entries = ejson["entries"];
            if (entries != null)
            {
                var items = entries["items"];
                if (items != null)
                {
                    var li = new List<MenuSection>();
                    foreach (var item in items)
                    {
                        var es = MenuSection.ParseJson(item);
                        if (es != null)
                        {
                            li.Add(es);
                        }
                    }
                    e.Sections = li;
                }
            }

            return e;
        }

        public string SpecializedComparisonString
        {
            get
            {
                return MenuId;
            }
        }
    }
}
