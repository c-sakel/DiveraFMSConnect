//-----------------------------------------------------------------------
// <copyright file="DiveraStatus.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Repräsentiert einen Fahrzeugstatus aus Divera.
    /// </summary>
    public class DiveraStatus
    {
        /// <summary>
        /// Holt oder setzt den Fahrzeugstatus.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Holt oder setzt die Fahrzeugstatus-ID.
        /// </summary>
        [JsonProperty("status_id")]
        public int StatusId { get; set; }

        /// <summary>
        /// Holt oder setzt den Zeitstempel des Fahrzeugstatus als UNIX-Timestamp.
        /// </summary>
        [JsonProperty("status_ts")]
        public int StatusTimestamp { get; set; }

        /// <summary>
        /// Holt oder setzt die Bemerkung zum Fahrzeugstatus.
        /// </summary>
        [JsonProperty("status_note")]
        public string StatusNote { get; set; }

        /// <summary>
        /// Holt oder setzt den Breitengrad der Fahrzeugposition.
        /// </summary>
        [JsonProperty("lat")]
        public decimal Latitude { get; set; }

        /// <summary>
        /// Holt oder setzt den Längengrad der Fahrzeugposition.
        /// </summary>
        [JsonProperty("lng")]
        public decimal Longitude { get; set; }
    }
}
