using Android.Content.Res;
using Android.Telephony.Data;
using Android.Webkit;
using Java.Net;
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
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Content.ClipData;

namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TripAssignPage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<TripModel> Triplists { get; set; }

        public IList<GradeModel> GradeList { get; set; }
        readonly ObservableCollection<string> vehicleData = new ObservableCollection<string>();
        UserModel LoginData = new UserModel();
        
        public IList<VehicleLockModel> VehicleList { get; set; }
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;
        public TripAssignPage()
        {
            InitializeComponent();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            GetVehicle();
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TeaCollection.db3");
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalClientSaveModel>();
            conn.CreateTable<LocalGradeSaveModel>();
            conn.CreateTable<SaveLocalCollectionModel>();
            
           DropAndRecreateTable();
            GetClient();
            GetGrade();
            CheckLockVehicle();
        }

     
        private async void OnLogoutTapped(object sender, EventArgs e)

        {
            if (sender is Image image)
            {

                string sourcePath = image.Source.ToString();

                switch (sourcePath)
                {
                        case "File: logout.png":
                        SessionManager.ClearSession();
                        await Navigation.PushAsync(new UserModule.UserLoginPage());
                        break;
                    default:
                        return;
                }
            }
        }


        public async void GetVehicle()
        {

            string url = _appSetting.ApiUrl + "Master/GetVehicle";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
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
                        vehicleData.Add(vehicle.VehicleNo);

                    }

                }

            }


        }
        private void ListView_OnItemTapped(Object sender, ItemTappedEventArgs e)
        {
            //EmployeeListView.IsVisible = false;  

            String listsd = e.Item as string;
            VehicleNo.Text = listsd;
            VehicleListView.IsVisible = false;

            ((ListView)sender).SelectedItem = null;
        }
        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            VehicleListView.IsVisible = true;
            VehicleListView.BeginRefresh();

            try
            {
                var dataEmpty = vehicleData.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));

                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                    VehicleListView.IsVisible = false;
                else if (dataEmpty.Max().Length == 0)
                    VehicleListView.IsVisible = false;
                else
                    VehicleListView.ItemsSource = vehicleData.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
                VehicleListView.IsVisible = false;

            }
            VehicleListView.EndRefresh();

        }

        private async void OnLockClicked(object sender, EventArgs e)
        {
            string _vehicle = VehicleNo.Text;
            if (string.IsNullOrWhiteSpace(_vehicle))
            {
                await DisplayAlert("Validation", "Please Enter Vehicle", "OK");
                return;
            }
            bool _lock = await DisplayAlert("Lock", "This vehicle is locked for "+ DateTime.Today.ToString("dd/MM/yyyy"), "Yes", "No");
            if (_lock)
            {
                var selectedTrip = (TripModel)Trip.SelectedItem;
                await SaveVehicleLockGlobally(selectedTrip.TripId);
                await Navigation.PushAsync(new UserModule.CollectionPage(VehicleNo.Text,Convert.ToInt16(selectedTrip.TripId)));
            }
        }

        private async Task SaveVehicleLockGlobally(int _TripId)
        {
            string url = _appSetting.ApiUrl + "Collection/VehicleLockSaveMobile";

            string _vehicle = VehicleNo.Text;
            if (string.IsNullOrWhiteSpace(_vehicle))
            {
                await DisplayAlert("Validation", "Please Enter Vehicle", "OK");
                return;
            }
            SaveVehicleLockModel dataToSend = new SaveVehicleLockModel
            {

                LockDate = DateTime.Now.ToString("yyyy-MM-dd"),
                VehicleNo = VehicleNo.Text,
                TripId = _TripId,
                TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId)
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var valueData = JsonConvert.DeserializeObject<SaveReturn>(results);

                    if (valueData.Id != 0)
                    {


                        await DisplayAlert("Success", valueData.Message, "OK");

                    }
                    else
                    {
                        await DisplayAlert("Info", valueData.Message, "OK");
                        ;
                    }

                }

            }
        }

        public async void GetClient()
        {

            string url = _appSetting.ApiUrl + "Master/GetClientList";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                Category = "STG",
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
                    var valueData = JsonConvert.DeserializeObject<ClientList>(results);
                    foreach (var _client in valueData.ClientDetails)
                    {
                        SaveClientLocally(_client.ClientId, _client.ClientName);
                        
                    }

                }

            }

            


        }

        void SaveClientLocally(int ClientId, string ClientName)
        {
            LocalClientSaveModel _client = new LocalClientSaveModel();
            _client.ClientId = ClientId;
            _client.ClientName = ClientName;
            _client.CreatedDate = DateTime.Now;
            try
            {
                conn.Insert(_client);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        void SaveGradeLocally(int GradeId, string GradeName)
        {
            LocalGradeSaveModel _grade = new LocalGradeSaveModel();
            _grade.GradeId = GradeId;
            _grade.GradeName = GradeName;
            _grade.CreatedDate = DateTime.Now;
            try
            {
                conn.Insert(_grade);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        // Drop and recreate the table
        private void DropAndRecreateTable()
        {
            conn.Execute("DROP TABLE IF EXISTS LocalClientSaveModel");
            conn.CreateTable<LocalClientSaveModel>();

            conn.Execute("DROP TABLE IF EXISTS LocalGradeSaveModel");
            conn.CreateTable<LocalGradeSaveModel>();

            conn.Execute("DROP TABLE IF EXISTS SaveLocalCollectionModel");
            conn.CreateTable<SaveLocalCollectionModel>();

        }
        void DeleteClientData()
        {
            conn.DeleteAll<LocalClientSaveModel>();
        }

        public async void GetGrade()
        {
            try
            {
                string url = _appSetting.ApiUrl + "Master/GetGrade";

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
                        var data = JsonConvert.DeserializeObject<GradeList>(results);
                        foreach (var _grade in data.GradeDetails)
                        {
                            SaveGradeLocally(_grade.GradeId, _grade.GradeName);
                           // GradeList.Add(new GradeModel { GradeId = _grade.GradeId, GradeName = _grade.GradeName });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {

            }


        }

        public async void CheckLockVehicle()
        {
            try
            {
                string url = _appSetting.ApiUrl + "Collection/GetVehicleLockDetails";

                var dataToSend = new
                {
                    VehicleNo= "",
                    TripId= 0,
                    LockDate= DateTime.Now.ToString("yyyy-MM-dd"),
                    CreatedBy= Convert.ToInt32(LoginData.LoginDetails[0].UserId),
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
                        var data = JsonConvert.DeserializeObject<VehicleLockList>(results);
                        foreach (var _vehicle in data.LockDetails)
                        {
          
                           // VehicleList.Add(new VehicleLockModel { VehicleId = _vehicle.VehicleId, TripId = _vehicle.TripId });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //  throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");
            }

            finally
            {

            }
        }
        }
}