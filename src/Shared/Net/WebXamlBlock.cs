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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using AgFx;
using JeffWilcox.FourthAndMayor;

namespace JeffWilcox.Controls
{
    public class WebXamlBlock : ContentControl
    {
        #region public Uri XamlUri
        /// <summary>
        /// Gets or sets the XAML Uri.
        /// </summary>
        public Uri XamlUri
        {
            get { return GetValue(XamlUriProperty) as Uri; }
            set { SetValue(XamlUriProperty, value); }
        }

        /// <summary>
        /// Identifies the XamlUri dependency property.
        /// </summary>
        public static readonly DependencyProperty XamlUriProperty =
            DependencyProperty.Register(
                "XamlUri",
                typeof(Uri),
                typeof(WebXamlBlock),
                new PropertyMetadata(null, OnXamlUriPropertyChanged));

        /// <summary>
        /// XamlUriProperty property changed handler.
        /// </summary>
        /// <param name="d">WebXamlBlock that changed its XamlUri.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnXamlUriPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebXamlBlock source = d as WebXamlBlock;
            source.TryDownloading();
        }
        #endregion public Uri XamlUri

        #region public object FallbackContent
        /// <summary>
        /// Gets or sets the content to fallback to if the request fails.
        /// </summary>
        public object FallbackContent
        {
            get { return GetValue(FallbackContentProperty) as object; }
            set { SetValue(FallbackContentProperty, value); }
        }

        /// <summary>
        /// Identifies the FallbackContent dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackContentProperty =
            DependencyProperty.Register(
                "FallbackContent",
                typeof(object),
                typeof(WebXamlBlock),
                new PropertyMetadata(null));
        #endregion public object FallbackContent

        public WebXamlBlock()
        {
        }

        public event EventHandler ContentReady;

        protected virtual void OnContentReady()
        {
            var handler = ContentReady;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            StopLoading();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            OnContentReady();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TryDownloading();
        }

        private bool _haveTried;

        private StatusToken _loadingToken;

        private void TryDownloading()
        {
            if (_haveTried)
            {
                return;
            }
            _haveTried = true;
            if (XamlUri != null)
            {
                IWebRequestFactory iwrf = Application.Current as IWebRequestFactory;
                if (iwrf != null)
                {
                    _loadingToken = CentralStatusManager.Instance.BeginShowEllipsisMessage("Downloading");

                    var ff = iwrf.CreateWebRequestClient();
                    var f = ff.GetWrappedClientTemporary();
                    f.DownloadStringCompleted += OnDownloadStringCompleted;
                    f.DownloadStringAsync(XamlUri);
                    //var wr = f.CreateSimpleWebRequest(XamlUri);
                    //f.DownloadStringAsync(wr);
                }
                else
                {
                    OnError();
                }
            }
        }

        private bool _hasStoppedLoading;
        private void StopLoading()
        {
            if (!_hasStoppedLoading)
            {
                _hasStoppedLoading = true;

                if (_loadingToken != null)
                {
                    _loadingToken.Complete();
                }
            }
        }

        private void OnError()
        {
            PriorityQueue.AddUiWorkItem(() =>
                {
                    StopLoading();

                    var b = new Binding("FallbackContent") {Source = this};
                    SetBinding(ContentProperty, b);
                });
        }

        private void OnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                OnError();
            }
            else
            {
                string xaml = e.Result;
                PriorityQueue.AddUiWorkItem(() =>
                    {
                        try
                        {
                            var o = XamlReader.Load(xaml);
                            if (o != null)
                            {
                                Content = o;
                            }
                        }
                        catch
                        {
                            OnError();
                        }
                    });
            }
        }
    }
}
