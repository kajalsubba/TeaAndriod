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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.CommonPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SalePopUpPage : ContentPage
    {
        readonly AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<FactoryAccountModel> FactoryAccountlists { get; set; }
        public IList<FactoryModel> Factorylists { get; set; }
        public SQLiteConnection conn;
        UserModel LoginData = new UserModel();
        public IList<FactoryAccountModel> Accountlists { get; set; }

        List<StgSaveModel> SaveData = new List<StgSaveModel>();
        string VehicleNo = string.Empty;
        public SalePopUpPage(List<StgSaveModel> _SaveData, string _VehicleNo)
        {
            InitializeComponent();
            SaveData = _SaveData;
            VehicleNo = _VehicleNo;
            Accountlists = new ObservableCollection<FactoryAccountModel>();
            Factorylists = new ObservableCollection<FactoryModel>();

            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            GetFactoryAccount();
            GetFactory();

            FactoryName.ItemsSource = (System.Collections.IList)Factorylists;
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<SaveLocalCollectionModel>();
        }

        public async Task SendSignalRNotification(bool message, int tenantId)
        {
            string url = $"{_appSetting.ApiUrl}SignalR/SendNotification?Message={message}&TenantId={tenantId}";

           // string url = $"{apiUrl}/SendNotification?Message={message}&TenantId={tenantId}";

            using (HttpClient client = new HttpClient())
            {
                // Send a POST request with no body
                var response = await client.PostAsync(url, null);

                // Read the response
                var results = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Notification sent successfully.");
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}, Content: {results}");
                }
            }

        }

        public async Task SaveSTGUsingMQ()
        {
            try
            {
                var loadingPage = new LoadingPage();
                await Navigation.PushModalAsync(loadingPage);

               

                var selectedAccount = AccountName.SelectedItem as FactoryAccountModel;

                string url = _appSetting.ApiUrl + "MessageBroker/ProduceStgList";
                int _FineLeaf = string.IsNullOrWhiteSpace(FineLeaf.Text) ? 0 : int.Parse(FineLeaf.Text);

                STGModelList dataToSend = new STGModelList
                {
                    Source = "Mobile",
                    Category = "STG",
                    ServerComment = remarksEditor.Text,
                    AccountId = selectedAccount.AccountId,
                    FineLeaf = _FineLeaf,
                    ChallanWeight = Convert.ToInt16(ChallanWgt.Text),
                    VehicleNo = VehicleNo,
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                    CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId),
                    StgListData = SaveData
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
                            foreach (var item in SaveData)
                            {
                                UpdateSendServerStatus(item.Id);
                            }

                            await DisplayAlert("Success", "Data is send to server successfully!", "OK");
                            //ServerComment.Text = string.Empty;
                            //  CollectionDetails();
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
                btnServer.IsEnabled = true;
                await Navigation.PopModalAsync();

            }
        }
        private async void OnSendToServerClicked(object sender, EventArgs e)
        {
            string challan = ChallanWgt.Text;


            if (string.IsNullOrWhiteSpace(challan))
            {
                await DisplayAlert("Validation", "Please Enter Challan Weight", "OK");
                return;
            }
            else if (FactoryName.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Factory", "OK");
                return;
            }
            else if (AccountName.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Account", "OK");
                return;
            }
            var _submit = await DisplayAlert("Save", "Do you want to send to server! ", "Yes", "No");
            if (_submit)
            {
                btnServer.IsEnabled = false;
                await SaveSTGUsingMQ();
                await SendSignalRNotification(true, Convert.ToInt32(LoginData.LoginDetails[0].TenantId));
                OnDismissed?.Invoke();
                await Navigation.PopModalAsync();
            }
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



        public async void GetFactory()
        {

            string url = _appSetting.ApiUrl + "Master/GetFactory";


            var dataToSend = new
            {
                IsClientView = false,
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
                    var data = JsonConvert.DeserializeObject<FactoryList>(results);
                    foreach (var factory in data.FactoryDetails)
                    {

                        Factorylists.Add(new FactoryModel { FactoryId = factory.FactoryId, FactoryName = factory.FactoryName });
                    }

                }
            }


        }



        public async void GetFactoryAccount()
        {

            string url = _appSetting.ApiUrl + "Master/GetFactoryAccount";

            var dataToSend = new
            {
                IsClientView = false,
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
                    var data = JsonConvert.DeserializeObject<FactoryAccountList>(results);
                    foreach (var factory in data.AccountDetails)
                    {

                        Accountlists.Add(new FactoryAccountModel { FactoryId = factory.FactoryId, AccountId = factory.AccountId, AccountName = factory.AccountName });
                    }

                }

            }


        }

        public Action OnDismissed;

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

        }
        private void OnFactotyIndexChanged(object sender, EventArgs e)
        {

            var selectedFactory = FactoryName.SelectedItem as FactoryModel;
            IList<FactoryAccountModel> filteredList = Accountlists.Where(p => p.FactoryId == selectedFactory.FactoryId).ToList();

            AccountName.ItemsSource = (System.Collections.IList)filteredList;
            AccountName.ItemDisplayBinding = new Binding("AccountName");

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
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

    }
}