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
    public abstract class Notification
    {
        protected Notification()
        {
        }

        public static Notification ParseJson(JToken notification, string venueId)
        {
            Notification n = null;

            string type = Json.TryGetJsonProperty(notification, "type");
            var item = notification["item"];

            if (item != null)
            {
                switch (type)
                {
                    case "message":
                        n = new MessageNotification(item);
                        break;

                    case "badge":
                        n = new BadgeNotification(item);
                        break;

                    case "mayorship":
                        n = new MayorshipNotification(item);
                        break;

                    case "tip":
                        n = new RecommendedTipNotification(item);
                        break;

                    case "leaderboard":
                        n = new LeaderboardNotification(item);
                        break;

                    case "special":
                        // TODO: Implement special support in notificationis. Comes AFTER mayor and BEFORE tip and points.
                        n = new SpecialNotification(item, venueId);
                        break;

                    case "score":
                        n = new ScoreNotification(item);
                        break;

                    default:
                        // Consider a silent watson here about the type.
                        var hasMessage = item["message"];
                        if (hasMessage != null)
                        {
                            n = new MessageNotification(item);
                        }
                        // else n is null

                        break;
                }
            }

            return n;
        }

    }
}
