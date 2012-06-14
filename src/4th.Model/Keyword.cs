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
using System.Globalization;
using System.Net;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Keyword
    {
        public Keyword(string section, string display, string value)
        {
            Section = section;
            DisplayName = display;
            Value = value;
        }
        public string Section { get; set; }
        public string SectionDisplayName { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }

        public Uri ExplorerUri
        {
            get
            {
                return new Uri(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "/Views/Explorer.xaml?section={0}&display={2}&query={1}&sectiondisplay={3}",
                    Uri.EscapeDataString(Section ?? string.Empty),
                    Uri.EscapeDataString(Value ?? string.Empty),
                    Uri.EscapeDataString(DisplayName ?? string.Empty),
                    Uri.EscapeDataString(SectionDisplayName ?? string.Empty)
                    ), UriKind.Relative);
            }
        }

        public override string ToString()
        {
            return Section + " " + DisplayName + " " + Value;
        }
    }
}
