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
 * @port Eric Snyder
 * @see com.mbientlab.metawear.api.GATT.GATTService#DEVICE_INFORMATION
 */
    public class MetaWear : GATTCharacteristic
    {
        /** UUID for issuing commands to MetaWear*/
        public static readonly MetaWear COMMAND = new MetaWear("COMMAND", "9001", true, true);
        /** UUID for receiving notifications from MetaWear i.e. reads or characteristic changes */
        public static readonly MetaWear NOTIFICATION_1 = new MetaWear("NOTIFICATION_1" , "9006", true , false , true );
        /** UUID for receiving notifications from MetaWear i.e. reads or characteristic changes */
        public static readonly MetaWear NOTIFICATION_2 = new MetaWear("NOTIFICATION_2", "9007", false );
        /** UUID for receiving notifications from MetaWear i.e. reads or characteristic changes */
        public static readonly MetaWear NOTIFICATION_3 = new MetaWear("NOTIFICATION_3", "9008", false );

        private MetaWear(string theName , string uuidPart , Boolean on = true, Boolean writeableChar = false , Boolean canNotify = false)
        {
            this.uuid = new Guid(string.Format("326A{0:S}-85CB-9195-D9DD-464CFBBAE75A", uuidPart));
            this.name = theName;
            this.enabled = on;
            this.writable = writeableChar;
            this.notifyable = canNotify;
        }

        public Guid Getuuid()
        {
            return uuid;
        }

        public GATTService gattService() { return GATTService.METAWEAR; }

        public static IEnumerable<MetaWear> Values
        {
            get
            {
                yield return COMMAND;
                yield return NOTIFICATION_1;
                yield return NOTIFICATION_2;
                yield return NOTIFICATION_3;
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

        public static GATTCharacteristic GetCharacteristic(string theName)
        {
            foreach( GATTCharacteristic gc in Values)
            {
                if ( gc.ToString() == theName)
                {
                    return gc;
                }
            }

            return null;
        }
    }
}

