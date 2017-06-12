using System;
using RosterManager.InternalObjects;
using UnityEngine;
using KSP.Localization;
using RosterManager.Windows.Tabs.Settings;

namespace RosterManager.Windows
{
  internal static class WindowSettings
  {
    #region Settings Window (GUI)

    internal static string Title = "Ship Manifest Settings";
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    private static Tab _selectedTab = Tab.Realism;

    private static Vector2 _displayViewerPosition = Vector2.zero;

    internal static void Display(int windowId)
    {
      //Title = SmUtils.Localize("#smloc_settings_001");
      // set input locks when mouseover window...
      //_inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      // "Close Window.\r\nSettings will not be immediately saved,\r\n but will be remembered while in game.")))
      if (GUI.Button(rect, new GUIContent("", Localizer.Format("#autoLOC_RM_1052"))))		// #autoLOC_RM_1052 = Close Window
      {
        ToolTip = "";
        RMSettings.RestoreTempSettings();
        ShowWindow = false;
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.BeginVertical();

      DisplayTabButtons();

      _displayViewerPosition = GUILayout.BeginScrollView(_displayViewerPosition, RMStyle.ScrollStyle,
        GUILayout.Height(300), GUILayout.Width(380));
      GUILayout.BeginVertical();

      DisplaySelectedTab(_displayViewerPosition);

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      DisplayActionButtons();

      GUILayout.EndVertical();

      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      RMSettings.RepositionWindow(ref Position);
    }

    private static void DisplayTabButtons()
    {
      GUILayout.BeginHorizontal();

      GUIStyle realiRMStyle = _selectedTab == Tab.Realism ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      GUIContent label = new GUIContent(Localizer.Format("#autoLOC_RM_1138"), Localizer.Format("#autoLOC_RM_1139"));		// #autoLOC_RM_1138 = Realism		// #autoLOC_RM_1139 = Displays all settings related to Realism behaviors.
      if (GUILayout.Button(label, realiRMStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Realism;
      }
      GUI.enabled = true;
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle tooltipStyle = _selectedTab == Tab.ToolTips ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      label = new GUIContent(Localizer.Format("#autoLOC_RM_1140"), Localizer.Format("#autoLOC_RM_1141"));		// #autoLOC_RM_1140 = ToolTip		// #autoLOC_RM_1141 = Displays all settings related to ToolTip behaviors.
      if (GUILayout.Button(label, tooltipStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.ToolTips;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle configStyle = _selectedTab == Tab.Config ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      label = new GUIContent(Localizer.Format("#autoLOC_RM_1142"), Localizer.Format("#autoLOC_RM_1143"));		// #autoLOC_RM_1142 = Config		// #autoLOC_RM_1143 = Displays all settings related to Mod Configuration.
      if (GUILayout.Button(label, configStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Config;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab(Vector2 displayViewerPosition)
    {
      switch (_selectedTab)
      {
        case Tab.Realism:
          TabRealism.Display(displayViewerPosition);
          break;
        case Tab.Config:
          TabConfig.Display(displayViewerPosition);
          break;
        case Tab.ToolTips:
          TabToolTips.Display(displayViewerPosition);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private static void DisplayActionButtons()
    {
      GUILayout.BeginHorizontal();

      // Save
      GUIContent label = new GUIContent(Localizer.Format("#autoLOC_RM_1053"), Localizer.Format("#autoLOC_RM_1144"));		// #autoLOC_RM_1053 = Save    //#autoLOC_RM_1144  Save the current settings to file.
      if (GUILayout.Button(label, GUILayout.Height(20)))
      {
        ToolTip = "";
        RMSettings.SaveSettings();
        ShowWindow = false;
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Cancel
      label = new GUIContent(Localizer.Format("#autoLOC_RM_1054"), Localizer.Format("#autoLOC_RM_1145"));		// #autoLOC_RM_1054 = Cancel		// #autoLOC_RM_1145 = Cancel the changes made.\nSettings will revert to before changes were made.
      if (GUILayout.Button(label, GUILayout.Height(20)))
      {
        ToolTip = "";
        // We've canclled, so restore original settings.
        RMSettings.RestoreTempSettings();
        ShowWindow = false;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
    }

    private enum Tab
    {
      Realism,
      Config,
      ToolTips
    }

    #endregion
  }
}