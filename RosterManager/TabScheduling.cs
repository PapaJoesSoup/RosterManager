using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace RosterManager
{
    class TabScheduling
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
            GUILayout.Label("Kerbal Scheduling", RMStyle.LabelStyleBold);
            GUILayout.Label(WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }
            GUILayout.EndScrollView();

            WindowRoster.DisplayEditActionButtons(ref rect, ref label, ref toolTip);
        }
    }
}
