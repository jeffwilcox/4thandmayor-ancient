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
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Badge : ISpecializedComparisonString
    {
        public Badge()
        {
        }

        public string Id { get; private set; }

        /// <summary>
        /// An optional canonical ID for this badge.
        /// </summary>
        public string BadgeId { get; private set; }

        public string Name { get; set; }
        public Uri IconUri { get; set; }
        public Uri IconMediumUri { get; set; }
        public Uri IconLargeUri { get; set; }
        public string Description { get; set; }

        public MultiResolutionImage MultiResolutionIcon { get; set; }

        public List<int> Sizes { get; private set; }
        public string Hint { get; set; }
        /// <summary>
        /// Gets an array of the unlock data (checkins) that earned the badge.
        /// </summary>
        public List<Checkin> Unlocks { get; private set; }

        public string ImagePrefix { get; private set; }
        public string ImagePostfix { get; private set; }

        public Uri InternalAppUri
        {
            get
            {
                return new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,

                    "/JeffWilcox.FourthAndMayor.Profile;component/Badge.xaml?id={3}&name={0}&icon={1}&d={2}",
                    //V2...                     "/Views/Badge.xaml?id={3}&name={0}&icon={1}&d={2}",
                    Uri.EscapeDataString(Name),
                    Uri.EscapeDataString(IconLargeUri.ToString()),
                    Uri.EscapeDataString(Description),
                    Id),

                    UriKind.Relative);
            }
        }

        public static Badge ParseJson(JToken badge)
        {
            Badge b = new Badge();

            b.Id = Json.TryGetJsonProperty(badge, "id");
            b.BadgeId= Json.TryGetJsonProperty(badge, "badgeId");

            b.Name = Json.TryGetJsonProperty(badge, "name");
            b.Hint = Json.TryGetJsonProperty(badge, "hint");
            b.Description = Json.TryGetJsonProperty(badge, "description");

            var image = badge["image"];
            if (image != null)
            {
                try
                {
                    // TODO: Move to using MultiResolutionImage!
                    b.MultiResolutionIcon = MultiResolutionImage.ParseJson(image);

                    string prefix = Json.TryGetJsonProperty(image, "prefix");
                    string imageName = Json.TryGetJsonProperty(image, "name");
                    var sizes = image["sizes"];
                    if (sizes != null)
                    {
                        List<int> szz = sizes.Select(size => int.Parse(size.ToString(), CultureInfo.InvariantCulture)).ToList();
                        b.Sizes = szz;
                        if (szz.Count > 0)
                        {
                            b.IconUri = new Uri(prefix + szz[0] + imageName, UriKind.Absolute);
                            b.IconLargeUri = new Uri(prefix + szz[szz.Count - 1] + imageName, UriKind.Absolute);

                            if (szz.Count > 2)
                            {
                                b.IconMediumUri = new Uri(prefix + szz[1] + imageName, UriKind.Absolute);
                            }
                            else
                            {
                                // No medium-sized icon available.
                                b.IconMediumUri = b.IconUri;
                            }
                        }
                        b.ImagePrefix = prefix;
                        b.ImagePostfix = imageName;
                    }
                }
                catch (Exception)
                {
                }
            }

            var unlocks = badge["unlocks"];
            if (unlocks != null)
            {
                List<Checkin> lc = new List<Checkin>();
                foreach (var unlock in unlocks)
                {
                    try
                    {
                        Checkin c = Checkin.ParseJson(unlock);
                        lc.Add(c);
                    }
                    catch (Exception)
                    {
                        // note: silent watchson?
                    }
                }
                b.Unlocks = lc;
            }

            return b;
        }

        public string SpecializedComparisonString
        {
            get { return BadgeId; }
        }
    }
}
