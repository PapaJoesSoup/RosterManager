using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RosterManager
{
    internal static class RMStyle
    {
        internal static GUIStyle WindowStyle;
        internal static GUIStyle IconStyle;
        internal static GUIStyle ButtonSourceStyle;
        internal static GUIStyle ButtonTargetStyle;
        internal static GUIStyle ButtonToggledSourceStyle;
        internal static GUIStyle ButtonToggledTargetStyle;
        internal static GUIStyle ButtonStyle;
        internal static GUIStyle ButtonToggledStyle;
        internal static GUIStyle ButtonStyleLeft;
        internal static GUIStyle ButtonToggledStyleLeft;
        internal static GUIStyle ErrorLabelRedStyle;
        internal static GUIStyle LabelStyle;
        internal static GUIStyle LabelStyleBold;
        internal static GUIStyle LabelStyleBoldCenter;
        internal static GUIStyle LabelStyleRed;
        internal static GUIStyle LabelStyleYellow;
        internal static GUIStyle LabelStyleGreen;
        internal static GUIStyle LabelStyleCyan;
        internal static GUIStyle LabelStyleHdrSort;
        internal static GUIStyle ToolTipStyle;
        internal static GUIStyle ScrollStyle;

        internal static void SetupGUI()
        {
            GUI.skin = null;
            //GUI.skin = HighLogic.Skin;
            if (WindowStyle == null)
            {
                RMSettings.LoadColors();
                SetStyles();
            }
        }

        internal static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);
            IconStyle = new GUIStyle();

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.hover.textColor = Color.white;
            ButtonStyle.fontSize = 14;
            ButtonStyle.fontStyle = FontStyle.Normal;

            ButtonToggledStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledStyle.normal.textColor = Color.green;
            ButtonToggledStyle.fontSize = 14;
            ButtonToggledStyle.hover.textColor = Color.white;
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;
            ButtonToggledStyle.fontStyle = FontStyle.Normal;


            ButtonStyleLeft = new GUIStyle(GUI.skin.button);
            ButtonStyleLeft.normal.textColor = Color.white;
            ButtonStyleLeft.hover.textColor = Color.green;
            ButtonStyleLeft.fontSize = 12;
            ButtonStyleLeft.fontStyle = FontStyle.Normal;
            ButtonStyleLeft.alignment = TextAnchor.MiddleLeft;

            ButtonToggledStyleLeft = new GUIStyle(GUI.skin.button);
            ButtonToggledStyleLeft.normal.textColor = Color.green;
            ButtonToggledStyleLeft.fontSize = 12;
            ButtonToggledStyleLeft.hover.textColor = Color.white;
            ButtonToggledStyleLeft.normal.background = ButtonToggledStyleLeft.onActive.background;
            ButtonToggledStyleLeft.fontStyle = FontStyle.Normal;
            ButtonToggledStyleLeft.alignment = TextAnchor.MiddleLeft;

            ButtonSourceStyle = new GUIStyle(GUI.skin.button);
            ButtonSourceStyle.normal.textColor = Color.white;
            ButtonSourceStyle.fontSize = 14;
            ButtonSourceStyle.hover.textColor = Color.blue;
            ButtonSourceStyle.fontStyle = FontStyle.Normal;
            ButtonSourceStyle.alignment = TextAnchor.UpperLeft;

            ButtonToggledSourceStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledSourceStyle.normal.textColor = RMSettings.Colors[RMSettings.SourcePartColor];
            ButtonToggledSourceStyle.fontSize = 14;
            ButtonToggledSourceStyle.hover.textColor = Color.blue;
            ButtonToggledSourceStyle.hover.textColor = Color.blue;
            ButtonToggledSourceStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
            ButtonToggledSourceStyle.fontStyle = FontStyle.Normal;
            ButtonToggledSourceStyle.alignment = TextAnchor.UpperLeft;

            ButtonTargetStyle = new GUIStyle(GUI.skin.button);
            ButtonTargetStyle.normal.textColor = Color.white;
            ButtonTargetStyle.fontSize = 14;
            ButtonTargetStyle.hover.textColor = Color.blue;
            ButtonTargetStyle.fontStyle = FontStyle.Normal;
            ButtonTargetStyle.alignment = TextAnchor.UpperLeft;

            ButtonToggledTargetStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledTargetStyle.normal.textColor = RMSettings.Colors[RMSettings.TargetPartColor];
            ButtonToggledTargetStyle.fontSize = 14;
            ButtonToggledTargetStyle.hover.textColor = Color.blue;
            ButtonToggledTargetStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
            ButtonToggledTargetStyle.fontStyle = FontStyle.Normal;
            ButtonToggledTargetStyle.alignment = TextAnchor.UpperLeft;

            ErrorLabelRedStyle = new GUIStyle(GUI.skin.label);
            ErrorLabelRedStyle.normal.textColor = Color.red;

            LabelStyle = new GUIStyle(GUI.skin.label);

            LabelStyleBold = new GUIStyle(GUI.skin.label);
            LabelStyleBold.fontSize = 14;
            LabelStyleBold.fontStyle = FontStyle.Bold;

            LabelStyleBoldCenter = new GUIStyle(GUI.skin.label);
            LabelStyleBoldCenter.fontSize = 16;
            LabelStyleBoldCenter.fontStyle = FontStyle.Bold;
            LabelStyleBoldCenter.alignment = TextAnchor.MiddleCenter;

            LabelStyleRed = new GUIStyle(LabelStyle);
            LabelStyleRed.normal.textColor = Color.red;

            LabelStyleYellow = new GUIStyle(LabelStyle);
            LabelStyleYellow.normal.textColor = Color.yellow;

            LabelStyleGreen = new GUIStyle(LabelStyle);
            LabelStyleGreen.normal.textColor = Color.green;

            LabelStyleCyan = new GUIStyle(LabelStyle);
            LabelStyleCyan.normal.textColor = Color.cyan;

            LabelStyleHdrSort = new GUIStyle(GUI.skin.label);
            LabelStyleHdrSort.fontStyle = FontStyle.Bold;

            ToolTipStyle = new GUIStyle(GUI.skin.label);
            ToolTipStyle.alignment = TextAnchor.MiddleLeft;
            ToolTipStyle.wordWrap = false;
            ToolTipStyle.fontStyle = FontStyle.Normal;
            ToolTipStyle.normal.textColor = Color.yellow;

            ScrollStyle = new GUIStyle(GUI.skin.box);

        }
    }
}
