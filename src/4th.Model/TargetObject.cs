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
    public enum TargetObjectType
    {
        Unknown, // not a valid foursquare type.

        User,
        Checkin,
        Venue,
        Tip,
        Badge,
        Special,
        Url,
    }

    public class TargetObject
    {
        public TargetObjectType Type { get; set; }
        public object Instance { get; set; }
        public Uri LocalUri 
        { get
            {
                if (Instance != null)
                {
                    switch (Type)
                    {
                        case TargetObjectType.Badge:
                            var b = (Badge)Instance;
                            return b.InternalAppUri;

                        case TargetObjectType.Checkin:
                            var c = (Checkin)Instance;
                            return c.LocalCommentsUri; // ?

                        case TargetObjectType.Special:
                            var s = (CompactSpecial)Instance;
                            return s.LocalSpecialUri;

                        case TargetObjectType.Tip:
                            var t = (Tip)Instance;
                            return t.TipUri;

                        case TargetObjectType.Url:
                            return (Uri)Instance;

                        case TargetObjectType.User:
                            var u = (CompactUser)Instance;
                            return u.UserUri;

                        case TargetObjectType.Venue:
                            var v = (CompactVenue)Instance;
                            return v.VenueUri;

                        case TargetObjectType.Unknown:
                        default:
                            break;
                    }
                }

                return null;
            }
        }

        public static TargetObject ParseJson(JToken obj)
        {
            TargetObject to = new TargetObject();

            to.Type = StringToType(Json.TryGetJsonProperty(obj, "type"));
            if (to.Type != TargetObjectType.Unknown)
            {
                to.Instance = HydrateCompactObject(to.Type, obj["object"]);
            }

            return to;
        }

        private static object HydrateCompactObject(TargetObjectType type, JToken item)
        {
            try
            {
                if (item != null)
                {
                    switch (type)
                    {
                        case TargetObjectType.Badge:
                            return Badge.ParseJson(item);

                        case TargetObjectType.Checkin:
                            return Checkin.ParseJson(item);

                        case TargetObjectType.Special:
                            // VenueId, if null, must be parsed out of the 
                            // notification or else it will throw. FYI.
                            return CompactSpecial.ParseJson(item, null);

                        case TargetObjectType.Tip:
                            return Tip.ParseJson(item);

                        case TargetObjectType.Url:
                            Uri uri = Json.TryGetUriProperty(item, "url");
                            return uri;

                        case TargetObjectType.User:
                            return CompactUser.ParseJson(item);

                        case TargetObjectType.Venue:
                            return CompactVenue.ParseJson(item);

                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static TargetObjectType StringToType(string s)
        {
            TargetObjectType type;
            switch (s)
            {
                case "user":
                    type = TargetObjectType.User;
                    break;

                case "checkin":
                    type = TargetObjectType.Checkin;
                    break;

                case "venue":
                    type = TargetObjectType.Venue;
                    break;

                case "tip":
                    type = TargetObjectType.Tip;
                    break;

                case "badge":
                    type = TargetObjectType.Badge;
                    break;

                case "special":
                    type = TargetObjectType.Special;
                    break;

                case "url":
                    type = TargetObjectType.Url;
                    break;

                default:
                    type = TargetObjectType.Unknown;
                    break;
            }
            return type;
        }
    }
}
