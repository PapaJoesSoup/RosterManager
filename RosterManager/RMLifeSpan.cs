using System.Collections.Generic;
using UnityEngine;

namespace RosterManager
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION)]
    public class RMLifeSpan : ScenarioModule
    {
        // This ScenarioModule class stores the Roster Manager RMKerbals instance of all known kerbals.
        // The class is used to:-
        // Persist the RMKerbals data in the save game file.
        // Load the RMLifeSpanAddon class which processes Age and Salary processing.        
        private static RMLifeSpan _Instance;

        public static bool isActive
        {
            get
            {
                return _Instance != null;
            }
        }

        public static RMLifeSpan Instance
        {
            get
            {
                return _Instance;
            }
        }

        internal RMKerbals rmKerbals { get; private set; }
        internal RMGameSettings rmGameSettings { get; set; }
        private readonly List<Component> children = new List<Component>();

        public override void OnAwake()
        {
            Utilities.LogMessage("RosterManagerLifeSpan.Awake Active...", "info", RMSettings.VerboseLogging);
            base.OnAwake();
            _Instance = this;
            rmKerbals = new RMKerbals();
            rmGameSettings = new RMGameSettings();

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding SpaceCenterManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<RMLifeSpanAddon>();
                children.Add(KLMem);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding FlightManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<RMLifeSpanAddon>();
                children.Add(KLMem);
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding EditorManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<RMLifeSpanAddon>();
                children.Add(KLMem);
            }
            else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding TrackingStationManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<RMLifeSpanAddon>();
                children.Add(KLMem);
            }
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            rmGameSettings.Load(gameNode);
            rmKerbals.Load(gameNode);
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                //Salaries and Profession change charing are disabled in non-career mode games.
                rmGameSettings.EnableSalaries = false;
                rmGameSettings.ChangeProfessionCharge = false;                
            }
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            rmGameSettings.Save(gameNode);
            rmKerbals.Save(gameNode);
        }

        protected void OnDestroy()
        {
            Utilities.LogMessage("RosterManagerLifeSpan.Awake OnDestroy...", "info", RMSettings.VerboseLogging);
            foreach (Component child in children)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake Destroying " + child.name, "info", RMSettings.VerboseLogging);
                Destroy(child);
            }
            children.Clear();
        }
    }
}