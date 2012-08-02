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
using System.Linq;
using System.Text;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Update : ISpecializedComparisonString
    {
        public List<string> Ids { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public int CreatedAt { get; set; }
        
        public string Created { get; set; }
        
        public bool IsUnread { get; set; }

        public TargetObject TargetObject { get; set; }

        public string Text { get; set; }

        public Uri FullImage { get; set; }

        public static Update ParseJson(JToken update)
        {
            Update u = new Update();

            string created = Json.TryGetJsonProperty(update, "createdAt");
            if (created != null)
            {
                int i;
                if (int.TryParse(created, out i))
                {
                    u.CreatedAt = i;
                }

                // FUTURE: Consider an option to NOT include people in the checkin list who have not checked in within the last month. (perf default!)
                DateTime dtc = UnixDate.ToDateTime(created);
                u.CreatedDateTime = dtc;
                u.Created = Checkin.GetDateString(dtc);
            }

            // unread
            u.IsUnread = Json.TryGetJsonBool(update, "unread");

            // ids
            var idj = update["ids"];
            if (idj != null)
            {
                u.Ids = idj.Values<string>().ToList();
            }

            // target
            var targetJson = update["target"];
            if (targetJson != null)
            {
                u.TargetObject = TargetObject.ParseJson(targetJson);
            }

            // text
            u.Text = Json.TryGetJsonProperty(update, "text");

            // entities

            // image
            var img = update["image"];
            if (img != null)
            {
                u.FullImage = Json.TryGetUriProperty(img, "fullPath");
            }

            // imageType

            // [icon]

            return u;
        }

        public string SpecializedComparisonString
        {
            get
            {
                if (Ids == null || Ids.Count == 0)
                {
                    return ToString();
                }

                StringBuilder sb = new StringBuilder();
                foreach (var id in Ids)
                {
                    sb.Append(id);
                }
                return sb.ToString();
            }
        }
    }
}
