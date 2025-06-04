#!/usr/bin/env python3
"""Synchronize vehicle status between Divera 24/7 and Feuersoftware Connect.

This script replicates the functionality of the original .NET service in
Python so that it can easily be run on Linux systems.
"""
import configparser
import datetime
import logging
import time
from dataclasses import dataclass
from typing import Dict, List, Tuple
import requests


@dataclass
class Position:
    latitude: float
    longitude: float


@dataclass
class ConnectStatus:
    Status: int
    Position: Position
    StatusTimestamp: str
    PositionTimestamp: str


class ConnectApiService:
    """Simple wrapper for the Connect REST API."""

    def __init__(self, base_address: str, api_key: str) -> None:
        self.base_address = base_address.rstrip('/') + '/'
        self.session = requests.Session()
        self.session.headers.update({'Authorization': f'Bearer {api_key}',
                                      'Accept': 'application/json'})

    def post_vehicle_status_by_id(self, vehicle_id: str, status: ConnectStatus) -> None:
        url = f"{self.base_address}interfaces/public/vehicle/{vehicle_id}/status"
        payload = {
            'Status': status.Status,
            'Position': {
                'Latitude': status.Position.latitude,
                'Longitude': status.Position.longitude,
            },
            'StatusTimestamp': status.StatusTimestamp,
            'PositionTimestamp': status.PositionTimestamp,
        }
        response = self.session.post(url, json=payload)
        if not response.ok:
            try:
                detail = response.json()
            except ValueError:
                detail = response.text
            raise RuntimeError(
                f"Error sending status for vehicle {vehicle_id}. "
                f"Status code {response.status_code}: {detail}"
            )


class DiveraApiService:
    """Simple wrapper for the Divera 24/7 API."""

    def __init__(self, base_address: str, api_key: str) -> None:
        self.base_address = base_address.rstrip('/') + '/'
        self.api_key = api_key
        self.session = requests.Session()
        self.session.headers.update({'Accept': 'application/json'})

    def get_vehicle_status_by_id(self, vehicle_id: str) -> Dict:
        url = f"{self.base_address}api/v2/using-vehicles/get-status/{vehicle_id}?accesskey={self.api_key}"
        response = self.session.get(url)
        if not response.ok:
            raise RuntimeError(
                f"Error fetching status for vehicle {vehicle_id}. "
                f"Status code {response.status_code}"
            )
        return response.json()


class FmsService:
    """Coordinates the synchronization between Divera and Connect."""

    def __init__(self, connect_service: ConnectApiService, divera_service: DiveraApiService,
                 divera_ids: List[str], connect_ids: List[str], logger: logging.Logger) -> None:
        if len(divera_ids) != len(connect_ids):
            raise ValueError("Divera and Connect vehicle id counts must match")
        self.vehicle_map = dict(zip(divera_ids, connect_ids))
        self.connect_service = connect_service
        self.divera_service = divera_service
        self.logger = logger
        self.cached_status: Dict[str, int] = {}
        self.cached_coords: Dict[str, tuple] = {}

    def initial_sync(self) -> None:
        self.logger.info("Initial sync started")
        for d_id, c_id in self.vehicle_map.items():
            try:
                divera_status = self.divera_service.get_vehicle_status_by_id(d_id)
                self.cached_status[d_id] = divera_status['status']
                self.cached_coords[d_id] = (divera_status['lat'], divera_status['lng'])
                converted = self._convert_status(divera_status)
                self.connect_service.post_vehicle_status_by_id(c_id, converted)
                self.logger.info("Initial sync for %s finished", d_id)
            except Exception as exc:
                self.logger.error("Error during initial sync of %s: %s", d_id, exc)

    def sync(self) -> None:
        for d_id, c_id in self.vehicle_map.items():
            try:
                divera_status = self.divera_service.get_vehicle_status_by_id(d_id)
                status = divera_status['status']
                coords: Tuple[float, float] = (divera_status['lat'], divera_status['lng'])

                if (
                    self.cached_status.get(d_id) == status
                    and self.cached_coords.get(d_id) == coords
                ):
                    continue

                self.cached_status[d_id] = status
                self.cached_coords[d_id] = coords
                converted = self._convert_status(divera_status)
                self.connect_service.post_vehicle_status_by_id(c_id, converted)
                self.logger.info("Synced vehicle %s", d_id)
            except Exception as exc:
                self.logger.error("Error syncing vehicle %s: %s", d_id, exc)

    def _convert_status(self, divera_status: Dict) -> ConnectStatus:
        utc_ts = datetime.datetime.fromtimestamp(
            divera_status['status_ts'], tz=datetime.timezone.utc
        )
        ts = utc_ts.astimezone().isoformat()
        return ConnectStatus(
            Status=divera_status['status'],
            Position=Position(divera_status['lat'], divera_status['lng']),
            StatusTimestamp=ts,
            PositionTimestamp=datetime.datetime.now().astimezone().isoformat(),
        )


def load_config(path: str) -> Dict:
    parser = configparser.ConfigParser()
    parser.read(path)
    cfg = parser['app']
    divera_ids = [i.strip() for i in cfg.get('divera_vehicle_ids', '').split(',') if i.strip()]
    connect_ids = [i.strip() for i in cfg.get('connect_vehicle_ids', '').split(',') if i.strip()]
    interval = int(cfg.get('timer_interval', '60'))
    if interval < 30:
        raise ValueError("Timer interval must not be smaller than 30 seconds")
    return {
        'divera_base': cfg['divera_base_address'],
        'connect_base': cfg['connect_base_address'],
        'divera_key': cfg['divera_api_key'],
        'connect_key': cfg['connect_api_key'],
        'interval': interval,
        'divera_ids': divera_ids,
        'connect_ids': connect_ids,
    }


def main() -> None:
    config = load_config('config.ini')
    logging.basicConfig(level=logging.INFO, format='%(asctime)s [%(levelname)s] %(message)s')
    logger = logging.getLogger('diverafmsconnect')

    connect_api = ConnectApiService(config['connect_base'], config['connect_key'])
    divera_api = DiveraApiService(config['divera_base'], config['divera_key'])
    service = FmsService(connect_api, divera_api,
                         config['divera_ids'], config['connect_ids'], logger)

    service.initial_sync()
    interval = config['interval']
    logger.info("Starting sync loop with interval %s seconds", interval)
    while True:
        time.sleep(interval)
        logger.info("Running sync cycle")
        service.sync()


if __name__ == '__main__':
    main()
