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
using AgFx;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class VenueSearch : FourSquareItemBase<VenueSearchLoadContext>
    {
        public VenueSearch(VenueSearchLoadContext context) : base(context)
        {
        }

        public VenueSearch() { }

        private CompactVenueList _groups;
        public CompactVenueList Results
        {
            get { return _groups; }
            set
            {
                _groups = value;
                RaisePropertyChanged("Results");
            }
        }

        public class VenueSearchDataLoader : FourSquareDataLoaderBase<VenueSearchLoadContext>
        {
            public override LoadRequest GetLoadRequest(VenueSearchLoadContext context, Type objectType)
            {
                Debug.Assert(context.Limit <= 100);

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "venues/search",
                    GeoMethodType.Required,

                    "query",
                    context.Query, // will be null if in the no-query mode.

                    // consider exposing other intents on the context
                    "intent",
                    "checkin",

                    "limit",
                    context.Limit.ToString(CultureInfo.InvariantCulture)
                    ));
            }

            protected override object DeserializeCore(JObject json, Type objectType, VenueSearchLoadContext context)
            {
                try
                {
                    var nv = new VenueSearch(context);
                    nv.IgnoreRaisingPropertyChanges = true;

                    // NOTE: BREAKING CHANGE IN THE SEARCH API!!!

                    var venues = json["venues"];
                    if (venues != null)
                    {
                        //nv.Groups = new List<CompactVenueList>(groups.Count);
                        nv.Results = new CompactVenueList();
                        foreach (JToken result in venues)
                        {
                            //string groupType = Json.TryGetJsonProperty(group, "type");
                            //var groupName = (string)group["name"];
                            //var venues = /*(JArray)*/ group["items"];

                            //var list = new CompactVenueList
                            //{
                                //Name = groupName,
                                //Type = groupType,
                            //};

                            //if (venues != null)
                            //{
                                //foreach (var v in venues)
                                //{

                                    CompactVenue venue = CompactVenue.ParseJson(result);
                                    //list.Add(venue);
                                    nv.Results.Add(venue);
                                //}
                            //}

                            //nv.Groups.Add(list);
                        }
                    }

                    nv.IgnoreRaisingPropertyChanges = false;
                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "Your place search could not be completed at this time.", e);
                }
            }
        }
    }
}
