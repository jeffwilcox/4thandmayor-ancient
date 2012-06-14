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
using AgFx;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Comment : ISpecializedComparisonString, IGenesis
    {
        public static void AddComment(string checkinId, string text, Action success, Action<Exception> failure)
        {
            var client = (new FourSquareWebClient()).GetWrappedClientTemporary();
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                "checkins/" + checkinId + "/addcomment",
                GeoMethodType.None,
                "text",
                text);
            // real app will... %20%26%20 for space & space ( & )
            var uri = uuri.Uri;
            var newUri = FourSquareWebClient.CreateServiceRequest(uri, true);
            client.UploadStringCompleted += (x, xe) =>
            //client.DownloadStringCompleted += (x, xe) =>
            {
                Exception e = null;
                if (xe.Error != null)
                {
                    e = xe.Error;
                }
                else
                {
                    string rs = xe.Result;
                    try
                    {
                        var json = FourSquareDataLoaderBase<LoadContext>.ProcessMetaAndNotificationsReturnJson(rs);
                    }
                    catch (Exception ee)
                    {
                        e = new UserIntendedException("There was a problem adding your comment, please try again later.", ee);
                    }
                }
                client = null;

                // Result now if there is not a photo.
                if (e != null)
                    failure(e);
                else
                    success();
            };

            // POST request.
            client.UploadStringAsync(newUri, string.Empty);
            //client.DownloadStringAsyncWithPost(newUri, string.Empty);
        }

        public string Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Created { get; set; }
        public CompactUser User { get; set; }
        public string Text { get; set; }
        public bool IsSelf { get; set; }

        public static Comment ParseJson(JToken json)
        {
            var comment = new Comment();

            comment.Id = Json.TryGetJsonProperty(json, "id");
            Debug.Assert(comment.Id != null);

            comment.User = CompactUser.ParseJson(json["user"]);

            comment.Text = Json.TryGetJsonProperty(json, "text");

            if (comment.User != null)
            {
                comment.IsSelf = comment.User.Relationship == FriendStatus.Self;
            }

            string created = Json.TryGetJsonProperty(json, "createdAt");
            if (created != null)
            {
                DateTime dtc = UnixDate.ToDateTime(created);
                comment.CreatedDateTime = dtc;
                comment.Created = Checkin.GetDateString(dtc);
            }

            return comment;
        }

        public string SpecializedComparisonString
        {
            get
            {
                return Id;
            }
        }
    }
}