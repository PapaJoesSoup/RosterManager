using UnityEngine;

namespace RosterManager
{
  internal class TabRecords
  {
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    internal static bool ToolTipActive = true;
    private static Vector2 _scrollDetailsPosition = Vector2.zero;

    internal static void Display()
    {
      _scrollDetailsPosition = GUILayout.BeginScrollView(_scrollDetailsPosition, RMStyle.ScrollStyle, GUILayout.Height(210), GUILayout.Width(680));
      GUILayout.Label("Kerbal Records:  " + WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Trait + ")", RMStyle.LabelStyleBold, GUILayout.Width(500));

      if (!string.IsNullOrEmpty(RMAddon.SaveMessage))
      {
        GUILayout.Label(RMAddon.SaveMessage, RMStyle.ErrorLabelRedStyle);
      }

      // Begin Tab contents.

      if (GUILayout.Button("Enter Notes", GUILayout.Width(100)))
      {
        // Let's do a modal window here.  child of parent, moves with parent window.
        // DisplayNotesWindow()
      }

      //End Tab contents
      GUILayout.EndScrollView();

      WindowRoster.DisplayActionButtonsEdit();
    }
  }
}