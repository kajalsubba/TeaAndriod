using Plugin.SimpleAudioPlayer;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;
using ZXing.Net.Mobile.Forms;

namespace TeaClient.QRScanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QRScannerPage : ContentPage
    {
        public SQLiteConnection conn;
        UserModel LoginData = new UserModel();
        string VehicleNo = string.Empty;
        int TripId;
        long VehicleFromId;
        public long QrClientId { get; private set; }

        public string QrClientName { get; private set; }

        public QRScannerPage(string _VehicleNo, int _TripId, long _VehicleFromId)
        {
            try
            {
                InitializeComponent();
                LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");

                conn = DependencyService.Get<ISqlLite>().GetConnection();
                conn.CreateTable<LocalClientSaveModel>();
                VehicleNo = _VehicleNo;
                TripId = _TripId;
                VehicleFromId = _VehicleFromId;
                 StartScanningLineAnimation();
                Task.Delay(1000);


                xing.OnScanResult += (result) => Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        validateQR(result.Text);
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception (log it or show an alert)
                        DisplayAlert("Error", ex.Message, "OK");
                    }
                });
            }
            catch(Exception ex)
            {
                 DisplayAlert("Error", ex.Message, "OK");

            }
        }
        private async void OnBtnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

            
        public string GetClientFromLocalDB(long _ClientId)
        {
            try
            {
                var ClientDetails = conn.Table<LocalClientSaveModel>()
               .Where(x => x.ClientId == _ClientId)
               .Select(x => new { x.ClientId, x.ClientName })
               .Distinct()
               .ToList();

                return ClientDetails[0].ClientName;
            }
            catch(Exception ex) 
            {
                DisplayAlert("Error", ex.Message, "OK");
                return string.Empty;
            }
            

        }

        private async void validateQR(string QRString)
        {
            try
            {
                string input = QRString;
                string[] parts = input.Split('/');
             
                if (LoginData.LoginDetails[0].TenantId == Convert.ToInt64(parts[0]))
                {
                    var ClientName = GetClientFromLocalDB(Convert.ToInt64(parts[1]));

                    if (!string.IsNullOrEmpty(ClientName))
                    {
                       
                        var player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
                        player.Load("success.mp3");
                        player.Play();

                        QrClientId = Convert.ToInt64(parts[1]);
                        QrClientName = ClientName;
                        OnDismissed?.Invoke();
                        await Navigation.PopModalAsync();
                    }
                    else
                    {

                        var player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
                        player.Load("error.mp3");
                        player.Play();
                        QrClientId = 0;
                        QrClientName = string.Empty;
                        OnDismissed?.Invoke();
                        await DisplayAlert("Validation", "Client is not exists!", "OK");
                        await Navigation.PopModalAsync();
                    
                    }
                }
                else
                {
                    var player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
                    player.Load("error.mp3");
                    player.Play();
                    QrClientId = 0;
                    QrClientName = string.Empty;
                    OnDismissed?.Invoke();
                    await DisplayAlert("Validation", "This Data does not belongs to Company!", "OK");
                    await Navigation.PopModalAsync();

                }

            }
            catch(Exception ex)
            {
               await DisplayAlert("Error", ex.Message, "OK");
            }

            finally
            {
                await Navigation.PopModalAsync();
            }


        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            xing.IsScanning = true;
            xing.IsVisible = true;
        }
        public Action OnDismissed;

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            xing.IsScanning = false;
            xing.IsVisible = false;  // Optionally hide the scanner view

          
        }
        private async void StartScanningLineAnimation()
        {
            try
            {
                var animation = new Animation(v => scanningLine.TranslationY = v, -150, 150, Easing.Linear);
                animation.Commit(this, "ScanningLineAnimation", 16, 1000, Easing.Linear, (v, c) => scanningLine.TranslationY = 0, () => true);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}