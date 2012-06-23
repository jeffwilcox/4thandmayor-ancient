using AgFx;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class ProfileBadges : PhoneApplicationPage
    {
        public ProfileBadges()
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

            var user = DataManager.Current.Load<Model.User>(id);
            DataContext = user;
        }
    }
}