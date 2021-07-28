//-----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Moritz Jökel">
//     Copyright (c) Moritz Jökel. All Rights Reserved.
//     Licensed under Creative Commons Zero v1.0 Universal
// </copyright>
//-----------------------------------------------------------------------

namespace DiveraFMSConnect
{
    using System.ComponentModel;
    using System.Configuration.Install;

    /// <summary>
    /// Ist für die Installation des Windows-Services zuständig.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="ProjectInstaller"/> Klasse.
        /// </summary>
        public ProjectInstaller()
        {
            this.InitializeComponent();
        }
    }
}
