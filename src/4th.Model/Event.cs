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
using System.Windows;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    using Controls;
    using System.Collections.Generic;

    public class Event : NotifyPropertyChangedBase, ISpecializedComparisonString
    {
        public Event()
        {
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Summary { get; set; }

        // ? public string Text

        public bool IsAllDay { get; set; }

        public List<Uri> Images { get; set; }

        public Uri FirstImage
        {
            get
            {
                return (Images != null && Images.Count > 0) ? Images[0] : null;
            }
        }

        public List<Category> Categories { get; set; }

        // TODO: hereNow; count, groups ... friends, etc.

        public static Event ParseJson(JToken ejson)
        {
            Event e = new Event();

            e.Id = Json.TryGetJsonProperty(ejson, "id");

            string isAllDay = Json.TryGetJsonProperty(ejson, "allDay");
            if (isAllDay != null && isAllDay.ToLowerInvariant() == "true")
            {
                e.IsAllDay = true;
            }

            e.Name = Json.TryGetJsonProperty(ejson, "name");

            // foreignIds ?
        //            "foreignIds": {
        //  "count": 0,
        //  "items": []
        //},

            // categories

            // hereNow
            /*
        "hereNow": {
          "count": 0,
          "groups": [
            {
              "type": "friends",
              "name": "Friends here",
              "count": 0,
              "items": []
            },
            {
              "type": "others",
              "name": "Other people here",
              "count": 0,
              "items": []
            }
          ]
        }

             * */


            return e;
        }

        public string SpecializedComparisonString
        {
            get
            {
                return Id;
            }
        }
    }
}
