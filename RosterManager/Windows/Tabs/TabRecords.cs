using UnityEngine;

namespace RosterManager.Windows.Tabs
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

            //if (GUILayout.Button("Enter Notes", GUILayout.Width(100)))
            //{
            // Let's do a modal window here.  child of parent, moves with parent window.
            // DisplayNotesWindow()
            GUILayout.BeginVertical();
            GUILayout.Label("Notes: ", RMStyle.LabelStyleBold, GUILayout.Width(500));
            var base64EncodedBytes = System.Convert.FromBase64String(WindowRoster.SelectedKerbal.Notes);
            string inputString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        
        inputString = GUILayout.TextArea(inputString, 2046, RMStyle.RichTextStyle, GUILayout.Width(660), GUILayout.Height(150));
            var richTextBytes = System.Text.Encoding.UTF8.GetBytes(inputString);
        WindowRoster.SelectedKerbal.Notes = System.Convert.ToBase64String(richTextBytes);
            GUILayout.EndVertical();
            //}

      //End Tab contents
      GUILayout.EndScrollView();

      WindowRoster.DisplayActionButtonsEdit();
    }
  }
}