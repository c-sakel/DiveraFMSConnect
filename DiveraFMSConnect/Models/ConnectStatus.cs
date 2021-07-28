//-----------------------------------------------------------------------
// <copyright file="ConnectStatus.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect.Models
{
    using System;

    /// <summary>
    /// Repräsentiert ein Fahrzeugstatus, wie er in Feuersoftware Connect dargestellt wird.
    /// </summary>
    public class ConnectStatus
    {
        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="ConnectStatus"/> Klasse.
        /// </summary>
        /// <param name="diveraStatus">Der Fahrzeugstatus aus Divera.</param>
        public ConnectStatus(DiveraStatus diveraStatus)
        {
            this.Status = diveraStatus.Status;
            this.Position = new Position(diveraStatus.Latitude, diveraStatus.Longitude);
            var timeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            this.StatusTimestamp = timeStampDateTime.AddSeconds(diveraStatus.StatusTimestamp).ToLocalTime();
            this.PositionTimestamp = DateTime.Now;
        }

        /// <summary>
        /// Holt oder setzt den Status des Fahrzeugs.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Holt oder setzt dis Geo-Position des Fahrzeugs.
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Holt oder setzt den Zeitstempel des Status.
        /// </summary>
        public DateTime StatusTimestamp { get; set; }

        /// <summary>
        /// Holt oder setzt den Zeitstempel der Geo-Position.
        /// </summary>
        public DateTime PositionTimestamp { get; set; }
    }
}
