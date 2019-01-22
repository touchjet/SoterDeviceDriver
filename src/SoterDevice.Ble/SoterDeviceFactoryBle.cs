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

        CancellationTokenSource cancellationTokenSource;

        public async Task StartDeviceSearchAsync()
        {
            Log.Information("Start device search.");
            Clear();
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            cancellationTokenSource = new CancellationTokenSource();
            await adapter.StartScanningForDevicesAsync(null, null, false, cancellationTokenSource.Token);
        }

        public async Task StopDeviceSearchAsync()
        {
            cancellationTokenSource.Cancel();
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered -= Adapter_DeviceDiscovered;
            await adapter.StopScanningForDevicesAsync();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                if ((!String.IsNullOrWhiteSpace(e.Device.Name)) && e.Device.Name.StartsWith("SOTW_", StringComparison.Ordinal))
                {
                    Log.Information($"Found device  {e.Device.Id} -- {e.Device.Name}");
                    var _soterDevice = new SoterDeviceBle(e.Device, e.Device.Name);
                    Devices.Add(_soterDevice);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public void Clear()
        {
            foreach(var device in Devices)
            {
                ((SoterDeviceBle)device).Dispose();
            }
            Devices.Clear();
        }
    }
}
