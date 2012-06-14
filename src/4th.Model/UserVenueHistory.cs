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
    public class UserVenueHistory : FourSquareItemBase<UserAndCategoryLoadContext>
    {
        public UserVenueHistory()
            : base()
        {
        }

        public UserVenueHistory(UserAndCategoryLoadContext context)
            : base(context)
        {
        }

        private List<CompactVenue> _favoriteVenues;
        public List<CompactVenue> Venues
        {
            get { return _favoriteVenues; }
            set
            {
                _favoriteVenues = value;
                RaisePropertyChanged("Venues");
            }
        }

        public class UserVenueHistoryDataLoader : FourSquareDataLoaderBase<UserAndCategoryLoadContext>
        {

            public override LoadRequest GetLoadRequest(UserAndCategoryLoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/venuehistory",
                        
                        GeoMethodType.Optional,
                        
                                                "categoryId",
                        context.CategoryId ?? string.Empty
));
            }

            protected override object DeserializeCore(JObject json, Type objectType, UserAndCategoryLoadContext context)
            {
                try
                {
                    var b = new UserVenueHistory(context);

                    var venues = json["venues"];
                    if (venues != null)
                    {
                        var items = venues["items"];
                        if (items != null)
                        {
                            var list = new List<CompactVenue>();
                            foreach (var ven in items)
                            {
                                var v = ven["venue"];
                                if (v != null)
                                {
                                    var compactVenue = CompactVenue.ParseJson(v);
                                    if (compactVenue != null)
                                    {
                                        if (compactVenue.CheckinsCount > 0)
                                        {
                                            // Hacky!
                                            compactVenue.OverrideHereNow(compactVenue.CheckinsCount == 1 ? "1 check-in" : compactVenue.CheckinsCount + " check-ins");
                                        }

                                        list.Add(compactVenue);
                                    }
                                }
                            }
                            b.Venues = list;
                        }
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read the category favorites info.", e);
                }
            }
        }
    }
}
