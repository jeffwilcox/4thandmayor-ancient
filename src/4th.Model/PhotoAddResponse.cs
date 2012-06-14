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

#if DEAD_CODE

using System;
using System.Globalization;
using AgFx;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    [CachePolicy(CachePolicy.ValidCacheOnly, CacheTimeInSeconds = 60 * 60 * 24 * 14)] // support 2 weeks of tombstoning
    public class PhotoAddResponse : FourSquareItemBase<PhotoAddLoadContext>
    {
        public PhotoAddResponse(PhotoAddLoadContext loadContext)
            : base(loadContext)
        {
        }

        public PhotoAddResponse()
        {
        }

        private Photo _photo;
        public Photo Photo
        {
            get
            {
                return _photo;
            }

            set
            {
                _photo = value;
                RaisePropertyChanged("Photo");
            }
        }

        public class PhotoAddResponseDataLoader : FourSquareDataLoaderBase<PhotoAddLoadContext>
        {
            public override LoadRequest GetLoadRequest(PhotoAddLoadContext loadContext, Type objectType)
            {
                if (LocalCredentials.Current != null && string.IsNullOrEmpty(LocalCredentials.Current.UserId))
                {
                    throw new UserIgnoreException();
                }

                var @params = loadContext.GetMultipartFormParameters();

                var la = LocationAssistant.Instance.LastKnownLocation;
                // TODO: Centralize the dictionary filling of this between this method.
                if (la != null && !double.IsNaN(la.Latitude) && !double.IsNaN((la.Longitude)))
                {
                    @params["ll"] = la.Latitude.ToString(CultureInfo.InvariantCulture)
                        + "," + la.Longitude.ToString(CultureInfo.InvariantCulture);
                    if (!double.IsNaN(la.HorizontalAccuracy))
                    {
                        @params["llAcc"] = la.HorizontalAccuracy.ToString(CultureInfo.InvariantCulture);
                    }
                    if (!double.IsNaN(la.VerticalAccuracy) && la.VerticalAccuracy != 0.0 && !double.IsNaN(la.Altitude))
                    {
                        @params["altAcc"] = la.VerticalAccuracy.ToString(CultureInfo.InvariantCulture);
                        @params["alt"] = la.Altitude.ToString(CultureInfo.InvariantCulture);
                    }
                }

                return BuildPostRequest(
                    loadContext,
                    FourSquareWebClient.BuildFourSquareUri(
                        "photos/add", 
                        GeoMethodType.None), 
                    @params, //                        GeoMethodType.Optional,
                    loadContext.GetPhotoBytes());
            }

            protected override object DeserializeCore(JObject json, Type objectType, PhotoAddLoadContext context)
            {
                try
                {
                    var data = new PhotoAddResponse((PhotoAddLoadContext)context);

                    var p = json["photo"];
                    if (p != null)
                    {
                        Photo photo = Photo.ParseJson(p);
                        if (photo != null)
                        {
                            data.Photo = photo;
                        }
                    }

                    data.IsLoadComplete = true;
                    return data;
                }
                catch (Exception e)
                {
                    throw new UserIntendedException(
                        "There was a problem trying to add the photo, please try again later.", e);
                }
            }
        }
    }
}

#endif
