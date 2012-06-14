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
    public class Checkins : FourSquareItemBase<LoadContext>
    {
        // We don't show people who no longer use foursquare. This helps speed
        // up the UI lists just ever so slightly.
        private static readonly TimeSpan MaxCheckinAge = TimeSpan.FromDays(31 + 7);

        public Checkins() : base()
        {
        }

        public Checkins(LoadContext context)
            : base(context)
        {
        }

        private List<CheckinsGroup> _groups;
        public List<CheckinsGroup> Groups
        {
            get
            {
                return _groups;
            }
            set
            {
                _groups = value;
                RaisePropertyChanged("Groups");
            }
        }

        public class CheckinsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                if (LocalCredentials.Current != null && string.IsNullOrEmpty(LocalCredentials.Current.UserId))
                {
                    throw new UserIgnoreException();
                    //throw new UserIntendedException("You must be signed into the service.", "Checkins.GetLoadRequest");
                }

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "checkins/recent",
                        GeoMethodType.Optional));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var nv = new Checkins(context);
                    nv.IgnoreRaisingPropertyChanges = true;
                    var checkins = (JArray)json["recent"];

                    var groups = new List<CheckinsGroup>();

                    var otherCities = new CheckinsGroup { Name = "Friends in other cities" };
                    var older = new CheckinsGroup { Name = "Older" };
                    var yesterday = new CheckinsGroup { Name = "Yesterday" };
                    var today = new CheckinsGroup { Name = "Today" };
                    var last3 = new CheckinsGroup { Name = "Last 3 Hours" };
                    var gps = new CheckinsGroup[]
                {
                    last3,
                    today,
                    yesterday,
                    older,
                    otherCities,
                };

                    var now = DateTime.UtcNow;
                    var nowAsLocal = now.ToLocalTime();
                    DateTime d3 = now - TimeSpan.FromHours(3);
                    DateTime dt = new DateTime(nowAsLocal.Year, nowAsLocal.Month, nowAsLocal.Day, 0, 0, 0, DateTimeKind.Local);
                    DateTime ydraw = nowAsLocal - TimeSpan.FromDays(1);
                    DateTime dy = new DateTime(ydraw.Year, ydraw.Month, ydraw.Day, 0, 0, 0, DateTimeKind.Local);
                    DateTime dTooMany = nowAsLocal - MaxCheckinAge;

                    foreach (JToken checkin in checkins)
                    {
                        Checkin c = Checkin.ParseJson(checkin);
                        var k = c.CreatedDateTime;

                        var kLocal = k.ToLocalTime();

                        string temp = c.Created;

                        var ku = k.ToUniversalTime();
                        System.Diagnostics.Debug.Assert(ku == k);

                        CheckinsGroup g = older;
                        if (c.IsInAnotherCity)
                        {
                            g = otherCities;
                        }
                        else if (k >= d3.ToUniversalTime())
                        {
                            g = last3;
                        }
                        else if (kLocal >= dt)
                        {
                            g = today;
                        }
                        else if (kLocal >= dy)
                        {
                            g = yesterday;
                        }

                        // If they have not checked in in a while.
                        if (kLocal < dTooMany)
                        {
                            continue;
                        }

                        g.Add(c);
                    }

                    foreach (var gp in gps)
                    {
                        if (gp.Count > 0)
                        {
                            groups.Add(gp);
                        }
                    }

                    nv.Groups = groups;

                    nv.IgnoreRaisingPropertyChanges = false;
                    nv.IsLoadComplete = true;
                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read information about recent checkins.", e);
                }
            }
        }
    }
}
