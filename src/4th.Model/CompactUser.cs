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
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class CompactUser
    {
        public string UserId { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public Uri Photo { get; set; }
        public Gender Gender { get; set; }
        public string HomeCity { get; set; }
        public FriendStatus Relationship { get; set; }

        public bool IsFriend { get; set; }

        public bool IsSelf { get; set; }

        private static readonly Uri MalePhoto = new Uri("/Images/blank_boy.png", UriKind.Relative);
        private static readonly Uri FemalePhoto = new Uri("/Images/blank_girl.png", UriKind.Relative);
        public Uri GenderPhoto { get; set; }

        public string ShortName
        {
            get
            {
                if (Last != null && Last.Length > 0)
                {
                    return First + " " + Last.Substring(0, 1) + ".";
                }
                return First;
            }
        }

        public Uri UserUri
        {
            get
            {
                return new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "/Views/Profile.xaml?id={0}&name={1}",
                    Uri.EscapeDataString(UserId),
                    Uri.EscapeDataString(ToString())), UriKind.Relative);
            }
        }

        // The following show up in friends' requests only but i'm not using
        //public string Twitter { get; set; }
        //public string Facebook { get; set; }
        //public string Phone { get; set; }
        //email
        // end friends' requests only

        public static CompactUser ParseJson(JToken user)
        {
            CompactUser bu = new CompactUser();

            bu.UserId = Json.TryGetJsonProperty(user, "id");
            Debug.Assert(bu.UserId != null);

            bu.First = Json.TryGetJsonProperty(user, "firstName");
            bu.Last = Json.TryGetJsonProperty(user, "lastName");

#if DEMO
            if (bu.Last != null && bu.Last.Length > 0)
            {
                bu.Last = bu.Last.Substring(0, 1);
            }
#endif

            bu.HomeCity = Json.TryGetJsonProperty(user, "homeCity");

            var photo = user["photo"];
            if (photo != null)
            {
                // 4.0
                var prefix = Json.TryGetJsonProperty(photo, "prefix");
                var suffix = Json.TryGetJsonProperty(photo, "suffix");
                string uri = null;
                if (prefix != null && suffix != null)
                {
                    uri = prefix + suffix;
                }
                else
                {
                    uri = Json.TryGetJsonProperty(user, "photo");
                }

                if (uri != null)
                {
                    if (!uri.Contains(".gif"))
                    {
                        bu.Photo = new Uri(uri);
                    }
                }
            }

            string relationship = Json.TryGetJsonProperty(user, "relationship");
            FriendStatus fs = User.UserDataLoader.GetFriendStatus(relationship);
            bu.Relationship = fs;

            bu.IsFriend = (fs == FriendStatus.Friend || fs == FriendStatus.Self);
            bu.IsSelf = fs == FriendStatus.Self;

            string gender = Json.TryGetJsonProperty(user, "gender");
            if (gender != null)
            {
                bu.Gender = gender == "female" ? Gender.Female : Gender.Male;
                bu.GenderPhoto = bu.Gender == Gender.Female ? FemalePhoto : MalePhoto;
            }
            return bu;
        }

        public override string ToString()
        {
            string last = Last;
            if (last != null)
            {
                last = First + " " + last;
            }
            else
            {
                last = First;
            }

            return last;
        }
    }
}