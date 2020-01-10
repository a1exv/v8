using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace v8.Models
{
    public class StatusResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
