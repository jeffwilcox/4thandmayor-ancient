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

using System.Windows;
using System;

namespace JeffWilcox.FourthAndMayor
{
    // refactor out sometime

    public static class ApplicationLocalizationExtensions
    {
        public static bool TryGetLocalizedString(this Application app, string key, out string @string)
        {
            return RequireAppLocalizationInterface(app).TryGetLocalizedString(key, out @string);
        }

        private static IProvideLocalizedStrings RequireAppLocalizationInterface(Application app)
        {
            var appStrings = app as IProvideLocalizedStrings;
            if (appStrings != null)
            {
                return appStrings;
            }

            throw new InvalidOperationException("The Application instance does not implement IProvideLocalizedStrings.");
        }

        public static string GetLocalizedString(this Application app, string key)
        {
            return RequireAppLocalizationInterface(app).GetLocalizedString(key);
        }
    }

    public interface IProvideLocalizedStrings
    {
        string GetLocalizedString(string identifier);
        bool TryGetLocalizedString(string key, out string @string);
    }
}
