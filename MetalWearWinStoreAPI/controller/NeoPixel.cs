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
     * Controller for the Neo Pixel module
     * @author Eric Tsai
     * @port Eric Snyder
     * @see com.mbientlab.metawear.api.Module#NEO_PIXEL
     */
    public abstract class NeoPixel : MetaWearController.ModuleController
    {
        /**
         * Enumeration of registers under the Neo Pixel module
         * @author Eric Tsai
         */
        public class Register : APIRegister
        {
            /** Initializes a strand and retrieves a strand state */
            public static readonly Register INITIALIZE = new Register(0x1, "Initialize", NEOPIXEL_Notify_type.StrandState);
            /** Sets and retrives a strand's hold state */
            public static readonly Register HOLD = new Register(0x2, "Hold" , NEOPIXEL_Notify_type.HoldState);
            /** Clears pixels on a strand */
            public static readonly Register CLEAR = new Register(0x3,"Clear");
            /** Sets or retrieves pixel color on a strand */
            public static readonly Register PIXEL = new Register(0x4, "Pixel",NEOPIXEL_Notify_type.PixelColor);
            /** Sets or retrieves rotation state */
            public static readonly Register ROTATE = new Register(0x5, "Rotate",NEOPIXEL_Notify_type.RotationState);
            /** Frees up the resources on a strand */
            public static readonly Register DEINITIALIZE = new Register(0x6,"Deinitialize");

            public static IEnumerable<Register> Values
            {
                get
                {
                    yield return INITIALIZE;
                    yield return HOLD;
                    yield return CLEAR;
                    yield return PIXEL;
                    yield return ROTATE;
                    yield return ROTATE;
                    yield return DEINITIALIZE;
                }
            }

            public string name { get; private set; }
            public NEOPIXEL_Notify_type notifyType { get; private set; }

            public enum NEOPIXEL_Notify_type
            {
                None,
                StrandState,
                HoldState,
                PixelColor,
                RotationState
            };

            private Register(byte setID, string theName, NEOPIXEL_Notify_type notify_type = NEOPIXEL_Notify_type.None)
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
            public override Module module() { return Module.MWMOD_NEO_PIXEL; }

            public override void notifyCallbacks(List<MetaWearController.ModuleCallbacks> callbacks,
                    byte[] data)
            {
                if ( notifyType == NEOPIXEL_Notify_type.None ) return;

                switch( notifyType )
                {
                    case NEOPIXEL_Notify_type.StrandState:
                    {
//                        StrandSpeed speed= StrandSpeed.values[(byte)(data[3] & 0xf)];
                        StrandSpeed speed = (StrandSpeed) (Enum.GetValues(typeof(StrandSpeed)) as byte[])       [(byte)(data[3] & 0xf)];
                        ColorOrdering order = (ColorOrdering) (Enum.GetValues(typeof(ColorOrdering)) as byte[]) [(byte)((data[3] >> 4) & 0xf)];
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedStrandState(data[2], order, speed, data[4], data[5]);
                        }

                        return;
                    }

                    case NEOPIXEL_Notify_type.HoldState:
                    {
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedHoldState(data[2], data[3]);
                        }

                        return;
                    }

                    case NEOPIXEL_Notify_type.PixelColor:
                    {
                        foreach (Callbacks cb in callbacks) 
                        {
                            cb.receivedPixelColor(data[2], data[3], data[4], data[5], data[6]);
                        }

                        return;
                    }

                    case NEOPIXEL_Notify_type.RotationState:
                    {

                        RotationDirection direction= (RotationDirection) (Enum.GetValues(typeof(RotationDirection)) as byte[])[data[3]];
//                        short delay= ByteBuffer.wrap(data, 5, 2).getShort();
                        short delay = BitConverter.ToInt16(data, 5);
                        foreach (Callbacks cb in callbacks)
                        {
                            cb.receivedRotatationState(data[2], direction, data[4], delay);
                        }

                        return;
                    }
                }
            }
        }
        /**
         * Callbacks for NeoPixel module
         * @author Eric Tsai
         */
        public abstract class Callbacks : MetaWearController.ModuleCallbacks
        {
            public override Module getModule() { return Module.MWMOD_NEO_PIXEL; }

            /**
             * Called when the strand state has been read
             * @param strandIndex Strand index read
             * @param order Color ordering of the specific strand
             * @param speed Interface speed of the strand
             * @param pin Pin number on the MetaWear board the NeoPixel strand is connected to 
             * @param strandLength Number of pixels on the strand
             */
            public void receivedStrandState(byte strandIndex, ColorOrdering order, StrandSpeed speed, byte pin, byte strandLength) { }
            /**
             * Called when the strand's hold state has been read
             * @param strandIndex Strand index read
             * @param state 0 if disabled, 1 if enabled
             */
            public void receivedHoldState(byte strandIndex, byte state) { }
            /**
             * Called when a pixel color has been read
             * @param strandIndex Strand index the pixel resides on
             * @param pixel Index of the pixel
             * @param red Red color value
             * @param green Green color value
             * @param blue Blue color value
             */
            public void receivedPixelColor(byte strandIndex, byte pixel, byte red, byte green, byte blue) { }
            /**
             * Called when the rotate state of a strand has been read 
             * @param strandIndex Strand index read
             * @param direction 0 if shifting away from the board, 1 if shifting towards the board
             * @param repetitions Number of times the rotation will occur, -1 if rotation is happening indefinitely
             * @param period Delay between rotations in milliseconds
             */
            public void receivedRotatationState(byte strandIndex, RotationDirection direction, byte repetitions, short period) { }
        }

        /**
         * Color ordering for the NeoPixel color values
         * @author Eric Tsai
         */
        public enum ColorOrdering
        {
            /** Red, green, blue order */
            MW_WS2811_RGB,
            /** Red, blue, green order */
            MW_WS2811_RBG,
            /** Green, red, blue order */
            MW_WS2811_GRB,
            /** Green, blue, red order */
            MW_WS2811_GBR

            //        public static final ColorOrdering values[]= values();
        }

        /**
         * Operating speeds for a NeoPixel strand
         * @author Eric Tsai
         */
        public enum StrandSpeed
        {
            /** 400 kHz */
            SLOW,
            /** 800 kHz */
            FAST

            //        public static final StrandSpeed values[]= values();
        }

        /**
         * Enumeration of rotation directions
         * @author Eric Tsai
         */
        public enum RotationDirection
        {
            /** Move pixels away from the board */
            AWAY,
            /** Move pixels towards the board */
            TOWARDS

            // public static final RotationDirection values[]= values();
        }

        /**
         * Initialize a NeoPixel strand
         * @param strand Index to initialize
         * @param ordering Color ordering format
         * @param speed Operating speed
         * @param ioPin MetaWear pin number the strand is connected to
         * @param length Number of pixels to initialize
         */
        public abstract void initializeStrand(byte strand, ColorOrdering ordering, StrandSpeed speed, byte ioPin, byte length);
        /**
         * Read the state of a NeoPixel strand.  When data is available, the receivedStrandState function will be called
         * @param strand Strand index to read information on
         * @see Callbacks#receivedStrandState(byte, 
         * com.mbientlab.metawear.api.controller.NeoPixel.ColorOrdering, 
         * com.mbientlab.metawear.api.controller.NeoPixel.StrandSpeed, byte, byte)
         */
        public abstract void readStrandState(byte strand);
        /**
         * Set the hold state on a strand 
         * @param strand Strand to set
         * @param holdState 0 to disable, 1 to enable
         */
        public abstract void holdStrand(byte strand, byte holdState);
        /**
         * Read the hold state of a NeoPixel strand.  When data is available, the receivedHoldState function will be called
         * @param strand Strand index to read the hold state on
         * @see Callbacks#receivedHoldState(byte, byte)
         */
        public abstract void readHoldState(byte strand);
        /**
         * Clear pixel states on a strand
         * @param strand Strand index to clear
         * @param start Pixel index to start clearing from
         * @param end Pixel index to clear to
         */
        public abstract void clearStrand(byte strand, byte start, byte end);
        /**
         * Set pixel color
         * @param strand Strand index the pixel is on
         * @param pixel Index of the pixel
         * @param red Red value
         * @param green Green value
         * @param blue Blue value
         */
        public abstract void setPixel(byte strand, byte pixel, byte red, byte green, byte blue);
        /**
         * Read pixel color.  When data is available, the receivedPixelColor function will be called
         * @param strand Strand index the pixel resides on
         * @param pixel Pixel index to read the color from
         * @see Callbacks#receivedPixelColor(byte, byte, byte, byte, byte)
         */
        public abstract void readPixelState(byte strand, byte pixel);
        /**
         * Rotate the pixels on a strand 
         * @param strand Strand to rotate
         * @param direction 0 to shift away from the board, 1 to shift towards
         * @param repetitions Number of extra times to repeat the rotation, -1 to rotate indefinitely
         * @param period Amount of time, in milliseconds, between rotations
         */
        public abstract void rotateStrand(byte strand, RotationDirection direction, byte repetitions, short period);
        /**
         * Read rotation state of a NeoPixel strand.  When data is available, the receivedRotatationStatefunction will be called
         * @param strand Strand index to read
         * @see Callbacks#receivedRotatationState(byte, 
         * com.mbientlab.metawear.api.controller.NeoPixel.RotationDirection, byte, short)
         */
        public abstract void readRotationState(byte strand);
        /**
         * Free resources on the NeoPixel strand
         * @param strand Strand index to free
         */
        public abstract void deinitializeStrand(byte strand);

    }
}
