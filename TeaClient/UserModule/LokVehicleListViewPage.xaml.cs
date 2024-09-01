using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.UserModule
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LokVehicleListViewPage : ContentPage
	{
		IList<VehicleLockModel> _VehicleList;

        public LokVehicleListViewPage (IList<VehicleLockModel> VehicleList)
		{
			InitializeComponent ();
			_VehicleList = VehicleList;
            DisplayDetails();
            HeaderName.Text ="Vehicle Lock On :"+ DateTime.Now.ToString("dd/MM/yyyy");
        }
        protected override bool OnBackButtonPressed()
        {
            //  DisplayConfirmation();
            return true; // Do not continue processing the back button
        }
        public void DisplayDetails()
        {

            var CollectionDetails = _VehicleList;
            LockVehicleListView.ItemsSource = CollectionDetails;
        }
        private async void OnSelectButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the button that was clicked
                Button button = sender as Button;

                // Get the item associated with the button
                var item = button?.CommandParameter as VehicleLockModel;

                // Handle the edit action (e.g., navigate to an edit page or display a form)
                if (item != null)
                {
                    await Navigation.PushAsync(new UserModule.CollectionPage(item.VehicleNo, Convert.ToInt16(item.TripId),item.VehicleId));
                }
            }
            catch (Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }

    }
}