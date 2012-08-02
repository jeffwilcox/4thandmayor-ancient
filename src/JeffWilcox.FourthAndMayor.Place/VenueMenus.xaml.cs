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
using System.Windows.Navigation;
using AgFx;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Place
{
    public partial class VenueMenus : PhoneApplicationPage
    {
        public VenueMenus()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string name = string.Empty;
            //string address = string.Empty;

            NavigationContext.QueryString.TryGetValue("name", out name);

            string id = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("id", out id))
            {
                //_id = id;
                var vv = DataManager.Current.Load<Model.VenueMenu>(id,
                    (ok) =>
                    {
                        ok.VenueName = name; // Not a real property.
                    },
                    (err) =>
                    {
                        /*throw new UserIntendedException(
                            "We couldn't download information about the place right now, please try again in a little while.",
                            err);*/
                    });
                DataContext = vv;

                //if (vv.IsLoadComplete == false)
                //{
                    vv.VenueName = name; // ?

                //}
            }
            else
            {
                throw new InvalidOperationException("No venue ID was specified along with the view model.");
            }
        }
    }
}
