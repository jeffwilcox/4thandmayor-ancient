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
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class UpdatesLoadContext : LimitingLoadContext
    {
        public UpdatesLoadContext(object identifier) : base(identifier)
        {
            Limit = 25; // My default. Fsq default suggested is 20.
        }

        public int Offset { get; set; }

        protected override string GenerateKey()
        {
            return base.GenerateKey() + "_" + Offset.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Latest notifications for the user.
    /// </summary>
    [CachePolicy(CachePolicy.ValidCacheOnly, FiveMinutes)]
    public class Updates : FourSquareItemBase<UpdatesLoadContext>
    {
        public Updates(UpdatesLoadContext context)
            : base(context)
        {
        }

        public Updates() { }

        private int _watermark;
        public int HighWatermark
        {
            get
            { return _watermark; }
            set
            {
                _watermark = value;
                RaisePropertyChanged("HighWatermark");
            }
        }

        private List<Update> _updates;
        public List<Update> LatestUpdates
        {
            get { return _updates; }
            set
            {
                _updates = value;
                RaisePropertyChanged("LatestUpdates");
            }
        }

        /// <summary>
        /// Data loader that communicates with the Foursquare web services to
        /// retrieve the JSON-P for the active user's leaderboard.
        /// </summary>
        public class UpdatesDataLoader : FourSquareDataLoaderBase<UpdatesLoadContext>
        {
            public override LoadRequest GetLoadRequest(UpdatesLoadContext context, Type objectType)
            {
                var parameters = new List<string>();
                parameters.Add("offset");
                parameters.Add(context.Offset.ToString(CultureInfo.InvariantCulture));

                parameters.Add("limit");
                parameters.Add(context.Limit.ToString(CultureInfo.InvariantCulture));

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                    "updates/notifications",
                    GeoMethodType.Optional,
                    parameters.ToArray()));
            }

            /// <summary>
            /// Parses the JSON response to retrieve the leaderboard, parsing
            /// of individual items are handled by their compact object
            /// parse routines.
            /// </summary>
            protected override object DeserializeCore(JObject json, Type objectType, UpdatesLoadContext context)
            {
                try
                {
                    var nv = new Updates(context);

                    int highWatermark = 0;

                    var results = json["notifications"];
                    var b = new List<Update>();
                    if (results != null)
                    {
                        var items = results["items"];
                        if (items != null)
                        {
                            foreach (var entry in items)
                            {
                                var u = Update.ParseJson(entry);
                                if (u != null)
                                {
                                    b.Add(u);

                                    if (u.CreatedAt > 0 && u.CreatedAt > highWatermark)
                                    {
                                        highWatermark = u.CreatedAt;
                                    }
                                }
                            }
                        }
                    }

                    nv.HighWatermark = highWatermark;

                    nv.LatestUpdates = b;

                    nv.IsLoadComplete = true;

                    return nv;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "Your updates could not be refreshed right now.", e);
                }
            }
        }
    }
}
