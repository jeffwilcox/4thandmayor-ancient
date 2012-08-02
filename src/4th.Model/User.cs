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
using System.Collections.Generic;
using System.Globalization;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class User : FourSquareItemBase<LoadContext>
    {
        public User()
        {
        }

        public User(LoadContext context) : base(context)
        {
        }

        // UserId can be different than the Identity.
        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set 
            { 
                _userId = value; 
                RaisePropertyChanged("UserId"); 
            }
        }

        private string _homeCity;
        public string HomeCity
        {
            get { return _homeCity; }
            set
            {
                _homeCity = value;
                RaisePropertyChanged("HomeCity");
            }
        }

        private string _totalCheckins;
        public string TotalCheckins
        {
            get
            {
                return _totalCheckins;
            }
            set
            {
                _totalCheckins = value;
                RaisePropertyChanged("TotalCheckins");
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { _email = value;
            RaisePropertyChanged("Email");}
        }

        private string _facebook;
        public string Facebook
        {
            get { return _facebook; }
            set { _facebook = value;
            RaisePropertyChanged("Facebook");}
        }

        private string _firstName;
        public string First { get { return _firstName; }
            set 
            { 
                _firstName = value;
                RaisePropertyChanged("First");
            }
        }

        private FriendStatus _friendStatus;
        public FriendStatus FriendStatus
        {
            get { return _friendStatus; }
            set { _friendStatus = value;
            RaisePropertyChanged("FriendStatus");}
        }

        [DependentOnProperty("FriendStatus")]
        public bool IsSelf
        {
            get
            {
                return _friendStatus == Model.FriendStatus.Self;
            }
        }


        [DependentOnProperty("FriendStatus")]
        public bool IsFriendOrSelf
        {
            get
            {
                return _friendStatus == Model.FriendStatus.Friend || _friendStatus == FriendStatus.Self;
            }
        }

        private Gender _gender;
        public Gender Gender
        {
            get { return _gender; }
            set { _gender = value;
            RaisePropertyChanged("Gender");}
        }

        private string _lastName;
        public string Last
        {
            get { return _lastName; }
            set 
            { 
                _lastName = value;
                RaisePropertyChanged("Last");
            }
        }

        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set { _phone = value;
            RaisePropertyChanged("Phone");}
        }

        private Uri _photo;
        public Uri Photo
        {
            get { return _photo; }
            set { _photo = value;
            RaisePropertyChanged("Photo");}
        }

        [DependentOnProperty("Photo")]
        public Uri FullResolutionPhotoViewer
        {
            get
            {
                var uri = FullResolutionPhoto;
                if (uri != null)
                {
                    return new Uri(string.Format(CultureInfo.InvariantCulture,
                        "/Views/PhotoViewer.xaml?photoUri={0}",
                        Uri.EscapeDataString(uri.ToString())), 
                        UriKind.Relative);
                }

                return null;
            }
        }

        private Uri _fullResolutionPhoto;
        public Uri FullResolutionPhoto
        {
            get
            {
                return _fullResolutionPhoto;
            }

            set
            {
                _fullResolutionPhoto = value;
                RaisePropertyChanged("FullResolutionPhoto");
            }
        }

        private string _twitter;
        public string Twitter
        {
            get { return _twitter; }
            set { _twitter = value;
            RaisePropertyChanged("Twitter");}
        }

        private string _mct;
        public string MayorshipCountText
        {
            get { return _mct; }
            set
            {
                _mct = value;
                RaisePropertyChanged("MayorshipCountText");
            }
        }

        private Uri _mayorshipsLocalUri;
        public Uri MayorshipsLocalUri
        {
            get
            {
                return _mayorshipsLocalUri;
            }
            set
            {
                _mayorshipsLocalUri = value;
                RaisePropertyChanged("MayorshipsLocalUri");
            }
        }

        private Uri _badgesLocalUri;
        public Uri BadgesLocalUri
        {
            get
            {
                return _badgesLocalUri;
            }
            set
            {
                _badgesLocalUri = value;
                RaisePropertyChanged("BadgesLocalUri");
            }
        }

        private Uri _recentCheckinsLocalUri;
        public Uri RecentCheckinsLocalUri
        {
            get
            {
                return _recentCheckinsLocalUri;
            }
            set
            {
                _recentCheckinsLocalUri = value;
                RaisePropertyChanged("RecentCheckinsLocalUri");
            }
        }

        private Uri _tipsLocalUri;
        public Uri TipsLocalUri
        {
            get
            {
                return _tipsLocalUri;
            }
            set
            {
                _tipsLocalUri = value;
                RaisePropertyChanged("TipsLocalUri");
            }
        }

        private Uri _friendListLocalUri;
        public Uri FriendListLocalUri
        {
            get
            {
                return _friendListLocalUri;
            }
            set
            {
                _friendListLocalUri = value;
                RaisePropertyChanged("FriendListLocalUri");
            }
        }

        private Uri _friendRequestsUri;
        public Uri FriendRequestsLocalUri
        {
            get { return _friendRequestsUri; }
            set
            {
                _friendRequestsUri = value;
                RaisePropertyChanged("FriendRequestsLocalUri");
            }
        }

        private Uri _topPlacesUri;
        public Uri TopPlacesLocalUri
        {
            get
            {
                return _topPlacesUri;
            }
            set
            {
                _topPlacesUri = value;
                RaisePropertyChanged("TopPlacesLocalUri");
            }
        }

        private Uri _topCatsUri;
        public Uri TopCategoriesLocalUri
        {
            get
            {
                return _topCatsUri;
            }
            set
            {
                _topCatsUri = value;
                RaisePropertyChanged("TopCategoriesLocalUri");
            }
        }

        private int _pendingFriendFrequests;
        public int PendingFriendRequests
        {
            get
            {
                return _pendingFriendFrequests;
            }
            set
            {
                _pendingFriendFrequests = value;
                RaisePropertyChanged("PendingFriendRequests");
            }
        }

        [DependentOnProperty("PendingFriendRequests")]
        public string PendingFriendRequestsText
        {
            get
            {
                if (_pendingFriendFrequests > 0)
                {
                    // LOCALIZE:
                    return _pendingFriendFrequests + " pending friend request" + (_pendingFriendFrequests == 1 ? string.Empty : "s");
                }

                return null;
            }
        }

        private string _bct;
        public string BadgeCountText
        {
            get { return _bct; }
            set
            {
                _bct = value;
                RaisePropertyChanged("BadgeCountText");
            }
        }

        private LeaderboardItemScores _scores;
        public LeaderboardItemScores Scores
        {
            get { return _scores; }
            set
            {
                _scores = value;
                RaisePropertyChanged("Scores");
            }
        }

        public UserMayorships Mayorships
        {
            get
            {
                return DataManager.Current.Load<UserMayorships>(LoadContext);
            }
        }

        public UserTips Tips
        {
            get
            {
                return DataManager.Current.Load<UserTips>(LoadContext);
            }
        }

        public UserLists Lists
        {
            get
            {
                return DataManager.Current.Load<UserLists>(LoadContext);
            }
        }

        // Deprecated in iOS v4 APIs!
        public UserTodos Todos
        {
            get
            {
                return DataManager.Current.Load<UserTodos>(LoadContext);
            }
        }

        [DependentOnProperty("FriendStatus")]
        public Leaderboard Leaderboard
        {
            get
            {
                var context = new LeaderboardLoadContext(LoadContext.Identity);
                if (FriendStatus == FriendStatus.Self)
                {
                    return DataManager.Current.Load<Leaderboard>(context);
                }
                return new Leaderboard(context);
            }
        }
        
        public Uri UserUri
        {
            get
            {
                return new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "/Views/Profile.xaml?id={0}&name={1}",
                    Uri.EscapeDataString(UserId),
                    Uri.EscapeDataString(ToString())), UriKind.Relative);
            }
        }


        [DependentOnProperty("FriendStatus")]
        public Leaderboard SmallLeaderboard
        {
            get
            {
                var context = new LeaderboardLoadContext(LoadContext.Identity);
                context.Neighbors = "2";
                if (FriendStatus == FriendStatus.Self)
                {
                    return DataManager.Current.Load<Leaderboard>(context);
                }
                return new Leaderboard(context);
            }
        }

        public UserCheckins RecentCheckins
        {
            get
            {
                if (FriendStatus == FriendStatus.Self)
                {
                    return DataManager.Current.Load<UserCheckins>(LoadContext);
                }
                // TODO: I think this is the right way to handle this...
                return new UserCheckins(LoadContext);
            }
        }

        public FriendRequests FriendRequests
        {
            get
            {
                if (FriendStatus == Model.FriendStatus.Self)
                {
                    return DataManager.Current.Load<FriendRequests>(LoadContext);
                }
                // TODO: I think this is the right way to handle this...
                return new FriendRequests(LoadContext);
            }
        }

        public UserBadges Badges 
        {
            get
            {
                return DataManager.Current.Load<UserBadges>(LoadContext);
            }
        }

        public UserVenueStats VenueStatistics
        {
            get
            {
                return DataManager.Current.Load<UserVenueStats>(LoadContext);
            }
        }

        [DependentOnProperty("Badges")]
        public bool HasBadges
        {
            get;
            set;
        }

        [DependentOnProperty("First")]
        [DependentOnProperty("Last")]
        public string FullName { get { return ToString(); } }

        public override string ToString()
        {
            return _firstName + " " + _lastName;
        }

        public FriendsList FriendsList
        {
            get
            {
                return DataManager.Current.Load<FriendsList>(LoadContext);
            }
        }

        private bool _pings;
        public bool Pings
        {
            get
            {
                return _pings;
            }

            set
            {
                _pings = value;
                RaisePropertyChanged("Pings");
            }
        }

        private string _tipsCount;
        public string TipsCount
        {
            get
            {
                return _tipsCount;
            }

            set
            {
                _tipsCount = value;
                RaisePropertyChanged("TipsCount");
            }
        }

        private string _friendsCount;
        public string FriendsCount
        {
            get { return _friendsCount; }
            set 
            {
                _friendsCount = value;
                RaisePropertyChanged("FriendsCount");
            }
        }

        private Checkin _recentCheckin;
        public Checkin RecentCheckin
        {
            get
            {
                return _recentCheckin;
            }
            set
            {
                _recentCheckin = value;
                RaisePropertyChanged("RecentCheckin");
            }
        }

        public static string FormatSimpleUnitedStatesPhoneNumberMaybe(string phone)
        {
            if (phone == null)
            {
                return null;
            }

            // might be US
            if (phone.Length == 10 || 
                (phone.StartsWith("1", StringComparison.InvariantCulture) && 
                phone.Length == 11))
            {
                double d;
                if (double.TryParse(phone, out d))
                {
                    return string.Format("{0:(###) ###-####}", d);
                }
            }

            // Didn't format, sorry! In the future, consider even more options.
            // TODO: International phone numbers support?
            return phone;
        }

        public class UserDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            // todo: move out of here
            internal static FriendStatus GetFriendStatus(string s)
            {
                s = s == null ? null : s.ToLowerInvariant();
                switch (s)
                {
                    case "self":
                        return Model.FriendStatus.Self;

                    case "friend":
                        return Model.FriendStatus.Friend;

                    case "pendingyou":
                    case "pendingme":
                        return Model.FriendStatus.PendingYou;

                    case "pendingthem":
                        return Model.FriendStatus.PendingThem;

                    case null:
                    case "none":
                    default:
                        return Model.FriendStatus.None;
                }
            }

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
