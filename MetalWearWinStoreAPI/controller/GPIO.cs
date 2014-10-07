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
     * Controller for the general purpose I/O module
     * @see com.mbientlab.metawear.api.Module#GPIO
     * @author Eric Tsai
     * @port Eric Snyder
     */
    public abstract class GPIO : MetaWearController.ModuleController
    {
        /**
         * Enumeration of registers under the GPIO module
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Sets a digital output pin */
            public static readonly Register SET_DIGITAL_OUTPUT = new Register(0x1,"Set_Digital_Input");
            /** Clears a digital output pin */
            public static readonly Register CLEAR_DIGITAL_OUTPUT = new Register(0x2,"Clear_Digital_Input");
            /** Sets a digital input pin in pull up mode */
            public static readonly Register SET_DIGITAL_IN_PULL_UP = new Register(0x3,"Set_Digital_In_Pull_Up");
            /** Sets a digital input pin in pull down mode */
            public static readonly Register SET_DIGITAL_IN_PULL_DOWN = new Register(0x4,"Set_Digital_In_Pull_Down");
            /** Clears a digital input pin in with no pull mode */
            public static readonly Register SET_DIGITAL_IN_NO_PULL = new Register(0x5,"Set_Digtial_In_No_Pull");
            /** Reads the analog input voltage as an absolute value */
            public static readonly Register READ_ANALOG_INPUT_ABS_VOLTAGE = new Register(0x6,"Read_Analog_INput_Abs_Voltage",GPIO_Notify_type.AnalogInputAbsVoltage);
            /** Reads the analog input voltage as a ratio to the supply voltage */
            public static readonly Register READ_ANALOG_INPUT_SUPPLY_RATIO = new Register(0x7, "Read_Analog_Input_Supply_Ratio",GPIO_Notify_type.AnalogInputSupplyRatio);
            /** Reads the value from a digital input pin */
            public static readonly Register READ_DIGITAL_INPUT = new Register(0x8,"Read_Digital_Input",GPIO_Notify_type.ReadDigitalInput);

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return SET_DIGITAL_OUTPUT;
                    yield return CLEAR_DIGITAL_OUTPUT;
                    yield return SET_DIGITAL_IN_PULL_UP;
                    yield return SET_DIGITAL_IN_PULL_DOWN;
                    yield return SET_DIGITAL_IN_NO_PULL;
                    yield return READ_ANALOG_INPUT_ABS_VOLTAGE;
                    yield return READ_ANALOG_INPUT_SUPPLY_RATIO;
                    yield return READ_DIGITAL_INPUT;
                }
            }

            public string name { get; private set; }
            public GPIO_Notify_type notifyType { get; private set; }

            public enum GPIO_Notify_type
            {
                None,
                AnalogInputAbsVoltage,
                AnalogInputSupplyRatio,
                ReadDigitalInput
            };

            private Register(byte setID, string theName, GPIO_Notify_type notify_type = GPIO_Notify_type.None)
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

            public override Module module() { return Module.MWMOD_GPIO; }

            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks,
                    byte[] data)
            {
                if (notifyType == GPIO_Notify_type.None) return;

                switch( notifyType )
                {
                    case GPIO_Notify_type.AnalogInputAbsVoltage:
                    {
                        short value = BitConverter.ToInt16(data, 2);  
                        // ByteBuffer.wrap(data, 2, 2).getShort();
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedAnalogInputAsAbsValue(value);
                        }

                        return;
                    }

                    case GPIO_Notify_type.AnalogInputSupplyRatio:
                    {
                        short value = BitConverter.ToInt16(data, 2);  // ByteBuffer.wrap(data, 2, 2).getShort();
//                        short value = (short)(ByteBuffer.wrap(data, 2, 2).getShort() >> 6);
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedAnalogInputAsSupplyRatio(value);
                        }

                        return;
                    }

                    case GPIO_Notify_type.ReadDigitalInput:
                    {
                        foreach (Callbacks cb in callbacks)
                        {
                            cb.receivedDigitalInput(data[2]);
                        }

                        return;
                    }
                }
            }
        }

        /**
         * Callbacks for the GPIO module 
         * @author Eric Tsai
         */
        public abstract class Callbacks : MetaWearController.ModuleCallbacks
        {
            public override Module getModule() { return Module.MWMOD_GPIO; }

            /**
             * Called when the analog input has been read as an absolute value
             * @param value Value in mV
             */
            public void receivedAnalogInputAsAbsValue(short value) { }
            /**
             * Called when the analog input has been read as a supply ratio
             * @param value 10 bit representation of the voltage where 0 = 0V and 1023 = 3V
             */
            public void receivedAnalogInputAsSupplyRatio(short value) { }
            /**
             * Called when the digital input has been read
             * @param value Either 0 or 1
             */
            public void receivedDigitalInput(byte value) { }
        }

        /**
         * Available reading modes from the GPIO analog pins
         * @author Eric Tsai
         */
        public class AnalogMode
        {
            /**
             * Read voltage as an absolute value
             * @see Callbacks#receivedAnalogInputAsAbsValue(short)
             * @see Register#READ_ANALOG_INPUT_ABS_VOLTAGE
             */
            public static readonly AnalogMode ABSOLUTE_VALUE = new AnalogMode(Register.READ_ANALOG_INPUT_ABS_VOLTAGE);
            /**
             * Read voltage as a supply ratio
             * @see Callbacks#receivedAnalogInputAsSupplyRatio(short)
             * @see Register#READ_ANALOG_INPUT_SUPPLY_RATIO
             */
            public static readonly AnalogMode SUPPLY_RATIO = new AnalogMode(Register.READ_ANALOG_INPUT_SUPPLY_RATIO);

            /** Op code corresponding to the specific read */
            public Register register;

            public static IEnumerable<AnalogMode> Values
            {
                get
                {
                    yield return ABSOLUTE_VALUE;
                    yield return SUPPLY_RATIO;
                }
            }

            /**
             * Construct an enum entry with the desired register
             * @param register Register the enum represents
             */
            private AnalogMode(Register register)
            {
                this.register = register;
            }
        }

        /**
         * Pull modes for setting digital input pins
         * @author Eric Tsai
         */
        public class PullMode
        {
            /**
             * Set with pull up
             * @see Register#SET_DIGITAL_IN_PULL_UP
             */
            public static readonly PullMode UP = new PullMode(Register.SET_DIGITAL_IN_PULL_UP);
            /**
             * Set with pull down
             * @see Register#SET_DIGITAL_IN_PULL_DOWN
             */
            public static readonly PullMode DOWN = new PullMode(Register.SET_DIGITAL_IN_PULL_DOWN);
            /**
             * Set with no pull
             * @see Register#SET_DIGITAL_IN_NO_PULL
             */
            public static readonly PullMode NONE = new PullMode(Register.SET_DIGITAL_IN_NO_PULL);

            public static IEnumerable<PullMode> Values
            {
                get
                {
                    yield return UP;
                    yield return DOWN;
                    yield return NONE;
                }
            }

            public List<PullMode> GetValues()
            {
                List<PullMode> tmp_list = new List<PullMode>();
                foreach(PullMode value in Values)
                {
                    tmp_list.Add(value);
                }

                return tmp_list;
            }

            /** Register corresponding to the pull mode */
            public Register register;

            private PullMode(Register register)
            {
                this.register = register;
            }
        }
        /**
         * Read the value of an analog pin.
         * When the data is ready, GPIO.receivedAnalogInputAsAbsValue will be called if the analog mode 
         * is set to ABSOLUTE_VALUE.  If mode is set to SUPPLY_RATIO, GPIO.receivedAnalogInputAsSupplyRatio 
         * will be called instead 
         * @param pin Pin to read
         * @param mode Read mode on the pin
         * @see Callbacks#receivedAnalogInputAsAbsValue(short)
         * @see Callbacks#receivedAnalogInputAsSupplyRatio(short)
         */
        public abstract void readAnalogInput(byte pin, AnalogMode mode);
        /**
         * Read the value of a digital pin.
         * When data is available, GPIO.receivedDigitalInput will be called
         * @param pin Pin to read
         * @see Callbacks#receivedDigitalInput(byte)
         */
        public abstract void readDigitalInput(byte pin);
        /**
         * Set a digital output pin
         * @param pin Pin to set
         */
        public abstract void setDigitalOutput(byte pin);
        /**
         * Clear a digital output pin
         * @param pin Pin to clear
         */
        public abstract void clearDigitalOutput(byte pin);
        /**
         * Set a digital input pin
         * @param pin Pin to set
         * @param mode Pull mode to use
         */
        public abstract void setDigitalInput(byte pin, PullMode mode);
    }
}
