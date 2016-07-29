using System;
using System.Collections.Generic;
using RosterManager.Windows;
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

    private static void EmptyWindow(int windowId)
    { }
  }
}