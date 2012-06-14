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
using System.Diagnostics;
using System.Globalization;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class TrendingVenues : FourSquareItemBase<TrendingVenuesLoadContext>
    {
        public TrendingVenues()
            : base()
        {
        }

        public TrendingVenues(TrendingVenuesLoadContext context)
            : base(context)
        {
        }

        public string TrendingHeader
        {
            get
            {
                string count = string.Empty;
                //if (_venues != null && _venues.Count > 0)
                //{
                //    count = "(" + _venues.Count.ToString() + " place" + 
                //        (_venues.Count > 1 ? "s" : "")
                //        + ")";
                //}

                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Trending places within {0}{1}", 
                    ImperialUnits.GetMetersInProperUnitsDisplay(LoadContext.RadiusMeters),
                    count);
            }
        }

        private List<CompactVenue> _venues;
        public List<CompactVenue> Venues
        {
            get { return _venues; }
            set
            {
                _venues = value;
                RaisePropertyChanged("Venues");
                RaisePropertyChanged("TrendingHeader");
            }
        }

        public class TrendingVenuesDataLoader : FourSquareDataLoaderBase<TrendingVenuesLoadContext>
        {
            public override LoadRequest GetLoadRequest(TrendingVenuesLoadContext context, Type objectType)
            {
                Debug.Assert(context.Limit <= 100);
                Debug.Assert(context.RadiusMeters > 0);
                //Debug.Assert(context.RadiusMeters <= 5000);

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "venues/trending",
                        GeoMethodType.Required,

                        "limit",
                        context.Limit.ToString(),

                        "radius",
                        context.RadiusMeters.ToString()

                        ));
            }

            protected override object DeserializeCore(JObject json, Type objectType, TrendingVenuesLoadContext context)
            {
                try
                {
                    var nv = new TrendingVenues(context);
                    nv.IgnoreRaisingPropertyChanges = true;

                    var venues = json["venues"];
                    var cvl = new List<CompactVenue>();
                    if (venues != null)
                    {
                        foreach (var venue in venues)
                        {
                            CompactVenue cv = CompactVenue.ParseJson(venue);
                            cvl.Add(cv);
                        }
                    }

                    nv.Venues = cvl;

                    nv.IgnoreRaisingPropertyChanges = false;
                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read information about nearby trending places.", e);
                }
            }
        }
    }
}