using UnityEngine;

namespace RosterManager.Windows.Tabs.Roster
{
  internal class TabHistory
  {
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    internal static bool ToolTipActive = true;
    private static Vector2 _scrollDetailsPosition = Vector2.zero;

    internal static void Display()
    {
      _scrollDetailsPosition = GUILayout.BeginScrollView(_scrollDetailsPosition, RMStyle.ScrollStyle, GUILayout.Height(210), GUILayout.Width(780));
      GUILayout.Label("Kerbal Flight History:  " + WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Trait + ")", RMStyle.LabelStyleBold, GUILayout.Width(500));

      if (!string.IsNullOrEmpty(RMAddon.SaveMessage))
      {
        GUILayout.Label(RMAddon.SaveMessage, RMStyle.ErrorLabelRedStyle);
      }

      // Begin Tab contents.
      FlightLog thisLog = WindowRoster.SelectedKerbal.Kerbal.flightLog;

      foreach (FlightLog.Entry thisEntry in thisLog.Entries)
      {
        GUILayout.Label(thisEntry.flight + " - " + thisEntry.target + " - " + thisEntry.type);
      }

      //End Tab contents
      GUILayout.EndScrollView();

      WindowRoster.DisplayActionButtonsEdit();
    }
  }
}