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

            var _grossWgtTotal = GetTotalFinalWeight();
            GrossTotalWgt.Text ="Gross Wgt: "+ _grossWgtTotal;


        }
        public int GetTotalFinalWeight()
        {
            // Compute the sum of FinalWeight
            var sum = conn.ExecuteScalar<int>("SELECT SUM(FinalWeight) FROM SaveLocalCollectionModel");
            return sum;
        }

        private void CheckDisplayWgt_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value) // e.Value is true if the CheckBox is checked
            {
                DisplayWgt.IsReadOnly = false;
               
            }
            else
            {
                DisplayWgt.IsReadOnly = true;
               
            }
        }
        private async void OnAddClicked(object sender, EventArgs e)
        {
            DisplayWgt.IsReadOnly = true;
            int newWeight;
            string _Client = Client.Text;
            if (string.IsNullOrWhiteSpace(_Client) || ClientId == null || ClientId <= 0)
            {
                await DisplayAlert("Validation", "Please Select Client", "OK");
                Client.Focus();
                return;
            }
            if (!int.TryParse(FieldWeight.Text, out newWeight))
            {
                await DisplayAlert("Validation", "Please enter a field weight.", "Yes");
                FieldWeight.Focus();
                return;
            }

          
            // If TotalWgt is not empty, prepend a '+' before adding the new weight
            if (!string.IsNullOrWhiteSpace(DisplayWgt.Text))
            {
                DisplayWgt.Text += "+" + FieldWeight.Text;

            }
            else
            {
                // If TotalWgt is empty, just add the new weight without a '+'
                DisplayWgt.Text = FieldWeight.Text;

            }
            InitialVal += newWeight;
            TotalWeight.Text = InitialVal.ToString();

            // Clear the FieldWeight entry after appending the value
            FieldWeight.Text = string.Empty;
            FinalWeight.Text = TotalWeight.Text;
            FieldWeight.Focus();
        }

        private void RainLeaf_TextChanged(object sender, TextChangedEventArgs e)
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
                throw ex;
            }
        }

        private void CalculateFinalWeight(int RainPercentage,int longLeafPercentage,double rate)
        {
            try
            {
                int FirstWeightValue = string.IsNullOrWhiteSpace(TotalWeight.Text) ? 0 : int.Parse(TotalWeight.Text);

                var RainResult = (RainPercentage * FirstWeightValue) / 100; // Example calculation
                var LongResult = (longLeafPercentage * FirstWeightValue) / 100; // Example calculation
                Deduction.Text = (RainResult + LongResult).ToString();
                FinalWeight.Text = (FirstWeightValue - RainResult - LongResult).ToString();

                FinalAmount.Text = (Convert.ToInt16(FinalWeight.Text) * rate).ToString();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        private void Rate_TextChanged(object sender, TextChangedEventArgs e)
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
                throw ex;
            }
        }

        
        private void LongLeaf_TextChanged(object sender, TextChangedEventArgs e)
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
                throw ex;
            }
        }

       
        public void GetClientFromLocalDB()
        {
            var ClientDetails = (from x in conn.Table<LocalClientSaveModel>() select x).ToList();
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
        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
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
                throw ex;
            }
            ClientListView.EndRefresh();

        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            //bool _back = await DisplayAlert("Info", "Do you want to logout", "Yes", "No");
            //if (_back)
            //{
                await Navigation.PushAsync(new LocalDataShow.DailyCollectionView());
                //SessionManager.ClearSession();
                //await Navigation.PushAsync(new MainPage());
           // }
        }
         private async void OnClearClicked(object sender, EventArgs e)
        {
            cleanForm();
        }
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            string FieldWeight = TotalWeight.Text;
            if (string.IsNullOrWhiteSpace(FieldWeight))
            {
                await DisplayAlert("Validation", "Please Enter Field Weight", "OK");
                return;
            }
          
            else if (Grade.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Grade", "OK");
                return;
            }

            SaveLocalCollectionModel _collect= new SaveLocalCollectionModel();
            _collect.CollectionDate = DateTime.Now;
            _collect.TripId = TripId;
            _collect.VehicleNo = lblVehicle.Text;
            _collect.ClientId = ClientId;
            _collect.ClientName = Client.Text;
            _collect.FinalWeight = Convert.ToInt16(TotalWeight.Text);
            _collect.WetLeaf = Convert.ToInt16(RainLeaf.Text);
            _collect.LongLeaf = Convert.ToInt16(LongLeaf.Text);

            _collect.Deduction = Convert.ToInt16(Deduction.Text);
            _collect.FinalWeight = Convert.ToInt16(FinalWeight.Text);
            _collect.Rate = Convert.ToDecimal(Rate.Text);

            var selectedGrade = (GradeModel)Grade.SelectedItem;


            _collect.GrossAmount =Convert.ToDecimal(FinalAmount.Text);
            _collect.GradeId = selectedGrade.GradeId;
            _collect.GradeName= selectedGrade.GradeName;
            _collect.Remarks = Remarks.Text;


            _collect.TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId);
            _collect.Status = "Pending";
            _collect.CollectionBreakList = DisplayWgt.Text;
            _collect.CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId);

            try
            {
                conn.Insert(_collect);
                await DisplayAlert("Info", "Data is added Successfully. ", "Yes");
                cleanForm();
                var _grossWgtTotal = GetTotalFinalWeight();
                GrossTotalWgt.Text = "Gross Wgt: " + _grossWgtTotal;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
               // conn.Close();
            }

        }

        void cleanForm()
        {
            Client.Text = "";
            ClientId = 0;
            TotalWeight.Text = "";
            RainLeaf.Text = "";
            LongLeaf.Text = "";
            Deduction.Text = "";
            FinalWeight.Text = "";
            DisplayWgt.Text = "";
            Rate.Text = "";
            FinalAmount.Text = "";
            Remarks.Text = "";
            Client.Focus();
        }




    }
}