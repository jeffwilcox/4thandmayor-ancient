using System;
using System.Globalization;
using AgFx;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class CompactList :  ISpecializedComparisonString
    {
        /// <summary>
        /// Gets or sets a unique identifier for this list.
        /// </summary>
        public string Id { get; set;}
        public string Name { get; set;}
        public string Description { get; set;}
        public CompactUser User { get; set; }
        public bool IsFollowing { get; set;}
        // TODO V4: Lists, "followers" property.
        // count and items of users who following this list. All items may not 
        // be present.
        public bool IsEditable { get; set;}
        public bool IsCollaborative { get; set;}
        public Uri CaonicalUri { get; set;}
        public Photo Photo { get; set;}
        public int DoneCount { get; set;}
        public int VenueCount { get; set;}
        public int VisitedCount { get; set;}
        public Uri LocalListUri { get; set; }

        public List LoadListInstance()
        {
            if (Id != null)
            {
                return DataManager.Current.Load<List>(new LoadContext(Id));
            }

            return null;
        }

        public static CompactList ParseJson(JToken list)
        {
            CompactList l = new CompactList();

            l.Id = Json.TryGetJsonProperty(list, "id");

            l.Name = Json.TryGetJsonProperty(list, "name");
            l.Description = Json.TryGetJsonProperty(list, "description");

            Uri uri = null;
            Uri.TryCreate(string.Format(
                CultureInfo.InvariantCulture,
                "/JeffWilcox.FourthAndMayor.Lists;component/ListView.xaml?id={0}&name={1}&description={2}",
                l.Id,
                l.Name,
                l.Description), UriKind.Relative, out uri);
            l.LocalListUri = uri;

            var cu = list["user"];
            if (cu != null)
            {
                var ocu = CompactUser.ParseJson(cu);
                if (ocu != null)
                {
                    l.User = ocu;
                }
            }

            l.IsFollowing = Json.TryGetJsonBool(list, "following");

            // TODO: v4: "followers"

            l.IsEditable = Json.TryGetJsonBool(list, "editable");
            l.IsCollaborative = Json.TryGetJsonBool(list, "collaborative");

            // TODO: v4: "collaborators"

            l.CaonicalUri = Json.TryGetUriProperty(list, "caonicalUrl");

            var pic = list["photo"];
            if (pic != null)
            {
                var opic = Photo.ParseJson(pic);
                if (opic != null)
                {
                    l.Photo = opic;
                }
            }

            string s = Json.TryGetJsonProperty(list, "doneCount");
            int i;
            if (int.TryParse(s, out i))
            {
                l.DoneCount = i;
            }

            s = Json.TryGetJsonProperty(list, "venueCount");
            if (int.TryParse(s, out i))
            {
                l.VenueCount = i;
            }

            s = Json.TryGetJsonProperty(list, "visitedCount");
            if (int.TryParse(s, out i))
            {
                l.VisitedCount = i;
            }

            //var b = new List<CompactListItem>();
            //var lis = list["listItems"];
            //if (lis != null)
            //{
            //    var items = lis["items"];
            //    if (items != null)
            //    {
            //        foreach (var entry in items)
            //        {
            //            var u = CompactListItem.ParseJson(entry);
            //            if (u != null)
            //            {
            //                b.Add(u);
            //            }
            //        }
            //    }
            //}

            // l.ListItems = b;

            return l;
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
