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
     * Controller for the IBeacon module
     * @author Eric Tsai
     * @port Eric Snyder
     * @see com.mbientlab.metawear.api.Module#IBEACON
     */
    public abstract class IBeacon : MetaWearController.ModuleController
    {
        /**
         * Enumeration of registers under the IBeacon module.  
         * The registers also function as keys for the IBeacon settings. 
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Checks the enable status and enables/disables IBeacon mode */
            public static readonly Register ENABLE = new Register(0x1, "Enable",IBEACON_Notify_type.Enable);
            /**
             * Contains IBeacon advertisement UUID.  Reading from the register will trigger 
             * a call to receivedUUID with the advertisement uuid
             * @see Callbacks#receivedUUID(UUID)
             */
            public static readonly Register ADVERTISEMENT_UUID = new Register(0x2, "Ad_UUID",IBEACON_Notify_type.Ad_UUID);
            /**
             * Contains advertisement major number.  Reading from the register will trigger 
             * a call to receivedMajor with the major number
             * @see Callbacks#receivedMajor(short)
             */
            public static readonly Register MAJOR = new Register(0x3, "Major",IBEACON_Notify_type.Major);
            /**
             * Contains advertisement minor number.  Reading from the register will trigger 
             * a call to receivedMinor with the minor number
             * @see Callbacks#receivedMinor(short)
             */
            public static readonly Register MINOR = new Register(0x4, "Minor", IBEACON_Notify_type.Minor);
            /**
             * Contains the receiving power.  Reading from the register will trigger 
             * a call to receivedRXPower with the receiving power
             * @see Callbacks#receivedRXPower(byte)
             */
            public static readonly Register RX_POWER = new Register(0x5, "RX_Power",IBEACON_Notify_type.RX_Power);
            /**
             * Contains the transmitting power.  Reading from the register will trigger 
             * a call to receivedTXPower with the transmitting power
             * @see Callbacks#receivedTXPower(byte)
             */
            public static readonly Register TX_POWER = new Register(0x6, "TX_Power",IBEACON_Notify_type.TX_Power);
            /**
             * Contains the advertisement period.  Reading from the register will trigger 
             * a call to receivedPeriod with the advertisement period
             * @see Callbacks#receivedPeriod(short)
             */
            public static readonly Register ADVERTISEMENT_PERIOD = new Register(0x7, "Ad_Period", IBEACON_Notify_type.Ad_Period);

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return ENABLE;
                    yield return ADVERTISEMENT_UUID;
                    yield return MAJOR;
                    yield return MINOR;
                    yield return RX_POWER;
                    yield return TX_POWER;
                    yield return ADVERTISEMENT_PERIOD;
                }
            }

            public string name { get; private set; }
            public IBEACON_Notify_type notifyType { get; private set; }

            public enum IBEACON_Notify_type
            {
                None,
                Enable,
                Ad_UUID,
                Major,
                Minor,
                RX_Power,
                TX_Power,
                Ad_Period
            };

            private Register(byte setID, string theName, IBEACON_Notify_type notify_type = IBEACON_Notify_type.None)
            {
                regID = setID;
                name = theName;
                notifyType = notify_type;
            }

            public static List<APIRegister> GetValues()
            {
                List<APIRegister> tmp_list = new List<APIRegister>();
                foreach (APIRegister reg in Values)
                {
                    tmp_list.Add(reg);
                }

                return tmp_list;
            }

            public override Module module() { return Module.MWMOD_IBEACON; }
            public override byte opcode() { return regID;  }

            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks,
                    byte[] data)
            {
                if (notifyType == IBEACON_Notify_type.None) return;

                // enable

                switch( notifyType )
                {
                    case IBEACON_Notify_type.Enable:
                    {
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedEnableState(data[2]);
                        }

                        return;
                    }

                    case IBEACON_Notify_type.Ad_UUID:
                    {
                        /*
                        Guid uuid= new Guid(ByteBuffer.wrap(data, 2, 8).getLong(), 
                                ByteBuffer.wrap(data, 10, 8).getLong());
                         */
                        /* optimize */
                        byte[] tmp = new byte[16];
                        Buffer.BlockCopy( data , 2 , tmp , 0 , 16);
                        Guid uuid = new Guid(tmp);

                        foreach (Callbacks cb in callbacks)
                        {
                            cb.receivedUUID(uuid);
                        }

                        return;
                    }

                    case IBEACON_Notify_type.Major:
                    {
                        short major = BitConverter.ToInt16(data, 2);
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedMajor(major);
                        }

                        return;
                    }

                    case IBEACON_Notify_type.Minor:
                    {
                        short minor = BitConverter.ToInt16(data, 2);
                        foreach (Callbacks cb in callbacks)
                        {
                            cb.receivedMinor(minor);
                        }

                        return;
                    }

                    case IBEACON_Notify_type.RX_Power:
                    {
                        foreach (Callbacks cb in callbacks)
                        {
                            cb.receivedRXPower(data[2]);
                        }

                        return;
                    }

                    case IBEACON_Notify_type.TX_Power:
                    {
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedTXPower(data[2]);
                        }

                        return;
                    }

                    case IBEACON_Notify_type.Ad_Period:
                    {
                        short period = BitConverter.ToInt16(data, 2);
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedPeriod(period);
                        }

                        return;
                    }
                }
            }
        }
        /**
         * Callbacks for the IBeacon module
         * @author Eric Tsai
         */
        public abstract class Callbacks : MetaWearController.ModuleCallbacks
        {
            public override Module getModule() { return Module.MWMOD_IBEACON; }

            /**
             * Called when the enable state has been read
             * @param state 0 if disabled, 1 if enabled
             */
            public void receivedEnableState(byte state) { }
            /**
             * Called when the advertisement UUID has been read
             * @param uuid Advertisement UUID
             */
            public void receivedUUID(Guid uuid) { }
            /**
             * Called when the major number has been read
             * @param value Value of the major number
             */
            public void receivedMajor(short value) { }
            /**
             * Called when the minor number has been read
             * @param value Value of the minor number
             */
            public void receivedMinor(short value) { }
            /**
             * Called when the calibrated receiving power has been read
             * @param power Calibrated receive power, default is -55dBm
             */
            public void receivedRXPower(byte power) { }
            /**
             * Called when the transmitting power has been read
             * @param power Transmitting power, default is 0dBm
             */
            public void receivedTXPower(byte power) { }
            /**
             * Called when the advertising period has been read
             * @param period Advertising period in milliseconds
             */
            public void receivedPeriod(short period) { }
        }

        /**
         * Enable the IBeacon module
         */
        public abstract void enableIBeacon();
        /**
         * Disable the IBeacon module
         */
        public abstract void disableIBecon();
        /**
         * Reads the IBeacon setting.  
         * @param setting Setting to read
         */
        public abstract void readSetting(Register setting);
        /**
         * Set IBeacon advertisement UUID
         * @param uuid Advertisement UUID *(hack -- look to see if we have to convert this from a Guid)*
         */
        public abstract IBeacon setUUID(Guid uuid);

        /**
         * Set IBeacon advertisement major number
         * @param major Advertisement major number
         */
        public abstract IBeacon setMajor(short major);

        /**
         * Set IBeacon advertisement minor number
         * @param minor Advertisement minor number
         */
        public abstract IBeacon setMinor(short minor);
        /**
         * Set IBeacon receiving power
         * @param power Receiving power
         */
        public abstract IBeacon setCalibratedRXPower(byte power);
        /**
         * Set IBeacon transmitting power
         * @param power Transmitting power
         */
        public abstract IBeacon setTXPower(byte power);
        /**
         * Set IBeacon advertisement period
         * @param period Advertisement period, in milliseconds
         */
        public abstract IBeacon setAdvertisingPeriod(short period);
    }
}
