/**
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
 * PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
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
 * C# / .NET Port Copyright (c) 2014, Eric Snyder / elssystems
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using WE = Windows.Devices.Enumeration;

namespace MetaWearWinStoreAPI
{
    public static class EnumExtenions
    {
        public static int Ordinal(this LED.ColorChannel c)
        {
            int i = 0;
            foreach (LED.ColorChannel v in Enum.GetValues(typeof(LED.ColorChannel)))
            {
                if ((int)v == (int)c)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public static int Ordinal(this LED.State s)
        {
            int i = 0;
            foreach (LED.State v in Enum.GetValues(typeof(LED.State)))
            {
                if ((int)v == (int)s)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public static int Ordinal(this NeoPixel.ColorOrdering o)
        {
            int i = 0;
            foreach (NeoPixel.ColorOrdering v in Enum.GetValues(typeof(NeoPixel.ColorOrdering)))
            {
                if ((int)v == (int)o)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public static int Ordinal(this NeoPixel.StrandSpeed s)
        {
            int i = 0;
            foreach (NeoPixel.StrandSpeed v in Enum.GetValues(typeof(NeoPixel.StrandSpeed)))
            {
                if ((int)v == (int)s)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public static int Ordinal(this NeoPixel.RotationDirection d)
        {
            int i = 0;
            foreach (NeoPixel.RotationDirection v in Enum.GetValues(typeof(NeoPixel.RotationDirection)))
            {
                if ((int)v == (int)d)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }
    }
}