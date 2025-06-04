# DiveraFMSConnect Python Version

This repository originally contained a Windows service written in C# for synchronising vehicle status data between Divera 24/7 and Feuersoftware Connect.  
This Python script provides the same functionality so that it can easily run on Linux systems. The Windows-specific project files have been removed so that only the Python implementation remains.


## Installing Python and pip on Debian/Ubuntu
If Python 3 and pip are not yet installed you can install them with:
```bash
sudo apt update
sudo apt install python3 python3-pip
```

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


## Running as a systemd service

To run the script automatically at boot you can use a systemd service. A sample
unit file `diverafmsconnect.service` is included in this repository.

1. Copy the file to `/etc/systemd/system/`:
   ```bash
   sudo cp diverafmsconnect.service /etc/systemd/system/
   ```
2. Adjust the paths inside the service file if you placed the repository
   elsewhere. By default it expects the code in `/opt/diverafmsconnect` and runs
   `python3` on the `diverafmsconnect.py` script.
3. Reload systemd and enable the service so it starts on boot:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable --now diverafmsconnect.service
   ```
4. Check the status with:
   ```bash
   sudo systemctl status diverafmsconnect.service
   ```
