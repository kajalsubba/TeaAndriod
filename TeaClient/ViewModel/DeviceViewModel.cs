using Android.Bluetooth;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;

namespace TeaClient.ViewModel
{
    public class DeviceViewModel
    {
        public IList<DevicesModel> DevicesList { get; set; }
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

        public DeviceViewModel()
        {
            DevicesList = new ObservableCollection<DevicesModel>();
          //  DevicesAll();
        }
        public string DevicesAll()
        {
            try
            {

                if (bluetoothAdapter == null)
                {
                    //  DisplayAlert("info", "Device is not supported Bluetooth!", "ok");
                    return "Device is not supported Bluetooth!";
                }
                else
                {
                    if (bluetoothAdapter.IsEnabled)
                    {
                        var bondedDevices = bluetoothAdapter.BondedDevices;
                        foreach (var device in bondedDevices)
                        {
                            DevicesList.Add(new DevicesModel { DeviceName = device.Name, DeviceType = "Bluetooth" });

                        }
                        //DevicesList.Add(new DevicesModel { DeviceName = "Stone", DeviceType = "Bluetooth" });
                        //DevicesList.Add(new DevicesModel { DeviceName = "MT11", DeviceType = "Bluetooth" });
                        return null;
                    }
                    else
                    {
                        //  DisplayAlert("info", "Device is not Enabled!", "ok");
                        return "Device is not Enabled!";
                    }
                }

            }
            catch (Exception ex)
            {
                // DisplayAlert("info", ex.Message, "ok");
                return ex.Message.ToString();
            }

        }

    }
}
