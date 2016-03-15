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
    internal static GUIStyle LabelStyleMagenta;
    internal static GUIStyle LabelStyleYellow;
    internal static GUIStyle LabelStyleGreen;
    internal static GUIStyle LabelStyleCyan;
    internal static GUIStyle LabelStyleHdrSort;
    internal static GUIStyle ToolTipStyle;
    internal static GUIStyle ScrollStyle;
    internal static GUIStyle RichTextStyle;

    internal static void SetupGui()
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

      ButtonStyle = new GUIStyle(GUI.skin.button)
      {
        normal = {textColor = Color.white},
        hover = {textColor = Color.white},
        fontSize = 14,
        fontStyle = FontStyle.Normal
      };

      ButtonToggledStyle = new GUIStyle(GUI.skin.button)
      {
        normal = {textColor = Color.green},
        fontSize = 14,
        hover = {textColor = Color.white},
        fontStyle = FontStyle.Normal
      };
      ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;

      ButtonStyleLeft = new GUIStyle(GUI.skin.button)
      {
        normal = {textColor = Color.white},
        hover = {textColor = Color.green},
        fontSize = 12,
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft
      };

      ButtonToggledStyleLeft = new GUIStyle(GUI.skin.button)
      {
        normal = {textColor = Color.green},
        fontSize = 12,
        hover = {textColor = Color.white},
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.MiddleLeft
      };
      ButtonToggledStyleLeft.normal.background = ButtonToggledStyleLeft.onActive.background;

      ButtonSourceStyle = new GUIStyle(GUI.skin.button)
      {
        normal = {textColor = Color.white},
        fontSize = 14,
        hover = {textColor = Color.blue},
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.UpperLeft
      };

      ButtonToggledSourceStyle = new GUIStyle(GUI.skin.button)
      {
        normal = {textColor = RMSettings.Colors[RMSettings.SourcePartColor]},
        fontSize = 14,
        hover = {textColor = Color.blue},
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.UpperLeft
      };
      ButtonToggledSourceStyle.normal.background = ButtonToggledSourceStyle.onActive.background;

      ButtonTargetStyle = new GUIStyle(GUI.skin.button)
      {
        normal = {textColor = Color.white},
        fontSize = 14,
        hover = {textColor = Color.blue},
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.UpperLeft
      };

      ButtonToggledTargetStyle = new GUIStyle(GUI.skin.button)
      {
        fontSize = 14,
        normal = {textColor = RMSettings.Colors[RMSettings.TargetPartColor]},
        hover = {textColor = Color.blue},
        fontStyle = FontStyle.Normal,
        alignment = TextAnchor.UpperLeft
      };
      ButtonToggledTargetStyle.normal.background = ButtonToggledSourceStyle.onActive.background;

      ErrorLabelRedStyle = new GUIStyle(GUI.skin.label) {normal = {textColor = Color.red}};

      LabelStyle = new GUIStyle(GUI.skin.label);

      LabelStyleBold = new GUIStyle(GUI.skin.label)
      {
        fontSize = 14,
        fontStyle = FontStyle.Bold
      };

      LabelStyleBoldCenter = new GUIStyle(GUI.skin.label)
      {
        fontSize = 16,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
      };

      LabelStyleRed = new GUIStyle(LabelStyle) {normal = {textColor = Color.red}};

      LabelStyleMagenta = new GUIStyle(LabelStyle) {normal = {textColor = Color.magenta}};

      LabelStyleYellow = new GUIStyle(LabelStyle) {normal = {textColor = Color.yellow}};

      LabelStyleGreen = new GUIStyle(LabelStyle) {normal = {textColor = Color.green}};

      LabelStyleCyan = new GUIStyle(LabelStyle) {normal = {textColor = Color.cyan}};

      LabelStyleHdrSort = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold};

      ToolTipStyle = new GUIStyle(GUI.skin.label)
      {
        alignment = TextAnchor.MiddleLeft,
        wordWrap = false,
        fontStyle = FontStyle.Normal,
        normal = {textColor = Color.yellow}
      };

      ScrollStyle = new GUIStyle(GUI.skin.box);

      RichTextStyle = new GUIStyle(GUI.skin.textArea)
      {
        wordWrap = true,
        fontStyle = FontStyle.Normal,
        richText = true        
      };
    }
  }
}