using Microsoft.Net.Http.Headers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SQLLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Content.ClipData;

namespace TeaClient.LocalDataShow
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DailyCollectionView : ContentPage
	{
        public SQLiteConnection conn;
        public SaveLocalCollectionModel _CollectionModel;
        readonly string VehicleNo=string.Empty;
        readonly int TripId;
        public IList<SaveLocalCollectionModel> dataItems { get; set; }
        public DailyCollectionView (string _VehicleNo,int _Trip)
		{
			InitializeComponent ();
            dataItems = new ObservableCollection<SaveLocalCollectionModel>();
            BindingContext = this;
            VehicleNo = _VehicleNo;
            TripId = _Trip;
            NavigationPage.SetHasBackButton(this, false);
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<SaveLocalCollectionModel>();
            // CollectionData();
            HeaderName.Text = DateTime.Now.ToString("dd/MM/yyyy");
            DisplayDetails();
            //var _finalWgtTotal = GetTotalFinalWeight();
            //FinalTotal.Text = "Total Final Wgt: " + _finalWgtTotal;
            GetTotal();
        }
        protected override bool OnBackButtonPressed()
        {
            DisplayConfirmation();
            return true; // Do not continue processing the back button
        }
        async void DisplayConfirmation()
        {

            await Navigation.PushAsync(new UserModule.CollectionPage(VehicleNo, TripId));


        }
        public void CollectionData()
        {

            var CollectionDetails = (from x in conn.Table<SaveLocalCollectionModel>() select x).ToList();

            foreach (var _collect in CollectionDetails)
            {

                dataItems.Add(new SaveLocalCollectionModel
                {
                    ClientName = _collect.ClientName,
                    GradeName=_collect.GradeName,
                    FirstWeight = _collect.FinalWeight,
                    Deduction = _collect.Deduction,
                    FinalWeight = _collect.FinalWeight,
                    Rate = _collect.Rate,
                    GrossAmount = _collect.GrossAmount,
                    Remarks = _collect.Remarks,
                   
                });
            }
        }
        void GetTotal()
        {

            var _recordTotal = GetTotalRecord();
            TotalRecord.Text = "Total Record:" + _recordTotal;

            var _firstWgtTotal = GetTotalFirstWeight();
            TotalFirstWgt.Text = "Total Field:" + _firstWgtTotal;

            var _deductionTotal = GetTotalDeduction();
            TotalDeducttion.Text = "Total Deduct:" + _deductionTotal;


            var _finalWgtTotal = GetTotalFinalWeight();
            FinalTotal.Text = "Total Final: " + _finalWgtTotal;

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
        public void DisplayDetails()
        {

            var CollectionDetails = (from x in conn.Table<SaveLocalCollectionModel>() select x).ToList();
            myListView.ItemsSource = CollectionDetails;
        }
        private async void OnEditButtonClicked(object sender, EventArgs e)
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
                    await Navigation.PushAsync(new UserModule.EditCollectionPage(item));
                }
            }
            catch(Exception ex)
            {
                // throw ex;
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }
 
        

        private async void OnSendToServerClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "This page is under construction. ", "Ok");

        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserModule.CollectionPage(VehicleNo,TripId));

        }
        

    }
}