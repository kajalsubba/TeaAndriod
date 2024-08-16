using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class TripModel
    {
        public int TripId { get; set; }
        public string TripName { get; set; }
       
    }

    public class TripModelList
    {
        public List<TripModel> TripDetails { get; set; }
    }
}
