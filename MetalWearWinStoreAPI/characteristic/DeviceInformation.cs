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

namespace MetaWearWinStoreAPI
{
/**
 * Enumeration of GATT characteristics specific to Device Information service.
 * @author Eric Tsai
 * @see com.mbientlab.metawear.api.GATT.GATTService#DEVICE_INFORMATION
 */
    public class DeviceInformation : GATTCharacteristic
    {
        /** Manufacturer's name i.e. Mbient Lab*/
        public static readonly DeviceInformation MANUFACTURER_NAME = new DeviceInformation("29");
        /** Serial number of the device */
        public static readonly DeviceInformation SERIAL_NUMBER = new DeviceInformation("25");

        /** Firmware version on the device */
        public static readonly DeviceInformation FIRMWARE_VERSION = new DeviceInformation("26");
        /** Hardware version of the device */
        public static readonly DeviceInformation HARDWARE_VERSION = new DeviceInformation("27");

        public Guid uuid { get; private set; }

        private DeviceInformation(string uuidPart)
        {
            this.uuid = new Guid(string.Format("00002a{0:S}-0000-1000-8000-00805f9b34fb", uuidPart));
        }
        public Guid Getuuid()
        {
            return uuid;
        }

        public GATTService gattService() { return GATTService.DEVICE_INFORMATION; }

        public static IEnumerable<DeviceInformation> Values
        {
            get
            {
                yield return MANUFACTURER_NAME;
                yield return SERIAL_NUMBER;
                yield return FIRMWARE_VERSION;
                yield return HARDWARE_VERSION;
            }
        }

        public static List<GATTCharacteristic> GetValues()
        {
            List<GATTCharacteristic> tmp_list = new List<GATTCharacteristic>();
            foreach (GATTCharacteristic gc in Values)
            {
                tmp_list.Add(gc);
            }

            return tmp_list;
        }

    }
}
