using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MetaWearTest.Data;
using MetaWearTest.Common;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WE = Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

using MetaWearWinStoreAPI;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace MetaWearTest
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private static MetaWearBleService MWService;
        private static WE.DeviceInformationCollection MWDevices;

        public static HubPage Mainpage;

        public MetaWearBleService.MetaWearControllerImpl.LEDImpl mwLED;
        public MetaWearBleService.MetaWearControllerImpl.TemperatureImpl mwTemp;
        public MetaWearBleService.MetaWearControllerImpl.DebugImpl mwDebug;

        // DAta binding
        public Double XAccelSliderValue { get; set; }
        public Double YAccelSliderValue { get; set; }
        public Double ZAccelSliderValue { get; set; }

        public Boolean FallingCheckValue { get; set; }
        public String OrientTextValue { get; set; }

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public HubPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;

            Mainpage = this;  // pointer for the callbacks to use
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-4");
            this.DefaultViewModel["Section3Items"] = sampleDataGroup;
        }

        #region Controls

        /// <summary>
        /// Invoked when a HubSection header is clicked.
        /// </summary>
        /// <param name="sender">The Hub that contains the HubSection whose header was clicked.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            HubSection section = e.Section;
            var group = section.DataContext;
            this.Frame.Navigate(typeof(SectionPage), ((SampleDataGroup)group).UniqueId);
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            MWService = new MetaWearBleService();

            MWDevices = await MWService.GetDevicesAsync("");

            int device_count = MWService.GetDeviceCount();

            theDeviceListBox.Items.Clear();

            if (MWDevices.Count > 0)
            {
                foreach (var device in MWDevices)
                {
                    theDeviceListBox.Items.Add(device);
                }
                theDeviceListBox.Visibility = Visibility.Visible;
            }
            else
            {
                /*
                rootPage.NotifyUser("Could not find any battery devices. Please make sure your device is paired " +
                    "and powered on!",
                    NotifyType.StatusMessage);
                 */
            }

            (sender as Button).IsEnabled = true;
        }

        string containerId;
        private short RISE_TIME = 1, HIGH_TIME = 1000 , FALL_TIME =  0, DURATION = 50;
        private static byte REPEAT_COUNT = 255; // forever
        private LED.ColorChannel currentChannel = LED.ColorChannel.BLUE;
        /*
         * The value (3rd byte) sets the sampling rate, where 0x20 is 50Hz.  Each decrease by 0x08 increases the 
         * sample rate, i.e, 100, 200, 400, 800Hz.  Each increase of the value by 0x08 decreasses the sample rate, 
         * i.e., 12.5, 6.25, 1.56Hz. Remember that if the sampling rate is very high, the code to handle the samples 
         * must be very fast.  If not, samples and threads will pile up.  This can become a problem during debugging 
         * sessions.  
         */
        private static byte SAMPLE_RATE = 0x28; // slow for now

        private async void DevicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            theRunButton.IsEnabled = false;

            var device = theDeviceListBox.SelectedItem as WE.DeviceInformation;
            theDeviceListBox.Visibility = Visibility.Collapsed;

            containerId = "{" + device.Properties["System.Devices.ContainerId"] + "}";

            // statusTextBlock.Text = "Initializing device...";

            await MWService.InitializeDeviceAsync(0);

        XAccelSliderValue = 1.0;
        YAccelSliderValue = 1.0;
        ZAccelSliderValue = -1.0;

            // statusTextBlock.Text = "Done with init of MWdevice...";

            var mwController = new MetaWearBleService.MetaWearControllerImpl();
            var accelController = mwController.getModuleController("Accelerometer");

#if true
            (accelController as MetaWearBleService.MetaWearControllerImpl.AccelerometerImpl) // first 0 is accel range
                 .setComponentConfiguration(Accelerometer.Component.DATA, new byte[] { 0, 0, SAMPLE_RATE, 0, 0 });

            var cb = new AccelerometerCallbacks();
            var accelCallacks = mwController.addModuleCallback(cb);
            cb.AccelReadingChanged += UpdateAccelValue;

            (accelController as MetaWearBleService.MetaWearControllerImpl.AccelerometerImpl).disableNotification(Accelerometer.Component.DATA);
// HACK off for now            (accelController as MetaWearBleService.MetaWearControllerImpl.AccelerometerImpl).enableNotification(Accelerometer.Component.DATA);
#else
            (accelController as MetaWearBleService.MetaWearControllerImpl.AccelerometerImpl).disableNotification(Accelerometer.Component.DATA);
#endif

#if true
            var mwSwitch = mwController.getMechanicalSwitchModule();
            mwSwitch.enableNotification();
            mwController.addModuleCallback(new SwitchCallbacks());
#endif

#if true
            mwLED = mwController.getLEDDriverModule() as MetaWearBleService.MetaWearControllerImpl.LEDImpl;

            mwLED.stop(true);

            mwLED.setColorChannel(LED.ColorChannel.BLUE).withHighIntensity((byte)128 /*slider */)
                    .withLowIntensity((byte)0 /* slider */)
                    .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                    .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();
   
            mwLED.setColorChannel(LED.ColorChannel.GREEN).withHighIntensity((byte)128 /*slider */)
                    .withLowIntensity((byte)0 /* slider */)
                    .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                    .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();
            
            mwLED.setColorChannel(LED.ColorChannel.RED).withHighIntensity((byte)128 /*slider */)
                    .withLowIntensity((byte)0 /* slider */)
                    .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                    .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();

#endif

#if false
            var mwHaptic = mwController.getHapticModule();
            mwHaptic.startBuzzer(10);
            mwHaptic.startMotor(20);
#endif

#if true
            mwController.addModuleCallback(new TempCallbacks());
            mwTemp = mwController.getTemperatureModule() as MetaWearBleService.MetaWearControllerImpl.TemperatureImpl;
            mwTemp.readTemperature();  // cause temp to be returned.  This is returned in the characteristic callback
#endif

#if true
            mwDebug = mwController.getDebugModule() as MetaWearBleService.MetaWearControllerImpl.DebugImpl;
#endif
        }

        private async void UpdateAccelValue(object sender, AccelReadingArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                theXAccelSlider.Value = e.x;
                theYAccelSlider.Value = e.y;
                theZAccelSlider.Value = e.z;
            });
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void GangCheck_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LEDSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider control = sender as Slider;
            double value = control.Value;

            // for now, a way to force a read
            if ( mwTemp != null )
            {
                mwTemp.readTemperature();  // cause temp to be returned.  This is returned in the characteristic callback
            }

            if (control.Name == "RedLEDSlider")
            {
                mwLED.setColorChannel(LED.ColorChannel.RED).withHighIntensity((byte)value /*slider */)
                        .withLowIntensity((byte)0 /* slider */)
                        .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                        .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();
                if( (Boolean)theGangCheck.IsChecked )
                {
                    theGreenLEDSlider.Value = theBlueLEDSlider.Value = value;
                }
            }
            else if ( control.Name == "GreenLEDSlider")
            {
                mwLED.setColorChannel(LED.ColorChannel.GREEN).withHighIntensity((byte)value /*slider */)
                        .withLowIntensity((byte)0 /* slider */)
                        .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                        .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();
                if ((Boolean)theGangCheck.IsChecked)
                {
                    theRedLEDSlider.Value = theBlueLEDSlider.Value = value;
                }
            }
            else
            {
                mwLED.setColorChannel(LED.ColorChannel.BLUE).withHighIntensity((byte)value /*slider */)
                        .withLowIntensity((byte)0 /* slider */)
                        .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                        .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();
                if ((Boolean)theGangCheck.IsChecked)
                {
                    theRedLEDSlider.Value = theGreenLEDSlider.Value = value;
                }
            }

            if ( (Boolean) theGangCheck.IsChecked)
            {
                mwLED.setColorChannel(LED.ColorChannel.RED).withHighIntensity((byte)value /*slider */)
                        .withLowIntensity((byte)0 /* slider */)
                        .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                        .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();
                mwLED.setColorChannel(LED.ColorChannel.GREEN).withHighIntensity((byte)value /*slider */)
                        .withLowIntensity((byte)0 /* slider */)
                        .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                        .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();
                mwLED.setColorChannel(LED.ColorChannel.BLUE).withHighIntensity((byte)value /*slider */)
                        .withLowIntensity((byte)0 /* slider */)
                        .withRiseTime(RISE_TIME).withHighTime(HIGH_TIME).withFallTime(FALL_TIME)
                        .withPulseDuration(DURATION).withRepeatCount(REPEAT_COUNT).commit();

            }
        }

        private async void OnDeviceConnectionUpdated(bool isConnected)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (isConnected)
                {
               //     statusTextBlock.Text = "Waiting for device to send data...";
                }
                else
                {
                //    statusTextBlock.Text = "Waiting for device to connect...";
                }
            });
        }

        #endregion

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Loaded Handlers
        // this section includes handlers to make items within the Hub Section data template visible 

        private ListBox theDeviceListBox;
        private void DevicesListBox_Loaded(object sender, RoutedEventArgs e)
        {
            theDeviceListBox = sender as ListBox;
        }

        private Button theRunButton;
        private void RunButton_Loaded(object sender, RoutedEventArgs e)
        {
            theRunButton = sender as Button;
        }

        private Button theResetButton;
        private void ResetButton_Loaded(object sender, RoutedEventArgs e)
        {
            theResetButton = sender as Button;
        }
        #endregion

        public Slider theXAccelSlider;
        private void XAccelSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theXAccelSlider = sender as Slider;
        }

        public Slider theYAccelSlider;
        private void YAccelSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theYAccelSlider = sender as Slider;
        }

        public Slider theZAccelSlider;
        private void ZAccelSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theZAccelSlider = sender as Slider;
        }

        public CheckBox theFallingCheck;
        private void FallingCheck_Loaded(object sender, RoutedEventArgs e)
        {
            theFallingCheck = sender as CheckBox;
        }

        public TextBlock theOrientText;
        private void OrientText_Loaded(object sender, RoutedEventArgs e)
        {
            theOrientText = sender as TextBlock;
        }

        private Slider theRedLEDSlider;
        private void RedLEDSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theRedLEDSlider = sender as Slider;
        }

        private Slider theBlueLEDSlider;
        private void BlueLEDSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theBlueLEDSlider = sender as Slider;
        }

        private Slider theGreenLEDSlider;
        private void GreenLEDSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theGreenLEDSlider = sender as Slider;
        }

        private CheckBox theGangCheck;
        private void GangCheck_Loaded(object sender, RoutedEventArgs e)
        {
            theGangCheck = sender as CheckBox;
        }

        private TextBlock theLEDText;
        private void LEDText_Loaded(object sender, RoutedEventArgs e)
        {
            theLEDText = sender as TextBlock;
        }

        public Slider theTempCSlider;
        private void TempCSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theTempCSlider = sender as Slider;
        }

        public Slider theTempFSlider;
        private void TempFSlider_Loaded(object sender, RoutedEventArgs e)
        {
            theTempFSlider = sender as Slider;
        }

        public TextBlock theTempText;
        private void TempText_Loaded(object sender, RoutedEventArgs e)
        {
            theTempText = sender as TextBlock;
        }

        private void XAccelSliderJ_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void AllOffButton_Click(object sender, RoutedEventArgs e)
        {
            mwLED.stop(false); // don't clear, so we can start again
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            mwLED.play(true);
        }

        public ToggleSwitch theSwitchState;
        private void SwitchState_Loaded(object sender, RoutedEventArgs e)
        {
            theSwitchState = sender as ToggleSwitch;
        }

        public Button theResetDeviceSwitch;
        private void ResetDevice_Loaded(object sender, RoutedEventArgs e)
        {
            theResetDeviceSwitch = sender as Button;
        }

        private void ResetDevice_Click(object sender, RoutedEventArgs e)
        {
            mwDebug.resetDevice();
        }
    }
}
