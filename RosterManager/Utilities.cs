using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosterManager
{
    internal static class Utilities
    {
        internal static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        internal static String PlugInPath = AppPath + "GameData/RosterManager/Plugins/PluginData/RosterManager/";
        internal static Vector2 DebugScrollPosition = Vector2.zero;

        private static List<string> _errors = new List<string>();

        internal static List<string> Errors
        {
            get { return _errors; }
        }

        internal static void LoadTexture(ref Texture2D tex, String FileName)
        {
            LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info", RMSettings.VerboseLogging);
            WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
            img1.LoadImageIntoTexture(tex);
        }

        internal static void LogMessage(string error, string type, bool verbose)
        {
            try
            {
                // Add rolling error list. This limits growth.  Configure with ErrorListLength
                if (_errors.Count() > int.Parse(RMSettings.ErrorLogLength) && int.Parse(RMSettings.ErrorLogLength) > 0)
                    _errors.RemoveRange(0, _errors.Count() - int.Parse(RMSettings.ErrorLogLength));
                if (verbose)
                    _errors.Add(type + ": " + error);
                if (type == "Error" && RMSettings.AutoDebug)
                    WindowDebugger.ShowWindow = true;
            }
            catch (Exception ex)
            {
                _errors.Add("Error: " + ex.ToString());
                WindowDebugger.ShowWindow = true;
            }
        }

        internal static void ShowToolTips()
        {
            if (RMAddon.toolTip != null && RMAddon.toolTip.Trim().Length > 0)
            {
                //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", RMAddon.toolTip, RMAddon.ToolTipPos.ToString()), "Info", Settings.VerboseLogging);
                ShowToolTip(RMAddon.ToolTipPos, RMAddon.toolTip);
            }

            // Obtain the new value from the last repaint.
            string ToolTip = "";
            if (WindowRoster.ToolTip != null && WindowRoster.ToolTip.Trim().Length > 0)
                ToolTip = WindowRoster.ToolTip;
            if (WindowDebugger.ToolTip != null && WindowDebugger.ToolTip.Trim().Length > 0)
                ToolTip = WindowDebugger.ToolTip;
            if (WindowSettings.ToolTip != null && WindowSettings.ToolTip.Trim().Length > 0)
                ToolTip = WindowSettings.ToolTip;
            if (WindowContractDispute.ToolTip != null && WindowContractDispute.ToolTip.Trim().Length > 0)
                ToolTip = WindowContractDispute.ToolTip;

            // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.
            // Tooltip will not display if changes are made during the curreint OnGUI.  (Unity issue with onGUI callback functions)
            RMAddon.toolTip = ToolTip;
        }

        internal static void ShowToolTip(Vector2 toolTipPos, string ToolTip)
        {
            if (RMSettings.ShowToolTips && (ToolTip != null) && (ToolTip.Trim().Length > 0))
            {
                Vector2 size = RMStyle.ToolTipStyle.CalcSize(new GUIContent(ToolTip));
                Rect rect = new Rect(toolTipPos.x + 20, toolTipPos.y - 4, size.x, size.y);
                GUI.Window(0, rect, EmptyWindow, ToolTip, RMStyle.ToolTipStyle);
                GUI.BringWindowToFront(0);
                //Utilities.LogMessage(string.Format("ShowToolTip: \r\nRectangle data:  {0} \r\nToolTip:  {1}\r\nToolTipPos:  {2}", rect.ToString(), ToolTip, toolTipPos.ToString()), "Info", true);
            }
        }

        internal static string SetActiveTooltip(Rect rect, Rect WindowPosition, string toolTip, ref bool toolTipActive, float xOffset, float yOffset)
        {
            if (!toolTipActive && rect.Contains(Event.current.mousePosition))
            {
                toolTipActive = true;
                // Since we are using GUILayout, the curent mouse position returns a position with reference to the source Details viewer.
                // Add the height of GUI elements already drawn to y offset to get the correct screen position
                if (rect.Contains(Event.current.mousePosition))
                {
                    RMAddon.ToolTipPos = Event.current.mousePosition;
                    RMAddon.ToolTipPos.x = RMAddon.ToolTipPos.x + WindowPosition.x + xOffset;
                    RMAddon.ToolTipPos.y = RMAddon.ToolTipPos.y + WindowPosition.y + yOffset;
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