using RosterManager.InternalObjects;
using UnityEngine;
using KSP.Localization;

namespace RosterManager.Windows.Tabs.Settings
{
  internal static class TabToolTips
  {
    internal static string StrFlowCost = "0";

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
      ToolTip = "";
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      // Enable Tool Tips
      GUI.enabled = true;
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1086"), RMStyle.LabelTabHeader); // #autoLOC_RM_1086 = ToolTips
      GUILayout.Label("____________________________________________________________________________________________",
        RMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

      //_toolTip = "Turns all tooltips On or Off.";
      //_toolTip += "\r\nThis is a global setting for all windows/tabs";
      _label = Localizer.Format("#autoLOC_RM_1087");		// #autoLOC_RM_1087 = Enable Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1119");		// #autoLOC_RM_1119 = Turns all tooltips On or Off.\nThis is a global setting for all windows/tabs
      _guiLabel = new GUIContent(_label, _toolTip);
      RMSettings.ShowToolTips = GUILayout.Toggle(RMSettings.ShowToolTips, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = RMSettings.ShowToolTips;

      // Debugger Window
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1090");		// #autoLOC_RM_1090 = Debugger Window Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1120");		// #autoLOC_RM_1120 = Turns tooltips On or Off for the Debugger Window only.\nRequires All ToolTips setting to be enabled.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Settings Window
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1088");    // #autoLOC_RM_1088 = Settings Window Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1121");		// #autoLOC_RM_1121 = Turns tooltips On or Off for the Settings Window only.\nRequires global ToolTips setting to be enabled.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowSettings.ShowToolTips = GUILayout.Toggle(WindowSettings.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = RMSettings.ShowToolTips && WindowSettings.ShowToolTips;

      // SW - Realism Tab
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1122");		// #autoLOC_RM_1122 = Realism Tab Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1123");		// #autoLOC_RM_1123 = Turns tooltips On or Off for the Settings Window's Realism Tab only.\nRequires global ToolTips setting to be enabled.\nAlso requires Settings Window tooltips to be enabled.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabRealism.ShowToolTips = GUILayout.Toggle(TabRealism.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - ToolTips Tab
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1126");		// #autoLOC_RM_1126 = ToolTips Tab Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1127");		// #autoLOC_RM_1127 = Turns tooltips On or Off for the Settings Window's ToolTips Tab only.\nRequires global ToolTips setting to be enabled.\nAlso requires Settings Window tooltips to be enabled.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      ShowToolTips = GUILayout.Toggle(ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Config Tab
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1128");		// #autoLOC_RM_1128 = Config Tab Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1129");		// #autoLOC_RM_1129 = Turns tooltips On or Off for the Settings Window's Config Tab only.\nRequires global ToolTips setting to be enabled.\nAlso requires Settings Window tooltips to be enabled.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabConfig.ShowToolTips = GUILayout.Toggle(TabConfig.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = RMSettings.ShowToolTips;

      // Roster Window
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1089");		// #autoLOC_RM_1089 = Roster Window Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1130");		// #autoLOC_RM_1130 = Turns tooltips On or Off for the Roster Window only.\nRequires global ToolTips setting to be enabled.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = RMSettings.ShowToolTips;

      // Contracts Window
      GUILayout.BeginHorizontal();
      _label = Localizer.Format("#autoLOC_RM_1091");		// #autoLOC_RM_1091 = Contract Disputes Window Tool Tips
      _toolTip = Localizer.Format("#autoLOC_RM_1131");		// #autoLOC_RM_1131 = Turns tooltips On or Off for the Contracts Window only.\nRequires global ToolTips setting to be enabled.
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = true;
    }
  }
}