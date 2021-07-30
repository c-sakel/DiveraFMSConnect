//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect
{
    using System.ServiceProcess;

    /// <summary>
    /// Haupteinstiegsklasse für die Anwendung.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        public static void Main()
        {
            ServiceBase[] servicesToRun;
            servicesToRun = new ServiceBase[]
            {
                new DiveraFMSConnect(),
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
