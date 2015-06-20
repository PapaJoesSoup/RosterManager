using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace RosterManager
{
    class TabAttributes
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
            GUILayout.Label(WindowRoster.SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal", RMStyle.LabelStyleBold);
            if (RMSettings.EnableKerbalRename)
            {
                GUILayout.BeginHorizontal();
                WindowRoster.SelectedKerbal.Name = GUILayout.TextField(WindowRoster.SelectedKerbal.Name, GUILayout.MaxWidth(300));
                GUILayout.Label(" - (" + WindowRoster.SelectedKerbal.Kerbal.experienceTrait.Title + ")");
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label(WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }
            if (RMSettings.EnableKerbalRename && RMSettings.RenameWithProfession)
            {
                WindowRoster.DisplaySelectProfession();
            }
            WindowRoster.DisplaySelectGender();
            WindowRoster.SelectedKerbal.Gender = WindowRoster.gender;

            GUILayout.Label("Courage");
            WindowRoster.SelectedKerbal.Courage = GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

            GUILayout.Label("Stupidity");
            WindowRoster.SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Stupidity, 0, 1, GUILayout.MaxWidth(300));

            WindowRoster.SelectedKerbal.Badass = GUILayout.Toggle(WindowRoster.SelectedKerbal.Badass, "Badass");

            GUILayout.EndScrollView();

            WindowRoster.DisplayEditActionButtons(ref rect, ref label, ref toolTip);
        }

    }
}
