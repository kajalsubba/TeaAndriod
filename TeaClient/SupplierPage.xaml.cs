using Android.App;
using Java.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Stormlion.ImageCropper;
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
using TeaClient.CommonPage;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Java.Util.Jar.Attributes;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SupplierPage : ContentPage
    {
        readonly AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<FactoryAccountModel> FactoryAccountlists { get; set; }
        string FilePath = "";
        readonly ObservableCollection<string> data = new ObservableCollection<string>();
        readonly ClientLoginData LoginData = new ClientLoginData();

        // private MediaFile _selectedImageFile;

        public IList<FactoryAccountModel> Accountlists { get; set; }
        public SupplierPage()
        {
            InitializeComponent();
            CrossMedia.Current.Initialize();
            Accountlists = new ObservableCollection<FactoryAccountModel>();
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
            GetFactoryAccount();
            GetVehicle();

            // HeaderName.Text = "Welcome " + LoginData.ClientLoginDetails[0].ClientName;
        }
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            var loadingPage = new LoadingPage();
            await Navigation.PushModalAsync(loadingPage);
            await SaveSupplierUsingMQ();
           // await Navigation.PopModalAsync();

        }
        protected override bool OnBackButtonPressed()
        {
            //  DisplayConfirmation();
            return true; // Do not continue processing the back button
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
        public async Task SaveSupplier()
        {
            // await UploadChallan(0);
            string vehicle = VehicleNo.Text;
            string challan = ChallanWgt.Text;
            if (string.IsNullOrWhiteSpace(vehicle))
            {
                await DisplayAlert("Validation", "Please Enter Vehicle", "OK");
                return;
            }
            else if (string.IsNullOrWhiteSpace(challan))
            {
                await DisplayAlert("Validation", "Please Enter Challan Weight", "OK");
                return;
            }
            else if (FactoryName.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Factory", "OK");
                return;
            }
            else if (AccountName.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Account", "OK");
                return;
            }

            Stream imageStream = ConvertImagePathToStream(FilePath);

            if (imageStream == null)
            {
                await DisplayAlert("Validation", "Please Upload challan Copy", "OK");
                return;
            }


            string url = _appSetting.ApiUrl + "Collection/SaveSupplier";

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
                        ChallanImage.Source = null;

                        clearFormControl();
                    }
                    else
                    {
                        await DisplayAlert("Info", valueData.Message, "OK");
                    }

                }

            }
        }

        public async Task SaveSupplierUsingMQ()
        {

            string vehicle = VehicleNo.Text;
            string challan = ChallanWgt.Text;
            if (string.IsNullOrWhiteSpace(vehicle))
            {
                await DisplayAlert("Validation", "Please Enter Vehicle", "OK");
                return;
            }
            else if (string.IsNullOrWhiteSpace(challan))
            {
                await DisplayAlert("Validation", "Please Enter Challan Weight", "OK");
                return;
            }
            else if (FactoryName.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Factory", "OK");
                return;
            }
            else if (AccountName.SelectedIndex == -1)
            {
                await DisplayAlert("Validation", "Please Select Account", "OK");
                return;
            }

            Stream imageStream = ConvertImagePathToStream(FilePath);

            if (imageStream == null)
            {
                await DisplayAlert("Validation", "Please Upload challan Copy", "OK");
                return;
            }


            string url = _appSetting.ApiUrl + "MessageBroker/ProduceSupplier";

            var selectedAccount = AccountName.SelectedItem as FactoryAccountModel;
            DateTime selectedDate = collectionDate.Date;
            string formattedDate = selectedDate.ToString("yyyy-MM-dd");

            using (HttpClient httpClient = new HttpClient())
            {
                using (MultipartFormDataContent formData = new MultipartFormDataContent())
                {
                    try
                    {
                       // string fineText = FineLeaf?.Text ?? string.Empty;

                        // Add additional form data
                        formData.Add(new StringContent("0"), "CollectionId");
                        formData.Add(new StringContent(formattedDate), "CollectionDate");
                        formData.Add(new StringContent(VehicleNo.Text), "VehicleNo");
                        formData.Add(new StringContent(LoginData.ClientLoginDetails[0].ClientId.ToString()), "ClientId");
                        formData.Add(new StringContent(selectedAccount.AccountId.ToString()), "AccountId");
                        formData.Add(new StringContent(FineLeaf?.Text ?? string.Empty), "FineLeaf");
                        formData.Add(new StringContent(ChallanWgt.Text), "ChallanWeight");
                        formData.Add(new StringContent("0"), "Rate");
                        formData.Add(new StringContent("0"), "GrossAmount");
                        formData.Add(new StringContent("1"), "TripId");
                        formData.Add(new StringContent("Pending"), "Status");
                        formData.Add(new StringContent(remarksEditor.Text ?? ""), "Remarks");
                        formData.Add(new StreamContent(imageStream), "ChallanImage", "your_image.png");
                        formData.Add(new StringContent(LoginData.ClientLoginDetails[0].UserId.ToString()), "CreatedBy");
                        formData.Add(new StringContent(LoginData.ClientLoginDetails[0].TenantId.ToString()), "TenantId");



                        HttpResponseMessage response = await httpClient.PostAsync(url, formData);
                        response.EnsureSuccessStatusCode();

                        // If needed, read the response content
                        string results = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {
                            var valueData = JsonConvert.DeserializeObject<SaveReturn>(results);

                            if (valueData.Id != 0)
                            {
                                await DisplayAlert("Success", valueData.Message, "OK");
                                ChallanImage.Source = null;
                                clearFormControl();
                            }
                            else
                            {
                                await DisplayAlert("Info", valueData.Message, "OK");
                            }

                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        await Navigation.PopModalAsync();
                        await DisplayAlert("Error", ex.Message, "ok");
                    }
                    finally
                    {
                        await Navigation.PopModalAsync();
                    }
                }

            }
        }
        //public static IFormFile ConvertFilePathToIFormFile(string filePath)
        //{
        //    // Open the file as a FileStream
        //    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        //    // Create an instance of FormFile
        //    var formFile = new FormFile(fileStream, 0, fileStream.Length, "file", Path.GetFileName(filePath))
        //    {
        //        Headers = new HeaderDictionary(),
        //        ContentType = GetContentType(filePath)
        //    };

        //    return formFile;
        //}
        //public static string GetContentType(string path)
        //{
        //    // You can use MimeMapping library or manually specify the content types
        //    var extension = Path.GetExtension(path).ToLowerInvariant();
        //    switch (extension)
        //    {
        //        case ".txt": return "text/plain";
        //        case ".jpg": return "image/jpeg";
        //        case ".png": return "image/png";
        //        case ".pdf": return "application/pdf";
        //        // Add more cases as needed
        //        default: return "application/octet-stream";
        //    }
        //}


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
        private async Task UploadChallan(long collId)
        {
            string url = _appSetting.ApiUrl + "Collection/UploadSupplierChallan";
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
                        // Console.WriteLine("Response: " + responseBody);
                    }
                    catch (HttpRequestException ex)
                    {
                        // Console.WriteLine("HTTP Request Error: " + ex.Message);
                        await DisplayAlert("Error", ex.Message, "ok");
                    }
                }
            }
        }

        public async void clearFormControl()
        {
            try
            {
                VehicleNo.Text = "";
                FineLeaf.Text = "";
                ChallanWgt.Text = "";
                remarksEditor.Text = ""; 
                //FactoryName.ItemsSource = null;
                //AccountName.ItemsSource = null;
            }
            catch ( Exception ex )
            {
                await DisplayAlert("Error", ex.Message, "ok");
            }
           
            
        }
        public async void GetFactoryAccount()
        {

            string url = _appSetting.ApiUrl + "Master/GetFactoryAccount";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                IsClientView = true,
                TenantId = Convert.ToInt32(LoginData.ClientLoginDetails[0].TenantId)
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

            }


        }

        public async void GetVehicle()
        {

            string url = _appSetting.ApiUrl + "Master/GetVehicle";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                IsClientView = true,
                TenantId = Convert.ToInt32(LoginData.ClientLoginDetails[0].TenantId)
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
        string imagefile;
        private async void OnCaptureClicked(object sender, EventArgs e)
        {
            //   var status = await Permissions.RequestAsync<Permissions.Camera>();

            if (!CrossMedia.Current.IsTakePhotoSupported && !CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "Format is not supported", "ok");
                return;
            }
            else
            {
                try
                {


                    await CrossMedia.Current.Initialize();

                    new ImageCropper()
                    {
                        PageTitle = "Select Image",
                        AspectRatioX = 19,
                        AspectRatioY = 9,
                        CropShape = ImageCropper.CropShapeType.Rectangle,
                        SelectSourceTitle = "Select source",
                        TakePhotoTitle = "Take Photo",
                        PhotoLibraryTitle = "Photo Library",
                        Success = (imageFile) =>
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                ChallanImage.Source = ImageSource.FromFile(imageFile);
                                imagefile = imageFile;
                                FilePath = imageFile;
                            });
                        }
                    }.Show(this);
                }
                catch (Exception ex)
                {
                    // System.Diagnostics.Debug.WriteLine("GalleryException:" + ex);
                    await DisplayAlert("Error", ex.Message, "ok");
                }



            }
        }

        protected void OnClickedRectangle(object sender, EventArgs e)
        {
            new ImageCropper()
            {
                //                PageTitle = "Test Title",
                //                AspectRatioX = 1,
                //                AspectRatioY = 1,
                Success = (imageFile) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ChallanImage.Source = ImageSource.FromFile(imageFile);
                    });
                }
            }.Show(this);
        }

    }
}