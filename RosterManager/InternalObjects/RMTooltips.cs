using RosterManager.Windows;
using RosterManager.Windows.Tabs.Roster;
using UnityEngine;

namespace RosterManager.InternalObjects
{
  // ReSharper disable once InconsistentNaming
  internal static class RMToolTips
  {
    // Tooltip vars
    internal static Rect ControlRect;
    internal static Vector2 ToolTipPos;
    internal static string ToolTip;
    internal static Rect Position;
    internal static float XOffset;

    internal static void ShowToolTips()
    {
      if (!string.IsNullOrEmpty(ToolTip))
      {
        //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", SmAddon.toolTip, RMToolTips.ToolTipPos.ToString()), RmUtils.LogType.Info, Settings.VerboseLogging);
        ShowToolTip(ToolTipPos, ToolTip);
      }

      // Obtain the new value from the last repaint.
      ToolTip = GetCurrentToolTip();
    }

    internal static void ShowToolTip(Vector2 toolTipPos, string toolTip)
    {
      if (!RMSettings.ShowToolTips || (toolTip == null) || (toolTip.Trim().Length <= 0)) return;
      Vector2 size = RMStyle.ToolTipStyle.CalcSize(new GUIContent(toolTip));
      Position = new Rect(toolTipPos.x, toolTipPos.y, size.x + 4, size.y + 4);
      RepositionToolTip();
      GUI.Window(0, Position, EmptyWindow, toolTip, RMStyle.ToolTipStyle);
      GUI.BringWindowToFront(0);
    }

    internal static string SetActiveToolTip(Rect control, string toolTip, ref bool toolTipActive, float xOffset)
    {
      // Note:  all values are screen point based.  (0,0 in lower left).  this removes confusion with the gui point of elements (o,o in upper left).
      if (!toolTipActive && control.Contains(Event.current.mousePosition))
      {
        toolTipActive = true;
        // Note at this time controlPosition is in Gui Point system and is local position.  convert to screenpoint.
        Rect newControl = new Rect
        {
          position = GUIUtility.GUIToScreenPoint(control.position),
          width = control.width,
          height = control.height
        };

        // Event.current.mousePosition returns sceen mouseposition.  GuI elements return a value in gui position.. 
        // Add the height of parent GUI elements already drawn to y offset to get the correct screen position
        if (control.Contains(Event.current.mousePosition))
        {
          // Let's use the rectangle as a solid anchor and a stable tooltip, forgiving of mouse movement within bounding box...
          ToolTipPos = new Vector2(newControl.xMax + xOffset, newControl.y - 10);

          ControlRect = newControl;
          XOffset = xOffset;
          ControlRect.x += xOffset;
          ControlRect.y -= 10;
        }
        else
          toolTip = "";
      }
      // We are in a loop so we don't need the return value from SetUpToolTip.  We will assign it instead.
      if (!toolTipActive)
        toolTip = "";
      return toolTip;
    }

    private static string GetCurrentToolTip()
    {
      // Only one of these values can be active at a time (onMouseOver), so this will trap it.
      // (Brute force, but functional)
      string toolTip = "";
      if (!string.IsNullOrEmpty(WindowRoster.ToolTip)) toolTip = WindowRoster.ToolTip;
      if (!string.IsNullOrEmpty(WindowContracts.ToolTip)) toolTip = WindowContracts.ToolTip;
      if (!string.IsNullOrEmpty(WindowDebugger.ToolTip)) toolTip = WindowDebugger.ToolTip;
      if (!string.IsNullOrEmpty(WindowSettings.ToolTip)) toolTip = WindowSettings.ToolTip;

      // Roster Window Tab switches
      if (!string.IsNullOrEmpty(TabAttributes.ToolTip)) toolTip = TabAttributes.ToolTip;
      if (!string.IsNullOrEmpty(TabHistory.ToolTip)) toolTip = TabHistory.ToolTip;
      if (!string.IsNullOrEmpty(TabMedical.ToolTip)) toolTip = TabMedical.ToolTip;
      if (!string.IsNullOrEmpty(TabRecords.ToolTip)) toolTip = TabRecords.ToolTip;
      if (!string.IsNullOrEmpty(TabScheduling.ToolTip)) toolTip = TabScheduling.ToolTip;
      if (!string.IsNullOrEmpty(TabTraining.ToolTip)) toolTip = TabTraining.ToolTip;

      // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.  
      // Tooltip will not display if changes are made during the current OnGUI.  
      // (Unity uses onGUI async callbacks so we need to allow for the callback)
      return toolTip;
    }

    private static void RepositionToolTip()
    {
      if (Position.xMax > Screen.width)
        Position.x = ControlRect.x - Position.width - (XOffset > 30 ? 30 : XOffset);
      if (Position.yMax > Screen.height)
        Position.y = Screen.height - Position.height;
    }

    private static void EmptyWindow(int windowId)
    {
    }
  }
}