using System.Collections.Generic;
using System.Runtime.Serialization;

namespace powerplant.Core
{
    public class Powerplant
    {
        public enum PowerplantType
        {
            [EnumMember(Value = "gasfired")]
            PT_GASFIRED,
            [EnumMember(Value = "turbojet")]
            PT_TURBOJET,
            [EnumMember(Value = "windturbine")]
            PT_WINDTURBINE,
            [EnumMember(Value = "[UNK]")]
            PT_UNK
        }

        public static readonly Dictionary<string, PowerplantType> DTypes = new()
        {
            { "gasfired", PowerplantType.PT_GASFIRED },
            { "turbojet", PowerplantType.PT_TURBOJET },
            { "windturbine", PowerplantType.PT_WINDTURBINE }
        };

        // Some functions still check for the type directly instead of this to check if
        //  this is green energy
        public static readonly Dictionary<PowerplantType, bool> DTypeIsGreen = new()
        {
            {PowerplantType.PT_WINDTURBINE, true}
        };

        public string name { get; set; }

        public PowerplantType _type;
        public string Type
        {
            get => "[should not be used]";
            set => _type = DTypes.ContainsKey(value) ? DTypes[value] : PowerplantType.PT_UNK;
        }

        public double Efficiency { get; set; }
        public int Pmin { get; set; }
        public int Pmax { get; set; }

        // Returns the actual pmax for a powerplant.
        // This is pmax except for windmills, which is pmax * wind%
        public int PMaxForFuels(Fuels f)
        {
            return this.Pmax * (DTypeIsGreen.ContainsKey(_type) ? f.Wind / 100 : 1);
        }

        public bool IsGreenEnergy()
        {
            return DTypeIsGreen.ContainsKey(_type);
        }
    }
}