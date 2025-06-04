# DiveraFMSConnect Python Version

This repository originally contained a Windows service written in C# for synchronising vehicle status data between Divera 24/7 and Feuersoftware Connect.  
This Python script provides the same functionality so that it can easily run on Linux systems.

## Usage

1. Install the required dependencies:
   ```bash
   pip install -r requirements.txt
   ```
2. Copy `config.ini` and adjust the values (API keys, vehicle IDs, timer interval).
3. Run the service:
   ```bash
   python diverafmsconnect.py
   ```

The script performs an initial synchronisation and then periodically syncs the status of all configured vehicles. The interval must not be less than 30 seconds.
