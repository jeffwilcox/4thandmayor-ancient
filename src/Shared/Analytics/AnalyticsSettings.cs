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
using System.Diagnostics;
using System.Globalization;
using AgFx;

namespace JeffWilcox.FourthAndMayor
{
    /// <summary>
    /// Stores settings about application use, analytic state, and other 
    /// important fields.
    /// </summary>
    public class AnalyticsSettings : SettingsStorage
    {
        public AnalyticsSettings()
            : base("use")
        {
        }

        #region VisitorId
        private string _visitorId;
        private const string VisitorIdKey = "visitorId";
        public string VisitorId
        {
            get
            {
                return _visitorId;
            }
            set
            {
                _visitorId = value;
                RaisePropertyChanged("VisitorId");
            }
        }
        #endregion
        #region SessionStartTime
        private string _sessionStartTime;
        private const string SessionStartTimeKey = "sessionStart";
        public string SessionStartTime
        {
            get
            {
                return _sessionStartTime;
            }
            set
            {
                _sessionStartTime = value;
                RaisePropertyChanged("SessionStartTime");
            }
        }
        #endregion
        #region PreviousSessionStartTime
        private string _previousSessionStartTime;
        private const string PreviousSessionStartTimeKey = "previousSessionStart";
        public string PreviousSessionStartTime
        {
            get
            {
                return _previousSessionStartTime;
            }
            set
            {
                _previousSessionStartTime= value;
                RaisePropertyChanged("PreviousSessionStartTime");
            }
        }
        #endregion
        #region VisitorStartTime
        private string _visitorStartTime;
        private const string VisitorStartTimeKey = "visitorStart";
        public string VisitorStartTime
        {
            get
            {
                return _visitorStartTime;
            }
            set
            {
                _visitorStartTime = value;
                RaisePropertyChanged("VisitorStartTime");
            }
        }
        #endregion
        #region LastEventTime
        private DateTime _lastEventTime;
        private const string LastEventTimeKey = "lastEvent";
        public DateTime LastEventTime
        {
            get { return _lastEventTime; }
            set { 
                _lastEventTime = value;
                RaisePropertyChanged("LastEventTime");
            }
        }
        #endregion
        #region UseCount
        private int _useCount;
        private const string UseCountKey = "uses";
        public int UseCount
        {
            get
            {
                return _useCount;
            }
            set
            {
                _useCount = value;
                RaisePropertyChanged("UseCount");

                // Debug.WriteLine("Use count: " + value);
            }
        }
        #endregion
        #region Check-in count
        private int _checkinCount;
        private const string CheckinCountKey = "checkins";
        public int Checkins
        {
            get
            {
                return _checkinCount;
            }
            set
            {
                // TODO: Migrate this setting. Should instead be part of the app
                // data and NOT in the generalized component for analytics, 
                // since it's specific to Foursquare.
                if (value != _checkinCount)
                {
                    RaisePropertyChanged("Checkins");
                }
                _checkinCount = value;
            }
        }
        #endregion

        protected override void Serialize()
        {
            Setting[VisitorIdKey] = _visitorId;
            Setting[UseCountKey] = _useCount.ToString(CultureInfo.InvariantCulture);
            Setting[CheckinCountKey] = _checkinCount.ToString(CultureInfo.InvariantCulture);
            Setting[SessionStartTimeKey] = _sessionStartTime;
            Setting[PreviousSessionStartTimeKey] = _previousSessionStartTime;
            Setting[VisitorStartTimeKey] = _visitorStartTime;
            Setting[LastEventTimeKey] = UnixDate.ToString(_lastEventTime);
        }

        protected override void Deserialize()
        {
            string imp;
            if (Setting.TryGetValue(VisitorIdKey, out imp))
            {
                _visitorId = imp;
            }
            else
            {
                _visitorId = GenerateNewVisitorId();
            }

            if (Setting.TryGetValue(UseCountKey, out imp))
            {
                int i;
                if (int.TryParse(imp, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                {
                    _useCount = i;
                }
            }

            if (Setting.TryGetValue(CheckinCountKey, out imp))
            {
                int i;
                if (int.TryParse(imp, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                {
                    _checkinCount = i;
                }
            }

            if (Setting.TryGetValue(SessionStartTimeKey, out imp))
            {
                // Set to the previous now.
                _previousSessionStartTime = imp;
            }

            if (Setting.TryGetValue(LastEventTimeKey, out imp))
            {
                _lastEventTime = UnixDate.ToDateTime(imp);
            }

            // The new session start time.
            _sessionStartTime = UnixDate.ToString(DateTime.UtcNow);

            if (Setting.TryGetValue(VisitorStartTimeKey, out imp))
            {
                _visitorStartTime = imp;
            }
            if (string.IsNullOrEmpty(_visitorStartTime))
            {
                _visitorStartTime = UnixDate.ToString(DateTime.UtcNow);
            }

            PriorityQueue.AddWorkItem(
                () =>
                    {
                        ++UseCount;
                        SaveSoon();
                    });

            base.Deserialize();
        }

        private static string GenerateNewVisitorId()
        {
            return (GoogleAnalytics.Random.Next(0x7fffffff) ^ (Guid.NewGuid().GetHashCode() & 0x7fffffff)).ToString(CultureInfo.InvariantCulture);
        }
    }
}
