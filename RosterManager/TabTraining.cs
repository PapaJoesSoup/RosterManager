using UnityEngine;

namespace RosterManager
{
    internal class TabTraining
    {
        #region Properties

        internal static bool ShowToolTips = true;
        internal static string ToolTip = "";
        internal static bool ToolTipActive = true;

        internal static bool _ShowExperienceTab = true;
        internal static bool _ShowTeamTab = false;
        internal static bool _ShowQualificationTab = false;

        internal static bool ShowExperienceTab
        {
            get
            {
                return _ShowExperienceTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowExperienceTab = value;
            }
        }

        internal static bool ShowTeamTab
        {
            get
            {
                return _ShowTeamTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowTeamTab = value;
            }
        }

        internal static bool ShowQualificationTab
        {
            get
            {
                return _ShowQualificationTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowQualificationTab = value;
            }
        }

        #endregion Properties

        private static Vector2 ScrollDetailsPosition = Vector2.zero;

        internal static void Display()
        {
            ScrollDetailsPosition = GUILayout.BeginScrollView(ScrollDetailsPosition, RMStyle.ScrollStyle, GUILayout.Height(210), GUILayout.Width(680));
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";

            GUILayout.Label("Kerbal Training:  " + WindowRoster.SelectedKerbal.Name + " - (" + WindowRoster.SelectedKerbal.Trait + ")", RMStyle.LabelStyleBold, GUILayout.Width(500));

            DisplayTabButtons();
            DisplaySelectedTab(ref rect, ref label, ref toolTip);

            GUILayout.EndScrollView();

            WindowRoster.DisplayEditActionButtons(ref rect, ref label, ref toolTip);
        }

        #region Tab management

        private static void DisplayTabButtons()
        {
            GUILayout.BeginHorizontal();
            var ExperienceStyle = ShowExperienceTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Experience", ExperienceStyle, GUILayout.Height(20)))
            {
                ShowExperienceTab = true;
            }
            var TeamStyle = ShowTeamTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Team", TeamStyle, GUILayout.Height(20)))
            {
                ShowTeamTab = true;
            }
            var QualificationStyle = ShowQualificationTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Qualification", QualificationStyle, GUILayout.Height(20)))
            {
                ShowQualificationTab = true;
            }
            GUILayout.EndHorizontal();
        }

        internal static void DisplaySelectedTab(ref Rect rect, ref string label, ref string toolTip)
        {
            if (ShowExperienceTab)
                TabExperienceDisplay(ref rect, ref label, ref toolTip);
            else if (ShowQualificationTab)
                TabQualificationDisplay(ref rect, ref label, ref toolTip);
            else if (ShowTeamTab)
                TabTeamDisplay(ref rect, ref label, ref toolTip);
        }

        private static void ResetTabs()
        {
            _ShowExperienceTab = _ShowExperienceTab = _ShowTeamTab = _ShowQualificationTab = false;
        }

        #endregion Tab management

        #region Tab Display

        private static void TabExperienceDisplay(ref Rect rect, ref string label, ref string toolTip)
        {
            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }

            //GUILayout.Label("", GUILayout.Width(10));
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

            if (RMSettings.EnableSalaries)
            {
                GUILayout.Label("Salary");
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(10));
                GUILayout.Label("0", GUILayout.Width(10));
                WindowRoster.SelectedKerbal.Salary = (int)GUILayout.HorizontalSlider((float)WindowRoster.SelectedKerbal.Salary, 0, 100000, GUILayout.Width(300));
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
                GUILayout.Label(WindowRoster.SelectedKerbal.Salary.ToString() + " / 100,000 " + RMSettings.SalaryPeriod);
                GUILayout.EndHorizontal();
            }
        }

        private static void TabTeamDisplay(ref Rect rect, ref string label, ref string toolTip)
        {
        }

        private static void TabQualificationDisplay(ref Rect rect, ref string label, ref string toolTip)
        {
        }

        #endregion Tab Display
    }
}