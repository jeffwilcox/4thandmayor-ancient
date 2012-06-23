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
    [CachePolicy(CachePolicy.CacheThenRefresh, 60 * 60 * 24 * 2)] // 2 days
    public class Categories : FourSquareItemBase<LoadContext>
    {
        public Categories() : base()
        {
        }

        public Categories(LoadContext context)
            : base(context)
        {
        }

        private List<Category> _rootCategories;
        public List<Category> RootCategories
        {
            get { return _rootCategories; }
            set {
                _rootCategories = value;
                RaisePropertyChanged("RootCategories");
            }
        }

        public bool TryGetCategory(string categoryId, out Category category)
        {
            if (RootCategories != null)
            {
                foreach (var cat in RootCategories)
                {
                    var c = TryGetInternal(cat, categoryId);
                    if (c != null)
                    {
                        category = c;
                        return true;
                    }
                }
            }

            category = null;
            return false;
        }

        private Category TryGetInternal(Category cat, string categoryId)
        {
            if (cat.CategoryId == categoryId)
            {
                return cat;
            }
            if (cat.SubCategories != null)
            {
                foreach (var sc in cat.SubCategories)
                {
                    var p = TryGetInternal(sc, categoryId);
                    if (p != null)
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        public class FlatCategory
        {
            public Category ActualCategory { get; set; }
            public int Depth { get; set; }
            public override string ToString()
            {
                return ActualCategory != null ? ActualCategory.PluralName : base.ToString();
            }
        }

        [DependentOnProperty("RootCategories")]
        public List<FlatCategory> FlatCategoriesList
        {
            get
            {
                List<FlatCategory> flat = new List<FlatCategory>();
                if (_rootCategories != null)
                {
                    foreach (var cat in _rootCategories)
                    {
                        foreach (var fc in FlattenCategory(cat, 0))
                        {
                            flat.Add(fc);
                        }
                    }
                }
                return flat;
            }
        }

        private IEnumerable<FlatCategory> FlattenCategory(Category c, int depth)
        {
            yield return new FlatCategory
            {
                ActualCategory = c,
                Depth = depth,
            };
            if (c != null && c.SubCategories != null)
            {
                foreach (Category cat in c.SubCategories)
                {
                    /*yield return new FlatCategory
                    {
                        ActualCategory = cat,
                        Depth = depth,
                    };*/
                    foreach (var fc in FlattenCategory(cat, depth + 1))
                    {
                        yield return fc;
                    }
                }
            }
        }

        public class CategoriesDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                //var id = (string)context.Identity;
                string method = "venues/categories"; // +id;

                return BuildRequest(
                    context,
                    FourSquareWebClient.BuildFourSquareUri(
                        method,
                        GeoMethodType.None));
            }

            protected override object DeserializeCore(JObject json, Type objectType, LoadContext context)
            {
                try
                {
                    var u = new Categories(context);
                    //u.IgnoreRaisingPropertyChanges = true;

                    var jc = json["categories"];
                    if (jc != null)
                    {
                        List<Category> cats = new List<Category>();
                        foreach (var j in jc)
                        {
                            if (j != null)
                            {
                                var cat = Category.ParseJson(j);
                                if (cat != null)
                                {
                                    cats.Add(cat);
                                }
                            }
                        }
                        if (cats.Count > 0)
                        {
                            u.RootCategories = cats;
                        }
                    }

                    //u.IgnoreRaisingPropertyChanges = false;
                    u.IsLoadComplete = true;

                    return u;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read the categories list.", e);
                }
            }
        }
    }
}
