using Newtonsoft.Json;
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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SmartHistoryPage : ContentPage
    {
        private long _totalFinalWeight;
        private decimal _totalGrossAmount;
        private decimal _avgRate;
        private int _totalDays;
        private decimal _totalPaymentAmount;
        //private decimal _previousBalance;
        //private decimal _standingSeasonAdv;
        public IList<SmartHistoryModel> dataItems { get; set; }

        public IList<SmartPaymentModel> paymentItems { get; set; }

        public IList<OutstandingModel> outstandingItems { get; set; }

        ClientLoginData LoginData = new ClientLoginData();
        AppSettings _appSetting = AppConfigService.GetConfig();

        public SmartHistoryPage()
        {
            InitializeComponent();
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
            dataItems = new ObservableCollection<SmartHistoryModel>();
            paymentItems = new ObservableCollection<SmartPaymentModel>();
            outstandingItems = new ObservableCollection<OutstandingModel>();
            BindingContext = this;
            HeaderName.Text = "Smart History for " + LoginData.ClientLoginDetails[0].ClientName;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await GetSmartHistory();
            if (dataItems != null && dataItems.Any())
            {
                TotalFinalWeight = dataItems.Where(item => item.Status != "Rejected")
                                .Sum(item =>Convert.ToInt32(item.FinalWeight));
                TotalGrossAmount = dataItems.Where(item => item.Status != "Rejected")
                                .Sum(item => item.Amount);
                AvgRate = TotalGrossAmount / TotalFinalWeight;

                TotalDays = dataItems.Where(item => item.Status != "Rejected")
                .Select(item =>item.CollectionDate).Distinct().Count();
            }
            else
            {
                TotalFinalWeight = 0;
                TotalGrossAmount = 0;
                AvgRate = 0;
                TotalDays = 0;
                dataItems.Clear();
                await DisplayAlert("Info", "Record is not found !", "OK");
            }

            if (paymentItems != null && paymentItems.Any())
            {
                TotalPaymentAmount = paymentItems.Sum(item => item.Amount);

            }
            else
            {
                TotalPaymentAmount = 0;
               
                paymentItems.Clear();
              //  await DisplayAlert("Info", "Record is not found !", "OK");
            }
            //if (outstandingItems != null && outstandingItems.Any())
            //{
            //    PreviousBalance = outstandingItems.Sum(item => item.PreviousBalance);
            //    TotalSeasonAdvance = outstandingItems.Sum(item => item.SeasonAdvance);
            //}
            //else
            //{
            //    TotalPaymentAmount = 0;

            //    outstandingItems.Clear();
            //    //  await DisplayAlert("Info", "Record is not found !", "OK");
            //}

        }

        //public decimal TotalSeasonAdvance
        //{
        //    get { return _standingSeasonAdv; }
        //    set
        //    {
        //        if (_standingSeasonAdv != value)
        //        {

        //            _standingSeasonAdv = Math.Round(value, 2);
        //            OnPropertyChanged(nameof(TotalSeasonAdvance));
        //        }
        //    }
        //}
        //public decimal PreviousBalance
        //{
        //    get { return _previousBalance; }
        //    set
        //    {
        //        if (_previousBalance != value)
        //        {

        //            _previousBalance = Math.Round(value, 2);
        //            OnPropertyChanged(nameof(PreviousBalance));
        //        }
        //    }
        //}
        public int TotalDays
        {
            get { return _totalDays; }
            set
            {
                if (_totalDays != value)
                {

                    _totalDays = value;
                    OnPropertyChanged(nameof(TotalDays));
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

        public decimal TotalPaymentAmount
        {
            get { return _totalPaymentAmount; }
            set
            {
                if (_totalPaymentAmount != value)
                {

                    _totalPaymentAmount = Math.Round(value, 2);
                    OnPropertyChanged(nameof(TotalPaymentAmount));
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
        public long TotalFinalWeight
        {
            get { return _totalFinalWeight; }
            set
            {
                if (_totalFinalWeight != value)
                {

                    _totalFinalWeight = value; ;
                    OnPropertyChanged(nameof(TotalFinalWeight));
                }
            }
        }

        public async Task GetSmartHistory()
        {
            paymentItems.Clear();
            dataItems.Clear();
            outstandingItems.Clear();
            string url = _appSetting.ApiUrl + "Accounts/GetSmartHistory";

            DateTime fromDate = FromDate.Date;
            string formattedfromDate = fromDate.ToString("yyyy-MM-dd");
            DateTime toDate = ToDate.Date;
            string formattedtoDate = toDate.ToString("yyyy-MM-dd");


            // Create an object to be sent as JSON in the request body
            SmartSearchModel dataToSend = new SmartSearchModel()
            {
                FromDate = Convert.ToDateTime(formattedfromDate),
                ToDate = Convert.ToDateTime(formattedtoDate),
                CategoryName= "Supplier",
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
                    var data = JsonConvert.DeserializeObject<SmartList>(results);
                    if (data.CollectionSummary.Count > 0)
                    {
                        foreach (var _supplier in data.CollectionSummary)
                        {

                            dataItems.Add(new SmartHistoryModel
                            {
                                CollectionDate = _supplier.CollectionDate,
                                FinalWeight = _supplier.FinalWeight,
                                Rate = _supplier.Rate,
                                Amount = _supplier.Amount,
                                Status = _supplier.Status,
                            
                            });
                        }
                    }
                    if (data.PaymentSummary.Count > 0)
                    {
                        foreach (var _payment in data.PaymentSummary)
                        {

                            paymentItems.Add(new SmartPaymentModel
                            {
                                PaymentDate = _payment.PaymentDate,
                                Narration = _payment.Narration,
                                Amount = _payment.Amount

                            });
                        }
                    }
                    if (data.OutstandingSummary.Count > 0)
                    {

                        foreach (var _outstanding in data.OutstandingSummary)
                        {

                            //outstandingItems.Add(new OutstandingModel
                            //{
                            //    SeasonAdvance = _outstanding.SeasonAdvance,
                            //    PreviousBalance = _outstanding.PreviousBalance,


                            //});
                            txtPreviousBalance.Text = _outstanding.SeasonAdvance.ToString();
                            txtStandingSeasonAdv.Text = _outstanding.PreviousBalance.ToString();
                        }
                    }
                    else
                    {
                        txtPreviousBalance.Text = "0";
                        txtStandingSeasonAdv.Text="0";

                    }

                }

            }
        }
    }
}