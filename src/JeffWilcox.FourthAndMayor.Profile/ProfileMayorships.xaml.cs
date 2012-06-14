using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using AgFx;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class ProfileMayorships : PhoneApplicationPage
    {
        public ProfileMayorships()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("id", out id))
            {
                id = "self";
            }

            var user = DataManager.Current.Load<Model.UserMayorships>(id);
            DataContext = user;
        }
    }
}