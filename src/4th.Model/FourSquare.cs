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
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using AgFx;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor.Model;

namespace JeffWilcox.FourthAndMayor
{
    public class FourSquare
    {
        private static readonly FourSquare SingleInstance = new FourSquare();

        public static FourSquare Instance { get { return SingleInstance; } }

        private FourSquare()
        {
        }

        private void ValidateIsUser()
        {
            if (LocalCredentials.Current != null && string.IsNullOrEmpty(LocalCredentials.Current.UserId))
            {
                throw new UserIgnoreException();
            }
        }

        public enum RelationshipAction
        {
            Request,
            Unfriend,
            Approve,
            Deny,
        }

        public void LikeUnlikeCheckin(
            string checkinId,
            bool likeYesNo,
            Action success, 
            Action<Exception> failure)
        {
            var client = (new FourSquareWebClient()).GetWrappedClientTemporary();
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                "checkins/" + checkinId + "/like",
                GeoMethodType.None,
                "set",
                likeYesNo ? "1" : "0");
            var uri = uuri.Uri;
            var newUri = FourSquareWebClient.CreateServiceRequest(uri, true);
            client.UploadStringCompleted += (x, xe) =>
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
                        // LOCALIZE:
                        e = new UserIntendedException("There was a problem liking or unliking the checkin, please try again later.", ee);
                    }
                }
                client = null;

                if (e != null)
                    failure(e);
                else
                    success();
            };

            // POST request.
            client.UploadStringAsync(newUri, string.Empty);
            //client.DownloadStringAsyncWithPost(newUri, string.Empty);
        }

        public void SetFriendRelationship(
            string userId, 
            RelationshipAction newRelationship, 
            Action ok, 
            Action<Exception> error)
        {
            ValidateIsUser();

            string lowercaseRequest = newRelationship.ToString().ToLowerInvariant();

            var uuri = FourSquareWebClient.BuildFourSquareUri(
                    "users/" + userId + "/" + lowercaseRequest,
                    GeoMethodType.None);
            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Updating friendship");

            r.CallAsync(
                (str, ex) =>
                {
                    Exception exx = ex;

                    try
                    {
                        if (exx == null)
                        {
                            // LOCALIZE:
                            token.CompleteWithAcknowledgement("OK");

                            //var json = FourSquareDataLoaderBase.ProcessMetaAndNotificationsReturnJson(str);

                            ok();
                        }
                        else
                        {
                            token.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        exx = e;
                    }

                    if (exx != null)
                    {
                        error(exx);
                    }
                });
        }

        public void SetNotificationsHighWatermark(int highWatermark, Action ok, Action<Exception> error)
        {
            ValidateIsUser();
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        "updates/marknotificationsread",
                        GeoMethodType.None,

                        "highWatermark", highWatermark.ToString()
                        );
            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            r.CallAsync(
                (str, ex) =>
                {
                    Exception exx = ex;

                    try
                    {
                        //var json = FourSquareDataLoaderBase.ProcessMetaAndNotificationsReturnJson(str);

                        // FourSquareApp.Instance.NotificationCount = notifications.UnreadNotifications; ! 0

                        if (ex == null && ok != null)
                        {
                            ok();

                            CentralStatusManager.Instance.BeginShowTemporaryMessage("Updating foursquare");
                        }
                    }
                    catch (Exception e)
                    {
                        exx = e;
                    }

                    if (exx != null)
                    {
                        if (error != null)
                            error(exx);
                    }
                });
        }

        public void SaveSetting(string settingName, string value, Action ok, Action<Exception> error)
        {
            ValidateIsUser();
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        "settings/" + settingName + "/set",
                        GeoMethodType.None,
                        
                        "value", value
                        );
            var r = new FourSquareServiceRequest
                {
                    Uri = uuri.Uri,
                    PostString = string.Empty,
                };

            r.CallAsync(
                (str, ex) =>
                {
                    Exception exx = ex;

                    try
                    {
                        //var json = FourSquareDataLoaderBase.ProcessMetaAndNotificationsReturnJson(str);
                        if (exx == null)
                        {
                            ok();
                        }
                    }
                    catch (Exception e)
                    {
                        exx = e;
                    }

                    if (exx != null)
                    {
                        error(exx);
                    }
                });
        }

        public static bool IgnoreNextNotification { get; set; }

        public bool IsLowMemoryDevice { get; set; }

        public string VenueCreationSuccessfulId;

        // temporary nesting
        public class DuplicateVenueChallenge : NotifyPropertyChangedBase
        {
            public string OriginalName { get; set; }
            public Dictionary<string, string> OriginalDataPairs { get; set; }

            public string IgnoreDuplicatesKey { get; set; }

            public List<CompactVenue> CandidateDuplicateVenues { get; set; }
        }

        public void AddVenue(
            string name, 
            Dictionary<string, string> dataPairs, 
            Action<CompactVenue> okNewVenue, 
            Action<DuplicateVenueChallenge> duplicatesChallenge,
            Action<Exception> error)
        {
            ValidateIsUser();

            var flat = new List<string>();
            dataPairs["name"] = name;
            foreach (var kp in dataPairs)
            {
                if (!string.IsNullOrEmpty(kp.Value))
                {
                    flat.Add(kp.Key);
                    flat.Add(kp.Value);
                }
            }

            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        "venues/add",
                        GeoMethodType.Optional,
                        flat.ToArray()
                        );
            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Saving place");

            r.CallAsync(
                (str, ex) =>
                {
                    Exception exx = ex;

                    try
                    {
                        FourSquare.IgnoreNextNotification = true;
                        var json = FourSquareDataLoaderBase<LoadContext>.ProcessMetaAndNotificationsReturnJson(str);
                        FourSquare.IgnoreNextNotification = false;

                        string ignoreDuplicatesKey = Json.TryGetJsonProperty(json, "ignoreDuplicatesKey");
                        if (!string.IsNullOrEmpty(ignoreDuplicatesKey))
                        {
                            var duplicates = json["candidateDuplicateVenues"];
                            if (duplicates != null)
                            {
                                List<CompactVenue> dupes = new List<CompactVenue>();
                                foreach (var dupe in duplicates)
                                {
                                    var dd = CompactVenue.ParseJson(dupe);
                                    if (dd != null)
                                    {
                                        dupes.Add(dd);
                                    }
                                }

                                DuplicateVenueChallenge dvc = new DuplicateVenueChallenge();
                                dvc.CandidateDuplicateVenues = dupes;
                                dvc.IgnoreDuplicatesKey = ignoreDuplicatesKey;
                                duplicatesChallenge(dvc);

                                token.Complete();
                                return;
                            }
                        }

                        var vjson = json["venue"];
                        CompactVenue cv = null;
                        if (vjson != null)
                        {
                            cv = CompactVenue.ParseJson(vjson);
                        }

                        okNewVenue(cv);

                        token.CompleteWithAcknowledgement();
                    }
                    catch (Exception e)
                    {
                        exx = e;
                    }

                    if (exx != null)
                    {
                        error(exx);
                        StatusToken.TryComplete(ref token);
                    }
                });
        }

        public enum ToDoState
        {
            MarkToDo,
            MarkDone,
            Unmark,
        }

        public enum VenueProblem
        {
            Mislocated,
            Closed,
            Duplicate,
        }

        //public void UpdateTipState(string tipId, ToDoState newState, Action success, Action<Exception> error)
        //{
        //    var uuri = FourSquareWebClient.BuildFourSquareUri(
        //                string.Format(System.Globalization.CultureInfo.InvariantCulture, "tips/{0}/{1}", tipId, newState.ToString().ToLowerInvariant()),
        //                GeoMethodType.None);
        //    var r = new FourSquareServiceRequest
        //    {
        //        Uri = uuri.Uri,
        //        PostString = string.Empty,
        //    };
            
        //    var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Updating");

        //    r.CallAsync((str, ex) =>
        //    {
        //        token.Complete();

        //        if (ex != null)
        //        {
        //            error(ex);
        //        }
        //        else
        //        {
        //            success();
        //        }
        //    });
        //}

        private static void Callback<T>(Action success, Action<Exception> error, T value, Exception ex)
        {
            if (ex == null)
            {
                if (success != null)
                {
                    success();
                }
            }
            else
            {
                if (error != null)
                {
                    error(ex);
                }
            }
        }

        public void UpdateListFollowingState(string listId, bool shouldFollow, Action success, Action<Exception> error)
        {
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(
                            CultureInfo.InvariantCulture, 
                            "lists/{0}/{1}", 
                            listId,

                            // LOCALIZE:
                            shouldFollow ? "follow" : "unfollow"),
                        GeoMethodType.Optional);
            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage(shouldFollow ? "Following" : "Unfollowing");

            r.CallAsync((str, ex) =>
            {
                token.Complete();
                Callback(success, error, str, ex);
            });
        }

        public void UpdateFriendPings(string friendId, bool shouldPing, Action success, Action<Exception> error)
        {
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "users/{0}/setpings",
                            friendId),
                        GeoMethodType.Optional,
                        "value",
                        shouldPing ? "true" : "false");
            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Updating notifications system");

            r.CallAsync((str, ex) =>
            {
                token.Complete();
                Callback(success, error, str, ex);
            });
        }

        public void CreateNewList(
            string listTitle, 
            string optionalDescription, 
            bool isCollaborative,
            Action<CompactList> success, 
            Action<Exception> error)
        {
            List<string> components = new List<string>
            {
                "name", 
                listTitle
            };
            if (!string.IsNullOrEmpty(optionalDescription))
            {
                components.Add("description");
                components.Add(optionalDescription);
            }
            if (isCollaborative)
            {
                components.Add("collaborative");
                components.Add("true"); // or "true" as docs say?
            }

            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        "lists/add",
                        GeoMethodType.Optional,

                        components.ToArray());

            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Creating new list");

            r.CallAsync(
                (str, ex) =>
                {
                    Exception exx = ex;

                    try
                    {
                        FourSquare.IgnoreNextNotification = true;
                        var json = FourSquareDataLoaderBase<LoadContext>.ProcessMetaAndNotificationsReturnJson(str);
                        FourSquare.IgnoreNextNotification = false;

                        var vjson = json["list"];
                        CompactList cl = null;
                        if (vjson != null)
                        {
                            cl = CompactList.ParseJson(vjson);
                        }

                        success(cl);

                        token.CompleteWithAcknowledgement();
                    }
                    catch (Exception e)
                    {
                        exx = e;
                    }

                    if (exx != null)
                    {
                        error(exx);
                        StatusToken.TryComplete(ref token);
                    }
                });
        }

        public void UpdateListAddItem(string listId, 
            string itemType,
            string itemId, 
            string addingEllipsisMessage,
            Action success, Action<Exception> error, string optionalText = null)
        {
            List<string> components = new List<string>
            {
                itemType, 
                itemId
            };
            if (!string.IsNullOrEmpty(optionalText))
            {
                components.Add("text");
                components.Add(optionalText);
            }

            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "lists/{0}/additem",
                            listId),
                        GeoMethodType.Optional,

                        components.ToArray());

            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage(addingEllipsisMessage);

            r.CallAsync((str, ex) =>
            {
                token.Complete();
                Callback(success, error, str, ex);
            });
        }

        public void UpdateListAddItemFromList(string listId,
            string originalListId,
            string itemId,
            string addingEllipsisMessage,
            Action success, Action<Exception> error, string optionalText = null)
        {
            List<string> components = new List<string>
            {
                "listId", 
                originalListId,

                "itemId",
                itemId
            };
            if (!string.IsNullOrEmpty(optionalText))
            {
                components.Add("text");
                components.Add(optionalText);
            }

            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "lists/{0}/additem",
                            listId),
                        GeoMethodType.Optional,

                        components.ToArray());

            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage(addingEllipsisMessage);

            r.CallAsync((str, ex) =>
            {
                token.Complete();
                Callback(success, error, str, ex);
            });
        }

        public void UpdateListRemoveItem(string listId,
            string itemId,
            string ellipsisMessage,
            Action success, Action<Exception> error)
        {
            List<string> components = new List<string>
            {
                "listId", 
                listId,

                "itemId",
                itemId
            };

            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "lists/{0}/deleteitem",
                            listId),
                        GeoMethodType.Optional,

                        components.ToArray());

            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage(ellipsisMessage);

            r.CallAsync((str, ex) =>
            {
                token.Complete();
                Callback(success, error, str, ex);
            });
        }


        public void AddPhoto(PhotoAddLoadContext context, Action<Photo> success, Action<Exception> error)
        {
            var @params = context.GetMultipartFormParameters();

            var la = LocationAssistant.Instance.LastKnownLocation;
            // TODO: Centralize the dictionary filling of this between this method.
            if (la != null && !double.IsNaN(la.Latitude) && !double.IsNaN((la.Longitude)))
            {
                @params["ll"] = la.Latitude.ToString(CultureInfo.InvariantCulture)
                    + "," + la.Longitude.ToString(CultureInfo.InvariantCulture);
                if (!double.IsNaN(la.HorizontalAccuracy))
                {
                    @params["llAcc"] = la.HorizontalAccuracy.ToString(CultureInfo.InvariantCulture);
                }
                if (!double.IsNaN(la.VerticalAccuracy) && la.VerticalAccuracy != 0.0 && !double.IsNaN(la.Altitude))
                {
                    @params["altAcc"] = la.VerticalAccuracy.ToString(CultureInfo.InvariantCulture);
                    @params["alt"] = la.Altitude.ToString(CultureInfo.InvariantCulture);
                }
            }

            var uri = FourSquareWebClient.BuildFourSquareUri(
                        "photos/add",
                        GeoMethodType.Optional);

            var r = new FourSquareServiceRequest
            {
                Uri = uri.Uri,
                PostBytes = context.GetPhotoBytes(),
            };

            r.PostParameters = @params;

            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Uploading photo");
            
            r.CallAsync((str, ex) =>
            {
                if (ex != null)
                {
                    token.Complete();
                    error(ex);
                }
                else
                {
                    token.CompleteWithAcknowledgement();

                    Photo photo = null;

                    var json = FourSquareDataLoaderBase<LoadContext>.ProcessMetaAndNotificationsReturnJson(str); // , request.VenueId);
                    if (json != null)
                    {
                        var p = json["photo"];
                        if (p != null)
                        {
                            photo = Photo.ParseJson(p);
                        }
                    }

                    success(photo);
                }
            });
        }

        //public void AddVenueToDo(string venueId, string optionalText, Action success, Action<Exception> error)
        //{
        //    string[] components = string.IsNullOrEmpty(optionalText)
        //                              ? new string[] {}
        //                              : new string[] {"text", optionalText};
        //    var uuri = FourSquareWebClient.BuildFourSquareUri(
        //                string.Format(System.Globalization.CultureInfo.InvariantCulture, "venues/{0}/{1}", venueId, "marktodo"),
        //                GeoMethodType.Optional, components);
        //    var r = new FourSquareServiceRequest
        //    {
        //        Uri = uuri.Uri,
        //        PostString = string.Empty,
        //    };

        //    var token = CentralStatusManager.Instance.BeginLoading();

        //    r.CallAsync((str, ex) =>
        //    {
        //        token.Complete();
        //        Callback(success, error, str, ex);
        //    });
        //}

        public void DeletePhoto(string photoId, Action success, Action<Exception> error)
        {
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "photos/{0}/delete", photoId),
                        GeoMethodType.Optional);
            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            // LOCALIZE:
            var token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Deleting");

            r.CallAsync((str, ex) =>
            {
                if (ex != null)
                {
                    token.Complete();
                }
                else
                {
                    token.CompleteWithAcknowledgement();
                }
                Callback(success, error, str, ex);
            });
        }

        public void FlagPhoto(string photoId, PhotoProblem problem, Action success, Action<Exception> error)
        {
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "photos/{0}/flag", photoId),
                        GeoMethodType.Optional,
                        "problem",
                        Photo.ProblemEnumToString(problem));
            var r = new FourSquareServiceRequest
            {
                Uri = uuri.Uri,
                PostString = string.Empty,
            };

            var token = CentralStatusManager.Instance.BeginLoading();

            r.CallAsync((str, ex) =>
            {
                token.Complete();

                if (ex != null)
                {
                    error(ex);
                }
                else
                {
                    success();
                }
            });
        }

        public void FlagVenue(string venueId, VenueProblem problem, Action success, Action<Exception> error)
        {
            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "venues/{0}/flag", venueId),
                        GeoMethodType.Optional,

                        "problem",
                        problem.ToString().ToLowerInvariant());
            var r = new FourSquareServiceRequest
                {
                    Uri = uuri.Uri,
                    PostString = string.Empty,
                };

            var token = CentralStatusManager.Instance.BeginLoading();

            // NICE: Simplify by adding a new CallAsync method that returns JSON.
            r.CallAsync((str, ex) =>
                {
                    token.Complete();

                    if (ex != null)
                    {
                        error(ex);
                    }
                    else
                    {
                        success();
                    }
                });
        }

        #region Check-in

        public CheckinRequest MostRecentCheckinRequest { get; set; }

        public void Checkin(CheckinRequest request, Action<CheckinResponse> success, Action<Exception> error)
        {
            if (LocalCredentials.Current != null && string.IsNullOrEmpty(LocalCredentials.Current.UserId))
            {
                throw new UserIgnoreException();
            }

            MostRecentCheckinRequest = request;

            var uuri = FourSquareWebClient.BuildFourSquareUri(
                        "checkins/add",
                        GeoMethodType.Optional,
                        request.GetRestParameters());
            var r = new FourSquareServiceRequest
                {
                    Uri = uuri.Uri,
                    PostString = string.Empty
                };

            var token = CentralStatusManager.Instance.BeginLoading();

            // NICE: Simplify by adding a new CallAsync method that returns JSON.
            r.CallAsync(
                (str, ex) =>
                    {
                        token.Complete();

                        Exception exx = ex;

                        try
                        {
                            var json = FourSquareDataLoaderBase<LoadContext>.ProcessMetaAndNotificationsReturnJson(str, request.VenueId);
                            var response = CheckinResponse.ParseJson(json);

                            success(response);

                                        var ias = Application.Current as IAnalyticsSettings;
                                        if (ias != null)
                                        {
                                            var ai = ias.AnalyticsInfo;
                                            if (ai != null)
                                            {
                                                // Increment the app's check-in counter.
                                                ai.Checkins++;
                                                // FourSquareApp.Instance.AnalyticsInfo.Checkins++;
                                            }
                                        }
                        }
                        catch(Exception e)
                        {
                            exx = e;
                        }

                        if (exx != null)
                        {
                            error(exx);
                        }
                    });
        }

        #endregion
    }
}
