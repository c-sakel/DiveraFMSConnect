using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiveraFMSConnect.Models
{
    class Position
    {
        public Position(decimal lat, decimal lng)
        {
            this.Latitude = lat;
            this.Longitude = lng;
        }

        /// <summary>
        /// Der Breitengrad
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Der Längengrad
        /// </summary>
        public decimal Longitude { get; set; }
    }
}
