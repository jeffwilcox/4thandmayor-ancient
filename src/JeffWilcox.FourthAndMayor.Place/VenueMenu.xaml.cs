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
using System.Linq;
using System.Windows.Navigation;
using AgFx;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Place
{
    public partial class VenueMenu : PhoneApplicationPage
    {
        public class VenueMenuViewModel : NotifyPropertyChangedBase
        {
            private string _name;
            public string VenueName
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                    RaisePropertyChanged("VenueName");
                }
            }

            private Model.VenueMenu _menus;
            public Model.VenueMenu Menus
            {
                get { return _menus; }
                set
                {
                    _menus = value;
                    RaisePropertyChanged("Menus");
                }
            }

            private Model.Menu _menu;
            public Model.Menu Menu {
                get
                {
                    return _menu;
                }
                set
                {
                    _menu = value;
                    RaisePropertyChanged("Menu");
                }
            }
        }

        public VenueMenu()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = string.Empty;
            string menu = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("venueid", out id))
            {
                NavigationContext.QueryString.TryGetValue("menuid", out menu);

                DataContext = new VenueMenuViewModel
                {
                    Menu = null,
                    VenueName = "MENU"
                };

                //_id = id;
                var vv = DataManager.Current.Load<Model.VenueMenu>(id,
                    (done) =>
                    {
                        var oneMenu = done.Menus.Where(m => m.MenuId == menu).FirstOrDefault();
                        Dispatcher.BeginInvoke(() =>
                            {
                                var dc = DataContext as VenueMenuViewModel;
                                if (dc != null)
                                {
                                    dc.Menu = oneMenu;
                                }

                                dc.Menus = done;

                                // Load the venue's name as a nice-to-have now.
                                DataManager.Current.Load<Model.Venue>(id, (ok) =>
                                    {
                                        if (dc != null && ok != null)
                                        {
                                            dc.VenueName = ok.Name;
                                        }
                                    }, (notOk) => { });
                            });
                    },
                    (err) =>
                    {
                        /*throw new UserIntendedException(
                            "We couldn't download information about the place right now, please try again in a little while.",
                            err);*/
                    });
            }
            else
            {
                throw new InvalidOperationException("No venue ID was specified along with the view model.");
            }
        }
    }
}
