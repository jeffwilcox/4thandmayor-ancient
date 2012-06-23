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
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Category
    {
        private List<Category> _subs;
        public List<Category> SubCategories
        {
            get { return _subs; }
        }

        public override string ToString()
        {
            return PluralName ?? NodeName;
        }

        public string Name { get { return NodeName; } }

        public string PluralName { get; set; }

        public string CategoryId { get; set; }

        public string FullPath { get; set; }

        public bool HasSubCategories
        {
            get { return (SubCategories != null && SubCategories.Count > 0); }
        }

        public string NodeName { get; set; }

        public bool IsPrimary { get; set; }

        public MultiResolutionImage MultiResolutionIcon { get; set; }

        private Uri _iconUri;
        public Uri IconUri 
        {
            get
            {
                if (_iconUri != null)
                {
                    return _iconUri;
                }

                if (MultiResolutionIcon != null)
                {
                    return MultiResolutionIcon.GetLargestResolution();
                }

                return null;
            }
            //set
            //{

            //}
        }

        public Uri MediumIconSize
        {
            get
            {
                if (MultiResolutionIcon != null)
                {
                    return MultiResolutionIcon.TryGetExactResolution(64);
                }

                if (_iconUri != null)
                {
                    string s = _iconUri.ToString();
                    s = s.Replace(".png", "_64.png");
                    return new Uri(s, UriKind.Absolute);
                }

                return null;
            }
        }
        public Uri LargeIconSize
        {
            get
            {
                if (MultiResolutionIcon != null)
                {
                    return MultiResolutionIcon.TryGetExactResolution(256);
                }

                if (_iconUri != null)
                {
                    string s = _iconUri.ToString();
                    s = s.Replace(".png", "_256.png");
                    return new Uri(s, UriKind.Absolute);
                }

                return null;
            }
        }

        public static Category ParseJson(JToken cat)
        {
            Category pc = new Category();

            pc.CategoryId = Json.TryGetJsonProperty(cat, "id");
            // NEW API will return these OK, old does not...
            // Debug.Assert(pc.CategoryId != null);

            pc.FullPath = Json.TryGetJsonProperty(cat, "fullpathname"); // old v1!
            pc.NodeName = Json.TryGetJsonProperty(cat, "name");

            pc.PluralName = Json.TryGetJsonProperty(cat, "pluralName"); // v2

            if (string.IsNullOrEmpty(pc.PluralName))
            {
                pc.PluralName = pc.NodeName;
            }

            var subcats = cat["categories"]; // v2, recursive
            if (subcats != null)
            {
                List<Category> sc = new List<Category>();
                foreach (var sub in subcats)
                {
                    if (sub != null)
                    {
                        Category c = Category.ParseJson(sub);
                        if (c != null)
                        {
                            sc.Add(c);
                        }
                    }
                }
                if (sc.Count > 0)
                {
                    pc._subs = sc;
                }
            }

            var parents = cat["parents"]; // old v1!
            if (parents != null)
            {
                // I wonder, HOW MULTIPLE PARENTS WOULD WORK.
                JArray prnts = (JArray) parents;
                foreach (var item in prnts)
                {
                    // V2 NOTE: THIS IS INCORRECT since its not really the full
                    // path, but instead the node.
                    pc.FullPath = item.ToString();
                    break;
                }
            }

            string primary = Json.TryGetJsonProperty(cat, "primary");
            if (primary != null && (primary == "true" || primary == "True"))
            {
                pc.IsPrimary = true;
            }

            // Since the isolated storage may still have older versions of the
            // data, this should use a fallback-mechanism.
            try
            {
                var icon = cat["icon"];
                if (icon != null)
                {
                    MultiResolutionImage mri = MultiResolutionImage.ParseJson(icon);
                    if (mri != null)
                    {
                        pc.MultiResolutionIcon = mri;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                try
                {
                    string uri = Json.TryGetJsonProperty(cat, "icon");
                    if (uri != null)
                    {
                        pc._iconUri = new Uri(uri);
                    }
                }
                catch (Exception)
                {
                }
            }

            return pc;
        }
    }
}
