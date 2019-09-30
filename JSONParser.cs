﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orcabot.Types;
using Orcabot.Helpers;
using Orcabot.Types.Enums;
using System.Linq;


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
        public static Dictionary<string, Orcabot.Types.System> Parse(string stationsFile, string populatedFile) {
            Dictionary<string, Orcabot.Types.System> systemDict = new Dictionary<string, Orcabot.Types.System>();

            {
                var result = GetJsonListFromFile<JSONModel.System>(populatedFile);
                //Now create the proper dictionary entries out of the result, witout the stations for now, that is
                foreach (var system in result) {
                    var sys = ParseSystem(system);
                    if (sys == null) {
                        continue;
                    }
                    systemDict[system.name.ToUpper()] = sys;
                    Logger.Instance.Write("Parsed system " + system.name, Logger.MessageType.Verbose);
                }
                result.Clear();
                Logger.Instance.Write("Finished parsing Systems", Logger.MessageType.Info);
            }
            GC.Collect();
            {
                var result = GetJsonListFromFile<JSONModel.Station>(stationsFile);
                foreach(var station in result) {
                    var stat = ParseStation(station);
                    if(stat == null) {
                        continue;
                    }
                    if (!systemDict.ContainsKey(station.systemName.ToUpper())) {
                        ErrorLog.Add($"Could not add station {station.name} because the system name ({station.systemName}) could not be found in the set up dictionary");
                        continue;
                    }
                    systemDict[station.systemName.ToUpper()].Stations.Add(stat);
                    Logger.Instance.Write($"Parsed station {station.name} from the {station.systemName} system.", Logger.MessageType.Verbose);
                }

            }
            PrintErrorLog();
            return systemDict;
            
        }
        public static string Stringify<T>(Dictionary<string,T> dict) {
            return JsonConvert.SerializeObject(dict, Formatting.None);
        }
       
        private static Orcabot.Types.System ParseSystem(JSONModel.System sys) {
            if(sys.name == null || sys.x == null|| sys.y == null || sys.z == null) {
                ErrorLog.Add(("Could not add ") + ((sys.name == null) ? "a" : ($"the {sys.name}") + " system."));
                return null;
            }
            return new Orcabot.Types.System() {
                Name = sys.name,
                Coordinate = new Coordinate {
                    X = sys.x,
                    Y = sys.y,
                    Z = sys.z
                },
                
                SystemSecurity = GetSecurityFromString(sys.security)
            };
        
        }
        private static Station ParseStation(JSONModel.Station station) {
            if(station.name == null || station.distanceToArrival == null || station.economy == null) {
                var stationName = (station.name == null) ? "a station" : $"the {station.name} station";
                var system = (station.systemName == null) ? "a system" : $"the {station.systemName} system";
                ErrorLog.Add($"Could not add {stationName} in {system}");
                return null;
            }
            var economy = GetEconomy(station.economy);
            var type = GetStationType(station.type);
            var facilities = GetFacilities(station.otherServices, economy, station.haveShipyard, station.haveOutfitting);


            return new Station() {
                Name = station.name,
                Economy = economy,
                
                Type = type,
                
                StationFacilities = facilities,
              
                Distance = (float)station.distanceToArrival
            };
        }

        private static PadSize GetLandingPadSize(Station st) {
            switch (st.Type) {
                case StationType.AsteroidBase:
                case StationType.Coriolis:
                case StationType.MegaShip:
                case StationType.Ocellus:
                case StationType.Orbis:
                case StationType.SurfaceStation:
                   return PadSize.Large; 
                case StationType.Outpost:
                    return PadSize.Medium; 
                default:
                   return PadSize.None; 
            }
        }

        private static Security GetSecurityFromString(string securityString) {
            if (securityString == null) {
                return Security.Unknown;
            }
            else {
                switch (securityString.ToLower()) {
                    case "high":
                        return Security.High;
                    case "medium":
                        return Security.Medium;
                    case "low":
                        return Security.Low; 
                    case "anarchy":
                        return Security.Anarchy;
                    default:
                        return Security.Unknown; 
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="facilities">Array of facilities as strings</param>
        /// <param name="economy">In case it is a Material Trader, the Economy is needed to identify the type. If none is given, Model.Facilities.Unknown will be returned</param>
        /// <returns></returns>
        static List<StationFacility> GetFacilities(string[] facilities, Economy economy = Economy.Unknown, bool hasShipyard = false, bool hasOutfitting = false) {
            List<StationFacility> returnList = new List<StationFacility>();
            foreach (var f in facilities) {
                var facility = GetFacility(f);
                if (facility != StationFacility.Unknown) {
                    returnList.Add(facility);
                }
            }
            if (hasShipyard) {
                returnList.Add(StationFacility.Shipyard);
            }
            if (hasOutfitting) {
                returnList.Add(StationFacility.Outfitting);
            }


            return returnList.Distinct().ToList();




            StationFacility GetFacility(string s) {
                if (string.IsNullOrEmpty(s)) {
                    return StationFacility.Unknown;
                }
                if (s.ToLower() == "material trader") {

                    return GetMaterialTraderType();
                }
                switch (s.ToLower()) {
                    case "interstellar factors": return StationFacility.InterstellarFactors;
                    case "repair": return StationFacility.Repair;
                    case "restock": return StationFacility.Restock;
                    case "black market": return StationFacility.BlackMarket;
                    default: return StationFacility.Unknown;
                }
                StationFacility GetMaterialTraderType() {
                    switch (economy) {
                        case Economy.Refinery:
                        case Economy.Extraction:
                            return StationFacility.TraderRaw;
                        case Economy.Industrial:
                            return StationFacility.TraderManufactured;
                        case Economy.HighTech:
                        case Economy.Military:
                            return StationFacility.TraderEncoded;
                        default:
                            return StationFacility.Unknown;
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
        private static Economy GetEconomy(string s) {
            if (string.IsNullOrEmpty(s)) {
                return Economy.Unknown; 
            }
            switch (s.ToLower()) {
                case "extraction": return Economy.Extraction;
                case "refinery": return Economy.Refinery;
                case "industrial": return Economy.Industrial;
                case "high tech": return Economy.HighTech;
                case "agriculture": return Economy.Agriculture;
                case "terraforming": return Economy.Terraforming;
                case "tourism": return Economy.Tourism;
                case "service": return Economy.Service;
                case "military": return Economy.Military;
                case "colony": return Economy.Colony;
                case "rescue": return Economy.Rescue;
                case "damaged": return Economy.Damaged;
                case "repair": return Economy.Repair;
                default: return Economy.Unknown;
            }
        }
        private static StationType GetStationType(string s) {
            if (string.IsNullOrEmpty(s)) {
                return StationType.Unknown;
            }
            switch (s.ToLower()) {
                case "planetary outpost": return StationType.SurfaceStation;
                case "orbis starport": return StationType.Orbis;
                case "ocellus starport": return StationType.Ocellus;
                case "coriolis starport": return StationType.Coriolis;
                case "mega ship": return StationType.MegaShip;
                case "outpost": return StationType.Outpost;
                case "asteroid base": return StationType.AsteroidBase;
                default: return StationType.Unknown;
            }
        }

        private static IList<T> GetJsonListFromFile<T>(string path) {
            using (StreamReader sr = File.OpenText(path)) {
                using (JsonReader jtr = new JsonTextReader(sr)) {
                    JsonSerializerSettings jss = new JsonSerializerSettings {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    JsonSerializer js = JsonSerializer.Create(jss);
                    return js.Deserialize<List<T>>(jtr);
                  
                }
            }
        }





    }
}

