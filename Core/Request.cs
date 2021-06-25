using System.Collections.Generic;

namespace powerplant.Core
{
    public class Request
    {
        public int load { get; set; }
        public Fuels fuels { get; set; }
        public List<Powerplant> Powerplants { get; set; }
    }
}

