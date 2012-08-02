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

//#if BUILDING_FUTURE_CODE
#define PUSH_CAP
//#endif

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows;
using JeffWilcox.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor
{
    public class PushNotificationService
    {
        private PushNotificationService()
        {
            _pushSettings = new PushChannelSettings();
        }

        private PushChannelSettings _pushSettings;

        private static PushNotificationService _instance;

        public static PushNotificationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PushNotificationService();
                }

                return _instance;
            }
        }

        public Uri PushUri
        {
            get
            {
                if (_pushSettings != null)
                {
                    return _pushSettings.PushChannelUri;
                }

                return null;
            }
        }

        public void SetCloudSetting(
            string key, 
            string value, 
            Action<string> success, 
            Action<Exception> failure)
        {
            // TODO: Global loading type stuff.

            var pushUri = PushUri;
            if (pushUri != null && key != null && value != null)
            {
                IWebRequestFactory iwrf = Application.Current as IWebRequestFactory;
                if (iwrf != null)
                {
                    var client = (iwrf.CreateWebRequestClient()).GetWrappedClientTemporary();

                    var uri = ((IGenerate4thAndMayorUri)(Application.Current))
                        .Get4thAndMayorUri("/v1/push/setSetting/?uri=" + PushNotificationService.Instance.PushUri.ToString() + "&key="
                        +Uri.EscapeDataString(key)
                        + "&value="
                        + Uri.EscapeDataString(value)
                        , true);

                    //var req = client.CreateSimpleWebRequest(uri);
                    client.UploadStringCompleted += (sndr, args) =>
                    //client.DownloadStringCompleted += (sndr, args) =>
                        {
                            AgFx.PriorityQueue.AddUiWorkItem(() =>
                                {
                                    if (!args.Cancelled && args.Error == null && success != null)
                                    {
                                        success(args.Result);
                                    }
                                    else if (args.Error != null && failure != null)
                                    {
                                        failure(args.Error);
                                    }
                                });
                        };
                    client.UploadStringAsync(uri, string.Empty);
                    //client.DownloadStringAsyncWithPost(req, string.Empty);
                }
            }
        }

        private void DisconnectFromCloud()
        {
            // Remove the URI
            _pushSettings.PushChannelUri = null;
            _pushSettings.Save();

            // Disconnect on the cloud side.
            IWebRequestFactory iwrf = Application.Current as IWebRequestFactory;
            if (iwrf != null)
            {
                var client = (iwrf.CreateWebRequestClient()).GetWrappedClientTemporary();
                var uri = ((IGenerate4thAndMayorUri)(Application.Current))
                    .Get4thAndMayorUri("/v1/disconnect/?uri=" + (PushUri == null ? string.Empty : 
                    PushUri.ToString()), true);
                client.UploadStringAsync(uri, string.Empty);
            }
        }

        public void Connect(string userId, string token, string checkinCount, string useCount)
        {
            // NOTE: The caller must have verified that the user has opted in
            // to the service first.

            OpenPushNotificationChannel(new CloudConnectionInformation
            {
                // Toast = "0",
                // Tile = "0",
                UserId = userId,
                CheckinCount = checkinCount,
                UseCount = useCount,
                Token = token,
            });
        }

        class CloudConnectionInformation
        {
            // public string Toast { get; set; }
            // public string Tile { get; set; }
            public string Token { get; set; }
            public string UserId { get; set; }
            public string CheckinCount { get; set; }
            public string UseCount { get; set; }
        }

        private void ConnectWithTheCloud(Uri pushUri)
        {
            if (_info == null)
            {
                throw new InvalidOperationException("_info cloud data was empty.");
            }

            if (_pushSettings != null && pushUri != null)
            {
                // Store our updated URI first.
                _pushSettings.PushChannelUri = pushUri;
                _pushSettings.Save();
            }

            System.Diagnostics.Debug.WriteLine("Push Notification Channel is now open: {0}", pushUri);

            string version = string.Empty;

            /*string deviceId;
            string liveId;
            GetDeviceInformation(out deviceId, out liveId);*/

            /*string accent = string.Empty;
            try
            {
                var accentColor = (Color)Application.Current.Resources["PhoneAccentColor"];
                if (accentColor != null)
                {
                    accent = accentColor.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch
            { 
                // timing issue?
            }*/

            IAppInfo iai = Application.Current as IAppInfo;
            if (iai != null)
            {
                version = iai.Version;
            }
            string av = version;

            string oat = _info.Token;

            string apv = string.Empty;
            IAppPlatformVersion iapv = Application.Current as IAppPlatformVersion;
            if (iapv != null)
            {
                apv = iapv.AppPlatformVersion;
            }

            string cc = _info.CheckinCount;
            string uc = _info.UseCount;

            string mfg = DeviceStatus.DeviceManufacturer + " " + DeviceStatus.DeviceName;

            string osv = Environment.OSVersion.Version.ToString();

            IWebRequestFactory iwrf = Application.Current as IWebRequestFactory;
            if (iwrf != null)
            {
                var client = (iwrf.CreateWebRequestClient()).GetWrappedClientTemporary();

                var uri = ((IGenerate4thAndMayorUri)(Application.Current))
                        .Get4thAndMayorUri(string.Format(
                    CultureInfo.InvariantCulture,
                    //"https://www.4thandmayor.com/v1/connect/?uri={0}&d={1}&lid={2}&oat={3}&u={4}&apv={5}&av={6}&cc={7}&uc={8}&mfg={9}&osv={10}&toast={11}&tile={12}&accent={13}",
                    "/v1/connect/?uri={0}&oat={1}&u={2}&apv={3}&av={4}&cc={5}&uc={6}&mfg={7}&osv={8}",

                    HttpUtility.UrlEncode(pushUri.ToString()),
                    //deviceId,
                    //liveId,
                    oat,
                    _info.UserId,
                    apv,
                    av,
                    cc,
                    uc,
                    HttpUtility.UrlEncode(mfg),
                    osv
                    //_info.Toast,
                    //_info.Tile,
                    //HttpUtility.UrlEncode(accent)
                    ), true);

                //var req = client.CreateSimpleWebRequest(uri);
                //client.DownloadStringCompleted += OnServiceConnectCompleted;

                // Try getting all active tiles.
                StringBuilder sb = new StringBuilder();
                try
                {
                    foreach (var t in ShellTile.ActiveTiles)
                    {
                        sb.AppendLine(t.NavigationUri.ToString());
                    }
                }
                catch(Exception)
                {
                }


                client.UploadStringAsync(uri, sb.ToString());
                //client.DownloadStringAsyncWithPost(req, sb.ToString());
            }
        }

        //private void OnServiceConnectCompleted(object sender, MyDownloadStringCompletedEventArgs e)
        //{
            // TODO: Anything to worry about?
        //}

#if PUSH_CAP
        private HttpNotificationChannel _channel;
#endif

        //public void Start()
        //{
            // Assumption: start is only called if authorized by the user; the 
            // main app settings has the master on/off switch for push.

            //OpenPushNotificationChannel();
        //}

        private void OpenPushNotificationChannel(CloudConnectionInformation info)
        {
            _info = info;

#if PUSH_CAP
            if (_channel == null)
            {
                IPushNotificationChannelName ipncn = Application.Current as IPushNotificationChannelName;
                if (ipncn != null)
                {
                    System.Diagnostics.Debug.WriteLine("Finding the push channel...");

                    _channel = HttpNotificationChannel.Find(ipncn.PushNotificationChannelName);
                    if (_channel != null) // && _channel.ChannelUri != null)
                    {
                        Hook();
                        if (_channel.ChannelUri != null)
                        {
                            ConnectWithTheCloud(_channel.ChannelUri);
                        }
                        return;
                    }

                    _channel = new HttpNotificationChannel(ipncn.PushNotificationChannelName);

                    Hook();

                    if (_channel != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Opening up the push channel...");

                        _channel.Open();
                        BindToShellToast();
                        BindToShellTile();
                    }
                }
            }
#endif
        }

        private void BindToShellToast()
        {
            if (_channel != null && !_channel.IsShellToastBound)
            {
                try
                {
                    _channel.BindToShellToast();
                }
                catch (InvalidOperationException)
                {
                    /*
BindToShellTile or BindToShellToast failed because it is already bound to the channel.  Check the IsShellToastBound or IsShellTileBound properties or UnbindToShellToast or UnbindToShellTile before binding again.
System.InvalidOperationException
   at Microsoft.Phone.Notification.SafeNativeMethods.ThrowExceptionFromHResult(Int32 hr, Exception defaultException, NotificationType type)
   at Microsoft.Phone.Notification.ShellObjectChannelInternals.Bind()
   at Microsoft.Phone.Notification.HttpNotificationChannel.BindToShellTile(Collection`1 baseUri)
   at JeffWilcox.FourthAndMayor.PushNotificationService.BindToShellTile()
   at JeffWilcox.FourthAndMayor.PushNotificationService.OnChannelUriUpdated(Object sender, NotificationChannelUriEventArgs e)
   at Microsoft.Phone.Notification.HttpNotificationChannel.OnDescriptorUpdated(IntPtr blob, UInt32 blobSize)
   at Microsoft.Phone.Notification.HttpNotificationChannel.ChannelHandler(UInt32 eventType, IntPtr blob1, UInt32 int1, IntPtr blob2, UInt32 int2)
   at Microsoft.Phone.Notification.HttpNotificationChannel.Dispatch(Object threadContext)
   at System.Threading.ThreadPool.WorkItem.doWork(Object o)
   at System.Threading.Timer.ring()
                     * */
                }
            }
        }

        private void Hook()
        {
#if PUSH_CAP
            if (_channel != null)
            {
                _channel.ChannelUriUpdated += OnChannelUriUpdated;
                _channel.ErrorOccurred += OnChannelErrorOccurred;
                _channel.ShellToastNotificationReceived += OnShellToastNotificationReceived;
            }
#endif
        }

        private void BindToShellTile()
        {
            if (_channel != null && !_channel.IsShellTileBound)
            {
                var c = new Collection<Uri>
                        {
                            new Uri("http://tiles.4thandmayor.com/", UriKind.Absolute),
                            new Uri ("https://www.4thandmayor.com/", UriKind.Absolute)
                        };
                try
                {
                    _channel.BindToShellTile(c);
                }
                catch (InvalidOperationException)
                {
                    // crash reported:
                    /*
BindToShellTile or BindToShellToast failed because it is already bound to the channel.  Check the IsShellToastBound or IsShellTileBound properties or UnbindToShellToast or UnbindToShellTile before binding again.
System.InvalidOperationException
   at Microsoft.Phone.Notification.SafeNativeMethods.ThrowExceptionFromHResult(Int32 hr, Exception defaultException, NotificationType type)
   at Microsoft.Phone.Notification.ShellObjectChannelInternals.Bind()
   at Microsoft.Phone.Notification.HttpNotificationChannel.BindToShellTile(Collection`1 baseUri)
   at JeffWilcox.FourthAndMayor.PushNotificationService.BindToShellTile()
   at JeffWilcox.FourthAndMayor.PushNotificationService.OnChannelUriUpdated(Object sender, NotificationChannelUriEventArgs e)
   at Microsoft.Phone.Notification.HttpNotificationChannel.OnDescriptorUpdated(IntPtr blob, UInt32 blobSize)
   at Microsoft.Phone.Notification.HttpNotificationChannel.ChannelHandler(UInt32 eventType, IntPtr blob1, UInt32 int1, IntPtr blob2, UInt32 int2)
   at Microsoft.Phone.Notification.HttpNotificationChannel.Dispatch(Object threadContext)
   at System.Threading.ThreadPool.WorkItem.doWork(Object o)
   at System.Threading.Timer.ring()
                     * */
                }
            }
        }

        public void DisconnectFromPushNotifications()
        {
            DisconnectFromCloud();

            Unhook();
            if (_channel != null)
            {
                // Removes the channel.
                System.Diagnostics.Debug.WriteLine("Removing the push notification channel from PNS via _channel.Close().");
                _channel.Close();

                _channel = null;
            }

            _info = null;
        }

        private void Unhook()
        {
#if PUSH_CAP
            if (_channel != null)
            {
                _channel.ChannelUriUpdated -= OnChannelUriUpdated;
                _channel.ErrorOccurred -= OnChannelErrorOccurred;
                _channel.ShellToastNotificationReceived -= OnShellToastNotificationReceived;
            }
#endif
        }

#if PUSH_CAP
        private void OnShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            if (e != null && e.Collection != null && e.Collection.Count > 0)
            {
                // TODO: ...

            }
        }
#endif

#if PUSH_CAP
        private void OnChannelErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Channel error happened! " + e.ErrorType);

            switch (e.ErrorType)
            {
                case ChannelErrorType.ChannelOpenFailed:
                    // ...
                    break;
                case ChannelErrorType.MessageBadContent:
                    // ...
                    break;
                case ChannelErrorType.NotificationRateTooHigh:
                    // ...
                    break;
                case ChannelErrorType.PayloadFormatError:
                    // ...
                    break;
                case ChannelErrorType.PowerLevelChanged:
                    // ...
                    break;
            }
        }
#endif

        private CloudConnectionInformation _info;

#if PUSH_CAP
        private void OnChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            if (_channel == null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine("Channel URI has been updated.");

            /* was crashing, hope the new protected methods are better:
             * Channel binding existed
System.InvalidOperationException
   at Microsoft.Phone.Notification.SafeNativeMethods.ThrowExceptionFromHResult(Int32 hr, Exception defaultException, NotificationType type)
   at Microsoft.Phone.Notification.ShellObjectChannelInternals.Bind()
   at Microsoft.Phone.Notification.HttpNotificationChannel.BindToShellTile(Collection`1 baseUri)
   at JeffWilcox.FourthAndMayor.PushNotificationService.BindToShellTile()
   at JeffWilcox.FourthAndMayor.PushNotificationService.OnChannelUriUpdated(Object sender, NotificationChannelUriEventArgs e)
   at Microsoft.Phone.Notification.HttpNotificationChannel.OnDescriptorUpdated(IntPtr blob, UInt32 blobSize)
   at Microsoft.Phone.Notification.HttpNotificationChannel.ChannelHandler(UInt32 eventType, IntPtr blob1, UInt32 blobSize1, IntPtr blob2, UInt32 blobSize2)
   at Microsoft.Phone.Notification.HttpNotificationChannel.Dispatch(Object threadContext)
   at System.Threading.ThreadPool.WorkItem.doWork(Object o)
   at System.Threading.Timer.ring()
             * */

            if (!_channel.IsShellToastBound)
            {
                BindToShellToast();
            }

            if (!_channel.IsShellTileBound)
            {
                BindToShellTile();
            }

            ConnectWithTheCloud(_channel.ChannelUri);

            /*
myPushChannel_ChannelUriUpdated is the most complex delegate. Here, the app checks to ensure that myPushChannel can receive Push Notification even when this app is not running by checking the IsShellTileBound and IsShellToastBound values. If these values are "false," the app binds the channel to the shell so that these notifications can be received when the app is not running, passing along a list of domains that are authorized to send such updates. Here, the domain to which we published Push Service is specified (e.g. http://your_url_prefix.cloudapp.net). The successful retrieval of a live channel URI is noted onscreen, again using the Dispatcher's BeginInvoke() method, and a call to SubscribeMyPhone() ensures that this URI is known by Push Service.             * */
        }
#endif
    }
}
