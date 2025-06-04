import logging
from unittest import mock

import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parents[1]))
import diverafmsconnect as dfc


def make_status(status=1, lat=1.0, lng=2.0, ts=0):
    return {'status': status, 'lat': lat, 'lng': lng, 'status_ts': ts}


def setup_service():
    connect = mock.Mock(spec=dfc.ConnectApiService)
    divera = mock.Mock(spec=dfc.DiveraApiService)
    logger = logging.getLogger('test')
    service = dfc.FmsService(connect, divera, ['d1'], ['c1'], logger)
    return service, connect, divera


def test_sync_no_change_does_not_post():
    service, connect, divera = setup_service()
    divera.get_vehicle_status_by_id.return_value = make_status()

    service.initial_sync()
    assert connect.post_vehicle_status_by_id.called
    connect.post_vehicle_status_by_id.reset_mock()

    divera.get_vehicle_status_by_id.return_value = make_status()
    service.sync()

    connect.post_vehicle_status_by_id.assert_not_called()


def test_sync_status_change_triggers_post():
    service, connect, divera = setup_service()
    divera.get_vehicle_status_by_id.return_value = make_status(1)
    service.initial_sync()
    connect.post_vehicle_status_by_id.reset_mock()

    divera.get_vehicle_status_by_id.return_value = make_status(2)
    service.sync()

    connect.post_vehicle_status_by_id.assert_called_once()
