using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace OrcaBotScheduledUpdate.Model
{
    class System {
        public Tuple<float, float, float> Position { get; set; }
        public Station[] Stations { get; set; }

        public Security Security { get; set; }
    }

    class Station
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("distanceToArrival")]
        public float Distance { get; set; }

        [JsonProperty("name")]
        public LandingPadSize PadSize { get; set; }
        public StationType StationType { get; set; }
        public Facilities[] Facilities { get; set; }
        public Economy Economy { get; set; }
    }
    public enum Facilities
    {
        InterstellarFactors,
        Repair,
        Restock,
        Shipyard,
        Trader_Encoded,
        Trader_Raw,
        Trader_Manufactured
    }
    public enum LandingPadSize
    {
        Medium,
        Large
    }
    public enum Economy
    {

    }
    public enum Security
    {
        Anarchy,
        Low,
        Medium,
        High
    }
    public enum StationType
    {

    }
}
