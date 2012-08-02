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
using AgFx;

namespace JeffWilcox.FourthAndMayor
{
    public class WittyBanter
    {
        private static Random _rand = new Random();

        public WittyBanter()
        {
            if (_banter == null)
            {
                _banter = this;
                PriorityQueue.AddWorkItem(Initialize);
            }
        }

        public string WelcomeBanter
        {
            get
            {
                return _banter.GetBanterFrom(_welcome);
            }
        }

        public string Banter
        {
            get
            {
                // Note that we don't fire property change notifications!
                return _banter.GetBanterFrom(_other);
            }
        }

        private void Initialize()
        {
            _welcome.Add(AppResources.InformalHi);
            _welcome.Add(AppResources.InformalHi);

            _welcome.Add(AppResources.InformalHello);
            _welcome.Add(AppResources.InformalHey);
            _welcome.Add(AppResources.InformalGreetings);

            DateTime now = DateTime.Now;
            string time = AppResources.InformalHello;
            if (now.Hour >= 4 && now.Hour < 6)
            {
                time = AppResources.InformalEarlyMorning;
            }
            else if (now.Hour >= 6 && now.Hour < 11)
            {
                time = AppResources.InformalGoodMorning;
            }
            else if (now.Hour >= 11 && now.Hour < 13)
            {
                time = AppResources.InformalLunchtime;
            }
            else if (now.Hour >= 13 && now.Hour < 17)
            {
                time = AppResources.InformalGoodAfternoon;
            }
            else if (now.Hour >= 17 && now.Hour < 20)
            {
                time = AppResources.InformalGoodEvening;
            }
            else if (now.Hour >= 20 && now.Hour < 23)
            {
                time = AppResources.InformalGoodNight;
            }
            _welcome.Add(time);
            _welcome.Add(time);
            _welcome.Add(time);
        }

        private string GetBanterFrom(List<string> list)
        {
            int size = list.Count;
            if (size > 0)
            {
                int index = _rand.Next(0, size - 1);
                return list[index];
            }
            else return AppResources.InformalLoading;
        }

        private List<string> _welcome = new List<string>();
        private List<string> _other = new List<string>();

        private static WittyBanter _banter;
    }
}
