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
using System.Diagnostics;
using System.Globalization;
using System.Text;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
        StandardRefreshInterval)]
    public class Venue : FourSquareItemBase<LoadContext>
    {
        public Venue(LoadContext context)
            : base(context)
        {
        }
        public Venue()
        {
        }

        private string _tagsString;
        public string TagsString
        {
            get { return _tagsString; }
            set
            {
                _tagsString = value;
                RaisePropertyChanged("TagsString");
            }
        }

        private List<string> _tags;
        public List<string> Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                RaisePropertyChanged("Tags");
            }
        }

        private bool _specialUnlocked;
        public bool SpecialUnlocked
        {
            get { return _specialUnlocked; }
            set
            {
                _specialUnlocked = value;
                RaisePropertyChanged("SpecialUnlocked");
            }
        }

        private List<SpecialGroup> _combinedSpecials;
        public List<SpecialGroup> CombinedSpecials
        {
            get { return _combinedSpecials; }
            set
            {
                _combinedSpecials = value;
                RaisePropertyChanged("CombinedSpecials");
            }
        }

        private bool _hasMenu;
        public bool HasMenu
        {
            get
            {
                return _hasMenu;
            }

            set
            {
                _hasMenu = value;
                RaisePropertyChanged("HasMenu");
            }
        }

        private SpecialGroup _nearbySpecials;
        public SpecialGroup NearbySpecials
        {
            get
            {
                return _nearbySpecials;
            }
            set
            {
                _nearbySpecials = value;
                RaisePropertyChanged("NearbySpecials");
            }
        }

        private SpecialGroup _specials;
        public SpecialGroup Specials
        {
            get
            {
                return _specials;
            }
            set
            {
                _specials = value;
                RaisePropertyChanged("Specials");
            }
        }

        private string _id;
        public string VenueId
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged("VenueId");
            }
        }

        [DependentOnProperty("Name")]
        public Uri VenueUri
        {
            get
            {
                return new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "/Views/Venue.xaml?id={0}&name={1}",
                    Uri.EscapeDataString(VenueId),
                    Uri.EscapeDataString(Name ?? string.Empty)), UriKind.Relative);
            }
        }

        [DependentOnProperty("Location")]
        public Uri MapUri
        {
            get
            {
                if (Location != null)
                {
                    return new Uri(string.Format(
                        CultureInfo.InvariantCulture,
                        "/Maps;component/MapView.xaml?lat={0}&long={1}&name={2}&address={3}&localUri={4}", 
                        Location.Latitude, 
                        Location.Longitude,
                        Uri.EscapeDataString(Name ?? string.Empty),
                        Uri.EscapeDataString(Address ?? string.Empty),
                        VenueUri == null ? string.Empty : Uri.EscapeDataString(VenueUri.ToString())
                        ), UriKind.Relative);
                }
                return null;
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private Category _pc;
        public Category PrimaryCategory
        {
            get
            {
                return _pc;
            }
            set
            {
                _pc = value;
                RaisePropertyChanged("PrimaryCategory");
            }
        }

        private string _address;
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
                RaisePropertyChanged("Address");
            }
        }

        private string _cs;
        public string CrossStreet
        {
            get
            {
                return _cs;
            }

            set
            {
                _cs = value;
                RaisePropertyChanged("CrossStreet");
            }
        }

        public string CityStateZipLine
        {
            get
            {
                // This might need to have INPCs on it!
                if (City != null && State != null && Zip != null)
                {
                    return City + ", " + State + " " + Zip;
                }
                return null;
            }
        }

        private string _city;
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                _city = value;
                RaisePropertyChanged("City");
            }
        }

        private string _state;
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                RaisePropertyChanged("State");
            }
        }

        private string _zip;
        public string Zip
        {
            get
            {
                return _zip;
            }
            set
            {
                _zip = value;
                RaisePropertyChanged("Zip");
            }
        }

        private bool _isVerified;
        public bool IsVerified
        {
            get { return _isVerified; }
            set
            {
                _isVerified = value;
                RaisePropertyChanged("IsVerified");
            }
        }

        private LocationPair _lp;
        public LocationPair Location
        {
            get
            {
                return _lp;
            }
            set
            {
                _lp = value;
                RaisePropertyChanged("Location");
            }
        }

        private CompactUser _mayor;
        public CompactUser Mayor
        {
            get { return _mayor; }
            set
            {
                _mayor = value;
                RaisePropertyChanged("Mayor");
                RaisePropertyChanged("HasMayor");
            }
        }

        public bool HasMayor
        {
            get { return _mayor != null && _mayor.First != null; }
        }

        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                RaisePropertyChanged("Phone");
            }
        }

        private string _formattedPhone;
        public string FormattedPhone
        {
            get { return _formattedPhone; }
            set
            {
                _formattedPhone = value;
                RaisePropertyChanged("FormattedPhone");
            }
        }

        [DependentOnProperty("Phone")]
        public string PhoneUri
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "tel:{0}?displayname={1}",
                    Phone ?? string.Empty,
                    Uri.EscapeDataString(Name ?? string.Empty)
                    );
            }
        }

        private Uri _shortWebUri;

        public Uri ShortWebUri
        {
            get { return _shortWebUri; }
            set
            {
                _shortWebUri = value;
                RaisePropertyChanged("ShortWebUri");
            }
        }

        private string _hereNow;
        public string HereNow
        {
            get { return _hereNow; }
            set
            {
                if (_hereNow != value)
                {
                    _hereNow = value;
                    RaisePropertyChanged("HereNow");
                }
                //RaisePropertyChanged("HasHereNow");
            }
        }
        //public bool HasHereNow { get { return _hereNow != null; } }

        private bool _hasHereNow;
        public bool HasHereNow
        {
            get { return _hasHereNow; }
            set
            {
                _hasHereNow = value;
                RaisePropertyChanged("HasHereNow");
            }
        }

        private string _othersHereNowText;
        public string OthersHereNowText
        {
            get { return _othersHereNowText; }
            set
            {
                _othersHereNowText = value;
                RaisePropertyChanged("OthersHereNowText");
            }
        }

        private List<CheckinsGroup> _hereNowGroups;
        public List<CheckinsGroup> HereNowGroups
        {
            get
            {
                return _hereNowGroups;
            }
            set
            {
                _hereNowGroups = value;
                // no PNC done above by HereNow during parse
            }
        }

        private bool _hasToDo;
        public bool HasToDo
        {
            get { return _hasToDo; }
            set
            {
                _hasToDo = value;
                RaisePropertyChanged("HasToDo");
            }
        }

        private List<Todo> _todos;
        public List<Todo> Todos
        {
            get { return _todos; }
            set { _todos = value;
                RaisePropertyChanged("Todos");
            }
        }

        private int _eventsCount;
        public int EventsCount
        {
            get
            {
                return _eventsCount;
            }
            set
            {
                _eventsCount = value;
                RaisePropertyChanged("EventsCount");
            }
        }

        [DependentOnProperty("EventsCount")]
        public bool HasEvents
        {
            get
            {
                return _eventsCount > 0 && !string.IsNullOrEmpty(_eventsSummary);
            }
        }

        private VenueEvents _ve;
        public VenueEvents Events
        {
            get {
                if (_ve == null)
                {
                    _ve = DataManager.Current.Load<VenueEvents>(
                        new LoadContext(LoadContext.Identity));
                }
                return _ve; 
            }
            set
            {
                _ve = value;
                RaisePropertyChanged("Events");
            }
        }

        private string _eventsSummary;
        public string EventsSummary
        {
            get { return _eventsSummary; }
            set
            {
                _eventsSummary = value;
                RaisePropertyChanged("EventsSummary");
            }
        }

        [DependentOnProperty("Todos")]
        public string TodosText
        {
            get
            {
                if (_todos != null && _todos.Count > 0)
                {
                    return _todos.Count == 1
                               ? "1 to-do here"
                               : string.Format(CultureInfo.CurrentCulture, "{0} to-dos here", _todos.Count);
                }
                return null;
            }
        }


        private string _twitter;
        public string Twitter
        {
            get { return _twitter; }
            set
            {
                _twitter = value;
                RaisePropertyChanged("Twitter");
            }
        }

        private Uri _homepage;
        public Uri Homepage
        {
            get { return _homepage; }
            set
            {
                _homepage = value;
                RaisePropertyChanged("Homepage");
            }
        }

        private List<TipGroup> _tips;
        public List<TipGroup> TipGroups
        {
            get { return _tips; }
            set
            {
                _tips = value;
                RaisePropertyChanged("TipGroups");
            }
        }

        [DependentOnProperty("TipGroups")]
        public bool HasTips { get { return _tips != null && _tips.Count > 0; } }

        private string _tipsCountText;
        [DependentOnProperty("TipGroups")]//?
        public string TipsCountText
        {
            get { return _tipsCountText; }
            set
            {
                _tipsCountText = value;
                RaisePropertyChanged("TipsCountText");
            }
        }

        private string _photosCount;
        public string PhotosCount
        {
            get
            {
                return _photosCount;
            }
            set
            {
                _photosCount = value;
                RaisePropertyChanged("PhotosCount");
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        private List<PhotoGroup> _photoGroups;
        public List<PhotoGroup> PhotoGroups
        {
            get
            {
                return _photoGroups;
            }
            set
            {
                _photoGroups = value;
                RaisePropertyChanged("PhotoGroups");
            }
        }

        private int _totalPeople;
        public int TotalPeople
        {
            get { return _totalPeople; }
            set
            {
                _totalPeople= value;
                RaisePropertyChanged("TotalPeople");
            }
        }

        private int _beenHere;
        public int BeenHere
        {
            get { return _beenHere; }
            set
            {
                _beenHere = value;
                RaisePropertyChanged("BeenHere");
            }
        }

        private int _checkins;
        public int Checkins
        {
            get
            {
                return _checkins;
            }
            set
            {
                _checkins = value;
                RaisePropertyChanged("Checkins");
            }
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }

        public class VenueDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                var id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "venues/" + id,
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                var v = new Venue(context);
                try
                {
                    var venue = json["venue"];

                    CompactVenue bv = CompactVenue.ParseJson(venue);
                    v.VenueId = bv.VenueId;
                    Debug.Assert(v.VenueId != null);

                    // TODO: Consider architecture.
                    v.Location = bv.Location;
                    v.Address = bv.Address;
                    v.City = bv.City;
                    v.CrossStreet = bv.CrossStreet;
                    v.State = bv.State;
                    v.Zip = bv.Zip;
                    v.Name = bv.Name;

                    var menuEntry = venue["menu"];
                    if (menuEntry != null)
                    {
                        string mobileMenu = Json.TryGetJsonProperty(menuEntry, "mobileUrl");
                        if (!string.IsNullOrEmpty(mobileMenu))
                        {
                            v.HasMenu = true;
                        }
                    }

                    string desc = Checkin.SanitizeString(Json.TryGetJsonProperty(venue, "description"));
                    if (desc != null)
                    {
                        v.Description = desc;
                    }

                    // TODO: V2: timeZone // timeZone: "America/New_York"

                    string swu = Json.TryGetJsonProperty(venue, "shortUrl");
                    if (swu != null)
                    {
                        v.ShortWebUri = new Uri(swu);
                    }

                    string verif = Json.TryGetJsonProperty(venue, "verified");
                    if (verif != null && verif.ToLowerInvariant() == "true")
                    {
                        v.IsVerified = true;
                    }

                    // older code.
                    string phone = Json.TryGetJsonProperty(venue, "phone");
                    if (phone != null)
                    {
                        v.Phone = phone; // User.FormatSimpleUnitedStatesPhoneNumberMaybe(phone);
                    }

                    // newer code for contact stuff.
                    var contact = venue["contact"];
                    if (contact != null)
                    {
                        v.Twitter = Json.TryGetJsonProperty(contact, "twitter");

                        string newerPhone = Json.TryGetJsonProperty(contact, "phone");
                        if (newerPhone != null)
                        {
                            v.Phone = newerPhone;
                        }

                        string bestPhone = Json.TryGetJsonProperty(contact, "formattedPhone");
                        if (bestPhone != null)
                        {
                            v.FormattedPhone = bestPhone;
                        }

                        // fallback.
                        if (v.FormattedPhone == null && !string.IsNullOrEmpty(v.Phone))
                        {
                            v.FormattedPhone = User.FormatSimpleUnitedStatesPhoneNumberMaybe(v.Phone);
                        }
                    }

                    string homepage = Json.TryGetJsonProperty(venue, "url");
                    if (!string.IsNullOrEmpty(homepage))
                    {
                        v.Homepage = new Uri(homepage, UriKind.Absolute);
                    }

                    var todos = venue["todos"];
                    if (todos != null)
                    {
                        var items = todos["items"];
                        if (items != null)
                        {
                            var todosList = new List<Todo>();
                            foreach (var todo in items)
                            {
                                var td = Todo.ParseJson(todo);
                                if (td != null)
                                {
                                    todosList.Add(td);
                                }
                            }
                            v.Todos = todosList;
                        }
                    }

                    var events = venue["events"];
                    if (events != null)
                    {
                        string pct = Json.TryGetJsonProperty(events, "count");
                        int pcti;
                        if (int.TryParse(pct, out pcti))
                        {
                            v.EventsCount = pcti;

                            if (pcti > 0)
                            {
                                v.EventsSummary = Json.TryGetJsonProperty(events, "summary");
                            }

                            if (v.HasEvents)
                            {
                                // ? will this work ?
                                v.Events = DataManager.Current.Load<VenueEvents>(
                                    new LoadContext(
                                        v.LoadContext.Identity
                                        )
                                        );
                            }
                        }
                    }

                    var photos = venue["photos"];
                    if (photos != null)
                    {
                        string pct = Json.TryGetJsonProperty(photos, "count");
                        int pcti;
                        if (int.TryParse(pct, out pcti))
                        {
                            if (pcti == 1)
                            {
                                v.PhotosCount = "1 photo";
                            }
                            else if (pcti > 1)
                            {
                                v.PhotosCount = pcti.ToString() + " photos";
                            }

                            // get the grounds
                            if (pcti > 0)
                            {
                                var groups = photos["groups"];
                                if (groups != null)
                                {
                                    var pg = new List<PhotoGroup>();
                                    foreach (var item in groups)
                                    {
                                        string name = Json.TryGetJsonProperty(item, "name");
                                        var items = item["items"];
                                        var group = new PhotoGroup();
                                        group.Name = name;
                                        foreach (var it in items)
                                        {
                                            Photo p = Photo.ParseJson(it);
                                            if (p != null)
                                            {
                                                group.Add(p);
                                            }
                                        }
                                        if (group.Count > 0)
                                        {
                                            pg.Add(group);
                                        }
                                    }
                                    if (pg.Count > 0)
                                    {
                                        v.PhotoGroups = pg;
                                    }
                                }
                            }
                        }
                    }
                    // Allowing the GIC to show the empty template.
                    if (v.PhotoGroups == null)
                    {
                        v.PhotoGroups = new List<PhotoGroup>();
                    }

                    string htodo = Json.TryGetJsonProperty(venue, "hasTodo"); // checkin mode only
                    if (htodo != null && htodo.ToLowerInvariant() == "true")
                    {
                        v.HasToDo = true;
                    }

                    v.HereNow = "Nobody";
                    bool hereNow = false;
                    var herenow = venue["hereNow"];
                    if (herenow != null)
                    {
                        bool isSelfHere = false;

                        string summary = Json.TryGetJsonProperty(herenow, "summary");
                        if (summary != null)
                        {
                            v.HereNow = summary;
                        }

                        var groups = herenow["groups"];
                        string hn = Json.TryGetJsonProperty(herenow, "count");
                        if (/*!string.IsNullOrEmpty(hn) &&*/ groups != null) // I still want to compute this anyway.
                        {
                            int totalCount = int.Parse(hn, CultureInfo.InvariantCulture);
                            int remainingCount = totalCount;

                            var hereNowGroups = new List<CheckinsGroup>();
                            foreach (var group in groups)
                            {
                                string type = Json.TryGetJsonProperty(group, "type"); // friends, others
                                string name = Json.TryGetJsonProperty(group, "name"); // "friends here", "other people here"
                                string count = Json.TryGetJsonProperty(group, "count"); // the count, an int
                                var items = group["items"];

                                if (items != null)
                                {
                                    var cg = new CheckinsGroup { Name = name };

                                    foreach (var item in items)
                                    {
                                        Checkin cc = Checkin.ParseJson(item);
                                        remainingCount--;
                                        if (cc.User != null && cc.User.Relationship == FriendStatus.Self)
                                        {
                                            // Self!
                                            var selfGroup = new CheckinsGroup {Name = "you're here!"};
                                            isSelfHere = true;
                                            selfGroup.Add(cc);
                                            hereNowGroups.Add(selfGroup);
                                        }
                                        else
                                        {
                                            cg.Add(cc);
                                        }
                                    }

                                    if (cg.Count > 0)
                                    {
                                        hereNowGroups.Add(cg);
                                    }
                                }
                            }

                            // Special last item with the remainder count.
                            if (remainingCount > 0 && hereNowGroups.Count > 0)
                            {
                                var lastGroup = hereNowGroups[hereNowGroups.Count - 1];
                                var hnr = new Checkin
                                {
                                    HereNowRemainderText =
                                        remainingCount == 1
                                            ? "... plus 1 person"
                                            : "... plus " + remainingCount.ToString() + " people",
                                };
                                lastGroup.Add(hnr);
                            }

                            v.HereNowGroups = hereNowGroups;

                            // subtract one for self
                            if (isSelfHere) totalCount--;
                            string prefix = (isSelfHere ? "You and " : string.Empty);
                            if (string.IsNullOrEmpty(summary))
                            {
                                if (totalCount > 1)
                                {
                                    v.HereNow = prefix + Json.GetPrettyInt(totalCount) + " " + (isSelfHere ? "other " : "") + "people";
                                }
                                else if (totalCount == 1)
                                {
                                    v.HereNow = prefix + "1 " + (isSelfHere ? "other " : "") + "person";
                                }
                                else if (totalCount == 0 && isSelfHere)
                                {
                                    v.HereNow = "You are here";
                                }
                            }

                            if (totalCount > 0)
                            {
                                hereNow = true;
                            }
                        }
                    }

                    v.HasHereNow = hereNow;

                    var stats = venue["stats"];
                    if (stats != null)
                    {
                        string checkins = Json.TryGetJsonProperty(stats, "checkinsCount");
                        if (checkins != null)
                        {
                            int i;
                            if (int.TryParse(checkins, out i))
                            {
                                v.Checkins = i;
                            }
                        }

                        checkins = Json.TryGetJsonProperty(stats, "usersCount");
                        if (checkins != null)
                        {
                            int i;
                            if (int.TryParse(checkins, out i))
                            {
                                v.TotalPeople = i;
                            }
                        }
                    }

                    var mayor = venue["mayor"];
                    if (mayor != null)
                    {
                        string mayorCheckinCount = Json.TryGetJsonProperty(mayor, "count");

                        var user = mayor["user"];
                        if (user != null)
                        {
                            v.Mayor = CompactUser.ParseJson(user);
                        }

                        // Note there is a mayor.count property, it is the num
                        // of checkins in the last 60 days.
                    }

                    var beenHere = venue["beenHere"];
                    if (beenHere != null)
                    {
                        string c = Json.TryGetJsonProperty(beenHere, "count");
                        if (c != null)
                        {
                            int i;
                            if (int.TryParse(c, out i))
                            {
                                v.BeenHere = i;
                            }
                        }
                    }

                    var tips = venue["tips"];
                    if (tips != null)
                    {
                        string tipsCountStr = Json.TryGetJsonProperty(tips, "count");
                        if (tipsCountStr != null)
                        {
                            int tc;
                            if (int.TryParse(tipsCountStr, out tc))
                            {
                                if (tc <= 0)
                                {
                                    tipsCountStr = "No tips";
                                }
                                else if (tc == 1)
                                {
                                    tipsCountStr = "1 tip";
                                }
                                else
                                {
                                    tipsCountStr = tc.ToString() + " tips";
                                }
                            }
                        }
                        else
                        {
                            tipsCountStr = "No tips";
                        }
                        v.TipsCountText = tipsCountStr;

                        var ml = new List<TipGroup>();
                        var tipGroups = tips["groups"];
                        if (tipGroups != null)
                        {
                            foreach (var tipGroup in tipGroups)
                            {
                                //string groupType = Json.TryGetJsonProperty(tipGroup, "type"); // others, ???v2
                                string groupName = Json.TryGetJsonProperty(tipGroup, "name"); // tips from others
                                //string countStr = Json.TryGetJsonProperty(tipGroup, "count");

                                var tipSet = tipGroup["items"];
                                if (tipSet != null)
                                {
                                    TipGroup tg = new TipGroup {Name = groupName};
                                    foreach (var tip in tipSet)
                                    {
                                        Tip t = Tip.ParseJson(tip, typeof(Model.Venue), (string)context.Identity);
                                        if (t != null)
                                        {
                                            tg.Add(t);
                                        }
                                    }
                                    if (tg.Count > 0)
                                    {
                                        ml.Add(tg);
                                    }
                                }
                            }
                        }
                        v.TipGroups = ml;
                    }

                    var specials = venue["specials"];
                    var specialsList = new SpecialGroup {Name = "specials here"};
                    if (specials != null)
                    {
                        try
                        {
                            var items = specials["items"];
                            if (items != null)
                            {
                                foreach (var s in items)
                                {
                                    CompactSpecial cs = CompactSpecial.ParseJson(s, v.VenueId);
                                    if (cs != null)
                                    {
                                        specialsList.Add(cs);

                                        if (cs.IsUnlocked)
                                        {
                                            v.SpecialUnlocked = true;
                                        }
                                    }
                                }
                                if (specialsList.Count == 1)
                                {
                                    specialsList.Name = "special here";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // 3.4 moves to a new Foursquare API version and so
                            // we don't want old cached data to throw here.
                        }
                    }
                    v.Specials = specialsList;

                    var nearby = venue["specialsNearby"];
                    var nearbySpecialsList = new SpecialGroup {Name = "specials nearby"};
                    if (nearby != null)
                    {
                        foreach (var s in nearby)
                        {
                            CompactSpecial cs = CompactSpecial.ParseJson(s, null);
                            if (cs != null)
                            {
                                nearbySpecialsList.Add(cs);
                            }
                        }
                        if (nearbySpecialsList.Count == 1)
                        {
                            nearbySpecialsList.Name = "special nearby";
                        }
                    }
                    v.NearbySpecials = nearbySpecialsList;

                    var cmb = new List<SpecialGroup>(2);
                    if (specialsList.Count > 0)
                    {
                        cmb.Add(specialsList);
                    }
                    if (nearbySpecialsList.Count > 0)
                    {
                        cmb.Add(nearbySpecialsList);
                    }
                    v.CombinedSpecials = cmb;

                    var categories = venue["categories"];
                    if (categories != null)
                    {
                        foreach (var cat in categories)
                        {
                            var cc = Category.ParseJson(cat);
                            if (cc != null && cc.IsPrimary)
                            {
                                v.PrimaryCategory = cc;
                                break;
                            }
                            // Just the first actually!
                            break;
                        }
                    }

                    // stats
                    // .herenow
                    // .checkins
                    // beenhere: me:true; 

                    // specials

                    var tags = venue["tags"];
                    if (tags != null)
                    {
                        List<string> tl = new List<string>();
                        foreach (string tag in tags)
                        {
                            tl.Add(tag);
                        }

                        if (tl.Count > 0)
                        {
                            v.Tags = tl;

                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < tl.Count; i++)
                            {
                                if (i > 0)
                                {
                                    sb.Append(", ");
                                }

                                sb.Append(tl[i]);
                            }

                            v.TagsString = sb.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read the venue information.", e);
                }

                v.IsLoadComplete = true;

                return v;
            }
        }
    }
}
