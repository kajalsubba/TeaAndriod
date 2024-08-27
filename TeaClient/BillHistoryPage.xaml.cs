using Android.Webkit;
using Newtonsoft.Json;
using Org.Apache.Http.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.CommonPage;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BillHistoryPage : ContentPage
    {
        private long _totalChallanWeight;
        private decimal _totalLeafAmount;
        private decimal _totalReceipt;
        private decimal _totalBillReceipt;

        ClientLoginData LoginData = new ClientLoginData();
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<SupplierBillHistoryModel> dataItems { get; set; }

        public BillHistoryPage()
        {
            InitializeComponent();
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
            dataItems = new ObservableCollection<SupplierBillHistoryModel>();
            BindingContext = this;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await GetSupplierBillHistory();
            if (dataItems != null && dataItems.Any())
            {
                TotalChallanWeight = dataItems.Sum(item => item.FinalWeight);
                TotalLeafAmount = dataItems.Sum(item => item.TotalStgAmount);
                TotalReceiptAmount = dataItems.Sum(item => item.TotalStgPayment);
                TotalBillReceiptAmount = dataItems.Sum(item => item.AmountToPay);
            }
            else
            {
                TotalChallanWeight = 0;
                TotalLeafAmount = 0;
                TotalReceiptAmount = 0;
                TotalBillReceiptAmount = 0;
                dataItems.Clear();
                await DisplayAlert("Info", "Record is not found !", "OK");
            }

        }
        public decimal TotalReceiptAmount
        {
            get { return _totalReceipt; }
            set
            {
                if (_totalReceipt != value)
                {

                    _totalReceipt = value;
                    OnPropertyChanged(nameof(TotalReceiptAmount));
                }
            }
        }

        public decimal TotalBillReceiptAmount
        {
            get { return _totalBillReceipt; }
            set
            {
                if (_totalBillReceipt != value)
                {

                    _totalBillReceipt = value;
                    OnPropertyChanged(nameof(TotalBillReceiptAmount));
                }
            }
        }

        private async void OnPrintButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string url = _appSetting.ApiUrl + "Print/SupplierBillPrint";

                var billId = button.CommandParameter;

            
                var data = new
                {
                    BillNo = billId,
                    TenantId = Convert.ToInt32(LoginData.ClientLoginDetails[0].TenantId)
                };
                try
                {
                    var loadingPage = new LoadingPage();
                    await Navigation.PushModalAsync(loadingPage);

                   
                    var pdfBytes = await PostDataAndGetPdfAsync(url, data);
                    var filePath = Path.Combine(FileSystem.CacheDirectory, "SupplierBill_"+DateTime.Now.ToString("HH:mm:ss") + ".pdf");
                    await SavePdfAsync(filePath, pdfBytes);
                    await OpenPdfAsync(filePath);
                    await Navigation.PopModalAsync();

                    //LoadingIndicator.IsRunning = false;
                    //LoadingIndicator.IsVisible = false;
                    // StatusLabel.Text = "PDF generated and opened successfully.";
                }
                catch (Exception ex)
                {
                    await Navigation.PopModalAsync();

                    //StatusLabel.Text = $"An error occurred: {ex.Message}";
                }
            }
        }
        private async Task<byte[]> PostDataAndGetPdfAsync<T>(string endpoint, T data)
        {
            using (HttpClient client = new HttpClient())
            {

                var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        private async Task SavePdfAsync(string filePath, byte[] pdfBytes)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await fileStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
            }
        }

        private async Task OpenPdfAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            else
            {
                await DisplayAlert("Error", "File not found.", "OK");
            }
        }

        public long TotalChallanWeight
        {
            get { return _totalChallanWeight; }
            set
            {
                if (_totalChallanWeight != value)
                {

                    _totalChallanWeight = value;
                    OnPropertyChanged(nameof(TotalChallanWeight));
                }
            }
        }
        public decimal TotalLeafAmount
        {
            get { return _totalLeafAmount; }
            set
            {
                if (_totalLeafAmount != value)
                {

                    _totalLeafAmount = value;
                    OnPropertyChanged(nameof(TotalLeafAmount));
                }
            }
        }

        public async Task GetSupplierBillHistory()
        {
            dataItems.Clear();
            string url = _appSetting.ApiUrl + "Accounts/GetSupplierBillHistory";

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
                    var data = JsonConvert.DeserializeObject<SupplierBillList>(results);
                    if (data.BillHistory.Count > 0)
                    {
                        foreach (var _supplier in data.BillHistory)
                        {
                            dataItems.Add(new SupplierBillHistoryModel
                            {
                                BillId = _supplier.BillId,
                                BillDate = _supplier.BillDate,
                                BillPeriod = _supplier.BillPeriod,
                                ClientName = _supplier.ClientName,
                                AvgRate = _supplier.AvgRate,
                                FinalWeight = _supplier.FinalWeight,
                                TotalStgAmount = _supplier.TotalStgAmount,
                                TotalStgPayment = _supplier.TotalStgPayment,
                                CommAmount = _supplier.CommAmount,
                                CessAmount = _supplier.CessAmount,
                                LessSeasonAdv = _supplier.LessSeasonAdv,
                                AmountToPay = _supplier.AmountToPay,
                                CreatedBy=_supplier.CreatedBy,
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