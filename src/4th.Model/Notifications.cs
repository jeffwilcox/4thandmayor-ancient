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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Notifications : List<Notification>
    {
        public Notifications()
        {
            UniqueId = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
        }

        // This will not be restored from hydration. Used for getting the URI
        // constructed properly with the check-in request's data.
        private Model.CheckinRequest _temporaryCheckinRequest { get; set; }

        public Uri LocalUri
        {
            get 
            {
                string secondaryOptionalQueryInformation =
                    _temporaryCheckinRequest == null ?
                    string.Empty :
                    string.Format(
                        CultureInfo.InvariantCulture, 
                        
                        "&facebook={0}&twitter={1}&private={2}", 
                        
                        _temporaryCheckinRequest.Facebook,
                        _temporaryCheckinRequest.Twitter,
                        _temporaryCheckinRequest.IsPrivate);

                return new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,

                    "/Views/Checkin.xaml?uid={0}{1}", 

                    UniqueId,
                    secondaryOptionalQueryInformation), 
                        UriKind.Relative); }
        }

        public int UnreadNotifications { get; private set; }

        public string UniqueId { get; private set; }

        public static Notifications Rehydrate(string uniqueId)
        {
            var tt = new TombstoningText("notifications");
            if (tt.Load(uniqueId))
            {
                var sr = new StringReader(tt.Text);
                string lineOne = sr.ReadLine();
                string venueId = null;
                if (lineOne != null)
                {
                    venueId = lineOne.Trim();
                }

                var text = sr.ReadToEnd();
                var json = JArray.Parse(text);
                if (json != null)
                {
                    return ParseJson(json, venueId, null);
                }
            }

            return null;
        }

        public static Notifications ParseJson(JToken json, string venueId, Model.CheckinRequest optionalCheckinRequest)
        {
            var ns = new Notifications();

            ns._temporaryCheckinRequest = optionalCheckinRequest;

            // Save as a tombstoning object.
            var tt = new TombstoningText("notifications");
            tt.Text = venueId + Environment.NewLine + json.ToString();
            tt.Save(ns.UniqueId);

            foreach (var notif in json)
            {
                Notification notification = Notification.ParseJson(notif, venueId);
                if (notification != null)
                {
                    ns.Add(notification);
                }
                else
                {
                    string s = Json.TryGetJsonProperty(notif, "type");
                    if (s == "notificationTray")
                    {
                        var item = notif["item"];
                        if (item != null)
                        {
                            string unreadCount = Json.TryGetJsonProperty(item, "unreadCount");
                            int i;
                            if (int.TryParse(unreadCount, out i))
                            {
                                ns.UnreadNotifications = i;
                            }
                        }
                    }
                }
            }

            // Reorder the notifications per Foursquare preferred ordering.
            int currentIndex = 0;
            TryReorderNotificationOfType(ns, typeof(MessageNotification), ref currentIndex);
            TryReorderNotificationOfType(ns, typeof(BadgeNotification), ref currentIndex);
            TryReorderNotificationOfType(ns, typeof(MayorshipNotification), ref currentIndex);
            TryReorderNotificationOfType(ns, typeof(SpecialNotification), ref currentIndex);
            TryReorderNotificationOfType(ns, typeof(ScoreNotification), ref currentIndex);
            TryReorderNotificationOfType(ns, typeof(LeaderboardNotification), ref currentIndex);
            TryReorderNotificationOfType(ns, typeof(RecommendedTipNotification), ref currentIndex);

            return ns;
        }

        // This code is full of "No Hire"!

        private static void TryReorderNotificationOfType(Notifications ns, Type type, ref int currentIndex)
        {
            Notification n = null;
            if (TryGetNotificationOfType(ns, type, out n))
            {
                ns.Remove(n);
                ns.Insert(currentIndex++, n);
            }
        }

        private static bool TryGetNotificationOfType(Notifications ns, Type type, out Notification notification)
        {
            notification = null;
            for (int i = 0; i < ns.Count; ++i)
            {
                var notif = ns[i];
                if (type.IsInstanceOfType(notif))
                {
                    notification = notif;
                    return true;
                }
            }
            return false;
        }
    }
}
