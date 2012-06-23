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
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class UserMayorships : FourSquareItemBase<LoadContext>
    {
        public UserMayorships()
            : base()
        {
        }

        public UserMayorships(LoadContext context)
            : base(context)
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

        public class UserMayorshipsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {

            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/mayorships",
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new UserMayorships(context);

                    var sets = json["mayorships"];
                    if (sets != null)
                    {
                        List<CompactVenue> venues = new List<CompactVenue>();
                        var groups = sets["items"];
                        if (groups != null)
                        foreach (var item in groups)
                        {
                            var venue = item["venue"];
                            if (venues != null)
                            {
                                var cv = CompactVenue.ParseJson(venue);
                                if (cv != null)
                                {
                                    venues.Add(cv);
                                }
                            }
                        }
                        b.Venues = venues;
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read the mayorships list.", e);
                }
            }
        }
    }
}
