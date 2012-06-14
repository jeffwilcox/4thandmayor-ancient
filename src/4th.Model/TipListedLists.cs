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
using AgFx;
using Newtonsoft.Json.Linq;
using System.Globalization;
using JeffWilcox.Controls;
using System.Windows;

namespace JeffWilcox.FourthAndMayor.Model
{
    /// <summary>
    /// Represents a foursquare list.
    /// </summary>
    [CachePolicy(CachePolicy.CacheThenRefresh, FiveMinutes)]
    public class TipListedLists : FourSquareItemBase<LoadContext> // tipId
    {
        public TipListedLists(LoadContext context)
            : base(context)
        {
        }

        public TipListedLists() { }

        private List<ListsList> _groups;
        public List<ListsList> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                RaisePropertyChanged("Groups");
            }
        }

        /// <summary>
        /// Data loader that communicates with the Foursquare web services to
        /// retrieve the JSON-P for the active user's leaderboard.
        /// </summary>
        public class ListListedDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "tips/" + context.Identity + "/listed",

                    GeoMethodType.Optional,
                    null));
            }

            /// <summary>
            /// Parses the JSON response to retrieve the leaderboard, parsing
            /// of individual items are handled by their compact object
            /// parse routines.
            /// </summary>
            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var l = new TipListedLists(context);

                    var lists = json["lists"];

                    if (lists != null)
                    {
                        var groups = lists["groups"];
                        if (groups != null)
                        {
                            var p = JeffWilcox.FourthAndMayor.Model.UserLists.UserListsDataLoader.ParseListGroups(groups);
                            if (p != null)
                            {
                                l.Groups = p;
                            }
                        }
                    }

                    l.IsLoadComplete = true;

                    return l;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "Listed list information could not be returned right now.", e);
                }
            }
        }
    }
}
