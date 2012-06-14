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

using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class LeaderboardItemScores
    {
        public string Recent { get; set; }
        public string Max { get; set; }
        public string Checkins { get; set; }

        public static LeaderboardItemScores ParseJson(JToken item)
        {
            var scores = new LeaderboardItemScores();

            scores.Recent = Json.TryGetJsonProperty(item, "recent");
            scores.Max = Json.TryGetJsonProperty(item, "max");
            scores.Checkins = Json.TryGetJsonProperty(item, "checkinsCount");

            return scores;
        }
    }
}