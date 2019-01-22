/*
 * Copyright (C) 2018 Touchjet Limited.
 * 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE;
using Serilog;

namespace SoterDevice.Ble
{
    public class SoterDeviceFactoryBle : ISoterDeviceFactory
    {
        private static SoterDeviceFactoryBle _instance;

        public static SoterDeviceFactoryBle Instance
        {
            get
            {
                if (_instance == null) { _instance = new SoterDeviceFactoryBle(); }
                return _instance;
            }
        }

        public SoterDeviceFactoryBle()
        {
            Devices = new ObservableCollection<ISoterDevice>();
        }

        public ObservableCollection<ISoterDevice> Devices { get; private set; }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public async Task StartDeviceSearchAsync()
        {
            Log.Information("Start device search.");
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered += (s, a) =>
            {
                try
                {
                    if ((!String.IsNullOrWhiteSpace(a.Device.Name)) && a.Device.Name.StartsWith(SoterDeviceBle.DEVICE_NAME_PREFIX, StringComparison.Ordinal))
                    {
                        Log.Information($"Found device  {a.Device.Id} -- {a.Device.Name}");
                        var _soterDevice = new SoterDeviceBle(a.Device, a.Device.Name);
                        Devices.Add(_soterDevice);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            };
            await adapter.StartScanningForDevicesAsync(null, null, false, cancellationTokenSource.Token);
        }

        public async Task StopDeviceSearchAsync()
        {
            cancellationTokenSource.Cancel();
            var adapter = CrossBluetoothLE.Current.Adapter;
            await adapter.StopScanningForDevicesAsync();
        }
    }
}
