using UnityEngine;

namespace RosterManager.Windows.Tabs.Roster
{
  internal class TabAttributes
  {
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    internal static bool ToolTipActive = true;
    private static Vector2 _scrollDetailsPosition = Vector2.zero;

    internal static void Display()
    {
      _scrollDetailsPosition = GUILayout.BeginScrollView(_scrollDetailsPosition, RMStyle.ScrollStyle, GUILayout.Height(230), GUILayout.Width(WindowRoster.ViewerWidth));
      GUILayout.Label(WindowRoster.SelectedKerbal.IsNew ? "Create a Kerbal" : "Kerbal Attributes", RMStyle.LabelStyleBold);

      GUILayout.BeginHorizontal();
      if (RMSettings.EnableKerbalRename)
      {
        GUILayout.Label("Name:", GUILayout.Width(80));
        WindowRoster.SelectedKerbal.Name = GUILayout.TextField(WindowRoster.SelectedKerbal.Name, GUILayout.Width(230));
        GUILayout.Label(" - (" + WindowRoster.SelectedKerbal.Kerbal.trait + ")");
        if (RMLifeSpan.Instance.RMGameSettings.EnableAging)
        {
          GUILayout.Label("Age: " + WindowRoster.SelectedKerbal.Age.ToString("##0"));
          GUILayout.Label("Next Bday: " + KSPUtil.PrintDate((int)WindowRoster.SelectedKerbal.TimeNextBirthday, false));
        }
      }
      else
      {
        GUILayout.Label(WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Trait + ")", RMStyle.LabelStyleBold, GUILayout.Width(300));
        if (RMLifeSpan.Instance.RMGameSettings.EnableAging)
        {
          GUILayout.Label("Age: " + WindowRoster.SelectedKerbal.Age.ToString("##0"));
          GUILayout.Label("Next Bday: " + KSPUtil.PrintDate((int)WindowRoster.SelectedKerbal.TimeNextBirthday, false));
        }
      }
      GUILayout.EndHorizontal();

      if (!string.IsNullOrEmpty(RMAddon.SaveMessage))
      {
        GUILayout.Label(RMAddon.SaveMessage, RMStyle.ErrorLabelRedStyle);
      }
      if (RMSettings.EnableKerbalRename)
      {
        WindowRoster.DisplaySelectProfession();
      }
      WindowRoster.DisplaySelectGender();

      WindowRoster.DisplaySelectSuit(ref WindowRoster.SelectedKerbal.Suit);

      GUILayout.Label("Courage");
      WindowRoster.SelectedKerbal.Courage = GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Courage, 0, 1, GUILayout.Width(300));

      GUILayout.Label("Stupidity");
      WindowRoster.SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Stupidity, 0, 1, GUILayout.Width(300));

      WindowRoster.SelectedKerbal.Badass = GUILayout.Toggle(WindowRoster.SelectedKerbal.Badass, "Badass");

      GUILayout.EndScrollView();

      WindowRoster.DisplayActionButtonsEdit();
    }
  }
}