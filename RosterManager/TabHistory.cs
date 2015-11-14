using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace RosterManager
{
    class TabHistory
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
            GUILayout.Label("Kerbal Flight History:  " + WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.Width(500));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }

            // Begin Tab contents.
            FlightLog thisLog = WindowRoster.SelectedKerbal.Kerbal.flightLog;

            foreach (FlightLog.Entry thisEntry in thisLog.Entries)
            {
                GUILayout.Label(thisEntry.flight.ToString() + " - " + thisEntry.target + " - " + thisEntry.type);
            }

            //End Tab contents
            GUILayout.EndScrollView();

            WindowRoster.DisplayEditActionButtons(ref rect, ref label, ref toolTip);
        }
    }
}
