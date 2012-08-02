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

namespace JeffWilcox.FourthAndMayor
{
    // TODO: REFACTOR to remove the Foursquare client/key/other values from
    //this.

    /// <summary>
    /// Exposes simple information used by my app. Need to refactor.
    /// </summary>
    public interface IAppInfo
    {
        string Version { get; }
        string BKey { get; }
        string FClient { get; }
        string FSecret { get; }
    }
}
