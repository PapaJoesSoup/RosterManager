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
        private static RMLifeSpanAddon _Instance;

        public static bool isActive
        {
            get
            {
                return _Instance != null;
            }
        }

        public static RMLifeSpanAddon Instance
        {
            get
            {
                return _Instance;
            }
        }                    
        
        private System.Random gen = new System.Random();  // Random seed for deciding when a kerbal dies of old age. Do we need two seeds?
                
        protected RMLifeSpanAddon()
        {
            Utilities.LogMessage("RosterManagerLifeSpanAddon.Constructor Active...", "info", RMSettings.VerboseLogging);
            _Instance = this;
        }

        private void Awake()
        {
            Utilities.LogMessage("RosterManagerLifeSpanAddon Awake...", "info", RMSettings.VerboseLogging);
            GameEvents.onKerbalAdded.Add(onKerbalAdded);
            GameEvents.onKerbalRemoved.Add(onKerbalRemoved);
        }

        private void Start()
        {
            Utilities.LogMessage("RosterManagerLifeSpanAddon Startup...", "info", RMSettings.VerboseLogging);
            try
            {
                checkDatabase();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerLifeSpanAddon.Start.  " + ex.ToString(), "Error", true);
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
                        foreach (ProtoCrewMember crew in FlightGlobals.ActiveVessel.GetVesselCrew().ToList())
                        {
                            KeyValuePair<string, RMKerbal> kerbal = RMLifeSpan.Instance.rmKerbals.ALLRMKerbals.FirstOrDefault(a => a.Key == crew.name);
                            if (kerbal.Key != null)
                            {
                                kerbal.Value.vesselID = FlightGlobals.ActiveVessel.id;
                                kerbal.Value.vesselName = FlightGlobals.ActiveVessel.vesselName;
                            }

                        }
                    }
                }
                //Update all known Crew, Applicants in any game scene.
                List<ProtoCrewMember> CrewList = new List<ProtoCrewMember>();
                CrewList = HighLogic.CurrentGame.CrewRoster.Crew.Concat(HighLogic.CurrentGame.CrewRoster.Applicants).ToList();
                //If Deepfreeze is installed add Unowned and Tourists to the list (could be frozen or comatose).
                if (DFInterface.IsDFInstalled)
                {
                    CrewList = CrewList.Concat(HighLogic.CurrentGame.CrewRoster.Unowned).Concat(HighLogic.CurrentGame.CrewRoster.Tourist).ToList();
                }
                foreach (ProtoCrewMember crew in CrewList)
                {
                    // If they are not Dead or they are Dead status and they are unowned (frozen) or tourist (comatose) - We update their Life stats.
                    if (crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead
                        || (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && (crew.type == ProtoCrewMember.KerbalType.Unowned || crew.type == ProtoCrewMember.KerbalType.Tourist)))
                    {
                        updateKerbal(crew, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerLifeSpanAddon.Update.  " + ex.ToString(), "Error", true);
            }
        }

        private void OnDestroy()
        {
            Utilities.LogMessage("RosterManagerLifeSpanAddon OnDestroy...", "info", RMSettings.VerboseLogging);
            GameEvents.onKerbalAdded.Remove(onKerbalAdded);
            GameEvents.onKerbalRemoved.Remove(onKerbalRemoved);
        }

        private void onKerbalAdded(ProtoCrewMember crew)
        {
            updateKerbal(crew, true);
        }

        private void onKerbalRemoved(ProtoCrewMember crew)
        {
            removeKerbal(crew);
        }

        private void checkDatabase()
        {
            // Check the roster list of crew and applicants for KerbalLife settings.
            List<ProtoCrewMember> crewkerbals = HighLogic.CurrentGame.CrewRoster.Crew.Concat(HighLogic.CurrentGame.CrewRoster.Applicants).Concat(HighLogic.CurrentGame.CrewRoster.Tourist).ToList();
            foreach (ProtoCrewMember crew in crewkerbals)
            {
                if ((crew.type == ProtoCrewMember.KerbalType.Tourist && RMLifeSpan.Instance.rmKerbals.ALLRMKerbals.ContainsKey(crew.name))
                    || crew.type != ProtoCrewMember.KerbalType.Tourist)
                {
                    updateKerbal(crew, true);
                }
            }

            // Check the roster list for any unknown dead kerbals (IE: DeepFreeze Frozen Compatibility).
            List<ProtoCrewMember> unknownkerbals = HighLogic.CurrentGame.CrewRoster.Unowned.ToList();
            foreach (ProtoCrewMember crew in unknownkerbals)
            {
                updateKerbal(crew, true);
            }
        }

        internal void updateKerbal(ProtoCrewMember crew, bool addifNotFound)
        {
            double currentTime = Planetarium.GetUniversalTime();
            //First find them in the internal Dictionary.
            KeyValuePair<string, RMKerbal> kerbal = RMLifeSpan.Instance.rmKerbals.ALLRMKerbals.FirstOrDefault(a => a.Key == crew.name);
            //If not found and addifNotFound is true create a new entry
            if (kerbal.Value == null && addifNotFound)
            {
                RMKerbal rmkerbal = new RMKerbal(Planetarium.GetUniversalTime(), crew, true, false);                
                RMLifeSpan.Instance.rmKerbals.ALLRMKerbals.Add(crew.name, rmkerbal);              
    }
            //If found update their entry
            else if (kerbal.Value != null)
            {                
                if (currentTime - kerbal.Value.lastUpdate > RMSettings.LifeInfoUpdatePeriod)  // Only update every 6 minutes. Can be changed in hidden settings
                {
                    if (RMLifeSpan.Instance.rmGameSettings.EnableAging)
                    {
                        checkAge(crew, kerbal, currentTime);
                    }                        
                    if (RMLifeSpan.Instance.rmGameSettings.EnableSalaries)
                    {
                        checkSalary(crew, kerbal, currentTime);
                    }
                    kerbal.Value.lastUpdate = currentTime;
                    kerbal.Value.Trait = crew.trait;
                    kerbal.Value.type = crew.type;
                    kerbal.Value.status = crew.rosterStatus;
                    if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                    {
                        kerbal.Value.vesselID = Guid.Empty;
                        kerbal.Value.vesselName = string.Empty;
                    }
                }
            }
        }

        private void checkAge(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal, double currentTime)
        {
            //Calculate and update their age.
            //If they are DeepFreeze Frozen - They Don't Age, until they are thawed.
            if (DFInterface.IsDFInstalled)
            {
                if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && crew.type == ProtoCrewMember.KerbalType.Unowned)
                {
                    //Frozen - check if we are tracking when they were frozen, if not, set it to the time they were frozen
                    if (kerbal.Value.timeDFFrozen == 0d)
                    {
                        if (RMAddon.FrozenKerbals.ContainsKey(crew.name))
                        {
                            kerbal.Value.timeDFFrozen = RMAddon.FrozenKerbals[crew.name].lastUpdate;
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
                if (crew.type == ProtoCrewMember.KerbalType.Crew && kerbal.Value.status == ProtoCrewMember.RosterStatus.Dead && kerbal.Value.timeDFFrozen > 0d)
                {
                    //We add the time they were frozen onto their time of last birthday - effectively extending their life.
                    double timeFrozen = currentTime - kerbal.Value.timeDFFrozen;  //The amount of time they were frozen
                    kerbal.Value.timelastBirthday += timeFrozen;
                    kerbal.Value.timeDFFrozen = 0d;
                }
            }

            //Is it their Birthday?
            
            if (currentTime >= kerbal.Value.timeNextBirthday)
            {
                //It's their Birthday!!!!
                kerbal.Value.age += 1;
                kerbal.Value.timelastBirthday = currentTime;
                kerbal.Value.timeNextBirthday = RMKerbal.BirthdayNextDue(currentTime);
                if (kerbal.Value.type != ProtoCrewMember.KerbalType.Applicant)
                    ScreenMessages.PostScreenMessage("It's " + crew.name + " Birthday! They are now " + kerbal.Value.age, 5.0f, ScreenMessageStyle.UPPER_CENTER);                                
            }

            //Check if they Die of Old Age
            if (kerbal.Value.lifespan - 2 <= kerbal.Value.age)  //We start rolling the dice when their age is within 2 years of their lifespan age.
            {
                int percentage = 20;                
                //Set random range based on:- if age is less than lifespan have 20% chance of death, if age is = or up to 2 years greater than lifespan have 40% chance of death.
                // if age is > than 2 years past lifespan have 60% chance of death. If age is > than 4 years past lifespan have 80% chance of death.
                if (kerbal.Value.age < kerbal.Value.lifespan)
                    percentage = 20;
                else if (kerbal.Value.lifespan + 2 < kerbal.Value.age)
                    percentage = 40;
                else if (kerbal.Value.lifespan + 4 < kerbal.Value.age)
                    percentage = 60;
                else
                    percentage = 80;
                if (gen.Next(100) < percentage)
                {
                    //Their Time has Come. As long as they aren't currently DeepFreeze Frozen/comatose                    
                    if (crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead && (crew.type != ProtoCrewMember.KerbalType.Unowned || crew.type != ProtoCrewMember.KerbalType.Tourist))
                    {
                        TimeWarp.SetRate(0, false);
                        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
                        {
                            CameraManager.Instance.SetCameraFlight();
                        }
                        Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckAge " + crew.name + " died from old age.", "info", RMSettings.VerboseLogging);
                        ScreenMessages.PostScreenMessage(crew.name + " died at the old age of " + kerbal.Value.age, 5.0f, ScreenMessageStyle.UPPER_CENTER);

                        if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)  //On active duty, need to find their vessel and remove them.
                        {
                            bool foundcrew = false;
                            //First try to find their assigned vessel and remove them.
                            if (kerbal.Value.vesselID != Guid.Empty)
                            {
                                Vessel v = FlightGlobals.Vessels.FirstOrDefault(a => a.id == kerbal.Value.vesselID);
                                if (v != null)
                                {
                                    if (v.loaded)
                                    {
                                        Part part = v.Parts.Find(p => p.protoModuleCrew.Contains(crew));
                                        if (part != null)
                                        {
                                            part.RemoveCrewmember(crew);
                                            crew.Die();
                                            foundcrew = true;                                            
                                        }
                                    }
                                    else
                                    {
                                        ProtoPartSnapshot part = v.protoVessel.protoPartSnapshots.Find(p => p.protoModuleCrew.Contains(crew));
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
                                foreach (Vessel v in FlightGlobals.Vessels)
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
                                        Part part = v.Parts.Find(p => p.protoModuleCrew.Contains(crew));
                                        if (part != null)
                                        {
                                            part.RemoveCrewmember(crew);
                                            crew.Die();
                                            foundcrew = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        ProtoPartSnapshot part = v.protoVessel.protoPartSnapshots.Find(p => p.protoModuleCrew.Contains(crew));
                                        if (part != null)
                                        {
                                            part.RemoveCrew(crew);
                                            crew.Die();
                                            foundcrew = true;
                                            break;
                                        }
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
                        removeKerbal(crew);

                        // set ReSpawn
                        if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                        {
                            crew.StartRespawnPeriod();
                        }
                    }
                }
            }
        }

        private void checkSalary(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal, double currentTime)
        {            
            if (currentTime >= kerbal.Value.timeSalaryDue) // Salary Due??
            {
                //Set time next salary due
                kerbal.Value.timeSalaryDue = RMKerbal.SalaryNextDue(currentTime);
                //Pay Salary
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    //Check if contractdispute is active and if it is process that
                    if (kerbal.Value.salaryContractDispute && kerbal.Value.salaryContractDisputeProcessed)
                    {
                        //If they are a Tourist they are on strike. We don't process dispute. 
                        if (kerbal.Value.type == ProtoCrewMember.KerbalType.Tourist)
                        {                            
                            return;
                        }
                        processContractDispute(crew, kerbal, currentTime, false, true);
                    }
                    else  //No contract dispute so process normal salary.
                    {
                        if (Funding.CanAfford((float)kerbal.Value.salary))
                        {
                            Funding.Instance.AddFunds(-kerbal.Value.salary, TransactionReasons.CrewRecruited);
                            kerbal.Value.timelastsalary = currentTime;                            
                            Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary paid " + crew.name + " salary.", "info", RMSettings.VerboseLogging);
                            ScreenMessages.PostScreenMessage("Paid " + crew.name + " salary of " + kerbal.Value.salary.ToString(), 5.0f, ScreenMessageStyle.UPPER_CENTER);
                        }
                        else  //Unable to pay, start a contract dispute.
                        {
                            if (!kerbal.Value.salaryContractDispute && kerbal.Value.salaryContractDisputeProcessed)
                                processContractDispute(crew, kerbal, currentTime, true, true);
                        }
                    }
                }
            }                  
        }

        private void processContractDispute(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal, double currentTime, bool start, bool extend)
        {
            // They will continue to work for RMLifeSpan.Instance.rmGameSettings.MaxContractDisputePeriods of salaryperiod, with a payrise that must be accepted each time. All backpay is accrued.
            // Or they quit/strike after RMLifeSpan.Instance.rmGameSettings.MaxContractDisputePeriods or if user does not accept the payrise.            
            Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary unable to pay " + crew.name + " salary.", "info", RMSettings.VerboseLogging);            
            if (start)  //Start a new contract dispute
            {
                ScreenMessages.PostScreenMessage("Insufficient funds to pay " + crew.name + " salary at this time.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                kerbal.Value.salaryContractDispute = true;
                kerbal.Value.RealTrait = kerbal.Value.Trait;
                //Start processing dispute, increase the periods we have been in dispute, user must accept payrise as well (if they don't the kerbal Quits) and calculate and store their backpay owed.
                extendContractDispute(crew, kerbal);
            }
            else  // Process existing contract dispute
            {
                //Check if we have funds now?
                //If we have pay them.
                //Else extend the contract dispute.
                if (Funding.CanAfford((float)kerbal.Value.salary + (float)kerbal.Value.owedSalary))
                {
                    Funding.Instance.AddFunds(-(kerbal.Value.salary + kerbal.Value.owedSalary), TransactionReasons.CrewRecruited);
                    kerbal.Value.timelastsalary = currentTime;
                    kerbal.Value.salaryContractDispute = false;
                    kerbal.Value.salaryContractDisputeProcessed = true;
                    kerbal.Value.salaryContractDisputePeriods = 0;
                    kerbal.Value.payriseRequired = 0d;
                    kerbal.Value.owedSalary = 0d;
                    //If they are a tourist (dispute) and not dead (DeepFreeze frozen/comatose) set them back to crew                   
                    if (kerbal.Value.type == ProtoCrewMember.KerbalType.Tourist && crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
                    {
                        kerbal.Value.type = ProtoCrewMember.KerbalType.Crew;
                        crew.type = ProtoCrewMember.KerbalType.Crew;
                        kerbal.Value.Trait = kerbal.Value.RealTrait;
                        crew.trait = kerbal.Value.RealTrait;
                        KerbalRoster.SetExperienceTrait(crew, crew.trait);
                        RMKerbal.RegisterExperienceTrait(kerbal.Value);                      
                    }
                    Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary paid " + crew.name + " salary.", "info", RMSettings.VerboseLogging);
                    Utilities.LogMessage("RosterManagerLifeSpanAddon.CheckSalary contract dispute ended " + crew.name, "info", RMSettings.VerboseLogging);
                    ScreenMessages.PostScreenMessage("Paid " + crew.name + " salary of " + (kerbal.Value.salary + kerbal.Value.owedSalary).ToString(), 5.0f, ScreenMessageStyle.UPPER_CENTER);
                    ScreenMessages.PostScreenMessage(crew.name + " contract dispute ended.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                }
                else  //Can't end dispute
                {                    
                    if (extend)
                    {
                        kerbal.Value.timelastsalary = currentTime;
                        extendContractDispute(crew, kerbal);
                    }                        
                }
            }
        }        

        private void extendContractDispute(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal)
        {
            //How many salaryperiods have we been in dispute? If > RMSettings.MaxContractDisputePeriods periods kerbal Quits (becomes a tourist).
            // otherwise, increase the periods we have been in dispute, user must accept payrise as well (if they don't the kerbal Quits) and calculate and store their backpay owed.
            kerbal.Value.salaryContractDisputePeriods++;
            if (kerbal.Value.salaryContractDisputePeriods > RMLifeSpan.Instance.rmGameSettings.MaxContractDisputePeriods)
            {                
                //Kerbal Quits.
                resignKerbal(crew, kerbal);
                kerbal.Value.salaryContractDisputeProcessed = true;
            }
            else
            {                
                //User must accept payrise of 10% * disputed periods. (IE; first period 10%, 2nd period 20%, etc)
                double payriseRequired = kerbal.Value.salary * (kerbal.Value.salaryContractDisputePeriods * 10d / 100d);                
                //Salaries cannot exceed 100000
                if (kerbal.Value.salary + payriseRequired > 100000)  //**WIP Marker This probably should be a settings VAR, Hard-coded upper salary limit of 100000 funds.
                    payriseRequired = 0;
                kerbal.Value.payriseRequired = payriseRequired;
                kerbal.Value.salaryContractDisputeProcessed = false;           
            }                            
        }

        internal void resignKerbal(ProtoCrewMember crew, KeyValuePair<string, RMKerbal> kerbal)
        {
            Utilities.LogMessage("RosterManagerLifeSpanAddon.resignKerbal " + crew.name + " contract in dispute. They will remain a tourist until they are paid.", "info", RMSettings.VerboseLogging);
            ScreenMessages.PostScreenMessage(crew.name + " contract in dispute. They will remain a tourist until they are paid.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
            //We don't change their status if they are unowned/dead (DeepFreeze Frozen)
            if (crew.type != ProtoCrewMember.KerbalType.Unowned && crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
            {                
                RMKerbal.UnregisterExperienceTrait(kerbal.Value);
                //kerbal.Value.RealTrait = kerbal.Value.Trait;
                kerbal.Value.type = ProtoCrewMember.KerbalType.Tourist;
                crew.type = ProtoCrewMember.KerbalType.Tourist;
                kerbal.Value.Trait = "Tourist";
                crew.trait = "Tourist";
                KerbalRoster.SetExperienceTrait(crew, crew.trait);
                kerbal.Value.payriseRequired = 0d;                
                kerbal.Value.salaryContractDisputeProcessed = true;
                kerbal.Value.salaryContractDispute = true;
            }                       
        }

        public void removeKerbal(ProtoCrewMember crew)
        {
            //First find them in the internal Dictionary.
            if (RMLifeSpan.Instance.rmKerbals.ALLRMKerbals.ContainsKey(crew.name))
            {
                //Then remove them.
                RMLifeSpan.Instance.rmKerbals.ALLRMKerbals.Remove(crew.name);
            }
        }        
    }
}