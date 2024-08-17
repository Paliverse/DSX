# Mod System

DSX v3.1+ will support Legacy v2 Mod System until a new Mod System is built for v3 with advanced features like APIs etc..

[1. Lightbar LED](https://github.com/Paliverse/DSX/edit/main/Mod%20System%20(DSX%20v3)/README.md#lightbar-led)

[2. Player LED](https://github.com/Paliverse/DSX/edit/main/Mod%20System%20(DSX%20v3)/README.md#player-led)

[3. Mic LED](https://github.com/Paliverse/DSX/edit/main/Mod%20System%20(DSX%20v3)/README.md#mic-led)

[4. Trigger Threshold](https://github.com/Paliverse/DSX/edit/main/Mod%20System%20(DSX%20v3)/README.md#trigger-threshold)

[5. Adaptive Triggers](https://github.com/Paliverse/DSX/edit/main/Mod%20System%20(DSX%20v3)/README.md#adaptive-triggers)

### Usage:
___________________
#### Lightbar LED
```cs
// Needs 4 Params in List<int> (Red: 0-255) (Green: 0-255) (Blue: 0-255) (Brightness: 0-255)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddPlayerLEDToPacket(packet, controllerIndex, PlayerLEDNewRevision.One);

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________
#### Player LED
```cs
// Layout for Player LEDs
// Needs 1 Param (PlayerLEDNewRevision: Enum)
//  - -x- -  Player 1
//  - x-x -  Player 2
//  x -x- x  Player 3
//  x x-x x  Player 4
//  x xxx x  Player 5
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddPlayerLEDToPacket(packet, controllerIndex, PlayerLEDNewRevision.One);

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________
#### Mic LED
```cs
// Three modes: ON, PU;SE, or OFF
// Needs 1 Param (MicLEDMode: Enum)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddMicLEDToPacket(packet, controllerIndex, MicLEDMode.Pulse);

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________
#### Trigger Threshold
```cs
// This is used for telling the emulation when to send the "pressed state"
// Needs 1 Param in List<int> (Threshold: 0-255)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddTriggerThresholdToPacket(packet, controllerIndex, Trigger.Left, 0);
packet = AddTriggerThresholdToPacket(packet, controllerIndex, Trigger.Right, 0);

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

### Adaptive Triggers

___________________
#### Reset To User Settings
```cs
// When sent to DSX, it will discard any modifications
// sent to the controller and apply the settings from the profile it's attached to in DSX
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddResetToPacket(packet, controllerIndex);

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Normal (OFF)
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Normal, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Normal, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### GameCube
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.GameCube, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.GameCube, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### VerySoft
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VerySoft, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VerySoft, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Soft
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Soft, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Soft, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Hard
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hard, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hard, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### VeryHard
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VeryHard, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VeryHard, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Hardest
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Hardest, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Hardest, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Rigid
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Rigid, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Rigid, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### VibrateTrigger
```cs
// Needs 1 Param in List<int> (Frequency: 0-255)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, new List<int> { 10 });
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTrigger, new List<int> { 10 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Choppy
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Choppy, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Choppy, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Medium
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Medium, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Medium, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### VibrateTriggerPulse
```cs
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.VibrateTriggerPulse, new List<int>());
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.VibrateTriggerPulse, new List<int>());

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### CustomTriggerValue
```cs
// With CustomTriggerValueMode
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddCustomAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseAB, new List<int> { 0, 101, 255, 255, 0, 0, 0 });
packet = AddCustomAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseAB, new List<int> { 0, 101, 255, 255, 0, 0, 0 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Resistance
```cs
// Needs 2 Params in List<int> (Start: 0-9) (Force: 0-8)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new List<int> { 0, 8 });
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new List<int> { 0, 8 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Bow
```cs
// Needs 4 Params in List<int> (Start: 0-8) (End: 0-8) (Force: 0-8) (SnapForce: 0-8)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Resistance, new List<int> { 0, 8, 2, 5 });
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Resistance, new List<int> { 0, 8, 2, 5 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Galloping
```cs
// Needs 5 Params in List<int> (Start: 0-8) (End: 0-9) (FirstFoot: 0-6) (SecondFoot: 0-7) (Frequency: 0-255)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Galloping, new List<int> { 0, 9, 2, 4, 10 });
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Galloping, new List<int> { 0, 9, 2, 4, 10 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### SemiAutomaticGun
```cs
// Needs 3 Params in List<int> (Start: 2-7) (End: 0-8) (Force: 0-8)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.SemiAutomaticGun, new List<int> { 2, 7, 8 });
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun, new List<int> { 2, 7, 8 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### AutomaticGun
```cs
// Needs 3 Params in List<int> (Start: 2-7) (End: 0-8) (Force: 0-8)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, new List<int> { 0, 8, 10 });
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, new List<int> { 0, 8, 10 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________

#### Machine
```cs
// Needs 6 Params in List<int> (Start: 0-8) (End: 0-9) (StrengthA: 0-7) (StrengthB: 0-7) (Frequency: 0-255) (Period: 0-2)
// Usage ==============

Packet packet = new Packet();
int controllerIndex = 0;

packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Left, TriggerMode.Machine, new List<int> { 0, 9, 7, 7, 10, 0 });
packet = AddAdaptiveTriggerToPacket(packet, controllerIndex, Trigger.Right, TriggerMode.Machine, new List<int> { 0, 9, 7, 7, 10, 0 });

SendDataToDSX(packet);
GetDataFromDSX();
```
___________________
