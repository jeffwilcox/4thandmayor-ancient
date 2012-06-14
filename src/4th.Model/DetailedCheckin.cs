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
using System.Diagnostics;
using System.Linq;
using System.IO;
using AgFx;
using Newtonsoft.Json.Linq;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.CacheThenRefresh,
#if DEBUG
 DebugIntervalMultiplier *
#endif
 StandardRefreshInterval)]
    public class DetailedCheckin : FourSquareItemBase<LoadContext>
    {
        public DetailedCheckin() : base()
        {
        }

        public DetailedCheckin(LoadContext context)
            : base(context)
        {
        }

        private List<object> _commentsAndPhotos;
        public List<object> CommentsAndPhotos
        {
            get { return _commentsAndPhotos; }
            set { 
                _commentsAndPhotos = value;
                RaisePropertyChanged("CommentsAndPhotos");
            }
        }

        private Checkin _checkin;
        public Checkin CompactCheckin
        {
            get { return _checkin; }
            set
            {
                _checkin = value;
                RaisePropertyChanged("CompactCheckin");
            }
        }

        public class DetailedCheckinDataLoader : FourSquareDataLoaderBase<LoadContext>
        {
            public override LoadRequest GetLoadRequest(LoadContext context, Type objectType)
            {
                var id = (string)context.Identity;
                string method = "checkins/" + id;

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
                    var u = new DetailedCheckin(context);
                    //u.IgnoreRaisingPropertyChanges = true;

                    var jcheckin = json["checkin"];
                    if (jcheckin != null)
                    {
                        Checkin compactCheckin = Checkin.ParseJson(jcheckin);

                        // So links are not displayed, etc.
                        if (compactCheckin != null)
                        {
                            compactCheckin.ReduceFunctionality = true;
                            compactCheckin.ReduceFunctionalityVis = System.Windows.Visibility.Visible;
                            compactCheckin.CompleteFunctionalityVis = System.Windows.Visibility.Collapsed;
                        }

                        u.CompactCheckin = compactCheckin;

                        List<Photo> photos = new List<Photo>();
                        var pl = jcheckin["photos"];
                        if (pl != null)
                        {
                            var pll = pl["items"];
                            if (pll != null)
                            {
                                foreach (var photo in pll)
                                {
                                    var po = Photo.ParseJson(photo);
                                    if (po != null)
                                    {
                                        photos.Add(po);
                                    }
                                }
                            }
                        }

                        List<Comment> comments = new List<Comment>();
                        var cl = jcheckin["comments"];
                        if (cl != null)
                        {
                            var cll = cl["items"];
                            if (cll != null)
                            {
                                foreach (var comment in cll)
                                {
                                    var co = Comment.ParseJson(comment);
                                    if (co != null)
                                    {
                                        comments.Add(co);
                                    }
                                }
                            }
                        }

                        //var list = combined.OrderBy(w => w.CreatedDateTime).ToList();
                        //cap.AddRange(list);

                        List<object> both = new List<object>(photos.Cast<object>());
                        both.AddRange(comments.Cast<object>());

                        //List<CommentsList> lcl = null;
                        //if (cap.Count > 0)
                        //{
                        //    lcl = new List<CommentsList>();
                        //    lcl.Add(cap);
                        //}

                        u.CommentsAndPhotos = both;
                    }

                    u.IgnoreRaisingPropertyChanges = false;
                    u.IsLoadComplete = true;

                    return u;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to read information about the checkin.", e);
                }
            }
        }
    }
}
