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

#if DEBUG
//#define DEBUG_ANALYTICS
#endif

// This is an incomplete yet simple implementation of working with the Google
// Analytics service, mostly just around navigation events.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AgFx;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor
{
    public class GoogleAnalytics
    {
        private const string GaPixel = "http://www.google-analytics.com/__utm.gif";

        private static GoogleAnalytics _instance;

        private bool _initialized;

        private string _appName;

        private Image _image;

        private GoogleAnalytics()
        {
            _sessionLength = new TimeSpan(0, 30, 0);

            IAnalyticsKey iak = Application.Current as IAnalyticsKey;
            if (iak != null)
            {
                _ga = iak.GA;
            }

            _language = CultureInfo.CurrentCulture.Name.ToLower(CultureInfo.InvariantCulture);
        }

        public static GoogleAnalytics Instance
        {
            get { return _instance ?? (_instance = new GoogleAnalytics()); }
        }

        public void SetImage(Image image)
        {
            if (_image == null && image != null)
            {
                _image = image;
            }
        }

        public void OnFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            PriorityQueue.AddUiWorkItem(() => HandleNavigation(e));
        }

        private void HandleNavigation(NavigatingCancelEventArgs e)
        {
            string s = null;
            if (e != null && e.Uri != null)
            {
                if (e.Uri.IsAbsoluteUri)
                {
                    s = e.Uri.LocalPath;
                }
                else
                {
                    s = e.Uri.ToString();
                }

                if (s.StartsWith("//", StringComparison.InvariantCulture))
                {
                    s = s.Substring(1);
                }

                int i = s.IndexOf(";component",0, StringComparison.InvariantCulture);
                if (i >= 0)
                {
                    s = s.Substring(i + 10);
                }

#if DEBUG_ANALYTICS
                Debug.WriteLine("Analytics: " + s);
#endif
                UpdateImage(s, DateTimeOffset.UtcNow);
            }
            else
            {
            
            }
        }

        public void Initialize(string appHostName, string appName)
        {
            _initialized = true;
            _hostname = appHostName;
            _appName = appName;

            MoveThroughPendings();
        }

        private void MoveThroughPendings()
        {
            if (_pendings != null && _pendings.Count > 0)
            {
                
                var item = _pendings.Dequeue();
                string uri = item.Value;
                var dt = item.Key;

                UpdateImage(uri, dt);

                PriorityQueue.AddUiWorkItem(MoveThroughPendings);
            }
        }

        // Google Analytics tracking variables
        // -----------------------------------

        // utmac: account string
        private string _ga;

        // utmhn: host name, URL-encoded string. TODO: Do mobile apps send this?
        private string _hostname; //utmhn

        // utmcs: language encoding
        private const string LanguageEncoding = "utf-8";

        // utmn: unique, prevents caching of GIF image
        internal static readonly Random Random = new Random();
        private static string GetRandomAndUnique()
        {
            return GoogleAnalytics.Random.Next(0x7fffffff).ToString(CultureInfo.InvariantCulture);
        }

        // utmr: referral, complete URL
        private const string ReferralUri = "-"; // not supported for now.

        // utmsc: screen color depth. =24-bit
        private const string FakeBitDepth = "16-bit"; // yes, some devices are 32-bit sort of, but let's hard-code this for today.

        // utmsc: screen resolution. 2400x1920
        // TODO: In the future other sizes may be important (i.e. detect real-time)
        private const string PortraitScreenResolution = "480x800";
        private const string LandscapeScreenResolution = "800x480";

        // utmul: browser language
        private string _language;

        // utmwv: tracking code version (from the analytics framework, better just use that i suppose)
        private const string TrackingCodeVersion = "4.4wp7";

        // utmcc: cookie variable
        // utmdt: page title, URL-encoded
        // utme: extensible parameter, encoded, used for events and custom vars
        // utmp: page request, encoded. /testDirectory/myPage.html
        // utmt: type of request. event, transaction, item, custom {default, page}

        private const string Category = "WP7";

        private TimeSpan _sessionLength;

        private static string GenerateTimestamp(DateTimeOffset dateTime)
        {
            return UnixDate.ToString(dateTime);
        }

        private string GenerateCookie()
        {
            var ias = Application.Current as IAnalyticsSettings;
            if (ias != null)
            {
                var ai = ias.AnalyticsInfo;
                if (ai != null)
                {
                    return Uri.EscapeDataString(string.Format(
                        CultureInfo.InvariantCulture,
                        "__utma={0}.{1}.{2}.{3}.{4}.{5};",
                        0,
                        ai.VisitorId,
                        ai.VisitorStartTime,
                        ai.PreviousSessionStartTime,
                        ai.SessionStartTime,
                        ai.UseCount));
                }
            }
            throw new InvalidOperationException("App must implement IAnalyticsSettings.");

        }

        private Uri GetTrackingUri(string pageUri)
        {
            int i = pageUri.IndexOf('?');
            if (i >= 0)
            {
                pageUri = pageUri.Substring(0, i);
            }

            var b = new StringBuilder();
            b.AppendFormat(CultureInfo.InvariantCulture, "{0}?utmac={1}", GaPixel, _ga);
            b.AppendFormat(CultureInfo.InvariantCulture, "&utmcc={0}", GenerateCookie());
            b.AppendFormat(CultureInfo.InvariantCulture, "&utmwv={0}", TrackingCodeVersion);
            b.AppendFormat(CultureInfo.InvariantCulture, "&utmn={0}", GetRandomAndUnique());
            b.AppendFormat(CultureInfo.InvariantCulture, "&utmdt={0}", Uri.EscapeDataString(_appName));
            b.AppendFormat(CultureInfo.InvariantCulture, "&utmr={0}", ReferralUri);

            bool isPortrait = Application.Current.GetFrame().IsPortrait();
            b.AppendFormat(CultureInfo.InvariantCulture, "&utsr={0}", isPortrait ? PortraitScreenResolution : LandscapeScreenResolution);

            b.AppendFormat(CultureInfo.InvariantCulture, "&utmp={0}", 
                //System.Net.HttpUtility.UrlEncode(
                Uri.EscapeDataString(
                pageUri));
            var imageUri = new Uri(b.ToString());
            return imageUri;
        }

        private void UpdateImage(string uri, DateTimeOffset eventTime)
        {
            UpdateSession(eventTime);
            var trackUri = GetTrackingUri(uri);
            if (_initialized && _image != null)
            {
                _image.Source = new BitmapImage(trackUri);
#if DEBUG_ANALYTICS
                //Debug.WriteLine(trackUri);
#endif
            }
            else
            {
                if (_pendings != null)
                {
                    _pendings.Enqueue(new KeyValuePair<DateTimeOffset, string>(eventTime, uri));
                }
            }

            var ias = Application.Current as IAnalyticsSettings;
            if (ias != null)
            {
                ias.AnalyticsInfo.SaveSoon();
            }
        }

        private Queue<KeyValuePair<DateTimeOffset, string>> _pendings =
            new Queue<KeyValuePair<DateTimeOffset, string>>();

        private void UpdateSession(DateTimeOffset eventTime)
        {
            var ias = Application.Current as IAnalyticsSettings;
            if (ias != null)
            {
                var ai = ias.AnalyticsInfo;
                if (ai != null)
                {
                    // FUTURE: Consider whether to not have it queue saves.
                    if (ai.LastEventTime == DateTime.MinValue)
                    {
                        ai.LastEventTime = eventTime.LocalDateTime;
                    }
                    if ((eventTime - ai.LastEventTime) > _sessionLength)
                    {
                        ai.UseCount++;
                        var sst = ai.SessionStartTime;
                        ai.PreviousSessionStartTime = sst;
                        ai.SessionStartTime = GenerateTimestamp(eventTime);
                        ai.LastEventTime = eventTime.LocalDateTime;
                    }
                    else
                    {
                        ai.LastEventTime = eventTime.LocalDateTime;
                    }
                }
            }
        }
    }
}
