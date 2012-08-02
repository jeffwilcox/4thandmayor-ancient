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
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class DetailedTip : FourSquareItemBase<LoadContext>
    {
        public DetailedTip()
        {
        }

        public DetailedTip(LoadContext context)
            : base(context)
        {
        }

        private Tip _tip;

        public Tip CompactTip
        {
            get { return _tip; }
            set
            {
                _tip = value;
                RaisePropertyChanged("CompactTip");
            }
        }

        public class DetailedTipDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                string id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "tips/" + id,
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var nv = new DetailedTip(context);

                    var tip = json["tip"];

                    var compactTip = Tip.ParseJson(tip, typeof(DetailedTip), null);
                    if (compactTip != null)
                    {
                        nv.CompactTip = compactTip;
                    }

                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read the tip.", e);
                }
            }
        }
    }
}