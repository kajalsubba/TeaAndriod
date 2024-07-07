using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BillHistoryPage : ContentPage
    {
        public BillHistoryPage()
        {
            InitializeComponent();
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            //await GetSupplierHistory();
            //if (dataItems != null && dataItems.Any())
            //{
            //    TotalChallanWeight = dataItems.Where(item => item.Status != "Rejected")
            //                    .Sum(item => item.ChallanWeight);
            //    TotalGrossAmount = dataItems.Where(item => item.Status != "Rejected")
            //                    .Sum(item => item.GrossAmount);
            //    AvarageRate = dataItems.Where(item => item.Status != "Rejected")
            //                    .Sum(item => item.Rate);
            //}
            //else
            //{
            //    TotalChallanWeight = 0;
            //    TotalGrossAmount = 0;
            //    AvarageRate = 0;
            //    dataItems.Clear();
            //    await DisplayAlert("Info", "Record is not found !", "OK");
            //}

        }

    }
}