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
     * Controller for the mechanical switch module
     * @author Eric Tsai
     * @see com.mbientlab.metawear.api.Module#MECHANICAL_SWITCH
     */
    public abstract class MechanicalSwitch : MetaWearController.ModuleController
    {
        /**
         * Enumeration of registers under the mechanical switch module
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Reads the state of the button */
            public static readonly Register SWITCH_STATE = new Register(0x1);

            public static List<APIRegister> GetValues()
            {
                List<APIRegister> tmp_list = new List<APIRegister>();
                foreach (APIRegister reg in Values)
                {
                    tmp_list.Add(reg);
                }

                return tmp_list;
            }

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return SWITCH_STATE;
                }
            }

            private Register(byte setID)
            {
                regID = setID;
            }

            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks, 
                    byte[] data) 
            {
                foreach (Callbacks cb in callbacks) 
                {
                    if (data[2] == 0x1) 
                    {
                        cb.pressed();
                    }
                    else 
                    {
                        cb.released();
                    }
                }
            }

            public override byte opcode()
            {
                return regID;
            }
            public override Module module() { return Module.MWMOD_MECHANICAL_SWITCH; }
        }

        /**
         * Callbacks for the mechanical switch module
         * @author Eric Tsai
         */
        public abstract class Callbacks : MetaWearController.ModuleCallbacks
        {
            public override Module getModule() { return Module.MWMOD_MECHANICAL_SWITCH; }

            /** Called when the switch is pressed */
            public abstract void pressed();
            /** Called when the switch is released */
            public abstract void released();
        }

        /** Enable notifications when the switch state changes */
        public abstract void enableNotification();
        /** Disable notifications from the switch */
        public abstract void disableNotification();
    }
}
