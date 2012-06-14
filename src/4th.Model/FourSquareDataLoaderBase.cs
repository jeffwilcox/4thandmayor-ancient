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
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    using Controls;
    using System.Windows;

    public abstract class FourSquareDataLoaderBase<T> : 
        DataLoaderBase<T> 
        where T: LoadContext
    {
        protected virtual FourSquareLoadRequest BuildRequest(LoadContext context, JeffWilcox.FourthAndMayor.FourSquareWebClient.UriErrorPair uri)
        {
            return new FourSquareLoadRequest(context, uri);
        }

        protected virtual FourSquareLoadRequest BuildPostRequest(LoadContext context, JeffWilcox.FourthAndMayor.FourSquareWebClient.UriErrorPair uri, string postData)
        {
            return new FourSquareLoadRequest(context, uri, postData);
        }

        protected virtual FourSquareLoadRequest BuildPostRequest(LoadContext context, JeffWilcox.FourthAndMayor.FourSquareWebClient.UriErrorPair uri, Dictionary<string, string> parameters, byte[] postData)
        {
            var ld = new FourSquareLoadRequest(context, uri, postData, parameters);
            ld.SetMultipartPostData(postData);
            return ld;
        }

        public override object Deserialize(T context, Type objectType, Stream stream)
        {
            string json;
            using (var sr = new StreamReader(stream))
            {
                json = sr.ReadToEnd();
            }
            var response = ProcessMetaAndNotificationsReturnJson(json);
            return DeserializeCore(response, objectType, context);
        }

        private static bool _oneTimeMessageBoxShown;
        public static JObject ProcessMetaAndNotificationsReturnJson(string json, string venueId = null)
        {
            // JSON-P needs to be parsed. We just use the callback of 'j'.
            int function = json.IndexOf('(');

            // If Foursquare is rather broken, the JSON-P will never be 
            // used. I've received this once on 1/26/2011.
            Debug.Assert(function >= 0);
            if (function < 0)
            {
                //"{\"meta\":{\"code\":500,\"errorType\":\"server_error\",\"errorDetail\":\"Foursquare servers are experiencing problems. Please retry and check status.foursquare.com for updates.\"},\"response\":{}}\n"
                if (json.Contains("500") && json.Contains("error"))
                {
                    // TODO: OneTimeMessageBox
                    if (!_oneTimeMessageBoxShown)
                    {
                        _oneTimeMessageBoxShown = true;
                        PriorityQueue.AddUiWorkItem(() =>
                            {
                                MessageBoxWindow.Show("The Foursquare servers are experiencing problems. Please check status.foursquare.com for updates.",
                                    "Foursquare is down", System.Windows.MessageBoxButton.OK);
                            });
                    }
                    return null;
                }
            }

            JObject response = null;

            try
            {
                string updated = json.Substring(function + 1, json.Length - function - 3);
                JObject jo = JObject.Parse(updated);

                var meta = jo["meta"];
                if (meta != null)
                {
                    string errorCode = Json.TryGetJsonProperty(meta, "errorType");
                    if (errorCode != null)
                    {
                        switch (errorCode)
                        {
                            case "invalid_auth":
                                var so = Application.Current as ISignOutAndClear;
                                if (so != null)
                                {
                                    so.SignOutAndClear();
                                    // FourSquareApp.Instance.SignOutAndClear();
                                }
                                throw new UserIntendedException(
                                    "Invalid OAuth credentials, please sign in again.",
                                    new InvalidOperationException("Invalid OAUTH"));
                            // OAuth token was not provided or was invalid.

                            case "param_error":
                                throw new InvalidOperationException("param error");
                            // A required parameter was missing or a parameter was malformed. This is also used if the resource ID in the path is incorrect.

                            case "endpoint_error":
                                throw new InvalidOperationException("The endpoint doesn't exist.");
                            // The requested path does not exist.

                            case "not_authorized":
                                // Although authentication succeeded, the acting user is not allowed to see this information due to privacy restrictions.
                                throw new InvalidOperationException("Not authorized.");

                            case "rate_limit_exceeded":
                                // Rate limit for this hour exceeded.
                                throw new UserIntendedException(
                                    "You've been using foursquare a lot this hour and are now rate limited!",
                                    new InvalidOperationException("Rate limit exceeded."));

                            case "deprecated":
                                // Something about this request is using deprecated functionality, or the response format may be about to change.
                                //QuietWatson.ReportException(
                                //    new InvalidOperationException("The foursquare API was deprecated.",
                                //                                  new InvalidOperationException(meta.ToString())));
                                break;

                            case "server_error":
                                throw new UserIntendedException(
                                    "Foursquare's servers are having difficulty. Please check status.foursquare.com for updates.",
                                    new InvalidOperationException("Server error"));
                            // Server is currently experiencing issues. Check status.foursquare.com for updates.

                            case "other":
                                break; // No longer failing out here...
//                                throw new InvalidOperationException("other error");
                            // Some other type of error occurred.
                        }
                    }

                    string httpStatusCode = Json.TryGetJsonProperty(meta, "code");
                    int code;
                    if (int.TryParse(httpStatusCode, out code))
                    {
                        switch (code)
                        {
                            // TODO: BEFORE INGESTION: FIGURE OUT THIS ERROR HANDLING!
                            case 400:
                                // Bad Request
                                /*
                                 * Any case where a parameter is invalid, or a required parameter is missing. This includes the case where no OAuth token is provided and the case where a resource ID is specified incorrectly in a path.
                                 */
                                break;

                            case 401:
                                // Unauthorized
                                /*
                                 * The OAuth token was provided but was invalid.
                                 * */

                                // Should probably...
                                // 1. Sign out
                                // (inc. clear iso-store)
                                // 2. Inform the user and have them sign in again

                                break;

                            case 403:
                                // Forbidden
                                /*
                                 * The requested information cannot be viewed by the acting user, for example, because they are not friends with the user whose data they are trying to read.
                                 * */
                                break;

                            case 404:
                                // Not found
                                /*
                                 * Endpoint does not exist.
                                 * */

                                break;

                            case 405:
                                // Method not allowed
                                /*
                                 * Attempting to use POST with a GET-only endpoint, or vice-versa.
                                 * */
                                break;

                            case 409:
                                // Conflict!
                                // Duplicate venue being added.
                                // For NOW, not doing anything here...
                                break;
                        }
                    }

                    Debug.Assert(httpStatusCode == "200" || httpStatusCode == "409");
                }

                response = (JObject)jo["response"];
                if (response != null)
                {
                    // JObject response = (JObject)jo["response"];

                    var notifications = jo["notifications"];
                    if (notifications != null)
                    {
                        if (FourSquare.IgnoreNextNotification)
                        {
                            FourSquare.IgnoreNextNotification = false;
                        }
                        else
                        {
                            var app = Application.Current as IProcessNotifications;
                            var optionalCheckinRequest = FourSquare.Instance.MostRecentCheckinRequest;
                            FourSquare.Instance.MostRecentCheckinRequest = null;
                            ((IProcessNotifications)app).ProcessNotifications(notifications, venueId, optionalCheckinRequest);
                            // FourSquareApp.Instance.ProcessNotifications(notifications, venueId);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                // Sometimes happening parsing JSON.
            }
            catch (UserIntendedException ex)
            {
                QuickMessageBox(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // These in theory are less user-friendly.
                QuickMessageBox(ex.Message);
            }

#if DEBUG
            if (response == null)
            {
                // Let's assume a message was already shown.
                //throw new InvalidOperationException("No response provided.");
            }
#endif

            return response ?? new JObject(); // new for 8/12/2011: hope this doesn't mess up too much parsing.
        }

        protected abstract object DeserializeCore(JObject json, Type objectType, T context);

        private static void QuickMessageBox(string message)
        {
            PriorityQueue.AddUiWorkItem(() =>
            {
                MessageBoxWindow.Show(message,
                    "Foursquare communication problem", System.Windows.MessageBoxButton.OK);
            });
        }
    }
}
