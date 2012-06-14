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

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Score
    {
        public string Points { get; set;}
        public Uri IconUri { get; set; }
        public string Message { get; set; }

        public static Score ParseJson(JToken score)
        {
            var s = new Score();

            string pts = Json.TryGetJsonProperty(score, "points");
            int intPts;
            if (int.TryParse(pts, out intPts))
            {
                string prefix = string.Empty;
                if (intPts < 0) prefix = "-";
                if (intPts > 0) prefix = "+";
                s.Points = prefix + intPts;
            }
            
            s.Message = Json.TryGetJsonProperty(score, "message");

            string u = Json.TryGetJsonProperty(score, "icon");
            try
            {
                if (u != null && u.StartsWith("/", StringComparison.InvariantCulture))
                {
                    u = "http://foursquare.com" + u;
                }
                if (u != null)
                {
                    s.IconUri = new Uri(u, UriKind.Absolute);
                }
            }
            catch (Exception)
            {
                // note: silent watson, or?
            }

            return s;
        }
    }
}
