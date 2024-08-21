using Android.Accounts;
using Android.Locations;
using Javax.Xml.Transform.Sax;
using Newtonsoft.Json;
using Org.Apache.Http.Client.Params;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CollectionPage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<ClientModel> clientData { get; set; }
        int InitialVal = 0;
        UserModel LoginData = new UserModel();
        int ClientId;
        int TripId;
        int BagCount=0;

   
        public IList<GradeModel> GradeList { get; set; }
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;

        public CollectionPage(string _VehicleNo,int _TripId)
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            clientData = new ObservableCollection<ClientModel>();
          
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");

            lblVehicle.Text = _VehicleNo;
            TripId = _TripId;
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalClientSaveModel>();
            conn.CreateTable<SaveLocalCollectionModel>();
            
            GetClientFromLocalDB();
            GetTotal();

        }

        void GetTotal()
        {

            var _recordTotal = GetTotalRecord();
            TotalRecord.Text= "Total Record:" + _recordTotal;

            var _firstWgtTotal = GetTotalFirstWeight();
            TotalFirstWgt.Text = "Total Field:" + _firstWgtTotal;

            var _deductionTotal = GetTotalDeduction();
            TotalDeducttion.Text = "Total Deduct:" + _deductionTotal;


            var _finalWgtTotal = GetTotalFinalWeight();
            GrossTotalWgt.Text = "Total Final: " + _finalWgtTotal;

        }
        public int GetTotalFirstWeight()
        {
            // Compute the sum of FinalWeight
            var sum = conn.ExecuteScalar<int>("SELECT SUM(FirstWeight) FROM SaveLocalCollectionModel");
            return sum;
        }
        public int GetTotalDeduction()
        {
            // Compute the sum of FinalWeight
            var sum = conn.ExecuteScalar<int>("SELECT SUM(Deduction) FROM SaveLocalCollectionModel");
            return sum;
        }
        public int GetTotalRecord()
        {
            // Compute the sum of FinalWeight
            var count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM SaveLocalCollectionModel");
            return count;
        }
        public int GetTotalFinalWeight()
        {
            // Compute the sum of FinalWeight
            var sum = conn.ExecuteScalar<int>("SELECT SUM(FinalWeight) FROM SaveLocalCollectionModel");
            return sum;
        }
    
   
        private async void OnNumberButtonClicked(object sender, EventArgs e)
        {
            string _ClientName = Client.Text;
            if (string.IsNullOrWhiteSpace(_ClientName) || ClientId == 0)
            {
                await DisplayAlert("Validation", "Please Enter Client", "OK");
                return;
            }
            var button = sender as Button;
            if (button != null)
            {
                // Append the number or decimal point to the entry
                EntryFieldWeight.Text += button.Text;
            }
        }

        private void OnOperatorButtonClicked(object sender, EventArgs e)
        {
            

            Button button = sender as Button;
            if (button != null && EntryFieldWeight != null)
            {
                string buttonText = button.Text;
                string currentText = EntryFieldWeight.Text;

                // Prevent multiple consecutive '+' operators
                if (buttonText == "+")
                {
                    if (currentText.Length > 0 && currentText.Last() == '+')
                    {
                        // Do nothing if the last character is already '+'
                        return;
                    }
                }

                // Append the operator to the current text
                EntryFieldWeight.Text += buttonText;

                TotalCollectionOnType();
            }
        }

        private void OnEqualsButtonClicked(object sender, EventArgs e)
        {

            //if (EntryFieldWeight != null)
            //{
            //    string expression = EntryFieldWeight.Text;

            //    // Remove trailing '+' if it exists
            //    if (expression.EndsWith("+"))
            //    {
            //        expression = expression.Substring(0, expression.Length - 1);
            //    }

            //    try
            //    {
            //        // Calculate the result
            //        double result = EvaluateExpression(expression);
            //        TotalFieldWeight.Text = result.ToString();
            //        FinalWeight.Text= result.ToString();
            //    }
            //    catch (Exception ex)
            //    {
            //        // Handle any errors that might occur during evaluation
            //        DisplayAlert("Error", "Invalid Expression", "OK");
            //    }

            //    TotalBagCount();
            //}

            TotalCollectionOnType();
        }

        void TotalCollectionOnType()
        {
            if (EntryFieldWeight != null)
            {
                string expression = EntryFieldWeight.Text;

                // Remove trailing '+' if it exists
                if (expression.EndsWith("+"))
                {
                    expression = expression.Substring(0, expression.Length - 1);
                }

                try
                {
                    // Calculate the result
                    double result = EvaluateExpression(expression);
                    TotalFieldWeight.Text = result.ToString();
                    FinalWeight.Text = result.ToString();
                }
                catch (Exception ex)
                {
                    // Handle any errors that might occur during evaluation
                    DisplayAlert("Error", "Invalid Expression", "OK");
                }

                TotalBagCount();
            }
        }

        private void OnClearButtonClicked(object sender, EventArgs e)
        {
            // Clear the entry
            EntryFieldWeight.Text = string.Empty;
            TotalFieldWeight.Text = string.Empty;
            FinalWeight.Text = string.Empty;
            txtBagCount.Text=string.Empty;
        }
        private async void OnStepCleanButtonClicked(object sender, EventArgs e)
        {
            if (EntryFieldWeight != null)
            {

                string currentText = EntryFieldWeight.Text;
                if (!string.IsNullOrEmpty(currentText))
                {
                    // Remove the last character from the text
                    EntryFieldWeight.Text = currentText.Substring(0, currentText.Length - 1);

                    string expression = EntryFieldWeight.Text;

                    // Remove trailing '+' if it exists
                    if (expression.EndsWith("+"))
                    {
                        expression = expression.Substring(0, expression.Length - 1);
                    }

                    try
                    {
                        // Calculate the result
                        double result = EvaluateExpression(expression);
                        TotalFieldWeight.Text = result.ToString();
                        FinalWeight.Text = result.ToString();
                    }
                    catch (Exception ex)
                    {
                        TotalFieldWeight.Text = string.Empty;
                        FinalWeight.Text = string.Empty;
                        await DisplayAlert("Error", ex.Message, "Ok");

                    }
                }
                TotalBagCount();
            }

          
        }

  
        private double EvaluateExpression(string expression)
        {
            var dataTable = new System.Data.DataTable();
            return Convert.ToDouble(dataTable.Compute(expression, string.Empty));
        }
      
        private async void RainLeaf_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int longLeafValue = string.IsNullOrWhiteSpace(LongLeaf.Text) ? 0 : int.Parse(LongLeaf.Text);
                Double RateValue = string.IsNullOrWhiteSpace(Rate.Text) ? 0 : double.Parse(Rate.Text);

                if (int.TryParse(e.NewTextValue, out int RainLeafPercentage))
                {
                    CalculateFinalWeight(RainLeafPercentage, longLeafValue, RateValue);

                }
                else
                {
                   
                    CalculateFinalWeight(0, longLeafValue, RateValue);
                }
            }
            catch(Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }

        private async void CalculateFinalWeight(int RainPercentage,int longLeafPercentage,double rate)
        {
            try
            {
                int FirstWeightValue = string.IsNullOrWhiteSpace(TotalFieldWeight.Text) ? 0 : int.Parse(TotalFieldWeight.Text);

                var RainResult =Math.Round((double)(RainPercentage * FirstWeightValue) / 100); 
                var LongResult = Math.Round((double)(longLeafPercentage * FirstWeightValue) / 100); 
                Deduction.Text = (RainResult + LongResult).ToString();
                FinalWeight.Text = (FirstWeightValue - RainResult - LongResult).ToString();

                FinalAmount.Text = (Convert.ToInt16(FinalWeight.Text) * rate).ToString();
            }
            catch(Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }
        private async void Rate_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int longLeafValue = string.IsNullOrWhiteSpace(LongLeaf.Text) ? 0 : int.Parse(LongLeaf.Text);
                int RainLeafValue = string.IsNullOrWhiteSpace(RainLeaf.Text) ? 0 : int.Parse(RainLeaf.Text);

                if (double.TryParse(e.NewTextValue, out double RateVal))
                {
                    CalculateFinalWeight(RainLeafValue, longLeafValue, RateVal);

                }
                else
                {
                  
                    CalculateFinalWeight(RainLeafValue, longLeafValue, 0);
                }
            }
            catch(Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }

        
        private async void LongLeaf_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int RainLeafValue = string.IsNullOrWhiteSpace(RainLeaf.Text) ? 0 : int.Parse(RainLeaf.Text);
                Double RateValue = string.IsNullOrWhiteSpace(Rate.Text) ? 0 : double.Parse(Rate.Text);

                if (int.TryParse(e.NewTextValue, out int LongLeafPercentage))
                {
                    CalculateFinalWeight(RainLeafValue, LongLeafPercentage, RateValue);

                }
                else
                {
                    
                    CalculateFinalWeight(RainLeafValue, 0, RateValue);
                }
            }
            catch (Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }

       
        public void GetClientFromLocalDB()
        {
            // var ClientDetails = (from x in conn.Table<LocalClientSaveModel>() select x).ToList();
            var ClientDetails=conn.Table<LocalClientSaveModel>()
                            .Select(x => new { x.ClientId, x.ClientName }) // Select the columns you want to be unique
                            .Distinct()
                            .ToList();

            foreach (var _client in ClientDetails)
            {
                clientData.Add(new ClientModel {ClientId= _client.ClientId,ClientName= _client.ClientName });

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
                        clientData.Add(new ClientModel {ClientId= _client.ClientId,ClientName= _client.ClientName });

                    }

                }

            }


        }

        private void ListView_OnItemTapped(Object sender, ItemTappedEventArgs e)
        {
            //EmployeeListView.IsVisible = false;  

            ClientModel listed = e.Item as ClientModel;
            Client.Text = listed.ClientName;
            ClientId= listed.ClientId;
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

        private async void OnViewCollectionClicked(object sender, EventArgs e)
        {
            //bool _back = await DisplayAlert("Info", "Do you want to logout", "Yes", "No");
            //if (_back)
            //{
                await Navigation.PushAsync(new LocalDataShow.DailyCollectionView(lblVehicle.Text,TripId));
                //SessionManager.ClearSession();
                //await Navigation.PushAsync(new MainPage());
           // }
        }
        protected override bool OnBackButtonPressed()
        {
            DisplayConfirmation();
            return true; // Do not continue processing the back button
        }
        async void DisplayConfirmation()
        {

          //  await Navigation.PushAsync(new UserModule.CollectionPage(VehicleNo, TripId));


        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            try
            {
                string _FldWeight = TotalFieldWeight.Text;
                string _ClientName = Client.Text;
                if (string.IsNullOrWhiteSpace(_FldWeight))
                {
                    await DisplayAlert("Validation", "Please Enter Field Weight", "OK");
                    return;
                }
             
                if (string.IsNullOrWhiteSpace(_ClientName) || ClientId==0)
                {
                    await DisplayAlert("Validation", "Please Enter Client", "OK");
                    return;
                }
                else if (Grade.SelectedIndex == -1)
                {
                    await DisplayAlert("Validation", "Please Select Grade", "OK");
                    return;
                }

                int FirstValue = string.IsNullOrWhiteSpace(TotalFieldWeight.Text) ? 0 : int.Parse(TotalFieldWeight.Text);
                int RainValue = string.IsNullOrWhiteSpace(RainLeaf.Text) ? 0 : int.Parse(RainLeaf.Text);
                int LongValue = string.IsNullOrWhiteSpace(LongLeaf.Text) ? 0 : int.Parse(LongLeaf.Text);
                int DeductValue = string.IsNullOrWhiteSpace(Deduction.Text) ? 0 : int.Parse(Deduction.Text);
                int FinalValue = string.IsNullOrWhiteSpace(FinalWeight.Text) ? 0 : int.Parse(FinalWeight.Text);
                Decimal RateValue = string.IsNullOrWhiteSpace(Rate.Text) ? 0 : Decimal.Parse(Rate.Text);
                Decimal FinalAmountValue = string.IsNullOrWhiteSpace(FinalAmount.Text) ? 0 : Decimal.Parse(FinalAmount.Text);

                SaveLocalCollectionModel _collect = new SaveLocalCollectionModel();
                _collect.CollectionDate = DateTime.Now;
                _collect.TripId = TripId;
                _collect.VehicleNo = lblVehicle.Text;
                _collect.ClientId = ClientId;
                _collect.ClientName = Client.Text;
                _collect.FirstWeight = FirstValue;
                _collect.WetLeaf = RainValue;
                _collect.LongLeaf = LongValue;
                _collect.Deduction = DeductValue;
                _collect.FinalWeight = FinalValue;
                _collect.Rate = RateValue;
                var selectedGrade = (GradeModel)Grade.SelectedItem;
                _collect.GrossAmount = FinalAmountValue;
                _collect.GradeId = selectedGrade.GradeId;
                _collect.GradeName = selectedGrade.GradeName;
                _collect.Remarks = Remarks.Text;
                _collect.TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId);
                _collect.Status = "Pending";
                _collect.CollectionBreakList = EntryFieldWeight.Text;
                _collect.CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId);

                try
                {
                    bool _submit = await DisplayAlert("Confirm", "Do you want to submit.", "Yes", "No");
                    if(_submit )
                    {
                        conn.Insert(_collect);
                        await DisplayAlert("Info", "Data is added Successfully. ", "Yes");
                        cleanForm();
                             Grade.SelectedItem = null;
                        //var _finalWgtTotal = GetTotalFinalWeight();
                        //GrossTotalWgt.Text = "Total Final Wgt: " + _finalWgtTotal;
                        GetTotal();
                        FieldEntryLaout.IsVisible = false;
                        BtnFinish.IsVisible = true;
                        CalculateView.IsVisible = true;
                    }
                 
                }
                catch (Exception ex)
                {
                    // throw ex;
                    await DisplayAlert("Error", ex.Message, "Ok");

                }
                finally
                {
                }
            }
            catch(Exception ex)
            {
                //  throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }

            finally
            {

            }

        }

      private async  void TotalBagCount()
        {
            try
            {
                string StrWgtSum = EntryFieldWeight.Text;

                // Remove trailing '+' if it exists
                if (StrWgtSum.EndsWith("+"))
                {
                    StrWgtSum = StrWgtSum.Substring(0, StrWgtSum.Length - 1);
                }
             //   string StrWgtSum = EntryFieldWeight.Text;// "12+33+44+55+66+77+88+66+77+88+99+99+90";
                var numbers = StrWgtSum.Split('+');

                // Convert the array of strings to an array of integers
                var intNumbers = numbers.Select(int.Parse).ToArray();

                // Calculate the count
                int count = intNumbers.Length;

              
                txtBagCount.Text = "Total Bag :" + count;

            }
            catch (Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
            finally
            {

            }
        }
      
        private async void OnBtnFinishClicked(object sender, EventArgs e)
        {
            string _totalWgt = TotalFieldWeight.Text;
            if (string.IsNullOrWhiteSpace(_totalWgt))
            {
                await DisplayAlert("Validation", "Please enter total field weight.", "OK");
                return;
            }
            TotalCollectionOnType();
            FieldEntryLaout.IsVisible = true;
            BtnFinish.IsVisible = false;
            CalculateView.IsVisible = false;
        }
            
        void cleanForm()
        {
            BagCount = 0;
            txtBagCount.Text = "";
            Client.Text = "";
            ClientId = 0;
            InitialVal = 0;
            TotalFieldWeight.Text = "";
            RainLeaf.Text = "";
            LongLeaf.Text = "";
            Deduction.Text = "";
            FinalWeight.Text = "";
            EntryFieldWeight.Text = "";
            Rate.Text = "";
            FinalAmount.Text = "";
            Remarks.Text = "";
            Client.Focus();
        }

        void cleanClientForm()
        {
            BagCount = 0;
            txtBagCount.Text = "";
            EntryFieldWeight.Text = "";
            EntryFieldWeight.Text = "";
            InitialVal = 0;
            TotalFieldWeight.Text = "";
            EntryFieldWeight.Focus();
        }



    }
}