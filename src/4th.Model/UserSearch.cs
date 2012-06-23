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
    public class UserSearch : FourSquareItemBase<UserSearchLoadContext>
    {
        public UserSearch(UserSearchLoadContext context)
            : base(context)
        {
        }

        public UserSearch() { }

        private List<CompactUser> _results;
        public List<CompactUser> Results
        {
            get { return _results; }
            set
            {
                _results = value;
                RaisePropertyChanged("Results");
            }
        }

        private List<CompactUser> _nyf;
        public List<CompactUser> NotYetFriends
        {
            get { return _nyf; }
            set
            {
                _nyf = value;
                RaisePropertyChanged("NotYetFriends");
            }
        }

        public class UserSearchDataLoader : FourSquareDataLoaderBase<UserSearchLoadContext>
        {
            public override LoadRequest GetLoadRequest(UserSearchLoadContext context, Type objectType)
            {
                string typeString =
                    context.SearchType == UserSearchLoadContext.UserSearchType.TwitterSource
                        ? "twitterSource"
                        : context.SearchType.ToString().ToLowerInvariant();

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "users/search",
                    GeoMethodType.Optional,

                    typeString,
                    context.Query));
            }

            protected override object DeserializeCore(JObject json, Type objectType, UserSearchLoadContext context)
            {
                try
                {
                    var nv = new UserSearch(context);
                    nv.IgnoreRaisingPropertyChanges = true;

                    var results = json["results"];
                    nv.Results = new List<CompactUser>();
                    nv.NotYetFriends = new List<CompactUser>();
                    if (results != null)
                    {
                        foreach (var user in results)
                        {
                            var u = CompactUser.ParseJson(user);
                            if (u != null)
                            {
                                nv.Results.Add(u);

                                if (!u.IsFriend && !(u.Relationship == FriendStatus.PendingThem))
                                {
                                    nv.NotYetFriends.Add(u);
                                }
                            }
                        }
                    }

                    nv.IgnoreRaisingPropertyChanges = false;
                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "Your search could not be completed at this time.", e);
                }
            }
        }
    }
}
