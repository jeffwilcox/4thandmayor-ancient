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
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    using System.Collections.Generic;
    using Controls;

    public class Checkin : NotifyPropertyChangedBase, ISpecializedComparisonString
    {
        public Checkin()
        {
            CompleteFunctionalityVis = Visibility.Visible;
            ReduceFunctionalityVis = Visibility.Collapsed;
        }

        public string CheckinId { get; set; }
        public CompactUser User { get; set; }

        private CompactVenue _venue;
        public CompactVenue Venue 
        {
            get { return _venue; }
            set
            {
                _venue = value;
                RaisePropertyChanged("Venue");
            }
        }

#if DEBUG
        public override string ToString()
        {
            return DisplayUser + DisplayBetween + DisplayVenue;
        }
#endif

        public double Meters { get; set; }
        public string Shout { get; set; }
        public bool HasShout { get { return Shout != null; }}
        public string VenuelessName { get; set; }
        public string DisplayUser { get; set; }
        public string DisplayBetween { get; set; }
        public string DisplayVenue { get; set; }
        public Uri UserUri { get; set; }
        public Uri VenueUri { get; set; }
        public bool IsMayor { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Created { get; set; }
        public bool HasCreated { get { return Created != null; } }
        public bool IsInAnotherCity { get; set; }
        public string DisplayAddressLine { get; set; }
        public string CheckinType { get; set; }

        public int PhotosCount { get; set; }
        public int CommentsCount { get; set; }
        public string CommentsAndPhotosOrAdd { get; set; }
        public bool HasPhotos { get; set; }
        public bool HasComments { get; set; }
        public bool HasPhotosOrComments { get; set; }
        public bool CanAddPhotos { get; set; }

        // Used when contained in other views such as a comments page summary 
        // or a users' checkins list. Not parsed actually.
        public bool ReduceFunctionality { get; set; }
        public Visibility ReduceFunctionalityVis { get; set; }
        public Visibility CompleteFunctionalityVis { get; set; }

        public Uri LocalCommentsUri { get; set; }

        public List<Photo> Photos { get; set; }
        public Photo FirstCheckinPhoto { get; set; }

        public Uri ClientWebUri { get; set; }
        public string ClientName { get; set; }

        // Used only for hacking around in the people view for venues.
        public string HereNowRemainderText { get; set; }

        public static string GetDateString(DateTime dt)
        {
            TimeSpan ts = DateTime.UtcNow - dt;
#if DEBUG
//            if (ts.TotalMinutes < -1)
  //          {
    //            return "wrong epoch time " + ts.TotalMinutes.ToString();
      //      }
#endif

            if (ts.TotalMinutes < 1.5)
            {
                return "just now";
            }
            int v = (int)Math.Round(ts.TotalMinutes);
            if (ts.TotalMinutes < 60)
            {
                return v + " minutes ago";
            }
            v = (int) Math.Round(ts.TotalHours);
            if (v == 1)
            {
                return "1 hour ago";
            }
            if (ts.TotalHours <= 24)
            {
                return v + " hours ago";
            }
            v = (int) Math.Round(ts.TotalDays);
            if (ts.TotalDays < 7)
            {
                if (v <= 1)
                {
                    return v + " day ago";
                }
                else
                {
                    return v + " days ago";
                }
            }
            if (ts.TotalDays < 14)
            {
                return "1 week ago";
            }
            if (ts.TotalDays < 31)
            {
                int wkCount = (int)Math.Round(ts.TotalDays / 7);
                return wkCount + " weeks ago";
            }
            if (v < 365)
            {
                return dt.ToString("m");
            }

            // foursquare iOS uses this sort of format: "Fri Sep 10"
            return dt.ToString("ddd MMM d");
        }

        public static Checkin ParseJson(JToken checkin)
        {
            Checkin c = new Checkin();

            string created = Json.TryGetJsonProperty(checkin, "createdAt");
            if (created != null)
            {
                DateTime dtc = UnixDate.ToDateTime(created);
                c.CreatedDateTime = dtc;
                c.Created = GetDateString(dtc);
            }

            // Client information.
            var source = checkin["source"];
            if (source != null)
            {
                c.ClientName = Json.TryGetJsonProperty(source, "name");
               
                // TODO: Create a crashless URI helper.
                try
                {
                    string url = Json.TryGetJsonProperty(source, "url");
                    if (!string.IsNullOrEmpty(url))
                    {
                        if (url.StartsWith("www"))
                        {
                            url = "http://" + url;
                        }
                        c.ClientWebUri = new Uri(url, UriKind.Absolute);
                    }
                }
                catch 
                {
                }
            }

            string type = Json.TryGetJsonProperty(checkin, "type");
            // Only if present. Won't show up in badge winnings, for instance.
            if (type != null)
            {
                Debug.Assert(type == "checkin" || type == "shout" || type == "venueless");
            }
            c.CheckinType = type;

            c.CheckinId = Json.TryGetJsonProperty(checkin, "id");
            // badge mode actually won't have this Debug.Assert(c.CheckinId != null);

            if (!string.IsNullOrEmpty(c.CheckinId))
            {
                c.LocalCommentsUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Views/Comments.xaml?checkin={0}", c.CheckinId),UriKind.Relative);
            }

            var location = checkin["location"];
            if (location != null && type == "venueless") // consider if that's right
            {
                // if shout or venueless, will provide...
                // lat, lng pair and/or a name
                string venuelessName = Json.TryGetJsonProperty(location, "name");
                c.VenuelessName = venuelessName;
            }

            var user = checkin["user"];
            if (user != null)
            {
                CompactUser bu = CompactUser.ParseJson(user);
                c.User = bu;
            }

            var venue = checkin["venue"];
            if (venue != null)
            {
                CompactVenue bv = CompactVenue.ParseJson(venue);
                if (bv != null)
                {
                    c.DisplayAddressLine = bv.AddressLine;
                }
                c.Venue = bv;
            }

            // Show venueless name at least.
            if (c.Venue == null && !string.IsNullOrEmpty(c.VenuelessName))
            {
                c.Venue = CompactVenue.CreateVenueless(c.VenuelessName);
            }

            c.Shout = Json.TryGetJsonProperty(checkin, "shout");

            string ismayor = Json.TryGetJsonProperty(checkin, "isMayor");
            if (ismayor != null && ismayor.ToLowerInvariant() == "true")
            {
                c.IsMayor = true;
            }
            
            string dist = Json.TryGetJsonProperty(checkin, "distance");
            if (dist == null)
            {
                c.Meters = double.NaN;
            }
            else
            {
                c.Meters = double.Parse(dist, CultureInfo.InvariantCulture);

                // Doing this here to centralize it somewhere at least.
                if (c.Meters > 40000) // NOTE: This is a random value, What value should define a different city?
                {
                    c.IsInAnotherCity = true;
                    if (c.Venue != null)
                    {
                        string s = c.Venue.City ?? string.Empty;
                        if (!string.IsNullOrEmpty(c.Venue.State))
                        {
                            s += ", ";
                        }
                        if (c.Venue.State != null)
                        {
                            s += c.Venue.State;
                        }

                        c.DisplayAddressLine = s;
                    }
                }
            }

            if (c.User != null)
            {
                c.DisplayUser = c.User.ShortName;
                c.UserUri = c.User.UserUri;
            }
            if (c.Venue != null)
            {
                c.DisplayBetween = null; // WAS:  "@";
                c.DisplayVenue = c.Venue.Name;
                c.VenueUri = c.Venue.VenueUri;
            }
            else
            {
                if (type == "shout")
                {
                    //c.DisplayBetween = "shouted:";
                }
                else if (type == "venueless")
                {
                    c.DisplayBetween = c.VenuelessName;
                }
                else
                {
                    c.DisplayBetween = "[off-the-grid]"; // @ 
                }
            }

            // Photo and Comment information
            c.CommentsCount = GetNodeCount(checkin, "comments");
            c.PhotosCount = GetNodeCount(checkin, "photos");

            c.HasComments = c.CommentsCount > 0;
            c.HasPhotos = c.PhotosCount > 0;
            c.HasPhotosOrComments = c.HasComments || c.HasPhotos;

            if (c.HasPhotos)
            {
                List<Photo> photos = new List<Photo>();
                var pl = checkin["photos"];
                if (pl != null)
                {
                    var pll = pl["items"];
                    if (pll != null)
                    {
                        foreach (var photo in pll)
                        {
                            var po = Photo.ParseJson(photo);
                            if (po != null)
                            {
                                photos.Add(po);
                            }
                        }
                    }
                }
                c.Photos = photos;

                if (photos.Count > 0)
                {
                    c.FirstCheckinPhoto = photos[0];
                }
            }

            int activityCount = c.CommentsCount; // +c.PhotosCount;
            c.CommentsAndPhotosOrAdd = activityCount > 0 ? activityCount.ToString(CultureInfo.InvariantCulture) : "+";

            if (c.User != null && c.User.Relationship == FriendStatus.Self)
            {
                c.CanAddPhotos = true;
            }

            return c;
        }

        private static int GetNodeCount(JToken obj, string nodeName)
        {
            var x = obj[nodeName];
            if (x != null)
            {
                int i;
                if (int.TryParse(Json.TryGetJsonProperty(x, "count"), out i))
                {
                    return i;
                }
            }

            return 0;
        }

        internal static string SanitizeString(string value)
        {
            if (value == null)
            {
                return null;
            }
            return value
                .Replace("", "") // "Denver International Airport (DEN) ✈"
                .Replace("✈", "");
        }

        public string SpecializedComparisonString
        {
            get
            {
                if (User != null)
                {
                    return User.UserId;
                }
//#if DEBUG
  //              throw new NotImplementedException("There must be a user!");
//#endif

                return CheckinId;
            }
        }
    }
}
