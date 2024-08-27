using Android.Webkit;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using TeaClient.UserModule;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.CommonPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferPopUpPage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        IList<VehicleLockModel> VehicleList { get; set; }
        readonly string VehicleNo = string.Empty;
        readonly int TripId;
        public SQLiteConnection conn;
        public SaveLocalCollectionModel _CollectionModel;
        IList<SaveLocalCollectionModel> CollectionList;
        UserModel LoginData = new UserModel();
        public TransferPopUpPage(string _VehicleNo, int _Trip,IList<SaveLocalCollectionModel> _CollectionList)
        {
            InitializeComponent();
          
            VehicleList = new ObservableCollection<VehicleLockModel>();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            VehicleNo = _VehicleNo;
            TripId = _Trip;
            CollectionList = _CollectionList;
            GetLockVehicle();
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<SaveLocalCollectionModel>();
            VehicleListView.ItemsSource = VehicleList;
        }

      
        public async void GetLockVehicle()
        {
            try
            {
                string url = _appSetting.ApiUrl + "Collection/GetLockedVehicleList";

                var dataToSend = new
                {
                    LockDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                    CreatedBy= Convert.ToInt32(LoginData.LoginDetails[0].UserId)
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
                        if (data.LockDetails.Count > 0)
                        {
                            foreach (var _vehicle in data.LockDetails)
                            {

                                VehicleList.Add(new VehicleLockModel { VehicleId = _vehicle.VehicleId, VehicleNo = _vehicle.VehicleNo, LoginUserName = _vehicle.LoginUserName, TripName = _vehicle.TripName,TripId=_vehicle.TripId, TransferTo =_vehicle.TransferTo });
                            }
                        }
                       else
                        {
                            await DisplayAlert("Info", "No vehicle is in Field. ", "Ok");
                            await Navigation.PopModalAsync();
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
        private async void OnTransferButtonClicked(object sender, EventArgs e)
        {

            try
            {
           
                Button button = sender as Button;
                var item = button?.CommandParameter as VehicleLockModel;
                if (item != null)
                {
                    if (CollectionList.Count > 0)
                    {
                        await TransferSTGUsingMQ(item.TransferTo, item.VehicleNo, CollectionList[0].VehicleFrom, CollectionList[0].TransferFrom);
                    }
                    else
                    {
                        await DisplayAlert("Info","No data for transfer!", "Ok");
                    }
                }
            }
            catch (Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }
        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            try
            {
                OnDismissed?.Invoke();
                await Navigation.PopModalAsync();
               
            }
            catch (Exception ex)
            {
               await DisplayAlert("Error",ex.Message, "Ok");
            }
        }

        public async Task TransferSTGUsingMQ(long TransferTo,string VehicleTo,long _VehicleFrom,long _TransferFrom)
        {
            try
            {

              
                List<TransferStgDataModel> _stgData = new List<TransferStgDataModel>();

                foreach (SaveLocalCollectionModel item in CollectionList)
                {
                    var objStg = new TransferStgDataModel
                    {
                        CollectionDate = item.CollectionDate,
                        TripId = item.TripId,
                        ClientId = item.ClientId,
                        FirstWeight = item.FirstWeight,
                        WetLeaf = item.WetLeaf,
                        LongLeaf = item.LongLeaf,
                        Deduction = item.Deduction,
                        FinalWeight = item.FinalWeight,
                        Rate = item.Rate,
                        GradeId = item.GradeId,
                        Remarks = item.Remarks,
                        BagList = item.BagList,
                        CollectionType="Transfer",
                        VehicleFrom= _VehicleFrom,
                        TransferFrom=_TransferFrom
                    };
                    _stgData.Add(objStg);
                }

                string url = _appSetting.ApiUrl + "MessageBroker/TransferSTGData";

                TransferStgList dataToSend = new TransferStgList
                {
                    Category = "Transfer",
                    VehicleNo = VehicleNo,
                    VehicleTo= VehicleTo,
                    TransferTo = TransferTo,
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                    TransferBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId),
                    StgTransferData = _stgData
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
                            
                            bool _submit = await DisplayAlert("Confirm", "Do you want to transfer.", "Yes", "No");
                            if (_submit)
                            {
                                await DisplayAlert("Success", "Data is Transfer successfully!", "OK");
                                conn.Execute("DROP TABLE IF EXISTS SaveLocalCollectionModel");
                                conn.CreateTable<SaveLocalCollectionModel>();
                                OnDismissed?.Invoke();
                                await Navigation.PopModalAsync();
                            }
                             
                        }
                        else
                        {
                            await DisplayAlert("Info", valueData.Message, "OK");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }
        public Action OnDismissed;

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
         
        }
       


    }
}