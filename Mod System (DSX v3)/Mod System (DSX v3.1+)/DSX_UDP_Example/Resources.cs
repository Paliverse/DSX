﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace DSX_UDP_Example
{
    internal class Resources
    {
    }

    public static class Triggers
    {
        public static IPAddress localhost = new IPAddress(new byte[] { 127, 0, 0, 1 });

        /// <summary>
        /// Converts a Packet object to its JSON string representation.
        /// </summary>
        /// <param name="packet">The Packet object to serialize.</param>
        /// <returns>A JSON string representing the Packet object.</returns>
        public static string PacketToJson(Packet packet)
        {
            try
            {
                return JsonConvert.SerializeObject(packet);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Serialization error: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Deserializes a JSON string into a Packet object.
        /// </summary>
        /// <param name="json">The JSON string representing a Packet.</param>
        /// <returns>A Packet object deserialized from the JSON string.</returns>
        public static Packet JsonToPacket(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
    }

    public struct Instruction
    {
        public InstructionType type;
        public object[] parameters;
    }

    public class Packet
    {
        public Instruction[] instructions;
    }

    public class ServerResponse
    {
        public string Status;
        public string TimeReceived;
        public bool isControllerConnected;
        public int BatteryLevel;
        public List<Device> Devices;
    }

    public class Device
    {
        public int Index;
        public string MacAddress;
        public DeviceType DeviceType;
        public ConnectionType ConnectionType;
        public int BatteryLevel;
        public bool IsSupportAT;
        public bool IsSupportLightBar;
        public bool IsSupportPlayerLED;
        public bool IsSupportMicLED;
    }

    public enum TriggerMode
    {
        Normal = 0,
        GameCube = 1,
        VerySoft = 2,
        Soft = 3,
        Hard = 4,
        VeryHard = 5,
        Hardest = 6,
        Rigid = 7,
        VibrateTrigger = 8,
        Choppy = 9,
        Medium = 10,
        VibrateTriggerPulse = 11,
        CustomTriggerValue = 12,
        Resistance = 13,
        Bow = 14,
        Galloping = 15,
        SemiAutomaticGun = 16,
        AutomaticGun = 17,
        Machine = 18
    }

    public enum CustomTriggerValueMode
    {
        OFF = 0,
        Rigid = 1,
        RigidA = 2,
        RigidB = 3,
        RigidAB = 4,
        Pulse = 5,
        PulseA = 6,
        PulseB = 7,
        PulseAB = 8,
        VibrateResistance = 9,
        VibrateResistanceA = 10,
        VibrateResistanceB = 11,
        VibrateResistanceAB = 12,
        VibratePulse = 13,
        VibratePulseA = 14,
        VibratePulsB = 15,
        VibratePulseAB = 16
    }

    public enum PlayerLEDNewRevision
    {
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3,
        Five = 4, // Five is Also All On
        AllOff = 5
    }
    public enum MicLEDMode
    {
        On = 0,
        Pulse = 1,
        Off = 2
    }

    public enum Trigger
    {
        Invalid,
        Left,
        Right
    }

    public enum InstructionType
    {
        GetDSXStatus,
        TriggerUpdate,
        RGBUpdate,
        PlayerLED,
        TriggerThreshold,
        MicLED,
        PlayerLEDNewRevision,
        ResetToUserSettings
    }

    public enum ConnectionType
    {
        USB,
        BLUETOOTH,
        DONGLE
    }

    public enum DeviceType
    {
        DUALSENSE,
        DUALSENSE_EDGE,
        DUALSHOCK_V1,
        DUALSHOCK_V2,
        DUALSHOCK_DONGLE,
        PS_VR2_LeftController,
        PS_VR2_RightController,
        ACCESS_CONTROLLER
    }

    
}