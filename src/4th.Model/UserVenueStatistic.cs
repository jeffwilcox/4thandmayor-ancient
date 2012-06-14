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

using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class UserVenueStatistic : ISpecializedComparisonString
    {
        public string SpecializedComparisonString
        {
            get { return Venue != null ? Venue.VenueId : string.Empty; }
        }

        public CompactVenue Venue { get; set; }
        public string BeenHereMessage { get; set; }
        public int BeenHere { get; set; }

        public static UserVenueStatistic ParseJson(JToken json)
        {
            var uvs = new UserVenueStatistic();

            uvs.BeenHereMessage = Json.TryGetJsonProperty(json, "beenHereMessage");

            string bh = Json.TryGetJsonProperty(json, "beenHere");
            if (bh != null)
            {
                int i;
                if (int.TryParse(bh, out i))
                {
                    uvs.BeenHere = i;
                }
            }
            
            var v = json["venue"];
            if (v != null)
            {
                var venue = CompactVenue.ParseJson(v);
                if (venue != null)
                {
                    // Overrides!
                    if (!string.IsNullOrEmpty(uvs.BeenHereMessage))
                    {
                        venue.OverrideHereNow(uvs.BeenHereMessage);
                    }

                    uvs.Venue = venue;
                }
            }

            return uvs;
        }
    }
}
