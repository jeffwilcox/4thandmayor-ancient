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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class ScoreNotification : Notification
    {
        public List<Score> Scores { get; private set; }

        public string TotalScore { get; private set; }

        public ScoreNotification(JToken item)
        {
            string tot = Json.TryGetJsonProperty(item, "total");
            if (tot != null)
            {
                int tt;
                if (int.TryParse(tot, out tt))
                {
                    string prefix = string.Empty;
                    if (tt < 0) prefix = "-";
                    if (tt > 0) prefix = "+";
                    TotalScore = prefix + tt;
                }
            }

            var scores = new List<Score>();
            var scoresJson = item["scores"];
            if (scoresJson != null)
            {
                foreach (var score in scoresJson)
                {
                    Score s = Score.ParseJson(score);
                    if (s != null)
                    {
                        scores.Add(s);
                    }
                }
            }

            Scores = scores;
        }
    }
}
