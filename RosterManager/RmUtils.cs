using System;
using System.Collections.Generic;
using RosterManager.Windows;
using UnityEngine;

namespace RosterManager
{
  internal static class RmUtils
  {
    private static Texture2D _resizeSquare = GameDatabase.Instance.GetTexture(RMAddon.TextureFolder + "resizeSquare", false);
    public static Texture2D resizeTexture
    {
      get
      {
        if (_resizeSquare == null)
          _resizeSquare = GameDatabase.Instance.GetTexture(RMAddon.TextureFolder + "resizeSquare", false);
        return _resizeSquare;
      }
    }
    internal static string AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
    internal static string PlugInPath = AppPath + "GameData/RosterManager/Plugins/PluginData/RosterManager/";
    internal static Vector2 DebugScrollPosition = Vector2.zero;
    private const ControlTypes BLOCK_ALL_CONTROLS = ControlTypes.UI | ControlTypes.All;

    private static readonly List<string> Errors = new List<string>();

    internal static List<string> ErrorList
    {
      get { return Errors; }
    }

    internal static void UpdateScale(float diff, float viewerHeight, ref float heightScale, float minHeight)
    {
      heightScale += diff;
      if (viewerHeight + heightScale < minHeight)
      {
        heightScale = minHeight - viewerHeight;
      }
    }

    private static bool MouseIsOverWindow(bool visible, Rect position)
    {
      return visible
             && position.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
    }

    internal static bool PreventClickThrough(bool visible, Rect position, bool lockedInputs)
    {
      // Still in work.  Not behaving correctly in Flight.  Works fine in Editor and Space Center...
      // Based on testing, it appears that Kerbals can still be accessed
      bool mouseOverWindow = MouseIsOverWindow(visible, position);
      if (!lockedInputs && mouseOverWindow)
      {
        InputLockManager.SetControlLock(BLOCK_ALL_CONTROLS, "RM_Window");
        lockedInputs = true;
      }
      if (!lockedInputs || mouseOverWindow) return lockedInputs;
      InputLockManager.RemoveControlLock("RM_Window");
      return false;
    }

    internal static void ResetResize()
    {
      if (WindowRoster.ResizingWindow) WindowRoster.ResizingWindow = false;
      if (WindowDebugger.ResizingWindow) WindowDebugger.ResizingWindow = false;
      if (WindowSettings.ResizingWindow) WindowSettings.ResizingWindow = false;
      if (WindowContracts.ResizingWindow) WindowContracts.ResizingWindow = false;

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