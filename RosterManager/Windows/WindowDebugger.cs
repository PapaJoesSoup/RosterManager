using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace RosterManager.Windows
{
  internal class WindowDebugger
  {
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      var rect = new Rect(496, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
      {
        ShowWindow = false;
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 0, 0);

      GUILayout.BeginVertical();
      Utilities.DebugScrollPosition = GUILayout.BeginScrollView(Utilities.DebugScrollPosition, GUILayout.Height(300), GUILayout.Width(500));
      GUILayout.BeginVertical();

      foreach (var error in Utilities.ErrorList)
        GUILayout.TextArea(error, GUILayout.Width(460));

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Clear log", GUILayout.Height(20)))
      {
        Utilities.ErrorList.Clear();
        Utilities.ErrorList.Add("Info:  Log Cleared at " + DateTime.UtcNow + " UTC.");
      }
      if (GUILayout.Button("Save Log", GUILayout.Height(20)))
      {
        // Create log file and save.
        Savelog();
      }
      if (GUILayout.Button("Close", GUILayout.Height(20)))
      {
        // Create log file and save.
        ShowWindow = false;
      }
      GUILayout.EndHorizontal();

      GUILayout.EndVertical();
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      RMSettings.RepositionWindows("WindowDebugger");
    }

    internal static void Savelog()
    {
      try
      {
        // time to create a file...
        var filename = "DebugLog_" + DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace(" ", "_").Replace("/", "").Replace(":", "") + ".txt";

        var path = Directory.GetCurrentDirectory() + @"\GameData\RosterManager\";
        if (RMSettings.DebugLogPath.StartsWith(@"\"))
          RMSettings.DebugLogPath = RMSettings.DebugLogPath.Substring(2, RMSettings.DebugLogPath.Length - 2);

        if (!RMSettings.DebugLogPath.EndsWith(@"\"))
          RMSettings.DebugLogPath += @"\";

        filename = path + RMSettings.DebugLogPath + filename;
        Utilities.LogMessage("File Name = " + filename, "Info", true);

        try
        {
          var sb = new StringBuilder();
          foreach (var line in Utilities.ErrorList)
          {
            sb.AppendLine(line);
          }

          File.WriteAllText(filename, sb.ToString());

          Utilities.LogMessage("File written", "Info", true);
        }
        catch (Exception ex)
        {
          Utilities.LogMessage("Error Writing File:  " + ex, "Info", true);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Savelog.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }
  }
}