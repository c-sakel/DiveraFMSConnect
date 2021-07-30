//-----------------------------------------------------------------------
// <copyright file="DiveraFMSConnect.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Timers;
    using global::DiveraFMSConnect.Configuration;
    using global::DiveraFMSConnect.Services;

    /// <summary>
    /// Der Haupt-Service der Anwendung.
    /// </summary>
    public partial class DiveraFMSConnect : ServiceBase
    {
        private readonly EventLog logger;
        private readonly FmsService fmsService;
        private readonly Timer timer;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="DiveraFMSConnect"/> Klasse.
        /// </summary>
        public DiveraFMSConnect()
        {
            this.InitializeComponent();
            this.logger = new EventLog
            {
                Source = "DiveraFMSConnect",
            };

            try
            {
                var connectApiService = new ConnectApiService(
                    ConfigurationProvider.GetConnectBaseAddress(),
                    ConfigurationProvider.GetConnectApiKey());
                var diveraApiService = new DiveraApiService(
                    ConfigurationProvider.GetDiveraBaseAddress(),
                    ConfigurationProvider.GetDiveraApiKey());

                this.fmsService = new FmsService(
                    connectApiService,
                    diveraApiService,
                    ConfigurationProvider.GetDiveraVehicleIds(),
                    ConfigurationProvider.GetConnectVehicleIds(),
                    this.logger);
            }
            catch (Exception exception)
            {
                this.logger.WriteEntry($"Fataler Fehler bei Erstellung des Services. Dienst wird beendet. Fehler: '{exception.Message}'", EventLogEntryType.Error);
                System.Environment.Exit(1);
            }

            this.timer = new Timer(ConfigurationProvider.GetTimerInterval());
            this.timer.Elapsed += this.OnElapsedTime;
            this.timer.AutoReset = true;
        }

        /// <inheritdoc/>
        protected override void OnStart(string[] args)
        {
            this.logger.WriteEntry("DiveraFMSConnect gestartet.", EventLogEntryType.Information);
            this.fmsService.InitialSync();
            this.timer.Start();
        }

        /// <inheritdoc/>
        protected override void OnStop()
        {
            this.timer.Stop();
            this.logger.WriteEntry("DiveraFMSConnect gestoppt.", EventLogEntryType.Information);
        }

        /// <summary>
        /// Handler zum Synchronisieren.
        /// </summary>
        /// <param name="source">Die Quelle des Events.</param>
        /// <param name="e">Die Argumente zum Event.</param>
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            this.fmsService.Sync();
        }
    }
}
