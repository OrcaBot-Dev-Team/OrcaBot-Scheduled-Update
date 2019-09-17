using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace OrcaBotScheduledUpdate.Model
{
    [Serializable()]
    class System {
      
        public string Name { get; set; }
        public Tuple<float, float, float> Position { get; set; }
        public List<Station> Stations { get; set; }

        public Security Security { get; set; }
        public System() {
            Stations = new List<Station>();
        }

    }
    [Serializable()]
    class Station
    {
        
        public string Name { get; set; }


        public float Distance { get; set; }

        public LandingPadSize PadSize { get; set; }
        public StationType StationType { get; set; }
        public Facilities[] Facilities { get; set; }
        public Economy Economy { get; set; }

        

    }
    [Serializable()]
    public enum Facilities : sbyte
    {
        InterstellarFactors,
        Repair,
        Restock,
        Shipyard,
        Trader_Encoded,
        Trader_Raw,
        Trader_Manufactured,
        Black_Market,
        Unknown,
        Outfitting
    }
    [Serializable()]
    public enum LandingPadSize : sbyte
    {
        None,
        Medium,
        Large
    }
    [Serializable()]
    public enum Economy : sbyte
    {
        Unknown,
        Extraction,
        Refinery,
        Industrial,
        High_Tech,
        Agriculture,
        Terraforming,
        Tourism,
        Service,
        Military,
        Colony,
        Rescue,
        Damaged,
        Repair,
        Engineer
    }
    [Serializable()]
    public enum Security : sbyte
    {
        Unknown = -1,
        Anarchy = 64,
        Low = 16,
        Medium = 32,
        High = 48
    }
    [Serializable()]
    public enum StationType : sbyte
    {
        Unknown = -1,
        Coriolis,
        Ocellus,
        Orbis,
        Outpost,
        Asteroid_Base,
        Installation,
        Mega_Ship,
        Surface_Station,
        Surface_Settlement


    }
}
