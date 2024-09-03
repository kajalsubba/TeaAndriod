using Android.Webkit;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class SupplierHistory : ContentPage
    {
        private decimal _totalChallanWeight;
        private decimal _totalGrossAmount;
        private decimal _avgRate;

        //  public decimal AvgRate;
        public IList<SupplierHistoryModel> dataItems { get; set; }
        readonly ClientLoginData LoginData = new ClientLoginData();
        readonly AppSettings _appSetting = AppConfigService.GetConfig();
        public SupplierHistory()
        {
            InitializeComponent();
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
            dataItems = new ObservableCollection<SupplierHistoryModel>();
            BindingContext = this;
        }
        protected override bool OnBackButtonPressed()
        {
            //  DisplayConfirmation();
            return true; // Do not continue processing the back button
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await GetSupplierHistory();
            if (dataItems != null && dataItems.Any())
            {
                TotalChallanWeight = dataItems.Where(item => item.Status != "Rejected")
                                .Sum(item => item.ChallanWeight);
                TotalGrossAmount = dataItems.Where(item => item.Status != "Rejected")
                                .Sum(item => item.GrossAmount);

                AvgRate = TotalGrossAmount / TotalChallanWeight;
            }
            else
            {
                TotalChallanWeight = 0;
                TotalGrossAmount = 0;
                AvgRate = 0;
                dataItems.Clear();
                await DisplayAlert("Info", "Record is not found !", "OK");
            }

        }

        public decimal TotalChallanWeight
        {
            get { return _totalChallanWeight; }
            set
            {
                if (_totalChallanWeight != value)
                {

                    _totalChallanWeight = Math.Round(value, 0);
                    OnPropertyChanged(nameof(TotalChallanWeight));
                }
            }
        }

        public decimal TotalGrossAmount
        {
            get { return _totalGrossAmount; }
            set
            {
                if (_totalGrossAmount != value)
                {

                    _totalGrossAmount = Math.Round(value, 2);
                    OnPropertyChanged(nameof(TotalGrossAmount));
                }
            }
        }

        public decimal AvgRate
        {
            get { return _avgRate; }
            set
            {
                if (_avgRate != value)
                {
                    _avgRate = Math.Round(value, 2);

                    OnPropertyChanged(nameof(AvgRate));
                }
            }
        }
        public async Task GetSupplierHistory()
        {
            dataItems.Clear();
            string url = _appSetting.ApiUrl + "Collection/GetSupplierMobileData";

            DateTime fromDate = FromDate.Date;
            string formattedfromDate = fromDate.ToString("yyyy-MM-dd");
            DateTime toDate = ToDate.Date;
            string formattedtoDate = toDate.ToString("yyyy-MM-dd");


            // Create an object to be sent as JSON in the request body
            SupplierSearchModel dataToSend = new SupplierSearchModel()
            {
                FromDate = Convert.ToDateTime(formattedfromDate),
                ToDate = Convert.ToDateTime(formattedtoDate),
                ClientId = Convert.ToInt32(LoginData.ClientLoginDetails[0].ClientId),
                TenantId = Convert.ToInt32(LoginData.ClientLoginDetails[0].TenantId)
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<SupplierList>(results);
                    if (data.MobileData.Count > 0)
                    {
                        foreach (var _supplier in data.MobileData)
                        {

                            dataItems.Add(new SupplierHistoryModel
                            {
                                CollectionDate = _supplier.CollectionDate,
                                FactoryName = _supplier.FactoryName,
                                VehicleNo = _supplier.VehicleNo,
                                FineLeaf = _supplier.FineLeaf,
                                ChallanWeight = _supplier.ChallanWeight,
                                Rate = _supplier.Rate,
                                RateStatus=_supplier.RateStatus,
                                GrossAmount = _supplier.GrossAmount,
                                Remarks = _supplier.Remarks,
                                Status = _supplier.Status,
                                CreatedBy = _supplier.CreatedBy
                            });
                        }
                    }
                    else
                    {


                    }

                }

            }
        }

    }
}