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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class LeaderboardNotification : Notification
    {
        public string Message { get; private set; }

        public List<LeaderboardItem> Leaderboard { get; private set; }

        // TODO: Scores and total from this entry... see how official app does it.


        public LeaderboardNotification(JToken item)
        {
            Message = Checkin.SanitizeString(Json.TryGetJsonProperty(item, "message"));

            var leaders = new List<LeaderboardItem>();
            var lj = item["leaderboard"];
            if (lj != null)
            {
                foreach (var victor in lj)
                {
                    var wolverine = LeaderboardItem.ParseJson(victor);
                    if (wolverine != null)
                    {
                        leaders.Add(wolverine);
                    }
                }
            }

            Leaderboard = leaders;
        }
    }
}
