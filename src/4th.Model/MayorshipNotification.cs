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
using Newtonsoft.Json.Linq;
using System.Windows;

namespace JeffWilcox.FourthAndMayor.Model
{
    public enum MayorshipNotificationType
    {
        NoChange,
        New,
        Stolen,
    }
    public class MayorshipNotification : Notification
    {
        public int Checkins { get; private set; } // during the last 60 days
        public string Message { get; private set; }
        public Uri Image { get; private set; }
        public CompactUser User { get; private set; }
        public int DaysBehindTheMayor { get; private set; }
        public string DaysBehindTheMayorText { get; private set; }
        public MayorshipNotificationType Type { get; private set; }
        public string ExtraText { get; private set; }

        public MayorshipNotification(JToken item)
        {
            string cks = Json.TryGetJsonProperty(item, "checkins");
            int checkins;
            if (int.TryParse(cks, out checkins))
            {
                Checkins = checkins;
            }

            string bhd = Json.TryGetJsonProperty(item, "daysBehind");
            if (bhd != null)
            {
                int bh;
                if (int.TryParse(bhd, out bh))
                {
                    DaysBehindTheMayor = bh;
                    // hardcoded for now
                    DaysBehindTheMayorText = string.Format(
                        CultureInfo.InvariantCulture,
                        "You are now {0} day{1} away from becoming the Mayor!",
                        bh.ToString(),
                        bh < 2 ? "" : "s");

                    if (bh > 10)
                    {
                        DaysBehindTheMayorText = null; // don't show
                    }

                    if (bh == 1)
                    {
                        ExtraText = "You must have a profile photo setup on foursquare.com to become the mayor.";
                    }
                }
            }

            Message = Checkin.SanitizeString(Json.TryGetJsonProperty(item, "message"));

            var user = item["user"];
            if (user != null)
            {
                User = CompactUser.ParseJson(user);
            }

            string imageUri = Json.TryGetJsonProperty(item, "image");
            try
            {
                Image = new Uri(imageUri, UriKind.Absolute);
            }
            catch (Exception)
            {
                // note: Might be relative, silent watson.
            }

            string tt = Json.TryGetJsonProperty(item, "type");
            switch (tt)
            {
                case "nochange":
                    Type = MayorshipNotificationType.NoChange;
                    break;

                case "new":
                    Type = MayorshipNotificationType.New;
                    break;

                case "stolen":
                    Type = MayorshipNotificationType.Stolen;
                    break;

                default:
                    // todo: sometehing for default, silent report?
                    break;
            }
        }
    }
}
