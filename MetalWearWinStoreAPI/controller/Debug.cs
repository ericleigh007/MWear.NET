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
     * Controller for debug module
     * @author Eric Tsai
     * @port Eric Snyder
     * @see com.mbientlab.metawear.api.Module#DEBUG
     */
    public abstract class Debug : MetaWearController.ModuleController
    {
        /**
         * Enumeration of registers under the debug module
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Used to restart the board */
            public static readonly Register RESET_DEVICE = new Register(0x1,"Reset_Device");
            /** Switch to bootloader mode */
            public static readonly Register JUMP_TO_BOOTLOADER = new Register(0x2,"Jump_To_Bootloader");

            public string name { get; private set; }

            private Register(byte setID, string theName )
            {
                regID = setID;
                name = theName;
            }

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return RESET_DEVICE;
                    yield return JUMP_TO_BOOTLOADER;
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

            public override Module module() { return Module.MWMOD_DEBUG; }
            public override byte opcode() { return regID; }
            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks, byte[] data)
            {

            }
        }
        /** Restart the board */
        public abstract void resetDevice();
        /** Restart the board in bootloader mode */
        public abstract void jumpToBootloader();

    }
}
