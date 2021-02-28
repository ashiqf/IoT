using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using System.IO.Ports;

namespace ControlDevicesusingCSharp
{
    class Program
    {
        static SerialPort SerialPort { get; set; }
        private const string IotHubUri = "XXXXX-env.azure-devices.net";
        private const string deviceKey = "XXXXXXXXXXXXXCVfexlh2fA9WIuvlpVdvuuuXCc=";
        private const string deviceId = "myarduinodevice";
        private static CancellationToken _ct;
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            _ct = cts.Token;

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine($"{DateTime.Now} > Cancelling...");
                cts.Cancel();

                eventArgs.Cancel = true;
            };

            try
            {
                var t = Task.Run(Run, cts.Token);
                await t;
            }
            catch (IotHubCommunicationException)
            {
                Console.WriteLine($"{DateTime.Now} > Operation has been cancelled.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"{DateTime.Now} > Operation has been cancelled.");
            }
            finally
            {
                cts.Dispose();
            }

            Console.ReadKey();
        }
        static void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            // Read the data that's in the serial buffer.
            var serialdata = serialPort.ReadExisting();
        }
        private static async Task Run()
        {
            using var deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
            
            // Initialises the serial port communication on COM4
            SerialPort = new SerialPort("COM4")
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None
            };
            // Subscribe to the event
            SerialPort.DataReceived += SerialPortDataReceived;
            // Now open the port.
            SerialPort.Open();

            Console.WriteLine($"Azure IoT Hub: {IotHubUri}");
            Console.WriteLine($"Device ID: {deviceId}");

            while (!_ct.IsCancellationRequested)
            {
                Console.WriteLine($"{DateTime.Now} > Waiting for new messages");
                var receivedMessage = await deviceClient.ReceiveAsync(_ct);
                if (receivedMessage == null) continue;

                var msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                Console.WriteLine($"{DateTime.Now} > Received message: {msg}");

                switch (msg)
                {
                    case "on1":
                        Console.WriteLine($"{DateTime.Now} > Turn on the light connected on Arduino.");
                        SerialPort.Write("A");
                        break;
                    case "off1":
                        Console.WriteLine($"{DateTime.Now} > Turn off the light connected on Arduino.");
                        SerialPort.Write("B");
                        break;		            
                    default:
                        Console.WriteLine($"Message not configured: {msg}");
                        break;
                }

                await deviceClient.CompleteAsync(receivedMessage, _ct);
            }
        }
    }
}
