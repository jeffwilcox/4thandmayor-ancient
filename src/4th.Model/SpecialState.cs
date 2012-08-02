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


namespace JeffWilcox.FourthAndMayor.Model
{
    /*
Possible values: 


unlocked

the special is unlocked (all types)



before start

the time after which the special may be unlocked is in the future (flash specials)



in progress

 the special is locked but could be unlocked if you check in (flash specials), or the special is locked but could be unlocked if enough of your friends check in (friends specials)



taken

the maximum number of people have unlocked the special for the day (flash and swarm specials)



locked

the special is locked (all other types)

     * */
    public enum SpecialState
    {
        Unlocked,
        BeforeStart,
        InProgress,
        Taken,
        Locked,
    }
}
