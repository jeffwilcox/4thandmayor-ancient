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

namespace JeffWilcox.FourthAndMayor
{
    using AgFx;
    using System.Diagnostics;

    public class Geocode : AgFx.NotifyPropertyChangedBase
    {
        private static readonly TimeSpan MinimumUpdateThreshold = TimeSpan.FromSeconds(30);

        public Geocode()
        {
            Update();
            Location = GeocodeService.Instance.LastGeocodeName;
        }

//        private DateTime _updated;

        private string _location;

        public void Update()
        {
/*            if (_updated + MinimumUpdateThreshold > DateTime.UtcNow)
            {
                // don't update
                Debug.WriteLine("Don't update GEO!");
            }
  *///          else
            {
                Debug.WriteLine("Updating GEO");
                GeocodeService.Instance.Update((s) =>
                    {
                        Debug.WriteLine("GEO is " + s);
                        Location = s;
                    });
            }
        }

        public bool HasLocation
        {
            get { return _location != null; }
        }

        public string Location
        {
            get { return _location ?? "Location service off or unavailable."; }
            set
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Geocode::Location updated " + value);
#endif

                _location = value;
                PriorityQueue.AddUiWorkItem(() =>
                    {
                        RaisePropertyChanged("Location");
                        RaisePropertyChanged("HasLocation");
                    });
//                _updated = DateTime.UtcNow;
            }
        }
    }
}
