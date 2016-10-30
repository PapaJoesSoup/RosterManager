using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RosterManager.Windows;
using UnityEngine;
using Random = System.Random;

namespace RosterManager
{
  internal class RMKerbals
  {
    // This class stores the Roster Manager Kerbal config node.
    // which includes the following Dictionaries:-
    // ALLRMKerbals - all known kerbals in the current save game

    public const string ConfigNodeName = "RMKerbals";

    internal Dictionary<string, RMKerbal> AllrmKerbals { get; set; }

    internal RMKerbals()
    {
      AllrmKerbals = new Dictionary<string, RMKerbal>();
    }

    internal void Load(ConfigNode node)
    {
      AllrmKerbals.Clear();

      if (node.HasNode(ConfigNodeName))
      {
        var kerbalLifeRecordNode = node.GetNode(ConfigNodeName);

        var kerbalNodes = kerbalLifeRecordNode.GetNodes(RMKerbal.ConfigNodeName);
        foreach (var kerbalNode in kerbalNodes)
        {
          if (kerbalNode.HasValue("kerbalName"))
          {
            var id = kerbalNode.GetValue("kerbalName");
            Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Loading kerbal = " + id, "info", RMSettings.VerboseLogging);
            var kerballifeinfo = RMKerbal.Load(kerbalNode, id);
            AllrmKerbals[id] = kerballifeinfo;
          }
        }
      }
      Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Loading Completed", "info", RMSettings.VerboseLogging);
    }

    internal void Save(ConfigNode node)
    {
      try
      {
                var kerbalLifeRecordNode = node.HasNode(ConfigNodeName) ? node.GetNode(ConfigNodeName) : node.AddNode(ConfigNodeName);

                foreach (var entry in AllrmKerbals)
                {
                    var kerbalNode = entry.Value.Save(kerbalLifeRecordNode);
                    Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Saving kerbal = " + entry.Key, "info", RMSettings.VerboseLogging);
                    kerbalNode.AddValue("kerbalName", entry.Key);
                }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("RosterManagerLifeSpan.RMKerbal Save error... " + ex, "Error", RMSettings.VerboseLogging);                
      }
       
      Utilities.LogMessage("RosterManagerLifeSpan.RMKerbals Saving Completed", "info", RMSettings.VerboseLogging);
    }
  }

  internal class RMKerbal
  {
    public const string ConfigNodeName = "RMKerbal";

    public double LastUpdate;
    public ProtoCrewMember.RosterStatus Status = ProtoCrewMember.RosterStatus.Available;
    public ProtoCrewMember.KerbalType Type = ProtoCrewMember.KerbalType.Crew;
    public Guid VesselId = Guid.Empty;
    public string VesselName = " ";
    public uint PartId;  //Probably not required - currently not used.
    public int SeatIdx;  //Probably not required - currently not used.
    public string SeatName = string.Empty;  //Probably not required - currently not used.
    public double Age = 25d;  //Their current age
    public double Lifespan = 75d;  //Their lifespan in years
    public double TimelastBirthday;  //Game time of their last birthday
    public double TimeNextBirthday; //Game time of their next birthday
    public double TimeDfFrozen;  //Game time they were DeepFreeze Frozen
    public double Salary = 10000d;  //Their Salary
    public bool SalaryContractDispute; //If their salary is in dispute (ie.Unpaid)
    public bool SalaryContractDisputeProcessed; //If current dispute period has been processed
    public double PayriseRequired; //Amount of funds required to continue working on contractdispute
    public double OwedSalary; //Backpay they are owed
    public int SalaryContractDisputePeriods; //Number of salary periods contract has been in dispute
    public double Timelastsalary; //Game Time they were last paid
    public double TimeSalaryDue; // Game Time salary is next due
    public string Notes = string.Empty; //Their notes

    public ProtoCrewMember Kerbal { get; set; }
    public bool IsNew { get; set; }
    public string Name;

    public float Stupidity;
    public float Courage;
    public bool Badass;
    public string Trait = "Pilot";
    public string RealTrait = "Pilot";
    public ProtoCrewMember.Gender Gender = ProtoCrewMember.Gender.Male;
    public int Skill;
    public float Experience;

    private readonly Random _rnd = new Random();  // Random seed for setting Kerbals ages
    internal static KSPUtil.DefaultDateTimeFormatter defaultDateTime = new KSPUtil.DefaultDateTimeFormatter();

    public RMKerbal(double currentTime, ProtoCrewMember kerbal, bool isnew, bool modKerbal)
    {
      LastUpdate = currentTime;
      Kerbal = kerbal;
      IsNew = isnew;
      Name = kerbal.name;
      if (isnew)
      {
        Trait = kerbal.trait;
        Type = kerbal.type;
        Status = kerbal.rosterStatus;
        VesselId = Guid.Empty;
        VesselName = string.Empty;
        double diceMinage = _rnd.Next(RMLifeSpan.Instance.RMGameSettings.MinimumAge - 3, RMLifeSpan.Instance.RMGameSettings.MinimumAge + 3); // Randomly set their age.
        Age = diceMinage;
        double diceMaxage = _rnd.Next(RMLifeSpan.Instance.RMGameSettings.MaximumAge - 5, RMLifeSpan.Instance.RMGameSettings.MaximumAge + 5); // Randomly set their age.
        Lifespan = diceMaxage;
        TimelastBirthday = currentTime;
        TimeNextBirthday = BirthdayNextDue(currentTime);
        Timelastsalary = currentTime;
        TimeSalaryDue = SalaryNextDue(currentTime);
        Salary = RMLifeSpan.Instance.RMGameSettings.DefaultSalary;
        if (Api.InstalledMods.IsDfInstalled)
        {
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type == ProtoCrewMember.KerbalType.Unowned)  // if they are frozen store time frozen
          {
            if (RMAddon.FrozenKerbals.ContainsKey(kerbal.name))
            {
              TimeDfFrozen = RMAddon.FrozenKerbals[kerbal.name].lastUpdate;
            }
          }
        }
        Name = kerbal.name;
        Stupidity = kerbal.stupidity;
        Courage = kerbal.courage;
        Badass = kerbal.isBadass;
        Gender = kerbal.gender;
        Skill = kerbal.experienceLevel;
        Experience = kerbal.experience;
        Kerbal = kerbal;
      }
      if (modKerbal)
      {
        Stupidity = kerbal.stupidity;
        Courage = kerbal.courage;
        Badass = kerbal.isBadass;
        if (SalaryContractDispute)
        {
          RealTrait = kerbal.trait;
          Trait = "Tourist";
          KerbalRoster.SetExperienceTrait(kerbal, Trait);
        }
        else
        {
          if (Status == ProtoCrewMember.RosterStatus.Assigned)
          {
            UnregisterExperienceTrait(this);
          }
          Trait = kerbal.trait;
          RealTrait = kerbal.trait;
          KerbalRoster.SetExperienceTrait(kerbal, Trait);
          if (Status == ProtoCrewMember.RosterStatus.Assigned)
          {
            RegisterExperienceTrait(this);
          }
        }
        Gender = kerbal.gender;
        Skill = kerbal.experienceLevel;
        Experience = kerbal.experience;
      }
    }

    public static RMKerbal CreateKerbal()
    {
      var kerbal = CrewGenerator.RandomCrewMemberPrototype();
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
        var dynMethod = HighLogic.CurrentGame.CrewRoster.GetType().GetMethod("AddCrewMember", BindingFlags.NonPublic | BindingFlags.Instance);
        Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
        dynMethod.Invoke(HighLogic.CurrentGame.CrewRoster, new object[] { Kerbal });
      }

      return string.Empty;
    }

    public void SyncKerbal()
    {
        if (RMSettings.EnableKerbalRename)
            Kerbal.ChangeName(Name);
      // remove old save game hack for backwards compatability...
      //Kerbal.name = Kerbal.name.Replace(char.ConvertFromUtf32(1), "");
      if (!SalaryContractDispute)
      {
        if (Status == ProtoCrewMember.RosterStatus.Assigned)
        {
          UnregisterExperienceTrait(this);
        }
        RealTrait = Trait;
        Kerbal.trait = Trait;
        KerbalRoster.SetExperienceTrait(Kerbal, Trait);
        if (Status == ProtoCrewMember.RosterStatus.Assigned)
        {
          RegisterExperienceTrait(this);
        }
      }
      Kerbal.gender = Gender;
      Kerbal.stupidity = Stupidity;
      Kerbal.courage = Courage;
      Kerbal.isBadass = Badass;
      Kerbal.experienceLevel = Skill;
      Kerbal.experience = Experience;
      Kerbal.type = Type;
      Kerbal.rosterStatus = Status;

      // Now let's do some validaton for Type and status
      if (Type == ProtoCrewMember.KerbalType.Tourist && Trait != "Tourist")
      {
        Kerbal.trait = "Tourist";
      }
      if (Type != ProtoCrewMember.KerbalType.Tourist && Trait == "Tourist")
      {
        Kerbal.type = ProtoCrewMember.KerbalType.Tourist;
      }
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
        var lastUpdate = GetNodes.GetNodeValue(node, "lastUpdate", 0d);
        var crewList = HighLogic.CurrentGame.CrewRoster.Crew.Concat(HighLogic.CurrentGame.CrewRoster.Applicants).ToList();
        //If Deepfreeze is installed add Unowned and Tourists to the list (could be frozen or comatose).
        if (Api.InstalledMods.IsDfInstalled)
        {
          crewList = crewList.Concat(HighLogic.CurrentGame.CrewRoster.Unowned).Concat(HighLogic.CurrentGame.CrewRoster.Tourist).ToList();
        }
        //var tmpvesselId = GetNodes.GetNodeValue(node, "vesselID", "");
        var kerbal = crewList.FirstOrDefault(a => a.name == name);
        var info = new RMKerbal(lastUpdate, kerbal, false, false)
        {
          Status = GetNodes.GetNodeValue(node, "status", ProtoCrewMember.RosterStatus.Available),
          Type = GetNodes.GetNodeValue(node, "type", ProtoCrewMember.KerbalType.Crew),
          VesselId = GetNodes.GetNodeValue(node, "vesselID", Guid.Empty),
          PartId = GetNodes.GetNodeValue(node, "partID", (uint)0),
          VesselName = GetNodes.GetNodeValue(node, "VesselName", " "),
          SeatIdx = GetNodes.GetNodeValue(node, "seatIdx", 0),
          SeatName = GetNodes.GetNodeValue(node, "seatName", ""),
          Age = GetNodes.GetNodeValue(node, "age", 25.0d),
          Lifespan = GetNodes.GetNodeValue(node, "lifespan", 75.0d),
          TimelastBirthday = GetNodes.GetNodeValue(node, "timelastBirthday", lastUpdate),
          TimeDfFrozen = GetNodes.GetNodeValue(node, "timeDFFrozen", 0d),
          Salary = GetNodes.GetNodeValue(node, "salary", 0d),
          TimeSalaryDue = GetNodes.GetNodeValue(node, "timeSalaryDue", lastUpdate),
          Timelastsalary = GetNodes.GetNodeValue(node, "timelastsalary", lastUpdate),
          Notes = GetNodes.GetNodeValue(node, "notes", ""),
          SalaryContractDispute = GetNodes.GetNodeValue(node, "salaryContractDispute", false),
          OwedSalary = GetNodes.GetNodeValue(node, "owedSalary", 0d),
          SalaryContractDisputePeriods = GetNodes.GetNodeValue(node, "salaryContractDisputePeriods", 0),
          SalaryContractDisputeProcessed = GetNodes.GetNodeValue(node, "salaryContractDisputeProcessed", false),
          PayriseRequired = GetNodes.GetNodeValue(node, "payriseRequired", 0d),
          Stupidity = GetNodes.GetNodeValue(node, "Stupidity", 0f),
          Courage = GetNodes.GetNodeValue(node, "Courage", 0f),
          Badass = GetNodes.GetNodeValue(node, "Badass", false),
          Trait = GetNodes.GetNodeValue(node, "Trait", "Pilot"),
          RealTrait = GetNodes.GetNodeValue(node, "RealTrait", "Pilot"),
          Gender = GetNodes.GetNodeValue(node, "Gender", ProtoCrewMember.Gender.Male),
          Skill = GetNodes.GetNodeValue(node, "Skill", 0),
          Experience = GetNodes.GetNodeValue(node, "Experience", 0f)
        };
        info.TimeNextBirthday = GetNodes.GetNodeValue(node, "timeNextBirthday", BirthdayNextDue(info.TimelastBirthday));
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
      var node = config.AddNode(ConfigNodeName);
      node.AddValue("lastUpdate", LastUpdate);
      node.AddValue("Name", Name);
      node.AddValue("status", Status);
      node.AddValue("type", Type);
      node.AddValue("vesselID", VesselId);
      node.AddValue("VesselName", VesselName);
      node.AddValue("partID", PartId);
      node.AddValue("seatIdx", SeatIdx);
      node.AddValue("seatName", SeatName);
      node.AddValue("age", Age);
      node.AddValue("lifespan", Lifespan);
      node.AddValue("timelastBirthday", TimelastBirthday);
      node.AddValue("timeNextBirthday", TimeNextBirthday);
      node.AddValue("timeDFFrozen", TimeDfFrozen);
      node.AddValue("salary", Salary);
      node.AddValue("timeSalaryDue", TimeSalaryDue);
      node.AddValue("timelastsalary", Timelastsalary);
      node.AddValue("notes", Notes);
      node.AddValue("salaryContractDispute", SalaryContractDispute);
      node.AddValue("owedSalary", OwedSalary);
      node.AddValue("salaryContractDisputePeriods", SalaryContractDisputePeriods);
      node.AddValue("salaryContractDisputeProcessed", SalaryContractDisputeProcessed);
      node.AddValue("payriseRequired", PayriseRequired);
      node.AddValue("Stupidity", Stupidity);
      node.AddValue("Courage", Courage);
      node.AddValue("Badass", Badass);
      node.AddValue("Trait", Trait);
      node.AddValue("RealTrait", RealTrait);
      node.AddValue("Gender", Gender);
      node.AddValue("Skill", Skill);
      node.AddValue("Experience", Experience);

      return node;
    }

    internal static void UnregisterExperienceTrait(RMKerbal rmkerbal)
    {
      var vsl = FlightGlobals.Vessels.FirstOrDefault(a => a.id == rmkerbal.VesselId);
      if (vsl == null) return;
      if (!vsl.loaded) return;
      foreach (var part in vsl.parts)
      {
        var found = false;
        if (part.protoModuleCrew.Any(partcrew => partcrew == rmkerbal.Kerbal))
        {
          rmkerbal.Kerbal.UnregisterExperienceTraits(part);
          found = true;
        }
        if (found)
          break;
      }
    }

    internal static void RegisterExperienceTrait(RMKerbal rmkerbal)
    {
      var vsl = FlightGlobals.Vessels.FirstOrDefault(a => a.id == rmkerbal.VesselId);
      if (vsl == null) return;
      if (!vsl.loaded) return;
      foreach (var part in vsl.parts)
      {
        var found = false;
        if (part.protoModuleCrew.Any(partcrew => partcrew == rmkerbal.Kerbal))
        {
          rmkerbal.Kerbal.RegisterExperienceTraits(part);
          found = true;
        }
        if (found)
          break;
      }
    }

    public static double SalaryNextDue(double time)
    {
      var salaryTimeYear = defaultDateTime.Year;
      var salaryTimeMonth = defaultDateTime.Year / 12;
      double salaryTimeSpan = salaryTimeMonth;

      if (RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly)
        salaryTimeSpan = salaryTimeYear; 
      
      var returnTime = time + salaryTimeSpan;
      return returnTime;
    }

    public static double BirthdayNextDue(double time)
    {
      double birthdayTimeSpan = defaultDateTime.Year; 
      return time + birthdayTimeSpan;
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
      return defaultValue;
    }

    internal static int GetNodeValue(ConfigNode confignode, string fieldname, int defaultValue)
    {
      int newValue;
      if (confignode.HasValue(fieldname) && int.TryParse(confignode.GetValue(fieldname), out newValue))
      {
        return newValue;
      }
      return defaultValue;
    }

    internal static uint GetNodeValue(ConfigNode confignode, string fieldname, uint defaultValue)
    {
      uint newValue;
      if (confignode.HasValue(fieldname) && uint.TryParse(confignode.GetValue(fieldname), out newValue))
      {
        return newValue;
      }
      return defaultValue;
    }

    internal static float GetNodeValue(ConfigNode confignode, string fieldname, float defaultValue)
    {
      float newValue;
      if (confignode.HasValue(fieldname) && float.TryParse(confignode.GetValue(fieldname), out newValue))
      {
        return newValue;
      }
      return defaultValue;
    }

    internal static double GetNodeValue(ConfigNode confignode, string fieldname, double defaultValue)
    {
      double newValue;
      if (confignode.HasValue(fieldname) && double.TryParse(confignode.GetValue(fieldname), out newValue))
      {
        return newValue;
      }
      return defaultValue;
    }

    internal static string GetNodeValue(ConfigNode confignode, string fieldname, string defaultValue)
    {
      if (confignode.HasValue(fieldname))
      {
        return confignode.GetValue(fieldname);
      }
      return defaultValue;
    }

    internal static Guid GetNodeValue(ConfigNode confignode, string fieldname, Guid guid)
    {
      if (confignode.HasValue(fieldname))
      {
        try
        {
          var id = new Guid(confignode.GetValue(fieldname));
          return id;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage("RosterManagerLifeSpan.RMKerbal error loading vesselID " + ex, "Error", RMSettings.VerboseLogging);
          return Guid.Empty;
        }
      }
      return Guid.Empty;
    }

    internal static T GetNodeValue<T>(ConfigNode confignode, string fieldname, T defaultValue) where T : IComparable, IFormattable, IConvertible
    {
      if (confignode.HasValue(fieldname))
      {
        var stringValue = confignode.GetValue(fieldname);
        if (Enum.IsDefined(typeof(T), stringValue))
        {
          return (T)Enum.Parse(typeof(T), stringValue);
        }
      }
      return defaultValue;
    }
  }
}