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
using System.Globalization;
using System.IO;
using AgFx;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Tip : ISpecializedComparisonString
    {
        // DESIGN: Not a very good place for a web service call like this.
        public static void AddNewTip(string venueId, string tipText, Stream photo, Action<Tip,Exception> result)
        {
            // NOTE: Official API supports an extra "url" parameter to associate
            var client = (new FourSquareWebClient()).GetWrappedClientTemporary();
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                "tips/add",
                GeoMethodType.None,
                "venueId",
                venueId,
                "text",
                tipText);
            Uri uri = uuri.Uri;
            var newUri = FourSquareWebClient.CreateServiceRequest(uri, true);
            client.UploadStringCompleted += (x, xe) =>
            {
                Exception e = null;
                Tip t = null;

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

                        Tip tip = Tip.ParseJson(json["tip"], typeof(Venue), venueId);

                        if (photo != null)
                        {
                            // Result comes after the photo upload.
                            var req = Model.PhotoAddLoadContext.AddPhotoToTip(tip.TipId, false, false);
                            req.SetPhotoBytes(photo);

                            FourSquare.Instance.AddPhoto(req,

                                (pic) =>
                                {
                                    // that's it..
                                    //RefreshVenue(null);
                                    result(tip, null);
                                },

                                (fail) =>
                                {
                                    result(null, fail);
                                });
                        }
                    }
                    catch (Exception ee)
                    {
                        e = new UserIntendedException("There was a problem adding the tip, please try again later.", ee);
                    }
                }
                client = null;

                // Result now if there is not a photo.
                if (photo == null)
                {
                    result(t, e);
                }
            };

            // POST request.
            client.UploadStringAsync(newUri, string.Empty);
            //client.DownloadStringAsyncWithPost(r, string.Empty);
        }

        public string TipId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Created { get; set; }
        public CompactUser User { get; set; }

        public TipStatus Status { get; set; }
        public string StatusText { get; set; }

        public Photo Photo { get; set; }
        public bool HasPhoto { get { return Photo != null;  } }

        public int TodoCount { get; set; }
        public int DoneCount { get; set; }
        public string DoneText { get; set; }

        public Type ParentType { get; set; }
        public string ParentIdentifier { get; set; }

        public CompactVenue Venue { get; set; }
        
        public string AddedText
        {
            get
            {
                // Used only in the TODO scenario.
                if (OverrideAddedText != null)
                {
                    return OverrideAddedText;
                }

                // Used for all other tips.
                return "added " + Created + (User != null ? " by " + User.ToString() : string.Empty);
            }
        }

        public string OverrideAddedText { get; set; }

        public Uri TipUri { get; set; }

        public static Tip ParseJson(JToken tip)
        {
            return ParseJson(tip, typeof (DetailedTip), null);
        }

        public static Tip ParseJson(JToken tip, Type parentType, string parentIdentifier)
        {
            var t = new Tip();

            t.ParentType = parentType;
            t.ParentIdentifier = parentIdentifier;

            t.TipId = Json.TryGetJsonProperty(tip, "id");
            Debug.Assert(t.TipId != null);

            string txt = Checkin.SanitizeString(Json.TryGetJsonProperty(tip, "text"));
            if (!string.IsNullOrEmpty(txt))
            {
                t.Text = txt;
            }

            var user = tip["user"];
            if (user != null)
            {
                t.User = CompactUser.ParseJson(user);
            }

            string created = Json.TryGetJsonProperty(tip, "createdAt");
            if (created != null)
            {
                DateTime dtc = UnixDate.ToDateTime(created);
                t.CreatedDateTime = dtc;
                t.Created = Checkin.GetDateString(dtc);
            }

            // NOTE: Consider getting tip group details. Only available in the
            // request. Would be a nice future release update probably.

            var todoCount = tip["todo"];
            if (todoCount != null)
            {
                string cc = Json.TryGetJsonProperty(todoCount, "count");
                int i;
                if (int.TryParse(cc, out i))
                {
                    t.TodoCount = i;
                }
            }

            var doneCount = tip["done"];
            if (doneCount != null)
            {
                string cc = Json.TryGetJsonProperty(doneCount, "count");
                int i;
                if (int.TryParse(cc, out i))
                {
                    t.DoneCount = i;
                }
            }

            if (t.DoneCount <= 0)
                t.DoneText = "No one has done this.";
            else if (t.DoneCount == 1)
                t.DoneText = "1 person has done this.";
            else
                t.DoneText = t.DoneCount.ToString(CultureInfo.InvariantCulture) + " people have done this.";

            var photo = tip["photo"];
            if (photo != null)
            {
                var pht = Model.Photo.ParseJson(photo);
                if (pht != null)
                {
                    t.Photo = pht;
                }
            }

            string status = Json.TryGetJsonProperty(tip, "status");
            //Debug.WriteLine("tip status read as (temp): " + status);
            t.Status = TipStatus.None;
            if (status != null)
            {
                if (status == "done")
                {
                    t.Status = TipStatus.Done;
                    t.StatusText = "You've done this!";
                }
                else if (status == "todo")
                {
                    t.Status = TipStatus.Todo;
                    t.StatusText = "You need to do this!";

                    // Don't tell the user nobody has done this if it's just them.
                    if (t.DoneCount <= 0)
                    {
                        t.DoneText = null;
                    }
                }
            }

            var compactVenue = tip["venue"];
            if (compactVenue != null)
            {
                t.Venue = CompactVenue.ParseJson(compactVenue);
            }

            string parentTypeText = t.ParentType == typeof(Model.Venue) ? "venue" : null;
            if (parentTypeText == null)
            {
                if (t.ParentType == typeof (Model.UserTips))
                {
                    parentTypeText = "usertips";
                }
                else if (t.ParentType == typeof(RecommendedTipNotification) || t.ParentType == typeof(DetailedTip))
                {
                    parentTypeText = "direct";
                }
                else
                {
                    parentTypeText = "unknown";
                }
            }

            Uri tipUri = new Uri(
                string.Format(
                CultureInfo.InvariantCulture,
                "/JeffWilcox.FourthAndMayor.Lists;component/ListItem.xaml?id={0}&tipId={0}",
                                               Uri.EscapeDataString(t.TipId))
                                               , UriKind.Relative);

            t.TipUri = tipUri;

            // TODO: This supports todo w/count, done w/count, ...

            return t;
        }

        public string SpecializedComparisonString
        {
            get { return TipId; }
        }
    }
}
