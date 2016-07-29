using System.Linq;
using RosterManager.InternalObjects;
using UnityEngine;

namespace RosterManager.Windows.Tabs
{
  internal class TabTraining
  {
    #region Properties

    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    internal static bool ToolTipActive = true;

    private static bool _showExperienceTab = true;
    private static bool _showTeamTab;
    private static bool _showQualificationTab;

    internal static bool ShowExperienceTab
    {
      get
      {
        return _showExperienceTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showExperienceTab = value;
      }
    }

    internal static bool ShowTeamTab
    {
      get
      {
        return _showTeamTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showTeamTab = value;
      }
    }

    internal static bool ShowQualificationTab
    {
      get
      {
        return _showQualificationTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showQualificationTab = value;
      }
    }

    #endregion Properties

    private static Vector2 _scrollDetailsPosition = Vector2.zero;

    internal static void Display()
    {
      _scrollDetailsPosition = GUILayout.BeginScrollView(_scrollDetailsPosition, RMStyle.ScrollStyle, GUILayout.Height(210), GUILayout.Width(680));
      GUILayout.Label("Kerbal Training:  " + WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Trait + ")", RMStyle.LabelStyleBold, GUILayout.Width(500));

      DisplayTabButtons();
      DisplaySelectedTab();

      GUILayout.EndScrollView();

      WindowRoster.DisplayActionButtonsEdit();
    }

    #region Tab management

    private static void DisplayTabButtons()
    {
      GUILayout.BeginHorizontal();
      var experienceStyle = ShowExperienceTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Experience", experienceStyle, GUILayout.Height(20)))
      {
        ShowExperienceTab = true;
      }
      var teamStyle = ShowTeamTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Team", teamStyle, GUILayout.Height(20)))
      {
        ShowTeamTab = true;
      }
      var qualificationStyle = ShowQualificationTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Qualification", qualificationStyle, GUILayout.Height(20)))
      {
        ShowQualificationTab = true;
      }
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab()
    {
      if (ShowExperienceTab)
        TabExperienceDisplay();
      else if (ShowQualificationTab)
        TabQualificationDisplay();
      else if (ShowTeamTab)
        TabTeamDisplay();
    }

    private static void ResetTabs()
    {
      _showExperienceTab = _showExperienceTab = _showTeamTab = _showQualificationTab = false;
    }

    #endregion Tab management

    #region Tab Display

    private static void TabExperienceDisplay()
    {
      if (!string.IsNullOrEmpty(RMAddon.SaveMessage))
      {
        GUILayout.Label(RMAddon.SaveMessage, RMStyle.ErrorLabelRedStyle);
      }

      //GUILayout.Label("", GUILayout.Width(10));
      GUILayout.Label("Skill");
      GUILayout.BeginHorizontal();
      GUILayout.Label("", GUILayout.Width(10));
      GUILayout.Label("0", GUILayout.Width(10));
      WindowRoster.SelectedKerbal.Skill = (int)GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Skill, 0, 5, GUILayout.MaxWidth(300));
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUILayout.Label(WindowRoster.SelectedKerbal.Skill + " / 5");
      GUILayout.EndHorizontal();

      GUILayout.Label("Experience");
      GUILayout.BeginHorizontal();
      GUILayout.Label("", GUILayout.Width(10));
      GUILayout.Label("0", GUILayout.Width(10));
      WindowRoster.SelectedKerbal.Experience = (int)GUILayout.HorizontalSlider(WindowRoster.SelectedKerbal.Experience, 0, 99999, GUILayout.MaxWidth(300));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUILayout.Label(WindowRoster.SelectedKerbal.Experience + " / 99999");
      GUILayout.EndHorizontal();

      if (RMLifeSpan.Instance.RMGameSettings.EnableSalaries && (WindowRoster.SelectedKerbal.Type == ProtoCrewMember.KerbalType.Crew || WindowRoster.SelectedKerbal.Type == ProtoCrewMember.KerbalType.Unowned))
      {
        var kerbal = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.FirstOrDefault(a => a.Key == WindowRoster.SelectedKerbal.Name);
        if (kerbal.Key != null)
        {
          if (kerbal.Value.SalaryContractDispute)
            GUI.enabled = false;
        }
        GUILayout.Label("Salary");
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(10));
        GUILayout.Label("0", GUILayout.Width(10));
        WindowRoster.SelectedKerbal.Salary = (int)GUILayout.HorizontalSlider((float)WindowRoster.SelectedKerbal.Salary, 0, 100000, GUILayout.Width(300));
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips)
          ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
        GUILayout.Label(WindowRoster.SelectedKerbal.Salary.ToString("###,##0") + " / 100,000 " + RMLifeSpan.Instance.RMGameSettings.SalaryPeriod);
        GUILayout.EndHorizontal();
      }
      GUI.enabled = true;
    }

    private static void TabTeamDisplay()
    {
    }

    private static void TabQualificationDisplay()
    {
    }

    #endregion Tab Display
  }
}