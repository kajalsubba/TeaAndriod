using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class SmartHistoryModel
    {
        public string CollectionDate { get; set; }
        public long FinalWeight { get; set; }
        public decimal Rate { get; set; }
        public string RateStatus { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }

    }
    public class SmartSearchModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string CategoryName { get; set; }
        public long TenantId { get; set; }
        public long ClientId { get; set; }

    }
    public class SmartList
    {
        public IList<SmartHistoryModel> CollectionSummary { get; set; }
        public IList<SmartPaymentModel> PaymentSummary { get; set; }
        public IList<OutstandingModel> OutstandingSummary { get; set; }

    }

    public class OutstandingModel
    {
        public decimal SeasonAdvance { get; set; }
        public decimal PreviousBalance { get; set; }
    }

    public class SmartPaymentModel
    {
        public string PaymentDate { get; set; }
        public string Narration { get; set; }
       public decimal Amount { get; set; }
    }
    
}
