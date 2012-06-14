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
using Newtonsoft.Json.Linq;
using System.Windows.Media;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class LeaderboardItem
    {
        public CompactUser User { get; set; }

        public string Rank { get; set; }

        public LeaderboardItemScores Scores { get; set; }

        public override string ToString()
        {
            return Rank + " " + User;
        }

        // more of view type stuff:
        public bool IsSelf { get; private set; }

        public static LeaderboardItem ParseJson(JToken leaderboardItem)
        {
            var li = new LeaderboardItem();

            var rank = Json.TryGetJsonProperty(leaderboardItem, "rank");
            li.Rank = "#" + rank;

            var scores = leaderboardItem["scores"];
            if (scores != null)
            {
                li.Scores = LeaderboardItemScores.ParseJson(scores);
            }

            var userJson = leaderboardItem["user"];
            if (userJson != null)
            {
                li.User = CompactUser.ParseJson(userJson);

                if (li.User != null)
                {
                    if (li.User.Relationship == FriendStatus.Self)
                    {
                        li.IsSelf = true;
                    }
                }
            }

            return li;
        }
    }
}
