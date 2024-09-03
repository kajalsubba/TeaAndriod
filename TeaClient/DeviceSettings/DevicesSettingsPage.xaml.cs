using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Interfaces;
using TeaClient.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Java.Util;
using SQLite;
using TeaClient.SQLLite;
using static Android.Bluetooth.BluetoothClass;

namespace TeaClient.DeviceSettings
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DevicesSettingsPage : ContentPage
	{
        public IList<DevicesModel> DevicesList { get; set; }
        public SQLiteConnection conn;
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        public DevicesSettingsPage ()
		{
			InitializeComponent ();
            DevicesList = new ObservableCollection<DevicesModel>();
            DevicesAll();
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<DevicesModel>();
            GetDevice();
        }

        void GetDevice()
        {
            var CollectionDetails = (from x in conn.Table<DevicesModel>() select x).ToList();
            DeviceListView.ItemsSource = CollectionDetails;

        }

        
        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            try
            {

                bool result = await DisplayAlert("Delete", "Do you want to delete!", "Yes", "No");
                if (result)
                {
                    conn.Execute("DROP TABLE IF EXISTS DevicesModel");
                    conn.CreateTable<DevicesModel>();

                    GetDevice();
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
          
        }
        private async void OnPrintClicked(object sender, EventArgs e)
        {
            var selectedDevice = (DevicesModel)Devices.SelectedItem;
            BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                      where bd?.Name == selectedDevice.DeviceName
                                      select bd).FirstOrDefault();

            if (device == null)
            {
                await DisplayAlert("Error", "Selected device is not bonded", "OK");
                return;
            }

            BluetoothSocket bluetoothSocket = null;

            try
            {
                // Attempt to create and connect the socket
                bluetoothSocket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));

                await Task.Run(() => bluetoothSocket.Connect()); // Use Task.Run to avoid blocking the UI thread

                byte[] buffer = Encoding.UTF8.GetBytes(DeviceDesc.Text);

                // Check if the output stream is available
                if (bluetoothSocket?.OutputStream != null)
                {
                    bluetoothSocket.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    await DisplayAlert("Error", "Output stream is not available", "OK");
                }
            }
            catch (Java.IO.IOException ioEx)
            {
                // Handle IOExceptions such as socket closure or read/write errors
                await DisplayAlert("IO Exception", ioEx.Message, "OK");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                await DisplayAlert("Exception", ex.Message, "OK");
            }
            finally
            {
                // Close the socket if necessary
                if (bluetoothSocket != null && bluetoothSocket.IsConnected)
                {
                    bluetoothSocket.Close();
                }
            }


        }
        private async void OnSaveDeviceClicked(object sender, EventArgs e)
        {
            var selectedDevice = (DevicesModel)Devices.SelectedItem;
            DevicesModel _device = new DevicesModel();
            _device.DeviceName = selectedDevice.DeviceName;
            _device.DeviceType = selectedDevice.DeviceType;
            _device.DeviceDesc =DeviceDesc.Text;
            try
            {
              
                conn.Insert(_device);
                await DisplayAlert("Info", "Printer is added Successfully. ", "Yes");
                GetDevice();
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

            }
        }
        public void DevicesAll()
        {
            try
            {
               
                if (bluetoothAdapter == null)
                {
                    DisplayAlert("info","Device is not supported Bluetooth!", "ok");
                }
                else
                {
                    if (bluetoothAdapter.IsEnabled)
                    {
                         var bondedDevices = bluetoothAdapter.BondedDevices;
                        foreach (var device in bondedDevices)
                        {
                            DevicesList.Add(new DevicesModel { DeviceName = device.Name ,DeviceType="Bluetooth"});

                        }
                        //DevicesList.Add(new DevicesModel { DeviceName = "jjj", DeviceType = "Bluetooth" });
                        //DevicesList.Add(new DevicesModel { DeviceName = "MT11", DeviceType = "Bluetooth" });
                        Devices.ItemsSource = DevicesList.ToList();
                    }
                    else
                    {
                        DisplayAlert("info", "Device is not Enabled!", "ok");
                    }
                }
             
            }
            catch (Exception ex)
            {
                DisplayAlert("info", ex.Message, "ok");
            }
           
        }
    }
}