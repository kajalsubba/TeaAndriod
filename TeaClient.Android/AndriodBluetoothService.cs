using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeaClient.Droid;
using TeaClient.Interfaces;

[assembly:Xamarin.Forms.Dependency(typeof (AndriodBluetoothService))]
namespace TeaClient.Droid
{

    public class AndriodBluetoothService : IBluetoothService
    {
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        IList<string> IBluetoothService.GetDevicesList()
        {
          var blueth= bluetoothAdapter?.BondedDevices.
                Select(device => device.Name).ToList();
            return blueth;
        }
    }
}