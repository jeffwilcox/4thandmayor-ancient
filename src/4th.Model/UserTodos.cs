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
using System.Collections.Generic;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class UserTodos : FourSquareItemBase<LoadContext>
    {
        public UserTodos()
            : base()
        {
        }

        public UserTodos(LoadContext context)
            : base(context)
        {
        }

        private List<Todo> _todos;
        public List<Todo> Todos
        {
            get { return _todos; }
            set
            {
                _todos = value;
                RaisePropertyChanged("Todos");
            }
        }

        public class UserTodosDataLoader : FourSquareDataLoaderBase<LoadContext>
        {

            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                // TODO: CONSIDER: Expose sorting properly through a load context instead.
                // LOCALIZE:
                string sortType = LocationAssistant.Instance.LastKnownLocation != null ? "nearby" : "recent";

                var id = (string)context.Identity;
                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        "users/" + id + "/todos",

                        GeoMethodType.Optional,

                        "sort",
                        sortType
                        ));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var b = new UserTodos(context);

                    var sets = json["todos"];
                    List<Todo> todos = new List<Todo>();
                    if (sets != null)
                    {
                        var items = sets["items"];
                        if (items != null)
                            foreach (var todo in items)
                            {
                                var otodo = Todo.ParseJson(todo);
                                if (otodo != null)
                                {
                                    todos.Add(otodo);
                                }
                            }
                    }
                    b.Todos = todos;

                    b.IsLoadComplete = true;

                    return b;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        // LOCALIZE:
                        "There was a problem trying to read the list of todos.", e);
                }
            }
        }
    }
}
