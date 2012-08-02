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
using System.Windows;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor
{
    public static class ImperialUnits
    {
        private const double MetersToFeet = 3.2808399;

        private static bool? _usingImperial;

        public static bool CheckWhetherToUse()
        {
            if (_usingImperial == null)
            {
                var cc = CultureInfo.CurrentUICulture;
                string cn = cc.Name;
                int ip = cn.IndexOf('-');
                if (ip > 0)
                {
                    string remainder = cn.Substring(ip + 1);

                    ip = remainder.IndexOf('/');
                    if (ip > 0)
                    {
                        // Should strip out the region code.
                        remainder = remainder.Substring(0, ip);
                    }

                    switch (remainder)
                    {
                            // PUERTO RICO?

                            // From Wikipedia:
                        case "US": // United States
                        case "GB": // Great Britan
                        case "LB": // Liberia
                        case "BU": // Berma
                        case "MM": // Now Berma
                            _usingImperial = true;
                            return true;
                    }
                }
                _usingImperial = false;
                return false; // Meters please.
            }

            return (bool)_usingImperial;
        }

        public static double ConvertMetersToFeet(double meters)
        {
            return meters * MetersToFeet;
        }

        //
        //
        // Time for some kind of NASA-style mistakes with the math here. =)

        private const double FeetInMile = 5280 - 25; // Bing. 25 removed to allow for some fuzzy math.
        private const double QuarterMile = FeetInMile / 4;

        private static string GetMilesPrefix(double miles)
        {
            // LOCALIZE:
            string post = miles > 1 ? " miles" : " mile";

            int iprefix = (int)Math.Floor(miles);
            string prefix = iprefix == 0 ? string.Empty : Json.GetPrettyInt(iprefix);

            double left = miles - iprefix;

            if (iprefix != 0 && left < (QuarterMile * .5))
            {
                // LOCALIZE:
                if (iprefix == 1) { post = " mile"; }
                return prefix + post;
            }
            if (left <= QuarterMile)
            {
                return prefix + "¼" + post;
            }
            if (left <= QuarterMile * 2.05)
            {
                return prefix + "½" + post;
            }
            //if (left <= QuarterMile * 3.05)
            //{
            return prefix + "¾" + post;
            //}
        }

        public static string GetMetersInProperUnitsDisplay(double meters)
        {
            if (_usingImperial == null)
            {
                // I use app settings now for this!      //CheckWhetherToUse();

                IExposeSettings ies = Application.Current as IExposeSettings;
                if (ies != null)
                {
                    var iuiu = ies.ImperialUnitsInUse;
                    if (iuiu != null)
                    {
                        _usingImperial = iuiu.AreImperialUnitsInUse;
                    }
                }

                // _usingImperial = FourSquareApp.Instance.Settings.AreImperialUnitsInUse;
            }
            if (_usingImperial == true)
            {
                // Imperial!
                double feet = ConvertMetersToFeet(meters);
                if (feet <= QuarterMile)
                {
                    int rnd = (int)Math.Round(feet);
                    if (rnd < 10)
                    {
                        // LOCALIZE:
                        return "right here";
                    }
                    // LOCALIZE:
                    // LOCALIZE:
                    return rnd == 1 ? "1 foot" : Json.GetPrettyInt(rnd) + " feet";
                }
                return GetMilesPrefix(feet / FeetInMile);
            }
            else
            {
                // Less imperial!
                int rnd = (int)Math.Round(meters);
                if (rnd > 500 && rnd < 750)
                {
                    // LOCALIZE:
                    return "½ km";
                }

                if (rnd >= 750)
                {
                    double km = meters / 1000;
                    int kmi = (int)Math.Round(km);

                    // LOCALIZE:
                    return Json.GetPrettyInt(kmi) + " km";
                }

                // LOCALIZE:
                return rnd == 1 ? "1 meter" : Json.GetPrettyInt(rnd) + " meters";
            }
        }
    }
}
