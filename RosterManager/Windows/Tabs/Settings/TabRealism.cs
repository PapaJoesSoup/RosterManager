using RosterManager.InternalObjects;
using UnityEngine;
using KSP.Localization;

namespace RosterManager.Windows.Tabs.Settings
{
  internal static class TabRealism
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static Rect _rect;
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

      GUI.enabled = true;
      GUILayout.Label(!RMSettings.LockSettings
        ? Localizer.Format("#autoLOC_RM_1094")		// #autoLOC_RM_1094 = Settings / Options
        : Localizer.Format("#autoLOC_RM_1095"));		// #autoLOC_RM_1095 = Settings / Options  (Locked.  Unlock in Config file)
      GUILayout.Label("____________________________________________________________________________________________",
        RMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

      bool isEnabled = !RMSettings.LockSettings;
      // Realism Mode
      GUI.enabled = isEnabled;
      _guiLabel = new GUIContent(Localizer.Format("#autoLOC_RM_1096"), Localizer.Format("#autoLOC_RM_1097"));		// #autoLOC_RM_1096 = Enable Realism Mode		// #autoLOC_RM_1097 = Turns on/off Realism Mode.\nWhen ON, causes changes in the interface and limits\r\nyour freedom to things that would not be 'Realistic'.\r\nWhen Off, Allows Fills, Dumps, Repeating Science, instantaneous Xfers, Crew Xfers anywwhere, etc.
      RMSettings.RealismMode = GUILayout.Toggle(RMSettings.RealismMode, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      // LockSettings Mode
      GUI.enabled = isEnabled;
      _guiLabel = new GUIContent(Localizer.Format("#autoLOC_RM_1098"), Localizer.Format("#autoLOC_RM_1099"));		// #autoLOC_RM_1098 = Lock Settings  (If set ON, disable in config file)		// #autoLOC_RM_1099 = Locks the settings in this section so they cannot be altered in game.\nTo turn off Locking you MUST edit the Config.xml file.
      RMSettings.LockSettings = GUILayout.Toggle(RMSettings.LockSettings, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);
      GUI.enabled = true;
      GUILayout.Label("____________________________________________________________________________________________",
        RMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
    }
  }
}