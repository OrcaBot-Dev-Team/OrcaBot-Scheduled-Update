using System;
using System.Collections.Generic;
using System.Text;

namespace OrcaBotScheduledUpdate.JSONModel
{
    public class System
    {
        public string name;
        public float x, y, z;
        public string security;
    }

    public class Station
    {
        public string type, name, economy, systemName;
        public bool haveMarket, haveShipyard, haveOutfitting;
        public string[] otherServices;
        public float? distanceToArrival;
    }
}
