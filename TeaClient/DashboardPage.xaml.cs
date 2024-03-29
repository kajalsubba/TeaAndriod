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
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
        }
        private async void OnSupplier_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SupplierPage());
        }
        private void OnSupplierView_Clicked(object sender, EventArgs e)
        {

        }

    }
}