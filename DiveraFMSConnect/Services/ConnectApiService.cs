//-----------------------------------------------------------------------
// <copyright file="ConnectApiService.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using global::DiveraFMSConnect.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Serviceklasse für die Verbindung mit der öffentlichen Connect-Schnittstelle.
    /// </summary>
    public class ConnectApiService
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string baseAddress;
        private readonly string apikey;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="ConnectApiService"/> Klasse.
        /// </summary>
        /// <param name="baseAddress">Die Basisadresse der API.</param>
        /// <param name="apikey">Der Zugriffsschlüssel zur API.</param>
        public ConnectApiService(string baseAddress, string apikey)
        {
            this.baseAddress = baseAddress;
            this.apikey = apikey;

            this.client.BaseAddress = new Uri(this.baseAddress);
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.apikey);
        }

        /// <summary>
        /// Sendet einen Fahrzeugstatus an die API.
        /// </summary>
        /// <param name="id">Die ID des Fahrzeugs, zu dem der Status gehört.</param>
        /// <param name="status">Der Fahrzeugstatus.</param>
        /// <returns>Nichts.</returns>
        public async Task PostVehicleStatusById(string id, ConnectStatus status)
        {
            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"interfaces/public/vehicle/{id}/status")
            {
                Content = new StringContent(JsonConvert.SerializeObject(status), Encoding.UTF8, "application/json"),
            };

            var response = await this.client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Fehler beim Senden des FMS-Status des Fahrzeugs mit der Connect-ID '{id}' an den Connect-Dienst. Statuscode '{response.StatusCode}'.");
            }
        }
    }
}
