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
using System.Globalization;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Photo : ISpecializedComparisonString, IGenesis
    {
        public static string ProblemEnumToString(PhotoProblem problem)
        {
            switch (problem)
            {
                case PhotoProblem.HateViolence:
                    // LOCALIZE:
                    return "hate_violence";

                case PhotoProblem.Illegal:
                    // LOCALIZE:
                    return "illegal";

                case PhotoProblem.Nudity:
                    // LOCALIZE:
                    return "nudity";

                case PhotoProblem.SpamScam:
                    // LOCALIZE:
                    return "spam_scam";

                case PhotoProblem.Unrelated:
                    // LOCALIZE:
                    return "unrelated";

                default:
                    throw new InvalidOperationException();
            }
        }

        public string Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Created { get; set; }
        public Uri Uri { get; set; }
        public Uri SmallestUri { get; set; }
        public Uri MediumUri { get; set; }
        public Uri LargerUri { get; set; }
        // FUTURE: Consider consolidating icon and photo size algorithms.

        // Used in comments:
        public bool IsSelf { get; set; }
        public CompactUser User { get; set; }

        public Uri LocalPhotoViewerUri { get; set; }

        public static Photo ParseJson(JToken json)
        {
            Photo p = new Photo();

            p.Id = Json.TryGetJsonProperty(json, "id");
            //Debug.Assert(p.Id != null);

            string created = Json.TryGetJsonProperty(json, "createdAt");
            if (created != null)
            {
                DateTime dtc = UnixDate.ToDateTime(created);
                p.CreatedDateTime = dtc;
                p.Created = Checkin.GetDateString(dtc);
            }

            string primaryUri = Json.TryGetJsonProperty(json, "url");
            if (primaryUri != null)
            {
                p.Uri = new Uri(primaryUri, UriKind.Absolute);
            }

            var userJson = json["user"];
            if (userJson != null)
            {
                p.User = CompactUser.ParseJson(userJson);
                if (p.User != null)
                {
                    p.IsSelf = p.User.Relationship == FriendStatus.Self;
                }
            }

            var sizes = json["sizes"];
            if (sizes != null)
            {
                var items = sizes["items"];
                List<UriWidthHeight> sz = new List<UriWidthHeight>();
                foreach (var item in items)
                {
                    sz.Add(new UriWidthHeight
                    {
                        Uri = new Uri(Json.TryGetJsonProperty(item, "url"), UriKind.Absolute),
                        Width = double.Parse(Json.TryGetJsonProperty(item, "width"), CultureInfo.InvariantCulture),
                        Height = double.Parse(Json.TryGetJsonProperty(item, "height"), CultureInfo.InvariantCulture),
                    });
                }
                if (sz.Count > 0)
                {
                    p.SmallestUri = sz[sz.Count - 1].Uri;
                    p.MediumUri = sz[sz.Count > 2 ? sz.Count - 2 : sz.Count - 1].Uri;
                    p.LargerUri = sz[1].Uri;
                }
            }

            // new Foursquare (august 2012)
            if (sizes == null && primaryUri == null)
            {
                string prefix = Json.TryGetJsonProperty(json, "prefix");
                string suffix = Json.TryGetJsonProperty(json, "suffix");
                if (prefix != null && suffix != null)
                {
                    p.Uri = new Uri(prefix + "original" + suffix, UriKind.Absolute);
                }
                else
                {
                    return p;
                }
            }

            if (p.Uri != null)
            {
                Uri pu = new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "/Views/PhotoViewer.xaml?photoUri={0}&isSelf={1}&id={2}",

                    Uri.EscapeDataString(p.Uri.ToString()),
                    p.IsSelf ? "true" : "false",
                    p.Id
                    )
                    , UriKind.Relative);
                p.LocalPhotoViewerUri = pu;
            }

            return p;
        }

        public string SpecializedComparisonString
        {
            get
            {
                return Id;
            }
        }
    }
}