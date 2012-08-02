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
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class Todo
    {
        public string TodoId { get; set;}

        public string Created { get; set; }

        public Tip Tip { get; set; }

        public override string ToString()
        {
            return TodoId;
        }

        public static Todo ParseJson(JToken json)
        {
            Todo todo = new Todo();
            string id = Json.TryGetJsonProperty(json, "id");

            string created = Json.TryGetJsonProperty(json, "createdAt");
            if (created != null)
            {
                DateTime dtc = UnixDate.ToDateTime(created);
                //t.CreatedDateTime = dtc;
                todo.Created = Checkin.GetDateString(dtc);
            }

            var tipJson = json["tip"];
            if (tipJson != null)
            {
                var tip = Tip.ParseJson(tipJson);
                if (tip != null)
                {
                    if (todo.Created != null)
                    {
                        // LOCALIZE:
                        tip.OverrideAddedText = "added " + todo.Created + (
                            tip.User != null ? (" (via " + tip.User.ToString() + ")") : string.Empty);
                    }

                    todo.Tip = tip;
                }
            }

            todo.TodoId = id;

            return todo;
        }
    }
}