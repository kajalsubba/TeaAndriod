using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class VehicleModel
    {
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }
    }

    public class VehicleList
    {
        public IList<VehicleModel> VehicleDetails { get; set; }


    }


}
