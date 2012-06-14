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
    public class CheckinResponse : NotifyPropertyChangedBase
    {
        private string _checkinId;
        public string CheckinId
        {
            get
            {
                return _checkinId;
            }
            set
            {
                _checkinId = value;
                RaisePropertyChanged("CheckinId");
            }
        }

        private DateTime _created;
        public DateTime Created
        {
            get { return _created; }
            set
            {
                _created = value;
                RaisePropertyChanged("Created");
            }
        }

        private CompactVenue _venue;
        public CompactVenue Venue
        {
            get { return _venue; }
            set
            {
                _venue = value;
                RaisePropertyChanged("Venue");
            }
        }

        public string UniqueId { get; private set; }

        public static CheckinResponse Rehydrate(string uniqueId)
        {
            var tt = new TombstoningText("response");
            if (tt.Load(uniqueId))
            {
                var text = tt.Text;
                var json = JArray.Parse(text);
                if (json != null)
                {
                    // TODO: Fix this pattern where unique ID is changing.
                    var o = ParseJson(json);
                    o.UniqueId = uniqueId;
                    return o;
                }
            }

            return null;
        }

        public static CheckinResponse ParseJson(JToken json)
        {
            var r = new CheckinResponse();

            var tt = new TombstoningText("response");
            tt.Text = json.ToString();
            tt.Save(r.UniqueId);

            try
            {
                var checkin = json["checkin"]; // (JArray)json["checkin"];
                // string type = Json.TryGetJsonProperty(checkin, "type");
                // checkin,shout,venueless

                string created = Json.TryGetJsonProperty(checkin, "createdAt");
                if (created != null)
                {
                    DateTime dtc = UnixDate.ToDateTime(created);
                    r.Created = dtc;
                }

                r.CheckinId = Json.TryGetJsonProperty(checkin, "id");

                var venue = checkin["venue"];
                if (venue != null)
                {
                    r.Venue = CompactVenue.ParseJson(venue);
                }

                return r;
            }
            catch (Exception e)
            {
                throw new UserIntendedException(
                    "There was a problem trying to check-in, please try again later.", e);
            }
        }
    }
}
