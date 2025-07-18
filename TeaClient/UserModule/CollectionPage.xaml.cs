﻿using Java.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TeaClient.CommonPage;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Java.Lang.Character;


namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CollectionPage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<ClientModel> clientData { get; set; }
        int InitialVal = 0;
        UserModel LoginData = new UserModel();
        long ClientId;
        int TripId;
        int BagCount = 0;
        long VehicleFromId;
        public IList<GradeModel> GradeList { get; set; }
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;

        public CollectionPage(string _VehicleNo, int _TripId, long _VehicleFromId, long? QRClientId = null, string QRClientName = null)
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            clientData = new ObservableCollection<ClientModel>();

            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            VehicleFromId = _VehicleFromId;
            lblVehicle.Text = _VehicleNo;
            TripId = _TripId;
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalClientSaveModel>();
            conn.CreateTable<SaveLocalCollectionModel>();

            GetClientFromLocalDB();
            GetTotal();

            if (QRCheck.IsChecked)
            {
                QRButton.IsEnabled = true;
                Client.IsReadOnly = true;
            }
            else
            {
                QRButton.IsEnabled = false;
                Client.IsReadOnly = false;
            }

        }
        private void QRCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                QRButton.IsEnabled = true;
                Client.IsReadOnly = true;
            }
            else
            {
                QRButton.IsEnabled = false;
                Client.IsReadOnly = false;
            }
        }
        void PatchClientByQR(long? _ClientId, string _ClientName)
        {
            if (_ClientId != 0)
            {

                Client.Text = _ClientName;
                ClientId = _ClientId ?? 0;
                ClientListView.IsVisible = false;

            }
            else
            {
                Client.Text = "";
                ClientId = 0;
                ClientListView.IsVisible = false;
            }
        }

        void GetTotal()
        {
            int tenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId);
            int userId = Convert.ToInt32(LoginData.LoginDetails[0].UserId);
            DateTime collectionDate = DateTime.Now.Date;

            var CollectionDetails = (from x in conn.Table<SaveLocalCollectionModel>()
                                     where x.TenantId == tenantId
                                      && x.CollectionDate == collectionDate
                                      && x.CreatedBy == userId
                                      && x.DataSendToServer == false
                                      && x.CollectionType == "Self"
                                     select x).ToList();

            var _recordTotal = CollectionDetails.Count();
            TotalRecord.Text = "Record:" + _recordTotal;

            var _firstWgtTotal = CollectionDetails.Sum(x => x.FirstWeight);
            TotalFirstWgt.Text = "Total Field:" + _firstWgtTotal;

            var _deductionTotal = CollectionDetails.Sum(x => x.Deduction);
            TotalDeducttion.Text = "Total Deduct:" + _deductionTotal;

            var _finalWgtTotal = CollectionDetails.Sum(x => x.FinalWeight);
            GrossTotalWgt.Text = "Total Final: " + _finalWgtTotal;

        }

        private async void OnNumberButtonClicked(object sender, EventArgs e)
        {
            VibrationMethod();
            string _ClientName = Client.Text;
            if (string.IsNullOrWhiteSpace(_ClientName) || ClientId == 0)
            {
                await DisplayAlert("Validation", "Please Enter Client", "OK");
                return;
            }
            var button = sender as Button;
            if (button != null)
            {
                EntryFieldWeight.Text += button.Text;
            }
        }

        private async void OnQRClicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new QRScanner.QRScannerPage(lblVehicle.Text,TripId,VehicleFromId));
            try
            {
                var QrPage = new QRScanner.QRScannerPage(lblVehicle.Text, TripId, VehicleFromId);
                QrPage.OnDismissed += () =>
                {
                    PatchClientByQR(QrPage.QrClientId, QrPage.QrClientName);

                };

                await Navigation.PushModalAsync(QrPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }
        private void OnOperatorButtonClicked(object sender, EventArgs e)
        {
            VibrationMethod();

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

            VibrationMethod();
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
            txtBagCount.Text = string.Empty;
        }
        private async void OnStepCleanButtonClicked(object sender, EventArgs e)
        {
            VibrationMethod();
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
            catch (Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }

        private async void CalculateFinalWeight(int RainPercentage, int longLeafPercentage, double rate)
        {
            try
            {
                int FirstWeightValue = string.IsNullOrWhiteSpace(TotalFieldWeight.Text) ? 0 : int.Parse(TotalFieldWeight.Text);

                var RainResult = Math.Round((double)(RainPercentage * FirstWeightValue) / 100);
                var LongResult = Math.Round((double)(longLeafPercentage * FirstWeightValue) / 100);
                Deduction.Text = (RainResult + LongResult).ToString();
                FinalWeight.Text = (FirstWeightValue - RainResult - LongResult).ToString();

                FinalAmount.Text = (Convert.ToInt16(FinalWeight.Text) * rate).ToString();
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            //EmployeeListView.IsVisible = false;  

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

        private async void OnViewCollectionClicked(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new LocalDataShow.DailyCollectionView(lblVehicle.Text, TripId, VehicleFromId));

        }
        protected override bool OnBackButtonPressed()
        {

            return true; // Do not continue processing the back button
        }

        private int DuplicateDataValidation()
        {
            int FinalValue = string.IsNullOrWhiteSpace(FinalWeight.Text) ? 0 : int.Parse(FinalWeight.Text);

            var selectedGrade = (GradeModel)Grade.SelectedItem;

            var CollectionList = (from x in conn.Table<SaveLocalCollectionModel>()
                                  where x.ClientId == ClientId
                                  && x.CollectionDate == DateTime.Now.Date
                                   && x.FinalWeight == FinalValue
                                   && x.GradeId == selectedGrade.GradeId
                                  select x).ToList();

            return CollectionList.Count;
        }

        public async void VibrationMethod()
        {
            try
            {
                // Use default vibration length
                Vibration.Vibrate();

                // Or use specified time
                var duration = TimeSpan.FromMilliseconds(100);
                Vibration.Vibrate(duration);
            }
            catch (FeatureNotSupportedException ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            try
            {
                Submitbtn.IsEnabled = false;
                string _FldWeight = TotalFieldWeight.Text;
                string _ClientName = Client.Text;
                if (string.IsNullOrWhiteSpace(_FldWeight))
                {
                    await DisplayAlert("Validation", "Please Enter Field Weight", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_ClientName) || ClientId == 0)
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
                _collect.CollectionDate = DateTime.Now.Date;
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
                _collect.DataSendToServer = false;
                _collect.CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId);
                _collect.TransferFrom = 0;
                _collect.VehicleFrom = VehicleFromId;
                _collect.FinishTimeInApp = DateTime.Now;
                if (EntryFieldWeight != null)
                {
                    string expression = EntryFieldWeight.Text;

                    // Remove trailing '+' if it exists
                    if (expression.EndsWith("+"))
                    {
                        expression = expression.Substring(0, expression.Length - 1);
                    }
                    _collect.BagList = expression;
                }

                _collect.CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId);

                try
                {
                    var dataVal = DuplicateDataValidation();
                    if (dataVal == 0)
                    {
                        conn.Insert(_collect);
                        bool _submit = await DisplayAlert("Save", "Data is saved successfully.Do you want to Print!", "Yes", "No");
                        if (_submit)
                        {
                            PrintService prt = new PrintService();
                            var deduct = DeductValue.ToString() == "0" ? "Pending" : DeductValue.ToString();
                            var result = await prt.PrintParameters(_collect.CollectionDate.ToString("dd/MM/yyyy"), LoginData.LoginDetails[0].CompanyName.ToString().Trim(), _collect.ClientName, _collect.GradeName, _collect.BagList, deduct, _collect.FirstWeight.ToString(),
                           _collect.FinalWeight.ToString(), _collect.GrossAmount.ToString(), _collect.Remarks, LoginData.LoginDetails[0].UserFirstName + " " + LoginData.LoginDetails[0].UserLastName);

                            await DisplayAlert("Info", result, "Ok");

                        }
                    }


                }
                catch (Exception ex)
                {
                    // throw ex;
                    await DisplayAlert("Error", ex.Message, "Ok");

                }
                finally
                {
                    //  await DisplayAlert("Info", "Data is added Successfully. ", "Ok");
                    cleanForm();
                    Grade.SelectedItem = null;
                    GetTotal();
                    FieldEntryLaout.IsVisible = false;
                    BtnFinish.IsVisible = true;
                    CalculateView.IsVisible = true;
                    Submitbtn.IsEnabled = true;
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

        private async void TotalBagCount()
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

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserModule.UserDashboardPage());
        }


    }
}