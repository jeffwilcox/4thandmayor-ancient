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
    public class RecommendedCompactVenue
    {
        public override string ToString()
        {
            if (Venue == null)
            {
                return base.ToString();
            }
            return Venue.ToString();
        }

        // Reasons
        public List<RecommendationReason> Reasons { get; set; }

        public RecommendationReason PrimaryReason { get; set; }

        public List<RecommendationReason> SecondaryReasons { get; set; }

        // Venue
        public CompactVenue Venue { get; set;}

        public List<Tip> Tips { get; set; }

        public Tip PrimaryTip { get; set; }

        public static RecommendedCompactVenue ParseJson(JToken jsonRecommendation)
        {
            var recommendation = new RecommendedCompactVenue();

            var reasons = jsonRecommendation["reasons"];
            var reasonList = new List<RecommendationReason>();
            if (reasons != null)
            {
                var items = reasons["items"];
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        string sType = Json.TryGetJsonProperty(item, "type");
                        string sMessage = Json.TryGetJsonProperty(item, "message");
                        reasonList.Add(new RecommendationReason
                                           {
                                               Reason = sType,
                                               Message = sMessage,
                                           });
                    }

                    if (reasonList.Count > 0)
                    {
                        recommendation.PrimaryReason = reasonList[0];

                        if (reasonList.Count > 1)
                        {
                            List<RecommendationReason> otherReasons = new List<RecommendationReason>();
                            for (int i = 1; i < reasonList.Count; i++)
                            {
                                otherReasons.Add(reasonList[i]);
                            }
                            recommendation.SecondaryReasons = otherReasons;
                        }
                    }
                }
            }
            recommendation.Reasons = reasonList;

            var venue = jsonRecommendation["venue"];
            if (venue != null)
            {
                recommendation.Venue = CompactVenue.ParseJson(venue);
            }

            var todos = jsonRecommendation["tips"];
            var tips = new List<Tip>();
            if (todos != null)
            {
                foreach (var todo in todos)
                {
                    tips.Add(Tip.ParseJson(todo));
                }
                if (tips.Count > 0)
                {
                    recommendation.PrimaryTip = tips[0];
                }
            }
            recommendation.Tips = tips;

            return recommendation;
        }
    }
}
