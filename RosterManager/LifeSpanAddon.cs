using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DF;

namespace RosterManager
{
    class LifeSpanAddon : MonoBehaviour
    {
        private static LifeSpanAddon _Instance;

        public static bool isActive
        {
            get
            {
                return _Instance != null;
            }
        }

        public static LifeSpanAddon Instance
        {
            get
            {
                return _Instance;
            } 
        }

        public static bool KilledOneYet = false;
        public static Vessel vssl;
        private System.Random rnd = new System.Random();  // Random seed for setting Kerbals ages
        //Make these two const settings fields.
        private const int minimum_Age = 25;
        private const int maximum_Age = 75;
        private System.Random gen = new System.Random();

        protected LifeSpanAddon()
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
            checkDatabase();            
            KilledOneYet = false;
        }

        public void Update()
        {
            //This is Experimental CODE!!
            
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (FlightGlobals.ActiveVessel != null)
                {
                    foreach (ProtoCrewMember crew in FlightGlobals.ActiveVessel.GetVesselCrew().ToList())
                    {
                        KeyValuePair<string, KerbalLifeInfo> kerbal = LifeSpan.Instance.kerbalLifeSpan.KerbalLifeSpans.FirstOrDefault(a => a.Key == crew.name);
                        kerbal.Value.vesselID = FlightGlobals.ActiveVessel.id;
                        kerbal.Value.vesselName = FlightGlobals.ActiveVessel.vesselName;
                    }
                    /*        
                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.EVA)
                    {
                        if (!KilledOneYet) //Only Kill One
                        {
                            vssl = FlightGlobals.ActiveVessel;
                            Debug.Log("KerbalLife or should we say DEATH");

                            List<KerbalEVA> kblEVAs = vssl.FindPartModulesImplementing<KerbalEVA>();
                            if (kblEVAs.Count > 0)  //We have a KerbalEVA partmodule
                            {
                                if (!kblEVAs[0].OnALadder) //Don't kill them if they are on a ladder
                                {
                                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.Crew.FirstOrDefault(a => vssl.name.Contains(a.name));
                                    vssl.vesselType = VesselType.Debris;  //set to Debris.. they do appear until you return to KSC.
                                    vssl.name = "DEAD - " + kerbal.name;    // Change the Vessel name to "DEAD name"
                                    //kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Dead;  //Change their roster status to DEAD!!!
                                    if (vssl.situation == Vessel.Situations.LANDED)
                                    {
                                        if (kblEVAs[0].flagItems < 1)
                                            kblEVAs[0].flagItems = 1;
                                        ScreenMessages.PostScreenMessage("After a long life you Kerbal has died of old age. Please Create a Flag to mark their burial spot.", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                                        kblEVAs[0].PlantFlag();
                                    }
                                    else
                                    {
                                        ScreenMessages.PostScreenMessage("After a long life you Kerbal has died of old age.", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                                    }
                                    kblEVAs[0].isRagdoll = true;  //Works briefly....                                   
                                    kerbal.Die();
                                    //if (kerbal.KerbalRef != null)
                                    //{
                                    //    if (kerbal.KerbalRef.GetComponent<ModuleCommand>() == null)
                                    //        kerbal.KerbalRef.gameObject.AddComponent<ModuleCommand>();
                                    //}
                                    //if (vssl != null)
                                    //{
                                    //    if (vssl.GetComponent<ModuleCommand>() == null)
                                    //        vssl.gameObject.AddComponent<ModuleCommand>();
                                    //}                                    
                                    KilledOneYet = true;
                                }

                            }
                        }
                    }
                    */
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
                // If they are not Dead status. or they are Dead status and they are unowned (frozen) or tourist (comatose) - We update their Life stats.
                if (crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead 
                    || (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && ( crew.type == ProtoCrewMember.KerbalType.Unowned || crew.type == ProtoCrewMember.KerbalType.Tourist)))
                {
                    updateKerbal(crew, true);
                }
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
            List<ProtoCrewMember> crewkerbals = HighLogic.CurrentGame.CrewRoster.Crew.Concat(HighLogic.CurrentGame.CrewRoster.Applicants).ToList();
            foreach (ProtoCrewMember crew in crewkerbals)
            {
                updateKerbal(crew, true);
            }

            // Check the roster list for any unknown dead kerbals (IE: DeepFreeze Frozen Compatibility).
            List<ProtoCrewMember> unknownkerbals = HighLogic.CurrentGame.CrewRoster.Unowned.ToList();
            foreach (ProtoCrewMember crew in crewkerbals)
            {
                updateKerbal(crew, true);
            }
        }

        public void updateKerbal(ProtoCrewMember crew, bool addifNotFound)
        {
            double currentTime = Planetarium.GetUniversalTime();
            //First find them in the internal Dictionary.
            KeyValuePair<string, KerbalLifeInfo> kerbal = LifeSpan.Instance.kerbalLifeSpan.KerbalLifeSpans.FirstOrDefault(a => a.Key == crew.name);
            //If not found and addifNotFound is true create a new entry
            if (kerbal.Value == null && addifNotFound)
            {
                KerbalLifeInfo kerballifeinfo = new KerbalLifeInfo(Planetarium.GetUniversalTime());
                kerballifeinfo.experienceTraitName = crew.experienceTrait.Title;
                kerballifeinfo.type = crew.type;
                kerballifeinfo.status = crew.rosterStatus;
                kerballifeinfo.vesselID = Guid.Empty;
                kerballifeinfo.vesselName = string.Empty;
                double dice_minage = rnd.Next(minimum_Age - 3, minimum_Age + 3); // Randomly set their age.
                kerballifeinfo.age = dice_minage;
                double dice_maxage = rnd.Next(maximum_Age - 5, maximum_Age + 5); // Randomly set their age.
                kerballifeinfo.lifespan = dice_maxage;
                LifeSpan.Instance.kerbalLifeSpan.KerbalLifeSpans.Add(crew.name, kerballifeinfo);
                kerballifeinfo.timelastBirthday = currentTime;
                if (DFInterface.IsDFInstalled)
                {
                    if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && crew.type == ProtoCrewMember.KerbalType.Unowned)  // if they are frozen store time frozen
                    {
                        if (RMAddon.FrozenKerbals.ContainsKey(crew.name))
                        {                           
                            kerballifeinfo.timeDFFrozen = RMAddon.FrozenKerbals[crew.name].lastUpdate;
                        }                            
                    }
                }
            }

            //If found update their entry
            if (kerbal.Value != null)
            {                               
                if (kerbal.Value.lastUpdate - currentTime > 360)
                {
                    checkAge(crew, kerbal, currentTime);
                    kerbal.Value.lastUpdate = currentTime;
                    kerbal.Value.experienceTraitName = crew.experienceTrait.Title;
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

        public void checkAge(ProtoCrewMember crew, KeyValuePair<string, KerbalLifeInfo> kerbal, double currentTime)
        {
            //Calculate and update their age in seconds?? or years?? or what?? TBD.
            //If they are DeepFreeze Frozen - They Don't Age, until they are thawed.
            if (DFInterface.IsDFInstalled)
            {
                if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && crew.type == ProtoCrewMember.KerbalType.Unowned)
                {
                    //Frozen
                    if (kerbal.Value.timeDFFrozen == 0d)
                    {
                        if (RMAddon.FrozenKerbals.ContainsKey(crew.name))
                        {
                            kerbal.Value.timeDFFrozen = RMAddon.FrozenKerbals[crew.name].lastUpdate;
                        }
                    }
                    return;
                }
                if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Dead && (crew.type != ProtoCrewMember.KerbalType.Unowned || crew.type != ProtoCrewMember.KerbalType.Tourist))
                {
                    //They are really dead.
                    return;
                }
                //If we get here, they aren't frozen and they aren't really dead... so were they frozen? IE: we know them as status dead and time frozen > 0
                if (crew.type == ProtoCrewMember.KerbalType.Crew && kerbal.Value.status == ProtoCrewMember.RosterStatus.Dead && kerbal.Value.timeDFFrozen > 0d)
                {
                    //We add the time they were frozen onto their time of last birthday - effectively extending their life.
                    double timeFrozen = currentTime - kerbal.Value.timeDFFrozen;
                    kerbal.Value.timelastBirthday += timeFrozen;
                }
            }

            //Is it their Birthday?
            double birthdayTimeDiff = currentTime - kerbal.Value.timelastBirthday;
            if (GameSettings.KERBIN_TIME)
            {

                double years = birthdayTimeDiff / 60 / 60 / 6 / 426;
                if (years >= 1)
                {
                    //It's their Birthday!!!!
                    kerbal.Value.age += years;
                    kerbal.Value.timelastBirthday = currentTime;
                }
            }
            else
            {
                double years = birthdayTimeDiff / 60 / 60 / 24 / 365;
                if (years >= 1)
                {
                    //It's their Birthday!!!!
                    kerbal.Value.age += years;
                    kerbal.Value.timelastBirthday = currentTime;
                }
            }
            //Check if they Die of Old Age
            if (kerbal.Value.lifespan - 2 <= kerbal.Value.age)
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
                    //Their Time has Come.
                    // Screen Message
                    if (crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead && (crew.type != ProtoCrewMember.KerbalType.Unowned || crew.type != ProtoCrewMember.KerbalType.Tourist))
                    {
                        crew.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
                        // set Repawn?  
                        if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn == true)
                        {
                            crew.StartRespawnPeriod();
                        }
                    }
                }
            }
        }

        public void removeKerbal(ProtoCrewMember crew)
        {
            //First find them in the internal Dictionary.
            if (LifeSpan.Instance.kerbalLifeSpan.KerbalLifeSpans.ContainsKey(crew.name))
            {
                //Then remove them.
                LifeSpan.Instance.kerbalLifeSpan.KerbalLifeSpans.Remove(crew.name);
            }
        }
    }
}
