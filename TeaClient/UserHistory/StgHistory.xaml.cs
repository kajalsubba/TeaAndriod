using Java.Util;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.CommonPage;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.UserHistory
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StgHistory : ContentPage
    {
        private decimal _totalFirstWeight;
        private decimal _totalDeduction;
        private decimal _totalFinalWeight;
        public IList<ClientModel> clientData { get; set; }

        public IList<StgHistoryModel> stgItems { get; set; }
        readonly UserModel LoginData = new UserModel();
        readonly AppSettings _appSetting = AppConfigService.GetConfig();
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;
        int ClientId=0;
        public StgHistory()
        {
            InitializeComponent();
            clientData = new ObservableCollection<ClientModel>();

            stgItems = new ObservableCollection<StgHistoryModel>();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            BindingContext = this;
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalClientSaveModel>();
            GetClientFromLocalDB();
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await GetStgHistory();
            if (stgItems != null && stgItems.Any())
            {
                TotalFirstWeight = stgItems.Where(item => item.Status != "Rejected")
                                .Sum(item => item.FirstWeight);
                TotalDeduction = stgItems.Where(item => item.Status != "Rejected")
                                .Sum(item => item.Deduction);
                TotalFinalWeight = stgItems.Where(item => item.Status != "Rejected")
                              .Sum(item => item.FinalWeight);



            }
            else
            {
                TotalFirstWeight = 0;
                TotalDeduction = 0;
                TotalFinalWeight = 0;
                stgItems.Clear();
                await DisplayAlert("Info", "Record is not found !", "OK");
            }

        }
        public void GetClientFromLocalDB()
        {
            var ClientDetails = conn.Table<LocalClientSaveModel>()
                            .Select(x => new { x.ClientId, x.ClientName }) // Select the columns you want to be unique
                            .Distinct()
                            .ToList();

            foreach (var _client in ClientDetails)
            {
                clientData.Add(new ClientModel { ClientId = _client.ClientId, ClientName = _client.ClientName });

            }
        }
        private void ListView_OnItemTapped(Object sender, ItemTappedEventArgs e)
        {
            ClientModel listed = e.Item as ClientModel;
            Client.Text = listed.ClientName;
            ClientId = listed.ClientId;
            ClientListView.IsVisible = false;

            ((ListView)sender).SelectedItem = null;
        }
        private async void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ClientListView.IsVisible = true;
            ClientListView.BeginRefresh();

            try
            {
                var dataEmpty = clientData.Where(i => i.ClientName.ToLower().Contains(e.NewTextValue.ToLower())).ToList();

                if (string.IsNullOrWhiteSpace(e.NewTextValue) || dataEmpty.Count == 0)
                {
                    ClientListView.IsVisible = false;
                }
                else
                {
                    // If there are results, show the ListView and bind the filtered data
                    ClientListView.IsVisible = true;
                    ClientListView.ItemsSource = dataEmpty;
                }
            }
            catch (Exception ex)
            {
                ClientListView.IsVisible = false;
                //  throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
            ClientListView.EndRefresh();

        }


        public decimal TotalFirstWeight
        {
            get { return _totalFirstWeight; }
            set
            {
                if (_totalFirstWeight != value)
                {

                    _totalFirstWeight = Math.Round(value, 0);
                    OnPropertyChanged(nameof(TotalFirstWeight));
                }
            }
        }

        public decimal TotalDeduction
        {
            get { return _totalDeduction; }
            set
            {
                if (_totalDeduction != value)
                {

                    _totalDeduction = Math.Round(value, 0);
                    OnPropertyChanged(nameof(TotalDeduction));
                }
            }
        }

        public decimal TotalFinalWeight
        {
            get { return _totalFinalWeight; }
            set
            {
                if (_totalFinalWeight != value)
                {

                    _totalFinalWeight = Math.Round(value, 0);
                    OnPropertyChanged(nameof(TotalFinalWeight));
                }
            }
        }
        public async Task GetStgHistory()
        {
            if (Client.Text==string.Empty)
            {
                ClientId = 0;
            }
            stgItems.Clear();
            string url = _appSetting.ApiUrl + "Collection/GetMobileStgData";

            DateTime fromDate = FromDate.Date;
            string formattedfromDate = fromDate.ToString("yyyy-MM-dd");
            DateTime toDate = ToDate.Date;
            string formattedtoDate = toDate.ToString("yyyy-MM-dd");

            try {
                // Create an object to be sent as JSON in the request body
                StgFilterRequest dataToSend = new StgFilterRequest()
                {
                    FromDate = Convert.ToDateTime(formattedfromDate),
                    ToDate = Convert.ToDateTime(formattedtoDate),
                    ClientId = ClientId,
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId)
                };
                var loadingPage = new LoadingPage();
                await Navigation.PushModalAsync(loadingPage);

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(dataToSend);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    var results = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var data = JsonConvert.DeserializeObject<StgHistoryList>(results);
                        if (data.STGDetails.Count > 0)
                        {
                            foreach (var _stg in data.STGDetails)
                            {

                                stgItems.Add(new StgHistoryModel
                                {
                                    SrlNo=_stg.SrlNo,
                                    CollectionId = _stg.CollectionId,
                                    CollectionDate = _stg.CollectionDate,
                                    GradeName = _stg.GradeName,
                                    VehicleNo = _stg.VehicleNo,
                                    ClientName = _stg.ClientName,
                                    FirstWeight = _stg.FirstWeight,
                                    Deduction = _stg.Deduction,
                                    FinalWeight = _stg.FinalWeight,
                                    Status = _stg.Status,
                                    ViewBag=_stg.ViewBag,
                                    CreatedBy = _stg.CreatedBy,
                                    CollectionType=_stg.CollectionType,
                                    TransferFrom=_stg.TransferFrom,
                                    VehicleFrom=_stg.VehicleFrom
                                });
                            }
                        }
                     }

                }
                await Navigation.PopModalAsync();
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error",ex.Message, "OK");
                await Navigation.PopModalAsync();
            }
            }



    }
}