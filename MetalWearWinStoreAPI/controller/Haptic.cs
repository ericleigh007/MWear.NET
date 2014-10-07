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
     * Controller for the Haptic Driver module
     * @author Eric Tsai
     * @port Eric Snyder
     */
    public abstract class Haptic : MetaWearController.ModuleController 
    {
        /**
         * Enumeration of registers under the Haptic Driver module
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Starts pulsing a buzzer or motor */
            public static readonly Register PULSE = new Register(0x1);

            public override byte opcode() { return regID;  }

            public override Module module() { return Module.MWMOD_HAPTIC; }

            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks,
                    byte[] data) 
            { 
                // none required
            }

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return PULSE;
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

            private Register(byte setID)
            {
                regID = setID;
            }
        }

        /**
         * Start pulsing a motor
         * @param pulseWidth How long to run the motor (ms)
         */
        public abstract void startMotor(short pulseWidth);
        /**
         * Start pulsing a buzzer
         * @param pulseWidth How long to run the buzzer (ms)
         */
        public abstract void startBuzzer(short pulseWidth);
    }
}
