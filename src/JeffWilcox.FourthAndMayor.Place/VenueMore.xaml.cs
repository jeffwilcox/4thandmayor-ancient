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
using System.Windows;
using System.Windows.Controls.Primitives;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class VenueMore : PhoneApplicationPage
    {
        //private string _id;
        //private string _name;

        public VenueMore()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string name = string.Empty;
            string address = string.Empty;

            NavigationContext.QueryString.TryGetValue("name", out name);

            string id = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("id", out id))
            {
                //_id = id;
                var vv = DataManager.Current.Load<Model.Venue>(id,
                    null,
                    (err) =>
                    {
                        /*throw new UserIntendedException(
                            "We couldn't download information about the place right now, please try again in a little while.",
                            err);*/
                    });
                DataContext = vv;

                if (vv.IsLoadComplete == false)
                {
                    vv.Name = name;
                }
            }
            else
            {
                throw new InvalidOperationException("No venue ID was specified along with the view model.");
            }
        }


        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var b = (ButtonBase)sender;
            var t = (string)b.Tag;

            var venue = DataContext as Model.Venue;

            switch (t)
            {
                case "FlagVenueDuplicate":
                    FlagThisVenue(FourSquare.VenueProblem.Duplicate);
                    break;

                case "FlagVenueClosed":
                    FlagThisVenue(FourSquare.VenueProblem.Closed);
                    break;

                case "FlagVenueMislocated":
                    FlagThisVenue(FourSquare.VenueProblem.Mislocated);
                    break;
            }
        }


        private void FlagThisVenue(FourSquare.VenueProblem problem)
        {
            var venue = DataContext as Model.Venue;
            if (venue == null)
            {
                return;
            }

            string caption;
            switch (problem)
            {
                case FourSquare.VenueProblem.Closed:
                    caption = "Closed place";
                    break;

                case FourSquare.VenueProblem.Duplicate:
                    caption = "Duplicate place";
                    break;

                case FourSquare.VenueProblem.Mislocated:
                    caption = "Mislocated place";
                    break;

                default:
                    caption = "Flag an issue";
                    break;
            }

            // trying to address random flag this venue crashes...
            /*
Invalid cross-thread access.
System.UnauthorizedAccessException
   at MS.Internal.XcpImports.CheckThread()
   at System.Windows.DependencyObject..ctor(UInt32 nativeTypeIndex, IntPtr constructDO)
   at System.Windows.Controls.ContentControl..ctor()
   at System.Windows.Controls.HeaderedContentControl..ctor()
   at JeffWilcox.Controls.WindowBase..ctor()
   at JeffWilcox.Controls.MessageBoxWindow..ctor()
   at JeffWilcox.Controls.MessageBoxWindow.Show(String text, String caption, String leftButton, String rightButton, String checkBoxContent)
   at JeffWilcox.Controls.MessageBoxWindow.Show(String text, String caption, String leftButton, String rightButton)
   at JeffWilcox.Controls.MessageBoxWindow.Show(String text, String caption, MessageBoxButton buttons)
   at JeffWilcox.FourthAndMayor.Views.VenueMore.<>c__DisplayClass8.<FlagThisVenue>b__6(Exception ex)
   at JeffWilcox.FourthAndMayor.FourSquare.<>c__DisplayClass22.<FlagVenue>b__21(String str, Exception ex)
   at JeffWilcox.FourthAndMayor.Model.FourSquareServiceRequest.DownloadStringCompleted(Object sender, MyDownloadStringCompletedEventArgs args)
   at JeffWilcox.FourthAndMayor.WebRequestClient.BeginResponse(IAsyncResult ar)
   at System.Net.Browser.ClientHttpWebRequest.<>c__DisplayClassa.<InvokeGetResponseCallback>b__8(Object state2)
   at System.Threading.ThreadPool.WorkItem.doWork(Object o)
   at System.Threading.Timer.ring()
             * */
            Dispatcher.BeginInvoke(() =>
            {
                MessageBoxWindow mbw = MessageBoxWindow.Show(
                    "Are you sure you want to report this problem?",
                    caption,
                    "yes, report",
                    "cancel");

                // TODO: 500ms of global loading for this type of action!

                mbw.LeftButtonClick += (x, xe) =>
                {
                    FourSquare.Instance.FlagVenue(venue.VenueId, problem,
                        () =>
                        {
                            Dispatcher.BeginInvoke(() => NavigationService.GoBackWhenReady());

                            Dispatcher.BeginInvoke(() =>
                            {
                                MessageBoxWindow.Show(
                                    "Thanks for reporting this issue. The foursquare moderators and super users will handle it from here.",
                                    null,
                                    MessageBoxButton.OK);
                            });
                        },
                        (ex) =>
                        {
                            Dispatcher.BeginInvoke(() => NavigationService.GoBackWhenReady());

                            Dispatcher.BeginInvoke(() =>
                            {
                                MessageBoxWindow.Show(
                                    "There was trouble reporting the issue. There could be a foursquare server issue. Can you try again later please?",
                                    null,
                                    MessageBoxButton.OK);
                            }); // fixed a crash.
                        });
                };
            });
        }
    }
}
