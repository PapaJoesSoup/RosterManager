﻿using System.Globalization;
using RosterManager.Api;
using RosterManager.InternalObjects;
using UnityEngine;
using KSP.Localization;

namespace RosterManager.Windows.Tabs.Settings
{
  internal static class TabConfig
  {
    // GUI tooltip and label support
    private static string _toolTip = "";
    private static Rect _rect;
    private static string _label = "";
    private static GUIContent _guiLabel;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;

    internal static Rect Position = WindowSettings.Position;

    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      GUILayout.Label(Localizer.Format("#autoLOC_RM_1055"));		// #autoLOC_RM_1055 = Configuraton
      GUILayout.Label("____________________________________________________________________________________________",
        RMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

      if (!ToolbarManager.ToolbarAvailable)
      {
        if (RMSettings.EnableBlizzyToolbar)
          RMSettings.EnableBlizzyToolbar = false;
        GUI.enabled = false;
      }
      else
        GUI.enabled = true;

      _label = Localizer.Format("#autoLOC_RM_1056");		// #autoLOC_RM_1056 = Enable Blizzy Toolbar (Replaces Stock Toolbar)
      _toolTip = Localizer.Format("#autoLOC_RM_1132");		// #autoLOC_RM_1132 = Switches the toolbar Icons over to Blizzy's toolbar, if installed.\nIf Blizzy's toolbar is not installed, option is not selectable.
      _guiLabel = new GUIContent(_label, _toolTip);
      RMSettings.EnableBlizzyToolbar = GUILayout.Toggle(RMSettings.EnableBlizzyToolbar, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = true;
      _label = Localizer.Format("#autoLOC_RM_1057");		// #autoLOC_RM_1057 = Enable Debug Window
      _toolTip = Localizer.Format("#autoLOC_RM_1133");		// #autoLOC_RM_1133 = Turns on or off the SM Debug window.\nAllows viewing log entries / errors generated by SM.
      _guiLabel = new GUIContent(_label, _toolTip);
      WindowDebugger.ShowWindow = GUILayout.Toggle(WindowDebugger.ShowWindow, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      _label = Localizer.Format("#autoLOC_RM_1058");		// #autoLOC_RM_1058 = Enable Verbose Logging
      _toolTip = Localizer.Format("#autoLOC_RM_1134");		// #autoLOC_RM_1134 = Turns on or off Expanded logging in the Debug Window.\nAids in troubleshooting issues in RM
      _guiLabel = new GUIContent(_label, _toolTip);
      RMSettings.VerboseLogging = GUILayout.Toggle(RMSettings.VerboseLogging, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      _label = Localizer.Format("#autoLOC_RM_1059");		// #autoLOC_RM_1059 = Enable RM Debug Window On Error
      _toolTip = Localizer.Format("#autoLOC_RM_1135");		// #autoLOC_RM_1135 = When On, Roster Manager automatically displays the RM Debug window on an error in RM.\nThis is a troubleshooting aid.
      _guiLabel = new GUIContent(_label, _toolTip);
      RMSettings.AutoDebug = GUILayout.Toggle(RMSettings.AutoDebug, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      _label = Localizer.Format("#autoLOC_RM_1060");		// #autoLOC_RM_1060 = Save Error log on Exit
      _toolTip = Localizer.Format("#autoLOC_RM_1136");		// #autoLOC_RM_1136 = When On, Roster Manager automatically saves the RM debug log on game exit.\nThis is a troubleshooting aid.
      _guiLabel = new GUIContent(_label, _toolTip);
      RMSettings.SaveLogOnExit = GUILayout.Toggle(RMSettings.SaveLogOnExit, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // create Limit Error Log Length slider;
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1061");		// #autoLOC_RM_1061 = Error Log Length: 
      _toolTip = Localizer.Format("#autoLOC_RM_1137");		// #autoLOC_RM_1137 = Sets the maximum number of error entries stored in the log.\nAdditional entries will cause first entries to be removed from the log (rolling).\nSetting this value to '0' will allow unlimited entries.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(110));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      RMSettings.ErrorLogLength = GUILayout.TextField(RMSettings.ErrorLogLength, GUILayout.Width(40));
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1062"), GUILayout.Width(50));		// #autoLOC_RM_1062 = (lines)
      GUILayout.EndHorizontal();

      _label = Localizer.Format("#autoLOC_RM_1065");		// #autoLOC_RM_1065 = Enable Kerbal Aging
      _toolTip = Localizer.Format("#autoLOC_RM_1066");		// #autoLOC_RM_1066 = Your Kerbals will age and eventually die from old age.
      RMLifeSpan.Instance.RMGameSettings.EnableAging = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.EnableAging, new GUIContent(_label, _toolTip), GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      if (!RMLifeSpan.Instance.RMGameSettings.EnableAging)
        GUI.enabled = false;
      GUILayout.BeginHorizontal();
      _toolTip = Localizer.Format("#autoLOC_RM_1067");		// #autoLOC_RM_1067 = Average age of new Applicant Kerbals.
      GUILayout.Label(new GUIContent(Localizer.Format("#autoLOC_RM_1068"), _toolTip), GUILayout.Width(140));		// #autoLOC_RM_1068 = Kerbal Minimum Age: 
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);
      string strMinimumAge = RMLifeSpan.Instance.RMGameSettings.MinimumAge.ToString();
      strMinimumAge = GUILayout.TextField(strMinimumAge, GUILayout.Width(40));
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1069"), GUILayout.Width(50));		// #autoLOC_RM_1069 = (Years)
      if (int.TryParse(strMinimumAge, out int minimumAge))
        RMLifeSpan.Instance.RMGameSettings.MinimumAge = minimumAge;
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      _toolTip = Localizer.Format("#autoLOC_RM_1070");		// #autoLOC_RM_1070 = Average Lifespan of Kerbals (how long they live).
      GUILayout.Label(new GUIContent(Localizer.Format("#autoLOC_RM_1071"), _toolTip), GUILayout.Width(140));		// #autoLOC_RM_1071 = Kerbal Lifespan: 
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);
      string strMaximumAge = RMLifeSpan.Instance.RMGameSettings.MaximumAge.ToString();
      strMaximumAge = GUILayout.TextField(strMaximumAge, GUILayout.Width(40));
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1072"), GUILayout.Width(50));		// #autoLOC_RM_1072 = (Years)
      if (int.TryParse(strMaximumAge, out int maximumAge))
        RMLifeSpan.Instance.RMGameSettings.MaximumAge = maximumAge;
      GUILayout.EndHorizontal();

      GUI.enabled = true;
      if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
        GUI.enabled = false;
      _label = Localizer.Format("#autoLOC_RM_1073");		// #autoLOC_RM_1073 = Enable Salaries (career game only)
      _toolTip = Localizer.Format("#autoLOC_RM_1074");		// #autoLOC_RM_1074 = Kerbals must be paid Salaries.
      RMLifeSpan.Instance.RMGameSettings.EnableSalaries = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.EnableSalaries, new GUIContent(_label, _toolTip), GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      if (!RMLifeSpan.Instance.RMGameSettings.EnableSalaries)
        GUI.enabled = false;
      DisplaySelectSalaryPeriod();

      GUILayout.BeginHorizontal();
      _toolTip = Localizer.Format("#autoLOC_RM_1075");		// #autoLOC_RM_1075 = Default salary for each Kerbal per each salary period.
      GUILayout.Label(new GUIContent(Localizer.Format("#autoLOC_RM_1076"), _toolTip), GUILayout.Width(140));		// #autoLOC_RM_1076 = Default Kerbal Salary: 
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);
      string strDefSalary = RMLifeSpan.Instance.RMGameSettings.DefaultSalary.ToString(CultureInfo.InvariantCulture);
      strDefSalary = GUILayout.TextField(strDefSalary, GUILayout.Width(70));
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1077"), GUILayout.Width(120));		// #autoLOC_RM_1077 = (Funds/per salary period)
      GUILayout.EndHorizontal();
      if (double.TryParse(strDefSalary, out double defaultSalary))
        RMLifeSpan.Instance.RMGameSettings.DefaultSalary = defaultSalary;

      GUI.enabled = true;
      if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
        GUI.enabled = false;

      _label = Localizer.Format("#autoLOC_RM_1078");		// #autoLOC_RM_1078 = Charge funds for Profession Change (career game only)
      _toolTip = Localizer.Format("#autoLOC_RM_1079");		// #autoLOC_RM_1079 = Charge funds for changing a Kerbal's profession.
      RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge, new GUIContent(_label, _toolTip), GUILayout.Width(320));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      if (!RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge)
        GUI.enabled = false;

      GUILayout.BeginHorizontal();
      _toolTip = Localizer.Format("#autoLOC_RM_1080");		// #autoLOC_RM_1080 = Cost of changing a Kerbals Profession.
      GUILayout.Label(new GUIContent(Localizer.Format("#autoLOC_RM_1081"), _toolTip), GUILayout.Width(140));		// #autoLOC_RM_1081 = Change Profession Cost: 
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);
      string strChgProf = RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCost.ToString(CultureInfo.InvariantCulture);
      strChgProf = GUILayout.TextField(strChgProf, GUILayout.Width(70));
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1082"), GUILayout.Width(50));		// #autoLOC_RM_1082 = (Funds)
      GUILayout.EndHorizontal();
      if (double.TryParse(strChgProf, out double chgProf))
        RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCost = chgProf;

      GUI.enabled = true;
    }

    internal static void DisplaySelectSalaryPeriod()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1083"), GUILayout.Width(80));		// #autoLOC_RM_1083 = SalaryPeriod:
      RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly, Localizer.Format("#autoLOC_RM_1084"), GUILayout.Width(70));		// #autoLOC_RM_1084 = Monthly
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
      RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly = GUILayout.Toggle(RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly, Localizer.Format("#autoLOC_RM_1085"), GUILayout.Width(80));		// #autoLOC_RM_1085 = Yearly
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

  }
}