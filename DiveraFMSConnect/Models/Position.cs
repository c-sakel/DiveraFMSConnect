//-----------------------------------------------------------------------
// <copyright file="Position.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect.Models
{
    /// <summary>
    /// Repräsentiert eine Geo-Position eines Fahrzeugs für Connect.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="Position"/> Klasse.
        /// </summary>
        /// <param name="latitude">Der Breitengrad.</param>
        /// <param name="longitude">Der Längengrad.</param>
        public Position(decimal latitude, decimal longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        /// <summary>
        /// Holt oder setzt den Breitengrad.
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Holt oder setzt den Längengrad.
        /// </summary>
        public decimal Longitude { get; set; }
    }
}
