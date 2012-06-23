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
    public class ExploreVenues : FourSquareItemBase<ExploreVenuesLoadContext>
    {
        private List<Keyword> _relatedKeywords;
        public List<Keyword> RelatedKeywords
        {
            get { return _relatedKeywords; }
            set
            {
                _relatedKeywords = value;
                RaisePropertyChanged("RelatedKeywords");
            }
        }

        private List<RecommendedCompactVenueList> _groups;
        public List<RecommendedCompactVenueList> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                RaisePropertyChanged("Groups");
            }
        }

        public ExploreVenues() : base()
        {
        }

        public ExploreVenues(ExploreVenuesLoadContext context) : base(context)
        {
        }

        public class ExploreVenuesDataLoader : FourSquareDataLoaderBase<ExploreVenuesLoadContext>
        {
            public override LoadRequest GetLoadRequest(ExploreVenuesLoadContext context, Type objectType)
            {
                var components = new List<string>();

                string section = context.Section;
                if (section != null)
                {
                    components.Add("section");
                    components.Add(section);
                }

                components.Add("radius");
                components.Add(context.RadiusMeters.ToString(CultureInfo.InvariantCulture));

                string query = context.Query;
                if (query != null)
                {
                    components.Add("query");
                    components.Add(query);
                }

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "venues/explore",
                    GeoMethodType.Required, components.ToArray()));
            }

            protected override object DeserializeCore(JObject json, Type objectType, ExploreVenuesLoadContext context)
            {
                try
                {
                    var nv = new ExploreVenues(context);

                    var keywords = json["keywords"];
                    var kwl = new List<Keyword>();
                    if (keywords != null)
                    {
                        var items = keywords["items"];
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                string displayName = Json.TryGetJsonProperty(item, "displayName");
                                string value = Json.TryGetJsonProperty(item, "keyword");
                                var keyword = new Keyword(context.Section, displayName, value);

                                // special hard-coding for now
                                if (context.SectionDisplayName != null)
                                {
                                    keyword.SectionDisplayName = context.SectionDisplayName;
                                }

                                kwl.Add(keyword);
                            }
                        }
                    }
                    nv.RelatedKeywords = kwl;

                    var groups = json["groups"];
                    var grps = new List<RecommendedCompactVenueList>();

                    bool isMemoryLimited = FourSquare.Instance.IsLowMemoryDevice;
                    
                    if (groups != null)
                    {
                        foreach (var group in groups)
                        {
                            string groupType = Json.TryGetJsonProperty(group, "type");
                            string groupName = Json.TryGetJsonProperty(group, "name");

                            var thisGroup = new RecommendedCompactVenueList
                                                                        {
                                                                         Name = groupName,
                                                                         Type = groupType,
                                                                        };

                            var itms = group["items"];
                            if (itms != null)
                            {
                                int itemCount = 0;
                                foreach (var item in itms)
                                {
                                    // LIMITED MEMORY EXPERIENCE: ONLY 3 items.
                                    ++itemCount;
                                    if (isMemoryLimited && itemCount == 4)
                                    {
                                        break;
                                    }

                                    if (item != null)
                                    {
                                        var recommendation = RecommendedCompactVenue.ParseJson(item);
                                        if (recommendation != null)
                                        {
                                            thisGroup.Add(recommendation);
                                        }
                                    }
                                }
                            }

                            grps.Add(thisGroup);
                        }
                    }
                    nv.Groups = grps;

                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "We couldn't explore right now, please try again later.", e);
                }
            }
        }
    }
}
