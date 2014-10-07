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
     * Controller for the accelerometer module
     * @author Eric Tsai
     * @port Eric Snyder
     * @see com.mbientlab.metawear.api.Module#ACCELEROMETER
     */
    public abstract class Accelerometer : MetaWearController.ModuleController
    {
        /**
         * Enumeration of registers for the accelerometer module
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Checks module enable status and enables/disables module notifications */
            public static readonly Register GLOBAL_ENABLE = new Register(0x1, "Global_Enable");
            /** Checks motion polling status and enables/disables motion polling */
            public static readonly Register DATA_ENABLE = new Register(0x2,"Data_Enable");
            /** Sets or retrieves motion polling configuration */
            public static readonly Register DATA_SETTINGS = new Register(0x3,"Data_Settings");
            /** Stores XYZ motion data. */
            public static readonly Register DATA_VALUE = new Register(0x4, "Data_Value", ACCEL_Notify_type.AccelValue);
            /** Checks free fall detection status and enables/disables free fall detection */
            public static readonly Register FREE_FALL_ENABLE = new Register(0x5, "Free_Fall_Enable");
            /** Sets or retrieves free fall detection configuration */
            public static readonly Register FREE_FALL_SETTINGS = new Register(0x6, "Free_Fall_Settings");
            /** Stores free fall state */
            public static readonly Register FREE_FALL_VALUE = new Register(0x7, "Free_Fall_Value",ACCEL_Notify_type.FreeFallBoolean);
            /*
             **/
            /** Sets or retrieves orientation notification status, and enables/disables orientation notifications */
            public static readonly Register ORIENTATION_ENABLE = new Register(0x8, "Orientation_Enable");
            /** Sets or retrieves the configuration for orientation notifications */
            public static readonly Register ORIENTATION_SETTING = new Register(0x9,"Orientation_Settings");
            /** Stores current orientation */
            public static readonly Register ORIENTATION_VALUE = new Register(0xa, "Orientation_Value", ACCEL_Notify_type.OrientationValue);

            public override Module module() { return Module.MWMOD_ACCELEROMETER; }

            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks, byte[] data)
            {
                if (notifyType == ACCEL_Notify_type.None) return;

                switch( notifyType )
                {
                    case ACCEL_Notify_type.FreeFallBoolean:
                    {
                        foreach (Callbacks cb in callbacks) 
                        {
                            if (data[2] != 0)
                            {
                                cb.inFreeFall();
                            }
                            else
                            {
                                cb.stoppedFreeFall();
                            }
                        }

                        return;
                    }

                    case ACCEL_Notify_type.AccelValue:
                    {
                        /*
                        short x= (short)(ByteBuffer.wrap(data, 2, 2).getShort() >> 4), 
                                y= (short)(ByteBuffer.wrap(data, 4, 2).getShort() >> 4), 
                                z= (short)(ByteBuffer.wrap(data, 6, 2).getShort() >> 4);
                         */
                        short x = (short) ( BitConverter.ToInt16(data,2) >> 4);
                        short y = (short) ( BitConverter.ToInt16(data,4) >> 4);
                        short z = (short) ( BitConverter.ToInt16(data,6) >> 4);

                        foreach(Callbacks cb in callbacks) 
                        {
                            cb.receivedDataValue(x, y, z);
                        }
                        
                        return;
                    }

                    case ACCEL_Notify_type.OrientationValue:
                    {
                        foreach(Callbacks cb in callbacks) 
                        {
                            cb.receivedOrientation(data[2]);
                        }

                        return;
                    }
                }
            }

            public string name { get; private set; }
            public ACCEL_Notify_type notifyType { get; private set; }

            public override byte opcode()
            {
                return regID;
            }

            public enum ACCEL_Notify_type
            {
                None,
                FreeFallBoolean,
                AccelValue,
                OrientationValue
            };

            private Register(byte setID, string theName , ACCEL_Notify_type notify_type = ACCEL_Notify_type.None)
            {
                regID = setID;
                name = theName;
                notifyType = notify_type;
            }

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return GLOBAL_ENABLE;
                    yield return DATA_ENABLE;
                    yield return DATA_SETTINGS;
                    yield return DATA_VALUE;
                    yield return FREE_FALL_ENABLE;
                    yield return FREE_FALL_SETTINGS;
                    yield return FREE_FALL_VALUE;
                    yield return ORIENTATION_ENABLE;
                    yield return ORIENTATION_SETTING;
                    yield return ORIENTATION_VALUE;
                }
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
        }
        /**
         * Callbacks for the accelerometer module
         * @author Eric Tsai
         * @port Eric Snyder
         */
        public abstract class Callbacks : MetaWearController.ModuleCallbacks
        {
            public override Module getModule() { return Module.MWMOD_ACCELEROMETER; }

            /**
             * Called when the accelerometer has sent its XYZ motion data
             * @param x X component of the motion
             * @param y Y component of the motion
             * @param z Z component of the motion
             */
            public abstract void receivedDataValue(short x, short y, short z);
            /** Called when free fall is detected */
            public abstract void inFreeFall();
            /** Called when free fall has stopped */
            public abstract void stoppedFreeFall();
            /** Called when the orientation has changed */
            public abstract void receivedOrientation(byte orientation);
        }

        /**
         * Enumeration of components in the accelerometer
         * @author Eric Tsai
         */
        public class Component
        {
            public static readonly Component DATA = new Component(Register.DATA_ENABLE, Register.DATA_SETTINGS, Register.DATA_VALUE);
            public static readonly Component FREE_FALL = new Component(Register.FREE_FALL_ENABLE, Register.FREE_FALL_SETTINGS, Register.FREE_FALL_VALUE);
            public static readonly Component ORIENTATION = new Component(Register.ORIENTATION_ENABLE, Register.ORIENTATION_SETTING, Register.ORIENTATION_VALUE);

            public Register enable, config, status;

            /**
             * @param enable
             * @param config
             */
            private Component(Register enable, Register config, Register status)
            {
                this.enable = enable;
                this.config = config;
                this.status = status;
            }

        }
        /**
         * Enable notifications from a component
         * @param component Component to enable notifications from
         */
        public abstract void enableNotification(Component component);
        /**
         * Disable notifications from a component
         * @param component Component to disable notifications from
         */
        public abstract void disableNotification(Component component);

        /**
         * Read component configuration
         * @param component Component to read configuration from
         */
        public abstract void readComponentConfiguration(Component component);
        /**
         * Set component configuration
         * @param component Component to write configuration to
         */
        public abstract void setComponentConfiguration(Component component, byte[] data);

    }
}

