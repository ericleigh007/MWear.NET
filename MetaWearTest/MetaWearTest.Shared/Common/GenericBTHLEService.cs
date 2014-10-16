using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

namespace MetaWearTest
{
    public class HeartRateMeasurement
    {
        public ushort HeartRateValue { get; set; }
        public bool HasExpendedEnergy { get; set; }
        public ushort ExpendedEnergy { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public override string ToString()
        {
            return HeartRateValue.ToString() + " bpm @ " + Timestamp.ToString();
        }
    }

    public delegate void ValueChangeCompletedHandler(HeartRateMeasurement heartRateMeasurementValue);

    public delegate void DeviceConnectionUpdatedHandler(bool isConnected);

    public class GenericBTHLEService
    {
        // Heart Rate Constants
        private List<string> supported_notify_char_uuids;
        private List<string> supported_command_char_uuids;
        // The Characteristic we want to obtain measurements for is the Heart Rate Measurement characteristic
        private Guid CHARACTERISTIC_UUID = GattCharacteristicUuids.BatteryLevel;
        // Heart Rate devices typically have only one Heart Rate Measurement characteristic.
        // Make sure to check your device's documentation to find out how many characteristics your specific device has.
        private const int CHARACTERISTIC_INDEX = 0;
        // The Heart Rate Profile specification requires that the Heart Rate Measurement characteristic is notifiable.
        private const GattClientCharacteristicConfigurationDescriptorValue CHARACTERISTIC_NOTIFICATION_TYPE = 
            GattClientCharacteristicConfigurationDescriptorValue.Notify;

        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser().
//        MainPage rootPage = MainPage.Current;

        private static GenericBTHLEService instance = new GenericBTHLEService();
        private GattDeviceService service;
        private List<GattCharacteristic> notify_characteristics = new List<GattCharacteristic>();
        private List<GattCharacteristic> command_characteristics = new List<GattCharacteristic>();
        private List<HeartRateMeasurement> datapoints;
        private PnpObjectWatcher watcher;
        private String deviceContainerId;

        public string DeviceName;

        public event ValueChangeCompletedHandler ValueChangeCompleted;
        public event DeviceConnectionUpdatedHandler DeviceConnectionUpdated;

        public static GenericBTHLEService Instance
        {
            get { return instance; }
        }

        public bool IsServiceInitialized { get; set; }

        public GattDeviceService Service
        {
            get { return service; }
        }

        public HeartRateMeasurement[] DataPoints
        {
            get
            {
                HeartRateMeasurement[] retval;
                lock (datapoints)
                {
                    retval = datapoints.ToArray();
                }

                return retval;
            }
        }

        private GenericBTHLEService()
        {
            datapoints = new List<HeartRateMeasurement>();
            App.Current.Suspending += App_Suspending;
            App.Current.Resuming += App_Resuming;
        }

        private void App_Resuming(object sender, object e)
        {
            // Since the Windows Runtime will close resources to the device when the app is suspended,
            // the device needs to be reinitialized when the app is resumed.
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            IsServiceInitialized = false;

            // This is an appropriate place to save to persistent storage any datapoint the application cares about.
            // For the purpose of this sample we just discard any values.
            datapoints.Clear();

            // Allow the GattDeviceService to get cleaned up by the Windows Runtime.
            // The Windows runtime will clean up resources used by the GattDeviceService object when the application is
            // suspended. The GattDeviceService object will be invalid once the app resumes, which is why it must be 
            // marked as invalid, and reinitalized when the application resumes.
            if (service != null)
            {
                service.Dispose();
                service = null;
            }

            notify_characteristics.Clear();
/*            if (characteristic != null)
            {
                characteristic = null;
            }
 */

            if (watcher != null)
            {
                watcher.Stop();
                watcher = null;
            }
        }

        public async Task InitializeServiceAsync(DeviceInformation device , List<string> sup_notify_char,
                                                                            List<string> sup_command_char)
        {
            try
            {
                deviceContainerId = "{" + device.Properties["System.Devices.ContainerId"] + "}";

                DeviceName = device.Name;

                supported_notify_char_uuids = sup_notify_char;
                supported_command_char_uuids = sup_command_char;

                service = await GattDeviceService.FromIdAsync(device.Id);
                if (service != null)
                {
                    IsServiceInitialized = true;
                    await ConfigureServiceForNotificationsAsync();
                }
                else
                {
                    /*
                    rootPage.NotifyUser("Access to the device is denied, because the application was not granted access, " +
                        "or the device is currently in use by another application.",
                        NotifyType.StatusMessage);
                     */
                }
            }
            catch (Exception e)
            {
                /*
                rootPage.NotifyUser("ERROR: Accessing your device failed." + Environment.NewLine + e.Message,
                    NotifyType.ErrorMessage);\
                 */
            }
        }

        /// <summary>
        /// Configure the Bluetooth device to send notifications whenever the Characteristic value changes
        /// </summary>
        private async Task ConfigureServiceForNotificationsAsync()
        {
            try
            {
                foreach( string chs in supported_notify_char_uuids )
                {
                    // Obtain the characteristic for which notifications are to be received
                    var chr = service.GetCharacteristics(new Guid(chs))[CHARACTERISTIC_INDEX];

                    notify_characteristics.Add(chr);

                    // While encryption is not required by all devices, if encryption is supported by the device,
                    // it can be enabled by setting the ProtectionLevel property of the Characteristic object.
                    // All subsequent operations on the characteristic will work over an encrypted link.
//                    chr.ProtectionLevel = GattProtectionLevel.EncryptionRequired;

                    // Register the event handler for receiving notifications
                    chr.ValueChanged += Notifiable_Char_ValueChanged;

                    // In order to avoid unnecessary communication with the device, determine if the device is already 
                    // correctly configured to send notifications.
                    GattCommunicationStatus status = await EnableNotificationAsync(chr);
                    if (status == GattCommunicationStatus.Unreachable)
                    {
                        // Register a PnpObjectWatcher to detect when a connection to the device is established,
                        // such that the application can retry device configuration.
                        StartDeviceConnectionWatcher();
                    }
                }
            }
            catch (Exception e)
            {
                /*
                rootPage.NotifyUser("ERROR: Accessing your device failed." + Environment.NewLine + e.Message,
                    NotifyType.ErrorMessage);
                 */
            }
        }

        /// <summary>
        /// Register to be notified when a connection is established to the Bluetooth device
        /// </summary>
        private void StartDeviceConnectionWatcher()
        {
            watcher = PnpObject.CreateWatcher(PnpObjectType.DeviceContainer,
                new string[] { "System.Devices.Connected" }, String.Empty);

            watcher.Updated += DeviceConnection_Updated;
            watcher.Start();
        }

        /// <summary>
        /// Invoked when a connection is established to the Bluetooth device
        /// </summary>
        /// <param name="sender">The watcher object that sent the notification</param>
        /// <param name="args">The updated device object properties</param>
        private async void DeviceConnection_Updated(PnpObjectWatcher sender, PnpObjectUpdate args)
        {
            var connectedProperty = args.Properties["System.Devices.Connected"];
            bool isConnected = false;
            if ((deviceContainerId == args.Id) && Boolean.TryParse(connectedProperty.ToString(), out isConnected) &&
                isConnected)
            {
                foreach (GattCharacteristic ch in notify_characteristics)
                {
                    GattCommunicationStatus notify_status = await EnableNotificationAsync(ch);
                    if (notify_status == GattCommunicationStatus.Success )
                    {
                        IsServiceInitialized = true;

                        // Once the Client Characteristic Configuration Descriptor is set, the watcher is no longer required
                        watcher.Stop();
                        watcher = null;
                    }
                }

                // Notifying subscribers of connection state updates
                if (DeviceConnectionUpdated != null)
                {
                    DeviceConnectionUpdated(isConnected);
                }
            }
        }

        private async Task<GattCommunicationStatus> EnableNotificationAsync(GattCharacteristic chr)
        {
            // By default ReadClientCharacteristicConfigurationDescriptorAsync will attempt to get the current
            // value from the system cache and communication with the device is not typically required.
            var currentDescriptorValue = await chr.ReadClientCharacteristicConfigurationDescriptorAsync();

            GattCommunicationStatus wcharstatus = GattCommunicationStatus.Success;

            if ((currentDescriptorValue.Status != GattCommunicationStatus.Success) ||
                (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != CHARACTERISTIC_NOTIFICATION_TYPE))
            {
                wcharstatus = await chr.WriteClientCharacteristicConfigurationDescriptorAsync(
                                                                CHARACTERISTIC_NOTIFICATION_TYPE);
            }

            if ( wcharstatus != GattCommunicationStatus.Success )
            {
                return wcharstatus;
            }

            // Now hack in a write so that we can try to enable the board.

            foreach( string stch in supported_command_char_uuids)
            {
                var wchr = service.GetCharacteristics(new Guid(stch))[CHARACTERISTIC_INDEX];
                DataWriter writer = new DataWriter();

                // set the configuration settings
                byte[] config_data = new byte[] {0, 0, 0x20, 0, 0};
                writer.WriteByte(3); // the module - accerlerometer
                writer.WriteByte(2); // data set - DATA_SETTINGS
                writer.WriteBytes(config_data);
                wcharstatus = await wchr.WriteValueAsync(writer.DetachBuffer());

                // really hackky.. just write the byes we want to the stream.
                writer.WriteByte(3); // the module - accerlerometer
                writer.WriteByte(2); // data enable - DATA_ENABLE
                writer.WriteByte(1); // enabled
                wcharstatus = await wchr.WriteValueAsync(writer.DetachBuffer());

                writer.WriteByte(3); // the module - accerlerometer
                writer.WriteByte(1); // data enable - GLOBAL_ENABLE
                writer.WriteByte(1); // enabled

                wcharstatus = await wchr.WriteValueAsync(writer.DetachBuffer());
            }
            
            return wcharstatus;
        }

        /// <summary>
        /// Invoked when Windows receives data from your Bluetooth device.
        /// </summary>
        /// <param name="sender">The GattCharacteristic object whose value is received.</param>
        /// <param name="args">The new characteristic value sent by the device.</param>
        private void Notifiable_Char_ValueChanged(
            GattCharacteristic sender,
            GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            DateTimeOffset dt_timestamp = args.Timestamp;

            int data_count = data.Length;

            /*
            // Process the raw data received from the device.
            var value = ProcessData(data);
            value.Timestamp = args.Timestamp;
             * 
            lock (datapoints)
            {
                datapoints.Add(value);
            }

            if (ValueChangeCompleted != null)
            {
                ValueChangeCompleted(value);
            }
             */
        }

        /// <summary>
        /// Process the raw data received from the device into application usable data, 
        /// according the the Bluetooth Heart Rate Profile.
        /// </summary>
        /// <param name="data">Raw data received from the heart rate monitor.</param>
        /// <returns>The heart rate measurement value.</returns>
        private HeartRateMeasurement ProcessData(byte[] data)
        {
            // Heart Rate profile defined flag values
            const byte HEART_RATE_VALUE_FORMAT = 0x01;
            const byte ENERGY_EXPANDED_STATUS = 0x08;

            byte currentOffset = 0;
            byte flags = data[currentOffset];
            bool isHeartRateValueSizeLong = ((flags & HEART_RATE_VALUE_FORMAT) != 0);
            bool hasEnergyExpended = ((flags & ENERGY_EXPANDED_STATUS) != 0);

            currentOffset++;

            ushort heartRateMeasurementValue = 0;

            if (isHeartRateValueSizeLong)
            {
                heartRateMeasurementValue = (ushort)((data[currentOffset + 1] << 8) + data[currentOffset]);
                currentOffset += 2;
            }
            else
            {
                heartRateMeasurementValue = data[currentOffset];
                currentOffset++;
            }

            ushort expendedEnergyValue = 0;

            if (hasEnergyExpended)
            {
                expendedEnergyValue = (ushort)((data[currentOffset + 1] << 8) + data[currentOffset]);
                currentOffset += 2;
            }

            // The Heart Rate Bluetooth profile can also contain sensor contact status information,
            // and R-Wave interval measurements, which can also be processed here. 
            // For the purpose of this sample, we don't need to interpret that data.

            return new HeartRateMeasurement
            {
                HeartRateValue = heartRateMeasurementValue,
                HasExpendedEnergy = hasEnergyExpended,
                ExpendedEnergy = expendedEnergyValue
            };
        }

        /// <summary>
        /// Process the raw data read from the device into an application usable string, according to the Bluetooth
        /// Specification.
        /// </summary>
        /// <param name="bodySensorLocationData">Raw data read from the heart rate monitor.</param>
        /// <returns>The textual representation of the Body Sensor Location.</returns>
        public string ProcessBodySensorLocationData(byte[] bodySensorLocationData)
        {
            // The Bluetooth Heart Rate Profile specifies that the Body Sensor Location characteristic value has
            // a single byte of data
            byte bodySensorLocationValue = bodySensorLocationData[0];
            string retval;

            retval = "";
            switch (bodySensorLocationValue)
            {
                case 0:
                    retval += "Other";
                    break;
                case 1:
                    retval += "Chest";
                    break;
                case 2:
                    retval += "Wrist";
                    break;
                case 3:
                    retval += "Finger";
                    break;
                case 4:
                    retval += "Hand";
                    break;
                case 5:
                    retval += "Ear Lobe";
                    break;
                case 6:
                    retval += "Foot";
                    break;
                default:
                    retval = "";
                    break;
            }
            return retval;
        }
    }
}
