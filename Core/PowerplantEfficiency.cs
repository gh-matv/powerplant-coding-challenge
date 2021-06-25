using System;

namespace powerplant.Core
{
    public class PowerplantEfficiency : IEquatable<PowerplantEfficiency>, IComparable<PowerplantEfficiency>
    {
        public Powerplant p;
        public Fuels f;

        private double _eff = 0;

        public PowerplantEfficiency(Powerplant pwp, Fuels fs)
        {
            p = pwp;
            f = fs;
        }

        public int CompareTo(PowerplantEfficiency other)
        {
            if (p.IsGreenEnergy() && !other.p.IsGreenEnergy()) return -1;
            if (!p.IsGreenEnergy() && other.p.IsGreenEnergy()) return 1;
            return (int)(GetEfficiency() - other.GetEfficiency());
        }

        public bool Equals(PowerplantEfficiency other)
        {
            return GetEfficiency() == other.GetEfficiency();
        }

        public double GetEfficiency()
        {
            // Use stored value if it exists
            if (_eff != 0) return _eff;

            var fuelPrice = p._type switch
            {
                Powerplant.PowerplantType.PT_GASFIRED => f.GasEuroMWh,
                Powerplant.PowerplantType.PT_TURBOJET => f.KerosineEuroMWh,
                Powerplant.PowerplantType.PT_WINDTURBINE => 0,
                _ => double.MinValue,// In case we dont know the type, try not to use it at all
            };

            // If it's not a green-type energy (currently only windmills)
            fuelPrice += p.IsGreenEnergy() ? 0 : Config.TonOfCo2PerMwh * f.Co2EuroTon;

            if (fuelPrice == 0) fuelPrice = .001;

            // Save the computed value to save time next call
            _eff = p.Efficiency / fuelPrice;

            if (p.IsGreenEnergy()) _eff *= ((float)f.Wind / 100);

            // The higher eff is, the better  
            return _eff;
        }
    }
}