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
using System.Diagnostics;
using AgFx;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh, 60 * 60 * 24)] // One day.
    public class UserSettings : FourSquareItemBase<LoadContext>
    {
        public UserSettings() : base()
        {
        }

        public UserSettings(LoadContext context)
            : base(context)
        {
        }

        private bool _receivePings;
        public bool ReceivePings
        {
            get
            {
                return _receivePings;
            }
            set
            {
                _receivePings = value;
                RaisePropertyChanged("ReceivePings");
            }
        }

        private bool _receiveCommentPings;
        public bool ReceiveCommentPings
        {
            get
            {
                return _receiveCommentPings;
            }
            set
            {
                _receiveCommentPings = value;
                RaisePropertyChanged("ReceiveCommentPings");
            }
        }

        //
        //
        // Dynamic values that support updating.

        private bool _sendToTwitter;
        public bool SendToTwitter
        {
            get
            {
                return _sendToTwitter;
            }
            set
            {
                if (_sendToTwitter != value && IsLoadComplete)
                {
                    Debug.WriteLine("Altering SendToTwitter value");
                    SaveNewSetting("sendToTwitter", value);
                }

                _sendToTwitter = value;
                RaisePropertyChanged("SendToTwitter");
            }
        }

        private bool _sendToFacebook;
        public bool SendToFacebook
        {
            get
            {
                return _sendToFacebook;
            }
            set
            {
                if (_sendToFacebook != value && IsLoadComplete)
                {
                    SaveNewSetting("sendToFacebook", value);
                }

                _sendToFacebook = value;
                RaisePropertyChanged("SendToFacebook");
            }
        }

        private void SaveNewSetting(string settingName, bool newValue)
        {
            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Saving setting");

            FourSquare.Instance.SaveSetting(settingName, newValue ? "1" : "0", () =>
                {
                    // Reload from the server the settings now.
                    DataManager.Current.Refresh<UserSettings>(LoadContext, null, null);

                    token.CompleteWithAcknowledgement();
                },
                                            (error) =>
                                                {
                                                    token.Complete();
                                                });

        }

        public class UserSettingsDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                if (LocalCredentials.Current != null && string.IsNullOrEmpty(LocalCredentials.Current.UserId))
                {
                    throw new UserIgnoreException();
                }

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "settings/all",
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var s = new UserSettings(context);

                    var j = json["settings"];
                    if (j != null)
                    {
                        s.ReceivePings = Json.TryGetJsonBool(j, "receivePings");
                        s.ReceiveCommentPings = Json.TryGetJsonBool(j, "receiveCommentPings");
                        s.SendToTwitter = Json.TryGetJsonBool(j, "sendToTwitter");
                        s.SendToFacebook = Json.TryGetJsonBool(j, "sendToFacebook");
                    }

                    Debug.WriteLine("SETTINGS: read from server settings: Twitter: {0}, Facebook: {1}", s.SendToTwitter, s.SendToFacebook);

                    s.IsLoadComplete = true;

                    return s;
                }
                catch (Exception)
                {
                    throw new UserIgnoreException();
                }
            }
        }
    }
}
