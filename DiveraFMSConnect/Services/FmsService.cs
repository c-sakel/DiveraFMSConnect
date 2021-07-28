// Copyright (c) Moritz Jökel. All Rights Reserved.
// Licensed under Creative Commons Zero v1.0 Universal

namespace DiveraFMSConnect.Services
{
    using Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    class FmsService
    {
        private readonly string connectBaseAddress;
        private readonly string connectApiKey;
        private readonly string diveraBaseAddress;
        private readonly string diveraApiKey;
        private readonly ConnectApiService connectApiService;
        private readonly DiveraApiService diveraApiService;
        private readonly Dictionary<string, string> vehicleIds;
        private readonly EventLog logger;
        private readonly ConcurrentDictionary<string, int> cachedStatuses;

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

            this.connectBaseAddress = connectBaseAddress;
            this.connectApiKey = connectApiKey;
            this.diveraBaseAddress = diveraBaseAddress;
            this.diveraApiKey = diveraApiKey;
            this.vehicleIds = diveraIds.Zip(connectIds, (key, value) => new { key, value }).ToDictionary(dic => dic.key, dic => dic.value);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.connectApiService = new ConnectApiService(connectBaseAddress, connectApiKey);
            this.diveraApiService = new DiveraApiService(diveraBaseAddress, diveraApiKey);
            this.cachedStatuses = new ConcurrentDictionary<string, int>();
        }

        public async void InitialSync()
        {
            this.logger.WriteEntry($"Initialer Sync... Folgende Fahrzeuge wurden gefunden: '{String.Join(", ", this.vehicleIds.Keys.ToArray())}'", EventLogEntryType.Information);
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
                catch(Exception exception)
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
