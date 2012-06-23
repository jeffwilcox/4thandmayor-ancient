using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Place
{
    public partial class AddVenueDuplicates : PhoneApplicationPage
    {
        public AddVenueDuplicates()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (Views.AddVenue.CurrentChallenge != null)
            {
                var challenge = Views.AddVenue.CurrentChallenge;
                Views.AddVenue.CurrentChallenge = null;

                NavigationService.RemoveBackEntry(); // remove the previous pg.

                DataContext = challenge;
            }
            else
            {
                // No action for now. This is horrible code.
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FourSquare.DuplicateVenueChallenge challenge = DataContext as FourSquare.DuplicateVenueChallenge;
            if (challenge != null)
            {
                FourSquare.Instance.AddVenue(challenge.OriginalName, 
                    challenge.OriginalDataPairs, OnVenueCreated, OnDuplicatesChallenge, OnVenueCreateFailure);

            }
        }

        private void OnVenueCreated(Model.CompactVenue venue)
        {
            if (venue != null)
            {
                FourSquare.Instance.VenueCreationSuccessfulId = venue.VenueId;
            }
            else
            {
                FourSquare.Instance.VenueCreationSuccessfulId = string.Empty; // still move back I suppose.
            }

            Dispatcher.BeginInvoke(() => {
                NavigationService.RemoveBackEntry();
                NavigationService.GoBack();
            });
        }

        private void OnDuplicatesChallenge(FourSquare.DuplicateVenueChallenge dvc)
        {
            OnVenueCreateFailure(null);
        }

        private void OnVenueCreateFailure(Exception e)
        {
            Dispatcher.BeginInvoke(
                () => MessageBox.Show("The venue could not be created."));
        }
    }
}