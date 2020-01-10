using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace v8.Models
{
    public class Order
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("notificationUri")]
        public string NotificationUri { get; set; }

        //[JsonProperty("successUri")]
        //public string SuccessUri { get; set; }

        //[JsonProperty("cancelUri")]
        //public string CancelUri { get; set; }

        //[JsonProperty("errorUri")]
        //public string ErrorUri { get; set; }

        //[JsonProperty("merchantOrderId")]
        //public string MerchantOrderId { get; set; }


        //[JsonProperty("orderStaticId")]
        //public string OrderStaticId { get; set; }


    }
}
