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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
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

        IAdapter adapter;

        public SoterDeviceFactoryBle()
        {
            adapter = CrossBluetoothLE.Current.Adapter;
            Devices = new ObservableCollection<ISoterDevice>();
        }

        public ObservableCollection<ISoterDevice> Devices { get; private set; }

        public ISoterDevice CurrentDevice { get; private set; }

        CancellationTokenSource cancellationTokenSource;

        public async Task StartDeviceSearchAsync()
        {
            Log.Information("Start device search.");
            Clear();
            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            cancellationTokenSource = new CancellationTokenSource();
            await adapter.StartScanningForDevicesAsync(null, null, false, cancellationTokenSource.Token);
        }

        public async Task StopDeviceSearchAsync()
        {
            cancellationTokenSource.Cancel();
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
            foreach (var device in Devices)
            {
                ((SoterDeviceBle)device).Dispose();
            }
            Devices.Clear();
        }

        public async Task<bool> ConnectByNameAsync(string deviceName)
        {
            uint waitCount = 0;
            await StartDeviceSearchAsync();
            while (!Devices.Any(d => d.Name.Equals(deviceName)) && (waitCount < 30))
            {
                await Task.Delay(100);
                waitCount++;
            }
            await StopDeviceSearchAsync();
            CurrentDevice = Devices.FirstOrDefault(d => d.Name.Equals(deviceName));
            return CurrentDevice != null;
        }

        public async Task<bool> ConnectByIdAsync(string deviceId)
        {
            using (var cts = new CancellationTokenSource())
            {
                var connectionTask = adapter.ConnectToKnownDeviceAsync(new Guid(deviceId), cancellationToken: cts.Token);
                uint delayCount = 0;
                while ((!connectionTask.IsCompleted) && (delayCount < 50))
                {
                    await Task.Delay(100);
                    delayCount++;
                }
                if (connectionTask.IsCompleted)
                {
                    CurrentDevice = new SoterDeviceBle(connectionTask.Result, connectionTask.Result.Name);
                    return true;
                }
                else
                {
                    cts.Cancel();
                    return false;
                }
            }
        }
    }
}
