using DF;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosterManager
{
  internal class RMLifeSpanAddon : MonoBehaviour
  {
    // This class maintains the Roster Manager RMKerbals instance of all known kerbals when a kerbal is created or removed from the game.
    // Which includes:-
    // Maintaining vessel data for each kerbal.
    // Maintain and process Age details for each kerbal, including when they Die.
    // Maintain and process Salary details for each kerbal including paying them, and handling contract dispute processing.
    private static RMLifeSpanAddon _instance;

    public static double Tolerance = 0.000001;

    public static bool IsActive
    {
      get
      {
        return _instance != null;
      }
    }

    public static RMLifeSpanAddon Instance
    {
      get
      {
        return _instance;
      }
    }

    private readonly System.Random _gen = new System.Random();  // Random seed for deciding when a kerbal dies of old age. Do we need two seeds?

    protected RMLifeSpanAddon()
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon.Constructor Active...", "info", RMSettings.VerboseLogging);
      _instance = this;
    }

    public void Awake()
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon Awake...", "info", RMSettings.VerboseLogging);
      GameEvents.onKerbalAdded.Add(OnKerbalAdded);
      GameEvents.onKerbalRemoved.Add(OnKerbalRemoved);
      GameEvents.OnCrewmemberSacked.Add(OnKerbalSacked);
    }

    public void Start()
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon Startup...", "info", RMSettings.VerboseLogging);
      try
      {
        CheckDatabase();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  RosterManagerLifeSpanAddon.Start.  " + ex, "Error", true);
      }
    }

    public void Update()
    {
      try
      {
        if (HighLogic.LoadedScene == GameScenes.FLIGHT)
        {
          //Update vesselID and Name for crew in the RM confignode dictionary. We aren't actually using this now, but will come into use/handy later.
          //This is necessary because SP doesn't actually store assigned vessels to crew in game neatly. They are stored ito the vessel modules, not the kerbal or crewroster,
          // which would make things easier!! DEVS take note!!!
          if (FlightGlobals.ActiveVessel != null)
          {
            foreach (var crew in FlightGlobals.ActiveVessel.GetVesselCrew().ToList())
            {
              var kerbal = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.FirstOrDefault(a => a.Key == crew.name);
              if (kerbal.Key != null)
              {
                kerbal.Value.VesselId = FlightGlobals.ActiveVessel.id;
                kerbal.Value.VesselName = FlightGlobals.ActiveVessel.vesselName;
              }
            }
          }
        }
        //Update all known Crew, Applicants in any game scene.
         var crewList = HighLogic.CurrentGame.CrewRoster.Crew.Concat(HighLogic.CurrentGame.CrewRoster.Applicants).ToList();
        //If Deepfreeze is installed add Unowned and Tourists to the list (could be frozen or comatose).
        if (Api.InstalledMods.IsDfInstalled)
        {
          crewList = crewList.Concat(HighLogic.CurrentGame.CrewRoster.Unowned).Concat(HighLogic.CurrentGame.CrewRoster.Tourist).ToList();
        }
        foreach (var crew in crewList)
        {
          // If they are not Dead or they are Dead status and they are unowned (frozen) or tourist (comatose) - We update their Life stats.
          if (crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead
              || (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && (crew.type == ProtoCrewMember.KerbalType.Unowned || crew.type == ProtoCrewMember.KerbalType.Tourist)))
          {
            UpdateKerbal(crew, true);
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  RosterManagerLifeSpanAddon.Update.  " + ex, "Error", true);
      }
    }

    public void OnDestroy()
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon OnDestroy...", "info", RMSettings.VerboseLogging);
      GameEvents.onKerbalAdded.Remove(OnKerbalAdded);
      GameEvents.onKerbalRemoved.Remove(OnKerbalRemoved);
      GameEvents.OnCrewmemberSacked.Remove(OnKerbalSacked);
    }

    private void OnKerbalAdded(ProtoCrewMember crew)
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon.onKerbalAdded " + crew.name + " has been added to the crew roster.", "info", RMSettings.VerboseLogging);
      UpdateKerbal(crew, true);
    }

    private void OnKerbalRemoved(ProtoCrewMember crew)
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon.onKerbalRemoved " + crew.name + " has been removed from the crew roster.", "info", RMSettings.VerboseLogging);
      RemoveKerbal(crew);
    }

    private void OnKerbalSacked(ProtoCrewMember crew, int num)
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon.onKerbalSacked " + crew.name + " has been sacked from the crew roster.", "info", RMSettings.VerboseLogging);
      RemoveKerbal(crew);
    }

    private void CheckDatabase()
    {
      // Check the roster list of crew and applicants for KerbalLife settings.
      var crewkerbals =
        HighLogic.CurrentGame.CrewRoster.Crew.Concat(HighLogic.CurrentGame.CrewRoster.Applicants)
          .Concat(HighLogic.CurrentGame.CrewRoster.Tourist)
          .ToList();
      foreach (
        var crew in
          crewkerbals.Where(
            crew =>
              (crew.type == ProtoCrewMember.KerbalType.Tourist &&
               RMLifeSpan.Instance.RMKerbals.AllrmKerbals.ContainsKey(crew.name))
              || crew.type != ProtoCrewMember.KerbalType.Tourist))
      {
        UpdateKerbal(crew, true);
      }
      if (!Api.InstalledMods.IsDfInstalled) return;
      // Check the roster list for any unknown dead kerbals (IE: DeepFreeze Frozen Compatibility).
      var unknownkerbals = HighLogic.CurrentGame.CrewRoster.Unowned.ToList();
      foreach (var crew in unknownkerbals)
      {
        UpdateKerbal(crew, true);
      }
    }

    internal void UpdateKerbal(ProtoCrewMember crew, bool addifNotFound)
    {
      var currentTime = Planetarium.GetUniversalTime();
      //First find them in the internal Dictionary.
      var kerbal = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.FirstOrDefault(a => a.Key == crew.name);
      //If not found and addifNotFound is true create a new entry
      if (kerbal.Value == null && addifNotFound)
      {
        Utilities.LogMessage("RosterManagerLifeSpanAddon.updateKerbal " + crew.name + " not found in ALLRMKerbals, adding new entry.", "info", RMSettings.VerboseLogging);
        var rmkerbal = new RMKerbal(Planetarium.GetUniversalTime(), crew, true, false);
        RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Add(crew.name, rmkerbal);
      }
      //If found update their entry
      else if (kerbal.Value != null)
      {
        if (!(currentTime - kerbal.Value.LastUpdate > RMSettings.LifeInfoUpdatePeriod)) return;
        Utilities.LogMessage("RosterManagerLifeSpanAddon.updateKerbal " + crew.name + " updating entry.", "info", RMSettings.VerboseLogging);
        if (RMLifeSpan.Instance.RMGameSettings.EnableAging)
        {
          CheckAge(crew, kerbal, currentTime);
        }
        if (RMLifeSpan.Instance.RMGameSettings.EnableSalaries)
        {
          CheckSalary(crew, kerbal, currentTime);
        }
        kerbal.Value.LastUpdate = currentTime;
        kerbal.Value.Trait = crew.trait;
        kerbal.Value.Type = crew.type;
        kerbal.Value.Status = crew.rosterStatus;
        if (crew.rosterStatus != ProtoCrewMember.RosterStatus.Available) return;
        kerbal.Value.VesselId = Guid.Empty;
        kerbal.Value.VesselName = string.Empty;
      }
    }

    private void CheckAge(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal, double currentTime)
    {
      //Calculate and update their age.
      //If they are DeepFreeze Frozen - They Don't Age, until they are thawed.
      if (Api.InstalledMods.IsDfInstalled)
      {
        if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && crew.type == ProtoCrewMember.KerbalType.Unowned)
        {
          //Frozen - check if we are tracking when they were frozen, if not, set it to the time they were frozen
          if (Math.Abs(kerbal.Value.TimeDfFrozen) < Tolerance)
          {
            if (RMAddon.FrozenKerbals.ContainsKey(crew.name))
            {
              kerbal.Value.TimeDfFrozen = RMAddon.FrozenKerbals[crew.name].lastUpdate;
            }
          }
          return;  //We don't process age any further if they are frozen.
        }
        if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && (crew.type != ProtoCrewMember.KerbalType.Unowned || crew.type != ProtoCrewMember.KerbalType.Tourist))
        {
          //They are really dead. Should this ever occur? Just in case.
          return;
        }
        //If we get here, they aren't frozen and they aren't really dead... so were they frozen?
        //IE: we know that if they are now crew, but their KerbalLifeInfo record has their status as dead and time frozen > 0
        if (crew.type == ProtoCrewMember.KerbalType.Crew && kerbal.Value.Status == ProtoCrewMember.RosterStatus.Dead && kerbal.Value.TimeDfFrozen > 0d)
        {
          //We add the time they were frozen onto their time of last birthday - effectively extending their life.
          var timeFrozen = currentTime - kerbal.Value.TimeDfFrozen;  //The amount of time they were frozen
          kerbal.Value.TimelastBirthday += timeFrozen;
          kerbal.Value.TimeDfFrozen = 0d;
        }
      }

      //Is it their Birthday?

      if (currentTime >= kerbal.Value.TimeNextBirthday)
      {
        //It's their Birthday!!!!
        kerbal.Value.Age += 1;
        kerbal.Value.TimelastBirthday = currentTime;
        kerbal.Value.TimeNextBirthday = RMKerbal.BirthdayNextDue(currentTime);
        if (kerbal.Value.Type != ProtoCrewMember.KerbalType.Applicant)
          ScreenMessages.PostScreenMessage("It's " + crew.name + " Birthday! They are now " + kerbal.Value.Age.ToString("###0"), 5.0f, ScreenMessageStyle.UPPER_CENTER);
        Utilities.LogMessage("RosterManagerLifeSpanAddon.checkAge " + crew.name + " just had a birthday. They are now " + kerbal.Value.Age.ToString("###0"), "info", RMSettings.VerboseLogging);
      }

      //Check if they Die of Old Age
      if (!(kerbal.Value.Lifespan - 2 <= kerbal.Value.Age)) return;
      int percentage;
      //Set random range based on:- if age is less than lifespan have 20% chance of death, if age is = or up to 2 years greater than lifespan have 40% chance of death.
      // if age is > than 2 years past lifespan have 60% chance of death. If age is > than 4 years past lifespan have 80% chance of death.
      if (kerbal.Value.Age < kerbal.Value.Lifespan)
        percentage = 20;
      else if (kerbal.Value.Lifespan + 2 < kerbal.Value.Age)
        percentage = 40;
      else if (kerbal.Value.Lifespan + 4 < kerbal.Value.Age)
        percentage = 60;
      else
        percentage = 80;
      if (_gen.Next(100) < percentage)
      {
        //Their Time has Come. As long as they aren't currently DeepFreeze Frozen/comatose
        if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead ||
            (crew.type == ProtoCrewMember.KerbalType.Unowned && crew.type == ProtoCrewMember.KerbalType.Tourist))
          return;
        TimeWarp.SetRate(0, false);
        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
        {
          CameraManager.Instance.SetCameraFlight();
        }
        Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckAge " + crew.name + " died from old age.", "info", RMSettings.VerboseLogging);
        ScreenMessages.PostScreenMessage(crew.name + " died at the old age of " + kerbal.Value.Age.ToString("###0"), 5.0f, ScreenMessageStyle.UPPER_CENTER);

        if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)  //On active duty, need to find their vessel and remove them.
        {
          var foundcrew = false;
          //First try to find their assigned vessel and remove them.
          if (kerbal.Value.VesselId != Guid.Empty)
          {
            var v = FlightGlobals.Vessels.FirstOrDefault(a => a.id == kerbal.Value.VesselId);
            if (v != null)
            {
              if (v.loaded)
              {
                var part = v.Parts.Find(p => p.protoModuleCrew.Contains(crew));
                if (part != null)
                {
                  part.RemoveCrewmember(crew);
                  crew.Die();
                  foundcrew = true;
                }
              }
              else
              {
                var part = v.protoVessel.protoPartSnapshots.Find(p => p.protoModuleCrew.Contains(crew));
                if (part != null)
                {
                  part.RemoveCrew(crew);
                  crew.Die();
                  foundcrew = true;
                }
              }
            }
          }
          if (!foundcrew)  //We didn't find their vessel and remove them so now search all vessels in game.
          {
            foreach (var v in FlightGlobals.Vessels)
            {
              if (v.isEVA && v.name.Contains(crew.name))
              {
                if (v.name.Contains(crew.name))
                {
                  v.rootPart.Die();
                  foundcrew = true;
                  break;
                }
              }
              if (v.loaded)
              {
                var part = v.Parts.Find(p => p.protoModuleCrew.Contains(crew));
                if (part == null) continue;
                part.RemoveCrewmember(crew);
                crew.Die();
                foundcrew = true;
                break;
              }
              else
              {
                var part = v.protoVessel.protoPartSnapshots.Find(p => p.protoModuleCrew.Contains(crew));
                if (part == null) continue;
                part.RemoveCrew(crew);
                crew.Die();
                foundcrew = true;
                break;
              }
            }
          }

          if (!foundcrew)  //We still didn't find them, log error and kill them anyway.
          {
            Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckAge " + crew.name + " couldn't find them to remove them from vessel.", "Error", RMSettings.VerboseLogging);
            crew.Die();
          }
        }
        else  //Not on active duty
        {
          crew.Die();
        }

        //Remove from LifeSpan.Instance.kerbalLifeRecord.KerbalLifeRecords
        RemoveKerbal(crew);

        // set ReSpawn
        if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
        {
          crew.StartRespawnPeriod();
        }
      }
    }

    private void CheckSalary(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal, double currentTime)
    {
      if (currentTime >= kerbal.Value.TimeSalaryDue) // Salary Due??
      {
        //Set time next salary due
        kerbal.Value.TimeSalaryDue = RMKerbal.SalaryNextDue(currentTime);
        //Pay Salary
        if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
        //Check if contractdispute is active and if it is process that
        if (kerbal.Value.SalaryContractDispute && kerbal.Value.SalaryContractDisputeProcessed)
        {
          //If they are a Tourist they are on strike. We don't process dispute.
          if (kerbal.Value.Type == ProtoCrewMember.KerbalType.Tourist)
          {
            return;
          }
          ProcessContractDispute(crew, kerbal, currentTime, false, true);
        }
        else  //No contract dispute so process normal salary.
        {
          if (Funding.CanAfford((float)kerbal.Value.Salary))
          {
            Funding.Instance.AddFunds(-kerbal.Value.Salary, TransactionReasons.CrewRecruited);
            kerbal.Value.Timelastsalary = currentTime;
            Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary paid " + crew.name + " salary.", "info", RMSettings.VerboseLogging);
            ScreenMessages.PostScreenMessage("Paid " + crew.name + " salary of " + kerbal.Value.Salary, 5.0f, ScreenMessageStyle.UPPER_CENTER);
          }
          else  //Unable to pay, start a contract dispute.
          {
            if (!kerbal.Value.SalaryContractDispute && kerbal.Value.SalaryContractDisputeProcessed)
              ProcessContractDispute(crew, kerbal, currentTime, true, true);
          }
        }
      }
    }

    private void ProcessContractDispute(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal, double currentTime, bool start, bool extend)
    {
      // They will continue to work for RMLifeSpan.Instance.rmGameSettings.MaxContractDisputePeriods of salaryperiod, with a payrise that must be accepted each time. All backpay is accrued.
      // Or they quit/strike after RMLifeSpan.Instance.rmGameSettings.MaxContractDisputePeriods or if user does not accept the payrise.
      Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary unable to pay " + crew.name + " salary.", "info", RMSettings.VerboseLogging);
      if (start)  //Start a new contract dispute
      {
        ScreenMessages.PostScreenMessage("Insufficient funds to pay " + crew.name + " salary at this time.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
        kerbal.Value.SalaryContractDispute = true;
        kerbal.Value.RealTrait = kerbal.Value.Trait;
        //Start processing dispute, increase the periods we have been in dispute, user must accept payrise as well (if they don't the kerbal Quits) and calculate and store their backpay owed.
        ExtendContractDispute(crew, kerbal);
      }
      else  // Process existing contract dispute
      {
        //Check if we have funds now?
        //If we have pay them.
        //Else extend the contract dispute.
        if (Funding.CanAfford((float)kerbal.Value.Salary + (float)kerbal.Value.OwedSalary))
        {
          Funding.Instance.AddFunds(-(kerbal.Value.Salary + kerbal.Value.OwedSalary), TransactionReasons.CrewRecruited);
          kerbal.Value.Timelastsalary = currentTime;
          kerbal.Value.SalaryContractDispute = false;
          kerbal.Value.SalaryContractDisputeProcessed = true;
          kerbal.Value.SalaryContractDisputePeriods = 0;
          kerbal.Value.PayriseRequired = 0d;
          kerbal.Value.OwedSalary = 0d;
          //If they are a tourist (dispute) and not dead (DeepFreeze frozen/comatose) set them back to crew
          if (kerbal.Value.Type == ProtoCrewMember.KerbalType.Tourist && crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
          {
            kerbal.Value.Type = ProtoCrewMember.KerbalType.Crew;
            crew.type = ProtoCrewMember.KerbalType.Crew;
            kerbal.Value.Trait = kerbal.Value.RealTrait;
            crew.trait = kerbal.Value.RealTrait;
            KerbalRoster.SetExperienceTrait(crew, crew.trait);
            RMKerbal.RegisterExperienceTrait(kerbal.Value);
          }
          Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary paid " + crew.name + " salary.", "info", RMSettings.VerboseLogging);
          Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary contract dispute ended " + crew.name, "info", RMSettings.VerboseLogging);
          ScreenMessages.PostScreenMessage("Paid " + crew.name + " salary of " + (kerbal.Value.Salary + kerbal.Value.OwedSalary), 5.0f, ScreenMessageStyle.UPPER_CENTER);
          ScreenMessages.PostScreenMessage(crew.name + " contract dispute ended.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
        }
        else  //Can't end dispute
        {
          if (!extend) return;
          kerbal.Value.Timelastsalary = currentTime;
          ExtendContractDispute(crew, kerbal);
        }
      }
    }

    private void ExtendContractDispute(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal)
    {
      //How many salaryperiods have we been in dispute? If > RMSettings.MaxContractDisputePeriods periods kerbal Quits (becomes a tourist).
      // otherwise, increase the periods we have been in dispute, user must accept payrise as well (if they don't the kerbal Quits) and calculate and store their backpay owed.
      kerbal.Value.SalaryContractDisputePeriods++;
      if (kerbal.Value.SalaryContractDisputePeriods > RMLifeSpan.Instance.RMGameSettings.MaxContractDisputePeriods)
      {
        //Kerbal Quits.
        ResignKerbal(crew, kerbal);
        kerbal.Value.SalaryContractDisputeProcessed = true;
      }
      else
      {
        //User must accept payrise of 10% * disputed periods. (IE; first period 10%, 2nd period 20%, etc)
        var payriseRequired = kerbal.Value.Salary * (kerbal.Value.SalaryContractDisputePeriods * 10d / 100d);
        //Salaries cannot exceed 100000
        if (kerbal.Value.Salary + payriseRequired > 100000)  //**WIP Marker This probably should be a settings VAR, Hard-coded upper salary limit of 100000 funds.
          payriseRequired = 0;
        kerbal.Value.PayriseRequired = payriseRequired;
        kerbal.Value.SalaryContractDisputeProcessed = false;
      }
    }

    internal void ResignKerbal(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal)
    {
      Utilities.LogMessage("RosterManagerLifeSpanAddon.resignKerbal " + crew.name + " contract in dispute. They will remain a tourist until they are paid.", "info", RMSettings.VerboseLogging);
      ScreenMessages.PostScreenMessage(crew.name + " contract in dispute. They will remain a tourist until they are paid.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
      //We don't change their status if they are unowned/dead (DeepFreeze Frozen)
      if (crew.type == ProtoCrewMember.KerbalType.Unowned || crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead)
        return;
      RMKerbal.UnregisterExperienceTrait(kerbal.Value);
      //kerbal.Value.RealTrait = kerbal.Value.Trait;
      kerbal.Value.Type = ProtoCrewMember.KerbalType.Tourist;
      crew.type = ProtoCrewMember.KerbalType.Tourist;
      kerbal.Value.Trait = "Tourist";
      crew.trait = "Tourist";
      KerbalRoster.SetExperienceTrait(crew, crew.trait);
      kerbal.Value.PayriseRequired = 0d;
      kerbal.Value.SalaryContractDisputeProcessed = true;
      kerbal.Value.SalaryContractDispute = true;
    }

    public void RemoveKerbal(ProtoCrewMember crew)
    {
      //First find them in the internal Dictionary.
      if (!RMLifeSpan.Instance.RMKerbals.AllrmKerbals.ContainsKey(crew.name)) return;
      Utilities.LogMessage("RosterManagerLifeSpanAddon.removeKerbal " + crew.name + " removed from ALLRMKerbals.", "info", RMSettings.VerboseLogging);
      //Then remove them.
      RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Remove(crew.name);
    }
  }
}