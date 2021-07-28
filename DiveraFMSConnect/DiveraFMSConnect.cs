// Copyright (c) Moritz Jökel. All Rights Reserved.
// Licensed under Creative Commons Zero v1.0 Universal

namespace DiveraFMSConnect
{
    using Configuration;
    using Services;
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Timers;

    public partial class DiveraFMSConnect : ServiceBase
    {
        private readonly EventLog logger;
        private readonly FmsService service;
        private readonly Timer timer;

        public DiveraFMSConnect()
        {
            InitializeComponent();
            this.logger = new EventLog();
            this.logger.Source = "DiveraFMSConnect";

            try
            {            
                this.service = new FmsService(
                ConfigurationProvider.GetConnectBaseAddress(),
                ConfigurationProvider.GetConnectApiKey(),
                ConfigurationProvider.GetDiveraBaseAddress(),
                ConfigurationProvider.GetDiveraApiKey(),
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

        protected override void OnStart(string[] args)
        {
            this.logger.WriteEntry("DiveraFMSConnect gestartet.", EventLogEntryType.Information);
            this.service.InitialSync();
            this.timer.Start();
        }

        protected override void OnStop()
        {
            this.timer.Stop();
            this.logger.WriteEntry("DiveraFMSConnect gestoppt.", EventLogEntryType.Information);
        }

        /// <summary>
        /// Handler zum Synchronisieren
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnElapsedTime(Object source, ElapsedEventArgs e)
        {
            this.service.Sync();
        }
    }
}
