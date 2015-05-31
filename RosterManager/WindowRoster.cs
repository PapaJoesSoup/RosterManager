using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace RosterManager
{
    internal static class WindowRoster
    {
        // Standard Window vars
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

        private static ModKerbal _selectedKerbal;
        internal static ModKerbal SelectedKerbal
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
        internal static bool _ShowProfessionTab = false;
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
        internal static bool ShowProfessionTab
        {
            get
            {
                return _ShowProfessionTab; 
            }
            set
            {
                if (value)
                    ResetTabs();
                _ShowProfessionTab = value;
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
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
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
                    GUILayout.Label("Kerbal Manager", RMStyle.LabelStyleBoldCenter, GUILayout.Width(580));
                    DisplayEditorTabButtons();
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

                    if (RMSettings.RenameWithProfession)
                    {
                        string toolTip = "This action resets all renamed Kerbals to their KSP default professions.\r\nIt removes any non printing chars used to maintain a specific profession.\r\nUse this when you wish to revert a game save to be compatabile with KerbalStats\r\n or some other mod that creates custom professions.";
                        if (GUILayout.Button(new GUIContent("Reset Professions", toolTip)))
                        {
                            ResetKerbalProfessions();
                        }
                    }
                    rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 30 - ScrollViewerPosition.y);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
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
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(140));
                GUILayout.Label("Gender", GUILayout.Width(50));
                GUILayout.Label("Profession", GUILayout.Width(65));
                GUILayout.Label("Skill", GUILayout.Width(30));
                GUILayout.Label("Status", GUILayout.Width(125));
                GUILayout.EndHorizontal();

                ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, RMStyle.ScrollStyle, GUILayout.Height(230), GUILayout.Width(580));
                List<ProtoCrewMember> AllCrew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
                // Support for DeepFreeze
                if (InstalledMods.IsDFInstalled)
                    AllCrew.AddRange(HighLogic.CurrentGame.CrewRoster.Unowned);

                foreach (ProtoCrewMember kerbal in AllCrew)
                {
                    if (CanDisplayKerbal(kerbal))
                    {
                        GUIStyle labelStyle = null;
                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                            labelStyle = RMStyle.LabelStyleRed;
                        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                            labelStyle = RMStyle.LabelStyleYellow;
                        else
                            labelStyle = RMStyle.LabelStyle;

                        // What vessel is this Kerbal Assigned to?
                        string rosterStatus = "";
                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                        {
                            foreach (Vessel thisVessel in FlightGlobals.Vessels)
                            {
                                List<ProtoCrewMember> crew = thisVessel.GetVesselCrew();
                                foreach (ProtoCrewMember crewMember in crew)
                                {
                                    if (crewMember == kerbal)
                                    {
                                        rosterStatus = thisVessel.name.Replace("(unloaded)", "");
                                        break;
                                    }
                                }
                            }
                        }
                        else if (InstalledMods.IsDFInstalled && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
                        {
                            // This kerbal could be frozen.  Lets find out...
                            string vesselName = GetFrozenDetials(kerbal);
                            labelStyle = RMStyle.LabelStyleCyan;
                        }
                        else
                        {
                            // Since the kerbal has no vessel assignment, lets show what their status...
                            rosterStatus = kerbal.rosterStatus.ToString();
                        }
                        string buttonText = string.Empty;
                        string buttonToolTip = string.Empty;
                        GUILayout.BeginHorizontal();
                        buttonText = kerbal.name;
                        buttonToolTip = "Opens the Edit section for this kerbal.";
                        GUIStyle btnStyle = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? RMStyle.ButtonStyleLeft : RMStyle.ButtonToggledStyleLeft;
                        if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), btnStyle, GUILayout.MaxWidth(140)))
                        {
                            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
                            {
                                SelectedKerbal = new ModKerbal(kerbal, false);
                                SetProfessionFlag();
                                gender = SelectedKerbal.Gender;
                            }
                            else
                            {
                                SelectedKerbal = null;
                                gender = ProtoCrewMember.Gender.Male;
                            }
                        }
                        Rect rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50 - ScrollViewerPosition.y);
                        GUILayout.Label(kerbal.gender.ToString(), labelStyle, GUILayout.Width(50));
                        GUILayout.Label(kerbal.experienceTrait.Title, labelStyle, GUILayout.Width(65));
                        GUILayout.Label(kerbal.experienceLevel.ToString(), labelStyle, GUILayout.Width(30));
                        GUILayout.Label(rosterStatus, labelStyle, GUILayout.Width(125));

                        if (!RMSettings.RealismMode || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                            GUI.enabled = true;
                        else
                            GUI.enabled = false;

                        buttonText = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit" : "Cancel";
                        if (GUI.enabled)
                            buttonToolTip = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit this Kerbal's characteristics" : "Cancel any changes to this Kerbal";
                        else
                            buttonToolTip = "Kerbal is not available at this time.\r\nEditing is disabled";

                        if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(60)))
                        {
                            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
                            {
                                SelectedKerbal = new ModKerbal(kerbal, false);
                                SetProfessionFlag();
                            }
                            else
                            {
                                SelectedKerbal = null;
                            }
                        }
                        rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50 - ScrollViewerPosition.y);

                        if (HighLogic.LoadedScene != GameScenes.SPACECENTER && ((RMSettings.RealismMode && RMAddon.IsPreLaunch) || !RMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && FlightGlobals.ActiveVessel != null && !RMAddon.VesselIsFull(FlightGlobals.ActiveVessel))
                        {
                            GUI.enabled = true;
                            buttonText = "Add";
                            buttonToolTip = "Adds a kerbal to the Active Vessel,\r\nin the first available seat.";
                        }
                        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                        {
                            GUI.enabled = true;
                            buttonText = "Respawn";
                            buttonToolTip = "Brings a Kerbal back to life.\r\nWill then become available.";
                        }
                        else if (HighLogic.LoadedScene != GameScenes.SPACECENTER && ((RMSettings.RealismMode && RMAddon.IsPreLaunch) || !RMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
                        {
                            GUI.enabled = true;
                            buttonText = "Remove";
                            buttonToolTip = "Removes a Kerbal from the active vessel.\r\nWill then become available.";
                        }
                        else if (HighLogic.LoadedScene != GameScenes.SPACECENTER && (RMSettings.RealismMode && !RMAddon.IsPreLaunch) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        {
                            GUI.enabled = false;
                            buttonText = "Add";
                            buttonToolTip = "Add Disabled.  Realism Settings are preventing this action.\r\nTo add a Kerbal, Change your realism Settings.";
                        }
                        else
                        {
                            GUI.enabled = false;
                            buttonText = "--";
                            buttonToolTip = "Kerbal is not available (" + kerbal.rosterStatus + ").\r\nCurrent status does not allow any action.";
                        }

                        if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(60)))
                        {
                            if (buttonText == "Add")
                                RMAddon.AddCrewMember(kerbal, FlightGlobals.ActiveVessel);
                            else if (buttonText == "Respawn")
                                RMAddon.RespawnKerbal(kerbal);
                            else if (buttonText == "Remove")
                            {
                                // get part...
                                Part part = RMAddon.FindKerbalPart(kerbal);
                                if (part != null)
                                    RMAddon.RemoveCrewMember(kerbal, part);
                            }
                        }
                        rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50 - ScrollViewerPosition.y);
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
                    SelectedKerbal = ModKerbal.CreateKerbal();
                    if (SelectedKerbal.Title == KerbalProfession && SelectedKerbal.Gender == gender)
                        kerbalFound = true;
                }
                OnCreate = false;
            }
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(80)))
            {
                OnCreate = false;
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayEditorTabButtons()
        {
            GUILayout.BeginHorizontal();

            var AttributeStyle = ShowAttributesTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Attributes", AttributeStyle, GUILayout.Height(20)))
            {
                ShowAttributesTab = true;
            }
            GUI.enabled = true;
            var ProfessionStyle = ShowProfessionTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Professions", ProfessionStyle, GUILayout.Height(20)))
            {
                ShowProfessionTab = true;
            }
            var HistoryStyle = ShowHistoryTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("History", HistoryStyle, GUILayout.Height(20)))
            {
                ShowHistoryTab = true;
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
            var RecordStyle = ShowRecordsTab ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
            if (GUILayout.Button("Records", RecordStyle, GUILayout.Height(20)))
            {
                ShowRecordsTab = true;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayAttributesTab()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal", RMStyle.LabelStyleBold);
            if (RMSettings.EnableKerbalRename)
            {
                GUILayout.BeginHorizontal();
                SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name, GUILayout.MaxWidth(300));
                GUILayout.Label(" - (" + SelectedKerbal.Kerbal.experienceTrait.Title + ")");
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }
            if (RMSettings.EnableKerbalRename && RMSettings.RenameWithProfession)
            {
                DisplaySelectProfession();
            }
            DisplaySelectGender();
            SelectedKerbal.Gender = gender;

            GUILayout.Label("Skill");
            SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

            GUILayout.Label("Courage");
            SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

            GUILayout.Label("Stupidity");
            SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1, GUILayout.MaxWidth(300));

            SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, "Badass");

            GUILayout.BeginHorizontal();
            label = "Apply";
            toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                if (RMSettings.EnableKerbalRename && RMSettings.RenameWithProfession)
                {
                    SelectedKerbal.Title = KerbalProfession;
                }
                RMAddon.saveMessage = SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(RMAddon.saveMessage))
                    SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
            {
                SelectedKerbal = null;
            }
             GUILayout.EndHorizontal();
        }

        private static void DisplayProfessionTab()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label("Edit Kerbal's Profession(s)", RMStyle.LabelStyleBold);
            GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }

            GUILayout.BeginHorizontal();
            label = "Apply";
            toolTip = "Applies the Profession changes made to this Kerbal.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                RMAddon.saveMessage = SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(RMAddon.saveMessage))
                    SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
            {
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayHistoryTab()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label("Kerbal Flight History", RMStyle.LabelStyleBold);
            GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }

            GUILayout.BeginHorizontal();
            label = "Apply";
            toolTip = "Applies any flight history changes made to this Kerbal.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                RMAddon.saveMessage = SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(RMAddon.saveMessage))
                    SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
            {
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayTrainingTab()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label("Kerbal Training", RMStyle.LabelStyleBold);
            GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }

            GUILayout.BeginHorizontal();
            label = "Apply";
            toolTip = "Applies any changes made to this Kerbal's training.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                RMAddon.saveMessage = SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(RMAddon.saveMessage))
                    SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
            {
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayMedicalTab()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal", RMStyle.LabelStyleBold);
            if (RMSettings.EnableKerbalRename)
            {
                GUILayout.BeginHorizontal();
                SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name, GUILayout.MaxWidth(300));
                GUILayout.Label(" - (" + SelectedKerbal.Kerbal.experienceTrait.Title + ")");
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }
            if (RMSettings.EnableKerbalRename && RMSettings.RenameWithProfession)
            {
                DisplaySelectProfession();
            }
            DisplaySelectGender();
            SelectedKerbal.Gender = gender;

            GUILayout.Label("Skill");
            SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

            GUILayout.Label("Courage");
            SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

            GUILayout.Label("Stupidity");
            SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1, GUILayout.MaxWidth(300));

            SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, "Badass");

            GUILayout.BeginHorizontal();
            label = "Apply";
            toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                if (RMSettings.EnableKerbalRename && RMSettings.RenameWithProfession)
                {
                    SelectedKerbal.Title = KerbalProfession;
                }
                RMAddon.saveMessage = SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(RMAddon.saveMessage))
                    SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
            {
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayRecordsTab()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal", RMStyle.LabelStyleBold);
            if (RMSettings.EnableKerbalRename)
            {
                GUILayout.BeginHorizontal();
                SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name, GUILayout.MaxWidth(300));
                GUILayout.Label(" - (" + SelectedKerbal.Kerbal.experienceTrait.Title + ")");
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Title + ")", RMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(RMAddon.saveMessage))
            {
                GUILayout.Label(RMAddon.saveMessage, RMStyle.ErrorLabelRedStyle);
            }
            if (RMSettings.EnableKerbalRename && RMSettings.RenameWithProfession)
            {
                DisplaySelectProfession();
            }
            DisplaySelectGender();
            SelectedKerbal.Gender = gender;

            GUILayout.Label("Skill");
            SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

            GUILayout.Label("Courage");
            SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

            GUILayout.Label("Stupidity");
            SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1, GUILayout.MaxWidth(300));

            SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, "Badass");

            GUILayout.BeginHorizontal();
            label = "Apply";
            toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                if (RMSettings.EnableKerbalRename && RMSettings.RenameWithProfession)
                {
                    SelectedKerbal.Title = KerbalProfession;
                }
                RMAddon.saveMessage = SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(RMAddon.saveMessage))
                    SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 50);
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
            {
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplaySelectGender()
        {
            GUILayout.BeginHorizontal();
            bool isMale = ProtoCrewMember.Gender.Male == gender ? true : false;
            GUILayout.Label("Gender:", GUILayout.Width(80));
            isMale = GUILayout.Toggle(isMale, ProtoCrewMember.Gender.Male.ToString(), GUILayout.Width(70));
            isMale = GUILayout.Toggle(!isMale, ProtoCrewMember.Gender.Female.ToString(), GUILayout.Width(80));
            gender = isMale ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
            GUILayout.EndHorizontal();
        }

        private static void SetProfessionFlag()
        {
            if (SelectedKerbal.Title == "Pilot")
            {
                isPilot = true;
                isEngineer = false;
                isScientist = false;
            }
            else if (SelectedKerbal.Title == "Engineer")
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

        private static void DisplaySelectProfession()
        {
            GUILayout.BeginHorizontal();
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
            GUILayout.EndHorizontal();
        }

        private static void DisplayRosterFilter()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Kerbal Filter:", GUILayout.Width(90));
            isAll = GUILayout.Toggle(isAll, "All", GUILayout.Width(50));
            if (isAll)
                isAssign = isAvail = isDead = false;
            else
            {
                if (!isAssign && !isAvail && !isDead)
                    isAll = true;
            }
            isAssign = GUILayout.Toggle(isAssign, "Assigned", GUILayout.Width(80));
            if (isAssign)
                isAll = isAvail = isDead = false;
            else
            {
                if (!isAll && !isAvail && !isDead)
                    isAssign = true;
            }
            isAvail = GUILayout.Toggle(isAvail, "Available", GUILayout.Width(80));
            if (isAvail)
                isAll = isAssign = isDead = false;
            else
            {
                if (!isAll && !isAssign && !isDead)
                    isAvail = true;
            }
            isDead = GUILayout.Toggle(isDead, "Dead/Missing", GUILayout.Width(100));
            if (isDead)
                isAll = isAssign = isAvail = false;
            else
            {
                if (!isAll && !isAssign && !isAvail)
                    isDead = true;
            }
            GUILayout.EndHorizontal();
        }

        private static bool CanDisplayKerbal(ProtoCrewMember kerbal)
        {
            if (isAll)
                return true;
            else if (isAssign && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                return true;
            else if (isAvail && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                return true;
            else if (isDead && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                return true;
            else
                return false;
        }

        private static void ResetKerbalProfessions()
        {
            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (kerbal.name.Contains(char.ConvertFromUtf32(1)))
                {
                    kerbal.name = kerbal.name.Replace(char.ConvertFromUtf32(1), "");
                    KerbalRoster.SetExperienceTrait(kerbal);
                }
            }
        }

        internal static void DisplaySelectedTab()
        {
            if (ShowAttributesTab)
                DisplayAttributesTab();
            else if (ShowProfessionTab)
                DisplayProfessionTab();
            else if (ShowHistoryTab)
                DisplayHistoryTab();
            else if (ShowTrainingTab)
                DisplayTrainingTab();
            else if (ShowMedicalTab)
                DisplayMedicalTab();
            else if (ShowRecordsTab)
                DisplayRecordsTab();
        }

        private static void ResetTabs()
        {
            _ShowAttributesTab = _ShowProfessionTab = _ShowHistoryTab = _ShowTrainingTab = _ShowMedicalTab = _ShowRecordsTab = false;
        }

        private static string GetFrozenDetials(ProtoCrewMember kerbal)
        {
            string rosterDetails = "";
            bool _found = false;
            foreach (Vessel thisVessel in FlightGlobals.Vessels)
            {
                List<Part> cryoParts = (from p in thisVessel.parts where p.name.Contains("cryofreezer") select p).ToList();
                foreach (Part pPart in cryoParts)
                {
                    List<PartModule> cryoModules = (from PartModule m in pPart.Modules where m.moduleName.Contains("DeepFreezer") select m).ToList();
                    foreach (PartModule pMmodule in cryoModules)
                    {
                        foreach (BaseEvent thisEvent in pMmodule.Events)
                        {
                            if (thisEvent.guiName.Contains(kerbal.name))
                            {
                                _found = true;
                                rosterDetails = "Frozen - " + thisVessel.GetName().Replace("(unloaded)", "");
                                break;
                            }
                        }
                    }
                    if (_found) break;
                }
            }
            if (!_found)
            {
                rosterDetails = "Frozen";
            }
            return rosterDetails;
        }

    }
}
