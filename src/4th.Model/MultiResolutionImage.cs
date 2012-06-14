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
using System.Diagnostics;
using System.Globalization;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class MultiResolutionImage // : ISpecializedComparisonString
    {
        private string _prefix;
        private string _name;
        private List<int> _sizes;

        // TODO: Expose endpoints to get URIs out.

        public Uri TryGetExactResolution(int size)
        {
            if (_sizes != null && _sizes.Contains(size))
            {
                return ComposeUri(size);
            }

            string sizesString = "(NO SIZES)";
            if (_sizes != null)
            {
                sizesString = string.Empty;
                foreach (var s in _sizes)
                {
                    sizesString += s + " ";
                }
            }
            Debug.WriteLine("The MultiResolutionImage did not contain {0}. It does however define {1}.", size, sizesString);

            return null;
        }

        public Uri GetLargestResolution()
        {
            if (_sizes != null && _sizes.Count > 0)
            {
                var last = _sizes[_sizes.Count - 1];
                return ComposeUri(last);
            }

            return null;
        }

        private Uri ComposeUri(int size)
        {
            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
                _prefix,
                size,
                _name
                ), UriKind.Absolute);
        }

        public static MultiResolutionImage ParseJson(JToken json)
        {
            MultiResolutionImage mri = new MultiResolutionImage();

            mri._prefix = Json.TryGetJsonProperty(json, "prefix");
            mri._name = Json.TryGetJsonProperty(json, "name");

            var sizes = json["sizes"];
            List<int> sz = new List<int>();
            if (sizes != null)
            {
                foreach (int size in sizes)
                {
                    sz.Add(size);
                }
            }
            mri._sizes = sz;

            return mri;
        }
    }
}
