﻿//
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

using System.Globalization;
using AgFx;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class SpecialLoadContext : LoadContext
    {
        public SpecialLoadContext(string specialId) : base(specialId)
        {
        }

        public string SpecialId
        {
            get { return (string) Identity; }
        }

        protected override string GenerateKey()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}", base.GenerateKey(), VenueId);
        }

        public string VenueId { get; set;}
    }
}