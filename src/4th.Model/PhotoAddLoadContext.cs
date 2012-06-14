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
using System.IO;
using AgFx;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class PhotoAddLoadContext : LoadContext
    {
        public PhotoAddLoadContext() : base("photoadd")
        {
            // TODO: This is bad. Should figure something better out.
        }

        public static PhotoAddLoadContext AddPhotoToCheckin(string checkinId, bool tweet, bool fb, bool publicForEveryone)
        {
            var cr = new PhotoAddLoadContext
            {
                CheckinId = checkinId,
                Facebook = fb,
                Twitter = tweet,
                IsPublicToEveryone = publicForEveryone,
            };
            return cr;
        }

        public static PhotoAddLoadContext AddPhotoToTip(string tipId, bool tweet, bool fb)
        {
            var cr = new PhotoAddLoadContext
            {
                TipId = tipId,
                Facebook = fb,
                Twitter = tweet,
            };
            return cr;
        }
        public static PhotoAddLoadContext AddPhotoToVenue(string venueId, bool tweet, bool fb)
        {
            var cr = new PhotoAddLoadContext
            {
                VenueId = venueId,
                Facebook = fb,
                Twitter = tweet,
            };
            return cr;
        }

        public override int GetHashCode()
        {
            return (CheckinId ?? "").GetHashCode() ^ (VenueId ?? "").GetHashCode() ^ (TipId ?? "").GetHashCode();
        }

        private byte[] _photoBytes;
        public byte[] GetPhotoBytes()
        {
            return _photoBytes;
        }
        public void SetPhotoBytes(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                _photoBytes = br.ReadBytes((int)s.Length);
            }
        }

        public string VenueId { get; set; }
        public string TipId { get; set; }
        public bool Twitter { get; set; } // 1: send, 0: don't send
        public bool Facebook { get; set; } // 1: send, 0: don't send
        public string CheckinId { get; set; }

        public bool IsPublicToEveryone { get; set; } // only valid for check-in photos.

        public Dictionary<string, string> GetMultipartFormParameters()
        {
            var p = new Dictionary<string, string>();

            if (VenueId != null)
            {
                p["venueId"] = VenueId;
            }
            if (TipId != null)
            {
                p["tipId"] = TipId;
            }
            if (CheckinId != null)
            {
                p["checkinId"] = CheckinId;

                // Only valid for check-in photos.
                p["public"] = IsPublicToEveryone ? "1" : "0";
            }

            string b = null;
            if (Facebook)
                b += "facebook";
            if (Twitter)
                b += (Facebook ? "," : "") + "twitter";

            if (b != null)
            {
                p["broadcast"] = b;
            }

            return p;
        }
    }
}
