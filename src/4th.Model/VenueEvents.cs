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
    public class VenueEvents : FourSquareItemBase<LoadContext>
    {
        public VenueEvents()
            : base()
        {
        }

        public VenueEvents(LoadContext context)
            : base(context)
        {
        }

        public Venue ParentVenue
        {
            get
            {
                return DataManager.Current.Load<Venue>(new LoadContext(LoadContext.Identity));
            }
        }

        private string _summary;
        public string Summary
        {
            get
            {
                return _summary;
            }
            set
            {
                _summary = value;
                RaisePropertyChanged("Summary");
            }
        }

        private List<Event> _items;
        public List<Event> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public class VenueEventsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "venues/" + id + "/events",
                        GeoMethodType.Optional));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new VenueEvents(context);

                    var sets = json["events"];
                    if (sets != null)
                    {
                        b.Summary = Json.TryGetJsonProperty(sets, "summary");

                        var items = sets["items"];
                        if (items != null)
                        {
                            var list = new List<Event>();
                            foreach (var s in items)
                            {
                                Event c = Event.ParseJson(s);
                                list.Add(c);
                            }
                            b.Items = list;
                        }
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read the venue events.", e);
                }
            }
        }
    }
}
