// Copyright (c) Moritz Jökel. All Rights Reserved.
// Licensed under Creative Commons Zero v1.0 Universal

namespace DiveraFMSConnect.Models
{
    using Newtonsoft.Json;
    using System;

    class DiveraStatus
    {
        public int Status { get; set; }

        [JsonProperty("status_id")]
        public int StatusId { get; set; }

        [JsonProperty("status_ts")]
        public int StatusTimestamp { get; set; }

        [JsonProperty("status_note")]
        public string StatusNote { get; set; }

        [JsonProperty("lat")]
        public decimal Latitude { get; set; }

        [JsonProperty("lng")]
        public decimal Longitude { get; set; }
    }
}
