using Newtonsoft.Json;

namespace powerplant.Core
{

    // could be improved :
    // We could make the fuels more dynamic by storing them in a map, and in another map store the relations
    //  between powerplant type and fuels consumption. But since the json looks like a human-readable file,
    //  that may not be a good idea.

    public class Fuels
    {
        [JsonProperty("gas(euro/MWh)")]
        public double GasEuroMWh { get; set; }

        [JsonProperty("kerosine(euro/MWh)")]
        public double KerosineEuroMWh { get; set; }

        [JsonProperty("co2(euro/ton)")]
        public int Co2EuroTon { get; set; }

        [JsonProperty("wind(%)")]
        public int Wind { get; set; }
    }
}