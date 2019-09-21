using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



/*
 *  var systemDefinition = new[] { new { name = "", x = 0f, y = 0f, z = 0f, security = "" } };
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
*/

namespace OrcaBotScheduledUpdate
{
    static class JSONParser
    {
        static List<string> ErrorLog = new List<string>();
        public static Dictionary<string, Model.System> Parse(string stationsFile, string populatedFile) {
            Dictionary<string, Model.System> systemDict = new Dictionary<string, Model.System>();
        
           
            using(StreamReader sr = File.OpenText(populatedFile)) {
                using (JsonReader jtr = new JsonTextReader(sr)) {
                    JsonSerializerSettings jss = new JsonSerializerSettings {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    JsonSerializer js = JsonSerializer.Create(jss);
                    IList<JSONModel.System> result = js.Deserialize<List<JSONModel.System>>(jtr);
                    //Now create the proper dictionary entries out of the result, witout the stations for now, that is
                    foreach(var system in result) {
                        var sys = ParseSystem(system);
                        if(sys == null) {
                            continue;
                        }
                        systemDict[system.name.ToUpper()] = sys;
                        Logger.Instance.Write("Parsed system " + system.name, Logger.MessageType.Verbose);
                    }
                    result.Clear();
                    Logger.Instance.Write("Finished parsing Systems", Logger.MessageType.Info);


                }
            }
           
            GC.Collect();
            using (StreamReader sr = File.OpenText(stationsFile))
            using(JsonTextReader jtr = new JsonTextReader(sr)) {
                JsonSerializerSettings jss = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                JsonSerializer js = JsonSerializer.Create(jss);
                IList<JSONModel.Station> result = js.Deserialize<List<JSONModel.Station>>(jtr);
                //Now create the proper dictionary out of the result.
                
                foreach (var station in result) {
                    if(station.name == null || station.distanceToArrival == null || station.economy == null) {
                        //No point having these in the Dictionary if any of these are not in the JSON
                        string statName = (station.name == null) ? "a station" : $"the {station.name} station";
                        ErrorLog.Add("Could not add " + statName);
                        continue;
                        
                    }
                    if (!systemDict.ContainsKey(station.systemName.ToUpper())) {
                        ErrorLog.Add($"Could not add station {station.name} because the system name ({station.systemName}) could not be found in the set up dictionary");
                        continue;
                    }
                    var economy = GetEconomy(station.economy);
                    var type = GetStationType(station.type);
                            
                    var stationToAdd = new Model.Station() {
                        Name = station.name,
                        Economy = economy,
                        StationType = type,
                        Facilities = GetFacilities(station.otherServices, economy, station.haveShipyard, station.haveOutfitting),
                        PadSize = GetLandingPadSize(type),
                        Distance = (float)station.distanceToArrival
                    };
                    systemDict[station.systemName.ToUpper()].Stations.Add(stationToAdd);
                    Logger.Instance.Write($"Parsed station {station.name} from the {station.systemName} system.", Logger.MessageType.Verbose);

                }
            }

            PrintErrorLog();
           
            
            return systemDict;
            
        }
        public static string Stringify<T>(Dictionary<string,T> dict) {
            return JsonConvert.SerializeObject(dict, Formatting.None);
        }
        static Model.Economy GetEconomy(string s) {
            if(string.IsNullOrEmpty(s)) {
                return Model.Economy.Unknown;
            }
            switch (s.ToLower()) {
                case "extraction": return Model.Economy.Extraction;
                case "refinery": return Model.Economy.Refinery;
                case "industrial": return Model.Economy.Industrial;
                case "high tech": return Model.Economy.High_Tech;
                case "agriculture": return Model.Economy.Agriculture;
                case "terraforming": return Model.Economy.Terraforming;
                case "tourism": return Model.Economy.Tourism;
                case "service": return Model.Economy.Service;
                case "military": return Model.Economy.Military;
                case "colony": return Model.Economy.Colony;
                case "rescue": return Model.Economy.Rescue;
                case "damaged": return Model.Economy.Damaged;
                case "repair": return Model.Economy.Repair;
                default: return Model.Economy.Unknown;
            } 
        }
        static Model.StationType GetStationType(string s) {
            if (string.IsNullOrEmpty(s)) {
                return Model.StationType.Unknown;
            }
            switch (s.ToLower()) {
                case "planetary outpost": return Model.StationType.Surface_Station;
                case "orbis starport": return Model.StationType.Orbis;
                case "ocellus starport": return Model.StationType.Ocellus;
                case "coriolis starport": return Model.StationType.Coriolis;
                case "mega ship": return Model.StationType.Mega_Ship;
                case "outpost": return Model.StationType.Outpost;
                case "asteroid base": return Model.StationType.Asteroid_Base;
                default: return Model.StationType.Unknown;
            }
        }
        private static Model.System ParseSystem(JSONModel.System sys) {
            if(sys.name == null || sys.x == null|| sys.y == null || sys.z == null) {
                ErrorLog.Add(("Could not add ") + ((sys.name == null) ? "a" : ($"the {sys.name}") + " system."));
                return null;
            }
            return new Model.System() {
                Name = sys.name,
                Position = new Tuple<float, float, float>(sys.x, sys.y, sys.z),
                Security = GetSecurityFromString(sys.security)
            };
        
        }

        private static Model.LandingPadSize GetLandingPadSize(Model.StationType st) {
            switch (st) {
                case Model.StationType.Asteroid_Base:
                case Model.StationType.Coriolis:
                case Model.StationType.Mega_Ship:
                case Model.StationType.Ocellus:
                case Model.StationType.Orbis:
                case Model.StationType.Surface_Station:
                   return Model.LandingPadSize.Large; 
                case Model.StationType.Outpost:
                    return Model.LandingPadSize.Medium; 
                default:
                   return Model.LandingPadSize.None; 
            }
        }

        private static Model.Security GetSecurityFromString(string securityString) {
            if (securityString == null) {
                return Model.Security.Unknown;
            }
            else {
                switch (securityString.ToLower()) {
                    case "high":
                        return Model.Security.High;
                    case "medium":
                        return Model.Security.Medium;
                    case "low":
                        return Model.Security.Low; 
                    case "anarchy":
                        return Model.Security.Anarchy;
                    default:
                        return Model.Security.Unknown; 
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="facilities">Array of facilities as strings</param>
        /// <param name="economy">In case it is a Material Trader, the Economy is needed to identify the type. If none is given, Model.Facilities.Unknown will be returned</param>
        /// <returns></returns>
        static Model.Facilities[] GetFacilities(string[] facilities, Model.Economy economy = Model.Economy.Unknown, bool hasShipyard = false, bool hasOutfitting = false) {
            HashSet<Model.Facilities> returnHashSet = new HashSet<Model.Facilities>();
            foreach (var f in facilities) {
                var facility = GetFacility(f);
                if (facility != Model.Facilities.Unknown) {
                    returnHashSet.Add(facility);
                }
            }
            if (hasShipyard) {
                returnHashSet.Add(Model.Facilities.Shipyard);
            }
            if (hasOutfitting) {
                returnHashSet.Add(Model.Facilities.Outfitting);
            }
            Model.Facilities[] returnArray = new Model.Facilities[returnHashSet.Count];
            returnHashSet.CopyTo(returnArray);
            return returnArray;



            Model.Facilities GetFacility(string s) {
                if (string.IsNullOrEmpty(s)) {
                    return Model.Facilities.Unknown;
                }
                if (s.ToLower() == "material trader") {

                    return GetMaterialTraderType();
                }
                switch (s.ToLower()) {
                    case "interstellar factors": return Model.Facilities.InterstellarFactors;
                    case "repair": return Model.Facilities.Repair;
                    case "restock": return Model.Facilities.Restock;
                    case "black market": return Model.Facilities.Black_Market;
                    default: return Model.Facilities.Unknown;
                }
                Model.Facilities GetMaterialTraderType() {
                    switch (economy) {
                        case Model.Economy.Refinery:
                        case Model.Economy.Extraction:
                            return Model.Facilities.Trader_Raw;
                        case Model.Economy.Industrial:
                            return Model.Facilities.Trader_Manufactured;
                        case Model.Economy.High_Tech:
                        case Model.Economy.Military:
                            return Model.Facilities.Trader_Encoded;
                        default:
                            return Model.Facilities.Unknown;
                    }
                }
            }

        }

       private static void PrintErrorLog() {
            if (ErrorLog.Count == 0) {
                Logger.Instance.Write("No Errors found when parsing the data.", Logger.MessageType.Info);
            }
            else {
                Logger.Instance.Write($"There were {ErrorLog.Count} Errors made. The according systems and stations have been ignored:", Logger.MessageType.Error);
                foreach (var err in ErrorLog) {
                    Logger.Instance.Write(err, Logger.MessageType.Error);
                }
            }
        }



    }
}

