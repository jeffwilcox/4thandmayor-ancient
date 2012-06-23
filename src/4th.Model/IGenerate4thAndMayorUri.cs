using System;

namespace JeffWilcox.FourthAndMayor
{
    public interface IGenerate4thAndMayorUri
    {
        Uri Get4thAndMayorUri(string path, bool isSecure);
    }
}
