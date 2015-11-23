using UnityEngine;

namespace RosterManager
{
    internal class TabAttributes
    {
        internal static bool ShowToolTips = true;
        internal static string ToolTip = "";
        internal static bool ToolTipActive = true;
        private static Vector2 ScrollDetailsPosition = Vector2.zero;

        internal static void Display()
        {
            ScrollDetailsPosition = GUILayout.BeginScrollView(ScrollDetailsPosition, RMStyle.ScrollStyle, GUILayout.Height(210), GUILayout.Width(680));
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label(WindowRoster.SelectedKerbal.IsNew ? "Create a Kerbal" : "Kerbal Attributes", RMStyle.LabelStyleBold);

            GUILayout.BeginHorizontal();
            if (RMSettings.EnableKerbalRename)
            {
                
                GUILayout.Label("Name:", GUILayout.Width(80));
                WindowRoster.SelectedKerbal.Name = GUILayout.TextField(WindowRoster.SelectedKerbal.Name, GUILayout.Width(230));
                GUILayout.Label(" - (" + WindowRoster.SelectedKerbal.Kerbal.trait + ")");
                if (RMSettings.EnableAging)
                {
                    GUILayout.Label("Age: " + WindowRoster.SelectedKerbal.age.ToString("##0"));
                }                
            }
            else
            {
                GUILayout.Label(WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Trait + ")", RMStyle.LabelStyleBold, GUILayout.Width(300));
                if (RMSettings.EnableAging)
                {
                    GUILayout.Label("Age: " + WindowRoster.SelectedKerbal.age.ToString("##0"));
                }
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }
            if (RMSettings.EnableKerbalRename)
            {
                WindowRoster.DisplaySelectProfession();
            }
            WindowRoster.DisplaySelectGender();
            WindowRoster.SelectedKerbal.Gender = WindowRoster.gender;

            GUILayout.Label("Courage");
            WindowRoster.SelectedKerbal.Courage = GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Courage, 0, 1, GUILayout.Width(300));

            GUILayout.Label("Stupidity");
            WindowRoster.SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Stupidity, 0, 1, GUILayout.Width(300));

            WindowRoster.SelectedKerbal.Badass = GUILayout.Toggle(WindowRoster.SelectedKerbal.Badass, "Badass");

            GUILayout.EndScrollView();

            WindowRoster.DisplayEditActionButtons(ref rect, ref label, ref toolTip);
        }
    }
}