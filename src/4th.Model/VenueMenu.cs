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
using System.Collections.Generic;
using System.Linq;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class VenueMenu : FourSquareItemBase<LoadContext>
    {
        public VenueMenu()
            : base()
        {
        }

        public VenueMenu(LoadContext context)
            : base(context)
        {
        }

        //public Venue ParentVenue
        //{
        //    get
        //    {
        //        return DataManager.Current.Load<Venue>(new LoadContext(LoadContext.Identity));
        //    }
        //}

        private string _provider;
        public string Provider
        {
            get
            {
                return _provider;
            }
            set
            {
                _provider = value;
                RaisePropertyChanged("Provider");
            }
        }

        private string _attributionText;
        public string AttributionText
        {
            get { return _attributionText; }
            set
            {
                _attributionText = value;
                RaisePropertyChanged("AttributionText");
            }
        }

        private Uri _attributionImage;
        public Uri AttributionImage
        {
            get
            { 
                return _attributionImage; 
            }
            set
            {
                _attributionImage = value;
                RaisePropertyChanged("AttributionImage");
            }
        }

        private Uri _attributionLink;
        public Uri AttributionLink
        {
            get
            {
                return _attributionLink;
            }
            set
            {
                _attributionLink = value;
                RaisePropertyChanged("AttributionLink");
            }
        }

        // not parsed.
        private string _venueName;
        public string VenueName
        {
            get { return _venueName; }
            set
            {
                _venueName = value;
                RaisePropertyChanged("VenueName");
            }
        }

        private List<Menu> _menus;
        public List<Menu> Menus
        {
            get { return _menus; }
            set
            {
                _menus = value;
                RaisePropertyChanged("Menus");
            }
        }

        public class VenueMenuDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "venues/" + id + "/menu",
                        GeoMethodType.Optional));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new VenueMenu(context);

                    var menu = json["menu"];
                    if (menu != null)
                    {
                        var provider = menu["provider"];
                        if (provider != null)
                        {
                            b.Provider = Json.TryGetJsonProperty(provider, "name");
                            b.AttributionImage = Json.TryGetUriProperty(provider, "attributionImage");
                            b.AttributionLink = Json.TryGetUriProperty(provider, "attributionLink");
                            b.AttributionText = Json.TryGetJsonProperty(provider, "attributionText");
                        }

                        var menus = menu["menus"];
                        if (menus != null)
                        {
                            var items = menus["items"];
                            var list = new List<Menu>();
                            if (items != null)
                            {
                                foreach (var item in items)
                                {
                                    var thisMenu = Menu.ParseJson(item, context.Identity as string);
                                    if (thisMenu != null)
                                    {
                                        list.Add(thisMenu);
                                    }
                                }
                            }
                            b.Menus = list;
                        }
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read the venue's menu.", e);
                }
            }
        }
    }
}
