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
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class Special : FourSquareItemBase<SpecialLoadContext>
    {
        public Special()
        {
        }

        public Special(SpecialLoadContext context)
            : base(context)
        {
        }

        private CompactSpecial _special;

        public CompactSpecial CompactSpecial
        {
            get { return _special; }
            set
            {
                _special = value;
                RaisePropertyChanged("CompactSpecial");
            }
        }

        private Venue _venue;

        public Venue Venue
        {
            get { return _venue; }
            set
            {
                _venue = value;
                RaisePropertyChanged("Venue");
            }
        }

        public class SpecialDataLoader : FourSquareDataLoaderBase<SpecialLoadContext>
        {
            public override LoadRequest GetLoadRequest(SpecialLoadContext context, Type objectType)
            {
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "specials/" + context.SpecialId,

                        GeoMethodType.None,

                        "venueId",
                        context.VenueId));
            }

            protected override object DeserializeCore(JObject json, Type objectType, SpecialLoadContext context)
            {
                try
                {
                    var nv = new Special(context);

                    var special = json["special"];
                    if (special != null)
                    {
                        nv.CompactSpecial = CompactSpecial.ParseJson(special, context.VenueId);
                    }

                    // Should be quick since this usually happens right after a
                    // check-in.
                    if (!string.IsNullOrEmpty(context.VenueId))
                    {
                        nv.Venue = DataManager.Current.Load<Venue>(context.VenueId,
                                                        (venue) =>
                                                            {
                                                                //nv.Venue
                                                                nv.IsLoadComplete = true;
                                                            },
                                                        (error) =>
                                                            {
                                                                nv.IsLoadComplete = true;
                                                            });
                    }
                    //else
                    //{
                        // This really is probably an error condition.
                        
                    
                    nv.IsLoadComplete = true;


                    //}

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read information about the special.", e);
                }
            }
        }
    }
}