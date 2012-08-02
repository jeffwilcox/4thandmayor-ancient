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
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class UserTips : FourSquareItemBase<LoadContext>
    {
        public UserTips()
            : base()
        {
        }

        public UserTips(LoadContext context)
            : base(context)
        {
        }

        private List<Tip> _tips;
        public List<Tip> Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                RaisePropertyChanged("Tips");
            }
        }

        public class UserTipsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {

            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                var id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/tips",
                        GeoMethodType.None,
                        "sort", "recent"));
            }

            // TODO: the iOS client offers sets the sort property to allow for recent, nearby, popular... hard-coding to recent for now! need to determine what kind of UI is useful for this kind of sort... and yeah it's probably yet another pivot...

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new UserTips(context);

                    var sets = json["tips"];
                    if (sets != null)
                    {
                        List<Tip> tips = new List<Tip>();

                        var items = sets["items"];
                        if (items != null)
                        foreach (var tip in items)
                        {
                            Tip t = Tip.ParseJson(tip, typeof (UserTips), (string) context.Identity);
                            if (t != null)
                            {
                                tips.Add(t);
                            }
                        }

                        b.Tips = tips;
                    }

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read the list of tips.", e);
                }
            }
        }
    }
}
