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
using System.IO;
using System.Threading;
using System.Windows;
using AgFx;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class LocalCredentials : ModelItemBase
    {
        private static LocalCredentials _current;

        public static LocalCredentials Current
        {
            get
            {
                if (_current == null)
                {
#if DEBUG
                    throw new InvalidOperationException("Initialize not called.");
#else
                        Initialize();
#endif
                }
                return _current;
            }
        }

        private LocalCredentials()
        {
            _userTokenSettings = UserTokenSettings.Instance;
        }

        private UserTokenSettings _userTokenSettings;

        public static bool Initialize()
        {
            if (_current == null)
            {
                _current = new LocalCredentials();
            }
            return _current.HasLoggedIn;
        }

        public static void Login(string authenticationToken)
        {
            System.Diagnostics.Debug.WriteLine("LocalCredentials.Login()");
            _current.LoginInternal(authenticationToken);
        }

        public static void Logout()
        {
            if (_current != null)
            {
                _current._userTokenSettings.OAuth2Token = null;
                _current._userTokenSettings.UserId = null;
                _current._userTokenSettings.Save();

                DataManager.Current.DeleteCache();

                _current.LogoutInternal();
            }
        }

        #region Property HasLoggedIn
        public bool HasLoggedIn
        {
            get
            {
                return !string.IsNullOrEmpty(_userTokenSettings.OAuth2Token) && !string.IsNullOrEmpty(_userTokenSettings.UserId);
            }
        }
        #endregion

        public string UserId
        {
            get
            {
                return _userTokenSettings.UserId; 
            }
        }

        public static string Token
        {
            get
            {
                return _current._userTokenSettings.OAuth2Token;
            }
        }

        private void LoginInternal(string authenticationToken)
        {
            _userTokenSettings.OAuth2Token = authenticationToken;
            _userTokenSettings.Save();

            System.Diagnostics.Debug.WriteLine("LoginInternal Refresh");

            // This used to do more magic... question: does local user still
            // get setup to determine whether this is all legit or not!?

            var so = Application.Current as ISetAuthenticationForThisAppInstance;
            if (so != null)
            {
                so.SetAuthenticationForThisAppInstance(_userTokenSettings.OAuth2Token);
            }

            DataManager.Current.Flush();

            if (_current != null && string.IsNullOrEmpty(_current.UserId))
            {
                DataManager.Current.Load<User>("self", 
                    
                (usr) => 
                {
                    if (usr.FriendStatus == FriendStatus.Self)
                    {
                        _userTokenSettings.UserId = usr.UserId;
                        _userTokenSettings.Save();

                        OnAuthenticationSuccess(EventArgs.Empty);
                    }
                    else
                    {
                        OnAuthenticationFailure(EventArgs.Empty);
                    }
                }, 
                
                (fail) => 
                {
                    OnAuthenticationFailure(EventArgs.Empty);
                });
            }
            else
            {
                OnAuthenticationSuccess(EventArgs.Empty);
            }
        }

        public event EventHandler AuthenticationSuccess;
        protected virtual void OnAuthenticationSuccess(EventArgs e)
        {
            var handler = AuthenticationSuccess;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler AuthenticationFailure;

        protected virtual void OnAuthenticationFailure(EventArgs e)
        {
            var handler = AuthenticationFailure;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void LogoutInternal()
        {
            // "hack" for v1, v2 release
            ((ISignedOutThisInstance) Application.Current).SignedOutThisInstance = true;

            // this code should be called by whatever actually calls us.
            if (_userTokenSettings != null)
            {
                _userTokenSettings.OAuth2Token = null;
                _userTokenSettings.UserId = null;
            }

            Initialize();
        }
    }
}
