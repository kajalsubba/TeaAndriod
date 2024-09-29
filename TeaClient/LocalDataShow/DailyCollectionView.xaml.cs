using Android.Telephony.Data;
using Android.Webkit;
using Java.Util.Streams;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.CommonPage;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using TeaClient.UserModule;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Content.ClipData;
using static Android.Provider.Telephony.Mms;

namespace TeaClient.LocalDataShow
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DailyCollectionView : ContentPage
    {
        public SQLiteConnection conn;
        public SaveLocalCollectionModel _CollectionModel;
        readonly string VehicleNo = string.Empty;
        readonly int TripId;
        UserModel LoginData = new UserModel();
        readonly AppSettings _appSetting = AppConfigService.GetConfig();
        IList<SaveLocalCollectionModel> CollectionList = new List<SaveLocalCollectionModel>();
        public IList<SaveLocalCollectionModel> collectionData { get; set; }

        long VehicleFrom;
        public DailyCollectionView(string _VehicleNo, int _Trip, long _VehicleFrom)
        {
            InitializeComponent();
            collectionData = new ObservableCollection<SaveLocalCollectionModel>();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            BindingContext = this;
            VehicleNo = _VehicleNo;
            TripId = _Trip;
            VehicleFrom = _VehicleFrom;

            NavigationPage.SetHasBackButton(this, false);
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<SaveLocalCollectionModel>();
            HeaderName.Text = DateTime.Now.ToString("dd/MM/yyyy");
            CollectionDetails();

        }
        protected override bool OnBackButtonPressed()
        {
            DisplayConfirmation();
            return true; // Do not continue processing the back button
        }
        async void DisplayConfirmation()
        {

            await Navigation.PushAsync(new UserModule.CollectionPage(VehicleNo, TripId, VehicleFrom));


        }

        public async void TransferData()
        {
            try
            {

                string url = _appSetting.ApiUrl + "Collection/GetTransferStgData";

                GetTransferDataModel dataToSend = new GetTransferDataModel
                {
                    CollectionDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    TripId = TripId,
                    VehicleNo = VehicleNo,
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                    CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId),
                };
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(dataToSend);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    var results = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var valueData = JsonConvert.DeserializeObject<TransferDataList>(results);

                        if (valueData.TransferData.Count > 0)
                        {
                            foreach (var item in valueData.TransferData)
                            {
                                await MergeTransferCollection(item.ClientId, item.ClientName, item.FinalWeight,
                                      item.WetLeaf, item.LongLeaf, item.Deduction, item.FinalWeight, item.Rate,
                                      item.GrossAmount, item.GradeId, item.GradeName, item.Remarks, item.BagList, item.TransferFrom, item.VehicleFrom);
                            }
                            var result = await UpdateTransferStatusInServer();

                            await DisplayAlert("Success", result.Message, "OK");


                        }

                        await CollectionDetails();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }

        private async Task<SaveReturn> UpdateTransferStatusInServer()
        {
            try
            {
                string url = _appSetting.ApiUrl + "Collection/UpdateTransferStatus";

                GetTransferDataModel dataToSend = new GetTransferDataModel
                {
                    CollectionDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    TripId = TripId,
                    VehicleNo = VehicleNo,
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                    CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId),
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
                        return valueData;
                    }
                    return null;
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
                return null;
            }

        }
        private async Task MergeTransferCollection(long ClientId, string ClientName, int FirstWgt,
            int WetLeaf, int LongLeaf, int Deduction, int FinalWgt, decimal Rate, decimal grossAmt,
            int GradeId, string GradeName, string Remarks, string Bagslist, long TransferFrom, long VehicleFrom)
        {
            try
            {

                SaveLocalCollectionModel _collect = new SaveLocalCollectionModel();
                _collect.CollectionDate = DateTime.Now.Date;
                _collect.TripId = TripId;
                _collect.VehicleNo = VehicleNo;
                _collect.ClientId = ClientId;
                _collect.ClientName = ClientName;
                _collect.FirstWeight = FirstWgt;
                _collect.WetLeaf = WetLeaf;
                _collect.LongLeaf = LongLeaf;
                _collect.Deduction = Deduction;
                _collect.FinalWeight = FinalWgt;
                _collect.Rate = Rate;
                _collect.GrossAmount = grossAmt;
                _collect.GradeId = GradeId;
                _collect.GradeName = GradeName;
                _collect.Remarks = Remarks ?? "";
                _collect.TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId);
                _collect.Status = "Pending";
                _collect.DataSendToServer = false;
                _collect.CollectionType = "Transfer";
                _collect.BagList = Bagslist;
                _collect.CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId);
                _collect.CollectionType = "Transfer";
                _collect.TransferFrom = TransferFrom;
                _collect.VehicleFrom = VehicleFrom;

                conn.Insert(_collect);

            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "Ok");

            }

            finally
            {

            }

        }

        public async Task CollectionDetails()
        {

            int tenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId);
            int userId = Convert.ToInt32(LoginData.LoginDetails[0].UserId);
            DateTime collectionDate = DateTime.Now.Date;

            //    var CollectionDetails1 = (from x in conn.Table<SaveLocalCollectionModel>() select x).ToList();

            CollectionList = (from x in conn.Table<SaveLocalCollectionModel>()
                              where x.TenantId == tenantId
                               && x.CollectionDate == collectionDate
                               && x.CreatedBy == userId
                               && x.DataSendToServer == false
                              //  && x.CollectionType=="Self"
                              select x).ToList();


            myListView.ItemsSource = CollectionList;

            var _recordTotal = CollectionList.Count();
            TotalRecord.Text = "Count:" + _recordTotal;


            var _firstWgtTotal = CollectionList.Sum(x => x.FirstWeight);
            TotalFirstWgt.Text = "Total Field:" + _firstWgtTotal;

            var _deductionTotal = CollectionList.Sum(x => x.Deduction);
            TotalDeducttion.Text = "Deduct:" + _deductionTotal;

            var _finalWgtTotal = CollectionList.Sum(x => x.FinalWeight);
            FinalTotal.Text = "Total Final: " + _finalWgtTotal;

        }


        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the button that was clicked
                Button button = sender as Button;

                // Get the item associated with the button
                var item = button?.CommandParameter as SaveLocalCollectionModel;

                // Handle the edit action (e.g., navigate to an edit page or display a form)
                if (item != null)
                {
                    //await Navigation.PushAsync(new UserModule.LateralCollectionEntryPage(item));
                }
            }
            catch (Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }
        private async void OnEditButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Button button = sender as Button;

                var item = button?.CommandParameter as SaveLocalCollectionModel;

                if (item != null)
                {
                    await Navigation.PushAsync(new UserModule.EditCollectionPage(item));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }


        private async void OnPrintButtonClicked(object sender, EventArgs e)
        {
            try
            {
                ImageButton button = sender as ImageButton;

                var item = button?.CommandParameter as SaveLocalCollectionModel;

                if (item != null)
                {
                    PrintService prt = new PrintService();
                    var deduct = item.Deduction.ToString() == "0" ? "Pending" : item.Deduction.ToString();
                    var result = await prt.PrintParameters(item.CollectionDate.ToString("dd/MM/yyyy"), LoginData.LoginDetails[0].CompanyName.ToString().Trim(), item.ClientName, item.GradeName, item.BagList, deduct, item.FirstWeight.ToString(),
                        item.FinalWeight.ToString(), item.GrossAmount.ToString(), item.Remarks, LoginData.LoginDetails[0].UserFirstName + " " + LoginData.LoginDetails[0].UserLastName);
                    await DisplayAlert("Info", result, "Ok");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }

        private async void OnRefreshBtnClicked(object sender, EventArgs e)
        {
            TransferData();
        }

        private List<StgSaveModel> ArrangeSendToServerData()
        {

            List<StgSaveModel> _stgData = new List<StgSaveModel>();

            foreach (SaveLocalCollectionModel item in myListView.ItemsSource)
            {
                var objStg = new StgSaveModel
                {
                    Id = item.Id,
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
                    CollectionType = item.CollectionType,
                    TransferFrom = item.TransferFrom,
                    VehicleFrom = item.VehicleFrom,
                    FinishTimeInApp = item.FinishTimeInApp,
                    UpdateTimeInApp = item.UpdateTimeInApp
                };
                _stgData.Add(objStg);
            }
            return _stgData;
        }
        public async Task SaveSTGUsingMQ()
        {
            try
            {
                var loadingPage = new LoadingPage();
                await Navigation.PushModalAsync(loadingPage);

                string _comment = "";
                if (string.IsNullOrWhiteSpace(_comment))
                {
                    await DisplayAlert("Validation", "Please Enter Comment Before sending to Server!", "OK");
                    return;
                }
                List<StgSaveModel> _stgData = new List<StgSaveModel>();

                foreach (SaveLocalCollectionModel item in myListView.ItemsSource)
                {
                    var objStg = new StgSaveModel
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
                        CollectionType = item.CollectionType,
                        TransferFrom = item.TransferFrom,
                        VehicleFrom = item.VehicleFrom,
                        FinishTimeInApp = item.FinishTimeInApp,
                        UpdateTimeInApp = item.UpdateTimeInApp
                    };
                    _stgData.Add(objStg);
                }

                string url = _appSetting.ApiUrl + "MessageBroker/ProduceStgList";

                STGModelList dataToSend = new STGModelList
                {
                    Source = "Mobile",
                    Category = "STG",
                    ServerComment = "",
                    VehicleNo = VehicleNo,
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                    CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId),
                    StgListData = _stgData
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
                            foreach (SaveLocalCollectionModel item in myListView.ItemsSource)
                            {
                                UpdateSendServerStatus(item.Id);
                            }

                            await DisplayAlert("Success", "Data is send to server successfully!", "OK");
                            //  ServerComment.Text = string.Empty;
                            CollectionDetails();
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
            finally
            {
                BtnTransfer.IsEnabled = true;
                await Navigation.PopModalAsync();

            }
        }

        private async void OnSendToServerClicked(object sender, EventArgs e)
        {
            //var _submit = await DisplayAlert("Save", "Do you want to send to server! ", "Yes", "No");
            //if (_submit)
            //{
            //    BtnTransfer.IsEnabled = false;
            //    await SaveSTGUsingMQ();
            //}

            var saveData = ArrangeSendToServerData();
            if (saveData.Count > 0)
            {
                try
                {
                    var SalePage = new SalePopUpPage(saveData, VehicleNo)
                    {
                        OnDismissed = async () =>
                        {

                            // CollectionDetails();
                            await Navigation.PushAsync(new UserModule.UserDashboardPage());

                        }
                    };

                    await Navigation.PushModalAsync(SalePage);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "Ok");
                }
            }

        }
        private async void OnTransferClicked(object sender, EventArgs e)
        {
            try
            {
                var transferPage = new TransferPopUpPage(VehicleNo, TripId, CollectionList)
                {
                    OnDismissed = async () =>
                    {

                        CollectionDetails();
                    }
                };

                await Navigation.PushModalAsync(transferPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }



        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserModule.CollectionPage(VehicleNo, TripId, VehicleFrom));

        }

        void UpdateSendServerStatus(long Id)
        {
            try
            {
                var existingCollect = conn.Find<SaveLocalCollectionModel>(Id);
                existingCollect.DataSendToServer = true;
                conn.Update(existingCollect);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Unsubscribe to avoid memory leaks
            //  MessagingCenter.Unsubscribe<TransferPopUpPage>(this, "RefreshParentPage");
        }
    }
}