using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Interfaces
{
    public interface IBluetoothService
    {
        IList<string> GetDevicesList();
    }
}
