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
    public class UserCheckins : FourSquareItemBase<LoadContext>
    {
        public UserCheckins()
            : base()
        {
        }

        public UserCheckins(LoadContext context)
            : base(context)
        {
        }

        private List<Checkin> _recent;
        public List<Checkin> Recent
        {
            get { return _recent; }
            set
            {
                _recent = value;
                RaisePropertyChanged("Recent");
            }
        }

        public class UserCheckinsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/checkins",
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new UserCheckins(context);

                    var sets = json["checkins"];
                    if (sets != null)
                    {
                        var items = sets["items"];
                        if (items != null)
                        {
                            var list = new List<Checkin>();
                            foreach (var s in items)
                            {
                                Checkin c = Checkin.ParseJson(s);
                                list.Add(c);
                            }
                            b.Recent = list;
                        }
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read the list of your recent check-ins.", e);
                }
            }
        }
    }
}
