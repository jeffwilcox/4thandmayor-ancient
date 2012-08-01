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
using System.Linq;
using System.Text;
using JeffWilcox.FourthAndMayor;

namespace JeffWilcox.Controls
{
    public class UserTokenSettings : SettingsStorage
    {
        private static UserTokenSettings _instance;

        public static UserTokenSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserTokenSettings();
                }

                return _instance;
            }
        }

        private UserTokenSettings()
            : base("token")
        {
        }

        private string _oauth2Token;
        private const string OAuth2TokenKey = "oat";
        public string OAuth2Token
        {
            get { return _oauth2Token; }
            set
            {
                _oauth2Token = value;
                RaisePropertyChanged("OAuth2Token");
            }
        }

        private string _userId;
        private const string UserIdKey = "id";
        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                RaisePropertyChanged("UserId");
            }
        }

        protected override void Serialize()
        {
            Setting[OAuth2TokenKey] = _oauth2Token;
            Setting[UserIdKey] = _userId;
        }

        protected override void Deserialize()
        {
            string imp;
            if (Setting.TryGetValue(OAuth2TokenKey, out imp))
            {
                _oauth2Token = imp;
            }

            if (Setting.TryGetValue(UserIdKey, out imp))
            {
                _userId = imp;
            }

            base.Deserialize();
        }
    }
}
