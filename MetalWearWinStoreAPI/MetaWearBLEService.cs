/**
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
 * PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
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
 * C# / .NET Port Copyright (c) 2014, Eric Snyder / elssystems
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using WE = Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

using SD = System.Diagnostics; // debug only

namespace MetaWearWinStoreAPI
{

     /* Represents the state of MetaWear service
     * @author Eric Tsai
     * @port Eric Snyder
      */
public class MetaWearBleService
{
    /**
     * Generic template for a MetaWear register.  Registers provide a means of communication with 
     * the MetaWear modules, mainly used to retrieve data from or issue commands to a module.    
     * @author Eric Tsai
     */
    private enum DeviceState {
        UNKNOWN,
        /** Service is in the process of enabling notifications */
        ENABLING_NOTIFICATIONS,
        /** Service is reading characteristics */
        READING_CHARACTERISTICS,
        /** Service is writing characteristics */
        WRITING_CHARACTERISTICS,
        /** Service is ready to process a command */
        READY
    }

    private static Guid CHARACTERISTIC_CONFIG= new Guid("00002902-0000-1000-8000-00805f9b34fb");
    private static Dictionary<Byte, List<MetaWearController.ModuleCallbacks>> moduleCallbackMap = 
                                          new Dictionary<Byte, List<MetaWearController.ModuleCallbacks>>();
    private static List<MetaWearController.DeviceCallbacks> deviceCallbacks= new List<MetaWearController.DeviceCallbacks>();

    private static List<WE.DeviceInformation> deviceList = new List<WE.DeviceInformation>();
    
    private static Boolean connected;

    private static PnpObjectWatcher watcher;

    /** GATT connection to the ble device */
/*    private BluetoothGatt metaWearGatt; */
    /** Current state of the device */
    private static DeviceState deviceState = DeviceState.UNKNOWN;
    private static int deviceListIndex = 0 ;

    private string selectedDeviceContainerID = "";

    private static GattDeviceService deviceService = null;

/*    private BluetoothDevice metaWearBoard;  */
    // on Windows, the bytes being written are written with an atomic operation, so we won't need these */
    /** Bytes still be to written to the MetaWear command characteristic */
    /*
    private final ConcurrentLinkedQueue<byte[]> commandBytes= new ConcurrentLinkedQueue<>();
    Characteristic UUIDs still to be read from MetaWear
    private final ConcurrentLinkedQueue<GATTCharacteristic> readCharUuids= new ConcurrentLinkedQueue<>();    
    */

    private static Queue<byte[]> commandBytes = new Queue<byte[]>();
    private static Queue<GATTCharacteristic> readCharUuids = new Queue<GATTCharacteristic>();

    /**
     * Get the IntentFilter for actions broadcasted by the MetaWear service
     * @return IntentFilter for MetaWear specific actions
     * @see ModuleCallbacks
     * @see DeviceCallbacks
     * Don't need this
     * 
    public static IntentFilter getMetaWearIntentFilter() {
        IntentFilter filter= new IntentFilter();
        
        filter.addAction(Action.NOTIFICATION_RECEIVED);
        filter.addAction(Action.CHARACTERISTIC_READ);
        filter.addAction(Action.DEVICE_CONNECTED);
        filter.addAction(Action.DEVICE_DISCONNECTED);
        return filter;
    }
    */

    // Build a AQS string for the devices we want to connect to, get a list of them.
    // Create a watcher to watch their state
    // Hook into the suspensing App event
    public MetaWearBleService()
    {
        string tmp_AQS = ""; // BuildDevicesAQSString();

        // initialize
        Module.InitializeModuleRegisterCrossReference();
    }

    // Eventually, this method will wildcard though the services offered by the device and build a complex query
    // string so that all the services will be returnd by the FindAllAsync call.
    private string BuildDevicesAQSString()
    {
        // HACK -- for now we don't really use the AQS string
 	    return "326A9000-85CB-9195-D9DD-464CFBBAE75A";
    }

    public async Task<DeviceInformationCollection> GetDevicesAsync( string query_string)
    {
        string local_selector = @"System.Devices.InterfaceClassGuid:=""{6E3BB679-4372-40C8-9EAA-4509DF260CD8}"" AND (System.DeviceInterface.Bluetooth.ServiceGuid:=""{0000180F-0000-1000-8000-00805F9B34FB}"" OR System.DeviceInterface.Bluetooth.ServiceGuid:=""{0000180A-0000-1000-8000-00805F9B34FB}"") AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
        WE.DeviceInformationCollection devices;

        if (query_string.Length > 0)
        {
            devices = await WE.DeviceInformation.FindAllAsync( query_string,
                    new string[] { "System.Devices.ContainerId" });

            foreach (WE.DeviceInformation di in devices)
            {
                deviceList.Add(di);
            }

            return devices;
        }
        else
        {
         
#if true 
            // HACK -- for now we don't use the AQS string
            devices = await WE.DeviceInformation.FindAllAsync( 
                    GattDeviceService.GetDeviceSelectorFromUuid( new Guid( GATTService.METAWEAR.uuid.ToString())),
                    new string[] { "System.Devices.ContainerId" });
#else
            var devices = await WE.DeviceInformation.FindAllAsync(
                    local_selector,
                    new string[] { "System.Devices.ContainerId" });
#endif
            foreach( WE.DeviceInformation di in devices)
            {
                deviceList.Add(di);
            }
            
        }

        return devices;
    }


    public int GetDeviceCount()
    {
        return deviceList.Count;
    }

    // this allows us to list the devies on the UX
    public static List<WE.DeviceInformation> GetDeviceList()
    {
        return deviceList;
    }

    public async Task InitializeDeviceAsync( int index )
    {
        deviceListIndex = index;

        var device = deviceList[deviceListIndex];

        selectedDeviceContainerID = "{" + device.Properties["System.Devices.ContainerId"] + "}";

        deviceService = await GattDeviceService.FromIdAsync(device.Id);

        // enumerate (later), but for now just the MetaWear service
//        foreach( GATTService gs in GATTService.GetValues() )
//        {
            // enumerate each characteristic
        //
        // Unlike a fully-compliant BLE device, the MetaWear has only one characteristic for ALL ouptut (as of this time
        // anyway).  A more compliant device would actually have multiple characteristics, one for EACH output.
        foreach (GATTCharacteristic gc in MetaWear.GetValues())
        {
            if (gc.enabled)
            {
                // Obtain the characteristic for which notifications are to be received
                var chr = deviceService.GetCharacteristics(gc.uuid)[0];

                // var chr = deviceService.GetCharacteristics(MetaWear.NOTIFICATION_1.uuid)[0];

                // If encryption is known to be supported (tests show it currently isn't), then enable ecnyprtion
                // on the charactistic 
                if ( gc.encryptable )
                {
                    chr.ProtectionLevel = GattProtectionLevel.EncryptionRequired;
                }

                // We tend to get exceptions trying to turn on things that aren't supported, so we've added priperties
                // to the characterisics to define them better
                if (gc.notifyable)
                {
                    // Register the event handler for receiving notifications.  
                    // Notifications can result in a "streaming" behavior for tihngs like the accelerator, or 
                    // a one-to-one behavior, which returns once for each query, such as Temperature
                    chr.ValueChanged += Notifiable_Char_ValueChanged;

                    // In order to avoid unnecessary communication with the device, determine if the device is already 
                    // correctly configured to send notifications.
                    GattCommunicationStatus status = await EnableNotificationAsync(chr);
                    if (status == GattCommunicationStatus.Unreachable)
                    {
                        // Register a PnpObjectWatcher to detect when a connection to the device is established,
                        // such that the application can retry device configuration.
                        // for now, just indicate the device is unreachable.  Add resiliency later
                        throw new NotImplementedException("device is not reachable -- resiliency not enabled yet");
                        // StartDeviceConnectionWatcher();
                    }
                }
            }
        }

        deviceState = DeviceState.READY;

//        }
    }

    private async Task<GattCommunicationStatus> EnableNotificationAsync(GattCharacteristic chr)
    {
        // By default ReadClientCharacteristicConfigurationDescriptorAsync will attempt to get the current
        // value from the system cache and communication with the device is not typically required.
        var currentDescriptorValue = await chr.ReadClientCharacteristicConfigurationDescriptorAsync();

        GattCommunicationStatus wcharstatus = GattCommunicationStatus.Success;

        if ((currentDescriptorValue.Status != GattCommunicationStatus.Success) ||
            (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify))
        {
            wcharstatus = await chr.WriteClientCharacteristicConfigurationDescriptorAsync(
                                                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }

        if ( wcharstatus != GattCommunicationStatus.Success )
        {
            return wcharstatus;
        }
            
        return wcharstatus;
    }

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
    private void DeviceConnection_Updated(PnpObjectWatcher sender, PnpObjectUpdate args)
    {
        var connectedProperty = args.Properties["System.Devices.Connected"];
        bool isConnected = false;
        if ((selectedDeviceContainerID == args.Id) && Boolean.TryParse(connectedProperty.ToString(), out isConnected) &&
            isConnected)
        {
            foreach( MetaWearController.DeviceCallbacks dc in deviceCallbacks)
            {
                dc.connected();
            }
        }
        else if ( selectedDeviceContainerID == args.Id )
        {
            foreach (MetaWearController.DeviceCallbacks dc in deviceCallbacks)
            {
                dc.disconnected();
            }
        }
    }

    /*
    private async void OnDeviceConnectionUpdated(bool isConnected)
    {
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            if (isConnected)
            {
                statusTextBlock.Text = "Waiting for device to send data...";
            }
            else
            {
                statusTextBlock.Text = "Waiting for device to connect...";
            }
        });
    }
     */

        private void Notifiable_Char_ValueChanged(
            GattCharacteristic sender,
            GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            DateTimeOffset dt_timestamp = args.Timestamp;

            int data_count = data.Length;

            LookupAndNotify(data , dt_timestamp);

        }

       private static void LookupAndNotify(byte[] buffer, DateTimeOffset timestamp)
       {
           // toss notifications until notifies are enabled (basically we need to build the opodeMap first)
           if (!Module.notifyEnabled)
#if DEBUG
           {
               SD.Debug.WriteLine("notifications not enabled -- need to call Module.InitializeModuleRegisterCrossReference first");
               return;
           }
#else
           {
               return;
           }
#endif

           if ( buffer.Length < 3 )
           {
               throw new Exception(string.Format("Data error in LookupAndNotify -- data length {0} too short", buffer.Length));
           }

           byte modOp = buffer[0];
           byte regOp = (byte)(buffer[1] & 0x7f);

           if (Module.lookupModule(modOp) == null )
           {
               throw new Exception(string.Format("Data error in LookupAndNotify -- Module # {0} is null", modOp));
           }

           if (Module.lookupModule(modOp).lookupRegister(regOp) == null)
           {
               throw new Exception(string.Format("Data error in LookupAndNotify -- Module # {0} Register # {1} is null", modOp , regOp));
           }

           if ( moduleCallbackMap.ContainsKey( modOp ))
           {
#if DEBUG
               SD.Debug.WriteLine(string.Format("t:{0:ss.fff}, m:{1} r:{2} d:{3} dl:{4}",
                                                 timestamp , modOp , regOp , buffer[2], buffer.Length));
#endif
               Module.lookupModule(modOp).lookupRegister(regOp).notifyCallbacks(moduleCallbackMap[modOp], buffer);
           }
#if DEBUG
           else
           {
               SD.Debug.WriteLine(String.Format("no callbacks registered for the {0} module", Module.lookupModule(modOp).name));
           }
#endif
       }
    /*
    private static BroadcastReceiver mwBroadcastReceiver;
    
    /**
     * Get the broadcast receiver for MetaWear intents.  An Activity using the MetaWear service 
     * will need to register this receiver to trigger its callback functions.
     * @return MetaWear specific broadcast receiver
     */

    /* this is the combination of a device state listener and a characteristic read listener delegate */
    /*
    public static BroadcastReceiver getMetaWearBroadcastReceiver() {
        if (mwBroadcastReceiver == null) {
            mwBroadcastReceiver= new BroadcastReceiver() {
                @Override
                public void onReceive(Context context, Intent intent) {
                    switch (intent.getAction()) {
                    case Action.NOTIFICATION_RECEIVED:
                        byte[] data= (byte[])intent.getExtras().get(Extra.CHARACTERISTIC_VALUE);
                        byte moduleOpcode= data[0], registerOpcode= (byte)(0x7f & data[1]);
                        Module.lookupModule(moduleOpcode).lookupRegister(registerOpcode)
                                .notifyCallbacks(moduleCallbackMap.get(moduleOpcode), data);
                        break;
                    case Action.DEVICE_CONNECTED:
                        for(DeviceCallbacks it: deviceCallbacks) {
                            it.connected();
                        }
                        break;
                    case Action.DEVICE_DISCONNECTED:
                        for(DeviceCallbacks it: deviceCallbacks) {
                            it.disconnected();
                        }
                        break;
                    case Action.CHARACTERISTIC_READ:
                        UUID serviceUuid= (UUID)intent.getExtras().get(Extra.SERVICE_UUID), 
                                charUuid= (UUID)intent.getExtras().get(Extra.CHARACTERISTIC_UUID);
                        GATTCharacteristic characteristic= GATTService.lookupGATTService(serviceUuid).getCharacteristic(charUuid);
                        for(DeviceCallbacks it: deviceCallbacks) {
                            it.receivedGATTCharacteristic(characteristic, (byte[])intent.getExtras().get(Extra.CHARACTERISTIC_VALUE));
                        }
                        break;
                    }
                }
            };
        }
        return mwBroadcastReceiver;
    }
     */
    
    /** Interacts with the MetaWear board */
    /* these are the implementations of the abstract classes inside the API */

    public class MetaWearControllerImpl : MetaWearController
    {
        private static List<Accelerometer.Component> enabledComponents= new List<Accelerometer.Component>();
        private static Dictionary<Module, ModuleController> modules= new Dictionary<Module, ModuleController>();

        private static MetaWearController controller;

        public MetaWearControllerImpl()
        {
            controller = this;
        }

        /**
            * Get a controller to the connected MetaWear device 
            * @return MetaWear controller
            */

        public MetaWearController getMetaWearController() 
        {
            return controller;
        }

        public override MetaWearController addModuleCallback(ModuleCallbacks callback)
        {
            byte moduleOpcode = callback.getModule().modID;
            if (!moduleCallbackMap.ContainsKey(moduleOpcode)) 
            {
                moduleCallbackMap[moduleOpcode] = new List<ModuleCallbacks>();
            }

            moduleCallbackMap[moduleOpcode].Add(callback);

            return this;
        }

        public override void removeModuleCallback(ModuleCallbacks callback) 
        {
            moduleCallbackMap[callback.getModule().modID].Remove(callback);
        }

        public override MetaWearController addDeviceCallback(DeviceCallbacks callback) 
        {
            deviceCallbacks.Add(callback);
            return this;
        }

        public override void removeDeviceCallback(DeviceCallbacks callback) 
        {
            deviceCallbacks.Remove(callback);
        }

        public override ModuleController getModuleController(Module mod)
        {
            throw new NotImplementedException("use getXXXXModule instead");
        }

        // Lookup a module by its name and return the object
        // Use names instead of the objects in the case statement, since C# doens't support object cases
        public ModuleController getModuleController(string moduleName )
        {
            switch(moduleName)
            {
                case "Mechanical Switch":
                    return getMechanicalSwitchModule();
                case "LED":
                    return getLEDDriverModule();
                case "Accelerometer":
                    return getAccelerometerModule();
                case "Temperature":
                    return getTemperatureModule();
                case "GPIO":
                    return getGPIOModule();
                case "NeoPixel":
                    return getNeoPixelDriver();
                case "iBeacon":
                    return getIBeaconModule();
                case "Haptic":
                    return getHapticModule();
                case "Debug":
                    return getDebugModule();

                // Add more modules here, and if we didn't find a match for hte module name, throw an exception
                // this I view as more correct than returning NULL, since a bad module name would indicate a 
                // static error in the app
                default:
                   throw new NotImplementedException( string.Format("App error -- module controller for module {0} does not exist"));
            }
        }

        public override async void readDeviceInformationAsync()
        {
            foreach (GATTCharacteristic gc in DeviceInformation.GetValues())
            {
                readCharUuids.Enqueue(gc);
            }

            if (deviceState == DeviceState.READY)
            {
                deviceState = DeviceState.READING_CHARACTERISTICS;
                await readCharacteristicAsync();
            }
        }

        public override Boolean isConnected()
        {
            return connected;
        }

        public class AccelerometerImpl : Accelerometer
        {
            public AccelerometerImpl() { }

            public override void enableNotification(Component component) 
            {
                enabledComponents.Add(component);
                writeRegister(component.enable, (byte)1);
                writeRegister(Register.GLOBAL_ENABLE, (byte)1);
            }

            public override void disableNotification(Component component) 
            {
                writeRegister(component.enable, (byte)0);
                enabledComponents.Remove(component);
                if (enabledComponents.Count == 0 )
                {
                    writeRegister(Register.GLOBAL_ENABLE, (byte)0);
                }
            }

            public override void readComponentConfiguration(Component component) 
            {
                readRegister(component.config, (byte)0);
            }

            public override void setComponentConfiguration(Component component,
                    byte[] data) 
            {
                writeRegister(component.config, data);
            }
        }

        public ModuleController getAccelerometerModule() 
        {
            if (!modules.ContainsKey(Module.MWMOD_ACCELEROMETER)) 
            {
                modules[Module.MWMOD_ACCELEROMETER] = new AccelerometerImpl();
            }

            return modules[Module.MWMOD_ACCELEROMETER];
        }

        public class DebugImpl : Debug
        {
            public override void resetDevice() 
            {
                writeRegister(Register.RESET_DEVICE);
            }
            public override void jumpToBootloader() 
            {
                writeRegister(Register.JUMP_TO_BOOTLOADER);
            }
        }

        public Debug getDebugModule() 
        {
            if (!modules.ContainsKey(Module.MWMOD_DEBUG)) 
            {
                modules[Module.MWMOD_DEBUG] = new DebugImpl();
            }

            return (Debug) modules[Module.MWMOD_DEBUG];
        }

        public class GPIOImpl : GPIO
        {
            public override void readAnalogInput(byte pin, AnalogMode mode) 
            {
                readRegister(mode.register, pin);
            }

            public override void readDigitalInput(byte pin) 
            {
                readRegister(Register.READ_DIGITAL_INPUT, pin);
            }

            public override void setDigitalOutput(byte pin) 
            {
                writeRegister(Register.SET_DIGITAL_OUTPUT, pin);
            }

            public override void clearDigitalOutput(byte pin) 
            {
                writeRegister(Register.CLEAR_DIGITAL_OUTPUT, pin);
            }

            public override void setDigitalInput(byte pin, PullMode mode) 
            {
                writeRegister(mode.register, pin);
            }
        }

        public GPIO getGPIOModule() 
        {
            if (!modules.ContainsKey(Module.MWMOD_GPIO)) 
            {
                modules[Module.MWMOD_GPIO] = new GPIOImpl();
            }

            return (GPIO)modules[Module.MWMOD_GPIO];
        }

        public class IBeaconImpl : IBeacon
        {
            public override void enableIBeacon() 
            {
                writeRegister(Register.ENABLE, (byte)1);                    
            }

            public override void disableIBecon() 
            {
                writeRegister(Register.ENABLE, (byte)0);
            }

            public override IBeacon setUUID(Guid uuid) 
            {
                // TODO - may be endianness problems here
                byte[] uuidBytes = uuid.ToByteArray();
                writeRegister(Register.ADVERTISEMENT_UUID, uuidBytes);
                return this;
            }

            public override void readSetting(Register register) 
            {
                readRegister(register);
            }

            public override IBeacon setMajor(short major) 
            {
                if (BitConverter.IsLittleEndian) major = major.SwapBytes();

                writeRegister(Register.MAJOR, (byte)(major >> 8 & 0xff), (byte)(major & 0xff));
                return this;
            }

            public override IBeacon setMinor(short minor) 
            {
                if (BitConverter.IsLittleEndian) minor = minor.SwapBytes();

                writeRegister(Register.MINOR, (byte)(minor >> 8 & 0xff), (byte)(minor & 0xff));
                return this;
            }

            public override IBeacon setCalibratedRXPower(byte power) 
            {
                writeRegister(Register.RX_POWER, power);
                return this;
            }

            public override IBeacon setTXPower(byte power) 
            {
                writeRegister(Register.TX_POWER, power);
                return this;
            }

            public override IBeacon setAdvertisingPeriod(short freq) 
            {
                if (BitConverter.IsLittleEndian) freq = freq.SwapBytes();

                writeRegister(Register.ADVERTISEMENT_PERIOD, (byte)(freq >> 8 & 0xff), (byte)(freq & 0xff));
                return this;
            }
        }

        public IBeacon getIBeaconModule() 
        {
            if (!modules.ContainsKey(Module.MWMOD_IBEACON)) 
            {
                modules[Module.MWMOD_IBEACON] = new IBeaconImpl();
            }

            return (IBeacon) modules[Module.MWMOD_IBEACON];
        }

        public class ChannelDataWriterImpl : MetaWearWinStoreAPI.LED.ChannelDataWriter
        {
            private byte[] channelData= new byte[15];
            public MetaWearWinStoreAPI.LED.ColorChannel color;
                
            public override MetaWearWinStoreAPI.LED.ColorChannel getChannel() 
            {
                return color;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withHighIntensity(byte intensity) 
            {
                channelData[2]= intensity;
                return this;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withLowIntensity(byte intensity) 
            {
                channelData[3]= intensity;
                return this;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withRiseTime(short time) 
            {
                if (BitConverter.IsLittleEndian) time = time.SwapBytes();

                channelData[5]= (byte)(time >> 8);
                channelData[4]= (byte)(time & 0xff);
                return this;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withHighTime(short time) 
            {
                if (BitConverter.IsLittleEndian) time = time.SwapBytes();

                channelData[7] = (byte)(time >> 8);
                channelData[6]= (byte)(time & 0xff);
                return this;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withFallTime(short time) 
            {
                if (BitConverter.IsLittleEndian) time = time.SwapBytes();

                channelData[9] = (byte)(time >> 8);
                channelData[8]= (byte)(time & 0xff);
                return this;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withPulseDuration(short period) 
            {
                if (BitConverter.IsLittleEndian) period = period.SwapBytes();

                channelData[11] = (byte)(period >> 8);
                channelData[10]= (byte)(period & 0xff);
                return this;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withPulseOffset(short offset) 
            {
                if (BitConverter.IsLittleEndian) offset = offset.SwapBytes();

                channelData[13] = (byte)(offset >> 8);
                channelData[12]= (byte)(offset & 0xff);
                return this;
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter withRepeatCount(byte count) 
            {
                channelData[14]= count;
                return this;
            }

            public override void commit() 
            {
                channelData[0] = (byte) (color.Ordinal());
                channelData[1]= 0x2;    ///< Keep it set to flash for now
                writeRegister(MetaWearWinStoreAPI.LED.Register.MODE, channelData);
            }                
        }

        public class LEDImpl : MetaWearWinStoreAPI.LED
        {
            public override void play(Boolean autoplay) 
            {
                writeRegister(Register.PLAY, (byte)(autoplay ? 2 : 1));
            }

            public override void pause() 
            {
                writeRegister(Register.PLAY, (byte)0);
            }
            
            public override void stop(Boolean resetChannels) 
            {
                writeRegister(Register.STOP, (byte)(resetChannels ? 1 : 0));
            }

            public override MetaWearWinStoreAPI.LED.ChannelDataWriter setColorChannel(MetaWearWinStoreAPI.LED.ColorChannel color)
            {
                var cc = new ChannelDataWriterImpl();
                cc.color = color;
                return cc as MetaWearWinStoreAPI.LED.ChannelDataWriter;
            }
        }

        public MetaWearWinStoreAPI.LED getLEDDriverModule() 
        {
            if (!modules.ContainsKey(Module.MWMOD_LED)) 
            {
                modules[Module.MWMOD_LED] = new LEDImpl(); 
            }

            return (LED)modules[Module.MWMOD_LED];
        }

        public class MechanicalSwitchImpl : MechanicalSwitch
        {
            public override void enableNotification() 
            {
                writeRegister(Register.SWITCH_STATE, (byte)1);
            }

            public override void disableNotification() 
            {
                writeRegister(Register.SWITCH_STATE, (byte)0);
            }
        }

        public MechanicalSwitch getMechanicalSwitchModule()
        {
            if (!modules.ContainsKey(Module.MWMOD_MECHANICAL_SWITCH)) 
            {
                modules[Module.MWMOD_MECHANICAL_SWITCH] = new MechanicalSwitchImpl();
            }

            return (MechanicalSwitch)modules[Module.MWMOD_MECHANICAL_SWITCH];
        }

        public class NeoPixelImpl : NeoPixel
        {
            public override void readStrandState(byte strand) 
            {
                readRegister(Register.INITIALIZE, strand);
            }

            public override void readHoldState(byte strand) 
            {
                readRegister(Register.HOLD, strand);
            }
            
            public override void readPixelState(byte strand, byte pixel) 
            {
                readRegister(Register.PIXEL, strand, pixel);
            }
            
            public override void readRotationState(byte strand) 
            {
                readRegister(Register.ROTATE, strand);
            }
            
            public override void initializeStrand(byte strand, ColorOrdering ordering,
                    StrandSpeed speed, byte ioPin, byte length) 
            {
                writeRegister(Register.INITIALIZE, strand, 
                        (byte)(ordering.Ordinal() << 4 | speed.Ordinal()), ioPin, length);
                        
            }
            
            public override void holdStrand(byte strand, byte holdState) 
            {
                writeRegister(Register.HOLD, strand, holdState);
                        
            }
            
            public override void clearStrand(byte strand, byte start, byte end) 
            {
                writeRegister(Register.CLEAR, strand, start, end);
                        
            }
            
            public override void setPixel(byte strand, byte pixel, byte red,
                    byte green, byte blue) 
            {
                writeRegister(Register.PIXEL, strand, pixel, red, green, blue);
                        
            }
            
            public override void rotateStrand(byte strand, RotationDirection direction, byte repetitions,
                    short delay) 
            {
                if (BitConverter.IsLittleEndian) delay = delay.SwapBytes();

                writeRegister(Register.ROTATE, strand, (byte)(direction.Ordinal()), repetitions, 
                        (byte)(delay >> 8 & 0xff), (byte)(delay & 0xff));
            }
            
            public override void deinitializeStrand(byte strand) 
            {
                writeRegister(Register.DEINITIALIZE, strand);    
            }
        }

        public NeoPixel getNeoPixelDriver() 
        {
            if (!modules.ContainsKey(Module.MWMOD_NEO_PIXEL)) 
            {
                modules[Module.MWMOD_NEO_PIXEL] = new NeoPixelImpl();
            }

            return (NeoPixel)modules[Module.MWMOD_NEO_PIXEL];
        }

        public class TemperatureImpl : Temperature
        {
            public override void readTemperature() 
            {
                readRegister(Register.TEMPERATURE, (byte)0);
            }
        }

        public Temperature getTemperatureModule() 
        {
            if (!modules.ContainsKey(Module.MWMOD_TEMPERATURE)) 
            {
                modules[Module.MWMOD_TEMPERATURE] = new TemperatureImpl();
            }

            return (Temperature)modules[Module.MWMOD_TEMPERATURE];
        }

        public class HapticImpl : Haptic
        {
            public override void startMotor(short pulseWidth) 
            {
                if (BitConverter.IsLittleEndian) pulseWidth = pulseWidth.SwapBytes();

                writeRegister(Register.PULSE, (byte)128, (byte)(pulseWidth & 0xff), (byte)(pulseWidth >> 8), (byte)1);
            }

            public override void startBuzzer(short pulseWidth) 
            {
                if (BitConverter.IsLittleEndian) pulseWidth = pulseWidth.SwapBytes();

                writeRegister(Register.PULSE, (byte)248, (byte)(pulseWidth & 0xff), (byte)(pulseWidth >> 8), (byte)0);
            }
        }

        public Haptic getHapticModule() 
        {
            if (!modules.ContainsKey(Module.MWMOD_HAPTIC)) 
            {
                modules[Module.MWMOD_HAPTIC] = new HapticImpl();
            }

            return (Haptic)modules[Module.MWMOD_HAPTIC];
        }

    }
    
    /**
     * Writes a command to MetaWear via the command register UUID
     * @see Characteristics.MetaWear#COMMAND
     */
    private static async Task<GattCommunicationStatus> writeCommandAsync() 
    {
        GattDeviceService service = deviceService;
        GattCharacteristic command = service.GetCharacteristics(MetaWear.COMMAND.uuid)[0];
        byte[] buf = commandBytes.Dequeue();
        DataWriter writer = new DataWriter();
        writer.WriteBytes(buf);
        GattCommunicationStatus st = await command.WriteValueAsync(writer.DetachBuffer());

        deviceState = DeviceState.READY;

        return st;
    }

    /**
     * Read a characteristic from MetaWear.
     * An intent with the action CHARACTERISTIC_READ will be broadcasted.
     * @see Action.BluetoothLe#ACTION_CHARACTERISTIC_READ
     */
    private static async Task<GattCommunicationStatus> readCharacteristicAsync() 
    {
        GattDeviceService service = deviceService;
        GATTCharacteristic charInfo = readCharUuids.Dequeue();

        GattCharacteristic characteristic = service.GetCharacteristics(charInfo.uuid)[0];
        GattReadResult rr = await characteristic.ReadValueAsync();

        byte[] retData = new byte[rr.Value.Length];

        DataReader.FromBuffer(rr.Value).ReadBytes(retData);

        LookupAndNotify(retData,DateTimeOffset.Now);

        return rr.Status;
    }
    
    /** MetaWear specific Bluetooth GATT callback
    private final BluetoothGattCallback metaWearGattCallback= new BluetoothGattCallback() 
    {
        private ArrayDeque<BluetoothGattCharacteristic> shouldNotify= new ArrayDeque<>();
        
        @Override
        public void onConnectionStateChange(BluetoothGatt gatt, int status,
                int newState) {
            Intent intent= new Intent();
            boolean broadcast= true;
            
            switch (newState) {
            case BluetoothProfile.STATE_CONNECTED:
                gatt.discoverServices();
                intent.setAction(Action.DEVICE_CONNECTED);
                connected= true;
                break;
            case BluetoothProfile.STATE_DISCONNECTED:
                intent.setAction(Action.DEVICE_DISCONNECTED);
                connected= false;
                break;
            default:
                broadcast= false;
                break;
            }
            if (broadcast) sendBroadcast(intent);
        }

        @Override
        public void onServicesDiscovered(BluetoothGatt gatt, int status) {
            for(BluetoothGattService service: gatt.getServices()) {
                for(BluetoothGattCharacteristic characteristic: service.getCharacteristics()) {
                    int charProps = characteristic.getProperties();
                    if ((charProps & BluetoothGattCharacteristic.PROPERTY_NOTIFY) != 0) {
                        shouldNotify.add(characteristic);
                    }
                }
            }
            deviceState= DeviceState.ENABLING_NOTIFICATIONS;
            setupNotification();
        }

        private void setupNotification() {
            metaWearGatt.setCharacteristicNotification(shouldNotify.peek(), true);
            BluetoothGattDescriptor descriptor= shouldNotify.poll().getDescriptor(CHARACTERISTIC_CONFIG);
            descriptor.setValue(BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE);
            metaWearGatt.writeDescriptor(descriptor);
        }

        @Override
        public void onDescriptorWrite(BluetoothGatt gatt,
                BluetoothGattDescriptor descriptor, int status) {
            if (!shouldNotify.isEmpty()) setupNotification();
            else deviceState= DeviceState.READY;
            
            if (deviceState == DeviceState.READY) {
                if (!commandBytes.isEmpty()) {
                    deviceState= DeviceState.WRITING_CHARACTERISTICS;
                    writeCommand();
                } else if (!readCharUuids.isEmpty()) {
                    deviceState= DeviceState.READING_CHARACTERISTICS;
                    readCharacteristic();
                }
            }
        }

        @Override
        public void onCharacteristicRead(BluetoothGatt gatt,
                BluetoothGattCharacteristic characteristic, int status) {
            Intent intent= new Intent(Action.CHARACTERISTIC_READ);
            intent.putExtra(Extra.SERVICE_UUID, characteristic.getService().getUuid());
            intent.putExtra(Extra.CHARACTERISTIC_UUID, characteristic.getUuid());
            intent.putExtra(Extra.CHARACTERISTIC_VALUE, characteristic.getValue());
            sendBroadcast(intent);

            if (!readCharUuids.isEmpty()) readCharacteristic(); 
            else deviceState= DeviceState.READY;
            
            if (deviceState == DeviceState.READY) {
                if (!commandBytes.isEmpty()) {
                    deviceState= DeviceState.WRITING_CHARACTERISTICS;
                    writeCommand();
                } else if (!readCharUuids.isEmpty()) {
                    deviceState= DeviceState.READING_CHARACTERISTICS;
                    readCharacteristic();
                }
            }
        }

        public void onCharacteristicWrite(BluetoothGatt gatt,
                BluetoothGattCharacteristic characteristic, int status) 
        {
            if (!commandBytes.isEmpty()) writeCommand();
            else deviceState= DeviceState.READY;
            if (deviceState == DeviceState.READY && !readCharUuids.isEmpty()) {
                deviceState= DeviceState.READING_CHARACTERISTICS;
                readCharacteristic();
            }
        }

        public void onCharacteristicChanged(BluetoothGatt gatt,
                BluetoothGattCharacteristic characteristic) 
        {
            Intent intent= new Intent(Action.NOTIFICATION_RECEIVED);
            intent.putExtra(Extra.CHARACTERISTIC_VALUE, characteristic.getValue());
            sendBroadcast(intent);
        }
    }
    */

    /**
     * Connect to the GATT service on the MetaWear device
     * @param metaWearBoard MetaWear device to connect to
     */
    /*
    public void connect(BluetoothDevice metaWearBoard) 
    {
        if (!metaWearBoard.equals(this.metaWearBoard)) 
        {
            commandBytes.clear();
            readCharUuids.clear();
        }

        deviceState= DeviceState.UNKNOWN;
        close();

        this.metaWearBoard= metaWearBoard;
        metaWearGatt= metaWearBoard.connectGatt(this, false, metaWearGattCallback);
    }
    
    public void reconnect() 
    {
        if (metaWearBoard != null) 
        {
            deviceState= DeviceState.UNKNOWN;
            close();
            metaWearGatt= metaWearBoard.connectGatt(this, false, metaWearGattCallback);
        }
    }
    
    public void disconnect() 
    {
        if (metaWearGatt != null) 
        {
            metaWearGatt.disconnect();
        }
    }
     */

    /** Close the GATT service and free up resources */
    /*
    public void close(Boolean notify) 
    {
        if (metaWearGatt != null) 
        {
            metaWearGatt.close();
            metaWearGatt= null;
            connected= false;
            if (notify) 
            {
                for(DeviceCallbacks it: deviceCallbacks) 
                {
                    it.disconnected();
                }
            }
        }
    }
     */

    /** Close the GATT service and free up resources */
    /*
    public void close() 
    {
        close(false);
    }
     */

    /*
    Binding between the Intent and this service 
    private Binder serviceBinder= new LocalBinder();

    Dummy class for getting the MetaWear BLE service from its binder
    public class LocalBinder extends Binder {
        /**
         * Get the MetaWearBLEService object
         * @return MetaWearBLEService object

         public MetaWearBleService getService() {
            return MetaWearBleService.this;
        }
    }

    @Override
    public IBinder onBind(Intent intent) {
        // TODO Auto-generated method stub
        return serviceBinder;
    }

    @Override
    public boolean onUnbind(Intent intent) 
    {
        close();
        return super.onUnbind(intent);
    }
    */

    // Reading a register really means writing a command to the command buffer and then receiving a response
    // as a characteristic output of the device.
    private static void readRegister( APIRegister register, params byte[] parameters)
    {
        queueCommand(Registers.buildReadCommand(register, parameters));
    }

    private static void writeRegister( APIRegister register, params byte[] data) 
    {
        queueCommand(Registers.buildWriteCommand(register, data));
    }

    /**
     * @param module
     * @param registerOpcode
     * @param data
     */
    private static async void queueCommand(byte[] command) 
    {
        // thsi will be replaced by the inerlocked write on Windows.
        commandBytes.Enqueue(command);
 //       if (deviceState == DeviceState.READY) 
        {
            deviceState= DeviceState.WRITING_CHARACTERISTICS;
            await writeCommandAsync();
        }
 //       else
 //       {
 //           deviceState = deviceState; // debug line
 //       }
    }
}
}