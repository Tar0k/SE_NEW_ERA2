using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace SE_NEW_ERA2;

public sealed class Program: MyGridProgram
{
    // BEGIN COPY
    
    private readonly List<IMyTerminalBlock> _allBlocks = new List<IMyTerminalBlock>();
    private readonly IMyTextSurface _plcScreen;
    private readonly Airlock _airlock1;


    internal class CoreSystem
    {
        private readonly List<IMyTerminalBlock> _allBlocks = new List<IMyTerminalBlock>();
        private readonly Airlock _airlock1;

        public CoreSystem(Program program)
        {
            program.GridTerminalSystem.GetBlocks(_allBlocks);
            _allBlocks = _allBlocks.Where(b => b.IsSameConstructAs(program.Me)).ToList();
            _airlock1 = new Airlock(_allBlocks, "Шлюз 1");
        }
    }

    private readonly CoreSystem _coreSystem;
    
    public Program()
    {
        _coreSystem = new CoreSystem(this);
        _plcScreen = Me.GetSurface(0);
        Runtime.UpdateFrequency = UpdateFrequency.Update100;
    }
    
    internal class LightSystem
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
            foreach (var light in  _lights)
            {
                light.Enabled = light.Enabled != true;
            }
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

        public void Default()
        {
            foreach (var light in _lights)
            {
                light.Color = Color.White;
                light.BlinkLength = 0;
            }
        }
    }
    
    
    internal class Airlock
    {
        private readonly IEnumerable<IMyTerminalBlock> _airLockBlocks;
        
        private readonly LightSystem _lightSystem;
        private readonly IEnumerable<IMyTextPanel> _displays;
        private readonly IEnumerable<IMySensorBlock> _sensors;
        private readonly IEnumerable<IMyAirVent> _airVents;
        private readonly IEnumerable<IMyDoor> _externalDoors;
        private readonly IEnumerable<IMyDoor> _internalDoors;

        internal enum AirlockStatus
        {
            Depressurized,
            Depressurizing,
            Pressurized,
            Pressurizing,
            Unknown
        }
        

        public Airlock(IEnumerable<IMyTerminalBlock> blocks, string name = "Airlock")
        {
            _airLockBlocks = blocks.Where(b => b.CustomData.ToLower().StartsWith(name.ToLower())).ToList();
            
            _lightSystem = new LightSystem(_airLockBlocks);
            _displays = _airLockBlocks.OfType<IMyTextPanel>();
            _sensors = _airLockBlocks.OfType<IMySensorBlock>();
            _airVents = _airLockBlocks.OfType<IMyAirVent>();
            
            var doors = _airLockBlocks.OfType<IMyDoor>().ToList();
            _externalDoors = doors.Where(d => d.CustomData.ToLower().EndsWith("external"));
            _internalDoors = doors.Where(d => d.CustomData.ToLower().EndsWith("internal"));
        }

        public AirlockStatus Status
        {
            get
            {
                if (_airVents.All(av => av.Status == VentStatus.Depressurized)) return AirlockStatus.Depressurized;
                if (_airVents.All(av => av.Status == VentStatus.Depressurizing)) return AirlockStatus.Depressurizing;
                if (_airVents.All(av => av.Status == VentStatus.Pressurized)) return AirlockStatus.Pressurized;
                if (_airVents.All(av => av.Status == VentStatus.Pressurizing)) return AirlockStatus.Pressurizing;
                return AirlockStatus.Unknown;
            }
        }

        private void ShowStatus()
        {
            switch (Status)
            {
                case AirlockStatus.Depressurized:
                    _lightSystem.WarningOff();
                    break;
                case AirlockStatus.Depressurizing:
                    _lightSystem.WarningOn();
                    break;
                case AirlockStatus.Pressurizing:
                    _lightSystem.WarningOn();
                    break;
                case AirlockStatus.Pressurized:
                    _lightSystem.WarningOff();
                    break;
                case AirlockStatus.Unknown:
                    _lightSystem.AlarmOn();
                    break;
                default:
                    _lightSystem.Default();
                    break;
            }
        }

        public override string ToString() => string.Join("\n", _airLockBlocks.Select(ab => ab.CustomName));
        
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