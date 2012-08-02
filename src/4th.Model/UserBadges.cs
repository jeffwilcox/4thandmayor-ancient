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
    public class UserBadges : FourSquareItemBase<LoadContext>
    {
        public UserBadges()
            : base()
        {
        }

        public UserBadges(LoadContext context)
            : base(context)
        {
        }

        private List<BadgeGroup> _groups;
        public List<BadgeGroup> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                RaisePropertyChanged("Groups");
            }
        }

        public class UserBadgesDataLoader : FourSquareDataLoaderBase<LoadContext>
        {

            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/badges",
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new UserBadges(context);

                    var sets = json["sets"];
                    if (sets != null)
                    {
                        /*
                         type: "all"
    name: "all badges"
    image: {
    prefix: "http://foursquare.com/img/badge/"
    sizes: [
    24
    32
    48
    64
    ]
    name: "/allbadges.png"
                         * */
                        // TODO: V2: support icons for groups

                        var listOfGroups = new List<BadgeGroup>();
                        var badgesInGroups = new Dictionary<BadgeGroup, Dictionary<string, bool>>();

                        var groups = sets["groups"];
                        foreach (var group in groups)
                        {
                            BadgeGroup bbgg = new BadgeGroup();
                            bbgg.Name = Json.TryGetJsonProperty(group, "name");
                            bbgg.Type = Json.TryGetJsonProperty(group, "type");
                            // TODO: FUTURE: V2 badge group image support here

                            var items = group["items"];
                            if (items != null)
                            {
                                var contained = new Dictionary<string, bool>();
                                foreach (string s in items)
                                {
                                    contained[s] = true;
                                }
                                badgesInGroups[bbgg] = contained;
                                
                                listOfGroups.Add(bbgg);
                            }
                        }

                        //BadgeGroup all = lbg.Where(z => z.Type == "all").FirstOrDefault();

                        var badges = json["badges"];
                        if (badges != null)
                        {
                            foreach (var bb in badges)
                            {
                                var item = bb.First();

                                string key = Json.TryGetJsonProperty(item, "id");
                                if (!string.IsNullOrEmpty(key))
                                {
                                    var thisBadget = badges[key];
                                    Badge badge = Badge.ParseJson(thisBadget);

                                    foreach (var c in badgesInGroups)
                                    {
                                        var badgeGroup = c.Key;
                                        var contained = c.Value;

                                        if (contained.ContainsKey(key))
                                        {
                                            badgeGroup.Add(badge);
                                        }
                                    }
                                }
                            }
                        }

                        // TODO: P0 SHIP: TESTING NEEDED: If they have zero badges, which group will we have - probably not the right one!
                        // NOTE: V2: For now I only want the first group.
                        var tempList = new List<BadgeGroup>(1);
                        tempList.Add(listOfGroups[0]);
                        b.Groups = tempList; // listOfGroups;
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read the list of friends.", e);
                }
            }
        }
    }
}
