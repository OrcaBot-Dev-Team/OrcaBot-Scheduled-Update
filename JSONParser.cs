using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrcaBotScheduledUpdate
{
    static class JSONParser
    {
        public static Dictionary<string, Model.System> Parse(string stationsData, string populatedSystemsData) {
            Dictionary<string, Model.System> systemDict = new Dictionary<string, Model.System>() ;
            var systemDefinition = new[] { new { name = "", x = 0f, y = 0f, z = 0f, security = "" } };
            var stationDefinition = new[] { new { type = "", name = "", distanceToArrival = 0f, economy = "", haveShipyard = false, haveOutfitting = false, otherServices = new[] { "" },systemName="" } };
            var settings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            JArray systemArray = JArray.Parse(populatedSystemsData);
            foreach(var system in systemArray) {
                int Security = (int)system["security_id"];
                Model.Security security;
                switch (Security) {
                    case 64: security = Model.Security.Anarchy; break;
                    case 48: security = Model.Security.High; break;
                    case 32: security = Model.Security.Medium; break;
                    case 16: security = Model.Security.Low; break;
                    default: security = Model.Security.Anarchy; break;
                }

                systemDict.Add(system["name"].ToString().ToUpper(), new Model.System {
                    Position = Tuple.Create<float, float, float>((float)system["x"], (float)system["y"], (float)system["z"]),
                    Security = security
                });
            }

            //var systemResult = JsonConvert.DeserializeAnonymousType(populatedSystemsData, systemDefinition,settings);
            //var stationResult = JArray.Parse(stationsData);
          
            var x = 0;
            return systemDict;
        }
    }
}
