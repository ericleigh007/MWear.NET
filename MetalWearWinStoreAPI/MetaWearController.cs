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

namespace MetaWearWinStoreAPI
{
    public abstract class MetaWearController
    {
        /** 
         * Generic type for classes that communicate with a MetaWear module 
         * @author Eric Tsai 
         * @port Eric Snyder
         * @see Module 
         */
        public class ModuleController
        {

        }

        /** 
         * Callback functions for MetaWear specific events.  These callbacks are for  
         * receiving data from a register, triggered by a read or notify request. 
         * @author Eric Tsai 
         * @see Module 
         * @see Register 
         */
        public abstract class ModuleCallbacks
        {
            /** 
             * Get the module the notification represents 
             * @return Module enum 
             * @see Module 
             */
            public abstract Module getModule();
        }


        /** 
         * Callbacks for general Bluetooth Le events 
         * @author Eric Tsai 
         */
        public abstract class DeviceCallbacks
        {
            /** 
             * Bluetooth device connected 
             */
            public void connected()
            {
            }
            /** 
             * Bluetooth device disconnected 
             */
            public void disconnected()
            {
            }
            /** 
             * The data from a GATT characteristic was received 
             * @param characteristic Characteristic that was read 
             * @param data Bytes read from the characteristic 
             */
            public void receivedGATTCharacteristic(GATTCharacteristic characteristic, byte[] data)
            {
            }

        }
    /** 
        * Get the controller for the desired module 
        * @param module Module to control 
        * @return Controller for the desired module 
        */
        public abstract ModuleController getModuleController(Module module);

        /** 
            * Reads general device information from the MetaWear board.  The information available is  
            * listed in the DeviceInformation enum and the receivedGATTCharacteristic function will be  
            * called for each characteristic that has been read 
            * @see DeviceCallbacks#receivedGATTCharacteristic(com.mbientlab.metawear.api.GATT.GATTCharacteristic, byte[]) 
            * @see com.mbientlab.metawear.api.characteristic.DeviceInformation 
            */
        public abstract void readDeviceInformationAsync();

        /** 
            * Add a module callback for the broadcast receiver 
            * @param callback Module callback to add 
            * @return The calling object 
            */
        public abstract MetaWearController addModuleCallback(ModuleCallbacks callback);
        /** 
            * Add a device callback for the broadcast receiver 
            * @param callback Device callback to add 
            * @return The calling object 
            */
        public abstract MetaWearController addDeviceCallback(DeviceCallbacks callback);

        public abstract void removeModuleCallback(ModuleCallbacks callback);

        public abstract void removeDeviceCallback(DeviceCallbacks callback);

        public abstract bool isConnected();
    }
}