using Android.Webkit;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SQLLite;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditCollectionPage : ContentPage
    {

        readonly SaveLocalCollectionModel collect;
        public SQLiteConnection conn;
        public EditCollectionPage(SaveLocalCollectionModel _collect)
        {
            InitializeComponent();
            collect= _collect;
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TeaCollection.db3");
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<SaveLocalCollectionModel>();
            PatchCollection();
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
            RainLeaf.Text= RainValue.ToString();
            LongLeaf.Text = LongValue.ToString();
            Deduction.Text= DeductValue.ToString();
            FinalWeight.Text= FinalValue.ToString();
            Rate.Text = RateValue.ToString();
            FinalAmount.Text= FinalAmountValue.ToString();
            Remarks.Text=collect.Remarks??"";

        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            try
            {
            int FirstValue = string.IsNullOrWhiteSpace(TotalFieldWeight.Text) ? 0 : int.Parse(TotalFieldWeight.Text);
            int RainValue = string.IsNullOrWhiteSpace(RainLeaf.Text) ? 0 : int.Parse(RainLeaf.Text);
            int LongValue = string.IsNullOrWhiteSpace(LongLeaf.Text) ? 0 : int.Parse(LongLeaf.Text);
            int DeductValue = string.IsNullOrWhiteSpace(Deduction.Text) ? 0 : int.Parse(Deduction.Text);
            int FinalValue = string.IsNullOrWhiteSpace(FinalWeight.Text) ? 0 : int.Parse(FinalWeight.Text);
            Decimal RateValue = string.IsNullOrWhiteSpace(Rate.Text) ? 0 : Decimal.Parse(Rate.Text);
            Decimal FinalAmountValue = string.IsNullOrWhiteSpace(FinalAmount.Text) ? 0 : Decimal.Parse(FinalAmount.Text);

          
            var existingCollect = conn.Find<SaveLocalCollectionModel>(collect.Id);
            existingCollect.FirstWeight = FirstValue;
            existingCollect.WetLeaf = RainValue;
            existingCollect.LongLeaf = LongValue;
            existingCollect.Deduction = DeductValue;
            existingCollect.FinalWeight = FinalValue;
            existingCollect.Rate = RateValue;
            existingCollect.GrossAmount = FinalAmountValue;
            existingCollect.Remarks = Remarks.Text;
           
                bool _submit = await DisplayAlert("Confirm", "Do you want to update!", "Yes", "No");
                if (_submit)
                {
                    conn.Update(existingCollect);
                    await DisplayAlert("Info", "Data is updated Successfully ", "Ok");
                    await Navigation.PushAsync(new LocalDataShow.DailyCollectionView(collect.VehicleNo,collect.TripId));

                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

               // throw ex;
            }
            finally
            {
            }
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CalculateFinalWeight(int RainPercentage, int longLeafPercentage, double rate)
        {
            try
            {
                int FirstWeightValue = string.IsNullOrWhiteSpace(TotalFieldWeight.Text) ? 0 : int.Parse(TotalFieldWeight.Text);

                //var RainResult = (RainPercentage * FirstWeightValue) / 100; // Example calculation
                //var LongResult = (longLeafPercentage * FirstWeightValue) / 100; // Example calculation
                var RainResult = Math.Round((double)(RainPercentage * FirstWeightValue) / 100);
                var LongResult = Math.Round((double)(longLeafPercentage * FirstWeightValue) / 100);

                Deduction.Text = (RainResult + LongResult).ToString();
                FinalWeight.Text = (FirstWeightValue - RainResult - LongResult).ToString();

                FinalAmount.Text = (Convert.ToInt16(FinalWeight.Text) * rate).ToString();
            }
            catch (Exception ex)
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
            catch (Exception ex)
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


    }
}