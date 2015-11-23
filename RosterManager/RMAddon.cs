using DF;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosterManager
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    internal partial class RMAddon : MonoBehaviour
    {
        #region Properties

        internal static string TextureFolder = "RosterManager/Textures/";
        internal static string saveMessage = string.Empty;

        internal static double timestamp = 0.0;

        // Toolbar Integration.
        private static IButton RMRoster_Blizzy = null;

        private static ApplicationLauncherButton RMRoster_Stock = null;
        internal static bool frameErrTripped = false;

        // Tooltip vars
        internal static Vector2 ToolTipPos;

        internal static string toolTip;

        internal static List<KeyValuePair<string, RMKerbal>> AllCrew = new List<KeyValuePair<string, RMKerbal>>();

        internal static string AllCrewSort = "";

        // DeepFreeze Frozen Crew interface
        internal static Dictionary<string, KerbalInfo> FrozenKerbals = new Dictionary<string, KerbalInfo>();

        #endregion Properties

        #region Event handlers

        // Addon state event handlers
        internal void Awake()
        {
            try
            {                
                if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    DontDestroyOnLoad(this);
                    RMSettings.ApplySettings();
                    WindowRoster.ResetKerbalProfessions();
                    Utilities.LogMessage("RosterManagerAddon.Awake Active...", "info", RMSettings.VerboseLogging);

                    if (RMSettings.EnableBlizzyToolbar)
                    {
                        // Let't try to use Blizzy's toolbar
                        Utilities.LogMessage("RosterManagerAddon.Awake - Blizzy Toolbar Selected.", "Info", RMSettings.VerboseLogging);
                        if (!ActivateBlizzyToolBar())
                        {
                            // We failed to activate the toolbar, so revert to stock
                            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                            Utilities.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
                        }
                    }
                    else
                    {
                        // Use stock Toolbar
                        Utilities.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
                        GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                        GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerAddon.Awake.  Error:  " + ex.ToString(), "Error", true);
            }
        }

        internal void Start()
        {
            Utilities.LogMessage("RosterManagerAddon.Start.", "Info", RMSettings.VerboseLogging);
            try
            {                
                if (WindowRoster.resetRosterSize)
                {
                    WindowRoster.Position.height = 330; //reset height
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerAddon.Start.  " + ex.ToString(), "Error", true);
            }
        }

        internal void OnGUI()
        {
            //Debug.Log("[RosterManager]:  RosterManagerAddon.OnGUI");
            try
            {
                Display();

                Utilities.ShowToolTips();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerAddon.OnGUI.  " + ex.ToString(), "Error", true);
            }
        }

        internal void Update()
        {
            try
            {
                refreshCrew(HighLogic.LoadedScene);
                CheckForToolbarTypeToggle();
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in Update (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
            }
        }

        private void DummyVoid()
        { }

        //Vessel state handlers
        internal void OnDestroy()
        {
            //Debug.Log("[RosterManager]:  RosterManagerAddon.OnDestroy");
            try
            {                
                if (RMSettings.Loaded)
                {
                    RMSettings.SaveSettings();
                }

                // Handle Toolbars
                if (RMRoster_Blizzy == null)
                {
                    if (RMRoster_Stock != null)
                    {
                        ApplicationLauncher.Instance.RemoveModApplication(RMRoster_Stock);
                        RMRoster_Stock = null;
                    }
                    if (RMRoster_Stock == null)
                    {
                        // Remove the stock toolbar button launcher handler
                        GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    }
                }
                else
                {
                    if (RMRoster_Blizzy != null)
                        RMRoster_Blizzy.Destroy();
                }
                //Reset Roster Window data
                WindowRoster.OnCreate = false;
                WindowRoster.SelectedKerbal = null;
                WindowRoster.ToolTip = "";
                //Settings.ShowRoster = false;
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerAddon.OnDestroy.  " + ex.ToString(), "Error", true);
            }
        }

        internal void refreshCrew(GameScenes scene)
        {            
            if (scene == GameScenes.EDITOR || scene == GameScenes.FLIGHT || scene == GameScenes.SPACECENTER || scene == GameScenes.TRACKSTATION)
            {
                RMAddon.FrozenKerbals.Clear();
                AllCrew.Clear();
                RMAddon.FrozenKerbals = WindowRoster.GetFrozenKerbals();
                if (RMLifeSpan.Instance != null)
                    AllCrew = RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.ToList();                
            }            
        }

        // Stock vs Blizzy Toolbar switch handler
        private void CheckForToolbarTypeToggle()
        {
            if (RMSettings.EnableBlizzyToolbar && !RMSettings.prevEnableBlizzyToolbar)
            {
                // Let't try to use Blizzy's toolbar
                Utilities.LogMessage("CheckForToolbarToggle - Blizzy Toolbar Selected.", "Info", RMSettings.VerboseLogging);
                if (!ActivateBlizzyToolBar())
                {
                    // We failed to activate the toolbar, so revert to stock
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

                    Utilities.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
                    RMSettings.EnableBlizzyToolbar = RMSettings.prevEnableBlizzyToolbar;
                }
                else
                {
                    OnGUIAppLauncherDestroyed();
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
                    RMSettings.prevEnableBlizzyToolbar = RMSettings.EnableBlizzyToolbar;
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        RMRoster_Blizzy.Visible = true;
                    }
                }
            }
            else if (!RMSettings.EnableBlizzyToolbar && RMSettings.prevEnableBlizzyToolbar)
            {
                // Use stock Toolbar
                Utilities.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    RMRoster_Blizzy.Visible = false;
                }
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                OnGUIAppLauncherReady();
                RMSettings.prevEnableBlizzyToolbar = RMSettings.EnableBlizzyToolbar;
            }
        }

        // Stock Toolbar Startup and cleanup
        private void OnGUIAppLauncherReady()
        {
            Utilities.LogMessage("RosterManagerAddon.OnGUIAppLauncherReady active...", "Info", RMSettings.VerboseLogging);
            try
            {
                // Setup Roster Button
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER && RMRoster_Stock == null && !RMSettings.EnableBlizzyToolbar)
                {
                    string Iconfile = "Icon_Off_38";
                    RMRoster_Stock = ApplicationLauncher.Instance.AddModApplication(
                        OnRMRosterToggle,
                        OnRMRosterToggle,
                        DummyVoid,
                        DummyVoid,
                        DummyVoid,
                        DummyVoid,
                        ApplicationLauncher.AppScenes.SPACECENTER,
                        (Texture)GameDatabase.Instance.GetTexture(TextureFolder + Iconfile, false));

                    if (WindowRoster.ShowWindow)
                        RMRoster_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowRoster.ShowWindow ? TextureFolder + "Icon_On_38" : TextureFolder + "Icon_Off_38", false));
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerAddon.OnGUIAppLauncherReady.  " + ex.ToString(), "Error", true);
            }
        }

        private void OnGUIAppLauncherDestroyed()
        {
            //Debug.Log("[RosterManager]:  RosterManagerAddon.OnGUIAppLauncherDestroyed");
            try
            {
                if (RMRoster_Stock != null)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(RMRoster_Stock);
                    RMRoster_Stock = null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerAddon.OnGUIAppLauncherDestroyed.  " + ex.ToString(), "Error", true);
            }
        }

        //Toolbar button click handlers
        internal static void OnRMRosterToggle()
        {
            //Debug.Log("[RosterManager]:  RosterManagerAddon.OnSMRosterToggleOn");
            try
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    WindowRoster.ShowWindow = !WindowRoster.ShowWindow;
                    if (RMSettings.EnableBlizzyToolbar)
                        RMRoster_Blizzy.TexturePath = WindowRoster.ShowWindow ? TextureFolder + "Icon_On_24" : TextureFolder + "Icon_Off_24";
                    else
                        RMRoster_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowRoster.ShowWindow ? TextureFolder + "Icon_On_38" : TextureFolder + "Icon_Off_38", false));

                    RMAddon.FrozenKerbals = WindowRoster.GetFrozenKerbals();
                    AllCrew.Clear();
                    if (RMLifeSpan.Instance != null)
                        AllCrew = RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.ToList();                    
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  RosterManagerAddon.OnSMRosterToggleOn.  " + ex.ToString(), "Error", true);
            }
        }

        #endregion Event handlers

        #region Action Methods

        internal static bool ActivateBlizzyToolBar()
        {
            if (RMSettings.EnableBlizzyToolbar)
            {
                try
                {
                    if (ToolbarManager.ToolbarAvailable)
                    {
                        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                        {
                            RMRoster_Blizzy = ToolbarManager.Instance.add("RosterManager", "Roster");
                            RMRoster_Blizzy.TexturePath = WindowSettings.ShowWindow ? TextureFolder + "Icon_On_24" : TextureFolder + "Icon_Off_24";
                            RMRoster_Blizzy.ToolTip = "Roster Manager Roster Window";
                            RMRoster_Blizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
                            RMRoster_Blizzy.Visible = true;
                            RMRoster_Blizzy.OnClick += (e) =>
                            {
                                OnRMRosterToggle();
                            };
                        }
                        Utilities.LogMessage("Blizzy Toolbar available!", "Info", RMSettings.VerboseLogging);
                        return true;
                    }
                    else
                    {
                        Utilities.LogMessage("Blizzy Toolbar not available!", "Info", RMSettings.VerboseLogging);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // Blizzy Toolbar instantiation error.
                    Utilities.LogMessage("Error in EnableBlizzyToolbar... Error:  " + ex, "Error", true);
                    return false;
                }
            }
            else
            {
                // No Blizzy Toolbar
                Utilities.LogMessage("Blizzy Toolbar not Enabled...", "Info", RMSettings.VerboseLogging);
                return false;
            }
        }

        internal void Display()
        {
            string step = "";
            try
            {
                step = "0 - Start";
                RMStyle.SetupGUI();

                if (WindowDebugger.ShowWindow)
                {
                    step = "2 - Debugger";
                    WindowDebugger.Position = GUILayout.Window(318643, WindowDebugger.Position, WindowDebugger.Display, "Roster Manager -  Debug Console - Ver. " + RMSettings.CurVersion, GUILayout.MinHeight(20));

                }

                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    if (WindowSettings.ShowWindow)
                    {
                        step = "3 - Show Settings";
                        WindowSettings.Position = GUILayout.Window(318546, WindowSettings.Position, WindowSettings.Display, "Roster Manager Settings", GUILayout.MinHeight(20));
                    }

                    if (WindowContractDispute.ShowWindow)
                    {
                        step = "4 - Contract Disputes";
                        WindowContractDispute.Position = GUILayout.Window(318987, WindowContractDispute.Position, WindowContractDispute.Display, "Contract Disputes", GUILayout.MinHeight(20));
                    }

                    if (WindowRoster.ShowWindow)
                    {
                        if (WindowRoster.resetRosterSize)
                        {
                            step = "5 - Reset Roster Size";
                            WindowRoster.Position.height = 330; //reset hight
                        }

                        step = "6 - Show Roster";
                        WindowRoster.Position = GUILayout.Window(318547, WindowRoster.Position, WindowRoster.Display, "Roster Manager", GUILayout.MinHeight(20));
                    }
                }
                step = "1 - Show Interface(s)";
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in drawGui at or near step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #endregion Action Methods

        #region Highlighting methods

        /// <summary>
        /// Remove highlighting on a part.
        /// </summary>
        /// <param name="part">Part to remove highlighting from.</param>
        internal static void ClearPartHighlight(Part part)
        {
            try
            {
                if (part != null)
                {
                    part.SetHighlight(false, false);
                    part.SetHighlightDefault();
                    part.highlightType = Part.HighlightType.OnMouseOver;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  ClearPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal static void SetPartHighlight(Part part, Color color)
        {
            try
            {
                if (part != null)
                {
                    part.SetHighlightColor(color);
                    part.SetHighlight(true, false);
                    part.highlightType = Part.HighlightType.AlwaysOn;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  SetPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #endregion Highlighting methods

        internal static bool CrewPartIsFull(Part part)
        {
            return !(part.protoModuleCrew.Count < part.CrewCapacity);
        }

        internal static bool VesselIsFull(Vessel vessel)
        {
            return !(vessel.GetCrewCount() < vessel.GetCrewCapacity());
        }

        internal static Part FindKerbalPart(ProtoCrewMember pKerbal)
        {
            Part kPart = FlightGlobals.ActiveVessel.Parts.Find(x => x.protoModuleCrew.Find(y => y == pKerbal) != null);
            return kPart;
        }

        internal static void RespawnKerbal(ProtoCrewMember kerbal)
        {
            kerbal.SetTimeForRespawn(0);
            // This call causes issues in KSC scene, and is not needed.
            //kerbal.Spawn();
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            HighLogic.CurrentGame.CrewRoster.GetNextAvailableKerbal();
        }

        internal void RespawnCrew(Vessel vessel)
        {
            vessel.SpawnCrew();
            RMAddon.FireEventTriggers(vessel);
        }

        internal void FillVesselCrew(Vessel vessel)
        {
            foreach (var part in vessel.parts)
            {
                if (part.CrewCapacity > 0)
                    FillPartCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
            }
            RMAddon.FireEventTriggers(vessel);
        }

        internal void EmptyVesselCrew(Vessel vessel)
        {
            foreach (var part in vessel.parts)
            {
                if (part.CrewCapacity > 0)
                {
                    for (int i = part.protoModuleCrew.Count - 1; i >= 0; i--)
                    {
                        RMAddon.RemoveCrewMember(part.protoModuleCrew[i], part);
                    }
                    RMAddon.FireEventTriggers(vessel);
                }
            }
        }

        private void FillPartCrew(int count, Part part)
        {
            if (!CrewPartIsFull(part))
            {
                for (int i = 0; i < part.CrewCapacity && i < count; i++)
                {
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNextOrNewKerbal();
                    part.AddCrewmember(kerbal);

                    if (kerbal.seat != null)
                        kerbal.seat.SpawnCrew();
                }
            }
        }

        internal static void AddCrewMember(ProtoCrewMember pKerbal, Vessel vessel)
        {
            foreach (Part part in vessel.parts)
            {
                if (part.CrewCapacity > 0 && !RMAddon.CrewPartIsFull(part))
                {
                    part.AddCrewmember(pKerbal);
                    pKerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
                    if (part.internalModel != null)
                    {
                        if (pKerbal.seat != null)
                            pKerbal.seat.SpawnCrew();
                    }
                    RMAddon.FireEventTriggers(part.vessel);
                    break;
                }
            }
        }

        internal static void AddCrewMember(ProtoCrewMember pKerbal, Part part)
        {
            part.AddCrewmember(pKerbal);
            pKerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            if (part.internalModel != null)
            {
                if (pKerbal.seat != null)
                    pKerbal.seat.SpawnCrew();
            }
            RMAddon.FireEventTriggers(part.vessel);
        }

        internal static void RemoveCrewMember(ProtoCrewMember pKerbal, Part part)
        {
            part.RemoveCrewmember(pKerbal);
            pKerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            RMAddon.FireEventTriggers(part.vessel);
        }

        internal static void FireEventTriggers(Vessel vessel)
        {
            // Per suggestion by shaw (http://forum.kerbalspaceprogram.com/threads/62270?p=1033866&viewfull=1#post1033866)
            // and instructions for using CLS API by codepoet.
            Utilities.LogMessage("FireEventTriggers:  Active.", "info", RMSettings.VerboseLogging);
            GameEvents.onVesselChange.Fire(vessel);
        }

        internal static bool IsPreLaunch
        {
            get
            {
                if (FlightGlobals.ActiveVessel != null)
                    return FlightGlobals.ActiveVessel.landedAt == "LaunchPad" || FlightGlobals.ActiveVessel.landedAt == "Runway";
                else return false;
            }
        }
    }

    internal class RosterManagerModule : PartModule
    {
        [KSPEvent(guiActive = true, guiName = "Destroy Part", active = true)]
        internal void DestoryPart()
        {
            if (this.part != null)
                this.part.temperature = 5000;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (this.part != null && part.name == "RosterManager")
                Events["Destroy Part"].active = true;
            else
                Events["Destroy Part"].active = false;
        }
    }
}