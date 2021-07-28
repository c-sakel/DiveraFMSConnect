//-----------------------------------------------------------------------
// <copyright file="ConfigurationProvider.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// Wrapperklasse für den Zugriff auf die Konfigurationswerte.
    /// </summary>
    public static class ConfigurationProvider
    {
        /// <summary>
        /// Ermittelt das Timer-Intervall aus der Konfiguration.
        /// </summary>
        /// <returns>Das Timer-Intervall in Millisekunden.</returns>
        public static int GetTimerInterval()
        {
            var timerInterval = int.Parse(ConfigurationManager.AppSettings.Get("TimerInterval"));

            if (timerInterval < 30000)
            {
                throw new ConfigurationErrorsException($"Der Konfigurationswert für den Timer-Intervall darf nicht kleiner als 30.000 (30 Sek.) sein. Aktueller Wert: '{timerInterval}'.");
            }

            return timerInterval;
        }

        /// <summary>
        /// Ermittelt die IDs der Fahrzeuge aus Divera.
        /// </summary>
        /// <returns>Die Divera-Fahrzeug-IDs.</returns>
        public static IEnumerable<string> GetDiveraVehicleIds()
        {
            var diveraVehicleIds = ConfigurationManager.AppSettings.Get("DiveraVehicleIds").Split(',');

            if (diveraVehicleIds.Length == 0)
            {
                throw new ConfigurationErrorsException($"Der Konfigurationswert für die DiveraVehicleIds darf nicht leer sein.");
            }
            else if (diveraVehicleIds.Length != ConfigurationProvider.GetConnectVehicleIdsCount())
            {
                throw new ConfigurationErrorsException($"Die Anzahl der DiveraVehicleIds darf nicht von der Anzahl der ConnectVehicleIds abweichen. Anzahl DiveraVehicleIds: '{diveraVehicleIds.Length}', Anzahl ConnectVehicleIds: '{ConfigurationProvider.GetDiveraVehicleIdsCount()}'.");
            }

            return diveraVehicleIds;
        }

        /// <summary>
        /// Ermittelt die IDs der Fahrzeuge aus Connect.
        /// </summary>
        /// <returns>Die Connect-Fahrzeug-IDs.</returns>
        public static IEnumerable<string> GetConnectVehicleIds()
        {
            var connectVehicleIds = ConfigurationManager.AppSettings.Get("ConnectVehicleIds").Split(',');

            if (connectVehicleIds.Length == 0)
            {
                throw new ConfigurationErrorsException($"Der Konfigurationswert für die ConnectVehicleIds darf nicht leer sein.");
            }
            else if (connectVehicleIds.Length != ConfigurationProvider.GetDiveraVehicleIdsCount())
            {
                throw new ConfigurationErrorsException($"Die Anzahl der ConnectVehicleIds darf nicht von der Anzahl der DiveraVehicleIds abweichen. Anzahl ConnectVehicleIds: '{connectVehicleIds.Length}', Anzahl DiveraVehicleIds: '{ConfigurationProvider.GetDiveraVehicleIdsCount()}'.");
            }

            return connectVehicleIds;
        }

        /// <summary>
        /// Ermittelt den API-Key für Divera aus der Konfiguration.
        /// </summary>
        /// <returns>Den-API-Key für Divera.</returns>
        public static string GetDiveraApiKey()
        {
            var diveraApiKey = ConfigurationManager.AppSettings.Get("DiveraApiKey");

            if (diveraApiKey is null || string.IsNullOrWhiteSpace(diveraApiKey))
            {
                throw new ConfigurationErrorsException($"Der Konfigurationswert für den DiveraApiKey darf nicht leer sein oder nur aus Whitespace bestehen.");
            }

            return diveraApiKey;
        }

        /// <summary>
        /// Ermittelt den API-Key für Connect aus der Konfiguration.
        /// </summary>
        /// <returns>Den-API-Key für Connect.</returns>
        public static string GetConnectApiKey()
        {
            var connectApikey = ConfigurationManager.AppSettings.Get("ConnectApiKey");

            if (connectApikey is null || string.IsNullOrWhiteSpace(connectApikey))
            {
                throw new ConfigurationErrorsException($"Der Konfigurationswert für den ConnectApiKey darf nicht leer sein oder nur aus Whitespace bestehen.");
            }

            return connectApikey;
        }

        /// <summary>
        /// Ermittelt die Basisadresse für Connect aus der Konfiguration.
        /// </summary>
        /// <returns>Die Basisadresse für die Connect-API.</returns>
        public static string GetConnectBaseAddress()
        {
            var connectBaseAddress = ConfigurationManager.AppSettings.Get("ConnectBaseAddress");

            if (connectBaseAddress is null || string.IsNullOrWhiteSpace(connectBaseAddress) || !Uri.TryCreate(connectBaseAddress, UriKind.Absolute, out var _))
            {
                throw new ConfigurationErrorsException($"Der Konfigurationswert für die ConnectBaseAddress darf nicht leer sein oder nur aus Whitespace bestehen. Ausserdem muss sie eine korrekte absolute URI sein.");
            }

            return connectBaseAddress;
        }

        /// <summary>
        /// Ermittelt die Basisadresse für Divera aus der Konfiguration.
        /// </summary>
        /// <returns>Die Basisadresse für die Divera-API.</returns>
        public static string GetDiveraBaseAddress()
        {
            var diveraBaseAddress = ConfigurationManager.AppSettings.Get("DiveraBaseAddress");

            if (diveraBaseAddress is null || string.IsNullOrWhiteSpace(diveraBaseAddress) || !Uri.TryCreate(diveraBaseAddress, UriKind.Absolute, out var _))
            {
                throw new ConfigurationErrorsException($"Der Konfigurationswert für die DiveraBaseAddress darf nicht leer sein oder nur aus Whitespace bestehen. Ausserdem muss sie eine korrekte absolute URI sein.");
            }

            return diveraBaseAddress;
        }

        private static int GetDiveraVehicleIdsCount()
        {
            return ConfigurationManager.AppSettings.Get("DiveraVehicleIds").Split(',').ToArray().Length;
        }

        private static int GetConnectVehicleIdsCount()
        {
            return ConfigurationManager.AppSettings.Get("ConnectVehicleIds").Split(',').ToArray().Length;
        }
    }
}
