using System.Collections.Generic;
using UnityEngine;

namespace RosterManager
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION)]
    public class LifeSpan : ScenarioModule
    {
        private static LifeSpan _Instance;

        public static bool isActive
        {
            get
            {
                return _Instance != null;
            }
        }

        public static LifeSpan Instance
        {
            get
            {
                return _Instance;
            }
        }

        internal KerbalLifeRecord kerbalLifeRecord { get; private set; }
        private readonly List<Component> children = new List<Component>();

        public override void OnAwake()
        {
            Utilities.LogMessage("RosterManagerLifeSpan.Awake Active...", "info", RMSettings.VerboseLogging);
            base.OnAwake();
            _Instance = this;
            kerbalLifeRecord = new KerbalLifeRecord();

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding SpaceCenterManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<LifeSpanAddon>();
                children.Add(KLMem);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding FlightManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<LifeSpanAddon>();
                children.Add(KLMem);
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding EditorManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<LifeSpanAddon>();
                children.Add(KLMem);
            }
            else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                Utilities.LogMessage("RosterManagerLifeSpan.Awake adding TrackingStationManager", "info", RMSettings.VerboseLogging);
                var KLMem = gameObject.AddComponent<LifeSpanAddon>();
                children.Add(KLMem);
            }
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            kerbalLifeRecord.Load(gameNode);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            kerbalLifeRecord.Save(gameNode);
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