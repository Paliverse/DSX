using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                if (devices.Count == 0)
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

        static void Connect()
        {
            try
            {
                Console.WriteLine($"Connecting to Server on Port: {6975}\n");
                client = new UdpClient();
                endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(7000));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

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

        static void GetDataFromDSX()
        {
            Console.WriteLine("Waiting for Server Response...\n");
            try
            {
                byte[] bytesReceivedFromServer = client.Receive(ref endPoint);

                if (bytesReceivedFromServer.Length > 0)
                {
                    ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>($"{Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length)}");
                    Console.WriteLine("===================================================================");

                    DateTime CurrentTime = DateTime.Now;
                    TimeSpan Timespan = CurrentTime - TimeSent;
                    // First send shows high Milliseconds response time for some reason

                    Console.WriteLine($"Status                  - {ServerResponseJson.Status}");
                    Console.WriteLine($"Time Received           - {ServerResponseJson.TimeReceived}, took: {Timespan.TotalMilliseconds} to receive response from DSX");
                    Console.WriteLine($"isControllerConnected   - {ServerResponseJson.isControllerConnected}");
                    Console.WriteLine($"BatteryLevel            - {ServerResponseJson.BatteryLevel}\n");

                    Console.WriteLine($"Devices Connected to DSX: {ServerResponseJson.Devices.Count}");

                    devices.Clear();
                    foreach (Device device in ServerResponseJson.Devices)
                    {
                        devices.Add(device);

                        Console.WriteLine("-------------------------------");
                        Console.WriteLine($"Controller Index    - {device.Index}");
                        Console.WriteLine($"MacAddress          - {device.MacAddress}");
                        Console.WriteLine($"DeviceType          - {device.DeviceType}");
                        Console.WriteLine($"ConnectionType      - {device.ConnectionType}");
                        Console.WriteLine($"BatteryLevel        - {device.BatteryLevel}");
                        Console.WriteLine($"IsSupportAT         - {device.IsSupportAT}");
                        Console.WriteLine($"IsSupportLightBar   - {device.IsSupportLightBar}");
                        Console.WriteLine($"IsSupportPlayerLED  - {device.IsSupportPlayerLED}");
                        Console.WriteLine($"IsSupportMicLED     - {device.IsSupportMicLED}");
                        Console.WriteLine("-------------------------------\n");
                    }

                    Console.WriteLine("===================================================================\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void GetConnectedDevicesFromDSX()
        {
            // Get Data from DSX first about connected devices
            Packet packet = new Packet();

            packet = AddResetToPacket(packet, 0);

            SendDataToDSX(packet);

            GetDataFromDSX();
        }

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
        static Packet AddCustomAdaptiveTriggerToPacket(Packet packet, int controllerIndex, Trigger trigger, TriggerMode triggerMode, CustomTriggerValueMode valueMode, List<int> parameters)
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

        // All Configurations:
        // ----------------------------------------------------------------------------------------------------------------------------

        // Reset To User Settings
        // When sent to DSX, it will discard any modifications
        // sent to the controller and apply the settings from the profile it's attached to in DSX
        // Usage ==============
        // packet = AddResetToPacket(packet, controllerIndex);

        // Normal (OFF):
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
        // Needs 2 Params in List<int> (Start: 0-9) (Force: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new List<int> { 0, 8 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new List<int> { 0, 8 });

        // Bow:
        // Needs 4 Params in List<int> (Start: 0-8) (End: 0-8) (Force: 0-8) (SnapForce: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new List<int> { 0, 8, 2, 5 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new List<int> { 0, 8, 2, 5 });

        // Galloping:
        // Needs 5 Params in List<int> (Start: 0-8) (End: 0-9) (FirstFoot: 0-6) (SecondFoot: 0-7) (Frequency: 0-255)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Galloping, new List<int> { 0, 9, 2, 4, 10 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Galloping, new List<int> { 0, 9, 2, 4, 10 });

        // SemiAutomaticGun:
        // Needs 3 Params in List<int> (Start: 2-7) (End: 0-8) (Force: 0-8)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.SemiAutomaticGun, new List<int> { 2, 7, 8 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun, new List<int> { 2, 7, 8 });

        // AutomaticGun:
        // Needs 3 Params in List<int> (Start: 0-9) (Strength: 0-8) (Frequency: 0-255)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, new List<int> { 0, 8, 10 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new List<int> { 0, 8, 10 });

        // Machine:
        // Needs 6 Params in List<int> (Start: 0-8) (End: 0-9) (StrengthA: 0-7) (StrengthB: 0-7) (Frequency: 0-255) (Period: 0-2)
        // Usage ==============
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Machine, new List<int> { 0, 9, 7, 7, 10, 0 });
        // packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Machine, new List<int> { 0, 9, 7, 7, 10, 0 });

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
