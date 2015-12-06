using System;
using System.Collections.Generic;
using UnityEngine;

namespace RosterManager
{
  internal static class Utilities
  {
    internal static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
    internal static String PlugInPath = AppPath + "GameData/RosterManager/Plugins/PluginData/RosterManager/";
    internal static Vector2 DebugScrollPosition = Vector2.zero;

    private static readonly List<string> Errors = new List<string>();

    internal static List<string> ErrorList
    {
      get { return Errors; }
    }

    internal static void LoadTexture(ref Texture2D tex, String fileName)
    {
      LogMessage(string.Format("Loading Texture - file://{0}{1}", PlugInPath, fileName), "Info", RMSettings.VerboseLogging);
      var img1 = new WWW(string.Format("file://{0}{1}", PlugInPath, fileName));
      img1.LoadImageIntoTexture(tex);
    }

    internal static void LogMessage(string error, string type, bool verbose)
    {
      try
      {
        // Add rolling error list. This limits growth.  Configure with ErrorListLength
        if (Errors.Count > int.Parse(RMSettings.ErrorLogLength) && int.Parse(RMSettings.ErrorLogLength) > 0)
          Errors.RemoveRange(0, Errors.Count - int.Parse(RMSettings.ErrorLogLength));
        if (verbose)
          Errors.Add(type + ": " + error);
        if (type == "Error" && RMSettings.AutoDebug)
          WindowDebugger.ShowWindow = true;
      }
      catch (Exception ex)
      {
        Errors.Add("Error: " + ex);
        WindowDebugger.ShowWindow = true;
      }
    }

    internal static void ShowToolTips()
    {
      if (RMAddon.ToolTip != null && RMAddon.ToolTip.Trim().Length > 0)
      {
        //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", RMAddon.toolTip, RMAddon.ToolTipPos.ToString()), "Info", Settings.VerboseLogging);
        ShowToolTip(RMAddon.ToolTipPos, RMAddon.ToolTip);
      }

      // Obtain the new value from the last repaint.
      var toolTip = "";
      if (WindowRoster.ToolTip != null && WindowRoster.ToolTip.Trim().Length > 0)
        toolTip = WindowRoster.ToolTip;
      if (WindowDebugger.ToolTip != null && WindowDebugger.ToolTip.Trim().Length > 0)
        toolTip = WindowDebugger.ToolTip;
      if (WindowSettings.ToolTip != null && WindowSettings.ToolTip.Trim().Length > 0)
        toolTip = WindowSettings.ToolTip;
      if (WindowContracts.ToolTip != null && WindowContracts.ToolTip.Trim().Length > 0)
        toolTip = WindowContracts.ToolTip;

      // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.
      // Tooltip will not display if changes are made during the curreint OnGUI.  (Unity issue with onGUI callback functions)
      RMAddon.ToolTip = toolTip;
    }

    internal static void ShowToolTip(Vector2 toolTipPos, string toolTip)
    {
      if (RMSettings.ShowToolTips && (toolTip != null) && (toolTip.Trim().Length > 0))
      {
        var size = RMStyle.ToolTipStyle.CalcSize(new GUIContent(toolTip));
        var rect = new Rect(toolTipPos.x + 20, toolTipPos.y - 4, size.x, size.y);
        GUI.Window(0, rect, EmptyWindow, toolTip, RMStyle.ToolTipStyle);
        GUI.BringWindowToFront(0);
        //Utilities.LogMessage(string.Format("ShowToolTip: \r\nRectangle data:  {0} \r\nToolTip:  {1}\r\nToolTipPos:  {2}", rect.ToString(), ToolTip, toolTipPos.ToString()), "Info", true);
      }
    }

    internal static string SetActiveTooltip(Rect rect, Rect windowPosition, string toolTip, ref bool toolTipActive, float xOffset, float yOffset)
    {
      if (!toolTipActive && rect.Contains(Event.current.mousePosition))
      {
        toolTipActive = true;
        // Since we are using GUILayout, the curent mouse position returns a position with reference to the source Details viewer.
        // Add the height of GUI elements already drawn to y offset to get the correct screen position
        if (rect.Contains(Event.current.mousePosition))
        {
          RMAddon.ToolTipPos = Event.current.mousePosition;
          RMAddon.ToolTipPos.x = RMAddon.ToolTipPos.x + windowPosition.x + xOffset;
          RMAddon.ToolTipPos.y = RMAddon.ToolTipPos.y + windowPosition.y + yOffset;
          //Utilities.LogMessage(string.Format("Setup Tooltip - Mouse inside Rect: \r\nRectangle data:  {0} \r\nWindowPosition:  {1}\r\nToolTip:  {2}\r\nToolTip Pos:  {3}", rect.ToString(), WindowPosition.ToString(), ToolTip, RosterManagerAddon.ToolTipPos.ToString()), "Info", true);
        }
        else
          toolTip = "";
      }
      // We are in a loop so we don't need the return value from SetUpToolTip.  We will assign it instead.
      if (!toolTipActive)
        toolTip = "";
      return toolTip;
    }

    private static void EmptyWindow(int windowId)
    { }
  }
}