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
    public class CompactVenue : ISpecializedComparisonString 
    {
        public string VenueId { get; private set; }
        public string Name { get; private set; }

        public string Address { get; private set; }
        public string CrossStreet { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Zip { get; private set; }
        public bool IsVerified { get; private set; }
        public LocationPair Location
        {
            get
            {
                return _location;
            }
        }
        public string Phone { get; private set; }
        
        // Only used in the checkins/place list:
        public bool? HasTodo { get; private set; }

        public Uri VenueUri { get; private set;}

        public List<CompactSpecial> Specials { get; private set; }

        public double Meters { get; private set; }

        public string HereNow { get; private set; }
        public bool HasHereNow { get { return HereNow != null; } }

        // CONSIDER: An accessor to return the very detailed venue information

        // HELPERS
        private static string WrapIfThere(string s, string before, string after)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : before + s + after;
        }
        private static string SpaceAfterIfThere(string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s + " ";
        }

        public Category PrimaryCategory
        {
            get;
            private set;
        }

        public string AddressLine { get; private set; }

        // these are used in the checkin pages...
        private LocationPair _location;
        public double Latitude
        {
            get
            {
                return _location != null ? _location.Latitude : double.NaN;
            }
        }
        public double Longtitude
        {
            get
            {
                return _location != null ? _location.Longitude : double.NaN;
            }
        }

        public int CheckinsCount { get; set; }

        // CONSIDER: Should it return string.Empty or null?
        public string Distance
        {
            get
            {
                if (double.IsNaN(Meters))
                {
                    return string.Empty;
                }
                else
                {
                    return ImperialUnits.GetMetersInProperUnitsDisplay(Meters);
                }
            }
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }

        // Used for customizing the data template in a hacky way.
        public void OverrideHereNow(string text)
        {
            HereNow = text;
        }

        public static CompactVenue CreateVenueless(string name)
        {
            return new CompactVenue
            {
                Name = name,
            };
        }

        public static CompactVenue ParseJson(JToken venue)
        {
            var b = new CompactVenue();

            b.VenueId = Json.TryGetJsonProperty(venue, "id");
            Debug.Assert(b.VenueId != null);

            // FUTURE: Centralize handling of location.
            var location = venue["location"];
            if (location != null)
            {
                b.Address = Json.TryGetJsonProperty(location, "address");

                b.City = Json.TryGetJsonProperty(location, "city");
                b.CrossStreet = Json.TryGetJsonProperty(location, "crossStreet");
                b.State = Json.TryGetJsonProperty(location, "state");
                b.Zip = Json.TryGetJsonProperty(location, "postalCode");

                if (b.Address != null || b.CrossStreet != null)
                {
                    b.AddressLine = SpaceAfterIfThere(b.Address) + WrapIfThere(b.CrossStreet, "(", ")");
                }

                var gl = location["lat"];
                var gll = location["lng"];
                // didn't work in Brazil and other place!
                //string gl = Json.TryGetJsonProperty(location, "lat");
                //string gll = Json.TryGetJsonProperty(location, "lng");
                if (gl != null && gll != null)
                {
                    try
                    {
                        b._location = new LocationPair
                        {
                            Latitude = (double)gl, // double.Parse(gl, CultureInfo.InvariantCulture),
                            Longitude = (double)gll, // double.Parse(gll, CultureInfo.InvariantCulture),
                        };
                    }
                    catch
                    {
                    }
                }

                string dist = Json.TryGetJsonProperty(location, "distance");
                if (dist == null)
                {
                    b.Meters = double.NaN;
                }
                else
                {
                    b.Meters = double.Parse(dist, CultureInfo.InvariantCulture);
                }
            }

            b.Name = Json.TryGetJsonProperty(venue, "name");
            if (b.Name != null)
            {
                b.Name = Checkin.SanitizeString(b.Name);
            }

            string verif = Json.TryGetJsonProperty(venue, "verified");
            // NOTE: Is this even useful to expose? (A bool property)

            var todos = venue["todos"];
            if (todos != null)
            {
                // temporary hack around lists...

                var subListOne = todos["id"];
                if (subListOne != null)
                {
                    var userSubTree = todos["user"];
                    if (userSubTree != null)
                    {
                        string userIsSelf = Json.TryGetJsonProperty(userSubTree, "relationship");
                        if (userIsSelf != null && userIsSelf == "self")
                        {
                            b.HasTodo = true;
                        }
                    }
                }

                //string htodo = Json.TryGetJsonProperty(todos, "count"); // checkin mode only
                //int i;
                //if (int.TryParse(htodo, CultureInfo.InvariantCulture);
                //if (i > 0)
                //{
                    //b.HasTodo = true;
                //}
            }

            var specials = venue["specials"];
            if (specials != null)
            {
                try
                {
                    var cv = new List<CompactSpecial>();
                    var specialsItems = specials["items"];
                    if (specialsItems != null)
                    {
                        foreach (var special in specialsItems)
                        {
                            if (special != null)
                            {
                                CompactSpecial spo = CompactSpecial.ParseJson(special, b.VenueId);
                                if (spo != null)
                                {
                                    cv.Add(spo);
                                }
                            }
                        }
                    }
                    if (cv.Count > 0)
                    {
                        b.Specials = cv;
                    }
                }
                catch (Exception)
                {
                    // As of 3.4, and a new Foursquare API version, specials
                    // returned are in dictionary format. This prevents cached
                    // data from throwing here.
                }
            }

            var stats = venue["stats"];
            if (stats != null)
            {
                // NOTE: V2: Add these properties to the POCO
                
                string checkinsCount = Json.TryGetJsonProperty(stats, "checkinsCount");
                int i;
                if (int.TryParse(checkinsCount, out i))
                {
                    b.CheckinsCount = i;
                }

                string usersCount = Json.TryGetJsonProperty(stats, "usersCount");
            }

            var hereNow = venue["hereNow"];
            if (hereNow != null)
            {
                string hnc = Json.TryGetJsonProperty(hereNow, "count");
                int hni;
                if (int.TryParse(hnc, out hni))
                {
                    if (hni > 1)
                    {
                        b.HereNow = Json.GetPrettyInt(hni) + " people";
                    }
                    else if (hni == 1)
                    {
                        b.HereNow = "1 person";
                    }
                }
            }

            var pc = venue["categories"];
            if (pc != null)
            {
                // FUTURE: A collection of all the categories.
                foreach (var category in pc)
                {
                    // NOTE: For this version, just grabbing the first categ.
                    b.PrimaryCategory = Category.ParseJson(category);
                    break;
                }
            }

            var contact = venue["contact"];
            if (contact != null)
            {
                b.Phone = Json.TryGetJsonProperty(contact, "phone");
                // NOTE: VENUE TWITTER?
            }

            b.VenueUri = new Uri(
                string.Format(
                CultureInfo.InvariantCulture,
                "/Views/Venue.xaml?id={0}&name={1}",
                Uri.EscapeDataString(b.VenueId),
                Uri.EscapeDataString(b.Name)), UriKind.Relative);

            return b;
        }

        public string SpecializedComparisonString
        {
            get
            {
                // Supports venueless.
                return VenueId ?? Name;
            }
        }
    }
}
