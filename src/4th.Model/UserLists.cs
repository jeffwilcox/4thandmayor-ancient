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
    [CachePolicy(CachePolicy.CacheThenRefresh, StandardRefreshInterval)]
    public class UserLists : FourSquareItemBase<LoadContext>
    {
        public UserLists()
            : base()
        {
        }

        public UserLists(LoadContext context)
            : base(context)
        {
        }

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

        public class UserListsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                var id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/lists",

                        GeoMethodType.Optional

                        //"sort",
                        //sortType
                        ));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new UserLists(context);

                    var lists = json["lists"];
                    if (lists != null)
                    {
                        var groups = lists["groups"];
                        b.Groups = ParseListGroups(groups);
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to find all of your lists.", e);
                }
            }

            internal static List<ListsList> ParseListGroups(JToken groups)
            {
                List<ListsList> ggg = new List<ListsList>();
                if (groups != null)
                {
                    ListsList lastToAdd = null;

                    foreach (var group in groups)
                    {
                        string type = Json.TryGetJsonProperty(group, "type");
                        var items = group["items"];
                        if (items != null)
                        {
                            var ni = new ListsList();
                            ni.Type = type;

                            string name = Json.TryGetJsonProperty(group, "name");
                            if (!string.IsNullOrEmpty(name))
                            {
                                ni.Name = name;
                            }
                            else
                            {
                                if (ni.Type == "todos")
                                {
                                    // LOCALIZE:
                                    ni.Name = "My To-dos";
                                }
                                else
                                {
                                    // can't be null...

                                    ni.Name = string.Empty; // !!! could be a bug farm.
                                    // warning, for 'My To-Do List' this is null!

                                    // LOCALIZE:
                                    ni.Name = "My Lists";

                                    lastToAdd = ni;
                                }
                            }

                            foreach (var item in items)
                            {
                                var list = CompactList.ParseJson(item);
                                if (list != null)
                                {
                                    ni.Add(list);
                                }
                            }

                            // This does mean that if there are more 
                            // than 1 list that has a null or empty 
                            // title, it will get hidden.
                            if (lastToAdd != ni)
                            {
                                ggg.Add(ni);
                            }
                        }
                    }

                    if (lastToAdd != null)
                    {
                        ggg.Add(lastToAdd);
                        lastToAdd = null;
                    }
                }

                return ggg;
            }
        }
    }
}
