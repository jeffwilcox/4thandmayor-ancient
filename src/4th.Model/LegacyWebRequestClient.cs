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
using System.IO;
using System.Net;

namespace JeffWilcox.FourthAndMayor
{
    using System.Text;

    public class LegacyWebRequestClient
    {
        #region Debug timing
#if DEBUG && TRACKING_TIME
        private DateTime _started;
        private void StartTimeTracking()
        {
            _started = DateTime.Now;
        }
        private void StopTimeTracking()
        {
            TimeSpan ts = DateTime.Now - _started;
            Debug.WriteLine("HWR: {0} ms elapsed request.", ts.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }
#endif
        #endregion

        public event EventHandler<MyDownloadStringCompletedEventArgs> DownloadStringCompleted;

        private HttpWebRequest _webRequest;

        protected HttpWebRequest WebRequest { get { return _webRequest; } }

        public void SetHeader(string key, string value)
        {
            if (_webRequest != null)
            {
                _webRequest.Headers[key] = value;
            }
        }

        public HttpWebRequest CreateSimpleWebRequest(Uri uri)
        {
            return CreateWebRequest(uri);
        }

        protected virtual HttpWebRequest CreateWebRequest(Uri uri)
        {
#if DEBUG
            Debug.WriteLine("HWR: {0}", uri);
#endif
            return (HttpWebRequest)System.Net.WebRequest.Create(uri);
        }

        public void DownloadStringAsyncWithPost(HttpWebRequest request, byte[] postData, string contentType)
        {
            _webRequest = request;
            _webRequest.Method = "POST";
            _webRequest.ContentType = contentType;
            _webRequest.AllowReadStreamBuffering = true;

            try
            {
                _webRequest.BeginGetRequestStream(BeginRequest, postData);
            }
            catch (SystemException)
            {
                // Bad news bears.
            }

#if DEBUG && TRACKING_TIME
            StartTimeTracking();
#endif
        }

        public void DownloadStringAsyncWithPost(HttpWebRequest request, string postData)
        {
            _webRequest = request;
            _webRequest.Method = "POST";
            _webRequest.AllowReadStreamBuffering = true;

            try
            {
                _webRequest.BeginGetRequestStream(BeginRequest, Encoding.UTF8.GetBytes(postData));
            }
            catch (SystemException)
            {
                // Bad news bears.
            }

#if DEBUG && TRACKING_TIME
            StartTimeTracking();
#endif
        }

        public void DownloadStringAsync(HttpWebRequest request, object state = null)
        {
            _webRequest = request;
            _webRequest.AllowReadStreamBuffering = true;
            _webRequest.BeginGetResponse(BeginResponse, state);
#if DEBUG && TRACKING_TIME
            StartTimeTracking();
#endif
        }

        private void BeginRequest(IAsyncResult ar)
        {
            try
            {
                using (var stm = _webRequest.EndGetRequestStream(ar))
                {
                    var postData = (byte[])ar.AsyncState;
                    stm.Write(postData, 0, postData.Length);
                    stm.Close();
                }

                _webRequest.BeginGetResponse(BeginResponse, null);
            }
            catch (SystemException)
            {
                // Bad news bears.
            }
        }

        private void BeginResponse(IAsyncResult ar)
        {
            string responseStr = null;

            try
            {
                using (var webResponse = _webRequest.EndGetResponse(ar))
                {
                    using (var sr = new StreamReader(webResponse.GetResponseStream()))
                    {
                        responseStr = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                var handler = DownloadStringCompleted;
                if (handler != null)
                {
                    handler(this, new MyDownloadStringCompletedEventArgs(null, e));
                }

                return;
            }

            var handler2 = DownloadStringCompleted;
#if DEBUG && TRACKING_TIME
            StopTimeTracking();
#endif
            if (handler2 != null)
            {
                handler2(this,
                    !string.IsNullOrEmpty(responseStr)
                        ? new MyDownloadStringCompletedEventArgs(responseStr, null)
                        : new MyDownloadStringCompletedEventArgs(null, new Exception("No response from server.")));
            }
        }
    }
}
