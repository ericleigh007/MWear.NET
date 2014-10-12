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
     * Controller for the temperature module
     * @author Eric Tsai
     * @port Eric Snyder
     * @see com.mbientlab.metawear.api.Module#TEMPERATURE
     */
    public abstract class Temperature : MetaWearController.ModuleController
    {
        /**
         * Enumeration of registers under the temperature module
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Retrieves the current temperature from the sensor */
            public static readonly Register TEMPERATURE = new Register(0x1, "Temperature",TEMPERATURE_Notify_type.Temperature);

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return TEMPERATURE;
                }
            }

            public string name { get; private set; }
            public TEMPERATURE_Notify_type notifyType { get; private set; }

            public enum TEMPERATURE_Notify_type
            {
                None,
                Temperature
            };

            private Register(byte setID, string theName, TEMPERATURE_Notify_type notify_type = TEMPERATURE_Notify_type.None)
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

            public override byte opcode()
            {
                return regID;
            }

            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks,
                    byte[] data)
            {
                if (notifyType == TEMPERATURE_Notify_type.None) return;

                switch( notifyType )
                {
                    case TEMPERATURE_Notify_type.Temperature:
                    {
                        // Java - byte reverse = new byte[] {data[3],data[2]};
                        // Java - big endian float degrees = (float)(Short.valueOf(ByteBuffer.wrap(reverse).getShort()).floatValue() / 4.0);

                        float degrees = 0.0f;
                        if (BitConverter.IsLittleEndian)
                        {
                            degrees = ((BitConverter.ToInt16(data, 2)) / 4.0f);
                        }
                        else
                        {
                            byte temp = data[3];
                            data[3] = data[2];
                            data[2] = temp;
                            degrees = ((BitConverter.ToInt16(data, 2)) / 4.0f);
                        }

                        foreach (Callbacks cb in callbacks)
                        {
                            cb.receivedTemperature(degrees);
                        }

                        return;
                    }
                }
            }

            public override Module module() { return Module.MWMOD_TEMPERATURE; }
        }

        /**
         * Callbacks for temperature module
         * @author Eric Tsai
         */
        public abstract class Callbacks : MetaWearController.ModuleCallbacks
        {
            public override Module getModule() { return Module.MWMOD_TEMPERATURE; }

            /**
             * Called when MetaWear has responded with the temperature reading
             * @param degrees Value of the temperature in Celsius
             */
            public abstract void receivedTemperature(float degrees);
        }

        /**
         * Read the temperature reported from MetaWear.
         * The function Temperature.receivedTemperature will be called the the data is available
         * @see Callbacks#receivedTemperature(float)
         */
        public abstract void readTemperature();


    }
}
