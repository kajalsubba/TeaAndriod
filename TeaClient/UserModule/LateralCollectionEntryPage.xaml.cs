using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
	public partial class LateralCollectionEntryPage : ContentPage
	{
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<ClientModel> clientData { get; set; }
        int InitialVal = 0;
        UserModel LoginData = new UserModel();
        readonly SaveLocalCollectionModel collect;

        int TripId;
        int BagCount = 0;
        long VehicleFromId;
        public IList<GradeModel> GradeList { get; set; }
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;

        public LateralCollectionEntryPage (SaveLocalCollectionModel _collect)
		{
			InitializeComponent ();
            NavigationPage.SetHasBackButton(this, false);
            clientData = new ObservableCollection<ClientModel>();

            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            collect = _collect;
            VehicleFromId = _collect.VehicleFrom;
            lblVehicle.Text = _collect.VehicleNo;
            Client.Text = _collect.ClientName;
         //   ClientId.Text =_collect.ClientId.ToString();
            TripId = _collect.TripId;
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalClientSaveModel>();
            conn.CreateTable<SaveLocalCollectionModel>();

           // GetClientFromLocalDB();
            GetTotal();
        }

        void PatchCollection()
        {
            int FirstValue = string.IsNullOrWhiteSpace(collect.FinalWeight.ToString()) ? 0 : int.Parse(collect.FinalWeight.ToString());
            int RainValue = string.IsNullOrWhiteSpace(collect.WetLeaf.ToString()) ? 0 : int.Parse(collect.WetLeaf.ToString());
            int LongValue = string.IsNullOrWhiteSpace(collect.LongLeaf.ToString()) ? 0 : int.Parse(collect.LongLeaf.ToString());
            int DeductValue = string.IsNullOrWhiteSpace(collect.Deduction.ToString()) ? 0 : int.Parse(collect.Deduction.ToString());
            int FinalValue = string.IsNullOrWhiteSpace(collect.FinalWeight.ToString()) ? 0 : int.Parse(collect.FinalWeight.ToString());
            Decimal RateValue = string.IsNullOrWhiteSpace(collect.Rate.ToString()) ? 0 : Decimal.Parse(collect.Rate.ToString());
            Decimal FinalAmountValue = string.IsNullOrWhiteSpace(collect.GrossAmount.ToString()) ? 0 : Decimal.Parse(collect.GrossAmount.ToString());

            Client.Text = collect.ClientName;
            TotalFieldWeight.Text = FirstValue.ToString();
            RainLeaf.Text = RainValue.ToString();
            LongLeaf.Text = LongValue.ToString();
            Deduction.Text = DeductValue.ToString();
            FinalWeight.Text = FinalValue.ToString();
            Rate.Text = RateValue.ToString();
            FinalAmount.Text = FinalAmountValue.ToString();
            Remarks.Text = collect.Remarks ?? "";

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
            TotalRecord.Text = "Total Record:" + _recordTotal;

            var _firstWgtTotal = CollectionDetails.Sum(x => x.FirstWeight);
            TotalFirstWgt.Text = "Total Field:" + _firstWgtTotal;

            var _deductionTotal = CollectionDetails.Sum(x => x.Deduction);
            TotalDeducttion.Text = "Total Deduct:" + _deductionTotal;

            var _finalWgtTotal = CollectionDetails.Sum(x => x.FinalWeight);
            GrossTotalWgt.Text = "Total Final: " + _finalWgtTotal;

        }
        private async void OnNumberButtonClicked(object sender, EventArgs e)
        {
            string _ClientName = Client.Text;
            if (string.IsNullOrWhiteSpace(_ClientName) )
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

                if (string.IsNullOrWhiteSpace(_ClientName))
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

                SaveLocalCollectionModel _collectNew = new SaveLocalCollectionModel();
                _collectNew.CollectionDate = DateTime.Now.Date;
                _collectNew.TripId = TripId;
                _collectNew.VehicleNo = lblVehicle.Text;
                _collectNew.ClientId =collect.ClientId;
                _collectNew.ClientName = Client.Text;
                _collectNew.FirstWeight = FirstValue;
                _collectNew.WetLeaf = RainValue;
                _collectNew.LongLeaf = LongValue;
                _collectNew.Deduction = DeductValue;
                _collectNew.FinalWeight = FinalValue;
                _collectNew.Rate = RateValue;
                var selectedGrade = (GradeModel)Grade.SelectedItem;
                _collectNew.GrossAmount = FinalAmountValue;
                _collectNew.GradeId = selectedGrade.GradeId;
                _collectNew.GradeName = selectedGrade.GradeName;
                _collectNew.Remarks = Remarks.Text;
                _collectNew.TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId);
                _collectNew.Status = "Pending";
                _collectNew.DataSendToServer = false;
                _collectNew.CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId);
                _collectNew.TransferFrom = 0;
                _collectNew.VehicleFrom = VehicleFromId;
                if (EntryFieldWeight != null)
                {
                    string expression = EntryFieldWeight.Text;

                    // Remove trailing '+' if it exists
                    if (expression.EndsWith("+"))
                    {
                        expression = expression.Substring(0, expression.Length - 1);
                    }
                    _collectNew.BagList = expression;
                }

                _collectNew.CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId);

                try
                {
                    bool _submit = await DisplayAlert("Confirm", "Do you want to submit.", "Yes", "No");
                    if (_submit)
                    {
                        conn.Insert(_collectNew);
                        await DisplayAlert("Info", "Data is added Successfully. ", "Yes");
                 
                        Grade.SelectedItem = null;
                        //var _finalWgtTotal = GetTotalFinalWeight();
                        //GrossTotalWgt.Text = "Total Final Wgt: " + _finalWgtTotal;
                        GetTotal();
                        FieldEntryLaout.IsVisible = false;
                        BtnFinish.IsVisible = true;
                        CalculateView.IsVisible = true;
                        await Navigation.PushAsync(new LocalDataShow.DailyCollectionView(collect.VehicleNo, collect.TripId, collect.VehicleFrom));

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
            catch (Exception ex)
            {
                //  throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }

            finally
            {

            }

        }
        private async void OnViewCollectionClicked(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new LocalDataShow.DailyCollectionView(lblVehicle.Text, TripId, VehicleFromId));

        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserModule.UserDashboardPage());
        }

    }
}