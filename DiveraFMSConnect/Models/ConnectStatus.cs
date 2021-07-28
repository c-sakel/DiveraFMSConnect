// Copyright (c) Moritz Jökel. All Rights Reserved.
// Licensed under Creative Commons Zero v1.0 Universal

namespace DiveraFMSConnect.Models
{
    using System;

    /// <summary>
    /// Repräsentiert ein Fahrzeugstatus, wie er in Feuersoftware Connect dargestellt wird.
    /// </summary>
    class ConnectStatus
    {
        /// <summary>
        /// Konstruktor für den Fahrzeugstatus für Feuersoftware Connect.
        /// </summary>
        /// <param name="diveraStatus">Der Fahrzeugstatus aus Divera 24/7</param>
        public ConnectStatus(DiveraStatus diveraStatus)
        {
            this.Status = diveraStatus.Status;
            this.Position = new Position(diveraStatus.Latitude, diveraStatus.Longitude);
            var timeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            this.StatusTimestamp = timeStampDateTime.AddSeconds(diveraStatus.StatusTimestamp).ToLocalTime();
            this.PositionTimestamp = DateTime.Now;
        }

        /// <summary>
        /// Der Status des Fahrzeugs
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Dis Geo-Position des Fahrzeugs
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Der Zeitstempel des Status
        /// </summary>
        public DateTime StatusTimestamp { get; set; }

        /// <summary>
        /// Der Zeitstempel der Geo-Position
        /// </summary>
        public DateTime PositionTimestamp { get; set; }
    }
}
