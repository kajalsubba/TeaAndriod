using Android.Bluetooth;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using System.Linq;
using SQLite;
using TeaClient.SQLLite;
using Xamarin.Forms;
using Android.Webkit;

namespace TeaClient.Services
{
    public class PrintService
    {
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        public SQLiteConnection conn;
        public PrintService()
        {
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<DevicesModel>();
        }


        public async Task<string> Print(StringBuilder Message)
        {
            var DevicesDetails = (from x in conn.Table<DevicesModel>() select x).FirstOrDefault();
            string DeviceName = DevicesDetails.DeviceName;
            string outputString = Message.ToString();
            BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                      where bd?.Name == DeviceName
                                      select bd).FirstOrDefault();

            if (device == null)
            {
                // await DisplayAlert("Error", "Selected device is not bonded", "OK");
                return "Selected device is not bonded";
            }

            BluetoothSocket bluetoothSocket = null;

            try
            {
                bluetoothSocket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));

                await Task.Run(() => bluetoothSocket.Connect()); // Use Task.Run to avoid blocking the UI thread

                byte[] buffer = Encoding.UTF8.GetBytes(outputString);

                if (bluetoothSocket?.OutputStream != null)
                {
                    bluetoothSocket.OutputStream.Write(buffer, 0, buffer.Length);
                    return "Print is done!";
                }
                else
                {
                    return "Output stream is not available";
                }
            }
            catch (Java.IO.IOException ioEx)
            {

                return ioEx.Message;
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            finally
            {

                if (bluetoothSocket != null && bluetoothSocket.IsConnected)
                {
                    bluetoothSocket.Close();
                }
            }
        }


        public async Task<string> PrintParameters(string CollectDate,string company, string ClientName, string Grade, string BagList, string deduct, string First,
           string Final, string GrossAmt, string remarks, string CollectedBy)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(company.Trim());
            sb.AppendLine();
            sb.AppendLine("GreenLeaf collect on " + CollectDate);
            sb.AppendLine("Client :"+ ClientName);
            sb.AppendLine("Grade :" + Grade);
            sb.AppendLine("Field :" + BagList + "=" + First);
            sb.AppendLine("Deduction :" + deduct);
            sb.AppendLine("Final :" + Final);
            sb.AppendLine("Amount :" + GrossAmt);
            sb.AppendLine("Remarks :" + remarks);
            sb.AppendLine();
            sb.AppendLine("Collected By " + CollectedBy);
            sb.AppendLine("Print Time " + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));

            var result = await Print(sb);
            return result;

        }
    }
}
