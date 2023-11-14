using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SE_NEW_ERA2
{
    public sealed class Program: MyGridProgram
    {
        // BEGIN COPY
        private readonly IMyTextSurface _plcScreen;
        private readonly CoreSystem _coreSystem;


        private class CoreSystem
        {
            private readonly List<IMyTerminalBlock> _allBlocks = new List<IMyTerminalBlock>();
            private readonly Airlock _airlock1;

            public CoreSystem(MyGridProgram program)
            {
                program.GridTerminalSystem.GetBlocks(_allBlocks);
                _allBlocks = _allBlocks.Where(b => b.IsSameConstructAs(program.Me)).ToList();
                _airlock1 = new Airlock(_allBlocks, "Шлюз 1");
            }

            public void Update()
            {
                _airlock1.Update();
            }
        }


        internal class SafeDoorSystem
        {
            private readonly List<SafeDoor> _safeDoors = new List<SafeDoor>();
        
            public SafeDoorSystem(IEnumerable<IMyTerminalBlock> blocks)
            {
                var doors = blocks.OfType<IMyDoor>();
                foreach (var door in doors)
                {
                    _safeDoors.Add(new SafeDoor(door));
                }
            }
        
            internal enum SafeDoorsStatus
            {
                Closed,
                Open,
                Closing,
                Opening,
                Unknown
            }

            public void Update()
            {
                foreach (var safeDoor in _safeDoors)
                {
                    safeDoor.Update();
                }
            }

            public bool Enabled
            {
                get
                {
                    return _safeDoors.All(sd => sd.Enabled);
                }
                set
                {
                    _safeDoors.ForEach(sd => sd.Enabled = value);
                } 
            }

            public SafeDoorsStatus DoorsStatus
            {
                get
                {
                    if (_safeDoors.All(sd => sd.DoorStatus == DoorStatus.Closed)) return SafeDoorsStatus.Closed;
                    if (_safeDoors.All(sd => sd.DoorStatus == DoorStatus.Open)) return SafeDoorsStatus.Open;
                    if (_safeDoors.Any(sd => sd.DoorStatus == DoorStatus.Closing)) return SafeDoorsStatus.Closing;
                    if (_safeDoors.Any(sd => sd.DoorStatus == DoorStatus.Opening)) return SafeDoorsStatus.Opening;
                    return SafeDoorsStatus.Unknown;
                }
            }
        
            public void OpenDoors() => _safeDoors.ForEach(d => d.OpenDoor());
            public void CloseDoors() => _safeDoors.ForEach(d => d.CloseDoor());

            private class SafeDoor
            {
                private readonly IMyDoor _door;
                private int _openDoorTimer;
                public SafeDoor(IMyDoor door)
                {
                    _door = door;
                    Update();
                }

                public void Update()
                {
                    switch (_door.Status)
                    {
                        case DoorStatus.Open:
                            _openDoorTimer += 1;
                            break;
                        case DoorStatus.Closed:
                            _openDoorTimer = 0;
                            break;
                        case DoorStatus.Opening:
                            break;
                        case DoorStatus.Closing:
                            break;
                        default:
                            break;
                    }
                    if (_openDoorTimer <= 1) return;
                    _openDoorTimer = 0;
                    _door.CloseDoor();
                }
            
                public DoorStatus DoorStatus => _door.Status;
                public void OpenDoor() => _door.OpenDoor();
                public void CloseDoor() => _door.CloseDoor();
                public void ToggleDoor() => _door.ToggleDoor();
                public bool Enabled
                {
                    get
                    {
                        return _door.Enabled;
                    }
                    set
                    { 
                        _door.Enabled = value;
                    }
                }
            }
        }

        private class LightSystem
        {
            private readonly List<IMyInteriorLight> _lights;

            public LightSystem(IEnumerable<IMyTerminalBlock> blocks)
            {
                _lights = blocks.OfType<IMyInteriorLight>().ToList();
                Default();
            }

            public void TurnOn()
            {
                foreach (var light in  _lights)
                {
                    light.Enabled = light.Enabled == false;
                }
            }

            public void TurnOff()
            {
                foreach (var light in  _lights) light.Enabled = light.Enabled != true;
            }

            public void WarningOn()
            {
                foreach (var light in _lights)
                {
                    light.Color = Color.Yellow;
                    light.BlinkLength = 3;
                }
            }

            public void WarningOff() => Default();

            public void AlarmOn()
            {
                foreach (var light in _lights)
                {
                    light.Color = Color.Red;
                    light.BlinkLength = 3;
                }
            }

            public void AlarmOff() => Default();

            private void Default()
            {
                foreach (var light in _lights)
                {
                    light.Color = Color.White;
                    light.BlinkLength = 0;
                }
            }
        }

        internal class DisplaySystem
        {
            private readonly List<IMyTextPanel> _displays;

            internal enum DisplayStatus
            {
                Normal,
                Warning,
                Alarm,
                Ok,
                Unknown
            }

            public DisplaySystem(IEnumerable<IMyTerminalBlock> blocks)
            {
                _displays = blocks.OfType<IMyTextPanel>().ToList();
                Default();
            }

            public DisplayStatus Status { get; private set; } = DisplayStatus.Unknown;

            public void WriteText(string value, DisplayStatus status = DisplayStatus.Normal, bool append = false)
            {
                switch (status)
                {
                    case DisplayStatus.Normal:
                        foreach (var display in _displays)
                        {
                            display.WriteText(value, append);
                        }
                        break;
                    case DisplayStatus.Alarm:
                        foreach (var display in _displays)
                        {
                            display.WriteText(value, append);
                            display.BackgroundColor = Color.Red;
                            display.FontColor = Color.White;
                        }
                        break;
                    case DisplayStatus.Warning:
                        foreach (var display in _displays)
                        {
                            display.WriteText(value, append);
                            display.BackgroundColor = Color.Yellow;
                            display.FontColor = Color.Black;
                        }
                        break;
                    case DisplayStatus.Ok:
                        foreach (var display in _displays)
                        {
                            display.WriteText(value, append);
                            display.BackgroundColor = Color.Green;
                            display.FontColor = Color.White;
                        }
                        break;
                    case DisplayStatus.Unknown:
                        foreach (var display in _displays)
                        {
                            display.WriteText(value, append);
                            display.BackgroundColor = Color.Red;
                            display.FontColor = Color.White;
                        }
                        break;
                    default:
                        Default();
                        break;
                }
            }

            private void Default()
            {
                foreach (var display in _displays)
                {
                    display.BackgroundColor = Color.Black;
                    display.FontColor = Color.White;
                    display.FontSize = 1.1f;
                    display.Alignment = TextAlignment.CENTER;
                }
            }

            private void Warning()
            {
                Status = DisplayStatus.Warning;
                foreach (var display in _displays)
                {
                    display.BackgroundColor = Color.Yellow;
                    display.FontColor = Color.Black;
                }
            }
        
            private void Alarm()
            {
                Status = DisplayStatus.Alarm;
                foreach (var display in _displays)
                {
                    display.BackgroundColor = Color.Red;
                    display.FontColor = Color.White;
                }
            }

            private void Ok()
            {
                Status = DisplayStatus.Normal;
                foreach (var display in _displays)
                {
                    display.BackgroundColor = Color.Green;
                    display.FontColor = Color.White;
                }
            }

        }


        private class Airlock
        {
            private readonly IEnumerable<IMyTerminalBlock> _airLockBlocks;

            private readonly LightSystem _lightSystem;
            private readonly DisplaySystem _displaySystem;
            private readonly IEnumerable<IMySensorBlock> _sensors;
            private readonly IEnumerable<IMyAirVent> _airVentsInternal;
            private readonly IEnumerable<IMyAirVent> _airVentsExternal;
            private readonly SafeDoorSystem _externalSafeDoors;
            private readonly SafeDoorSystem _internalSafeDoors;
            
            private int stepExternal;

            private enum AirlockStatus
            {
                Depressurized,
                Balancing,
                Pressurized,
                Unknown
            }

            private string Name { get; }

            public Airlock(IEnumerable<IMyTerminalBlock> blocks, string name = "Airlock")
            {
                Name = name;
                _airLockBlocks = blocks.Where(b => b.CustomData.ToLower().StartsWith(name.ToLower())).ToList();

                _lightSystem = new LightSystem(_airLockBlocks);
                _displaySystem = new DisplaySystem(_airLockBlocks);
                _sensors = _airLockBlocks.OfType<IMySensorBlock>();
                _airVentsExternal = _airLockBlocks.OfType<IMyAirVent>()
                    .Where(av => av.CustomData.ToLower().EndsWith("external"));
                _airVentsInternal = _airLockBlocks.OfType<IMyAirVent>()
                    .Where(av => av.CustomData.ToLower().EndsWith("internal"));

                var doors = _airLockBlocks.OfType<IMyDoor>().ToList();
                var externalDoors = doors.Where(d => d.CustomData.ToLower().EndsWith("external"));
                var internalDoors = doors.Where(d => d.CustomData.ToLower().EndsWith("internal"));

                _externalSafeDoors = new SafeDoorSystem(externalDoors);
                _internalSafeDoors = new SafeDoorSystem(internalDoors);

                Update();
            }

            private float OxygenLevelInternal
            {
                get
                {
                    return _airVentsInternal.Sum(av => av.GetOxygenLevel()) / _airVentsInternal.Count();
                }
            }

            private float OxygenLevelExternal
            {
                get
                {
                    return _airVentsExternal.Sum(av => av.GetOxygenLevel()) / _airVentsExternal.Count();
                }
            }


            private AirlockStatus Status
            {
                get
                {
                    if (OxygenLevelInternal < 0.2f) return AirlockStatus.Depressurized;
                    if (OxygenLevelInternal >= 0.2f && OxygenLevelInternal < 0.8f) return AirlockStatus.Balancing;
                    if (OxygenLevelInternal >= 0.8f) return AirlockStatus.Pressurized;
                    return AirlockStatus.Unknown;
                }
            }

            public void Update()
            {
                _externalSafeDoors.Update();
                _internalSafeDoors.Update();
                ShowStatus();
            }

            private void RequestExternal()
            {
                if (stepExternal == 0 || _sensors.Any(s => s.DetectPlayers)) return;
                stepExternal = 1;
            }

            private void Step1External()
            {
                _internalSafeDoors.CloseDoors();
                if (Math.Abs(OxygenLevelInternal - OxygenLevelExternal) < 0.01)
                {
                    _externalSafeDoors.OpenDoors();
                    stepExternal = 2;
                }
                else
                {
                    _airVentsInternal.ForEach(av => av.Depressurize = true);
                }
            }

            private void Step2External()
            {
                if (_externalSafeDoors.DoorsStatus != SafeDoorSystem.SafeDoorsStatus.Open) return;
                stepExternal = 3;
            }

            private void Step3External()
            {
                if (_sensors.Any(s => s.DetectPlayers)) return;
                _externalSafeDoors.CloseDoors();
                _internalSafeDoors.CloseDoors();
                stepExternal = 4;
            }

            private void Step4External()
            {
                if (Status == AirlockStatus.Pressurized)
                {
                    _internalSafeDoors.OpenDoors();
                    stepExternal = 5;
                }
                else
                {
                    _airVentsInternal.ForEach(av => av.Depressurize = false);
                }
            }

            private void Step5External()
            {
                if (!_sensors.Any(s => s.DetectPlayers) && _internalSafeDoors.DoorsStatus == SafeDoorSystem.SafeDoorsStatus.Closed)
                {
                    stepExternal = 0;
                }
            }
            

            private void RequestInternal()
            {
            }
        
            private void ShowStatus()
            {
                var oxygenLevelInternal = Math.Truncate(OxygenLevelInternal * 100);
                var oxygenLevelExternal = Math.Truncate(OxygenLevelExternal * 100);

                var infoText = $"{Name}\n" +
                               $"--Уровень кислорода--\n" +
                               $" Внутри: {oxygenLevelInternal}% Снаружи: {oxygenLevelExternal}%\n";
                switch (Status)
                {
                    case AirlockStatus.Depressurized:
                        infoText += "Воздух откачан";
                        _displaySystem.WriteText(infoText, DisplaySystem.DisplayStatus.Warning);
                        break;
                    case AirlockStatus.Pressurized:
                        infoText += "Воздух накачен";
                        _displaySystem.WriteText(infoText, DisplaySystem.DisplayStatus.Ok);
                        break;
                    case AirlockStatus.Balancing:
                        infoText += "Давление выравнивается";
                        _displaySystem.WriteText(infoText, DisplaySystem.DisplayStatus.Warning);
                        break;
                    case AirlockStatus.Unknown:
                        break;
                    default:
                        infoText += "Статус неопределен";
                        _displaySystem.WriteText(infoText, DisplaySystem.DisplayStatus.Alarm);
                        break;
                }
            }
        
        
            public override string ToString() => string.Join("\n", _airLockBlocks.Select(ab => ab.CustomName));
        
        }
    
        public Program()
        {
            _coreSystem = new CoreSystem(this);
            _plcScreen = Me.GetSurface(0);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
    
        public void Save()
        {
        }
    
    
        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.None:
                    break;
                case UpdateType.Terminal:
                    break;
                case UpdateType.Trigger:
                    switch (argument)
                    {
                        case "Шлюз 1 requestExternal":
                            Echo("Что-то запросило вход снаружи");
                            break;
                        case "Шлюз 1 requestInternal":
                            Echo("Что-то запросило вход изнутри");
                            break;
                    }
                    break;
                case UpdateType.Mod:
                    break;
                case UpdateType.Script:
                    break;
                case UpdateType.Update1:
                    break;
                case UpdateType.Update10:
                    break;
                case UpdateType.Update100:
                    _coreSystem.Update();
                    break;
                case UpdateType.Once:
                    break;
                case UpdateType.IGC:
                    break;
                default:
                    break;
            }
        }
    
        // END COPY
    }
}