using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace RosterManager
{
    class TabTraining
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
            GUILayout.Label("Kerbal Training", RMStyle.LabelStyleBold);
            GUILayout.Label(WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }

            GUILayout.Label("", GUILayout.Width(10));
            GUILayout.Label("Skill");
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(10));
            GUILayout.Label("0", GUILayout.Width(10));
            WindowRoster.SelectedKerbal.Skill = (int)GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Skill, 0, 5, GUILayout.MaxWidth(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            GUILayout.Label(WindowRoster.SelectedKerbal.Skill.ToString() + " / 5");
            GUILayout.EndHorizontal();

            GUILayout.Label("Experience");
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(10));
            GUILayout.Label("0", GUILayout.Width(10));
            WindowRoster.SelectedKerbal.Experience = (int)GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Experience, 0, 99999, GUILayout.MaxWidth(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            GUILayout.Label(WindowRoster.SelectedKerbal.Experience.ToString() + " / 99999");
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            WindowRoster.DisplayEditActionButtons(ref rect, ref label, ref toolTip);
        }
    }
}
