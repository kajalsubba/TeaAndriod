using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class SupplierBillHistoryModel
    {
        public long BillId { get; set; }
        public string BillDate { get; set; }
        public string BillPeriod { get; set; }
        public string ClientName { get; set; }
        public decimal AvgRate { get; set; }
        public long FinalWeight { get; set; }
        public decimal TotalStgAmount { get; set; }
        public decimal TotalStgPayment { get; set; }
        public decimal CommAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LessSeasonAdv { get; set; }
        public decimal AmountToPay { get; set; }
    }

    public class SupplierBillList
    {
        public IList<SupplierBillHistoryModel> BillHistory { get; set; }


    }
}
