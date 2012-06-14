using System;
using System.Globalization;
using System.Windows.Controls;
using AgFx;
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class ProfileMostExploredCategories : PhoneApplicationPage
    {
        public ProfileMostExploredCategories()
        {
            InitializeComponent();
        }

        private string _id;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _id = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("id", out _id))
            {
                _id = "self";
            }

            var stats = DataManager.Current.Load<Model.User>(_id);
            DataContext = stats;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var user = DataContext as Model.User;
            if (user != null && user.IsSelf == false)
            {
                // for privacy reasons...
                return;
            }

            var item = ((Button) sender).Tag as UserCategoryStatistic;
            if (item != null)
            {
                NavigationService.Navigate(
                    new Uri(
                        string.Format(CultureInfo.InvariantCulture,
                                      "/JeffWilcox.FourthAndMayor.Profile;component/ProfileMostExploredCategory.xaml?categoryid={0}&id={1}&categoryname={2}",
                                      item.Category.CategoryId,
                                      _id,
                                      System.Net.HttpUtility.UrlEncode(item.Category.PluralName)
                                      ), UriKind.Relative));
            }
        }
    }
}