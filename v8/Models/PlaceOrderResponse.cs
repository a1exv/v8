using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace v8.Models
{
    public class PlaceOrderResponse
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("paymentUri")]
        public string PaymentUri { get; set; }
    }
}
