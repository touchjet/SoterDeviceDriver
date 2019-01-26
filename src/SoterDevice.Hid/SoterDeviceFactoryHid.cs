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
using HidSharp;
using Serilog;

namespace SoterDevice.Hid
{
    public class SoterDeviceFactoryHid : ISoterDeviceFactory
    {
        private static SoterDeviceFactoryHid _instance;

        public static SoterDeviceFactoryHid Instance
        {
            get
            {
                if (_instance == null) { _instance = new SoterDeviceFactoryHid(); }
                return _instance;
            }
        }

        public SoterDeviceFactoryHid()
        {
            Devices = new ObservableCollection<ISoterDevice>();
        }

        public ObservableCollection<ISoterDevice> Devices { get; private set; }

        public ISoterDevice CurrentDevice { get; private set; }

        CancellationTokenSource cancellationTokenSource;

        public Task StartDeviceSearchAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();
            return Task.Run(
            () =>
            {
                Clear();
                var hidDeviceList = DeviceList.Local.GetHidDevices().ToArray();

                foreach (var device in hidDeviceList)
                {
                    Log.Information($"Found HID Device VID:{device.VendorID} PID:{device.ProductID}  {device.GetProductName()}");
                    if ((device.VendorID == SoterDeviceHid.VID) && (device.ProductID == SoterDeviceHid.PID))
                    {
                        if (device.GetReportDescriptor().DeviceItems.Any(item => item.Usages.GetAllValues().Any(usage => usage == SoterDeviceHid.HID_USAGE)))
                        {
                            try
                            {
                                var _soterDevice = new SoterDeviceHid(device);
                                Devices.Add(_soterDevice);
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"Error adding new soter hid device : {ex.ToString()}");
                            }
                        }
                    }
                }
            },
            cancellationTokenSource.Token);
        }

        public Task StopDeviceSearchAsync()
        {
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public void Clear()
        {
            foreach (var device in Devices)
            {
                ((SoterDeviceHid)device).Dispose();
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

        public Task<bool> ConnectByIdAsync(string deviceId)
        {
            throw new NotImplementedException();
        }
    }
}
