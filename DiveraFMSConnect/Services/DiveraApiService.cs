//-----------------------------------------------------------------------
// <copyright file="DiveraApiService.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using global::DiveraFMSConnect.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Serviceklasse für die Verbindung mit der Schnittstelle von Divera 24/7.
    /// </summary>
    public class DiveraApiService
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string baseAddress;
        private readonly string apikey;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="DiveraApiService"/> Klasse.
        /// </summary>
        /// <param name="baseAddress">Die Basisadresse der Divera-API.</param>
        /// <param name="apikey">Der Zugriffsschlüssel für die Divera-API.</param>
        public DiveraApiService(string baseAddress, string apikey)
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
            {
                throw new ArgumentException($"\"{nameof(baseAddress)}\" darf nicht NULL oder ein Leerraumzeichen sein.", nameof(baseAddress));
            }

            if (string.IsNullOrWhiteSpace(apikey))
            {
                throw new ArgumentException($"\"{nameof(apikey)}\" darf nicht NULL oder ein Leerraumzeichen sein.", nameof(apikey));
            }

            this.baseAddress = baseAddress;
            this.apikey = apikey;

            this.client.BaseAddress = new Uri(this.baseAddress);
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Ruft den Fahrzeugstatus eines Fahrzeugs anhand der ID von Divera 24/7 ab.
        /// </summary>
        /// <param name="id">Die ID des Fahrzeugs, zu welchem der Status geholt werden soll.</param>
        /// <returns>Den Fahrzeugstatus.</returns>
        public async Task<DiveraStatus> GetVehicleStatusById(string id)
        {
            DiveraStatus status;
            HttpResponseMessage response = await this.client.GetAsync($"api/v2/using-vehicles/get-status/{id}?accesskey={this.apikey}");

            if (response.IsSuccessStatusCode)
            {
                status = JsonConvert.DeserializeObject<DiveraStatus>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                throw new HttpRequestException($"Fehler beim Abruf des FMS-Status bei Divera des Fahrzeugs mit der Id '{id}'. Statuscode '{response.StatusCode}'.");
            }

            return status;
        }
    }
}
