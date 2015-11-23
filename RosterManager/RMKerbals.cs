using DF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RosterManager
{
    internal class RMKerbals
    {
        // This class stores the Roster Manager Kerbal config node.
        // which includes the following Dictionaries:-
        // ALLRMKerbals - all known kerbals in the current save game

        public const string configNodeName = "RMKerbals";

        internal Dictionary<string, RMKerbal> ALLRMKerbals { get; set; }        

        internal RMKerbals()
        {
            ALLRMKerbals = new Dictionary<string, RMKerbal>();
        }

        internal void Load(ConfigNode node)
        {
            ALLRMKerbals.Clear();

            if (node.HasNode(configNodeName))
            {
                ConfigNode KerbalLifeRecordNode = node.GetNode(configNodeName);

                var kerbalNodes = KerbalLifeRecordNode.GetNodes(RMKerbal.ConfigNodeName);
                foreach (ConfigNode kerbalNode in kerbalNodes)
                {
                    if (kerbalNode.HasValue("kerbalName"))
                    {
                        string id = kerbalNode.GetValue("kerbalName");
                        Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Loading kerbal = " + id, "info", RMSettings.VerboseLogging);
                        RMKerbal kerballifeinfo = RMKerbal.Load(kerbalNode, id);
                        ALLRMKerbals[id] = kerballifeinfo;
                    }
                }
            }
            Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Loading Completed", "info", RMSettings.VerboseLogging);
        }

        internal void Save(ConfigNode node)
        {
            ConfigNode kerbalLifeRecordNode;
            if (node.HasNode(configNodeName))
            {
                kerbalLifeRecordNode = node.GetNode(configNodeName);
            }
            else
            {
                kerbalLifeRecordNode = node.AddNode(configNodeName);
            }

            foreach (var entry in ALLRMKerbals)
            {
                ConfigNode kerbalNode = entry.Value.Save(kerbalLifeRecordNode);
                Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Saving kerbal = " + entry.Key, "info", RMSettings.VerboseLogging);
                kerbalNode.AddValue("kerbalName", entry.Key);
            }
            Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Saving Completed", "info", RMSettings.VerboseLogging);
        }
    }

    internal class RMKerbal
    {
        public const string ConfigNodeName = "RMKerbal";

        public double lastUpdate = 0f;
        public ProtoCrewMember.RosterStatus status = ProtoCrewMember.RosterStatus.Available;
        public ProtoCrewMember.KerbalType type = ProtoCrewMember.KerbalType.Crew;
        public Guid vesselID = Guid.Empty;
        public string vesselName = " ";
        public uint partID = 0;  //Probably not required - currently not used.
        public int seatIdx = 0;  //Probably not required - currently not used.
        public string seatName = string.Empty;  //Probably not required - currently not used.
        public double age = 25d;  //Their current age
        public double lifespan = 75d;  //Their lifespan in years
        public double timelastBirthday = 0d;  //Game time of their last birthday
        public double timeDFFrozen = 0d;  //Game time they were DeepFreeze Frozen
        public double salary = 10000d;  //Their Salary
        public bool salaryContractDispute = false; //If their salary is in dispute (ie.Unpaid)
        public bool salaryContractDisputeProcessed = false; //If current dispute period has been processed
        public double payriseRequired = 0d; //Amount of funds required to continue working on contractdispute
        public double owedSalary = 0d; //Backpay they are owed
        public int salaryContractDisputePeriods = 0; //Number of salary periods contract has been in dispute
        public double timelastsalary = 0d; //Game Time they were last paid
        public string notes = string.Empty; //Their notes

        public ProtoCrewMember Kerbal { get; set; }
        public bool IsNew { get; set; }
        public string Name = string.Empty;

        public float Stupidity = 0f;
        public float Courage = 0f;
        public bool Badass = false;
        public string Trait = "Pilot";
        public ProtoCrewMember.Gender Gender = ProtoCrewMember.Gender.Male;
        public int Skill = 0;
        public float Experience = 0f;

        public RMKerbal(double currentTime, ProtoCrewMember kerbal, bool isnew, bool modKerbal)
        {
            lastUpdate = currentTime;
            Kerbal = kerbal;
            IsNew = isnew;
            Name = kerbal.name;
            if (modKerbal)
            {
                Stupidity = kerbal.stupidity;
                Courage = kerbal.courage;
                Badass = kerbal.isBadass;
                Trait = kerbal.trait;
                Gender = kerbal.gender;
                Skill = kerbal.experienceLevel;
                Experience = kerbal.experience;
            }
        }

        public static RMKerbal CreateKerbal()
        {
            ProtoCrewMember kerbal = CrewGenerator.RandomCrewMemberPrototype();
            return new RMKerbal(Planetarium.GetUniversalTime(), kerbal, true, false);
        }

        public string SubmitChanges()
        {
            if (NameExists())
            {
                return "That name is in use!";
            }

            SyncKerbal();

            if (IsNew)
            {
                // Add to roster.
                MethodInfo dynMethod = HighLogic.CurrentGame.CrewRoster.GetType().GetMethod("AddCrewMember", BindingFlags.NonPublic | BindingFlags.Instance);
                Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                dynMethod.Invoke(HighLogic.CurrentGame.CrewRoster, new object[] { Kerbal });
            }

            return string.Empty;
        }

        public void SyncKerbal()
        {
            if (RMSettings.EnableKerbalRename)
                Kerbal.name = Name;
            // remove old save game hack for backwards compatability...
            Kerbal.name = Kerbal.name.Replace(char.ConvertFromUtf32(1), "");
            // New trait management is easy!
            Kerbal.trait = Trait;
            Kerbal.gender = Gender;
            Kerbal.stupidity = Stupidity;
            Kerbal.courage = Courage;
            Kerbal.isBadass = Badass;
            Kerbal.experienceLevel = Skill;
            Kerbal.experience = Experience;
        }

        private bool NameExists()
        {
            if (IsNew || Kerbal.name != Name)
            {
                return HighLogic.CurrentGame.CrewRoster.Exists(Name);
            }

            return false;
        }

        public static RMKerbal Load(ConfigNode node, string name)
        {
            try
            {
                double lastUpdate = GetNodes.GetNodeValue(node, "lastUpdate", 0d);
                List<ProtoCrewMember> CrewList = new List<ProtoCrewMember>();
                CrewList = HighLogic.CurrentGame.CrewRoster.Crew.Concat(HighLogic.CurrentGame.CrewRoster.Applicants).ToList();
                //If Deepfreeze is installed add Unowned and Tourists to the list (could be frozen or comatose).
                if (DFInterface.IsDFInstalled)
                {
                    CrewList = CrewList.Concat(HighLogic.CurrentGame.CrewRoster.Unowned).Concat(HighLogic.CurrentGame.CrewRoster.Tourist).ToList();
                }
                ProtoCrewMember kerbal = CrewList.FirstOrDefault(a => a.name == name);
                RMKerbal info = new RMKerbal(lastUpdate, kerbal, false, false);
                info.status = GetNodes.GetNodeValue(node, "status", ProtoCrewMember.RosterStatus.Available);
                info.type = GetNodes.GetNodeValue(node, "type", ProtoCrewMember.KerbalType.Crew);
                string tmpvesselID = GetNodes.GetNodeValue(node, "vesselID", "");
                info.vesselID = GetNodes.GetNodeValue(node, "vesselID", Guid.Empty);
                info.partID = GetNodes.GetNodeValue(node, "partID", (uint)0);
                info.vesselName = GetNodes.GetNodeValue(node, "VesselName", " ");
                info.seatIdx = GetNodes.GetNodeValue(node, "seatIdx", 0);
                info.seatName = GetNodes.GetNodeValue(node, "seatName", "");
                info.age = GetNodes.GetNodeValue(node, "age", 25.0d);
                info.lifespan = GetNodes.GetNodeValue(node, "lifespan", 75.0d);
                info.timelastBirthday = GetNodes.GetNodeValue(node, "timelastBirthday", lastUpdate);
                info.timeDFFrozen = GetNodes.GetNodeValue(node, "timeDFFrozen", 0d);
                info.salary = GetNodes.GetNodeValue(node, "salary", 0d);
                info.timelastsalary = GetNodes.GetNodeValue(node, "timelastsalary", lastUpdate);
                info.salaryContractDispute = GetNodes.GetNodeValue(node, "salaryContractDispute", false);
                info.owedSalary = GetNodes.GetNodeValue(node, "owedSalary", 0d);
                info.salaryContractDisputePeriods = GetNodes.GetNodeValue(node, "salaryContractDisputePeriods", 0);
                info.salaryContractDisputeProcessed = GetNodes.GetNodeValue(node, "salaryContractDisputeProcessed", false);
                info.payriseRequired = GetNodes.GetNodeValue(node, "payriseRequired", 0d);
                info.Stupidity = GetNodes.GetNodeValue(node, "Stupidity", 0f);
                info.Courage = GetNodes.GetNodeValue(node, "Courage", 0f);
                info.Badass = GetNodes.GetNodeValue(node, "Badass", false);
                info.Trait = GetNodes.GetNodeValue(node, "Trait", "Pilot");
                info.Gender = GetNodes.GetNodeValue(node, "Gender", ProtoCrewMember.Gender.Male);
                info.Skill = GetNodes.GetNodeValue(node, "Skill", 0);
                info.Experience = GetNodes.GetNodeValue(node, "Experience", 0f);
                return info;
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.RMKerbal Load error... " + ex, "Error", RMSettings.VerboseLogging);
                return null;
            }
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("lastUpdate", lastUpdate);
            node.AddValue("Name", Name);
            node.AddValue("status", status);
            node.AddValue("type", type);
            node.AddValue("vesselID", vesselID);
            node.AddValue("VesselName", vesselName);
            node.AddValue("partID", partID);
            node.AddValue("seatIdx", seatIdx);
            node.AddValue("seatName", seatName);
            node.AddValue("age", age);
            node.AddValue("lifespan", lifespan);
            node.AddValue("timelastBirthday", timelastBirthday);
            node.AddValue("timeDFFrozen", timeDFFrozen);
            node.AddValue("salary", salary);
            node.AddValue("timelastsalary", timelastsalary);
            node.AddValue("notes", notes);
            node.AddValue("salaryContractDispute", salaryContractDispute);
            node.AddValue("owedSalary", owedSalary);
            node.AddValue("salaryContractDisputePeriods", salaryContractDisputePeriods);
            node.AddValue("salaryContractDisputeProcessed", salaryContractDisputeProcessed);
            node.AddValue("payriseRequired", payriseRequired);
            node.AddValue("Stupidity", Stupidity);
            node.AddValue("Courage", Courage);
            node.AddValue("Badass", Badass);
            node.AddValue("Trait", Trait);
            node.AddValue("Gender", Gender);
            node.AddValue("Skill", Skill);
            node.AddValue("Experience", Experience);

            return node;
        }
    }

    internal class GetNodes
    {
        internal static bool GetNodeValue(ConfigNode confignode, string fieldname, bool defaultValue)
        {
            bool newValue;
            if (confignode.HasValue(fieldname) && bool.TryParse(confignode.GetValue(fieldname), out newValue))
            {
                return newValue;
            }
            else
            {
                return defaultValue;
            }
        }

        internal static int GetNodeValue(ConfigNode confignode, string fieldname, int defaultValue)
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

        internal static uint GetNodeValue(ConfigNode confignode, string fieldname, uint defaultValue)
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

        internal static float GetNodeValue(ConfigNode confignode, string fieldname, float defaultValue)
        {
            float newValue;
            if (confignode.HasValue(fieldname) && float.TryParse(confignode.GetValue(fieldname), out newValue))
            {
                return newValue;
            }
            else
            {
                return defaultValue;
            }
        }

        internal static double GetNodeValue(ConfigNode confignode, string fieldname, double defaultValue)
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

        internal static string GetNodeValue(ConfigNode confignode, string fieldname, string defaultValue)
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

        internal static Guid GetNodeValue(ConfigNode confignode, string fieldname, Guid guid)
        {
            if (confignode.HasValue(fieldname))
            {
                try
                {
                    Guid id = new Guid(confignode.GetValue(fieldname));
                    return id;
                }
                catch (Exception ex)
                {
                    Utilities.LogMessage("RosterManagerLifeSpan.RMKerbal error loading vesselID " + ex, "Error", RMSettings.VerboseLogging);
                    return Guid.Empty;
                }
            }
            else
            {
                return Guid.Empty;
            }
        }

        internal static T GetNodeValue<T>(ConfigNode confignode, string fieldname, T defaultValue) where T : IComparable, IFormattable, IConvertible
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