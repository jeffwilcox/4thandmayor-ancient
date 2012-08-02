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
using System.IO;
using System.Text;
using AgFx;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class LocalCredentialsLoadRequest : LoadRequest
    {
        public LocalCredentialsLoadRequest(LoadContext context, string token)
            : base(context)
        {
            _token = token;
        }

        private string _token;

        public override void Execute(Action<LoadRequestResult> callback)
        {
            FourSquareWebClient.SetCredentials(_token);

            DataManager.Current.Load<User>("self",
                 (user) =>
                 {
                     string theirUserId = user.UserId;
                     string result = _token + Environment.NewLine + theirUserId;
                     _token = null;

                     Debug.WriteLine("LoadCredentialsLoadRequest: OAuth2 Authentication Success.");

                     Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(result));
                     callback(new LoadRequestResult(stream));
                 },
                 (error) =>
                     {
                         Debug.WriteLine("LoadCredentialsLoadRequest: Authentication failure.");
                         _token = null;
                     FourSquareWebClient.ClearCredentials();

                     callback(new LoadRequestResult(error));
                 });
        }
    }
}
