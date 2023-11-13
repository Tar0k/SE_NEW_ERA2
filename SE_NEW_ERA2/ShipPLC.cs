using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SE_NEW_ERA2;

public sealed class Program: MyGridProgram
{
    // BEGIN COPY
    private readonly IMyTextSurface _plcScreen;
    internal readonly CoreSystem _coreSystem;
    
    
    
    
    internal class CoreSystem
    {
        private readonly List<IMyTerminalBlock> _allBlocks = new List<IMyTerminalBlock>();
        public readonly Airlock Airlock1;

        public CoreSystem(Program program)
        {
            program.GridTerminalSystem.GetBlocks(_allBlocks);
            _allBlocks = _allBlocks.Where(b => b.IsSameConstructAs(program.Me)).ToList();
            Airlock1 = new Airlock(_allBlocks, "Шлюз 1");
        }

        public void Update()
        {
            Airlock1.Update();
        }
    }


    internal class DoorSystem
    {
        private readonly List<SafeDoor> _safeDoors = new List<SafeDoor>();
        
        public DoorSystem(IEnumerable<IMyTerminalBlock> blocks)
        {
            var doors = blocks.OfType<IMyDoor>();
            foreach (var door in doors)
            {
                _safeDoors.Add(new SafeDoor(door));
            }
        }

        public void Update()
        {
            foreach (var safeDoor in _safeDoors)
            {
                safeDoor.Update();
            }
        }


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


    internal class Airlock
    {
        private readonly IEnumerable<IMyTerminalBlock> _airLockBlocks;

        private readonly LightSystem _lightSystem;
        private readonly DisplaySystem _displaySystem;
        private readonly IEnumerable<IMySensorBlock> _sensors;
        private readonly IEnumerable<IMyAirVent> _airVentsInternal;
        private readonly IEnumerable<IMyAirVent> _airVentsExternal;
        private readonly DoorSystem _externalDoors;
        private readonly DoorSystem _internalDoors;

        private bool _inProgress;
        private bool step1;

        internal enum AirlockStatus
        {
            Depressurized,
            Balancing,
            Pressurized,
            Unknown
        }

        public string Name { get; }

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

            _externalDoors = new DoorSystem(externalDoors);
            _internalDoors = new DoorSystem(internalDoors);

            Update();
        }

        public float OxygenLevelInternal
        {
            get
            {
                return _airVentsInternal.Sum(av => av.GetOxygenLevel()) / _airVentsInternal.Count();
            }
        }
        
        public float OxygenLevelExternal
        {
            get
            {
                return _airVentsExternal.Sum(av => av.GetOxygenLevel()) / _airVentsExternal.Count();
            }
        }

        
        public AirlockStatus Status
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
            _externalDoors.Update();
            _internalDoors.Update();
            ShowStatus();
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