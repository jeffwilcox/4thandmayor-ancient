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
    /// <summary>
    /// Represents the leaderboard for the active, signed in Foursquare user.
    /// </summary>
    [CachePolicy(CachePolicy.CacheThenRefresh, FiveMinutes)]
    public class Leaderboard : FourSquareItemBase<LeaderboardLoadContext>
    {
        public Leaderboard(LeaderboardLoadContext context)
            : base(context)
        {
        }

        public Leaderboard() { }

        private List<LeaderboardItem> _board;
        public List<LeaderboardItem> Board
        {
            get { return _board; }
            set
            {
                _board = value;
                RaisePropertyChanged("Board");
            }
        }

        /// <summary>
        /// Data loader that communicates with the Foursquare web services to
        /// retrieve the JSON-P for the active user's leaderboard.
        /// </summary>
        public class LeaderboardDataLoader : FourSquareDataLoaderBase<LeaderboardLoadContext>
        {
            public override LoadRequest GetLoadRequest(LeaderboardLoadContext context, Type objectType)
            {
                var parameters = new List<string>();
                if (context.Neighbors != null)
                {
                    parameters.Add("neighbors");
                    parameters.Add(context.Neighbors);
                }

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "users/leaderboard",
                    GeoMethodType.Optional,
                    parameters.ToArray()));
            }

            /// <summary>
            /// Parses the JSON response to retrieve the leaderboard, parsing
            /// of individual items are handled by their compact object
            /// parse routines.
            /// </summary>
            protected override object DeserializeCore(JObject json, Type objectType, LeaderboardLoadContext context)
            {
                try
                {
                    var nv = new Leaderboard(context);

                    var results = json["leaderboard"];
                    var b = new List<LeaderboardItem>();
                    if (results != null)
                    {
                        var items = results["items"];
                        if (items != null)
                        {
                            foreach (var entry in items)
                            {
                                var u = LeaderboardItem.ParseJson(entry);
                                if (u != null)
                                {
                                    b.Add(u);
                                }
                            }
                        }
                    }

                    nv.Board = b;

                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "The leaderboard could not be refreshed right now.", e);
                }
            }
        }
    }
}
