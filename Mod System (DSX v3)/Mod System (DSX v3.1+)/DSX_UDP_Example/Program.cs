using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DSX_UDP_Example
{
    internal class Program
    {
        static UdpClient client;
        static IPEndPoint endPoint;

        static DateTime TimeSent;

        static List<Device> devices = new List<Device>();

      
        static void Main(string[] args)
        {
            Connect();
            GetConnectedDevicesFromDSX();

            while (true)
            {
                if (!devices.Any())
                {
                    GetConnectedDevicesFromDSX();
                }
                else
                {
                    for (int i = 0; i < devices.Count; i++)
                    {
                        // Send the below Packet to all devices in a for loop

                        Packet packet = new Packet();

                        int controllerIndex = devices[i].Index;

                        packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, new List<int> { 0, 8, 10});
                        packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Normal, new List<int>());

                        packet = AddTriggerThresholdToPacket(packet, controllerIndex, Trigger.Left, 0);

                        packet = AddTriggerThresholdToPacket(packet, controllerIndex, Trigger.Right, 0);

                        packet = AddRGBToPacket(packet, controllerIndex, 255, 255, 255, 255);

                        packet = AddPlayerLEDToPacket(packet, controllerIndex, PlayerLEDNewRevision.One);

                        packet = AddMicLEDToPacket(packet, controllerIndex, MicLEDMode.Pulse);


                        SendDataToDSX(packet);

                    }

                    GetDataFromDSX();
                }





                Console.WriteLine("Press any key to send again\n");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Establishes a connection to the DSX server on a specified port.
        /// Initializes the UDP client and endpoint for communication with the server.
        /// </summary>
        static void Connect()
        {
            try
            {
                var port = FetchPortNumber();
                Console.WriteLine($"Connecting to Server on Port: {port}\n");
                client = new UdpClient();
                endPoint = new IPEndPoint(Triggers.localhost, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        /// <summary>
        /// Fetches the UDP port number from a configuration file in the AppData\Local\DSX directory.
        /// If the file is not found, contains invalid data, or an error occurs, it falls back to a default port number.
        /// Provides logging for all relevant steps and potential issues.
        /// </summary>
        /// <returns>The port number to use for communication (default: 6969 if an error occurs).</returns>
        static int FetchPortNumber()
        {
            // ONLY WORKS WITH DSX v3.1 BETA 1.37 AND ABOVE

            const int defaultPort = 6969;
            const string appFolderName = "DSX";
            const string fileName = "DSX_UDP_PortNumber.txt";

            try
            {
                Console.WriteLine("Fetching Port Number locally...");

                // Get the Local AppData path for the application
                string localAppDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    appFolderName
                );

                string portFilePath = Path.Combine(localAppDataPath, fileName);

                // Check if the file exists
                if (File.Exists(portFilePath))
                {
                    Console.WriteLine($"Port file found at: {portFilePath}");

                    // Try to read and parse the port number
                    string portNumberContent = File.ReadAllText(portFilePath).Trim();
                    if (int.TryParse(portNumberContent, out int portNumber))
                    {
                        Console.WriteLine($"Port Number successfully read: {portNumber}");
                        return portNumber;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid port number format in file: {portNumberContent}");
                    }
                }
                else
                {
                    Console.WriteLine($"Port file not found at: {portFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching the port number: {ex.Message}");
            }

            // Fallback to default port number
            Console.WriteLine($"Falling back to default port number: {defaultPort}");
            return defaultPort;
        }

        /// <summary>
        /// Sends a packet of data to the DSX server.
        /// Converts the packet to a JSON string, sends it via UDP, and logs the time the data was sent.
        /// </summary>
        /// <param name="data">The packet of data to be sent to the DSX server.</param>
        static void SendDataToDSX(Packet data)
        {
            try
            {
                var RequestData = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
                client.Send(RequestData, RequestData.Length, endPoint);
                TimeSent = DateTime.Now;
                Console.WriteLine($"Instructions Sent at {DateTime.Now} with data: ({Triggers.PacketToJson(data)})\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        /// <summary>
        /// Receives and processes data from the DSX server.
        /// Deserializes the JSON response from the server, logs the information about connected devices,
        /// and updates the device list with the data received.
        /// </summary>
        static void GetDataFromDSX()
        {
            Console.WriteLine("Waiting for Server Response...\n");

            try
            {
                // Receive the response bytes from the server.
                byte[] bytesReceivedFromServer = client.Receive(ref endPoint);

                // Check if the server has sent a response.
                if (bytesReceivedFromServer.Length > 0)
                {
                    // Deserialize the received JSON response into a ServerResponse object.
                    ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>(
                        Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length));

                    // Print a visual separator in the console for better readability.
                    Console.WriteLine("===================================================================");

                    // Capture the current time to calculate the response time.
                    DateTime CurrentTime = DateTime.Now;
                    TimeSpan Timespan = CurrentTime - TimeSent;

                    // Log the status and response time from the server.
                    Console.WriteLine($"Status                  - {ServerResponseJson.Status}");
                    Console.WriteLine($"Time Received           - {ServerResponseJson.TimeReceived}, took: {Timespan.TotalMilliseconds} ms to receive response from DSX");
                    Console.WriteLine($"isControllerConnected   - {ServerResponseJson.isControllerConnected}");
                    Console.WriteLine($"BatteryLevel            - {ServerResponseJson.BatteryLevel}\n");

                    // Log the number of devices connected to the server (DSX).
                    Console.WriteLine($"Devices Connected to DSX: {ServerResponseJson.Devices.Count}");

                    // Clear the existing list of devices before populating it with new data.
                    devices.Clear();

                    // Iterate through each device in the server's response and log its details.
                    foreach (Device device in ServerResponseJson.Devices)
                    {
                        // Add the device to the devices list.
                        devices.Add(device);

                        // Log the device's properties.
                        Console.WriteLine("-------------------------------");
                        Console.WriteLine($"Controller Index        - {device.Index}");
                        Console.WriteLine($"MacAddress              - {device.MacAddress}");
                        Console.WriteLine($"DeviceType              - {device.DeviceType}");
                        Console.WriteLine($"ConnectionType          - {device.ConnectionType}");
                        Console.WriteLine($"BatteryLevel            - {device.BatteryLevel}");
                        Console.WriteLine($"IsSupportAT             - {device.IsSupportAT}");
                        Console.WriteLine($"IsSupportLightBar       - {device.IsSupportLightBar}");
                        Console.WriteLine($"IsSupportPlayerLED      - {device.IsSupportPlayerLED}");
                        Console.WriteLine($"IsSupportMicLED         - {device.IsSupportMicLED}");
                        Console.WriteLine("-------------------------------\n");
                    }

                    // Print a closing visual separator for better readability.
                    Console.WriteLine("===================================================================\n");
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the process.
                // Possible that DSX Server is not running, or DSX is not running at all.
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Sends a request to the DSX server to retrieve information about connected devices.
        /// Combines several steps: prepares the packet, sends it to the server, and then retrieves the data.
        /// </summary>
        static void GetConnectedDevicesFromDSX()
        {
            // Get Data from DSX first about connected devices
            Packet packet = new Packet();

            packet = AddGetDSXStatusToPacket(packet);

            SendDataToDSX(packet);

            GetDataFromDSX();
        }

        /// <summary>
        /// Adds an adaptive trigger instruction to the packet for a specified controller index.
        /// This instruction configures the trigger mode and parameters for the adaptive trigger.
        /// </summary>
        /// <param name="packet">The packet to which the instruction will be added.</param>
        /// <param name="controllerIndex">The index of the controller to apply the trigger instruction.</param>
        /// <param name="trigger">The trigger (e.g., left or right trigger) to be configured.</param>
        /// <param name="triggerMode">The mode to set for the adaptive trigger.</param>
        /// <param name="parameters">Additional parameters required by the trigger mode.</param>
        /// <returns>Returns the packet with the adaptive trigger instruction added.</returns>
        static Packet AddAdaptiveTriggerToPacket(Packet packet, int controllerIndex, Trigger trigger, TriggerMode triggerMode, List<int> parameters)
        {
            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            // Combine the fixed and variable parameters
            var combinedParameters = new object[3 + parameters.Count];
            combinedParameters[0] = controllerIndex;
            combinedParameters[1] = trigger;
            combinedParameters[2] = triggerMode;

            // Copy the List<int> parameters into the combinedParameters array
            for (int i = 0; i < parameters.Count; i++)
            {
                combinedParameters[3 + i] = parameters[i];
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = combinedParameters
            };

            return packet;
        }

        /// <summary>
        /// Adds a custom adaptive trigger instruction to the packet for a specified controller index.
        /// This allows for more complex trigger configurations, including a custom value mode and additional parameters.
        /// </summary>
        /// <param name="packet">The packet to which the instruction will be added.</param>
        /// <param name="controllerIndex">The index of the controller to apply the trigger instruction.</param>
        /// <param name="trigger">The trigger (e.g., left or right trigger) to be configured.</param>
        /// <param name="triggerMode">The mode to set for the adaptive trigger.</param>
        /// <param name="valueMode">The custom value mode for more detailed trigger control.</param>
        /// <param name="parameters">Additional parameters required by the custom trigger mode.</param>
        /// <returns>Returns the packet with the custom adaptive trigger instruction added.</returns>
        static Packet AddCustomAdaptiveTriggerToPacket(Packet packet, int controllerIndex, Trigger trigger, TriggerMode triggerMode, CustomTriggerValueMode valueMode, List<int> parameters)
        {
            // This method is only for TriggerMode.CustomTriggerValue

            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            // Combine the fixed and variable parameters
            var combinedParameters = new object[4 + parameters.Count];
            combinedParameters[0] = controllerIndex;
            combinedParameters[1] = trigger;
            combinedParameters[2] = triggerMode;
            combinedParameters[3] = valueMode;

            // Copy the List<int> parameters into the combinedParameters array
            for (int i = 0; i < parameters.Count; i++)
            {
                combinedParameters[3 + i] = parameters[i];
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = combinedParameters
            };

            return packet;
        }

        /// <summary>
        /// Adds a trigger threshold instruction to the packet for a specified controller index.
        /// This instruction sets the threshold for a trigger's actuation point.
        /// </summary>
        /// <param name="packet">The packet to which the instruction will be added.</param>
        /// <param name="controllerIndex">The index of the controller to apply the threshold instruction.</param>
        /// <param name="trigger">The trigger (e.g., left or right trigger) to be configured.</param>
        /// <param name="threshold">The threshold value for the trigger.</param>
        /// <returns>Returns the packet with the trigger threshold instruction added.</returns>
        static Packet AddTriggerThresholdToPacket(Packet packet, int controllerIndex, Trigger trigger, int threshold)
        {
            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.TriggerThreshold,
                parameters = new object[] { controllerIndex, trigger, threshold }
            };

            return packet;
        }

        /// <summary>
        /// Adds an RGB update instruction to the packet for a specified controller index.
        /// This instruction sets the color and brightness of the RGB LEDs on the controller.
        /// </summary>
        /// <param name="packet">The packet to which the instruction will be added.</param>
        /// <param name="controllerIndex">The index of the controller to apply the RGB instruction.</param>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="brightness">The brightness level of the LEDs.</param>
        /// <returns>Returns the packet with the RGB update instruction added.</returns>
        static Packet AddRGBToPacket(Packet packet, int controllerIndex, int red, int green, int blue, int brightness)
        {
            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.RGBUpdate,
                parameters = new object[] { controllerIndex, red, green, blue, brightness }
            };

            return packet;
        }

        /// <summary>
        /// Adds a player LED update instruction to the packet for a specified controller index.
        /// This instruction configures the player indicator LEDs on the controller.
        /// </summary>
        /// <param name="packet">The packet to which the instruction will be added.</param>
        /// <param name="controllerIndex">The index of the controller to apply the player LED instruction.</param>
        /// <param name="playerLED">The player LED configuration to apply.</param>
        /// <returns>Returns the packet with the player LED update instruction added.</returns>
        static Packet AddPlayerLEDToPacket(Packet packet, int controllerIndex, PlayerLEDNewRevision playerLED)
        {
            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.PlayerLEDNewRevision,
                parameters = new object[] { controllerIndex, playerLED }
            };

            return packet;
        }

        /// <summary>
        /// Adds a microphone LED update instruction to the packet for a specified controller index.
        /// This instruction configures the microphone mute/unmute LED on the controller.
        /// </summary>
        /// <param name="packet">The packet to which the instruction will be added.</param>
        /// <param name="controllerIndex">The index of the controller to apply the microphone LED instruction.</param>
        /// <param name="micLED">The microphone LED configuration to apply.</param>
        /// <returns>Returns the packet with the microphone LED update instruction added.</returns>
        static Packet AddMicLEDToPacket(Packet packet, int controllerIndex, MicLEDMode micLED)
        {
            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.MicLED,
                parameters = new object[] { controllerIndex, micLED }
            };

            return packet;
        }

        /// <summary>
        /// Adds a reset instruction to the packet for a specified controller index.
        /// This instruction resets the controller's settings to the user's predefined settings from DSX.
        /// </summary>
        /// <param name="packet">The packet to which the instruction will be added.</param>
        /// <param name="controllerIndex">The index of the controller to reset.</param>
        /// <returns>Returns the packet with the reset instruction added.</returns>
        static Packet AddResetToPacket(Packet packet, int controllerIndex)
        {
            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.ResetToUserSettings,
                parameters = new object[] { controllerIndex }
            };

            return packet;
        }

        /// <summary>
        /// Adds a request to get the DSX status to the packet.
        /// This instruction requests the current status of the DSX server with the list of devices.
        /// </summary>
        /// <param name="packet">The packet to which the status request will be added.</param>
        /// <returns>Returns the packet with the status request instruction added.</returns>
        static Packet AddGetDSXStatusToPacket(Packet packet)
        {
            int instCount;

            if (packet.instructions == null)
            {
                packet.instructions = new Instruction[1];
                instCount = 0;
            }
            else
            {
                instCount = packet.instructions.Length;
                Array.Resize(ref packet.instructions, instCount + 1);
            }

            packet.instructions[instCount] = new Instruction
            {
                type = InstructionType.GetDSXStatus,
                parameters = new object[] {  }
            };

            return packet;
        }


        // All Configurations:
        // ----------------------------------------------------------------------------------------------------------------------------

        // Reset To User Settings
        // When sent to DSX, it will discard any modifications
        // sent to the controller and apply the settings from the profile it's attached to in DSX
        // Usage ==============
        // packet = AddResetToPacket(packet, controllerIndex);

        // DSX v3 Trigger Modes:

        // OFF:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.OFF, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.OFF, new List<int>());

        // FEEDBACK:
        // Needs 2 Params in List<int> (Start Position: 1-9) -> (Resistance Strength: 1-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, new List<int> { 1, 8 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.FEEDBACK, new List<int> { 1, 8 });

        // WEAPON:
        // Needs 3 Params in List<int> (Start Position: 2-7) -> (End Position: 3-8) -> (Resistance Strength: 1-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.WEAPON, new List<int> { 2, 6, 8 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.WEAPON, new List<int> { 2, 6, 8 });

        // VIBRATION:
        // Needs 3 Params in List<int> (Start Position: 1-9) -> (Amplitude: 1-8) -> (Frequency: 1-40)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VIBRATION, new List<int> { 1, 8, 10 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VIBRATION, new List<int> { 1, 8, 10 });

        // SLOPE_FEEDBACK:
        // Needs 4 Params in List<int> (Start Position: 1-8) -> (End Position: 2-9) -> (Start Resistance Strength: 1-8) -> (End Resistance Strength: 1-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.SLOPE_FEEDBACK, new List<int> { 1, 9, 8, 1 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.SLOPE_FEEDBACK, new List<int> { 1, 9, 8, 1 });

        // MULTIPLE_POSITION_FEEDBACK:
        // Needs 10 Params in List<int> 10 Region Resistance Strength: (Region 1: 0-8) -> (Region 2: 0-8) -> (Region 3: 0-8) -> (Region 4: 0-8) -> (Region 5: 0-8) -> (Region 6: 0-8) -> (Region 7: 0-8) -> (Region 8: 0-8) -> (Region 9: 0-8) -> (Region 10: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.MULTIPLE_POSITION_FEEDBACK, new List<int> { 8, 8, 8, 0, 0, 0, 8, 8, 0, 0 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.MULTIPLE_POSITION_FEEDBACK, new List<int> { 8, 8, 8, 0, 0, 0, 8, 8, 0, 0 });

        // MULTIPLE_POSITION_VIBRATION:
        // Needs 11 Params in List<int> (Frequency: 1-40) -> 10 Region Amplitude: (Region 1: 0-8) -> (Region 2: 0-8) -> (Region 3: 0-8) -> (Region 4: 0-8) -> (Region 5: 0-8) -> (Region 6: 0-8) -> (Region 7: 0-8) -> (Region 8: 0-8) -> (Region 9: 0-8) -> (Region 10: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.MULTIPLE_POSITION_VIBRATION, new List<int> { 10, 8, 8, 8, 8, 8, 0, 0, 0, 8, 8 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.MULTIPLE_POSITION_VIBRATION, new List<int> { 10, 8, 8, 8, 8, 8, 0, 0, 0, 8, 8 });

        // Normal:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Normal, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Normal, new List<int>());

        // GameCube:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.GameCube, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.GameCube, new List<int>());

        // VerySoft:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VerySoft, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VerySoft, new List<int>());

        // Soft:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Soft, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Soft, new List<int>());

        // Hard:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hard, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hard, new List<int>());

        // VeryHard:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VeryHard, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VeryHard, new List<int>());

        // Hardest:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hardest, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest, new List<int>());

        // Rigid:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Rigid, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Rigid, new List<int>());

        // VibrateTrigger:
        // Needs 1 Param in List<int> (Frequency: 0-255)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, new List<int> { 10 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTrigger, new List<int> { 10 });

        // Choppy:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Choppy, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Choppy, new List<int>());

        // Medium:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Medium, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Medium, new List<int>());

        // VibrateTriggerPulse:
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTriggerPulse, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTriggerPulse, new List<int>());

        // CustomTriggerValue:
        // With CustomTriggerValueMode
        // Usage ==============
        // packet = AddCustomAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseAB, new List<int> { 0, 101, 255, 255, 0, 0, 0 });
        // packet = AddCustomAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseAB, new List<int> { 0, 101, 255, 255, 0, 0, 0 });

        // Resistance
        // Needs 2 Params in List<int> (Start: 0-9) -> (Force: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new List<int> { 0, 8 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new List<int> { 0, 8 });

        // Bow:
        // Needs 4 Params in List<int> (Start: 0-8) -> (End: 0-8) -> (Force: 0-8) -> (SnapForce: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new List<int> { 0, 8, 2, 5 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new List<int> { 0, 8, 2, 5 });

        // Galloping:
        // Needs 5 Params in List<int> (Start: 0-8) -> (End: 0-9) -> (FirstFoot: 0-6) -> (SecondFoot: 0-7) -> (Frequency: 0-255)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Galloping, new List<int> { 0, 9, 2, 4, 10 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Galloping, new List<int> { 0, 9, 2, 4, 10 });

        // SemiAutomaticGun:
        // Needs 3 Params in List<int> (Start: 2-7) -> (End: 0-8) -> (Force: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.SemiAutomaticGun, new List<int> { 2, 7, 8 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun, new List<int> { 2, 7, 8 });

        // AutomaticGun:
        // Needs 3 Params in List<int> (Start: 0-9) -> (Strength: 0-8) -> (Frequency: 0-255)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, new List<int> { 0, 8, 10 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new List<int> { 0, 8, 10 });

        // Machine:
        // Needs 6 Params in List<int> (Start: 0-8) -> (End: 0-9) -> (StrengthA: 0-7) -> (StrengthB: 0-7) -> (Frequency: 0-255) -> (Period: 0-2)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Machine, new List<int> { 0, 9, 7, 7, 10, 0 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Machine, new List<int> { 0, 9, 7, 7, 10, 0 });

        // VIBRATE_TRIGGER_10Hz:
        // Applies Vibration Effect with 10 Hz (Frequency)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VIBRATE_TRIGGER_10Hz, new List<int>());
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VIBRATE_TRIGGER_10Hz, new List<int>());


        // TriggerThreshold
        // This is used for telling the emulation when to send the "pressed state"
        // Needs 1 Param in List<int> (Threshold: 0-255)
        // Usage ==============
        // packet = AddTriggerThresholdToPacket(packet, controllerIndex, Trigger.Left, 0);
        // packet = AddTriggerThresholdToPacket(packet, controllerIndex, Trigger.Right, 0);

        // Lightbar LED
        // Needs 4 Params in List<int> (Red: 0-255) (Green: 0-255) (Blue: 0-255) (Brightness: 0-255)
        // Usage ==============
        // packet = AddRGBToPacket(packet, controllerIndex, 255, 255, 255, 255);

        // Player LED
        // Layout for Player LEDs
        // Needs 1 Param (PlayerLEDNewRevision: Enum)
        //  - -x- -  Player 1
        //  - x-x -  Player 2
        //  x -x- x  Player 3
        //  x x-x x  Player 4
        //  x xxx x  Player 5
        // Usage ==============
        // packet = AddPlayerLEDToPacket(packet, controllerIndex, PlayerLEDNewRevision.One);

        // Mic LED
        // Three modes: ON, PU;SE, or OFF
        // Needs 1 Param (MicLEDMode: Enum)
        // Usage ==============
        // packet = AddMicLEDToPacket(packet, controllerIndex, MicLEDMode.Pulse);

    }
}
