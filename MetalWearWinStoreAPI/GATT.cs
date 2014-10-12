/*
 * Copyright 2014 MbientLab Inc. All rights reserved.
 *
 * IMPORTANT: Your use of this Software is limited to those specific rights
 * granted under the terms of a software license agreement between the user who 
 * downloaded the software, his/her employer (which must be your employer) and 
 * MbientLab Inc, (the "License").  You may not use this Software unless you 
 * agree to abide by the terms of the License which can be found at 
 * www.mbientlab.com/terms . The License limits your use, and you acknowledge, 
 * that the  Software may not be modified, copied or distributed and can be used 
 * solely and exclusively in conjunction with a MbientLab Inc, product.  Other 
 * than for the foregoing purpose, you may not use, reproduce, copy, prepare 
 * derivative works of, modify, distribute, perform, display or sell this 
 * Software and/or its documentation for any purpose.
 *
 * YOU FURTHER ACKNOWLEDGE AND AGREE THAT THE SOFTWARE AND DOCUMENTATION ARE 
 * PROVIDED �AS IS� WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
 * INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF MERCHANTABILITY, TITLE, 
 * NON-INFRINGEMENT AND FITNESS FOR A PARTICULAR PURPOSE. IN NO EVENT SHALL 
 * MBIENTLAB OR ITS LICENSORS BE LIABLE OR OBLIGATED UNDER CONTRACT, NEGLIGENCE, 
 * STRICT LIABILITY, CONTRIBUTION, BREACH OF WARRANTY, OR OTHER LEGAL EQUITABLE 
 * THEORY ANY DIRECT OR INDIRECT DAMAGES OR EXPENSES INCLUDING BUT NOT LIMITED 
 * TO ANY INCIDENTAL, SPECIAL, INDIRECT, PUNITIVE OR CONSEQUENTIAL DAMAGES, LOST 
 * PROFITS OR LOST DATA, COST OF PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY, 
 * SERVICES, OR ANY CLAIMS BY THIRD PARTIES (INCLUDING BUT NOT LIMITED TO ANY 
 * DEFENSE THEREOF), OR OTHER SIMILAR COSTS.
 *
 * Should you have any questions regarding your right to use this Software, 
 * contact MbientLab Inc, at www.mbientlab.com.
 * 
 * C# / .NET Port Copyright (c) 2014, Eric Snyder / elssystems
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MetaWearWinStoreAPI
{
    public interface GATT
    {
    }

    public abstract class GATTCharacteristic : GATT
    {
        public GATTService gattService { get; set; }

        public Boolean enabled { get; protected set; }
        public Boolean encryptable { get; protected set; }
        public Guid uuid { get; protected set; }
        public Boolean notifyable { get; protected set; }
        public Boolean writable { get; protected set; }
        public string name { get; protected set; }
    }

    public class GATTService : GATT
    {
        public static readonly GATTService DEVICE_INFORMATION = new GATTService("180a", DeviceInformation.GetValues());
        public static readonly GATTService METAWEAR = new GATTService( new Guid("326A9000-85CB-9195-D9DD-464CFBBAE75A"), MetaWear.GetValues());
        public static readonly GATTService BATTERY = new GATTService( GattServiceUuids.Battery, Battery.GetValues());

        public GATTService()
        {

        }

        private Dictionary<Guid,GATTService> services;

        public static IEnumerable<GATTService> Values
        {
            get
            {
                yield return DEVICE_INFORMATION;
                yield return METAWEAR;
                yield return BATTERY;
            }
        }

        public static List<GATTService> GetValues()
        {
            List<GATTService> tmp_list = new List<GATTService>();
            foreach (GATTService gc in Values)
            {
                tmp_list.Add(gc);
            }

            return tmp_list;
        }

        public Guid uuid { get; private set; }

        private Dictionary<Guid, GATTCharacteristic> characteristics;

        private GATTService(string uuidPart , List<GATTCharacteristic> characteristics)
        {
            uuid = new Guid((String.Format("0000{0:S}-0000-1000-8000-00805f9b34fb", uuidPart)));

            this.characteristics = new Dictionary<Guid, GATTCharacteristic>();

            foreach (GATTCharacteristic ch in characteristics)
            {
                this.characteristics[ch.uuid] = ch;
            } 
        }

        //** hack figure this one out **//
        private GATTService(Guid uuid, List<GATTCharacteristic> characteristics) 
        { 
            this.uuid = uuid; 
            this.characteristics = new Dictionary<Guid, GATTCharacteristic>();
             
            foreach( GATTCharacteristic ch in characteristics) 
            { 
                this.characteristics[ch.uuid] = ch;
            } 
        }

        // figure out whether this is needed
        public void InitServices()
        {
            services = new Dictionary<Guid, GATTService>();
            foreach( GATTService serv in GATTService.Values)
            {
                services[serv.getuuid()] = serv;
            }
        }

        public GATTCharacteristic getCharacteristic( Guid charUUID )
        {
            return characteristics[charUUID];
        }

        public GATTService getGATTService( Guid servUUID )
        {
            return services[servUUID];
        }

        public Guid getuuid()
        {
            return uuid;
        }
    }
}
