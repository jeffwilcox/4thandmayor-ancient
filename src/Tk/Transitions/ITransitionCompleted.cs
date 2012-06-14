using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Jeff Wilcox custom code.

namespace Microsoft.Phone.Controls
{
    public interface ITransitionCompleted
    {
        void OnTransitionCompleted();
        void OnTransitionGoodbyeTemporary();
    }
}
