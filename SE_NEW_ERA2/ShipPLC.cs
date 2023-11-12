using System.Collections;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

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
    
    internal class Airlock
    {
        private readonly IEnumerable<IMyTerminalBlock> _airLockBlocks;
        private readonly IEnumerable<IMyTextPanel> _displays;
        private readonly IEnumerable<IMyInteriorLight> _lights;
        private readonly IEnumerable<IMySensorBlock> _sensors;
        private readonly IEnumerable<IMyAirVent> _airVents;
        private readonly IEnumerable<IMyDoor> _externalDoors;
        private readonly IEnumerable<IMyDoor> _internalDoors;
        

        public Airlock(IEnumerable<IMyTerminalBlock> blocks, string name = "Airlock")
        {
            _airLockBlocks = blocks.Where(b => b.CustomData.ToLower().StartsWith(name.ToLower())).ToList();
            
            _displays = _airLockBlocks.OfType<IMyTextPanel>();
            _lights = _airLockBlocks.OfType<IMyInteriorLight>();
            _sensors = _airLockBlocks.OfType<IMySensorBlock>();
            _airVents = _airLockBlocks.OfType<IMyAirVent>();
            
            var doors = _airLockBlocks.OfType<IMyDoor>().ToList();
            _externalDoors = doors.Where(d => d.CustomData.ToLower().EndsWith("external"));
            _internalDoors = doors.Where(d => d.CustomData.ToLower().EndsWith("internal"));
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