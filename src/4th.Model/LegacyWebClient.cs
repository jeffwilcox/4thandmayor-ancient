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
using System.Globalization;
using System.Net;

namespace JeffWilcox.FourthAndMayor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Windows;
    using JeffWilcox.Controls;

    public class LegacyWebClient : LegacyWebRequestClient
    {
        private const string FourSquarePrefix = "https://api.foursquare.com/v2/";

        private static string _clientInformation;

        public static string ClientInformation
        {
            get
            {
                if (_clientInformation == null)
                {
                    SetClientInformation();
                }
                return _clientInformation;
            }
        }

        private static void SetClientInformation()
        {
            IAppInfo iai = Application.Current as IAppInfo;
            string v = "UnknownVersion";
            if (iai != null)
            {
                v = iai.Version;
            }

            // For now this is all hard-coded and that.
            // Only want this calculated once, not with every call.
            _clientInformation = "4thAndMayor:"
                + v
                + " "
                +
#if DEBUG
 "DBG"
#else
                "SHP"
#endif
 + "/"
                + "WP7"
                + "|"
                +
                (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator ?
                "Emulator" :
                "Device"
                );

            // Debug.WriteLine("FWC: User Agent: " + _clientInformation);
        }

        private static string _token;

        public static void SetCredentials(string oauthToken)
        {
            _token = oauthToken;
        }

        public static void ClearCredentials()
        {
            _token = null;
        }

        protected override HttpWebRequest CreateWebRequest(Uri uri)
        {
            HttpWebRequest client = base.CreateWebRequest(uri);
            client.UserAgent = ClientInformation;
            return client;
        }

        private static string _client;
        private static string _secret;
        private static string _apiVersion;
        private static void EnsureCredentials()
        {
            if (_client == null) // assumes all come in at one time
            {
                IAppInfo iai = (IAppInfo)Application.Current;
                _client = iai.FClient;
                _secret = iai.FSecret;

                IFoursquareApiVersion ifai = (IFoursquareApiVersion)Application.Current;
                _apiVersion = ifai.FoursquareApiVersion;
            }
        }

        public HttpWebRequest CreateServiceRequest(Uri uriBeforeAuth, bool? useCredentials = null) // only if they are around and available
        {
            EnsureCredentials();

            if (uriBeforeAuth == null || !uriBeforeAuth.ToString().Contains("?"))
            {
                throw new InvalidOperationException("Invalid URI provided. " + (uriBeforeAuth == null ? "null" : uriBeforeAuth.ToString()));
            }

            string queryString = "callback=j"; // the ampersand, & comes from the earlier page
            bool auth = false;
            if (useCredentials == true || useCredentials == null)
            {
                if (_token == null && useCredentials == true)
                {
                    // LOCALIZE:
                    throw new InvalidOperationException("No valid authentication information is present. Make sure that you are signed in.");
                }
                if (_token != null)
                {
                    auth = true;
                    queryString += "&oauth_token=" + _token;
                }
                else
                {
                    // Anonymous-style request format for v2.
                    // Some endpoints (e.g. venue search) allow you to not act 
                    // as any particular user. We will return unpersonalized 
                    // data suitable for generic use, and the performance should
                    // be slightly better. In these cases, pass your client ID
                    // as client_id and your client secret as client_secret.
                    queryString +=
                        "&client_id=" +

                        _client +
                        //((App)App.Current).FClient + 

                        "&client_secret=" +

                        //((App)App.Current).FSecret
                        _secret
                        ;
                }
            }

            Uri uri = new Uri(uriBeforeAuth.AbsoluteUri + queryString, UriKind.Absolute);

            /* Debug.WriteLine("FWC: {1} Creating FourSquare Web Request for: {0}", uri.AbsoluteUri,
                            auth ? "(+OAuth2)" : "(-anonymous)"); */

            var client = CreateWebRequest(uri);

            return client;
        }

        public class UriErrorPair
        {
            public Uri Uri { get; set; }
            public Exception Error { get; set; }
            public UriErrorPair(Uri uri)
            {
                Uri = uri;
            }
            public UriErrorPair(Exception error)
            {
                Error = error;
            }
            public UriErrorPair(Uri uri, Exception error)
                : this(uri)
            {
                Error = error;
            }
        }

        public static UriErrorPair BuildFourSquareUri(string method, GeoMethodType geoType, params string[] components)
        {
            Exception er = null;
            var d = new Dictionary<string, string>();

            EnsureCredentials();

            if (geoType != GeoMethodType.None)
            {
                var la = LocationAssistant.Instance.LastKnownLocation;
                if (la == null)
                {
                    // LOCALIZE:
                    string error =
                        "The location service has been turned off, so this feature is unfortunately not available.";

                    // TODO: Consider whether this is a fatal condition or not. 4S seems to indicate that it isn't really a big deal.
                    if (geoType == GeoMethodType.Required)
                    {
                        // Want this to get handled outside this scope.
                        // TODO: Understand if this is a good idea or not.
                        Debug.WriteLine("GeoMethodType Required, check failed.");
                        /*PriorityQueue.AddUiWorkItem(() =>
                            {
                                throw new UserIntendedException(error, "GeoMethodType was Required.");
                            });*/
                        er = new UserIntendedException(error, "GeoMethodType was Required.");
                    }
                    else
                    {
                        Debug.WriteLine(error);
                    }
                }
                else
                {
                    if (!double.IsNaN(la.Latitude) && !double.IsNaN((la.Longitude)))
                    {
                        d["ll"] = la.Latitude.ToString(CultureInfo.InvariantCulture) +
                            "," + la.Longitude.ToString(CultureInfo.InvariantCulture);
                        if (!double.IsNaN(la.HorizontalAccuracy))
                        {
                            d["llAcc"] = la.HorizontalAccuracy.ToString(CultureInfo.InvariantCulture);
                        }
                        if (!double.IsNaN(la.VerticalAccuracy) && la.VerticalAccuracy != 0.0 && !double.IsNaN(la.Altitude))
                        {
                            d["altAcc"] = la.VerticalAccuracy.ToString(CultureInfo.InvariantCulture);
                            d["alt"] = la.Altitude.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            // API version.
            d["v"] = _apiVersion;

            // Will allow for override from the method inputs:))
            if (components != null && components.Length > 0)
            {
                // Can overrun if not even number. Messy code!
                for (int i = 0; i < components.Length; i += 2)
                {
                    d[components[i]] = components[i + 1];
                }
            }

            var sb = new StringBuilder();
            sb.Append(FourSquarePrefix);

            // Method name. Should have .json after for json return types.
            sb.Append(method);

            // I always expect this since I add an oauth_token afterwards.
            sb.Append("?");
            foreach (var c in d)
            {
                sb.Append(c.Key);
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(c.Value));
                sb.Append("&");
            }

            Uri uri = new Uri(sb.ToString(), UriKind.Absolute);

            return new UriErrorPair(uri, er);
        }
    }
}
