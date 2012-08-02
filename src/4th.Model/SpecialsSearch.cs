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
using System.Diagnostics;
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
    public class SpecialsSearch : FourSquareItemBase<LimitingLoadContext>
    {
        public SpecialsSearch(LimitingLoadContext context)
            : base(context)
        {
        }

        public SpecialsSearch() { }

        private string _specialsText;
        public string SpecialsText
        {
            get { return _specialsText; }
            set
            {
                _specialsText = value;
                RaisePropertyChanged("SpecialsText");
            }
        }

        private SpecialGroup _specials;
        public SpecialGroup Specials
        {
            get { return _specials; }
            set
            {
                _specials = value;
                RaisePropertyChanged("Specials");
            }
        }

        public class SpecialsSearchDataLoader : FourSquareDataLoaderBase<LimitingLoadContext>
        {
            public override LoadRequest GetLoadRequest(LimitingLoadContext context, Type objectType)
            {
                Debug.Assert(context.Limit <= 100);

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "specials/search",
                    GeoMethodType.Required,

                    "limit",
                    context.Limit.ToString(CultureInfo.InvariantCulture)
                    ));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LimitingLoadContext context)
            {
                try
                {
                    var nv = new SpecialsSearch(context);
                    nv.IgnoreRaisingPropertyChanges = true;

                    var specials = json["specials"];
                    // LOCALIZE:
                    nv.SpecialsText = "no specials";
                    if (specials != null)
                    {
                        // cout
                        var items = specials["items"];
                        nv.Specials = new SpecialGroup();
                        foreach (var item in items)
                        {
                            CompactSpecial special = CompactSpecial.ParseJson(item, null);
                            nv.Specials.Add(special);
                        }

                        if (nv.Specials.Count > 1)
                        {
                            // LOCALIZE:
                            nv.SpecialsText = nv.Specials.Count.ToString(CultureInfo.InvariantCulture) +
                                              " specials nearby";
                        }
                        else
                        {
                            if (nv.Specials.Count == 1)
                            {
                                // LOCALIZE:
                                nv.SpecialsText = "1 special nearby";
                            }
                        }
                    }

                    nv.IgnoreRaisingPropertyChanges = false;
                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    // LOCALIZE:
                    throw new UserIntendedException("Nearby specials could not be loaded right now, sorry.", e);
                }
            }
        }
    }
}
