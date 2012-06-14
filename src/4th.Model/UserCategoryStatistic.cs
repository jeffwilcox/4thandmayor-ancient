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
using System.Globalization;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class UserCategoryStatistic : ISpecializedComparisonString
    {
        public string SpecializedComparisonString
        {
            get { return Category != null ? Category.CategoryId : string.Empty; }
        }

        public Category Category { get; set; }
        public int VenueCount { get; set; }

        public string VenueCountText
        {
            get
            {
                if (VenueCount > 0)
                {
                    if (VenueCount == 1)
                    {
                        return "1 Place";
                    }

                    return VenueCount + " Different Places";
                }
                return null;
            }
        }

        public static UserCategoryStatistic ParseJson(JToken json)
        {
            var uvs = new UserCategoryStatistic();

            string bh = Json.TryGetJsonProperty(json, "venueCount");
            if (bh != null)
            {
                int i;
                if (int.TryParse(bh, out i))
                {
                    uvs.VenueCount = i;
                }
            }

            var v = json["category"];
            if (v != null)
            {
                var cat = Category.ParseJson(v);
                if (cat != null)
                {
                    uvs.Category = cat;
                }
            }

            return uvs;
        }
    }
}
