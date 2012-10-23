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
using AgFx;
using Newtonsoft.Json.Linq;

#if WINDOWS8

namespace JeffWilcox.FourthAndMayor.Model
{
    public class CivicAddressLoadContext : LoadContext
    {
        public CivicAddressLoadContext()
            : base("civic")
        {
        }

        protected override string GenerateKey()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}x{2}_{3}x{4}", Identity, Latitude, Longtitude, BoxX, BoxY);
        }

        public int Latitude { get; set; }
        public int Longtitude { get; set; }
        public int BoxX { get; set; }
        public int BoxY { get; set; }
    }

    public class HttpItemBase<T> : ModelItemBase<T>
        where T : LoadContext
    {
        public HttpItemBase() { }

        public HttpItemBase(T context)
            : base(context)
        {
        }
    }

    [CachePolicy(CachePolicy.CacheThenRefresh, 60 * 60 * 24 * 30)] // 1 month.
    public class CivicAddress : HttpItemBase<CivicAddressLoadContext>
    {
        public CivicAddress() { }
        public CivicAddress(CivicAddressLoadContext context)
            : base(context)
        {
        }

        public class CivicAddressDataLoader : HttpItemDataLoaderBase<CivicAddressLoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                string method = "users/" + id;

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        method,
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var u = new User(context);
                    u.IgnoreRaisingPropertyChanges = true;

                    var user = json["user"];

                    string uid = Json.TryGetJsonProperty(user, "id");
                    u.UserId = uid;

                    u.First = Json.TryGetJsonProperty(user, "firstName");
                    u.Last = Json.TryGetJsonProperty(user, "lastName");
#if DEMO
                    if (u.Last != null && u.Last.Length > 0)
                    {
                        u.Last = u.Last.Substring(0, 1);
                    }
#endif
                    u.Pings = Json.TryGetJsonBool(user, "pings");

                    u.HomeCity = Json.TryGetJsonProperty(user, "homeCity");

                    var contact = user["contact"];
                    if (contact != null)
                    {
                        u.Phone = FormatSimpleUnitedStatesPhoneNumberMaybe(Json.TryGetJsonProperty(contact, "phone"));
                        u.Email = Json.TryGetJsonProperty(contact, "email");
                        u.Twitter = Json.TryGetJsonProperty(contact, "twitter");
                        u.Facebook = Json.TryGetJsonProperty(contact, "facebook");
#if DEMO
                        if (!string.IsNullOrEmpty(u.Phone))
                        {
                            u.Phone = "(206) 555-1212";
                        }
                        if (!string.IsNullOrEmpty(u.Email))
                        {
                            u.Email = "someone@yahoo.com";
                        }
#endif

                    }

                    u.FriendStatus = GetFriendStatus(Json.TryGetJsonProperty(user, "relationship"));

                    u.MayorshipsLocalUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Profile;component/ProfileMayorships.xaml?id={0}", uid), UriKind.Relative);
                    u.BadgesLocalUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Profile;component/ProfileBadges.xaml?id={0}", uid), UriKind.Relative);
                    u.TipsLocalUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Profile;component/ProfileTips.xaml?id={0}", uid), UriKind.Relative);
                    u.RecentCheckinsLocalUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Profile;component/ProfileCheckins.xaml?id={0}", uid), UriKind.Relative);
                    u.TopCategoriesLocalUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Profile;component/ProfileMostExploredCategories.xaml?id={0}", uid), UriKind.Relative);
                    u.TopPlacesLocalUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Profile;component/ProfileTopPlaces.xaml?id={0}", uid), UriKind.Relative);
                    u.FriendListLocalUri = new Uri(
                        string.Format(
                        CultureInfo.InvariantCulture,
                        "/JeffWilcox.FourthAndMayor.Profile;component/ProfileFriends.xaml?id={0}&self={1}",
                        uid,
                        u.FriendStatus == FriendStatus.Self ? "1" : string.Empty
                        ),
                        UriKind.Relative);

                    u.FriendRequestsLocalUri = new Uri(
                                                string.Format(
                        CultureInfo.InvariantCulture,
                        "/JeffWilcox.FourthAndMayor.Profile;component/ProfileFriends.xaml?id={0}&requests=please&self={1}",
                        uid,
                        u.FriendStatus == FriendStatus.Self ? "1" : string.Empty
                        ),
                        UriKind.Relative);

                    var photo = user["photo"];
                    if (photo != null)
                    {
                        // 4.0
                        var prefix = Json.TryGetJsonProperty(photo, "prefix");
                        var suffix = Json.TryGetJsonProperty(photo, "suffix");
                        string uri = null;
                        if (prefix != null && suffix != null)
                        {
                            // hard-coding for now in the new v4/5 syntax.
                            uri = prefix + "110x110" + suffix;

                            u.FullResolutionPhoto = new Uri(prefix + "original" + suffix, UriKind.Absolute);
                        }
                        else
                        {
                            // Pre v-4... the full resolution version will be
                            // broken since I've removed that code.
                            uri = Json.TryGetJsonProperty(user, "photo");
                        }

                        if (uri != null)
                        {
                            if (!uri.Contains(".gif"))
                            {
                                u.Photo = new Uri(uri);
                            }
                        }
                    }

                    string gender = Json.TryGetJsonProperty(user, "gender");
                    if (gender != null)
                    {
                        u.Gender = gender == "female" ? Model.Gender.Female : Model.Gender.Male;
                    }

                    var scores = user["scores"];
                    if (scores != null)
                    {
                        u.Scores = LeaderboardItemScores.ParseJson(scores);
                    }

                    var checkins = user["checkins"];
                    if (checkins != null)
                    {
                        string totalCheckinCount = Json.TryGetJsonProperty(checkins, "count");
                        int i;
                        if (int.TryParse(totalCheckinCount, out i))
                        {
                            if (i > 0)
                            {
                                // LOCALIZE:
                                u.TotalCheckins =
                                    Json.GetPrettyInt(i)
                                    + " "
                                    + "check-in"
                                    + (i == 1 ? string.Empty : "s");
                            }
                        }

                        if (u.TotalCheckins == null)
                        {
                            // LOCALIZE:
                            u.TotalCheckins = "No check-ins";
                        }

                        // recent checkins
                        var items = checkins["items"];
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                // Just grabbing the first one.
                                u.RecentCheckin = Checkin.ParseJson(item);

                                break;
                            }
                        }
                    }

                    /*
                                 "requests": {
                    "count": 0
                },
                "tips": {
                    "count": 2
                },
                "todos": {
                    "count": 0
                }
                     * */

                    var tips = user["tips"];
                    if (tips != null)
                    {
                        string tipsNumber = Json.TryGetJsonProperty(tips, "count");
                        int tc;
                        if (int.TryParse(tipsNumber, out tc))
                        {
                            if (tc <= 0)
                            {
                                // LOCALIZE:
                                u.TipsCount = "No tips";
                            }
                            else if (tc == 1)
                            {
                                // LOCALIZE:
                                u.TipsCount = "1 tip";
                            }
                            else
                            {
                                // LOCALIZE:
                                u.TipsCount = tc.ToString(CultureInfo.InvariantCulture) + " tips";
                            }
                        }
                    }

                    var friends = user["friends"];
                    if (friends != null)
                    {
                        string friendsCount = Json.TryGetJsonProperty(friends, "count");
                        if (friendsCount != null)
                        {
                            // LOCALIZE:
                            u.FriendsCount = "No friends yet";
                            int i = int.Parse(friendsCount, CultureInfo.InvariantCulture);
                            if (i == 1)
                            {
                                // LOCALIZE:
                                u.FriendsCount = "1 friend";
                            }
                            else if (i > 1)
                            {
                                // LOCALIZE:
                                u.FriendsCount = i.ToString(CultureInfo.InvariantCulture) + " friends";
                            }
                        }
                    }

                    var requests = user["requests"];
                    if (requests != null)
                    {
                        string fr = Json.TryGetJsonProperty(requests, "count");
                        int i;
                        // TODO: V2: VALIDATE THIS JUST TO BE SURE.
                        if (int.TryParse(fr, out i))
                        {
                            // TODO: UI: Add pending friend requests to profile.xaml
                            u.PendingFriendRequests = i;
                        }
                    }

                    //var stats = user["stats"];
                    //if (stats != null)
                    //{
                    //    // this was deprecated in v2 I believe
                    //}

                    var badges = user["badges"];
                    // LOCALIZE:
                    u.BadgeCountText = "No badges";
                    if (badges != null)
                    {
                        string count = Json.TryGetJsonProperty(badges, "count");
                        if (count != null)
                        {
                            int badgeCount = int.Parse(count, CultureInfo.InvariantCulture);
                            if (badgeCount == 1)
                            {
                                u.HasBadges = true;
                                // LOCALIZE:
                                u.BadgeCountText = "1 badge";
                            }
                            else
                            {
                                u.HasBadges = true;
                                // LOCALIZE:
                                u.BadgeCountText = badgeCount + " badges";
                            }
                        }
                    }

                    var mayorships = user["mayorships"];
                    // LOCALIZE:
                    u.MayorshipCountText = "No mayorships";
                    if (mayorships != null)
                    {
                        string mayorcount = Json.TryGetJsonProperty(mayorships, "count");
                        var ml = new List<CompactVenue>();
                        var items = mayorships["items"];
                        if (items != null)
                        {
                            foreach (var mayorship in items)
                            {
                                CompactVenue bv = CompactVenue.ParseJson(mayorship);
                                ml.Add(bv);
                            }
                        }

                        if (mayorcount != null)
                        {
                            int i = int.Parse(mayorcount, CultureInfo.InvariantCulture);
                            if (i == 0)
                            {
                                // u.MayorshipCountText = "No mayorships";
                            }
                            else if (i == 1)
                            {
                                // LOCALIZE:
                                u.MayorshipCountText = "1 mayorship";
                            }
                            else
                            {
                                // LOCALIZE:
                                u.MayorshipCountText = i + " mayorships";
                            }
                        }
                    }

                    u.IgnoreRaisingPropertyChanges = false;
                    u.IsLoadComplete = true;

                    return u;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "The person's information could not be downloaded at this time, please try again later.", e);
                }
            }
        }

    }
}

#endif
