using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeffWilcox.Controls
{
    public interface IExposeSettings
    {
        ILocationEnabled LocationEnabled { get; }
        IAreImperialUnitsInUse ImperialUnitsInUse { get; }
        IPushEnabled PushEnabled { get; }
    }

    // hacks

    public interface IPushEnabled
    {
        bool PushEnabled { get; set;  }
    }

    public interface IAreImperialUnitsInUse
    {
        bool AreImperialUnitsInUse { get; }
    }
}
