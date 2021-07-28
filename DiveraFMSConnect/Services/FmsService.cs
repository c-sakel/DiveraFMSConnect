//-----------------------------------------------------------------------
// <copyright file="FmsService.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using global::DiveraFMSConnect.Models;

    /// <summary>
    /// Ist für den Aufruf der Synchronisation im definierten Zeitintervall zuständig.
    /// </summary>
    public class FmsService
    {
        private readonly ConnectApiService connectApiService;
        private readonly DiveraApiService diveraApiService;
        private readonly Dictionary<string, string> vehicleIds;
        private readonly EventLog logger;
        private readonly ConcurrentDictionary<string, int> cachedStatuses;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="FmsService"/> Klasse.
        /// </summary>
        /// <param name="connectBaseAddress">Die Basisadresse für die Connect-API.</param>
        /// <param name="connectApiKey">Der Zugriffsschlüssel für die Connect-API.</param>
        /// <param name="diveraBaseAddress">Die Basisadresse für die Divera-API.</param>
        /// <param name="diveraApiKey">Der Zugriffsschlüssel für die Divera-API.</param>
        /// <param name="diveraIds">Die Fahrzeug-IDs für Divera.</param>
        /// <param name="connectIds">Die Fahrzeug-IDs für Connect.</param>
        /// <param name="logger">Der Logger für das EventLog.</param>
        public FmsService(
            string connectBaseAddress,
            string connectApiKey,
            string diveraBaseAddress,
            string diveraApiKey,
            IEnumerable<string> diveraIds,
            IEnumerable<string> connectIds,
            EventLog logger)
        {
            if (string.IsNullOrEmpty(connectBaseAddress))
            {
                throw new ArgumentException($"\"{nameof(connectBaseAddress)}\" kann nicht NULL oder leer sein.", nameof(connectBaseAddress));
            }

            if (string.IsNullOrEmpty(connectApiKey))
            {
                throw new ArgumentException($"\"{nameof(connectApiKey)}\" kann nicht NULL oder leer sein.", nameof(connectApiKey));
            }

            if (string.IsNullOrEmpty(diveraBaseAddress))
            {
                throw new ArgumentException($"\"{nameof(diveraBaseAddress)}\" kann nicht NULL oder leer sein.", nameof(diveraBaseAddress));
            }

            if (string.IsNullOrEmpty(diveraApiKey))
            {
                throw new ArgumentException($"\"{nameof(diveraApiKey)}\" kann nicht NULL oder leer sein.", nameof(diveraApiKey));
            }

            if (diveraIds is null || !diveraIds.Any())
            {
                throw new ArgumentNullException(nameof(diveraIds));
            }

            if (connectIds is null || !connectIds.Any())
            {
                throw new ArgumentNullException(nameof(connectIds));
            }

            this.vehicleIds = diveraIds.Zip(connectIds, (key, value) => new { key, value }).ToDictionary(dic => dic.key, dic => dic.value);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.connectApiService = new ConnectApiService(connectBaseAddress, connectApiKey);
            this.diveraApiService = new DiveraApiService(diveraBaseAddress, diveraApiKey);
            this.cachedStatuses = new ConcurrentDictionary<string, int>();
        }

        /// <summary>
        /// Führt die initiale Synchronisation der Fahrzeugstatusdaten durch. Hier wird der Cache befüllt.
        /// </summary>
        public async void InitialSync()
        {
            this.logger.WriteEntry($"Initialer Sync... Folgende Fahrzeuge wurden gefunden: '{string.Join(", ", this.vehicleIds.Keys.ToArray())}'", EventLogEntryType.Information);

            foreach (KeyValuePair<string, string> vehicle in this.vehicleIds)
            {
                try
                {
                    this.logger.WriteEntry($"Initialer Sync für Fahrzeug mit der Divera-ID '{vehicle.Key}' gestartet.", EventLogEntryType.Information);
                    var diveraStatus = await this.diveraApiService.GetVehicleStatusById(vehicle.Key);

                    this.cachedStatuses.TryAdd(vehicle.Key, diveraStatus.Status);

                    var convertedStatus = this.ConvertDiveraToConnect(diveraStatus);

                    await this.connectApiService.PostVehicleStatusById(vehicle.Value, convertedStatus);

                    this.logger.WriteEntry($"Initialer Sync für Fahrzeug mit der Divera-ID '{vehicle.Key}' abgeschlossen.", EventLogEntryType.Information);
                }
                catch (Exception exception)
                {
                    this.logger.WriteEntry($"Fehler beim initialen Sync des Fahrzeugs mit der Divera-ID '{vehicle.Key}' und der Connect-ID '{vehicle.Value}'. Fehler: '{exception.Message}'", EventLogEntryType.Error);
                }
            }
        }

        /// <summary>
        /// Führt eine Synchronisation zwischen Divera und Connect mit allen Fahrzeugen durch und beachtet dabei den Cache.
        /// Es wird nur synchronisiert, wenn sich der Fahrzeugstatus geändert hat.
        /// </summary>
        public async void Sync()
        {
            foreach (KeyValuePair<string, string> vehicle in this.vehicleIds)
            {
                try
                {
                    this.logger.WriteEntry($"Synchronisation für Fahrzeug mit der Divera-ID '{vehicle.Key}' gestartet.", EventLogEntryType.Information);
                    var diveraStatus = await this.diveraApiService.GetVehicleStatusById(vehicle.Key);

                    this.cachedStatuses.TryGetValue(vehicle.Key, out var cachedStatus);

                    if (cachedStatus == diveraStatus.Status)
                    {
                        this.logger.WriteEntry($"Fahrzeug mit Divera-ID '{vehicle.Key}' nicht aktualisiert. Fahrzeug befand sich bereits vorher im Status '{cachedStatus}'.", EventLogEntryType.Information);
                        this.logger.WriteEntry($"Synchronisation für Fahrzeug mit der Divera-ID '{vehicle.Key}' abgeschlossen.", EventLogEntryType.Information);
                        continue;
                    }

                    this.cachedStatuses.TryUpdate(vehicle.Key, diveraStatus.Status, diveraStatus.Status);
                    var convertedStatus = this.ConvertDiveraToConnect(diveraStatus);

                    await this.connectApiService.PostVehicleStatusById(vehicle.Value, convertedStatus);
                    this.logger.WriteEntry($"Synchronisation für Fahrzeug mit der Divera-ID '{vehicle.Key}' abgeschlossen. Status '{diveraStatus.Status}' übertragen.", EventLogEntryType.Information);
                }
                catch (Exception exception)
                {
                    this.logger.WriteEntry($"Fehler beim Synchronisieren des Fahrzeugs mit der Divera-ID '{vehicle.Key}' und der Connect-ID '{vehicle.Value}'. Fehler: '{exception.Message}'", EventLogEntryType.Error);
                }
            }
        }

        private ConnectStatus ConvertDiveraToConnect(DiveraStatus diveraStatus)
        {
            return new ConnectStatus(diveraStatus);
        }
    }
}
