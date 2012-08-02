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
using System.Diagnostics;
using System.Globalization;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class CompactSpecial : ISpecializedComparisonString
    {
        public string SpecializedComparisonString
        {
            get
            {
                return SpecialId + "," + VenueId;
            }
        }

        public string SpecialId { get; set; }

        public SpecialType Type { get; set; }
        
        public string Message { get; set; }

        /// <summary>
        /// A description of how to unlock the special.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The specific rules for this special.
        /// </summary>
        public string FinePrint { get; set; }

        public bool IsUnlocked { get; set; }

        public bool IsLocked { get; set; }

        public string IconType { get; set; } // string, probably maps to a Uri

        /// <summary>
        /// The header text describing the type of special.
        /// </summary>
        public string Title { get; set; }

        public string State { get; set; } // probably is supposed to be an enum

        /// <summary>
        /// Possible state values as an enum. New in my 1.5.
        /// </summary>
        public SpecialState SpecialState { get; set; }

        public string Provider { get; set; }

        /// <summary>
        /// A description of how close you are to unlocking the special. Either
        /// the number of people who have already unlocked the special (flash 
        /// and swarm specials), or the number of your friends who have already 
        /// checked in (friends specials).
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Minutes remaining until the special can be unlocked (flash special 
        /// only).
        /// </summary>
        public int Detail { get; set; }

        /// <summary>
        /// A number indicating the maximum (swarm, flash) or minimum (friends)
        /// number of people allowed to unlock the special.
        /// </summary>
        public int Target { get; set; }

        /// <summary>
        /// A label describing what the number in the progress field means.
        /// </summary>
        public string ProgressDescription { get; set; }

        //public string Redemption { get; set; }

        public Uri LocalSpecialUri { get; set; }

        public Uri IconUri { get; set; }

        // passed in...

        public string VenueId { get; set; }

        public CompactVenue CompactVenue { get; set; }

        /// <summary>
        /// A list of friends currently checked in, as compact user objects 
        /// (friends special only).
        /// </summary>
        public List<CompactUser> FriendsHere { get; set; }

        public static CompactSpecial ParseJson(JToken json, string venueId)
        {
            var cs = new CompactSpecial();

            // OVERRIDE! Used for nearby specials.
            var vn = json["venue"];
            if (vn != null)
            {
                var venue = CompactVenue.ParseJson(vn);
                if (venue != null)
                {
                    cs.CompactVenue = venue;
                    venueId = venue.VenueId;
                }
            }

            Debug.Assert(venueId != null);

            cs.VenueId = venueId;

            cs.SpecialId = Json.TryGetJsonProperty(json, "id");

            Debug.Assert(cs.VenueId != null);

            if (!string.IsNullOrEmpty(cs.SpecialId))
            {
                cs.LocalSpecialUri = new Uri(
                    string.Format(CultureInfo.InvariantCulture,
                    "/JeffWilcox.FourthAndMayor.Specials;component/Special.xaml?id={0}&venueId={1}", cs.SpecialId, cs.VenueId),
                    UriKind.Relative);
            }

            cs.Message = Json.TryGetJsonProperty(json, "message");

            cs.Title = Json.TryGetJsonProperty(json, "title");

            cs.Description = Json.TryGetJsonProperty(json, "description");

            cs.FinePrint = Json.TryGetJsonProperty(json, "finePrint");

            cs.IconType = Json.TryGetJsonProperty(json, "icon");
            if (!string.IsNullOrEmpty(cs.IconType))
            {
                cs.IconUri = new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "http://foursquare.com/img/business/special_icons/{0}.png", 
                    cs.IconType), UriKind.Absolute);
            }

            // new in my 1.5 v
            cs.State = Json.TryGetJsonProperty(json, "state");
            SpecialState sst;
            switch (cs.State)
            {
                case "unlocked":
                    sst = SpecialState.Unlocked;
                    break;

                case "before start":
                    sst = SpecialState.BeforeStart;
                    break;

                case "in progress":
                    sst = SpecialState.InProgress;
                    break;

                case "taken":
                    sst = SpecialState.Taken;
                    break;

                default:
                case "locked":
                    sst = SpecialState.Locked;
                    break;
            }
            cs.SpecialState = sst;

            cs.Provider = Json.TryGetJsonProperty(json, "provider");

            string prog = Json.TryGetJsonProperty(json, "progress");
            if (!string.IsNullOrEmpty(prog))
            {
                int pg;
                if (int.TryParse(prog, out pg))
                {
                    cs.Progress = pg;
                }
            }

            cs.ProgressDescription = Json.TryGetJsonProperty(json, "progressDescription");

            // Minutes remaining until the special can be unlocked (flash only)
            prog = Json.TryGetJsonProperty(json, "detail");
            if (!string.IsNullOrEmpty(prog))
            {
                int pg;
                if (int.TryParse(prog, out pg))
                {
                    cs.Detail = pg;
                }
            }

            // A number indicating the maximum (swarm, flash) or minimum 
            // (friends) number of people allowed to unlock the special.
            prog = Json.TryGetJsonProperty(json, "target");
            if (!string.IsNullOrEmpty(prog))
            {
                int pg;
                if (int.TryParse(prog, out pg))
                {
                    cs.Target = pg;
                }
            }

            // not part of the v3 docs...
            // cs.Redemption = Json.TryGetJsonProperty(json, "redemption");

            cs.IsUnlocked = Json.TryGetJsonBool(json, "unlocked");
            cs.IsLocked = !cs.IsUnlocked;

            // A list of friends currently checked in, as compact user objects 
            // (friends special only).
            var fh = json["friendsHere"];
            if (fh != null)
            {
                var list = new List<CompactUser>();
                foreach (var friend in fh)
                {
                    var cu = CompactUser.ParseJson(friend);
                    if (cu != null)
                    {
                        list.Add(cu);
                    }
                }
                cs.FriendsHere = list;
            }

            string type = Json.TryGetJsonProperty(json, "type");
            SpecialType st;
            switch (type)
            {
                case "mayor":
                    st = SpecialType.Mayor;
                    break;

                case "frequency":
                    st = SpecialType.Frequency;
                    break;

                case "count":
                    st = SpecialType.Count;
                    break;

                case "regular":
                    st = SpecialType.Regular;
                    break;

                case "friends":
                    st = SpecialType.Friends;
                    break;

                case "swarm":
                    st = SpecialType.Swarm;
                    break;

                case "flash":
                    st = SpecialType.Flash;
                    break;

                case "other":
                default:
                st = SpecialType.Other;
                break;
            }
            cs.Type = st;

            /*
             "item": {
      "special": {
        "id": "4d6d69fa40fc8eecde57a5ba",
        "type": "other",
        "message": "New clients: Receive 20% off your first service at Capelli's when you check in at Foursquare!",
        "description": "for some other condition",
        "unlocked": true,
        "icon": "frequency",
        "title": "Special Offer",
        "state": "unlocked",
        "provider": "foursquare",
        "redemption": "standard"
      }
             * */

            return cs;
        }
    }
}
