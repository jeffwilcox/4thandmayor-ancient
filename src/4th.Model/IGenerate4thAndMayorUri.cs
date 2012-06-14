using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace JeffWilcox.FourthAndMayor
{
    public interface IGenerate4thAndMayorUri
    {
        Uri Get4thAndMayorUri(string path, bool isSecure);
    }
}
