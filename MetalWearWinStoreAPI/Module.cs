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
     * Enumeration of modules on the MetaWear board.  A module can be thought of as a supported 
     * service by the MetaWear board.  Each module is designed for a specific purpose and 
     * communicates with the user via a collection of registers.
     * @author Eric Tsai
     * @port Eric Snyder
     * @see Register
     */

    public class Module
    {
        /** Miniature push button switch */
        public static readonly Module MWMOD_MECHANICAL_SWITCH = new Module("Mechanical Switch" , 0x01, MechanicalSwitch.Register.GetValues());
        /** Ultra-Bright RGB LED */
        public static readonly Module MWMOD_LED = new Module("LED" , 0x2, LED.Register.GetValues());
        /** 3-axis accelerometer */
        public static readonly Module MWMOD_ACCELEROMETER = new Module("Accelerometer" , 0x3, Accelerometer.Register.GetValues());
        /** Temperature sensor */
        public static readonly Module MWMOD_TEMPERATURE = new Module("Temperature" , 0x4, Temperature.Register.GetValues());
        /** General purpose I/O*/
        public static readonly Module MWMOD_GPIO = new Module("GPIO" , 0x5, GPIO.Register.GetValues());
        /** Neo pixel */
        public static readonly Module MWMOD_NEO_PIXEL = new Module("NeoPixel" , 0x6, NeoPixel.Register.GetValues());
        /** IBeacon  */
        public static readonly Module MWMOD_IBEACON = new Module("iBeacon" , 0x7, IBeacon.Register.GetValues());
        public static readonly Module MWMOD_HAPTIC = new Module("Haptic" , 0x8, Haptic.Register.GetValues());
        /** Debug mode for testing purposes */
        public static readonly Module MWMOD_DEBUG = new Module("Debug" , 0xfe, Debug.Register.GetValues());

        /** Opcode of the module */
        public byte modID { get; private set; }
        public string name { get; private set; }

        public static Boolean notifyEnabled = false;

        private Dictionary<Byte, APIRegister> registers = new Dictionary<Byte, APIRegister>();

        public static Dictionary<Byte, Module> opcodeMap = null;

        private Module(string theName , byte modId, List<APIRegister> registers)
        {
            this.modID = modId;
            this.name = theName;

            foreach (APIRegister reg in registers)
            {
                if (this.registers.ContainsKey(reg.opcode()))
                {
                    //* hack until we understand it *//
//                    throw new Exception(string.Format("Duplicate opcpode found (opcpode = {0})",
//                            reg.opcode()));
                }
                this.registers[reg.opcode()] = reg;
            }
        }

        public static IEnumerable<Module> Values
        {
            get
            {
                yield return MWMOD_MECHANICAL_SWITCH;
                yield return MWMOD_LED;
                yield return MWMOD_ACCELEROMETER;
                yield return MWMOD_TEMPERATURE;
                yield return MWMOD_GPIO;
                yield return MWMOD_NEO_PIXEL;
                yield return MWMOD_IBEACON;
                yield return MWMOD_HAPTIC;
                yield return MWMOD_DEBUG;
            }
        }

        public static List<Module> GetValues()
        {
            List<Module> tmp_list = new List<Module>();
            foreach (Module m in Values)
            {
                tmp_list.Add(m);
            }

            return tmp_list;
        }

        public static Module GetByName( string theName )
        {
            foreach( Module m in Values)
            {
                if (m.name == theName )
                {
                    return m;
                }
            }

            return null;
        }

        public static void InitializeModuleRegisterCrossReference()
        {
            opcodeMap = new Dictionary<Byte, Module>();
            foreach (Module m in Values)
            {
                if (opcodeMap.ContainsKey(m.modID))
                {
                    throw new Exception(String.Format("Duplicate opcpode found for module '{0}' and '{1}'",
                            opcodeMap[m.modID].name, m.name));
                }
                opcodeMap[m.modID] = m;
            }

            notifyEnabled = true;
        }
            
        /**
         * Find the register belonging to the module with the specific opcode
         * @param opcode Register opcode to search
         * @return The register with the matching opcode, or null if no match is found
         */
        public APIRegister lookupRegister(byte opcode)
        {
            return registers[opcode];
        }
        /**
         * Find the Module with the opcode
         * @param opcode Module opcode to search
         * @return Module with the matching opcode, or null if no match is found
         */
        public static Module lookupModule(byte opcode)
        {
            // note opcodeMap will be initalized by the getter if it isn't alraedy
            return opcodeMap[opcode];
        }
    }
}

