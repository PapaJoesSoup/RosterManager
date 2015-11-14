using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterManager
{
    internal class KerbalLifeSpan
    {
        // This class stores the Kerbal LifeSpan config node.
        // which includes the following Dictionaries
        // KerbalLifeSpans - all known kerbals in the current save game
        
        public const string configNodeName = "KerbalLifeSpan";
        
        internal Dictionary<string, KerbalLifeInfo> KerbalLifeSpans { get; private set; }
        
        internal KerbalLifeSpan()
        {
            KerbalLifeSpans = new Dictionary<string, KerbalLifeInfo>();            
        }

        internal void Load(ConfigNode node)
        {
            KerbalLifeSpans.Clear();
            
            if (node.HasNode(configNodeName))
            {
                ConfigNode KerbalLifeSpanNode = node.GetNode(configNodeName);
                                
                var kerbalNodes = KerbalLifeSpanNode.GetNodes(KerbalLifeInfo.ConfigNodeName);
                foreach (ConfigNode kerbalNode in kerbalNodes)
                {
                    if (kerbalNode.HasValue("kerbalName"))
                    {
                        string id = kerbalNode.GetValue("kerbalName");
                        Utilities.LogMessage("RosterManagerLifeSpan.KerbalLifeSpan Loading kerbal = " + id, "info", RMSettings.VerboseLogging);                        
                        KerbalLifeInfo kerballifeinfo = KerbalLifeInfo.Load(kerbalNode);
                        KerbalLifeSpans[id] = kerballifeinfo;
                    }
                }                               
            }
            Utilities.LogMessage("RosterManagerLifeSpan.KerbalLifeSpan Loading Completed", "info", RMSettings.VerboseLogging);            
        }

        internal void Save(ConfigNode node)
        {
            ConfigNode kerballifespanNode;
            if (node.HasNode(configNodeName))
            {
                kerballifespanNode = node.GetNode(configNodeName);
            }
            else
            {
                kerballifespanNode = node.AddNode(configNodeName);
            }                       

            foreach (var entry in KerbalLifeSpans)
            {
                ConfigNode kerbalNode = entry.Value.Save(kerballifespanNode);                
                Utilities.LogMessage("RosterManagerLifeSpan.KerbalLifeSpan Saving kerbal = " + entry.Key, "info", RMSettings.VerboseLogging);
                kerbalNode.AddValue("kerbalName", entry.Key);
            }
            Utilities.LogMessage("RosterManagerLifeSpan.KerbalLifeSpan Saving Completed", "info", RMSettings.VerboseLogging);            
        }                
    }

    internal class KerbalLifeInfo
    {
        public const string ConfigNodeName = "KerbalLifeInfo";

        public double lastUpdate = 0f;
        public ProtoCrewMember.RosterStatus status;
        public ProtoCrewMember.KerbalType type;
        public Guid vesselID;
        public string vesselName;
        public uint partID;
        public int seatIdx;
        public string seatName;
        public string experienceTraitName;
        public double age;  //Their current age
        public double lifespan;  //Their lifespan in years
        public double timelastBirthday;  //Game time of their last birthday
        public double timeDFFrozen;  //Game time they were DeepFreeze Frozen

        public KerbalLifeInfo(double currentTime)
        {
            lastUpdate = currentTime;
        }

        public static KerbalLifeInfo Load(ConfigNode node)
        {
            double lastUpdate = GetNodes.GetNodeValue(node, "lastUpdate", 0.0);

            KerbalLifeInfo info = new KerbalLifeInfo(lastUpdate);
            info.status = GetNodes.GetNodeValue(node, "status", ProtoCrewMember.RosterStatus.Available);
            info.type = GetNodes.GetNodeValue(node, "type", ProtoCrewMember.KerbalType.Crew);
            string tmpvesselID = GetNodes.GetNodeValue(node, "vesselID", "");

            try
            {
                info.vesselID = new Guid(tmpvesselID);
            }
            catch (Exception ex)
            {
                info.vesselID = Guid.Empty;
                Utilities.LogMessage("RosterManagerLifeSpan.KerbalLifeInfo error loading vesselID " + ex, "Error", RMSettings.VerboseLogging);                
            }
            info.partID = GetNodes.GetNodeValue(node, "partID", (uint)0);
            info.vesselName = GetNodes.GetNodeValue(node, "VesselName", " ");
            info.seatIdx = GetNodes.GetNodeValue(node, "seatIdx", 0);
            info.seatName = GetNodes.GetNodeValue(node, "seatName", "");
            info.experienceTraitName = GetNodes.GetNodeValue(node, "experienceTraitName", " ");
            info.age = GetNodes.GetNodeValue(node, "age", 25.0d);
            info.lifespan = GetNodes.GetNodeValue(node, "lifespan", 75.0d);
            info.timelastBirthday = GetNodes.GetNodeValue(node, "timelastBirthday", lastUpdate);
            info.timeDFFrozen = GetNodes.GetNodeValue(node, "timeDFFrozen", 0d);

            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("lastUpdate", lastUpdate);
            node.AddValue("status", status);
            node.AddValue("type", type);
            node.AddValue("vesselID", vesselID);
            node.AddValue("VesselName", vesselName);
            node.AddValue("partID", partID);
            node.AddValue("seatIdx", seatIdx);
            node.AddValue("seatName", seatName);
            node.AddValue("experienceTraitName", experienceTraitName);
            node.AddValue("age", age);
            node.AddValue("lifespan", lifespan);
            node.AddValue("timelastBirthday", timelastBirthday);
            node.AddValue("timeDFFrozen", timeDFFrozen);

            return node;
        }        
    }

    internal class GetNodes
    {
        public static int GetNodeValue(ConfigNode confignode, string fieldname, int defaultValue)
        {
            int newValue;
            if (confignode.HasValue(fieldname) && int.TryParse(confignode.GetValue(fieldname), out newValue))
            {
                return newValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public static uint GetNodeValue(ConfigNode confignode, string fieldname, uint defaultValue)
        {
            uint newValue;
            if (confignode.HasValue(fieldname) && uint.TryParse(confignode.GetValue(fieldname), out newValue))
            {
                return newValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public static double GetNodeValue(ConfigNode confignode, string fieldname, double defaultValue)
        {
            double newValue;
            if (confignode.HasValue(fieldname) && double.TryParse(confignode.GetValue(fieldname), out newValue))
            {
                return newValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public static string GetNodeValue(ConfigNode confignode, string fieldname, string defaultValue)
        {
            if (confignode.HasValue(fieldname))
            {
                return confignode.GetValue(fieldname);
            }
            else
            {
                return defaultValue;
            }
        }

        public static Guid GetNodeValue(ConfigNode confignode, string fieldname)
        {
            if (confignode.HasValue(fieldname))
            {
                confignode.GetValue(fieldname);
                return new Guid(fieldname);
            }
            else
            {
                return Guid.Empty;
            }
        }

        public static T GetNodeValue<T>(ConfigNode confignode, string fieldname, T defaultValue) where T : IComparable, IFormattable, IConvertible
        {
            if (confignode.HasValue(fieldname))
            {
                string stringValue = confignode.GetValue(fieldname);
                if (Enum.IsDefined(typeof(T), stringValue))
                {
                    return (T)Enum.Parse(typeof(T), stringValue);
                }
            }
            return defaultValue;
        }
    }
}
