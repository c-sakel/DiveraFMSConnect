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
        private readonly Dictionary<string, int> cachedStatuses;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="FmsService"/> Klasse.
        /// </summary>
        /// <param name="connectApiService">Der Service für die Kommunikation mit Connect.</param>
        /// <param name="diveraApiService">Der Service für die Kommunikation mit Divera 24/7.</param>
        /// <param name="diveraIds">Die Fahrzeug-IDs aus Divera.</param>
        /// <param name="connectIds">Die Fahrzeug-IDs aus Connect.</param>
        /// <param name="logger">Das EventLog.</param>
        public FmsService(
            ConnectApiService connectApiService,
            DiveraApiService diveraApiService,
            IEnumerable<string> diveraIds,
            IEnumerable<string> connectIds,
            EventLog logger)
        {
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
            this.connectApiService = connectApiService ?? throw new ArgumentNullException(nameof(connectApiService));
            this.diveraApiService = diveraApiService ?? throw new ArgumentNullException(nameof(diveraApiService));
            this.cachedStatuses = new Dictionary<string, int>();
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

                    this.cachedStatuses.Add(vehicle.Key, diveraStatus.Status);

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
                    var cachedStatus = this.cachedStatuses[vehicle.Key];

                    if (cachedStatus == diveraStatus.Status)
                    {
                        this.logger.WriteEntry($"Fahrzeug mit Divera-ID '{vehicle.Key}' nicht aktualisiert. Fahrzeug befand sich bereits vorher im Status '{cachedStatus}'.", EventLogEntryType.Information);
                        this.logger.WriteEntry($"Gecachte Fahrzeugstatus sind: \n {string.Join(Environment.NewLine, this.cachedStatuses.Select(a => $"{a.Key}: {a.Value}"))}", EventLogEntryType.Information);
                        this.logger.WriteEntry($"Synchronisation für Fahrzeug mit der Divera-ID '{vehicle.Key}' abgeschlossen.", EventLogEntryType.Information);
                        continue;
                    }

                    this.cachedStatuses[vehicle.Key] = diveraStatus.Status;
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
