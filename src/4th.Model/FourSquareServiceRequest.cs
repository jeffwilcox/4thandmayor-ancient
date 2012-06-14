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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using AgFx;

namespace JeffWilcox.FourthAndMayor.Model
{ 
    internal class FourSquareServiceRequest
    {
        public Uri Uri { get; set; }

        public string PostString { get; set; }

        public byte[] PostBytes { get; set; }

        public Dictionary<string, string> PostParameters { get; set; }

        public bool? UseCredentials { get; set; }

        public void CallAsync(Action<string, Exception> callback)
        {
            Debug.Assert(_callback == null);
            _callback = callback;

            PriorityQueue.AddWorkItem(ExecuteOffThread);
        }

        private void DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs args)
        {
            DownloadOrUploadStringCompleted(sender, args);
        }

        private void DownloadOrUploadStringCompleted(object sender, object args)
        {
            var cb = _callback;
            _callback = null;

            var down = args as System.Net.DownloadStringCompletedEventArgs;
            var up = args as System.Net.UploadStringCompletedEventArgs;
            if (down != null || up != null)
            {
                // new clients
                Exception ex = down != null ? down.Error : up.Error;
                string str = down != null ? down.Result : up.Result;

                if (cb != null)
                {
                    cb(str, ex);
                }
            }
            else
            {
                // Legacy case. Ick.
                var legacy = args as MyDownloadStringCompletedEventArgs;
                if (legacy != null)
                {
                    if (cb != null)
                    {
                        cb(legacy.Result, legacy.Error);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Legacy client isn't being used, but something entirely different!");
                }
            }
        }

        private Action<string, Exception> _callback;

        private void ExecuteOffThread()
        {
            var client = (new FourSquareWebClient()).GetWrappedClientTemporary();
            var newUri = FourSquareWebClient.CreateServiceRequest(Uri, UseCredentials);
            //client.DownloadStringCompleted += DownloadStringCompleted;

            // 1. Try posting string.
            // 2. Try multipart post.
            // 3. Regular get request.

            if (PostString != null)
            {
                client.UploadStringCompleted += client_UploadStringCompleted;
                client.UploadStringAsync(newUri, PostString);
                //client.DownloadStringAsyncWithPost(r, PostString);
            }
            else if (PostBytes != null && PostBytes.Length > 0)
            {
                // TODO: PHOTO UPLOAD: Add aprams for NAME (photo) and FILENAME
                string uploadFilename = "image.jpg";

                string boundary = Guid.NewGuid().ToString();
                string mpt = "multipart/form-data; boundary=" + boundary;

                var ms = new MemoryStream();

                byte[] headerBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] footerBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                string keyValueFormat = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

                if (PostParameters != null)
                {
                    foreach (string key in PostParameters.Keys)
                    {
                        string formitem = string.Format(
                            CultureInfo.InvariantCulture,
                            keyValueFormat, key, PostParameters[key]);
                        byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                        ms.Write(formitembytes, 0, formitembytes.Length);
                    }
                }
                ms.Write(headerBytes, 0, headerBytes.Length);

                string hdt = string.Format(
                    CultureInfo.InvariantCulture,
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n",
                    "photo",
                    uploadFilename);
                byte[] hb = Encoding.UTF8.GetBytes(hdt);
                ms.Write(hb, 0, hb.Length);
                ms.Write(PostBytes, 0, PostBytes.Length);

                // this worked when i wrote only header bytes length!

                ms.Write(footerBytes, 0, footerBytes.Length);

                byte[] finalData = ms.ToArray();
                ms.Close();

                var legacyClient = new LegacyWebClient();
                var r = legacyClient.CreateServiceRequest(Uri, UseCredentials);
                legacyClient.DownloadStringCompleted += OnLegacyClientDownloadStringCompleted;
                legacyClient.DownloadStringAsyncWithPost(r, finalData, mpt);
            }
            else
            {
                client.DownloadStringCompleted += DownloadStringCompleted;
                client.DownloadStringAsync(newUri);
            }
        }

        private void OnLegacyClientDownloadStringCompleted(object sender, MyDownloadStringCompletedEventArgs e)
        {
            DownloadOrUploadStringCompleted(sender, e);
        }

        private void client_UploadStringCompleted(object sender, System.Net.UploadStringCompletedEventArgs e)
        {
            DownloadOrUploadStringCompleted(sender, e);
        }
    }
}