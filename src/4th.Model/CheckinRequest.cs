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

using System.Collections.Generic;
using AgFx;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class CheckinRequest
    {
        public static CheckinRequest VenueCheckin(Venue venue, bool tweet, bool fb)
        {
            var cr = new CheckinRequest
            {
                ActualVenue = venue,
                VenueId = venue.VenueId,
                Facebook = fb,
                Twitter = tweet,
            };
            DataManager.Current.Clear<CheckinResponse>(cr);
            return cr;
        }

        public static CheckinRequest VenueCheckin(string venueId, bool tweet, bool fb)
        {
            var cr = new CheckinRequest
            {
                ActualVenue = null,
                VenueId = venueId,
                Facebook = fb,
                Twitter = tweet,
            };
            DataManager.Current.Clear<CheckinResponse>(cr);
            return cr;
        }

        public static CheckinRequest CheckinWithoutVenueId(string orphanVenueName, bool tweet, bool fb)
        {
            var cr = new CheckinRequest
            {
                OrphanVenueName = orphanVenueName,
                Twitter = tweet,
                Facebook = fb,
            };
            DataManager.Current.Clear<CheckinResponse>(cr);
            return cr;
        }

        public static CheckinRequest GoOffTheGrid(Venue venue)
        {
            var cr = new CheckinRequest
            {
                ActualVenue = venue,
                VenueId = venue.VenueId,
                IsPrivate = true,
            };
            DataManager.Current.Clear<CheckinResponse>(cr);
            return cr;
        }

        public static CheckinRequest GoOffTheGrid(string orphanVenueName)
        {
            var cr = new CheckinRequest
            {
                OrphanVenueName = orphanVenueName,
                IsPrivate = true,
            };
            DataManager.Current.Clear<CheckinResponse>(cr);
            return cr;
        }

        public static CheckinRequest Shout(string shout, bool tweet, bool fb)
        {
            var cr = new CheckinRequest
            {
                ShoutMessage = shout,
                Twitter = tweet,
                Facebook = fb,
            };
            DataManager.Current.Clear<CheckinResponse>(cr);
            return cr;
        }

        public override int GetHashCode()
        {
            return (VenueId??"").GetHashCode() ^ (ShoutMessage??"").GetHashCode() ^ (OrphanVenueName??"").GetHashCode();
        }

        public Venue ActualVenue { get; set; } // for refreshing!
        public string VenueId { get; set; }
        public string OrphanVenueName { get; set; }
        public string ShoutMessage { get; set; } // max 140 char
        public bool Twitter { get; set; } // 1: send, 0: don't send
        public bool Facebook { get; set; } // 1: send, 0: don't send
        public bool IsPrivate { get; set; }

        public string[] GetRestParameters()
        {
            Dictionary<string, string> p = new Dictionary<string, string>();

            p["venueId"] = VenueId;
            p["venue"] = OrphanVenueName;
            p["shout"] = ShoutMessage;

            string b = "private";
            if (!IsPrivate)
            {
                b = "public";
                if (Facebook)
                    b += ",facebook";
                if (Twitter)
                    b += ",twitter";
            }

            p["broadcast"] = b;

            List<string> l = new List<string>();
            foreach (var k in p)
            {
                if (!string.IsNullOrEmpty(k.Value))
                {
                    l.Add(k.Key);
                    l.Add(k.Value);
                }
            }
            return l.ToArray();
        }
    }
}
