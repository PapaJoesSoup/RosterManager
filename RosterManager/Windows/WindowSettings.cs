using System.Globalization;
using RosterManager.Api;
using RosterManager.InternalObjects;
using UnityEngine;

namespace RosterManager.Windows
{
  internal static class WindowSettings
  {
    #region Settings Window (GUI)

    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";

    private static Vector2 _scrollViewerPosition = Vector2.zero;

    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      var rect = new Rect(371, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
      {
        ToolTip = "";
        RMSettings.RestoreTempSettings();
        ShowWindow = false;
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Store settings in case we cancel later...
      //RMSettings.StoreTempSettings();

      GUILayout.BeginVertical();
      _scrollViewerPosition = GUILayout.BeginScrollView(_scrollViewerPosition, GUILayout.Height(280), GUILayout.Width(375));
      GUILayout.BeginVertical();

      DisplayOptions();

      DisplayHighlighting();

      DisplayToolTips();

      DisplayConfiguration();

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Save"))
      {
        //If EnableAging has been turned ON when it was previously OFF, we reset age processing, otherwise they could all die instantly.
        if (RMLifeSpan.Instance.RMGameSettings.EnableAging && RMSettings.PrevEnableAging == false)
        {
          Utilities.LogMessage("RosterManagerWindowSettings.Display Save settings, aging has been enabled. Reset all birthdays.", "info", RMSettings.VerboseLogging);
          var currentTime = Planetarium.GetUniversalTime();
          foreach (var rmkerbal in RMLifeSpan.Instance.RMKerbals.AllrmKerbals)
          {
            rmkerbal.Value.TimelastBirthday = currentTime;
            rmkerbal.Value.TimeNextBirthday = RMKerbal.BirthdayNextDue(currentTime);
          }
        }
        //If EnableSalaries has been turned OFF when it was previously ON, reset any kerbals from tourist back to active.
        if (!RMLifeSpan.Instance.RMGameSettings.EnableSalaries && RMSettings.PrevEnableSalaries)
        {
          Utilities.LogMessage("RosterManagerWindowSettings.Display Save settings, salaries have been turned off. Reset all salary related fields for all kerbals.", "info", RMSettings.VerboseLogging);
          foreach (var rmkerbal in RMLifeSpan.Instance.RMKerbals.AllrmKerbals)
          {
            if (rmkerbal.Value.Type == ProtoCrewMember.KerbalType.Tourist && rmkerbal.Value.Kerbal.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
            {
              rmkerbal.Value.Type = ProtoCrewMember.KerbalType.Crew;
              rmkerbal.Value.Kerbal.type = ProtoCrewMember.KerbalType.Crew;
              rmkerbal.Value.Trait = rmkerbal.Value.RealTrait;
              rmkerbal.Value.Kerbal.trait = rmkerbal.Value.RealTrait;
              KerbalRoster.SetExperienceTrait(rmkerbal.Value.Kerbal, rmkerbal.Value.Trait);
              RMKerbal.RegisterExperienceTrait(rmkerbal.Value);
            }
            rmkerbal.Value.SalaryContractDispute = false;
            rmkerbal.Value.SalaryContractDisputePeriods = 0;
            rmkerbal.Value.SalaryContractDisputeProcessed = true;
          }
        }
        //If EnableSalaries has been turned ON when it was previously OFF, reset all kerbals salary time to now.
        if (RMLifeSpan.Instance.RMGameSettings.EnableSalaries && RMSettings.PrevEnableSalaries == false)
        {
          Utilities.LogMessage("RosterManagerWindowSettings.Display Save settings, salaries have been turned on. Reset all salary related fields for all kerbals.", "info", RMSettings.VerboseLogging);
          var currentTime = Planetarium.GetUniversalTime();
          foreach (var rmkerbal in RMLifeSpan.Instance.RMKerbals.AllrmKerbals)
          {
            rmkerbal.Value.Timelastsalary = currentTime;
            rmkerbal.Value.TimeSalaryDue = RMKerbal.SalaryNextDue(currentTime);
          }
        }
        RMSettings.SaveSettings();
        ShowWindow = false;
      }
      if (GUILayout.Button("Cancel"))
      {
        // We've canclled, so restore original settings.
        RMSettings.RestoreTempSettings();
        ShowWindow = false;
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();

      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      RMSettings.RepositionWindows("WindowSettings");
    }

    private static void DisplayConfiguration()
    {
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
      GUILayout.Label("Configuraton");
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

      if (!ToolbarManager.ToolbarAvailable)
      {
        if (RMSettings.EnableBlizzyToolbar)
          RMSettings.EnableBlizzyToolbar = false;
        GUI.enabled = false;
      }
      else
        GUI.enabled = true;

      var label = "Enable Blizzy Toolbar (Replaces Stock Toolbar)";
      RMSettings.EnableBlizzyToolbar = GUILayout.Toggle(RMSettings.EnableBlizzyToolbar, label, GUILayout.Width(300));

      GUI.enabled = true;
      label = "Enable Debug Window";
      WindowDebugger.ShowWindow = GUILayout.Toggle(WindowDebugger.ShowWindow, label, GUILayout.Width(300));

      label = "Enable Verbose Logging";
      RMSettings.VerboseLogging = GUILayout.Toggle(RMSettings.VerboseLogging, label, GUILayout.Width(300));

      label = "Enable RM Debug Window On Error";
      RMSettings.AutoDebug = GUILayout.Toggle(RMSettings.AutoDebug, label, GUILayout.Width(300));

      label = "Save Error log on Exit";
      RMSettings.SaveLogOnExit = GUILayout.Toggle(RMSettings.SaveLogOnExit, label, GUILayout.Width(300));

      // create Limit Error Log Length slider;
      GUILayout.BeginHorizontal();
      GUILayout.Label("Error Log Length: ", GUILayout.Width(140));
      RMSettings.ErrorLogLength = GUILayout.TextField(RMSettings.ErrorLogLength, GUILayout.Width(40));
      GUILayout.Label("(lines)", GUILayout.Width(50));
      GUILayout.EndHorizontal();

      label = "Enable Kerbal Renaming";
      var toolTip = "Allows renaming a Kerbal.";
      RMSettings.EnableKerbalRename = GUILayout.Toggle(RMSettings.EnableKerbalRename, new GUIContent(label, toolTip), GUILayout.Width(300));
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      label = "Enable Kerbal Aging";
      toolTip = "Your Kerbals will age and eventually die from old age.";
      RMLifeSpan.Instance.RMGameSettings.EnableAging = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.EnableAging, new GUIContent(label, toolTip), GUILayout.Width(300));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      if (!RMLifeSpan.Instance.RMGameSettings.EnableAging)
        GUI.enabled = false;
      GUILayout.BeginHorizontal();
      toolTip = "Average age of new Applicant Kerbals.";
      GUILayout.Label(new GUIContent("Kerbal Minimum Age: ", toolTip), GUILayout.Width(140));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      var strMinimumAge = RMLifeSpan.Instance.RMGameSettings.MinimumAge.ToString();
      int minimumAge;
      strMinimumAge = GUILayout.TextField(strMinimumAge, GUILayout.Width(40));
      GUILayout.Label("(Years)", GUILayout.Width(50));
      if (int.TryParse(strMinimumAge, out minimumAge))
        RMLifeSpan.Instance.RMGameSettings.MinimumAge = minimumAge;
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      toolTip = "Average Lifespan of Kerbals (how long they live).";
      GUILayout.Label(new GUIContent("Kerbal Lifespan: ", toolTip), GUILayout.Width(140));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      var strMaximumAge = RMLifeSpan.Instance.RMGameSettings.MaximumAge.ToString();
      int maximumAge;
      strMaximumAge = GUILayout.TextField(strMaximumAge, GUILayout.Width(40));
      GUILayout.Label("(Years)", GUILayout.Width(50));
      if (int.TryParse(strMaximumAge, out maximumAge))
        RMLifeSpan.Instance.RMGameSettings.MaximumAge = maximumAge;
      GUILayout.EndHorizontal();

      GUI.enabled = true;
      if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
        GUI.enabled = false;
      label = "Enable Salaries (career game only)";
      toolTip = "Kerbals must be paid Salaries.";
      RMLifeSpan.Instance.RMGameSettings.EnableSalaries = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.EnableSalaries, new GUIContent(label, toolTip), GUILayout.Width(300));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      if (!RMLifeSpan.Instance.RMGameSettings.EnableSalaries)
        GUI.enabled = false;
      DisplaySelectSalaryPeriod();

      GUILayout.BeginHorizontal();
      toolTip = "Default salary for each Kerbal per each salary period.";
      GUILayout.Label(new GUIContent("Default Kerbal Salary: ", toolTip), GUILayout.Width(140));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      var strDefSalary = RMLifeSpan.Instance.RMGameSettings.DefaultSalary.ToString(CultureInfo.InvariantCulture);
      double defaultSalary;
      strDefSalary = GUILayout.TextField(strDefSalary, GUILayout.Width(70));
      GUILayout.Label("(Funds/per salary period)", GUILayout.Width(120));
      GUILayout.EndHorizontal();
      if (double.TryParse(strDefSalary, out defaultSalary))
        RMLifeSpan.Instance.RMGameSettings.DefaultSalary = defaultSalary;

      GUI.enabled = true;
      if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
        GUI.enabled = false;

      label = "Charge funds for Profession Change (career game only)";
      toolTip = "Charge funds for changing a Kerbal's profession.";
      RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge, new GUIContent(label, toolTip), GUILayout.Width(320));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      if (!RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge)
        GUI.enabled = false;

      GUILayout.BeginHorizontal();
      toolTip = "Cost of changing a Kerbals Profession.";
      GUILayout.Label(new GUIContent("Change Profession Cost: ", toolTip), GUILayout.Width(140));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      var strChgProf = RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCost.ToString(CultureInfo.InvariantCulture);
      double chgProf;
      strChgProf = GUILayout.TextField(strChgProf, GUILayout.Width(70));
      GUILayout.Label("(Funds)", GUILayout.Width(50));
      GUILayout.EndHorizontal();
      if (double.TryParse(strChgProf, out chgProf))
        RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCost = chgProf;

      GUI.enabled = true;
    }

    internal static void DisplaySelectSalaryPeriod()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("SalaryPeriod:", GUILayout.Width(80));
      RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly, "Monthly", GUILayout.Width(70));
      if (RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly)
      {
        RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly = false;
      }
      else
      {
        if (!RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly)
        {
          RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly = true;
          RMLifeSpan.Instance.RMGameSettings.SalaryPeriod = "Monthly";
        }
      }
      RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly, "Yearly", GUILayout.Width(80));
      if (RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly)
        RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly = false;
      else
      {
        if (!RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly)
        {
          RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly = true;
          RMLifeSpan.Instance.RMGameSettings.SalaryPeriod = "Yearly";
        }
      }
      GUILayout.EndHorizontal();
    }

    private static void DisplayToolTips()
    {
      // Enable Tool Tips
      GUI.enabled = true;
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
      GUILayout.Label("ToolTips");
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

      var label = "Enable Tool Tips";
      RMSettings.ShowToolTips = GUILayout.Toggle(RMSettings.ShowToolTips, label, GUILayout.Width(300));

      GUI.enabled = RMSettings.ShowToolTips;
      label = "Settings Window Tool Tips";
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      ShowToolTips = GUILayout.Toggle(ShowToolTips, label, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      label = "Roster Window Tool Tips";
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, label, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      label = "Debugger Window Tool Tips";
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, label, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      label = "Contract Disputes Window Tool Tips";
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      WindowContracts.ShowToolTips = GUILayout.Toggle(WindowContracts.ShowToolTips, label, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      GUI.enabled = true;
    }

    private static void DisplayHighlighting()
    {
      GUI.enabled = true;
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
      GUILayout.Label("Highlighting");
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

      // EnableHighlighting Mode
      GUILayout.BeginHorizontal();
      var label = "Enable Highlighting";
      RMSettings.EnableHighlighting = GUILayout.Toggle(RMSettings.EnableHighlighting, label, GUILayout.Width(300));
      GUILayout.EndHorizontal();
    }

    private static void DisplayOptions()
    {
      GUI.enabled = true;
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
      GUILayout.Label(!RMSettings.LockSettings
        ? "Settings / Options"
        : "Settings / Options  (Locked.  Unlock in Config file)");
      GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

      var isEnabled = !RMSettings.LockSettings;
      // Realism Mode
      GUI.enabled = isEnabled;
      var guiLabel = new GUIContent("Enable Realism Mode", "Turns on/off Realism Mode.\r\nWhen ON, causes changes in the interface and limits\r\nyour freedom to things that would not be 'Realistic'.\r\nWhen Off, Allows Fills, Dumps, Repeating Science, instantaneous Xfers, Crew Xfers anywwhere, etc.");
      RMSettings.RealismMode = GUILayout.Toggle(RMSettings.RealismMode, guiLabel, GUILayout.Width(300));
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // LockSettings Mode
      GUI.enabled = isEnabled;
      guiLabel = new GUIContent("Lock Settings  (If set ON, disable in config file)", "Locks the settings in this section so they cannot be altered in game.\r\nTo turn off Locking you MUST edit the Config.xml file.");
      RMSettings.LockSettings = GUILayout.Toggle(RMSettings.LockSettings, guiLabel, GUILayout.Width(300));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
    }

    #endregion Settings Window (GUI)
  }
}