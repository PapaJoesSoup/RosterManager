using DF;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosterManager
{
    internal static class WindowRoster
    {
        // Standard Window vars
        internal static float windowWidth = 700;

        internal static float windowHeight = 330;
        internal static Rect Position = new Rect(0, 0, 0, 0);
        internal static bool ShowWindow = false;
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;

        //Profession vars
        internal static bool isPilot = false;
        internal static bool isEngineer = false;
        internal static bool isScientist = false;

        internal static string KerbalProfession
        {
            get
            {
                if (isPilot)
                    return "Pilot";
                else if (isEngineer)
                    return "Engineer";
                else if (isScientist)
                    return "Scientist";
                else
                    return "";
            }
        }

        // Gender var
        internal static ProtoCrewMember.Gender gender = ProtoCrewMember.Gender.Male;

        //Kerbal List Filter vars
        internal static bool isAll = true;
        internal static bool isAssign = false;
        internal static bool isAvail = false;
        internal static bool isDead = false;
        internal static bool isFrozen = false;
        internal static bool isDispute = false;

        internal static bool OnCreate = false;

        internal static bool resetRosterSize
        {
            get
            {
                if (!OnCreate && SelectedKerbal == null)
                    return true;
                else
                    return false;
            }
        }

        private static RMKerbal _selectedKerbal;

        internal static RMKerbal SelectedKerbal
        {
            get { return _selectedKerbal; }
            set
            {
                _selectedKerbal = value;
                if (_selectedKerbal == null)
                {
                    RMAddon.saveMessage = string.Empty;
                }
            }
        }

        // Editor Tab bools

        internal static bool _ShowAttributesTab = true;
        internal static bool _ShowSchedulingTab = false;
        internal static bool _ShowTrainingTab = false;
        internal static bool _ShowHistoryTab = false;
        internal static bool _ShowMedicalTab = false;
        internal static bool _ShowRecordsTab = false;

        internal static bool ShowAttributesTab
        {
            get
            {
                return _ShowAttributesTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowAttributesTab = value;
            }
        }

        internal static bool ShowSchedulingTab
        {
            get
            {
                return _ShowSchedulingTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowSchedulingTab = value;
            }
        }

        internal static bool ShowTrainingTab
        {
            get
            {
                return _ShowTrainingTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowTrainingTab = value;
            }
        }

        internal static bool ShowHistoryTab
        {
            get
            {
                return _ShowHistoryTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowHistoryTab = value;
            }
        }

        internal static bool ShowMedicalTab
        {
            get
            {
                return _ShowMedicalTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowMedicalTab = value;
            }
        }

        internal static bool ShowRecordsTab
        {
            get
            {
                return _ShowRecordsTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowRecordsTab = value;
            }
        }

        private static Vector2 ScrollViewerPosition = Vector2.zero;

        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;

            Rect rect = new Rect(Position.width - 20, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                OnCreate = false;
                SelectedKerbal = null;
                ToolTip = "";
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.EDITOR || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT)
                    RMAddon.OnRMRosterToggle();
                else
                    ShowWindow = false;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 0, 0);
            try
            {
                GUIStyle style = GUI.skin.button;
                var defaultColor = style.normal.textColor;
                GUILayout.BeginVertical();
                DisplayRosterFilter();

                DisplayRosterListViewer();

                if (OnCreate)
                    CreateKerbalViewer();
                else if (SelectedKerbal != null)
                {
                    GUILayout.Label("Kerbal Manager", RMStyle.LabelStyleBoldCenter, GUILayout.Width(680));
                    DisplayTabButtons();
                    DisplaySelectedTab();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Create Kerbal"))
                    {
                        OnCreate = true;
                    }
                    var settingsStyle = WindowSettings.ShowWindow ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
                    if (GUILayout.Button("Settings", settingsStyle))
                    {
                        try
                        {
                            WindowSettings.ShowWindow = !WindowSettings.ShowWindow;
                            if (WindowSettings.ShowWindow)
                            {
                                // Store settings in case we cancel later...
                                RMSettings.StoreTempSettings();
                            }
                        }
                        catch (Exception ex)
                        {
                            Utilities.LogMessage(string.Format(" opening Settings Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                        }
                    }

                    if (RMSettings.EnableSalaries)
                    {
                        var disputesStyle = WindowContractDispute.ShowWindow ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
                        if (GUILayout.Button("Contract Disputes", disputesStyle))
                        {
                            try
                            {
                                WindowContractDispute.ShowWindow = !WindowContractDispute.ShowWindow;
                            }
                            catch (Exception ex)
                            {
                                Utilities.LogMessage(string.Format(" opening Contract Disputes Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                            }
                        }
                    }

                    rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 30 - ScrollViewerPosition.y);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
                RMSettings.RepositionWindows("WindowRoster");
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void DisplayRosterListViewer()
        {
            try
            {
                string buttonToolTip = string.Empty;
                GUIStyle hdrlabelStyle = new GUIStyle(GUI.skin.label);
                hdrlabelStyle.fontStyle = FontStyle.Bold;
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label("", hdrlabelStyle, GUILayout.Width(5));
                buttonToolTip = "Click to sort.  Click again to sort the opposite direction.";
                if (GUILayout.Button(new GUIContent("Name", buttonToolTip), hdrlabelStyle, GUILayout.Width(150)))
                    SortRosterList("Name");
                Rect rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                if (GUILayout.Button(new GUIContent("|Gender", buttonToolTip), hdrlabelStyle, GUILayout.Width(50)))
                    SortRosterList("Gender");
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                if (RMSettings.EnableAging)
                {
                    if (GUILayout.Button(new GUIContent("|Age", "Age of Kerbal"), hdrlabelStyle, GUILayout.Width(35)))
                        SortRosterList("Age");    
                    rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);
                }

                if (GUILayout.Button(new GUIContent("|Profession", buttonToolTip), hdrlabelStyle, GUILayout.Width(75)))
                    SortRosterList("Profession");
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                if (GUILayout.Button(new GUIContent("|Skill", buttonToolTip), hdrlabelStyle, GUILayout.Width(35)))
                    SortRosterList("Skill");
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                if (GUILayout.Button(new GUIContent("|Experience", buttonToolTip), hdrlabelStyle, GUILayout.Width(75)))
                    SortRosterList("Experience");
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                if (GUILayout.Button(new GUIContent("|Status", buttonToolTip), hdrlabelStyle, GUILayout.Width(200)))
                    SortRosterList("Status");
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);                                          

                GUILayout.EndHorizontal();

                ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, RMStyle.ScrollStyle, GUILayout.Height(230), GUILayout.Width(680));
                //foreach (ProtoCrewMember kerbal in RMAddon.AllCrew
                foreach (KeyValuePair<string, RMKerbal> rmkerbal in RMAddon.AllCrew)
                {
                    ProtoCrewMember kerbal = rmkerbal.Value.Kerbal;
                    if (CanDisplayKerbal(rmkerbal.Value))
                    {
                        GUIStyle labelStyle = null;
                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing
                            || rmkerbal.Value.salaryContractDispute)
                            labelStyle = RMStyle.LabelStyleRed;
                        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                            labelStyle = RMStyle.LabelStyleYellow;
                        else
                            labelStyle = RMStyle.LabelStyle;

                        // What vessel is this Kerbal Assigned to?
                        string rosterDetails = "";
                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                        {
                            foreach (Vessel thisVessel in FlightGlobals.Vessels)
                            {
                                List<ProtoCrewMember> crew = thisVessel.GetVesselCrew();
                                foreach (ProtoCrewMember crewMember in crew)
                                {
                                    if (crewMember == kerbal)
                                    {
                                        if (rmkerbal.Value.salaryContractDispute)
                                            rosterDetails = "Dispute - " + thisVessel.GetName().Replace("(unloaded)", "");
                                        else
                                            rosterDetails = "Assigned - " + thisVessel.GetName().Replace("(unloaded)", "");
                                        break;
                                    }
                                }
                            }
                        }
                        else if (InstalledMods.IsDFInstalled && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
                        {
                            // This kerbal could be frozen.  Lets find out...
                            rosterDetails = GetFrozenDetials(kerbal);
                            labelStyle = RMStyle.LabelStyleCyan;
                        }
                        else if (rmkerbal.Value.salaryContractDispute)
                        {
                            // This kerbal is in contract dispute
                            rosterDetails = "Contract Dispute";
                        }
                        else
                        {
                            // Since the kerbal has no vessel assignment, lets show what their status...
                            rosterDetails = kerbal.rosterStatus.ToString();
                        }
                        string buttonText = string.Empty;
                        GUILayout.BeginHorizontal();
                        buttonText = kerbal.name;
                        buttonToolTip = "Opens the Edit section for this kerbal.";
                        GUIStyle btnStyle = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? RMStyle.ButtonStyleLeft : RMStyle.ButtonToggledStyleLeft;
                        if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), btnStyle, GUILayout.Width(160)))
                        {
                            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
                            {
                                //Find the RMKerbal entry for the selected kerbal.
                                SelectedKerbal = RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.FirstOrDefault(a => a.Key == kerbal.name).Value;
                                if (SelectedKerbal == null) //Didn't find the RMKerbal entry? Should never happen? Create a new one just in case.
                                {
                                    SelectedKerbal = new RMKerbal(Planetarium.GetUniversalTime(), kerbal, true, true);
                                    RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.Add(kerbal.name, SelectedKerbal);
                                }
                                SetProfessionFlag();
                                gender = SelectedKerbal.Gender;
                            }
                            else
                            {
                                SelectedKerbal = null;
                                gender = ProtoCrewMember.Gender.Male;
                            }
                        }
                        rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50 - ScrollViewerPosition.y);
                        GUILayout.Label(kerbal.gender.ToString(), labelStyle, GUILayout.Width(50));
                        if (RMSettings.EnableAging)
                        {
                            KeyValuePair<string, RMKerbal> kerbalInfo = RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.FirstOrDefault(a => a.Key == kerbal.name);
                            if (kerbalInfo.Key != null)
                            {
                                GUILayout.Label(kerbalInfo.Value.age.ToString("##0"), labelStyle, GUILayout.Width(35));
                            }
                        }                        
                        GUILayout.Label(rmkerbal.Value.salaryContractDispute ? rmkerbal.Value.RealTrait : kerbal.trait, labelStyle, GUILayout.Width(75));
                        GUILayout.Label(kerbal.experienceLevel.ToString(), labelStyle, GUILayout.Width(35));
                        GUILayout.Label(kerbal.experience.ToString(), labelStyle, GUILayout.Width(75));
                        GUILayout.Label(rosterDetails, labelStyle, GUILayout.Width(240));

                        //if (!RMSettings.RealismMode || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        //    GUI.enabled = true;
                        //else
                        //    GUI.enabled = false;

                        //buttonText = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit" : "Cancel";
                        //if (GUI.enabled)
                        //    buttonToolTip = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit this Kerbal's characteristics" : "Cancel any changes to this Kerbal";
                        //else
                        //    buttonToolTip = "Kerbal is not available at this time.\r\nEditing is disabled";

                        //if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(60)))
                        //{
                        //    if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
                        //    {
                        //        SelectedKerbal = new ModKerbal(kerbal, false);
                        //        SetProfessionFlag();
                        //    }
                        //    else
                        //    {
                        //        SelectedKerbal = null;
                        //    }
                        //}

                        //rect = GUILayoutUtility.GetLastRect();
                        //if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        //    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50 - ScrollViewerPosition.y);

                        //    if (HighLogic.LoadedScene != GameScenes.SPACECENTER && ((RMSettings.RealismMode && RMAddon.IsPreLaunch) || !RMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && FlightGlobals.ActiveVessel != null && !RMAddon.VesselIsFull(FlightGlobals.ActiveVessel))
                        //    {
                        //        GUI.enabled = true;
                        //        buttonText = "Add";
                        //        buttonToolTip = "Adds a kerbal to the Active Vessel,\r\nin the first available seat.";
                        //    }
                        //    else if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Unowned) || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                        //    {
                        //        GUI.enabled = true;
                        //        buttonText = "Respawn";
                        //        buttonToolTip = "Brings a Kerbal back to life.\r\nWill then become available.";
                        //    }
                        //    else if (HighLogic.LoadedScene != GameScenes.SPACECENTER && ((RMSettings.RealismMode && RMAddon.IsPreLaunch) || !RMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
                        //    {
                        //        GUI.enabled = true;
                        //        buttonText = "Remove";
                        //        buttonToolTip = "Removes a Kerbal from the active vessel.\r\nWill then become available.";
                        //    }
                        //    else if (HighLogic.LoadedScene != GameScenes.SPACECENTER && (RMSettings.RealismMode && !RMAddon.IsPreLaunch) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        //    {
                        //        GUI.enabled = false;
                        //        buttonText = "Add";
                        //        buttonToolTip = "Add Disabled.  Realism Settings are preventing this action.\r\nTo add a Kerbal, Change your realism Settings.";
                        //    }
                        //    else
                        //    {
                        //        GUI.enabled = false;
                        //        buttonText = "--";
                        //        buttonToolTip = "Kerbal is not available (" + kerbal.rosterStatus + ").\r\nCurrent status does not allow any action.";
                        //    }

                        //    if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(70)))
                        //    {
                        //        if (buttonText == "Add")
                        //            RMAddon.AddCrewMember(kerbal, FlightGlobals.ActiveVessel);
                        //        else if (buttonText == "Respawn")
                        //            RMAddon.RespawnKerbal(kerbal);
                        //        else if (buttonText == "Remove")
                        //        {
                        //            // get part...
                        //            Part part = RMAddon.FindKerbalPart(kerbal);
                        //            if (part != null)
                        //                RMAddon.RemoveCrewMember(kerbal, part);
                        //        }
                        //    }
                        //    rect = GUILayoutUtility.GetLastRect();
                        //    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        //        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50 - ScrollViewerPosition.y);
                        GUILayout.EndHorizontal();
                        GUI.enabled = true;
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in RosterListViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void CreateKerbalViewer()
        {
            DisplaySelectProfession();
            DisplaySelectGender();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create", GUILayout.MaxWidth(80)))
            {
                bool kerbalFound = false;
                while (!kerbalFound)
                {
                    SelectedKerbal = RMKerbal.CreateKerbal();
                    if (SelectedKerbal.Trait == KerbalProfession && SelectedKerbal.Gender == gender)
                        kerbalFound = true;
                }
                if (!RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.ContainsKey(SelectedKerbal.Name))
                    RMLifeSpan.Instance.rmkerbals.ALLRMKerbals.Add(SelectedKerbal.Name, SelectedKerbal);
                OnCreate = false;
            }
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(80)))
            {
                OnCreate = false;
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayTabButtons()
        {
            GUILayout.BeginHorizontal();
            var AttributeStyle = ShowAttributesTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Attributes", AttributeStyle, GUILayout.Height(20)))
            {
                ShowAttributesTab = true;
            }
            var TrainingStyle = ShowTrainingTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Training", TrainingStyle, GUILayout.Height(20)))
            {
                ShowTrainingTab = true;
            }
            var MedicalStyle = ShowMedicalTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Medical", MedicalStyle, GUILayout.Height(20)))
            {
                ShowMedicalTab = true;
            }
            var ProfessionStyle = ShowSchedulingTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Scheduling", ProfessionStyle, GUILayout.Height(20)))
            {
                ShowSchedulingTab = true;
            }
            var HistoryStyle = ShowHistoryTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("History", HistoryStyle, GUILayout.Height(20)))
            {
                ShowHistoryTab = true;
            }
            var RecordStyle = ShowRecordsTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Records", RecordStyle, GUILayout.Height(20)))
            {
                ShowRecordsTab = true;
            }
            GUILayout.EndHorizontal();
        }

        private static void SetProfessionFlag()
        {
            if (SelectedKerbal.Trait == "Pilot")
            {
                isPilot = true;
                isEngineer = false;
                isScientist = false;
            }
            else if (SelectedKerbal.Trait == "Engineer")
            {
                isPilot = false;
                isEngineer = true;
                isScientist = false;
            }
            else
            {
                isPilot = false;
                isEngineer = false;
                isScientist = true;
            }
        }

        private static void DisplayRosterFilter()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Kerbal Filter:", GUILayout.Width(90));
            isAll = GUILayout.Toggle(isAll, "All", GUILayout.Width(50));
            if (isAll)
                isAssign = isAvail = isDead = isFrozen = isDispute = false;
            else
            {
                if (!isAssign && !isAvail && !isDead && !isFrozen && !isDispute)
                    isAll = true;
            }
            isAssign = GUILayout.Toggle(isAssign, "Assigned", GUILayout.Width(80));
            if (isAssign)
                isAll = isAvail = isDead = isFrozen = isDispute = false;
            else
            {
                if (!isAll && !isAvail && !isDead && !isFrozen && !isDispute)
                    isAssign = true;
            }
            isAvail = GUILayout.Toggle(isAvail, "Available", GUILayout.Width(80));
            if (isAvail)
                isAll = isAssign = isDead = isFrozen = isDispute = false;
            else
            {
                if (!isAll && !isAssign && !isDead && !isFrozen && !isDispute)
                    isAvail = true;
            }
            isDead = GUILayout.Toggle(isDead, "Dead/Missing", GUILayout.Width(100));
            if (isDead)
                isAll = isAssign = isAvail = isFrozen = isDispute = false;
            else
            {
                if (!isAll && !isAssign && !isAvail && !isFrozen && !isDispute)
                    isDead = true;
            }
            isDispute = GUILayout.Toggle(isDispute, "Dispute", GUILayout.Width(80));
            if (isDispute)
                isAll = isAssign = isAvail = isFrozen = isDead = false;
            else
            {
                if (!isAll && !isAssign && !isAvail && !isFrozen && !isDead)
                    isDispute = true;
            }
            
            if (DFInterface.IsDFInstalled)
            {
                isFrozen = GUILayout.Toggle(isFrozen, "Frozen", GUILayout.Width(80));
                if (isFrozen)
                    isAll = isAssign = isAvail = isDead = isDispute = false;
                else
                {
                    if (!isAll && !isAssign && !isAvail && !isDead && !isDispute)
                        isFrozen = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        private static bool CanDisplayKerbal(RMKerbal kerbal)
        {
            if (isAll)
                return true;
            else if (isAssign &&
                (kerbal.Kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned
                || (kerbal.Kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Crew)))
                return true;
            else if (isAvail && kerbal.Kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                return true;
            else if (isDead && (kerbal.Kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type == ProtoCrewMember.KerbalType.Crew)
                || kerbal.Kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                return true;
            else if (isFrozen && (kerbal.Kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Crew))
                return true;
            else if (isDispute && (kerbal.salaryContractDispute))
                return true;
            else
                return false;
        }

        internal static void ResetKerbalProfessions()
        {
            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (kerbal.name.Contains(char.ConvertFromUtf32(1)))
                {
                    kerbal.name = kerbal.name.Replace(char.ConvertFromUtf32(1), "");                    
                }
            }
        }

        internal static void DisplaySelectedTab()
        {
            if (ShowAttributesTab)
                TabAttributes.Display();
            else if (ShowSchedulingTab)
                TabScheduling.Display();
            else if (ShowHistoryTab)
                TabHistory.Display();
            else if (ShowTrainingTab)
                TabTraining.Display();
            else if (ShowMedicalTab)
                TabMedical.Display();
            else if (ShowRecordsTab)
                TabRecords.Display();
        }

        private static void ResetTabs()
        {
            _ShowAttributesTab = _ShowSchedulingTab = _ShowHistoryTab = _ShowTrainingTab = _ShowMedicalTab = _ShowRecordsTab = false;
        }

        internal static Dictionary<string, KerbalInfo> GetFrozenKerbals()
        {
            if (DFInterface.IsDFInstalled)
            {
                IDFInterface IDF = DFInterface.GetFrozenKerbals();
                return IDF.FrozenKerbals;
            }
            else
                return new Dictionary<string, KerbalInfo>();
        }

        private static string GetFrozenDetials(ProtoCrewMember kerbal)
        {
            string rosterDetails = "";
            if (RMAddon.FrozenKerbals.ContainsKey(kerbal.name))
                if (RMAddon.FrozenKerbals[kerbal.name].vesselName != null)
                    rosterDetails = "Frozen - " + (RMAddon.FrozenKerbals[kerbal.name]).vesselName.Replace("(unloaded)", "");
                else
                    rosterDetails = "Frozen";
            else
                rosterDetails = "Frozen";

            return rosterDetails;
        }

        internal static void DisplaySelectProfession()
        {
            GUILayout.BeginHorizontal();
            if (SelectedKerbal.salaryContractDispute)
                GUI.enabled = false;
            GUILayout.Label("Profession:", GUILayout.Width(80));
            isPilot = GUILayout.Toggle(isPilot, "Pilot", GUILayout.Width(70));
            if (isPilot)
                isEngineer = isScientist = false;
            else
            {
                if (!isEngineer && !isScientist)
                    isPilot = true;
            }
            isEngineer = GUILayout.Toggle(isEngineer, "Engineer", GUILayout.Width(80));
            if (isEngineer)
                isPilot = isScientist = false;
            else
            {
                if (!isPilot && !isScientist)
                    isEngineer = true;
            }
            isScientist = GUILayout.Toggle(isScientist, "Scientist", GUILayout.Width(80));
            if (isScientist)
                isPilot = isEngineer = false;
            else
            {
                if (!isPilot && !isEngineer)
                    isScientist = true;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        internal static void DisplaySelectGender()
        {
            GUILayout.BeginHorizontal();
            bool isMale = ProtoCrewMember.Gender.Male == gender ? true : false;
            GUILayout.Label("Gender:", GUILayout.Width(80));
            isMale = GUILayout.Toggle(isMale, ProtoCrewMember.Gender.Male.ToString(), GUILayout.Width(70));
            isMale = GUILayout.Toggle(!isMale, ProtoCrewMember.Gender.Female.ToString(), GUILayout.Width(80));
            gender = isMale ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
            GUILayout.EndHorizontal();
        }

        internal static void DisplayEditActionButtons(ref Rect rect, ref string label, ref string toolTip)
        {
            GUILayout.BeginHorizontal();
            label = "Apply";
            toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                if (RMSettings.EnableKerbalRename)
                {
                    WindowRoster.SelectedKerbal.Trait = WindowRoster.KerbalProfession;                    
                }
                RMAddon.saveMessage = WindowRoster.SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(RMAddon.saveMessage))
                    WindowRoster.SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
            {
                WindowRoster.SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        internal static void SortRosterList(string sort)
        {
            if (sort == "Name")
                if (RMAddon.AllCrewSort != "Name-A")
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Name-A";
                }
                else
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Name descending select k).ToList();
                    RMAddon.AllCrewSort = "Name-D";
                }
            else if (sort == "Gender")
                if (RMAddon.AllCrewSort != "Gender-A")
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Gender descending, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Gender-A";
                }
                else
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Gender, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Gender-D";
                }
            else if (sort == "Profession")
                if (RMAddon.AllCrewSort != "Profession-A")
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Trait, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Profession-A";
                }
                else
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Trait descending, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Profession-D";
                }
            else if (sort == "Skill")
                if (RMAddon.AllCrewSort != "Skill-D")
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Kerbal.experienceLevel descending, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Skill-D";
                }
                else
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Kerbal.experienceLevel, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Skill-A";
                }
            else if (sort == "Experience")
                if (RMAddon.AllCrewSort != "Experience-D")
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Experience descending, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Experience-D";
                }
                else
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Experience descending, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Experience-A";
                }
            else if (sort == "Status")
                if (RMAddon.AllCrewSort != "Status-A")
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.status, k.Value.type, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Status-A";
                }
                else
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.status descending, k.Value.type descending, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Status-D";
                } 
            else if (sort == "Age")
                if (RMAddon.AllCrewSort != "Age-A")
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.age, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Age-A";
                }
                else
                {
                    RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.age descending, k.Value.Name select k).ToList();
                    RMAddon.AllCrewSort = "Age-D";
                }
        }
    }
}