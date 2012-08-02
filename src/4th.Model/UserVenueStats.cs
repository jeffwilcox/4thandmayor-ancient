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
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class UserVenueStats : FourSquareItemBase<LoadContext>
    {
        public UserVenueStats()
            : base()
        {
        }

        public UserVenueStats(LoadContext context)
            : base(context)
        {
        }

        private List<UserVenueStatistic> _favoriteVenues;
        public List<UserVenueStatistic> FavoriteVenues
        {
            get { return _favoriteVenues; }
            set
            {
                _favoriteVenues = value;
                RaisePropertyChanged("FavoriteVenues");
            }
        }

        private List<UserCategoryStatistic> _favoriteCategories;
        public List<UserCategoryStatistic> FavoriteCategories
        {
            get { return _favoriteCategories; }
            set
            {
                _favoriteCategories = value;
                RaisePropertyChanged("FavoriteCategories");
            }
        }

        public class UserVenueStatsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {

            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/venuestats",
                        GeoMethodType.Optional));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new UserVenueStats(context);

                    var venues = json["venues"];
                    if (venues != null)
                    {
                        List<UserVenueStatistic> list = new List<UserVenueStatistic>();
                        foreach (var ven in venues)
                        {
                            var v = UserVenueStatistic.ParseJson(ven);
                            if (v != null)
                            {
                                list.Add(v);
                            }
                        }

                        b.FavoriteVenues = list;
                    }

                    var categories = json["categories"];
                    if (categories != null)
                    {
                        List<UserCategoryStatistic> list = new List<UserCategoryStatistic>();
                        foreach (var cat in categories)
                        {
                            var c = UserCategoryStatistic.ParseJson(cat);
                            if (c != null)
                            {
                                list.Add(c);
                            }
                        }

                        b.FavoriteCategories = list;
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read the venue stats.", e);
                }
            }
        }
    }
}
