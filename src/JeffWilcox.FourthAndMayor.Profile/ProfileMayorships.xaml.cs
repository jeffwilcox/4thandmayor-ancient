using AgFx;
using Microsoft.Phone.Controls;

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