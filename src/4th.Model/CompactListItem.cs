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
using System.Globalization;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class CompactListItem : ISpecializedComparisonString
    {
        // -=------------------------------------------------------------------

        // NOTE: THIS FIELD IS NOT SET IN THE RESPONSE, BUT INSTEAD IS THE
        // CONTEXTUAL VALUE. ANYONE PARSING A COMPACT LIST ITEM SHOULD 
        // ASSOCIATE THIS ITEM!!!!!!!
        public string ListId { get; set; }

        // -=------------------------------------------------------------------

        // NOTE: IDs are only valid within the context of a list.
        public string Id { get; set; }

        /// <summary>
        /// [optional] Gets a compact tip if this item contains a tip.
        /// </summary>
        public Tip Tip { get; set; }

        public Photo Photo { get; set; }

        /// <summary>
        /// [optional] The compact venue this item is for, unless the venue is
        /// clear from context.
        /// </summary>
        public CompactVenue Venue { get; set; }

        /// <summary>
        /// [optional] The compact user who added this item to the current 
        /// list. Only present when viewing a specific list and when the user
        /// is not the same as the list owner.
        /// </summary>
        public CompactUser User { get; set; }

        /// <summary>
        /// Optional text entered by the user when creating this item. This 
        /// field is private and only returned to the author.
        /// </summary>
        public string Note { get; set; }

        public Uri LocalListItemUri
        {
            get
            {
                return new Uri(string.Format(CultureInfo.InvariantCulture,
                    "/JeffWilcox.FourthAndMayor.Lists;component/ListItem.xaml?id={0}&list={2}&venueName={1}",

                    Id,
                    (Venue != null && Venue.Name != null ) ? Uri.EscapeDataString(Venue.Name) : string.Empty,
                    ListId)

                    , UriKind.Relative);
            }
        }

        /// <summary>
        /// Seconds since epoch when this item was added to the list.
        /// </summary>
        public string CreatedAt { get; set; }

        public DateTime Created { get; set; }

        /// <summary>
        /// Gets a value indicating whether the acting user wants to do this
        /// item. Absent if there is no acting user.
        /// </summary>
        public bool Todo { get; set; }

        /// <summary>
        /// Gets a value indicating whether the active user has done this item.
        /// Done state should be treated carefully since it may reflect 
        /// off-the-grid checkins. The done state is repeated in the tip if
        /// present. Absent if there is no acting user.
        /// </summary>
        public bool IsDone { get; set; }

        /// <summary>
        /// An integer indicating the number of times the acting user has 
        /// visited the item's venue. Absent if there is no acting user.
        /// </summary>
        public int VisitedCount { get; set; }

        /// <summary>
        /// A list of information about what other lists this item appears on.
        /// </summary>
        //public List<CompactList> Listed { get; set; }
        // TODO: v4: compact list

        public override string ToString()
        {
            return Id;
        }

        // specialized comparison string getter?

        public static CompactListItem CreateTipFaçade(Tip tip)
        {
            if (tip != null)
            {
                CompactListItem c = new CompactListItem();

                c.Created = tip.CreatedDateTime;
                //c.CreatedAt
                c.Id = tip.TipId;

                c.IsDone = tip.Status == TipStatus.Done;
                c.Todo = tip.Status == TipStatus.Todo;

                c.ListId = null;
                // ? c.LocalListItemUri
                c.Note = null;
                c.Photo = tip.Photo;
                c.Tip = tip;

                c.User = tip.User;
                c.Venue = tip.Venue;
                // c.VisitedCount

                return c;
            }

            return null;
        }

        public static CompactListItem ParseJson(JToken json)
        {
            CompactListItem c = new CompactListItem();
            c.Id = Json.TryGetJsonProperty(json, "id");

            string created = Json.TryGetJsonProperty(json, "createdAt");
            if (created != null)
            {
                DateTime dtc = UnixDate.ToDateTime(created);
                c.CreatedAt = Checkin.GetDateString(dtc);
                c.Created = dtc;
            }

            var user = json["user"];
            if (user != null)
            {
                c.User = CompactUser.ParseJson(user);
            }

            var photo = json["photo"];
            if (photo != null)
            {
                c.Photo = Photo.ParseJson(photo);
            }

            var venue = json["venue"];
            if (venue != null)
            {
                c.Venue = CompactVenue.ParseJson(venue);
            }

            var tip = json["tip"];
            if (tip != null)
            {
                c.Tip = Tip.ParseJson(tip);
            }

            var note = json["note"];
            if (note != null)
            {
                c.Note = Json.TryGetJsonProperty(note, "text");
            }

            c.Todo = Json.TryGetJsonBool(json, "todo");
            c.IsDone = Json.TryGetJsonBool(json, "done");

            string s = Json.TryGetJsonProperty(json, "visitedCount");
            if (s != null)
            {
                int i;
                if (int.TryParse(s, out i))
                {
                    c.VisitedCount = i;
                }
            }

            // TODO: V4: "listed" list of compact venues where the item appears on.

            return c;
        }

        public string SpecializedComparisonString
        {
            get {
                return Id; } // ?? new Guid().ToString(); }
        }
    }
}