using System;
using System.Collections.Generic;
using System.Linq;
using RosterManager.Api;
using RosterManager.Windows;
using UnityEngine;
using KSP.Localization;
using KSP.UI.Screens;
using RosterManager.InternalObjects;

namespace RosterManager
{
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  internal class RMAddon : MonoBehaviour
  {
    #region Properties

    internal static string TextureFolder = "RosterManager/Textures/";
    internal static string SaveMessage = string.Empty;

    internal static double Timestamp = 0.0;

    // Toolbar Integration.
    private static IButton _rmRosterBlizzy;

    private static ApplicationLauncherButton _rmRosterStock;
    internal static bool FrameErrTripped;

    internal static List<KeyValuePair<string, RMKerbal>> AllCrew = new List<KeyValuePair<string, RMKerbal>>();

    // DeepFreeze Frozen Crew interface
    internal static Dictionary<string, DfWrapper.KerbalInfo> FrozenKerbals = new Dictionary<string, DfWrapper.KerbalInfo>();

    #endregion Properties

    #region Event handlers

    // Addon state event handlers
    internal void Awake()
    {
      try
      {
        if (!IsCorrectSceneLoaded()) return;
        //DontDestroyOnLoad(this);
        RMSettings.ApplySettings();
        //WindowRoster.ResetKerbalProfessions();
        RmUtils.LogMessage("RosterManagerAddon.Awake Active...", "info", RMSettings.VerboseLogging);

        if (RMSettings.EnableBlizzyToolbar)
        {
          // Let't try to use Blizzy's toolbar
          RmUtils.LogMessage("RosterManagerAddon.Awake - Blizzy Toolbar Selected.", "Info", RMSettings.VerboseLogging);
          if (!ActivateBlizzyToolBar())
          {
            // We failed to activate the toolbar, so revert to stock
            if (ApplicationLauncher.Ready)
            {
              OnGuiAppLauncherReady();
            }
            else
              GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
            RmUtils.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
          }
        }
        else
        {
          // Use stock Toolbar
          RmUtils.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
          if (ApplicationLauncher.Ready)
          {
            OnGuiAppLauncherReady();
          }
          else
            GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
        }
        // lets add our event handlers for kerbal actions
        //GameEvents.onKerbalAdded.Add(OnKerbalAdded);
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage("Error in:  RosterManagerAddon.Awake.  Error:  " + ex, "Error", true);
      }
    }

    internal void Start()
    {
      RmUtils.LogMessage("RosterManagerAddon.Start.", "Info", RMSettings.VerboseLogging);
      try
      {
        if (WindowRoster.ResetRosterSize)
        {
          WindowRoster.Position.height = WindowRoster.WindowHeight; //reset height
        }
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage("Error in:  RosterManagerAddon.Start.  " + ex, "Error", true);
      }
    }

    // ReSharper disable once InconsistentNaming
    internal void OnGUI()
    {
      //Debug.Log("[RosterManager]:  RosterManagerAddon.OnGUI");
      if (Event.current.type == EventType.MouseUp)
      {
          // Turn off window resizing
          RmUtils.ResetResize();
      }
      try
      {
        Display();

        RMToolTips.ShowToolTips();
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage("Error in:  RosterManagerAddon.OnGUI.  " + ex, "Error", true);
      }
    }

    internal void Update()
    {
      try
      {
        CheckForToolbarTypeToggle();
      }
      catch (Exception ex)
      {
        if (!FrameErrTripped)
        {
          RmUtils.LogMessage($" in Update (repeating error).  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", "Error", true);
          FrameErrTripped = true;
        }
      }
    }

    private static void DummyVoid()
    { }

    //Vessel state handlers
    internal void OnDestroy()
    {
      //Debug.Log("[RosterManager]:  RosterManagerAddon.OnDestroy");
      try
      {
        if (RMSettings.Loaded)
        {
          try
          {
            RMSettings.SaveSettings();
          }
          catch
          {
            // Do nothing.
          }
        }

        // Handle Toolbars
        if (_rmRosterBlizzy == null)
        {
          if (_rmRosterStock != null)
          {
            ApplicationLauncher.Instance.RemoveModApplication(_rmRosterStock);
            _rmRosterStock = null;
          }
          if (_rmRosterStock == null)
          {
            // Remove the stock toolbar button launcher handler
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
          }
        }
        else
        {
          _rmRosterBlizzy?.Destroy();
        }
        //Reset Roster Window data
        WindowRoster.DisplayMode = WindowRoster.DisplayModes.None;
        WindowRoster.SelectedKerbal = null;
        WindowRoster.ToolTip = "";
        //Settings.ShowRoster = false;
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage("Error in:  RosterManagerAddon.OnDestroy.  " + ex, "Error", true);
      }
    }

    internal static void RefreshCrew()
    {
      if (!IsCorrectSceneLoaded()) return;
      //RMAddon.FrozenKerbals.Clear();
      AllCrew.Clear();
      FrozenKerbals = WindowRoster.GetFrozenKerbals();
      if (RMLifeSpan.Instance != null)
        AllCrew = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.ToList();
    }

    // Stock vs Blizzy Toolbar switch handler
    private void CheckForToolbarTypeToggle()
    {
      if (RMSettings.EnableBlizzyToolbar && !RMSettings.PrevEnableBlizzyToolbar)
      {
        // Let't try to use Blizzy's toolbar
        RmUtils.LogMessage("CheckForToolbarToggle - Blizzy Toolbar Selected.", "Info", RMSettings.VerboseLogging);
        if (!ActivateBlizzyToolBar())
        {
          // We failed to activate the toolbar, so revert to stock
          GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
          RmUtils.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
          RMSettings.EnableBlizzyToolbar = RMSettings.PrevEnableBlizzyToolbar;
        }
        else
        {
          OnGuiAppLauncherDestroyed();
          GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
          RMSettings.PrevEnableBlizzyToolbar = RMSettings.EnableBlizzyToolbar;
          if (IsCorrectSceneLoaded())
          {
            _rmRosterBlizzy.Visible = true;
          }
        }
      }
      else if (!RMSettings.EnableBlizzyToolbar && RMSettings.PrevEnableBlizzyToolbar)
      {
        // Use stock Toolbar
        RmUtils.LogMessage("RosterManagerAddon.Awake - Stock Toolbar Selected.", "Info", RMSettings.VerboseLogging);
        if (IsCorrectSceneLoaded())
        {
          _rmRosterBlizzy.Visible = false;
        }
        GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
        OnGuiAppLauncherReady();
        RMSettings.PrevEnableBlizzyToolbar = RMSettings.EnableBlizzyToolbar;
      }
    }

    // Stock Toolbar Startup and cleanup
    private void OnGuiAppLauncherReady()
    {
      RmUtils.LogMessage("RosterManagerAddon.OnGUIAppLauncherReady active...", "Info", RMSettings.VerboseLogging);
      try
      {
        // Setup Roster Button
        if (!IsCorrectSceneLoaded() || _rmRosterStock != null || RMSettings.EnableBlizzyToolbar) return;
        const string iconfile = "Icon_Off_128";
        _rmRosterStock = ApplicationLauncher.Instance.AddModApplication(
          OnRMRosterToggle,
          OnRMRosterToggle,
          DummyVoid,
          DummyVoid,
          DummyVoid,
          DummyVoid,
          ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT |
          ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB |
          ApplicationLauncher.AppScenes.TRACKSTATION,
          GameDatabase.Instance.GetTexture(TextureFolder + iconfile, false));

        if (WindowRoster.ShowWindow)
          _rmRosterStock.SetTexture(GameDatabase.Instance.GetTexture(WindowRoster.ShowWindow ? TextureFolder + "Icon_On_128" : TextureFolder + "Icon_Off_128", false));
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage("Error in:  RosterManagerAddon.OnGUIAppLauncherReady.  " + ex, "Error", true);
      }
    }

    private void OnGuiAppLauncherDestroyed()
    {
      //Debug.Log("[RosterManager]:  RosterManagerAddon.OnGUIAppLauncherDestroyed");
      try
      {
        if (_rmRosterStock == null) return;
        ApplicationLauncher.Instance.RemoveModApplication(_rmRosterStock);
        _rmRosterStock = null;
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage("Error in:  RosterManagerAddon.OnGUIAppLauncherDestroyed.  " + ex, "Error", true);
      }
    }

    //Toolbar button click handlers
    internal static void OnRMRosterToggle()
    {
      //Debug.Log("[RosterManager]:  RosterManagerAddon.OnRMRosterToggle");
      try
      {
        if (!IsCorrectSceneLoaded()) return;
        WindowRoster.ShowWindow = !WindowRoster.ShowWindow;
        if (RMSettings.EnableBlizzyToolbar)
          _rmRosterBlizzy.TexturePath = WindowRoster.ShowWindow ? TextureFolder + "Icon_On_24" : TextureFolder + "Icon_Off_24";
        else
          _rmRosterStock.SetTexture(GameDatabase.Instance.GetTexture(WindowRoster.ShowWindow ? TextureFolder + "Icon_On_128" : TextureFolder + "Icon_Off_128", false));

        if (!WindowRoster.ShowWindow) return;
        WindowRoster.DisplayMode = WindowRoster.DisplayModes.None;
        WindowRoster.UpdateRosterList();
        //AllCrew.Clear();
        //if (RMLifeSpan.Instance != null)
        //    AllCrew = RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.ToList();
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage("Error in:  RosterManagerAddon.OnRMRosterToggle.  " + ex, "Error", true);
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
            if (IsCorrectSceneLoaded())
            {
              _rmRosterBlizzy = ToolbarManager.Instance.add("RosterManager", "Roster");
              _rmRosterBlizzy.TexturePath = WindowSettings.ShowWindow ? TextureFolder + "Icon_On_24" : TextureFolder + "Icon_Off_24";
              _rmRosterBlizzy.ToolTip = Localizer.Format("#autoLOC_RM_1000");		// #autoLOC_RM_1000 = Roster Manager Roster Window
              _rmRosterBlizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
              _rmRosterBlizzy.Visible = true;
              _rmRosterBlizzy.OnClick += e =>
              {
                OnRMRosterToggle();
              };
            }
            RmUtils.LogMessage("Blizzy Toolbar available!", "Info", RMSettings.VerboseLogging);
            return true;
          }
          RmUtils.LogMessage("Blizzy Toolbar not available!", "Info", RMSettings.VerboseLogging);
          return false;
        }
        catch (Exception ex)
        {
          // Blizzy Toolbar instantiation error.
          RmUtils.LogMessage("Error in EnableBlizzyToolbar... Error:  " + ex, "Error", true);
          return false;
        }
      }
      // No Blizzy Toolbar
      RmUtils.LogMessage("Blizzy Toolbar not Enabled...", "Info", RMSettings.VerboseLogging);
      return false;
    }

    internal void Display()
    {
      string step = "";
      try
      {
        step = "0 - Start";
        RMStyle.SetupGui();

        if (WindowDebugger.ShowWindow)
        {
          step = "2 - Debugger";
          WindowDebugger.Position = GUILayout.Window(318643, WindowDebugger.Position, WindowDebugger.Display, $"{Localizer.Format("#autoLOC_RM_1001")} {RMSettings.CurVersion}", GUILayout.MinHeight(20));		// #autoLOC_RM_1001 = Roster Manager -  Debug Console - Ver. 
        }

        if (IsCorrectSceneLoaded())
        {
          if (WindowSettings.ShowWindow)
          {
            step = "3 - Show Settings";
            WindowSettings.Position = GUILayout.Window(318546, WindowSettings.Position, WindowSettings.Display, Localizer.Format("#autoLOC_RM_1002"), GUILayout.MinHeight(20));		// #autoLOC_RM_1002 = Roster Manager Settings
          }

          if (WindowContracts.ShowWindow)
          {
            step = "4 - Roster Contracts";
            WindowContracts.Position = GUILayout.Window(318987, WindowContracts.Position, WindowContracts.Display, Localizer.Format("#autoLOC_RM_1003"), GUILayout.MinHeight(20));		// #autoLOC_RM_1003 = Roster Contracts
          }

          if (WindowRoster.ShowWindow)
          {
            if (WindowRoster.DisplayMode == WindowRoster.DisplayModes.None)
            {
              step = "5 - Reset Roster Size";
              WindowRoster.Position.height = WindowRoster.WindowHeight + WindowRoster.HeightScale; //reset height
            }

            step = "6 - Show Roster";
            WindowRoster.Position = GUILayout.Window(318547, WindowRoster.Position, WindowRoster.Display, Localizer.Format("#autoLOC_RM_1004"), GUILayout.MinHeight(20));		// #autoLOC_RM_1004 = Roster Manager
          }
        }
        step = "1 - Show Interface(s)";
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage($" in drawGui at or near step:  {step}.  Error:  {ex.Message} \n\n{ex.StackTrace}", "Error", true);
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
        if (part == null) return;
        part.SetHighlight(false, false);
        part.SetHighlightDefault();
        part.highlightType = Part.HighlightType.OnMouseOver;
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage($" in  ClearPartHighlight.  Error:  {ex.Message} \n\n{ex.StackTrace}", "Error", true);
      }
    }

    internal static void SetPartHighlight(Part part, Color color)
    {
      try
      {
        if (part == null) return;
        part.SetHighlightColor(color);
        part.SetHighlight(true, false);
        part.highlightType = Part.HighlightType.AlwaysOn;
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage($" in  SetPartHighlight.  Error:  {ex.Message} \n\n{ex.StackTrace}", "Error", true);
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
      FireEventTriggers(vessel);
    }

    internal void FillVesselCrew(Vessel vessel)
    {
      foreach (Part part in vessel.parts.Where(part => part.CrewCapacity > 0))
      {
        FillPartCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
      }
      FireEventTriggers(vessel);
    }

    internal void EmptyVesselCrew(Vessel vessel)
    {
      foreach (Part part in vessel.parts.Where(part => part.CrewCapacity > 0))
      {
        for (int i = part.protoModuleCrew.Count - 1; i >= 0; i--)
        {
          RemoveCrewMember(part.protoModuleCrew[i], part);
        }
        FireEventTriggers(vessel);
      }
    }

    private static void FillPartCrew(int count, Part part)
    {
      if (CrewPartIsFull(part)) return;
      for (int i = 0; i < part.CrewCapacity && i < count; i++)
      {
        ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNextOrNewKerbal();
        part.AddCrewmember(kerbal);

        if (kerbal.seat != null)
          kerbal.seat.SpawnCrew();
      }
    }

    internal static void AddCrewMember(ProtoCrewMember pKerbal, Vessel vessel)
    {
      foreach (Part part in vessel.parts.Where(part => part.CrewCapacity > 0 && !CrewPartIsFull(part)))
      {
        part.AddCrewmember(pKerbal);
        pKerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
        if (part.internalModel != null)
        {
          if (pKerbal.seat != null)
            pKerbal.seat.SpawnCrew();
        }
        FireEventTriggers(part.vessel);
        break;
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
      FireEventTriggers(part.vessel);
    }

    internal static void RemoveCrewMember(ProtoCrewMember pKerbal, Part part)
    {
      part.RemoveCrewmember(pKerbal);
      pKerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
      FireEventTriggers(part.vessel);
    }

    internal static void FireEventTriggers(Vessel vessel)
    {
      // Per suggestion by shaw (http://forum.kerbalspaceprogram.com/threads/62270?p=1033866&viewfull=1#post1033866)
      // and instructions for using CLS API by codepoet.
      RmUtils.LogMessage("FireEventTriggers:  Active.", "info", RMSettings.VerboseLogging);
      WindowRoster.UpdateRosterList();
      GameEvents.onVesselChange.Fire(vessel);
    }

    internal static bool IsPreLaunch
    {
      get
      {
        if (FlightGlobals.ActiveVessel != null)
          return FlightGlobals.ActiveVessel.landedAt == "LaunchPad" || FlightGlobals.ActiveVessel.landedAt == "Runway";
        return false;
      }
    }

    internal static bool IsCorrectSceneLoaded()
    {
      return HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.EDITOR ||
             HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT;
    }

  }

  internal class RosterManagerModule : PartModule
  {
    [KSPEvent(guiActive = true, guiName = "Destroy Part", active = true)]
    internal void DestoryPart()
    {
      if (part != null)
        part.temperature = 5000;
    }

    public override void OnUpdate()
    {
      base.OnUpdate();

      if (part != null && part.name == "RosterManager")
        Events["Destroy Part"].active = true;
      else
        Events["Destroy Part"].active = false;
    }

  }
}