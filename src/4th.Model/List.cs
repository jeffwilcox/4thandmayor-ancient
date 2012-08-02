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
using System.Windows;
using AgFx;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    /// <summary>
    /// Represents a foursquare list.
    /// </summary>
    [CachePolicy(CachePolicy.CacheThenRefresh, FiveMinutes)]
    public class List : FourSquareItemBase<ListLoadContext>
    {
        public List(ListLoadContext context)
            : base(context)
        {
        }

        public List() { }

        private string _id;
        /// <summary>
        /// Gets or sets a unique identifier for this list.
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        private string _name;
        /// <summary>
        /// Gets or sets the user-entered name for this list.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string _description;
        /// <summary>
        /// Gets or sets the user-entered description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        private CompactUser _compactUser;
        public CompactUser User
        {
            get { return _compactUser; }
            set
            {
                _compactUser = value;
                RaisePropertyChanged("User");
            }
        }

        private bool _following;
        /// <summary>
        /// Gets or sets a value indicating whether the acting user is 
        /// followign this list. Absent if there is no acting user.
        /// </summary>
        public bool IsFollowing
        {
            get { return _following; }
            set
            {
                _following = value;
                RaisePropertyChanged("IsFollowing");
            }
        }

        // TODO V4: Lists, "followers" property.
        // count and items of users who following this list. All items may not 
        // be present.

        private bool _editable;
        /// <summary>
        /// Gets or sets a value indicating whether the acting user can edit
        /// this list.
        /// </summary>
        public bool IsEditable
        {
            get { return _editable; }
            set
            {
                _editable = value;
                RaisePropertyChanged("IsEditable");
            }
        }

        private bool _isOwnList;
        public bool IsOwnList
        {
            get { return _isOwnList; }
            set
            {
                _isOwnList = value;
                RaisePropertyChanged("IsOwnList");
            }
        }

        private bool _collaborative;
        /// <summary>
        /// Gets or sets a value indicating whether this list is editable by
        /// the owner's friends.
        /// </summary>
        public bool IsCollaborative
        {
            get { return _collaborative; }
            set
            {
                _collaborative = value;
                RaisePropertyChanged("IsCollaborative");
            }
        }

        // TODO: v4: "collaborators" count and items of users who have edited 
        // this list. All items may not be present.

        private Uri _caonicalUri;
        /// <summary>
        /// Gets or sets the caonical URL for this list, e.g. https://foursquare.com/dens/list/a-brief-history-of-foursquare
        /// </summary>
        public Uri CaonicalUri
        {
            get { return _caonicalUri; }
            set
            {
                _caonicalUri = value;
                RaisePropertyChanged("CaonicalUri");
            }
        }

        private Photo _photo;
        /// <summary>
        /// Gets or sets an optional photo for this list.
        /// </summary>
        public Photo Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                RaisePropertyChanged("Photo");
            }
        }

        private int _doneCount;
        private int _venueCount;
        private int _visitedCount;

        /// <summary>
        /// Gets or sets the number of items on the list done by the viewing
        /// user.
        /// </summary>
        public int DoneCount
        {
            get { return _doneCount; }
            set
            {
                _doneCount = value;
                RaisePropertyChanged("DoneCount");
            }
        }

        /// <summary>
        /// Gets or sets the number of unique venues on the list.
        /// </summary>
        public int VenueCount
        {
            get { return _venueCount; }
            set
            {
                _venueCount = value;
                RaisePropertyChanged("VenueCount");
            }
        }

        /// <summary>
        /// Gets or sets the number of venues on the list visited by the acting
        /// user.
        /// </summary>
        public int VisitedCount
        {
            get { return _visitedCount; }
            set
            {
                _visitedCount = value;
                RaisePropertyChanged("VisitedCount");
            }
        }

        private string _visitedPlacesString;
        public string VisitedPlacesString
        {
            get { return _visitedPlacesString; }
            set
            {
                _visitedPlacesString = value;
                RaisePropertyChanged("VisitedPlacesString");
            }
        }

        private List<CompactListItem> _items;
        public List<CompactListItem> ListItems
        {
            get
            {
                return _items;
            }

            set
            {
                _items = value;
                RaisePropertyChanged("ListItems");
            }
        }

        /// <summary>
        /// Data loader that communicates with the Foursquare web services to
        /// retrieve the JSON-P for the active user's leaderboard.
        /// </summary>
        public class ListDataLoader : FourSquareDataLoaderBase<ListLoadContext>
        {
            public override LoadRequest GetLoadRequest(ListLoadContext context, Type objectType)
            {
                var parameters = new List<string>();
                //if (context.Limit != null)
                //{
                    parameters.Add("limit");
                    parameters.Add(context.Limit.ToString(CultureInfo.InvariantCulture));
                //}

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "lists/" + context.Identity,

                    GeoMethodType.Optional,
                    parameters.ToArray()));
            }

            /// <summary>
            /// Parses the JSON response to retrieve the leaderboard, parsing
            /// of individual items are handled by their compact object
            /// parse routines.
            /// </summary>
            protected override object DeserializeCore(JObject json, Type objectType, ListLoadContext context)
            {
                try
                {
                    var l = new List(context);

                    var list = json["list"];

                    l.Id = Json.TryGetJsonProperty(list, "id");
                    l.Name = Json.TryGetJsonProperty(list, "name");
                    l.Description = Json.TryGetJsonProperty(list, "description");

                    var cu = list["user"];
                    if (cu != null)
                    {
                        var ocu = CompactUser.ParseJson(cu);
                        if (ocu != null)
                        {
                            l.User = ocu;

                            l.IsOwnList = ocu.IsSelf;
                        }
                    }

                    l.IsFollowing = Json.TryGetJsonBool(list, "following");
                    
                    // TODO: v4: "followers"

                    l.IsEditable = Json.TryGetJsonBool(list, "editable");
                    l.IsCollaborative = Json.TryGetJsonBool(list, "collaborative");

                    // TODO: v4: "collaborators"

                    l.CaonicalUri = Json.TryGetUriProperty(list, "caonicalUrl");

                    var pic = list["photo"];
                    if (pic != null)
                    {
                        var opic = Photo.ParseJson(pic);
                        if (opic != null)
                        {
                            l.Photo = opic;
                        }
                    }

                    string s = Json.TryGetJsonProperty(list, "doneCount");
                    int i;
                    if (int.TryParse(s, out i))
                    {
                        l.DoneCount = i;
                    }

                    s = Json.TryGetJsonProperty(list, "venueCount");
                    if (int.TryParse(s, out i))
                    {
                        l.VenueCount = i;
                    }

                    s = Json.TryGetJsonProperty(list, "visitedCount");
                    if (int.TryParse(s, out i))
                    {
                        l.VisitedCount = i;
                    }

                    if (l.VenueCount > 0)
                    {
                        var appStrings = Application.Current as IProvideLocalizedStrings;
                        if (appStrings != null)
                        {
                            string key = "ListYouveBeenTo";
                            if (l.VenueCount == 1)
                            {
                                key = "ListYouveBeenToSingular";
                            }
                            var format = appStrings.GetLocalizedString(key);
                            if (format != null)
                            {
                                l.VisitedPlacesString = string.Format(
                                    CultureInfo.CurrentCulture,
                                    format,
                                    Math.Min(l.VisitedCount, l.VenueCount), // was reporting at times "35 out of 30" which is not cool.
                                    l.VenueCount
                                    );
                            }
                        }
                    }

                    var b = new List<CompactListItem>();
                    var lis = list["listItems"];
                    if (lis != null)
                    {
                        var items = lis["items"];
                        if (items != null)
                        {
                            foreach (var entry in items)
                            {
                                var u = CompactListItem.ParseJson(entry);
                                if (u != null)
                                {
                                    // Associate the contextual parent/list ID.
                                    u.ListId = l.Id;
                                    b.Add(u);
                                }
                            }
                        }
                    }

                    l.ListItems = b;

                    l.IsLoadComplete = true;

                    return l;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "The list information could not be returned right now.", e);
                }
            }
        }
    }
}
