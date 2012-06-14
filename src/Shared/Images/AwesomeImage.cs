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

// Originally built for 4th & Mayor, these components were refactored and 
// prepared for developer geek friends. Support is pretty limited though! :-)

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AgFx;

namespace JeffWilcox.Controls
{
    /// <summary>
    /// AwesomeImage allows for data binding scenarios with the 
    /// AwesomeScrollViewer control.
    /// </summary>
    public class AwesomeImage
    {
        private static IsoStoreCache _iso;

        private const int RecentLimit = 12;

        private static Queue<Uri> _recent = new Queue<Uri>();

        private static Dictionary<Uri, bool> _recentQuick = new Dictionary<Uri, bool>();

        static AwesomeImage()
        {
             _iso = new IsoStoreCache();

             System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => 
             { 
                 _iso.CollectGarbage(); 
             });
        }

        internal static void TransferToImage(Image image, Uri uri)
        {
            SetUriSource(image, null);
            PendingRequest pr = new PendingRequest(image, uri);
            PriorityQueue.AddWorkItem(() =>
                {
                    PendingRequest ppr = pr;
                    ProcessTransfer(ppr);
                });

            // The original implementation that I created would simply flip the
            // image.Source = new BitmapImage(uri) bit. I've decided not to do
            // that in this newer version where I manage my own set of bytes[].

            if (!_recentQuick.ContainsKey(uri))
            {
                if (_recent.Count == RecentLimit)
                {
                    Uri byebyebye = _recent.Dequeue();
                    _recentQuick.Remove(byebyebye);
                }

                _recentQuick.Add(uri, true);
                _recent.Enqueue(uri);
            }
        }

        private class PendingRequest
        {
            public Image Image { get; private set; }
            public Uri Uri { get; private set; }
            public PendingRequest(Image image, Uri uri)
            {
                Image = image;
                Uri = uri;
            }
        }

        private class ResponseState
        {
            public WebRequest WebRequest { get; private set; }
            public Image Image { get; private set; }
            public Uri Uri { get; private set; }
            public ResponseState(WebRequest webRequest, Image image, Uri uri)
            {
                WebRequest = webRequest;
                Image = image;
                Uri = uri;
            }
        }

        private class PendingCompletion
        {
            public Image Image { get; private set; }
            public Uri Uri { get; private set; }
            public Stream Stream { get; private set; }
            public PendingCompletion(Image image, Uri uri, Stream stream)
            {
                Image = image;
                Uri = uri;
                Stream = stream;
            }
        }

        private static void HandleGetResponseResult(IAsyncResult result)
        {
            var pendingResponse = result;
            PriorityQueue.AddWorkItem(() =>
                {
                    var responseState = (ResponseState)pendingResponse.AsyncState;
                    try
                    {
                        var response = responseState.WebRequest.EndGetResponse(pendingResponse);

                        byte[] bytes = null;
                        using (var stream = response.GetResponseStream())
                        {
                            bytes = new byte[stream.Length];
                            stream.Read(bytes, 0, (int)stream.Length);
                        }

                        var pc = new PendingCompletion(responseState.Image, responseState.Uri, new MemoryStream(bytes));
                        PriorityQueue.AddStorageWorkItem(() =>_iso.Write(responseState.Uri, bytes));
                        PriorityQueue.AddUiWorkItem(() => HandleCompletion(pc));

                        //System.Diagnostics.Debug.WriteLine("Saving to the iso image cache...");
                    }
                    catch (WebException)
                    {
                        // Ignore web exceptions (ex: not found)
                    }
                    catch
                    {
                        // Other exceptions...
                    }
                });
        }

        private static void HandleCompletion(PendingCompletion pendingCompletion)
        {
            // fadeClear, clearFade, clear, fadeRemoveSiblings are all specific
            // side effects that met my needs when building 4th & Mayor. At
            // this time I'm keeping these around.

            var bitmap = new BitmapImage();
            try
            {
                bitmap.SetSource(pendingCompletion.Stream);
                pendingCompletion.Image.Source = bitmap;
                var tag = pendingCompletion.Image.Tag as string;

                if (tag != null && tag == "clear")
                {
                    var img = pendingCompletion.Image;
                    ClearParentBackground(img);
                }
                else if (tag != null && (tag == "fadeClear" || tag == "clearFade"))
                {
                    var img = pendingCompletion.Image;
                    OpacityAnimator oa = null;
                    OpacityAnimator.EnsureAnimator(img, ref oa);
                    if (img != null)
                    {
                        img.Opacity = 0;
                        oa.GoTo(1.0, new Duration(TimeSpan.FromSeconds(1)), () =>
                            {
                                ClearParentBackground(img);
                                oa = null;
                            });
                    }
                }
                else if (tag != null && tag == "fadeRemoveSiblings")
                {
                    // Specialized for badge display.
                    var img = pendingCompletion.Image;
                    OpacityAnimator oa = null;
                    OpacityAnimator.EnsureAnimator(img, ref oa);
                    if (img != null)
                    {
                        img.Opacity = 0;
                        oa.GoTo(1.0, new Duration(TimeSpan.FromSeconds(1)), () =>
                        {
                            ClearNonImageSiblings(img);
                            oa = null;
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Ignore image decode exceptions (ex: invalid image)
                // TODO: Consider what to do here.
                //QuietWatson.ReportException(ex);
            }
            finally
            {
                // Dispose of response stream
                if (pendingCompletion.Stream != null)
                {
                    pendingCompletion.Stream.Dispose();
                }
            }
        }

        private static void ClearParentBackground(Image image)
        {
            var grid = image.Parent as Grid;
            if (grid != null)
            {
                grid.Background = null;
            }
        }

        private static void ClearNonImageSiblings(Image image)
        {
            var p = image.Parent as Panel;
            if (p != null)
            {
                List<UIElement> toRemove = new List<UIElement>();
                foreach (var uie in p.Children)
                {
                    if (!(uie is Image))
                    {
                        toRemove.Add(uie);
                    }
                }
                foreach (var uie in toRemove)
                {
                    p.Children.Remove(uie);
                }
            }
        }

        private static void ProcessTransfer(PendingRequest pendingRequest)
        {
            if (pendingRequest == null || pendingRequest.Uri == null)
            {
                return;
            }

            try
            {
                if (pendingRequest.Uri.IsAbsoluteUri)
                {
                    _iso.GetItem(pendingRequest.Uri, (img, exc, stat) =>
                        {
                            if (stat == IsoStoreCache.ItemCacheStatus.Hit)
                            {
                                var ms = new MemoryStream(img);
                                var pc = new PendingCompletion(pendingRequest.Image, pendingRequest.Uri, ms);
                                PriorityQueue.AddUiWorkItem(() =>
                                {
                                    HandleCompletion(pc);
                                });
                            }
                            else
                            {
                                // Download from network
                                var webRequest = HttpWebRequest.CreateHttp(pendingRequest.Uri);
                                webRequest.AllowReadStreamBuffering = true; // Don't want to block this thread or the UI thread on network access
                                webRequest.BeginGetResponse(HandleGetResponseResult, new ResponseState(webRequest, pendingRequest.Image, pendingRequest.Uri));
                            }
                        });
                }
                else
                {
                    // Load from application (must have "Build Action"="Content")
                    var originalUriString = pendingRequest.Uri.OriginalString;
                    // Trim leading '/' to avoid problems
                    var resourceStreamUri = originalUriString.StartsWith("/", StringComparison.Ordinal) ? new Uri(originalUriString.TrimStart('/'), UriKind.Relative) : pendingRequest.Uri;
                    // Enqueue resource stream for completion
                    var streamResourceInfo = Application.GetResourceStream(resourceStreamUri);
                    if (null != streamResourceInfo)
                    {
                        var pc = new PendingCompletion(pendingRequest.Image, pendingRequest.Uri, streamResourceInfo.Stream);
                        PriorityQueue.AddUiWorkItem(() =>
                            {
                                HandleCompletion(pc);
                            });
                    }
                }
            }
            catch (NullReferenceException)
            {
                // Trying to address user-found bugs here.
            }
        }

        /// <summary>
        /// Gets the value of the Uri to use for providing the contents of the Image's Source property.
        /// </summary>
        /// <param name="obj">Image needing its Source property set.</param>
        /// <returns>Uri to use for providing the contents of the Source property.</returns>
        public static Uri GetUriSource(Image obj)
        {
            return (Uri)obj.GetValue(UriSourceProperty);
        }

        /// <summary>
        /// Sets the value of the Uri to use for providing the contents of the Image's Source property.
        /// </summary>
        /// <param name="obj">Image needing its Source property set.</param>
        /// <param name="value">Uri to use for providing the contents of the Source property.</param>
        public static void SetUriSource(Image obj, Uri value)
        {
            obj.SetValue(UriSourceProperty, value);
        }

        /// <summary>
        /// Identifies the UriSource attached DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached(
            "UriSource", typeof(Uri), typeof(AwesomeImage), new PropertyMetadata(OnUriSourceChanged));
        
        private static void OnUriSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var image = (Image)o;
            var uri = (Uri)e.NewValue;

            if (uri != null)
            {
                // Check if it is recent enough in use that we want it now.
                if (_recentQuick.ContainsKey(uri))
                {
                    TransferToImage(image, uri);
                }
                else
                {
                    // Standard bump
                    AwesomeScrollViewer.QueueBump();
                }
            }
        }
    }
}
