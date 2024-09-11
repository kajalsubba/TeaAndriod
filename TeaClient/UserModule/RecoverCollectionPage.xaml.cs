using Android.Telephony.Data;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.LocalDataShow;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecoverCollectionPage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<TripModel> Triplists { get; set; }

        readonly ObservableCollection<VehicleModel> vehicleData = new ObservableCollection<VehicleModel>();

        UserModel LoginData = new UserModel();

        long VehicleFromId;
        public IList<VehicleLockModel> VehicleList { get; set; }
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;
        public RecoverCollectionPage()
        {
            InitializeComponent();
            VehicleList = new ObservableCollection<VehicleLockModel>();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            GetVehicle();
            //var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TeaCollection.db3");
            //conn = DependencyService.Get<ISqlLite>().GetConnection();
            //conn.CreateTable<LocalClientSaveModel>();
            //conn.CreateTable<LocalGradeSaveModel>();
            //conn.CreateTable<SaveLocalCollectionModel>();

        
        }
        public async void GetVehicle()
        {

            string url = _appSetting.ApiUrl + "Master/GetVehicle";

            var dataToSend = new
            {
                IsClientView = true,
                TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId)
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var valueData = JsonConvert.DeserializeObject<VehicleList>(results);
                    foreach (var vehicle in valueData.VehicleDetails)
                    {
                        vehicleData.Add(new VehicleModel { VehicleId = vehicle.VehicleId, VehicleNo = vehicle.VehicleNo });

                    }

                }

            }


        }
        private async void ListView_OnItemTapped(Object sender, ItemTappedEventArgs e)
        {
            try
            {
                VehicleModel listsd = e.Item as VehicleModel;
                VehicleNo.Text = listsd.VehicleNo;
                VehicleFromId = listsd.VehicleId;
                VehicleListView.IsVisible = false;

                ((ListView)sender).SelectedItem = null;
            }
            catch (Exception ex)
            {
                 await DisplayAlert("Error", ex.Message, "OK");

            }
        }
        private async void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            VehicleListView.IsVisible = true;
            VehicleListView.BeginRefresh();

            try
            {

                var dataEmpty = vehicleData.Where(i => i.VehicleNo.ToLower().Contains(e.NewTextValue.ToLower())).ToList();

                if (string.IsNullOrWhiteSpace(e.NewTextValue) || dataEmpty.Count == 0)
                {
                    VehicleListView.IsVisible = false;
                }
                else
                {
                    // If there are results, show the ListView and bind the filtered data
                    VehicleListView.IsVisible = true;
                    VehicleListView.ItemsSource = dataEmpty;
                }

            }
            catch (Exception ex)
            {
                VehicleListView.IsVisible = false;
                await DisplayAlert("Error", ex.Message, "Ok");


            }
            VehicleListView.EndRefresh();

        }

        private async void OnLockClicked(object sender, EventArgs e)
        {
            try
            {
                DateTime selectedDate = collectionDate.Date;

                string _vehicle = VehicleNo.Text;
                if (string.IsNullOrWhiteSpace(_vehicle))
                {
                    await DisplayAlert("Validation", "Please Enter Vehicle", "OK");
                    return;
                }
                var selectedTrip = (TripModel)Trip.SelectedItem;
                await Navigation.PushAsync(new LocalDataShow.RecoveryCollectionView(VehicleNo.Text, Convert.ToInt16(selectedTrip.TripId), VehicleFromId, selectedDate));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");

            }
        }

    }
}