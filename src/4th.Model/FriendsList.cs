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

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class FriendsList : FourSquareItemBase<LoadContext>
    {
        public FriendsList()
        {
        }

        public FriendsList(LoadContext context)
            : base(context)
        {
        }

        private List<CompactUser> _friends;

        public bool HasFriends
        {
            get
            {
                return _friends != null && _friends.Count > 0;
            }
        }

        public List<CompactUser> Friends
        {
            get { return _friends; }
            set
            {
                _friends = value;
                RaisePropertyChanged("Friends");
                RaisePropertyChanged("HasFriends");
            }
        }

        public class FriendsListDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/friends",
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var nv = new FriendsList(context);

                    var friends = json["friends"];
                    var list = new List<CompactUser>();

                    var items = friends["items"];

                    foreach (JToken friend in items)
                    {
                        list.Add(CompactUser.ParseJson(friend));
                    }
                    nv.Friends = list;

                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read the list of friends.", e);
                }
            }
        }
    }
}