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
using AgFx;
using System.Windows;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor.Model
{
    // This entire file should be refactored and AgFx should not be used for 
    // credentials.

    [CachePolicy(CachePolicy.ValidCacheOnly, 60 * 60 * 24 * 365 * 10)] // NEV'A EVA unless you have a WP7 in 2020
    public class LocalCredentials : ModelItemBase // DataLoaderModelItemBase
    {
        private const string LoginIdentity = "_CurrentUser_";

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

        public static bool Initialize()
        {
            if (_current == null)
            {
                // Synchronous load attempt of the credentials.
                _current = DataManager.Current.LoadFromCache<LocalCredentials>(LoginIdentity);
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
            _token = null;
            if (_current != null)
            {
                _current._userId = null;

                DataManager.Current.Clear<LocalCredentials>(LoginIdentity);

                DataManager.Current.DeleteCache();

                _current.LogoutInternal();
            }
        }

        #region Property HasLoggedIn
        public bool HasLoggedIn
        {
            get
            {
                return !string.IsNullOrEmpty(_token) && !string.IsNullOrEmpty(_userId);
            }
        }
        #endregion

        #region Property UserId
        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                RaisePropertyChanged("UserId");
            }
        }
        #endregion

        #region Property Token
        private static string _token;
        public static string Token
        {
            get
            {
                return _token;
            }
            set
            {
                if (_token != value)
                {
                    _token = value;
                    // THIS IS STATIC! RaisePropertyChanged("Token");
                }
            }
        }
        #endregion

        public LocalCredentials() : base(LoginIdentity)
        {
            base.NotificationContext = SynchronizationContext.Current;
        }

        private void LoginInternal(string authenticationToken)
        {
            Token = authenticationToken;
            System.Diagnostics.Debug.WriteLine("LoginInternal Refresh");
            DataManager.Current.Refresh<LocalCredentials>(LoginIdentity,
                (fbl) =>
                {
                    var so = Application.Current as ISetAuthenticationForThisAppInstance;
                    if (so != null)
                    {
                        so.SetAuthenticationForThisAppInstance(_token);
                        //FourSquareApp.Instance.SetAuthenticationForThisAppInstance(_token);
                    }
                    DataManager.Current.Flush();
                    if (_current != null && string.IsNullOrEmpty(_current.UserId))
                    {
                        _current = DataManager.Current.LoadFromCache<LocalCredentials>(LoginIdentity);
                    }

                    OnAuthenticationSuccess(EventArgs.Empty);
                }
                ,
                (ex) =>
                {
                    PriorityQueue.AddUiWorkItem(() => OnAuthenticationFailure(EventArgs.Empty));
                }
            );
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

            Token = null;
            _current = null;
            Initialize();
        }

        public class LocalCredentialsDataLoader : DefaultDataLoader 
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                if (string.IsNullOrEmpty(Token))
                {
                    return null;
                }
                return new LocalCredentialsLoadRequest(context, Token);
            }

            public override object Deserialize(LoadContext context, Type objectType, Stream stream)
            {
                var sr = new StreamReader(stream);

                var lc = new LocalCredentials();

                try
                {
                    Token = sr.ReadLine();
                    lc.UserId = sr.ReadLine();

                    if (!string.IsNullOrEmpty(Token) &&
                        !string.IsNullOrEmpty(lc.UserId))
                    {
                        return lc;
                    }
                }
                catch
                {
                }

                throw new InvalidOperationException("There was a problem validating your credentials.");
            }
        }
    }
}
