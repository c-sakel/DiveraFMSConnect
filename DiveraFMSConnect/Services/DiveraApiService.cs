// Copyright (c) Moritz Jökel. All Rights Reserved.
// Licensed under Creative Commons Zero v1.0 Universal

namespace DiveraFMSConnect.Services
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Serviceklasse für die Verbindung mit der Schnittstelle von Divera 24/7
    /// </summary>
    class DiveraApiService
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string baseAddress;
        private readonly string apikey;

        /// <summary>
        /// Konstruktor für die Serviceklasse für die Verbindung zu Divera 24/7.
        /// </summary>
        /// <param name="baseAddress">Die Basisadresse der Divera-API</param>
        /// <param name="apikey">Der Zugriffsschlüssel für die Divera-API</param>
        public DiveraApiService(string baseAddress, string apikey)
        {
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
        /// <param name="id">Die ID des Fahrzeugs, zu welchem der Status geholt werden soll</param>
        /// <returns>Den Fahrzeugstatus</returns>
        public async Task<DiveraStatus> GetVehicleStatusById(string id)
        {
            DiveraStatus status = new DiveraStatus();
            HttpResponseMessage response = await client.GetAsync($"api/v2/using-vehicles/get-status/{id}?accesskey={this.apikey}");

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
