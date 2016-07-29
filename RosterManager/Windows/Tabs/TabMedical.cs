using UnityEngine;

namespace RosterManager.Windows.Tabs
{
  internal class TabMedical
  {
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    internal static bool ToolTipActive = true;
    private static Vector2 _scrollDetailsPosition = Vector2.zero;

    internal static void Display()
    {
      _scrollDetailsPosition = GUILayout.BeginScrollView(_scrollDetailsPosition, RMStyle.ScrollStyle, GUILayout.Height(210), GUILayout.Width(680));
      GUILayout.Label("Kerbal Medical:  " + WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Trait + ")", RMStyle.LabelStyleBold, GUILayout.Width(500));
      GUILayout.Label("");
      WindowRoster.DisplaySelectType();
      WindowRoster.DisplaySelectState();

      DisplayAnyWarnings();

      WindowRoster.SelectedKerbal.Type = WindowRoster._kerbalType;
      WindowRoster.SelectedKerbal.Status = WindowRoster._rosterStatus;


      if (!string.IsNullOrEmpty(RMAddon.SaveMessage))
      {
        GUILayout.Label(RMAddon.SaveMessage, RMStyle.ErrorLabelRedStyle);
      }
      GUILayout.EndScrollView();

      WindowRoster.DisplayActionButtonsEdit();
    }

    private static void DisplayAnyWarnings()
    {
      if (Api.InstalledMods.IsDfInstalled && WindowRoster.SelectedKerbal.Type == ProtoCrewMember.KerbalType.Unowned &&
          WindowRoster.SelectedKerbal.Status == ProtoCrewMember.RosterStatus.Dead &&
          (!WindowRoster.IsKerbalInDeepFreezePart(WindowRoster.SelectedKerbal)))
      {
        GUILayout.Label("");
        GUILayout.Label(
          "DeepFreeze is installed,and the current settings will make the selected Kerbal appear Frozen.\r\nEnsure you have the kerbal assigned to a vessel and inside a deepFreze part.  \r\nIf you save, this kerbal will 'disappear' in game.",
          RMStyle.LabelStyleYellow);
        GUILayout.Label("");
      }
      else if (WindowRoster.SelectedKerbal.Type == ProtoCrewMember.KerbalType.Unowned)
      {
        GUILayout.Label("");
        GUILayout.Label(
          "The Selected Kerbal is set to Unowned.  This removes them from the active kerbal list. \r\nIf you save, this kerbal will 'disappear' in game.",
          RMStyle.LabelStyleYellow);
        GUILayout.Label("");
      }
      else if (WindowRoster.SelectedKerbal.Type == ProtoCrewMember.KerbalType.Tourist &&
               WindowRoster.SelectedKerbal.Trait != "Tourist")
      {
        GUILayout.Label("");
        GUILayout.Label(
          "The selected Kerbal is set to Tourist.\r\nIf you save, the Profession will be changed to Tourist.",
          RMStyle.LabelStyleYellow);
        GUILayout.Label("");
      }
    }
  }
}