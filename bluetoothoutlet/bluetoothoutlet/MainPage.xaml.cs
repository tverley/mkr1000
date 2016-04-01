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

using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace bluetoothoutlet
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //IStream connection; BT Connection
        NetworkSerial wifiConnection;
        RemoteDevice mkr1000;

        byte upperOutletPin = 3;
        byte lowerOutletPin = 2;
        byte wificonnectled = 11;
        byte analogupper = 15; //"A1"
        byte analoglower = 16; //"A2"
        int uppercounter = 0;
        int lowercounter = 0;
        int uppercurrenttotal = 0;
        int lowercurrenttotal = 0;


        public MainPage()
        {
            this.InitializeComponent();

            ConnectWifi();

            mkr1000.AnalogPinUpdated += AnalogPinsUpdated;

        }

        
        private void ConnectWifi()
        {

            connectedTB.Text = "Connecting";
            //connection = new BluetoothSerial("HC-06"); BT Connection
            wifiConnection = new NetworkSerial(new Windows.Networking.HostName("192.168.0.123"), 3030); // wifi connection
            //uno = new RemoteDevice(connection); BT Connection
            mkr1000 = new RemoteDevice(wifiConnection);
            //connection.ConnectionEstablished += OnConnectionEstablished; BT Connection
            //connection.begin(9600, SerialConfig.SERIAL_8N1); BT Connection
            wifiConnection.ConnectionEstablished += OnConnectionEstablished;
            wifiConnection.ConnectionFailed += OnConnectionFailed;
            
            wifiConnection.begin(115200, SerialConfig.SERIAL_8N1);
        }
                
        private void OnConnectionFailed(string message)
        {
            connectedTB.Text = "Failed.....Reconnecting";
            ConnectWifi();

        }

        private void OnConnectionEstablished()
        {            
            connectedTB.Text = "Connected";
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                
                upperOnButton.IsEnabled = false;
                upperOffButton.IsEnabled = true;
                lowerOnButton.IsEnabled = false;
                lowerOffButton.IsEnabled = true;

                
                mkr1000.pinMode(upperOutletPin, PinMode.OUTPUT);
                mkr1000.pinMode(lowerOutletPin, PinMode.OUTPUT);
                mkr1000.pinMode(wificonnectled, PinMode.OUTPUT);
                mkr1000.digitalWrite(wificonnectled, PinState.HIGH);

                upperCurrentTB.Text = "Upper Current Display :";
                lowerCurrentTB.Text = "Lower Current Display :";
                mkr1000.pinMode(analogupper, PinMode.ANALOG);
                mkr1000.pinMode(analoglower, PinMode.ANALOG);

                //mkr1000.AnalogPinUpdated += AnalogPinsUpdated;

                //GetUpperCurrent();
            }));           


        }

        private void GetUpperCurrent()
        {
            uppercurrenttotal = mkr1000.analogRead("A1");

            upperCurrentTB.Text = uppercurrenttotal.ToString();
        }

        private async void AnalogPinsUpdated(string pin, ushort value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (pin == "A1")
                {
                    uppercurrenttotal += value;
                    uppercounter++;
                }

                if (pin == "A2")
                {
                    lowercurrenttotal += value;
                    lowercounter++;
                }

                if (uppercounter == 60)
                {
                    upperCurrentTB.Text = (uppercurrenttotal / 60).ToString();
                    uppercurrenttotal = 0;
                    uppercounter = 0;
                }

                if (lowercounter == 60)
                {
                    lowerCurrentTB.Text = (lowercurrenttotal / 60).ToString();
                    lowercurrenttotal = 0;
                    lowercounter = 0;
                }                
            });
        }

        private void upperOnButton_Click(object sender, RoutedEventArgs e)
        {            
            mkr1000.digitalWrite(upperOutletPin, PinState.LOW);           
            upperOnButton.IsEnabled = false;
            upperOffButton.IsEnabled = true;
        }

        private void upperOffButton_Click(object sender, RoutedEventArgs e)
        {            
            mkr1000.digitalWrite(upperOutletPin, PinState.HIGH);
            upperOnButton.IsEnabled = true;
            upperOffButton.IsEnabled = false;
        }

        private void lowerOnButton_Click(object sender, RoutedEventArgs e)
        {            
            mkr1000.digitalWrite(lowerOutletPin, PinState.LOW);
            lowerOnButton.IsEnabled = false;
            lowerOffButton.IsEnabled = true;
        }

        private void lowerOffButton_Click(object sender, RoutedEventArgs e)
        {            
            mkr1000.digitalWrite(lowerOutletPin, PinState.HIGH);
            lowerOnButton.IsEnabled = true;
            lowerOffButton.IsEnabled = false;
        }
    }
}
