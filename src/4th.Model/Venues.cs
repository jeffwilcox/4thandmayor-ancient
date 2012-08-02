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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using AgFx;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class NearbyVenues : FourSquareItemBase<LoadContext>
    {
        public NearbyVenues() : base()
        {
        }

        public NearbyVenues(LoadContext context) : base(context)
        {
        }

        private List<CompactVenue> _venues;
        public List<CompactVenue> Venues
        {
            get { return _venues; }
            set
            {
                _venues = value;
                RaisePropertyChanged("Venues");
            }
        }

        private GeocodeService _geocode;
        public GeocodeService Geocode
        {
            get
            {
                if (_geocode == null)
                {
                    _geocode = GeocodeService.Instance;
                }

                return _geocode;
            }
        }

        // Coupling!
        public ILocationEnabled AppSettings
        {
            get
            {
                return ((IExposeSettings)(Application.Current)).LocationEnabled;
                //FourSquareApp.Instance.Settings; 
            }
        }

        public class VenuesDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                // The "MORE" button should temporarily increase this...
                int limit = 30;

                Debug.Assert(limit >= 0 && limit < 50, "0-50 is the valid range for the limit.");

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "venues/search",
                        GeoMethodType.Required,

                        "intent",
                        "checkin",

                        "limit",
                        limit.ToString(CultureInfo.InvariantCulture)));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    // This one really is a duplicate of venue search.

                    var nv = new NearbyVenues(context);
                    nv.IgnoreRaisingPropertyChanges = true;

                    // Grouping, part of fsq2-3.
                    //var groups = json["groups"];
                    //if (groups != null)
                    //{
                    //    nv.Groups = new List<CompactVenueList>();
                    //    foreach (var group in groups)
                    //    {
                    //        string groupType = Json.TryGetJsonProperty(group, "type");
                    //        var groupName = (string)group["name"];
                    //        var venues = group["items"];

                    //        var list = new CompactVenueList
                    //        {
                    //            Name = groupName,
                    //            Type = groupType,
                    //        };

                    //        if (venues != null)
                    //        {
                    //            foreach (var v in venues)
                    //            {
                    //                CompactVenue venue = CompactVenue.ParseJson(v);
                    //                list.Add(venue);
                    //            }
                    //        }

                    //        nv.Groups.Add(list);
                    //    }
                    //}
                    //else
                    //{
                        // Latest API in June 2011, no longer grouped.
                        var vs = json["venues"];
                        if (vs != null)
                        {
                            var list = new List<CompactVenue>();
                            //var cvl = new CompactVenueList { Name = "Nearby", Type = "nearby" };
                            foreach (var ven in vs)
                            {
                                var cv = CompactVenue.ParseJson(ven);
                                if (cv != null)
                                {
                                    //cvl.Add(cv);
                                    list.Add(cv);
                                }
                            }
                            nv.Venues = list;
                        }
                    //}

                    nv.IgnoreRaisingPropertyChanges = false;
                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read information about nearby places.", e);
                }
            }
        }
    }
}