﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using RosterManager.InternalObjects;
using UnityEngine;

namespace RosterManager.Windows
{
  internal class WindowDebugger
  {
    internal static float WindowHeight = 355;
    internal static float HeightScale;
    internal static float ViewerHeight = 300;
    internal static float MinHeight = 200;
    internal static bool ResizingWindow = false;
    internal static Rect Position = RMSettings.DefaultPosition;
    private static bool _inputLocked;
    private static bool _showWindow;
    internal static bool ShowWindow
    {
      get => _showWindow;
      set
      {
        if (!value)
        {
          InputLockManager.RemoveControlLock("RM_Window");
          _inputLocked = false;
        }
        _showWindow = value;
      }
    }
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(int windowId)
    {

      // set input locks when mouseover window...
      _inputLocked = RmUtils.PreventClickThrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(496, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
      {
        ShowWindow = false;
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.BeginVertical();
      RmUtils.DebugScrollPosition = GUILayout.BeginScrollView(RmUtils.DebugScrollPosition, GUILayout.Height(ViewerHeight + HeightScale), GUILayout.Width(500));
      GUILayout.BeginVertical();

      foreach (string error in RmUtils.ErrorList)
        GUILayout.TextArea(error, GUILayout.Width(460));

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Clear log", GUILayout.Height(20)))
      {
        RmUtils.ErrorList.Clear();
        RmUtils.ErrorList.Add("Info:  Log Cleared at " + DateTime.UtcNow + " UTC.");
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
      //resizing
      Rect resizeRect =
        new Rect(Position.width - 18, Position.height - 18, 16, 16);
      GUI.DrawTexture(resizeRect, RmUtils.resizeTexture, ScaleMode.StretchToFill, true);
      if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
      {
        ResizingWindow = true;
      }

      if (Event.current.type == EventType.Repaint && ResizingWindow)
      {
        if (Mouse.delta.y != 0)
        {
          float diff = Mouse.delta.y;
          RmUtils.UpdateScale(diff, ViewerHeight, ref HeightScale, MinHeight);
        }
      }
      //ResetZoomKeys();
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      Position.height = WindowHeight + HeightScale;
      RMSettings.RepositionWindow(ref Position);
    }

    internal static void Savelog()
    {
      try
      {
        // time to create a file...
        string filename = "DebugLog_" + DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace(" ", "_").Replace("/", "").Replace(":", "") + ".txt";

        string path = Directory.GetCurrentDirectory() + @"\GameData\RosterManager\";
        if (RMSettings.DebugLogPath.StartsWith(@"\"))
          RMSettings.DebugLogPath = RMSettings.DebugLogPath.Substring(2, RMSettings.DebugLogPath.Length - 2);

        if (!RMSettings.DebugLogPath.EndsWith(@"\"))
          RMSettings.DebugLogPath += @"\";

        filename = path + RMSettings.DebugLogPath + filename;
        RmUtils.LogMessage("File Name = " + filename, "Info", true);

        try
        {
          StringBuilder sb = new StringBuilder();
          foreach (string line in RmUtils.ErrorList)
          {
            sb.AppendLine(line);
          }

          File.WriteAllText(filename, sb.ToString());

          RmUtils.LogMessage("File written", "Info", true);
        }
        catch (Exception ex)
        {
          RmUtils.LogMessage("Error Writing File:  " + ex, "Info", true);
        }
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage($" in Savelog.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", "Error", true);
      }
    }
  }
}