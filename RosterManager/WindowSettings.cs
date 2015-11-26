using UnityEngine;

namespace RosterManager
{
    internal static class WindowSettings
    {
        #region Settings Window (GUI)

        internal static Rect Position = new Rect(0, 0, 0, 0);
        internal static bool ShowWindow = false;
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;
        internal static string ToolTip = "";

        private static Vector2 ScrollViewerPosition = Vector2.zero;

        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            Rect rect = new Rect();
            ToolTipActive = false;

            rect = new Rect(371, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                ToolTip = "";
                WindowSettings.ShowWindow = false;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 0, 0);

            // Store settings in case we cancel later...
            RMSettings.StoreTempSettings();

            GUILayout.BeginVertical();
            ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, GUILayout.Height(280), GUILayout.Width(375));
            GUILayout.BeginVertical();

            DisplayOptions();

            DisplayHighlighting();

            DisplayToolTips();

            DisplayConfiguration();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                RMSettings.SaveSettings();
                WindowSettings.ShowWindow = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                // We've canclled, so restore original settings.
                RMSettings.RestoreTempSettings();
                WindowSettings.ShowWindow = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            RMSettings.RepositionWindows("WindowSettings");
        }

        private static void DisplayConfiguration()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";

            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
            GUILayout.Label("Configuraton");
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            if (!ToolbarManager.ToolbarAvailable)
            {
                if (RMSettings.EnableBlizzyToolbar)
                    RMSettings.EnableBlizzyToolbar = false;
                GUI.enabled = false;
            }
            else
                GUI.enabled = true;

            label = "Enable Blizzy Toolbar (Replaces Stock Toolbar)";
            RMSettings.EnableBlizzyToolbar = GUILayout.Toggle(RMSettings.EnableBlizzyToolbar, label, GUILayout.Width(300));

            GUI.enabled = true;
            label = "Enable Debug Window";
            WindowDebugger.ShowWindow = GUILayout.Toggle(WindowDebugger.ShowWindow, label, GUILayout.Width(300));

            label = "Enable Verbose Logging";
            RMSettings.VerboseLogging = GUILayout.Toggle(RMSettings.VerboseLogging, label, GUILayout.Width(300));

            label = "Enable RM Debug Window On Error";
            RMSettings.AutoDebug = GUILayout.Toggle(RMSettings.AutoDebug, label, GUILayout.Width(300));

            label = "Save Error log on Exit";
            RMSettings.SaveLogOnExit = GUILayout.Toggle(RMSettings.SaveLogOnExit, label, GUILayout.Width(300));

            // create Limit Error Log Length slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Error Log Length: ", GUILayout.Width(140));
            RMSettings.ErrorLogLength = GUILayout.TextField(RMSettings.ErrorLogLength, GUILayout.Width(40));
            GUILayout.Label("(lines)", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            label = "Enable Kerbal Renaming";
            toolTip = "Allows renaming a Kerbal.";
            RMSettings.EnableKerbalRename = GUILayout.Toggle(RMSettings.EnableKerbalRename, new GUIContent(label, toolTip), GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            label = "Enable Kerbal Aging";
            toolTip = "Your Kerbals will age and eventually die from old age.";
            RMLifeSpan.Instance.rmGameSettings.EnableAging = GUILayout.Toggle(RMLifeSpan.Instance.rmGameSettings.EnableAging, new GUIContent(label, toolTip), GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            if (!RMLifeSpan.Instance.rmGameSettings.EnableAging)
                GUI.enabled = false;
            GUILayout.BeginHorizontal();
            toolTip = "Average age of new Applicant Kerbals.";
            GUILayout.Label(new GUIContent("Kerbal Minimum Age: ", toolTip), GUILayout.Width(140));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            string strMinimum_Age = RMLifeSpan.Instance.rmGameSettings.Minimum_Age.ToString();
            int Minimum_age = RMLifeSpan.Instance.rmGameSettings.Minimum_Age;
            strMinimum_Age = GUILayout.TextField(strMinimum_Age, GUILayout.Width(40));
            GUILayout.Label("(Years)", GUILayout.Width(50));            
            if (int.TryParse(strMinimum_Age, out Minimum_age))
                RMLifeSpan.Instance.rmGameSettings.Minimum_Age = Minimum_age;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            toolTip = "Average Lifespan of Kerbals (how long they live).";
            GUILayout.Label(new GUIContent("Kerbal Lifespan: ", toolTip), GUILayout.Width(140));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            string strMaximum_Age = RMLifeSpan.Instance.rmGameSettings.Maximum_Age.ToString();
            int Maximum_age = RMLifeSpan.Instance.rmGameSettings.Maximum_Age;
            strMaximum_Age = GUILayout.TextField(strMaximum_Age, GUILayout.Width(40));
            GUILayout.Label("(Years)", GUILayout.Width(50));
            if (int.TryParse(strMaximum_Age, out Maximum_age))
                RMLifeSpan.Instance.rmGameSettings.Maximum_Age = Maximum_age;
            GUILayout.EndHorizontal();

            GUI.enabled = true;
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                GUI.enabled = false;
            label = "Enable Salaries (career game only)";
            toolTip = "Kerbals must be paid Salaries.";
            RMLifeSpan.Instance.rmGameSettings.EnableSalaries = GUILayout.Toggle(RMLifeSpan.Instance.rmGameSettings.EnableSalaries, new GUIContent(label, toolTip), GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            if (!RMLifeSpan.Instance.rmGameSettings.EnableSalaries)
                GUI.enabled = false;
            DisplaySelectSalaryPeriod();

            GUILayout.BeginHorizontal();
            toolTip = "Default salary for Kerbals.";
            GUILayout.Label(new GUIContent("Default Kerbal Salary: ", toolTip), GUILayout.Width(140));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            string strDef_salary = RMLifeSpan.Instance.rmGameSettings.DefaultSalary.ToString();
            double Default_salary = RMLifeSpan.Instance.rmGameSettings.DefaultSalary;
            strDef_salary = GUILayout.TextField(strDef_salary, GUILayout.Width(70));
            GUILayout.Label("(Funds)", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            if (double.TryParse(strDef_salary, out Default_salary))
                RMLifeSpan.Instance.rmGameSettings.DefaultSalary = Default_salary;

            GUI.enabled = true;
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                GUI.enabled = false;

            label = "Charge funds for Profession Change (career game only)";
            toolTip = "Charge funds for changing a Kerbal's profession.";
            RMLifeSpan.Instance.rmGameSettings.ChangeProfessionCharge = GUILayout.Toggle(RMLifeSpan.Instance.rmGameSettings.ChangeProfessionCharge, new GUIContent(label, toolTip), GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            if (!RMLifeSpan.Instance.rmGameSettings.ChangeProfessionCharge)
                GUI.enabled = false;

            GUILayout.BeginHorizontal();
            toolTip = "Cost of changing a Kerbals Profession.";
            GUILayout.Label(new GUIContent("Change Profession Cost: ", toolTip), GUILayout.Width(140));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            string strChg_prof = RMLifeSpan.Instance.rmGameSettings.ChangeProfessionCost.ToString();
            double Chg_prof = RMLifeSpan.Instance.rmGameSettings.ChangeProfessionCost;
            strChg_prof = GUILayout.TextField(strChg_prof, GUILayout.Width(70));
            GUILayout.Label("(Funds)", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            if (double.TryParse(strChg_prof, out Chg_prof))
                RMLifeSpan.Instance.rmGameSettings.ChangeProfessionCost = Chg_prof;

            GUI.enabled = true;
        }

        internal static void DisplaySelectSalaryPeriod()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("SalaryPeriod:", GUILayout.Width(80));
            RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisMonthly = GUILayout.Toggle(RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisMonthly, "Monthly", GUILayout.Width(70));
            if (RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisMonthly)
            {
                RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisYearly = false;
            }
            else
            {
                if (!RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisYearly)
                {
                    RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisMonthly = true;
                    RMLifeSpan.Instance.rmGameSettings.SalaryPeriod = "Monthly";
                }
            }
            RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisYearly = GUILayout.Toggle(RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisYearly, "Yearly", GUILayout.Width(80));
            if (RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisYearly)
                RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisMonthly = false;
            else
            {
                if (!RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisMonthly)
                {
                    RMLifeSpan.Instance.rmGameSettings.SalaryPeriodisYearly = true;
                    RMLifeSpan.Instance.rmGameSettings.SalaryPeriod = "Yearly";
                }
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayToolTips()
        {
            // Enable Tool Tips
            string label = "";
            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
            GUILayout.Label("ToolTips");
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            label = "Enable Tool Tips";
            RMSettings.ShowToolTips = GUILayout.Toggle(RMSettings.ShowToolTips, label, GUILayout.Width(300));

            GUI.enabled = RMSettings.ShowToolTips;
            label = "Settings Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowSettings.ShowToolTips = GUILayout.Toggle(WindowSettings.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Roster Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Debugger Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Contract Disputes Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowContractDispute.ShowToolTips = GUILayout.Toggle(WindowContractDispute.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private static void DisplayHighlighting()
        {
            string label = "";
            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
            GUILayout.Label("Highlighting");
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            // EnableHighlighting Mode
            GUILayout.BeginHorizontal();
            label = "Enable Highlighting";
            RMSettings.EnableHighlighting = GUILayout.Toggle(RMSettings.EnableHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
        }

        private static void DisplayOptions()
        {
            Rect rect = new Rect();
            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));
            if (!RMSettings.LockSettings)
                GUILayout.Label("Settings / Options");
            else
                GUILayout.Label("Settings / Options  (Locked.  Unlock in Config file)");
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            bool isEnabled = (!RMSettings.LockSettings);
            // Realism Mode
            GUI.enabled = isEnabled;
            GUIContent guiLabel = new GUIContent("Enable Realism Mode", "Turns on/off Realism Mode.\r\nWhen ON, causes changes in the interface and limits\r\nyour freedom to things that would not be 'Realistic'.\r\nWhen Off, Allows Fills, Dumps, Repeating Science, instantaneous Xfers, Crew Xfers anywwhere, etc.");
            RMSettings.RealismMode = GUILayout.Toggle(RMSettings.RealismMode, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            // LockSettings Mode
            GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Lock Settings  (If set ON, disable in config file)", "Locks the settings in this section so they cannot be altered in game.\r\nTo turn off Locking you MUST edit the Config.xml file.");
            RMSettings.LockSettings = GUILayout.Toggle(RMSettings.LockSettings, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowSettings.Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
        }

        #endregion Settings Window (GUI)
    }
}