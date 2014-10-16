using System.Collections.Generic;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using SD = System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WE = Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

using MetaWearWinStoreAPI;

namespace MetaWearTest
{
    public class AccelReadingArgs : EventArgs
    {
        public double x, y, z;

        public AccelReadingArgs( double x , double y , double z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    // this file is used to implement your MetaWearAPI callbacks   Once these are implemented here, just
    // new them up in your app and do an addModuleCallback(new blah())
    public class AccelerometerCallbacks : MetaWearBleService.MetaWearControllerImpl.AccelerometerImpl.Callbacks
    {
        /**
 * Called when the accelerometer has sent its XYZ motion data
 * @param x X component of the motion
 * @param y Y component of the motion
 * @param z Z component of the motion
 */
        public event EventHandler<AccelReadingArgs> AccelReadingChanged;

        public static UInt32 test_int = 10;
        public async override void receivedDataValue(short x, short y, short z)
        {
#if DEBUG
            SD.Debug.WriteLine(string.Format("Accel {0:f} {0:f} {0:f}"
                                      , (float) x / 1024.0f , (float) y / 1024.0f , (float) z / 1024.0f));

                if ( AccelReadingChanged != null )
                {
                    AccelReadingChanged(this, new AccelReadingArgs((double)x / 1024.0, y / (double)1024.0, (double)z / 1024.0));
                }

//            });
#endif
        }
        /** Called when free fall is detected */
        public async override void  inFreeFall()
        {
#if DEBUG
            SD.Debug.WriteLine("Falling");
#endif
            await HubPage.Mainpage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                HubPage.Mainpage.theFallingCheck.IsChecked = true;
            });
        }
        /** Called when free fall has stopped */
        public async override void stoppedFreeFall()
        {
#if DEBUG
            SD.Debug.WriteLine("Stopped falling");
#endif
            await HubPage.Mainpage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                HubPage.Mainpage.theFallingCheck.IsChecked = false;
            });
        }
        /** Called when the orientation has changed */
        public async override void receivedOrientation(byte orientation)
        {
#if DEBUG
            SD.Debug.WriteLine("Orientatation code: {0}", orientation);
#endif
            await HubPage.Mainpage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                HubPage.Mainpage.theOrientText.Text = "o:" + orientation.ToString();
            });
        }

    }

    public class SwitchCallbacks : MetaWearBleService.MetaWearControllerImpl.MechanicalSwitchImpl.Callbacks
    {
        public async override void pressed()
        {
#if DEBUG
            SD.Debug.WriteLine("Switch is pressed");
#endif
            await HubPage.Mainpage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                HubPage.Mainpage.theSwitchState.IsOn = true;
            });
        }

        public async override void released()
        {
#if DEBUG
            SD.Debug.WriteLine("Switch is released");
#endif
            await HubPage.Mainpage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                HubPage.Mainpage.theSwitchState.IsOn = false;
            });
        }
    }

    public class TempCallbacks : MetaWearBleService.MetaWearControllerImpl.TemperatureImpl.Callbacks
    {
        public async override void receivedTemperature( float degrees )
        {
#if DEBUG
            SD.Debug.WriteLine("Temp reading is {0} celsius", degrees);
#endif
            await HubPage.Mainpage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                HubPage.Mainpage.theTempCSlider.Value = degrees;
                HubPage.Mainpage.theTempFSlider.Value = degrees * (9 / 5) + 32;  // TODO: recheck
            });
        }
    }
}

