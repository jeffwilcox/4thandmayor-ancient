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
using System.IO;
using System.Text;
using AgFx;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class FourSquareLoadRequest : LoadRequest
    {
        private readonly FourSquareServiceRequest _sr;
        private Exception _error;

        /// <summary>
        /// Cancels the load request.
        /// </summary>
        public void Cancel(Exception error)
        {
            _error = error;
        }

        public FourSquareLoadRequest(LoadContext context, JeffWilcox.FourthAndMayor.FourSquareWebClient.UriErrorPair uri)
            : base(context)
        {
            if (_sr == null)
            {
                _sr = new FourSquareServiceRequest();
            }
            _sr.Uri = uri.Uri;
            _sr.UseCredentials = null;

            if (uri.Error != null)
            {
                _error = uri.Error;
            }
        }

        public FourSquareLoadRequest(LoadContext context, JeffWilcox.FourthAndMayor.FourSquareWebClient.UriErrorPair uri, byte[] postData, Dictionary<string, string> parameters) 
            : this(context, uri)
        {
            Debug.Assert(_sr != null);
            
            _sr.PostBytes = postData;
            _sr.PostParameters = parameters;
        }

        public FourSquareLoadRequest(LoadContext context, JeffWilcox.FourthAndMayor.FourSquareWebClient.UriErrorPair uri, string postData)
            : this(context, uri)
        {
            Debug.Assert(_sr != null);

            _sr.PostString = postData;
        }

        public void SetMultipartPostData(byte[] data)
        {
            Debug.Assert(_sr != null);

            _sr.PostBytes = data;
        }

        private Action<LoadRequestResult> _callback;

        private void ProcessResult(string str, Exception ex)
        {
            var callback = _callback;
            _callback = null;

            LoadRequestResult res;
            if (ex != null)
            {
                res = new LoadRequestResult(ex);
            }
            else
            {
                var rs = str;
                try
                {
                    // TODO: PERFORMANCE NOTE: MOVE AWAY FROM USING DOWNLOAD STRING ASYNC HERE!@!!
                    Stream s = new MemoryStream(Encoding.UTF8.GetBytes(rs));
                    res = new LoadRequestResult(s);
                }
                catch (Exception e)
                {
                    res = new LoadRequestResult(
                        new InvalidOperationException(
                            "There was a problem understanding the response from the service.", e));
                }
            }

            callback(res);
        }

        public override void Execute(Action<LoadRequestResult> result)
        {
            Debug.Assert(_callback == null);
            _callback = result;

            if (_error != null)
            {
                ProcessResult(null, _error);
            }
            else
            {
                _sr.CallAsync(ProcessResult);
            }
        }
    }
}
