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

namespace JeffWilcox.FourthAndMayor.Model
{
    public enum FriendStatus
    {
        ValueNotYetLoaded, // internal to this app only,

        None, // the requested user is not your friend (and neither party has made an attempt at connecting)
        Self, // it's you!
        Friend, // the requested user is your friend
        PendingYou, // the requested user sent you a friend request that you have not accepted
        PendingThem, // you have sent a friend request to the requested user but they have not accepted
    }
}