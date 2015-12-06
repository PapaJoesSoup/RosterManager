using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DF;
using RosterManager.Api;
using RosterManager.Windows.Tabs;
using UnityEngine;

namespace RosterManager.Windows
{
  internal static class WindowRoster
  {
    // Standard Window vars
    internal static float WindowWidth = 700;

    internal static float WindowHeight = 335;
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    //Profession vars
    private static ProfessionType _typeOfProfession;
    internal enum ProfessionType
    {
      Pilot,
      Engineer,
      Scientist,
      Tourist
    }

    internal static string KerbalProfession
    {
      get { return _typeOfProfession.ToString(); }
    }

    // Gender var
    internal static ProtoCrewMember.Gender Gender = ProtoCrewMember.Gender.Male;

    //Kerbal List display filter var
    internal static FilterBy FilteredBy = FilterBy.All;

    internal static bool ResetRosterSize
    {
      get
      {
        return DisplayMode == DisplayModes.Index;
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
          RMAddon.SaveMessage = string.Empty;
        }
      }
    }

    // Editor Tab bools
    internal enum TabSelected
    {
      Attributes,
      Scheduleing,
      Training,
      History,
      Medical,
      Records
    }
    private static TabSelected _selectedTab = TabSelected.Attributes;
    internal static TabSelected SelectedTab
    {
      get
      {
        return _selectedTab;
      }
      set
      {
        _selectedTab = value;
      }
    }

    // DisplayMode
    internal static DisplayModes DisplayMode = DisplayModes.Index;

    internal enum DisplayModes
    {
      Index,
      Edit,
      Create
    }

    private static Vector2 _scrollViewerPosition = Vector2.zero;
    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      // Close window button
      var rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
      {
        SelectedKerbal = null;
        DisplayMode = DisplayModes.Index;
        ToolTip = "";
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.EDITOR || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT)
          RMAddon.OnRMRosterToggle();
        else
          ShowWindow = false;
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 0, 0);

      // Ok, now lets render the window...
      try
      {
        GUILayout.BeginVertical();
        DisplayRosterFilter();
        DisplayRosterViewer();

        switch (DisplayMode)
        {
          case DisplayModes.Index:
            DisplayActionButtonsIndex();
            break;
          case DisplayModes.Edit:
            GUILayout.Label("Kerbal Manager", RMStyle.LabelStyleBoldCenter, GUILayout.Width(680));
            DisplayTabButtons();
            DisplaySelectedTab();
            break;
          case DisplayModes.Create:
            GUILayout.Label("Create a Kerbal", RMStyle.LabelStyleBoldCenter, GUILayout.Width(680));
            DisplaySelectProfession();
            DisplaySelectGender();
            DisplayActionButtonsCreate();
            break;
          default:
            DisplayActionButtonsIndex();
            break;
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

    private static void DisplayRosterHeader()
    {
      GUILayout.BeginHorizontal();
      var hdrlabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
      GUILayout.Label("", hdrlabelStyle, GUILayout.Width(5));
      const string buttonToolTip = "Click to sort.  Click again to sort the opposite direction.";

      if (GUILayout.Button(new GUIContent("Name", buttonToolTip), hdrlabelStyle, GUILayout.Width(155)))
        SortRosterList("Name");
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - _scrollViewerPosition.y);

      if (GUILayout.Button(new GUIContent("Gender", buttonToolTip), hdrlabelStyle, GUILayout.Width(50)))
        SortRosterList("Gender");
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - _scrollViewerPosition.y);

      if (RMLifeSpan.Instance.RMGameSettings.EnableAging)
      {
        if (GUILayout.Button(new GUIContent("Age", "Age of Kerbal"), hdrlabelStyle, GUILayout.Width(35)))
          SortRosterList("Age");
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - _scrollViewerPosition.y);
      }

      if (GUILayout.Button(new GUIContent("Profession", buttonToolTip), hdrlabelStyle, GUILayout.Width(75)))
        SortRosterList("Profession");
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - _scrollViewerPosition.y);

      if (GUILayout.Button(new GUIContent("Skill", buttonToolTip), hdrlabelStyle, GUILayout.Width(35)))
        SortRosterList("Skill");
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - _scrollViewerPosition.y);

      if (GUILayout.Button(new GUIContent("Experience", buttonToolTip), hdrlabelStyle, GUILayout.Width(75)))
        SortRosterList("Experience");
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - _scrollViewerPosition.y);

      if (GUILayout.Button(new GUIContent("Status", buttonToolTip), hdrlabelStyle, GUILayout.Width(200)))
        SortRosterList("Status");
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - _scrollViewerPosition.y);

      GUILayout.EndHorizontal();
    }

    private static void DisplayRosterFilter()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Kerbal Filter:", GUILayout.Width(90));
      var isAll = FilteredBy == FilterBy.All;
      isAll = GUILayout.Toggle(isAll, "All", GUILayout.Width(50));
      if (isAll)
        FilteredBy = FilterBy.All;
      var isAssign = FilteredBy == FilterBy.Assigned;
      isAssign = GUILayout.Toggle(isAssign, "Assigned", GUILayout.Width(80));
      if (isAssign)
        FilteredBy = FilterBy.Assigned;
      var isAvail = FilteredBy == FilterBy.Available;
      isAvail = GUILayout.Toggle(isAvail, "Available", GUILayout.Width(80));
      if (isAvail)
        FilteredBy = FilterBy.Available;
      var isDead = FilteredBy == FilterBy.Dead;
      isDead = GUILayout.Toggle(isDead, "Dead/Missing", GUILayout.Width(100));
      if (isDead)
        FilteredBy = FilterBy.Dead;
      var isDispute = FilteredBy == FilterBy.Dispute;
      isDispute = GUILayout.Toggle(isDispute, "Dispute", GUILayout.Width(80));
      if (isDispute)
        FilteredBy = FilterBy.Dispute;
      if (DFInterface.IsDFInstalled)
      {
        var isFrozen = FilteredBy == FilterBy.Frozen;
        isFrozen = GUILayout.Toggle(isFrozen, "Frozen", GUILayout.Width(80));
        if (isFrozen)
          FilteredBy = FilterBy.Frozen;
      }
      GUILayout.EndHorizontal();
    }

    private static void DisplayRosterViewer()
    {
      try
      {
        // We have changed to an Update refreshed list, so I've moved filtering and sorting to the beginning of the routine, instead of the end.
        FilterRosterList(FilteredBy);
        SortRosterList(RMAddon.AllCrewSort);
        DisplayRosterHeader();

        _scrollViewerPosition = GUILayout.BeginScrollView(_scrollViewerPosition, RMStyle.ScrollStyle, GUILayout.Height(230), GUILayout.Width(680));
        //foreach (ProtoCrewMember kerbal in RMAddon.AllCrew
        foreach (var rmkerbal in RMAddon.AllCrew)
        {
          var kerbal = rmkerbal.Value.Kerbal;
          var labelStyle = GetLabelStyle(kerbal, rmkerbal);

          // What vessel is this Kerbal Assigned to?
          var rosterDetails = "";
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
          {
            foreach (var thisVessel in from thisVessel in FlightGlobals.Vessels let crew = thisVessel.GetVesselCrew() where crew.Any(crewMember => crewMember == kerbal) select thisVessel)
            {
              if (rmkerbal.Value.SalaryContractDispute)
                if (rmkerbal.Value.Trait == "Tourist")
                  rosterDetails = "Strike - " + thisVessel.GetName().Replace("(unloaded)", "");
                else
                  rosterDetails = "Dispute - " + thisVessel.GetName().Replace("(unloaded)", "");
              else
                rosterDetails = "Assigned - " + thisVessel.GetName().Replace("(unloaded)", "");
            }
          }
          else if (InstalledMods.IsDfInstalled && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
          {
            // This kerbal could be frozen.  Lets find out...
            rosterDetails = GetFrozenDetials(kerbal);
            labelStyle = RMStyle.LabelStyleCyan;
          }
          else if (rmkerbal.Value.SalaryContractDispute)
          {
            // This kerbal is in contract dispute
            rosterDetails = rmkerbal.Value.Trait == "Tourist" ? "Contract Cancelled - Strike" : "Contract Dispute";
          }
          else
          {
            // Since the kerbal has no vessel assignment, lets show what their status...
            rosterDetails = kerbal.rosterStatus.ToString();
          }
          GUILayout.BeginHorizontal();
          // Column Name (is a Button)
          var buttonText = kerbal.name;
          const string buttonToolTip = "Opens the Edit section for this kerbal.";
          var btnStyle = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? RMStyle.ButtonStyleLeft : RMStyle.ButtonToggledStyleLeft;
          if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), btnStyle, GUILayout.Width(160)))
          {
            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
            {
              //Find the RMKerbal entry for the selected kerbal.
              SelectedKerbal = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.FirstOrDefault(a => a.Key == kerbal.name).Value;
              if (SelectedKerbal == null) //Didn't find the RMKerbal entry? Should never happen? Create a new one just in case.
              {
                SelectedKerbal = new RMKerbal(Planetarium.GetUniversalTime(), kerbal, true, false);
                RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Add(kerbal.name, SelectedKerbal);
              }
              SetProfessionFlag();
              Gender = SelectedKerbal.Gender;
              DisplayMode = DisplayModes.Edit;
            }
            else
            {
              SelectedKerbal = null;
              DisplayMode = DisplayModes.Index;
              Gender = ProtoCrewMember.Gender.Male;
            }
          }
          var rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50 - _scrollViewerPosition.y);

          // Column Gender
          GUILayout.Label(kerbal.gender.ToString(), labelStyle, GUILayout.Width(50));
          if (RMLifeSpan.Instance.RMGameSettings.EnableAging)
          {
            var kerbalInfo = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.FirstOrDefault(a => a.Key == kerbal.name);
            if (kerbalInfo.Key != null)
            {
              GUILayout.Label(kerbalInfo.Value.Age.ToString("##0"), labelStyle, GUILayout.Width(35));
            }
          }
          // Column Salary
          GUILayout.Label(rmkerbal.Value.SalaryContractDispute ? rmkerbal.Value.RealTrait : kerbal.trait, labelStyle, GUILayout.Width(75));
          // Column Skill level
          GUILayout.Label(kerbal.experienceLevel.ToString(), labelStyle, GUILayout.Width(35));
          // column Experience
          GUILayout.Label(kerbal.experience.ToString(CultureInfo.InvariantCulture), labelStyle, GUILayout.Width(75));
          // Column Status
          var statusWidth = GUILayout.Width(240);
          if (RMLifeSpan.Instance.RMGameSettings.EnableAging) statusWidth = GUILayout.Width(200);
          GUILayout.Label(rosterDetails, labelStyle, statusWidth);
          GUILayout.EndHorizontal();
          GUI.enabled = true;
        }

        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in RosterListViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static void DisplayTabButtons()
    {
      GUILayout.BeginHorizontal();
      var attributeStyle = SelectedTab == TabSelected.Attributes ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Attributes", attributeStyle, GUILayout.Height(20)))
      {
        SelectedTab = TabSelected.Attributes;
      }
      var trainingStyle = SelectedTab == TabSelected.Training ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Training", trainingStyle, GUILayout.Height(20)))
      {
        SelectedTab = TabSelected.Training;
      }
      var medicalStyle = SelectedTab == TabSelected.Medical ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Medical", medicalStyle, GUILayout.Height(20)))
      {
        SelectedTab = TabSelected.Medical;
      }
      var professionStyle = SelectedTab == TabSelected.Scheduleing ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Scheduling", professionStyle, GUILayout.Height(20)))
      {
        SelectedTab = TabSelected.Scheduleing;
      }
      var historyStyle = SelectedTab == TabSelected.History ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("History", historyStyle, GUILayout.Height(20)))
      {
        SelectedTab = TabSelected.History;
      }
      var recordStyle = SelectedTab == TabSelected.Records ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button("Records", recordStyle, GUILayout.Height(20)))
      {
        SelectedTab = TabSelected.Records;
      }
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab()
    {
      switch (SelectedTab)
      {
        case TabSelected.Attributes:
          TabAttributes.Display();
          break;
        case TabSelected.Scheduleing:
          TabScheduling.Display();
          break;
        case TabSelected.History:
          TabHistory.Display();
          break;
        case TabSelected.Training:
          TabTraining.Display();
          break;
        case TabSelected.Medical:
          TabMedical.Display();
          break;
        case TabSelected.Records:
          TabRecords.Display();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    internal static void DisplaySelectProfession()
    {
      GUILayout.BeginHorizontal();
      // at first initialization in Create Mode, Selected Kerbal is null...
      if (SelectedKerbal != null && SelectedKerbal.SalaryContractDispute)
        GUI.enabled = false;
      GUILayout.Label("Profession:", GUILayout.Width(80));

      var isPilot = KerbalProfession == ProfessionType.Pilot.ToString();
      isPilot = GUILayout.Toggle(isPilot, "Pilot", GUILayout.Width(70));
      if (isPilot) _typeOfProfession = ProfessionType.Pilot;

      var isEngineer = KerbalProfession == ProfessionType.Engineer.ToString();
      isEngineer = GUILayout.Toggle(isEngineer, "Engineer", GUILayout.Width(80));
      if (isEngineer) _typeOfProfession = ProfessionType.Engineer;

      var isScientist = KerbalProfession == ProfessionType.Scientist.ToString();
      isScientist = GUILayout.Toggle(isScientist, "Scientist", GUILayout.Width(80));
      if (isScientist) _typeOfProfession = ProfessionType.Scientist;

      GUI.enabled = true;
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectGender()
    {
      GUILayout.BeginHorizontal();
      var isMale = ProtoCrewMember.Gender.Male == Gender;
      GUILayout.Label("Gender:", GUILayout.Width(80));
      isMale = GUILayout.Toggle(isMale, ProtoCrewMember.Gender.Male.ToString(), GUILayout.Width(70));
      isMale = GUILayout.Toggle(!isMale, ProtoCrewMember.Gender.Female.ToString(), GUILayout.Width(80));
      Gender = isMale ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
      GUILayout.EndHorizontal();
    }

    private static void DisplayActionButtonsIndex()
    {
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Create Kerbal"))
      {
        DisplayMode = DisplayModes.Create;
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

      if (RMLifeSpan.Instance.RMGameSettings.EnableSalaries)
      {
        var disputesStyle = WindowContracts.ShowWindow ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
        if (GUILayout.Button("Salary Contracts", disputesStyle))
        {
          try
          {
            WindowContracts.ShowWindow = !WindowContracts.ShowWindow;
          }
          catch (Exception ex)
          {
            Utilities.LogMessage(string.Format(" opening Salary Contracts Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          }
        }
      }

      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 30 - _scrollViewerPosition.y);
      GUILayout.EndHorizontal();
    }

    private static void DisplayActionButtonsCreate()
    {
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Create", GUILayout.MaxWidth(80)))
      {
        var kerbalFound = false;
        while (!kerbalFound)
        {
          SelectedKerbal = RMKerbal.CreateKerbal();
          if (SelectedKerbal.Trait == KerbalProfession && SelectedKerbal.Gender == Gender)
            kerbalFound = true;
        }
        if (!RMLifeSpan.Instance.RMKerbals.AllrmKerbals.ContainsKey(SelectedKerbal.Name))
          RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Add(SelectedKerbal.Name, SelectedKerbal);
        DisplayMode = DisplayModes.Edit;
      }
      if (GUILayout.Button("Cancel", GUILayout.MaxWidth(80)))
      {
        SelectedKerbal = null;
        DisplayMode = DisplayModes.Index;
      }
      GUILayout.EndHorizontal();
    }

    internal static void DisplayActionButtonsEdit()
    {
      GUILayout.BeginHorizontal();
      const string label = "Apply";
      const string toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
      if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
      {
        if (RMSettings.EnableKerbalRename)
        {
          SelectedKerbal.Trait = KerbalProfession;
        }
        RMAddon.SaveMessage = SelectedKerbal.SubmitChanges();
        if (string.IsNullOrEmpty(RMAddon.SaveMessage))
        {
          SelectedKerbal = null;
          DisplayMode = DisplayModes.Index;
        }
      }
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips)
        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 50);
      if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
      {
        SelectedKerbal = null;
        DisplayMode = DisplayModes.Index;
      }
      GUILayout.EndHorizontal();
    }

    internal static void FilterRosterList(FilterBy filterBy)
    {
      switch (filterBy)
      {
        case FilterBy.All:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew select k).ToList();
          break;
        case FilterBy.Assigned:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Assigned || (k.Value.Status == ProtoCrewMember.RosterStatus.Dead && k.Value.Type == ProtoCrewMember.KerbalType.Unowned) select k).ToList();
          break;
        case FilterBy.Available:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Available select k).ToList();
          break;
        case FilterBy.Dead:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Missing || (k.Value.Status == ProtoCrewMember.RosterStatus.Dead && k.Value.Type != ProtoCrewMember.KerbalType.Unowned) select k).ToList();
          break;
        case FilterBy.Dispute:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew where k.Value.Type == ProtoCrewMember.KerbalType.Tourist && k.Value.SalaryContractDispute select k).ToList();
          break;
        case FilterBy.Frozen:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Dead && k.Value.Type == ProtoCrewMember.KerbalType.Unowned select k).ToList();
          break;
        default:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew select k).ToList();
          break;
      }
    }

    private static string GetFrozenDetials(ProtoCrewMember kerbal)
    {
      if (RMAddon.FrozenKerbals.ContainsKey(kerbal.name) && RMAddon.FrozenKerbals[kerbal.name].vesselName != null)
        return "Frozen - " + RMAddon.FrozenKerbals[kerbal.name].vesselName.Replace("(unloaded)", "");
      return "Frozen";
    }

    private static void SetProfessionFlag()
    {
      switch (SelectedKerbal.Trait)
      {
        case "Pilot":
          _typeOfProfession = ProfessionType.Pilot;
          break;
        case "Engineer":
          _typeOfProfession = ProfessionType.Engineer;
          break;
        default:
          _typeOfProfession = ProfessionType.Scientist;
          break;
      }
    }

    internal static void SortRosterList(string sort)
    {
      // if allCrewSort is not null, then we want to force a sort by the value.
      switch (sort)
      {
        case "Name":
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
          break;
        case "Name-A":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Name-A";
          break;
        case "Name-D":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Name descending select k).ToList();
          RMAddon.AllCrewSort = "Name-D";
          break;
        case "Gender":
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
          break;
        case "Gender-A":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Gender descending, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Gender-A";
          break;
        case "Gender-D":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Gender, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Gender-D";
          break;
        case "Profession":
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
          break;
        case "Profession-A":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Trait, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Profession-A";
          break;
        case "Profession-D":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Trait descending, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Profession-D";
          break;
        case "Skill":
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
          break;
        case "Skill-A":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Kerbal.experienceLevel descending, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Skill-D";
          break;
        case "Skill-D":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Kerbal.experienceLevel, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Skill-A";
          break;
        case "Experience":
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
          break;
        case "Experience-A":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Experience descending, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Experience-D";
          break;
        case "Esperience-D":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Experience descending, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Experience-A";
          break;
        case "Status":
          if (RMAddon.AllCrewSort != "Status-A")
          {
            RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Status, k.Value.Type, k.Value.Name select k).ToList();
            RMAddon.AllCrewSort = "Status-A";
          }
          else
          {
            RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Status descending, k.Value.Type descending, k.Value.Name select k).ToList();
            RMAddon.AllCrewSort = "Status-D";
          }
          break;
        case "Status-A":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Status, k.Value.Type, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Status-A";
          break;
        case "Status-D":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Status descending, k.Value.Type descending, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Status-D";
          break;
        case "Age":
          if (RMAddon.AllCrewSort != "Age-A")
          {
            RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Age, k.Value.Name select k).ToList();
            RMAddon.AllCrewSort = "Age-A";
          }
          else
          {
            RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Age descending, k.Value.Name select k).ToList();
            RMAddon.AllCrewSort = "Age-D";
          }
          break;
        case "Age-A":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Age, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Age-A";
          break;
        case "Age-D":
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Age descending, k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Age-D";
          break;
        default:
          RMAddon.AllCrew = (from k in RMAddon.AllCrew orderby k.Value.Name select k).ToList();
          RMAddon.AllCrewSort = "Name-A";
          break;
      }
    }

    internal static Dictionary<string, KerbalInfo> GetFrozenKerbals()
    {
      if (!DFInterface.IsDFInstalled) return new Dictionary<string, KerbalInfo>();
      var idf = DFInterface.GetFrozenKerbals();
      return idf.FrozenKerbals;
    }

    internal static void ResetKerbalProfessions()
    {
      foreach (var kerbal in HighLogic.CurrentGame.CrewRoster.Crew.Where(kerbal => kerbal.name.Contains(char.ConvertFromUtf32(1))))
      {
        kerbal.name = kerbal.name.Replace(char.ConvertFromUtf32(1), "");
      }
    }

    private static GUIStyle GetLabelStyle(ProtoCrewMember kerbal, KeyValuePair<string, RMKerbal> rmkerbal)
    {
      GUIStyle labelStyle;
      if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Unowned) || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing || (rmkerbal.Value.SalaryContractDispute && rmkerbal.Value.Trait == "Tourist"))
        labelStyle = RMStyle.LabelStyleRed;
      else if (rmkerbal.Value.SalaryContractDispute && rmkerbal.Value.Trait != "Tourist")
        labelStyle = RMStyle.LabelStyleMagenta;
      else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
        labelStyle = RMStyle.LabelStyleYellow;
      else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Unowned)
        labelStyle = RMStyle.LabelStyleCyan;
      else
        labelStyle = RMStyle.LabelStyle;
      return labelStyle;
    }

    internal enum FilterBy
    {
      All,
      Assigned,
      Available,
      Dead,
      Dispute,
      Frozen
    }
  }
}