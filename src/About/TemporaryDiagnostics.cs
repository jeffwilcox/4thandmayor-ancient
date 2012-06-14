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
using System.Windows;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor;
using Microsoft.Phone.Info;

namespace About
{
    public class Diag
    {
        public Diag() { }
        public Diag(string title, string value)
        {
            Title = title;
            Value = value;
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
    }

    public static class TemporaryDiagnostics
    {
        public static List<Diag> GetDiagnostics()
        {
            var list = new List<Diag>();

            // Better design: diagnostics can retrieve a value safely.
            try
            {
                string os = Environment.OSVersion.ToString();
                os = os.Replace("Windows CE", "Windows Phone");

                list.Add(new Diag
                {
                    Title = "operating system",
                    Value = os
                });

                IAppPlatformVersion iapv = Application.Current as IAppPlatformVersion;
                if (iapv != null)
                {
                    string a = "unknown";
                    if (iapv.AppPlatformVersion == "7.0")
                    {
                        a = "Windows Phone 7";
                    }
                    else if (iapv.AppPlatformVersion == "7.1")
                    {
                        a = "Windows Phone 7.5";
                    }

                    list.Add(new Diag("designed for app platform", a));
                }

                IAppInfo iai = Application.Current as IAppInfo;
                if (iai != null)
                {
                    list.Add(new Diag("app version", iai.Version));
                }

                list.Add(new Diag("current culture", CultureInfo.CurrentCulture.ToString()));
                list.Add(new Diag("user interface culture", CultureInfo.CurrentUICulture.ToString()));

//                object o;

                list.Add(
                    new Diag(
                        "phone manufacturer",
                        string.Format(CultureInfo.InvariantCulture, "{0}", DeviceStatus.DeviceManufacturer)
                        ));

                    string s = DeviceStatus.DeviceName;
                    if (s != null)
                    {
                        // device friendly names =)
                        var upper = s.ToUpperInvariant();
                        if (upper == "SGH-I937")
                        {
                            s = "Focus S";
                        }
                        if (upper == "SGH-I917")
                        {
                            s = "Focus";
                        }
                        if (upper == "SGH-I917R")
                        {
                            s = "Focus*";
                        }
                        if (upper.StartsWith("SGH-I1677"))
                        {
                            s = "Focus Flash"; // need to validate.
                        }
                        if (upper == "XDEVICEEMULATOR")
                        {
                            s = "Windows Phone Emulator";
                        }

                        list.Add(
                            new Diag(
                                "phone model",
                                string.Format(CultureInfo.InvariantCulture, "{0}", s)
                                ));
                    }


                list.Add(
                    new Diag(
                        "peak memory use",
                        string.Format(CultureInfo.CurrentCulture, "{0:N} MB", DeviceStatus.ApplicationPeakMemoryUsage / (1024 * 1024))
                        ));

                list.Add(
                    new Diag(
                        "available app memory",
                        string.Format(CultureInfo.CurrentCulture, "{0:N} MB", DeviceStatus.ApplicationMemoryUsageLimit / (1024 * 1024))
                        ));

                list.Add(
                    new Diag(
                        "reported memory",
                        string.Format(CultureInfo.CurrentCulture, "{0:N} MB", DeviceStatus.DeviceTotalMemory / (1024 * 1024))
                        ));

                list.Add(new Diag("web services agent", FourSquareWebClient.ClientInformation));
            }
            catch
            {

            }

            return list;
        }
    }
}
