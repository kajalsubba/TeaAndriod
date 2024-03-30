using Android.App;
using Java.Util;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SessionHelper;
using TeaClient.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Java.Util.Jar.Attributes;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SupplierPage : ContentPage
    {
        public IList<FactoryAccountModel> FactoryAccountlists { get; set; }
        // public IList<VehicleModel> Vehiclelists { get; set; }
        string FilePath = "";
        ObservableCollection<string> data = new ObservableCollection<string>();
        ClientLoginData LoginData = new ClientLoginData();
        public IList<FactoryAccountModel> Accountlists { get; set; }
        public SupplierPage()
        {
            InitializeComponent();
            Accountlists = new ObservableCollection<FactoryAccountModel>();
            GetFactoryAccount();
            GetVehicle();
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
            HeaderName.Text = "Welcome " + LoginData.ClientLoginDetails[0].ClientName;
        }
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            SaveSupplier();
        }
        public async void ListOfStore() //List of Countries  
        {
            try
            {
                data.Add("Afghanistan");
                data.Add("Austria");
                data.Add("Australia");
                data.Add("Azerbaijan");
                data.Add("Bahrain");
                data.Add("Bangladesh");
                data.Add("Belgium");
                data.Add("Botswana");
                data.Add("China");
                data.Add("Colombia");
                data.Add("Denmark");
                data.Add("Kmart");
                data.Add("Pakistan");
            }
            catch (Exception ex)
            {
                await DisplayAlert("", "" + ex, "Ok");
            }
        }
        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            VehicleListView.IsVisible = true;
            VehicleListView.BeginRefresh();

            try
            {
                var dataEmpty = data.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));

                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                    VehicleListView.IsVisible = false;
                else if (dataEmpty.Max().Length == 0)
                    VehicleListView.IsVisible = false;
                else
                    VehicleListView.ItemsSource = data.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
                VehicleListView.IsVisible = false;

            }
            VehicleListView.EndRefresh();

        }
        private void ListView_OnItemTapped(Object sender, ItemTappedEventArgs e)
        {
            //EmployeeListView.IsVisible = false;  

            String listsd = e.Item as string;
            VehicleNo.Text = listsd;
            VehicleListView.IsVisible = false;

            ((ListView)sender).SelectedItem = null;
        }
        private void OnFactotyIndexChanged(object sender, EventArgs e)
        {

            var selectedFactory = FactoryName.SelectedItem as FactoryModel;
            IList<FactoryAccountModel> filteredList = Accountlists.Where(p => p.FactoryId == selectedFactory.FactoryId).ToList();

            AccountName.ItemsSource = (System.Collections.IList)filteredList;
            AccountName.ItemDisplayBinding = new Binding("AccountName");

        }
        public async void SaveSupplier()
        {
           // await UploadChallan(0);
            string vehicle = VehicleNo.Text;
            string challan = ChallanWgt.Text;
            if (string.IsNullOrWhiteSpace(vehicle))
            {
                await DisplayAlert("Validation","Please Enter Vehicle", "OK");
                return;
            }
            else if (string.IsNullOrWhiteSpace(challan))
            {
                await DisplayAlert("Validation", "Please Enter Challan Weight", "OK");
                return;
            }
            else if (FactoryName.SelectedIndex==-1)
            {
                await DisplayAlert("Validation", "Please Select Factory", "OK");
                return;
            }
            else if (AccountName.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Account", "OK");
                return;
            }

            string url = "http://72.167.37.70:82/Collection/SaveSupplier";

            var selectedAccount = AccountName.SelectedItem as FactoryAccountModel;
            DateTime selectedDate = collectionDate.Date;
            string formattedDate = selectedDate.ToString("yyyy-MM-dd");

            // Create an object to be sent as JSON in the request body
            SupplierModel dataToSend = new SupplierModel
            {
                CollectionId = 0,
                CollectionDate = Convert.ToDateTime(formattedDate),
                VehicleNo = VehicleNo.Text,
                ClientId = Convert.ToInt32(LoginData.ClientLoginDetails[0].ClientId),
                AccountId = selectedAccount.AccountId,
                FineLeaf = Convert.ToInt32(FineLeaf.Text ?? "0"),
                ChallanWeight = Convert.ToInt32(ChallanWgt.Text),
                Rate = 0,
                GrossAmount = 0,
                TripId = 1,
                Status = "Pending",
                Remarks = remarksEditor.Text ?? "",
                TenantId = Convert.ToInt32(LoginData.ClientLoginDetails[0].TenantId),
                CreatedBy = Convert.ToInt32(LoginData.ClientLoginDetails[0].UserId)
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var valueData = JsonConvert.DeserializeObject<SaveReturn>(results);

                    if (valueData.Id != 0)
                    {
                       
                      await UploadChallan(valueData.Id);
                      await DisplayAlert("Success", valueData.Message, "OK");
                      clearFormControl();
                    }
                    else
                    {
                        await DisplayAlert("Info", valueData.Message, "OK");
                    }

                }

            }
        }
        public Stream ConvertImagePathToStream(string imagePath)
        {
            try
            {
                // Check if the file exists
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine("Image file does not exist.");
                    return null;
                }

                // Open the file stream
                using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    // Create a MemoryStream to store the image data
                    MemoryStream memoryStream = new MemoryStream();

                    // Copy the contents of the file stream to the memory stream
                    fileStream.CopyTo(memoryStream);

                    // Reset the memory stream position to the beginning
                    memoryStream.Position = 0;

                    // Return the memory stream
                    return memoryStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting image path to stream: " + ex.Message);
                return null;
            }
        }
        private async Task  UploadChallan(long collId)
        {
            string url = "http://72.167.37.70:83/Collection/UploadSupplierChallan";
            // Load the image from the embedded resource
            // string imagePath = "YourNamespace.Images.your_image.png";
          //  ImageConverter imageConverter = new ImageConverter();

            Stream imageStream = ConvertImagePathToStream(FilePath);




            if (imageStream == null)
            {
                Console.WriteLine("Image not found.");
                return;
            }

            // Create a HttpClient instance
            using (HttpClient httpClient = new HttpClient())
            {
                using (MultipartFormDataContent formData = new MultipartFormDataContent())
                {
                    // Add image data
                    formData.Add(new StreamContent(imageStream), "ChallanImage", "your_image.png");

                    // Add additional form data
                    formData.Add(new StringContent(LoginData.ClientLoginDetails[0].TenantId.ToString()), "TenantId");
                    formData.Add(new StringContent(collId.ToString()), "CollectionId");

                    try
                    {
                   
                        HttpResponseMessage response = await httpClient.PostAsync(url, formData);
                        response.EnsureSuccessStatusCode();

                        // If needed, read the response content
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response: " + responseBody);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine("HTTP Request Error: " + ex.Message);
                    }
                }
            }
        }
        
        public void clearFormControl()
        {
            VehicleNo.Text = "";
           // FactoryName.SelectedIndex = -1;
            //AccountName.SelectedIndex = -1;
            FineLeaf.Text = "";
            ChallanWgt.Text = "";
            remarksEditor.Text = "";
        }
        public async void GetFactoryAccount()
        {

            string url = "http://72.167.37.70:82/Master/GetFactoryAccount";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                IsClientView = true,
                TenantId = 1
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<FactoryAccountList>(results);
                    foreach (var factory in data.AccountDetails)
                    {

                        Accountlists.Add(new FactoryAccountModel { FactoryId = factory.FactoryId, AccountId = factory.AccountId, AccountName = factory.AccountName });
                    }

                }
                //AccountName.ItemsSource = (System.Collections.IList)Accountlists;
                //AccountName.ItemDisplayBinding = new Binding("AccountName");

            }


        }

        public async void GetVehicle()
        {

            string url = "http://72.167.37.70:82/Master/GetVehicle";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                IsClientView = true,
                TenantId = 1
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var valueData = JsonConvert.DeserializeObject<VehicleList>(results);
                    foreach (var vehicle in valueData.VehicleDetails)
                    {
                        data.Add(vehicle.VehicleNo);

                      //  Vehiclelists.Add(new VehicleModel { VehicleId = vehicle.VehicleId, VehicleNo = vehicle.VehicleNoe });
                    }

                }
        
            }


        }
        private async void OnCaptureClicked(object sender, EventArgs e)
        {
           await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsTakePhotoSupported && !CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "Format is not supported", "ok");
                return;
            }
            else
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Images",
                    Name = DateTime.Now.ToString() + ".jpg"
                }) ; 
                if (file==null)
                {
                    return;
                  

                }
                FilePath = file.Path;
                await DisplayAlert("File Path ", file.Path, "ok");
                ChallanImage.Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    return stream;
                });

            }
        }

        

    }
}