using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RosterManager.Api;
using RosterManager.InternalObjects;
using UnityEngine;
using KSP.Localization;
using RosterManager.Windows.Tabs.Roster;

namespace RosterManager.Windows
{
  internal static class WindowRoster
  {
    // Standard Window vars
    internal static float WindowWidth = 810;
    internal static float WindowHeight = 340;
    internal static float CurrWindowHeight = 340;
    internal static float HeightScale;
    internal static float ViewerHeight = 230;
    internal static float ViewerWidth = 780;
    internal static float SuitHeight = 55;
    internal static float EditHeight = 285;
    internal static float CreateHeight = 75;
    internal static float MinHeight = 230;
    internal static bool ResizingWindow = false;
    internal static Rect Position = RMSettings.DefaultPosition;
    internal static Rect ViewBox = new Rect(0, 0, ViewerWidth, ViewerHeight);
    private static bool _inputLocked;
    private static bool _showWindow;
    internal static bool ShowWindow
    {
        get => _showWindow;
        set
        {
            if (!value)
            {
                InputLockManager.RemoveControlLock("RM_Window");
                _inputLocked = false;
            }
            _showWindow = value;
        }
    }
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static List<KeyValuePair<string, RMKerbal>> FilteredCrew = new List<KeyValuePair<string, RMKerbal>>();

    // Roster List sort vars
    internal static SortColumn SortedBy = SortColumn.Name;
    internal static SortOrder OrderedBy = SortOrder.Ascending;

    // Roster List display filter var
    internal static FilterBy FilteredBy = FilterBy.All;

    // Profession vars
    private static ProfessionType _typeOfProfession;
    internal static string KerbalProfession
    {
      get { return _typeOfProfession.ToString(); }
    }

    // Kerbal Type var
    internal static ProtoCrewMember.KerbalType Kerbal_Type;
    internal static string KerbalType
    {
      get { return Kerbal_Type.ToString(); }
    }

    // Kerbal state var
    internal static ProtoCrewMember.RosterStatus Roster_Status;

    internal static string RosterStatus
    {
      get { return Roster_Status.ToString(); }
    }

    // Gender var
    internal static ProtoCrewMember.Gender Gender = ProtoCrewMember.Gender.Male;


    internal static bool ResetRosterSize
    {
      get
      {
        //return DisplayMode == DisplayModes.None;
        return DisplayMode != DisplayModes.Create && SelectedKerbal == null;
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
      Scheduling,
      Training,
      History,
      Medical,
      Records
    }

    internal static TabSelected SelectedTab { get; set; } = TabSelected.Attributes;

    // DisplayMode
    internal static DisplayModes DisplayMode = DisplayModes.None;

    internal enum DisplayModes
    {
      None,
      Edit,
      Create,
      Suit
    }

    private static Vector2 _scrollViewerPosition = Vector2.zero;
    internal static void Display(int windowId)
    {
        // set input locks when mouseover window...
        _inputLocked = RmUtils.PreventClickThrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      // Close window button
      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", Localizer.Format("#autoLOC_RM_1005"))))		// #autoLOC_RM_1005 = Close Window
      { 
        DisplayMode = DisplayModes.None;
        SelectedKerbal = null;
        ToolTip = "";
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.EDITOR || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT)
          RMAddon.OnRMRosterToggle();
        else
          ShowWindow = false;
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Ok, now lets render the window...
      try
      {
        GUILayout.BeginVertical();
        DisplayRosterFilter();
        DisplayRosterViewer();

        switch (DisplayMode)
        {
          case DisplayModes.None:
            DisplayActionButtonsIndex();
            break;
          case DisplayModes.Edit:
            GUILayout.Label(Localizer.Format("#autoLOC_RM_1006"), RMStyle.LabelStyleBoldCenter, GUILayout.Width(ViewerWidth));		// #autoLOC_RM_1006 = Kerbal Manager
            DisplayTabButtons();
            DisplaySelectedTab();
            break;
          case DisplayModes.Create:
            GUILayout.Label(Localizer.Format("#autoLOC_RM_1007"), RMStyle.LabelStyleBoldCenter, GUILayout.Width(ViewerWidth));		// #autoLOC_RM_1007 = Create a Kerbal
            DisplaySelectProfession();
            DisplaySelectGender();
            DisplayActionButtonsCreate();
            break;
          case DisplayModes.Suit:
              EditSuitViewer();
              break;
          default:
            DisplayActionButtonsIndex();
            break;
        }
        GUILayout.EndVertical();
        //resizing
        CurrWindowHeight = Position.height;
        Rect resizeRect =
            new Rect(Position.width - 18, Position.height - 18, 16, 16);
        GUI.DrawTexture(resizeRect, RmUtils.resizeTexture, ScaleMode.StretchToFill, true);
        if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
        {
            ResizingWindow = true;
        }
        if (Event.current.type == EventType.Repaint && ResizingWindow)
        {
            if (Mouse.delta.y != 0)
            {
                float diff = Mouse.delta.y;
                RmUtils.UpdateScale(diff, ViewerHeight, ref HeightScale, MinHeight);
            }
        }
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        //Account for resizing based on actions..
        Position.height = WindowHeight + ActionHeight() + HeightScale;
        RMSettings.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage($" in Roster Window.  Error:  {ex.Message} \n\n{ex.StackTrace}", "Error", true);
      }
    }

    private static void DisplayRosterHeader()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("", RMStyle.LabelStyleHdr, GUILayout.Width(5));
      string buttonToolTip = Localizer.Format("#autoLOC_RM_1008");    // #autoLOC_RM_1008 = Click to sort.  Click again to sort the opposite direction.

      SortColumn column = SortColumn.Name;
      GUIStyle hdrlabelStyle = SortedBy == column ? RMStyle.LabelStyleHdrSort: RMStyle.LabelStyleHdr;
      string hdrDisplay = Localizer.Format("#autoLOC_RM_1009") + SortIcon(column);
      if (GUILayout.Button(new GUIContent(hdrDisplay, buttonToolTip), hdrlabelStyle,
        GUILayout.Width(155))) // #autoLOC_RM_1009 = Name
        ChangeSorting(column);
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      column = SortColumn.Gender;
      hdrlabelStyle = SortedBy == column ? RMStyle.LabelStyleHdrSort : RMStyle.LabelStyleHdr;
      hdrDisplay = Localizer.Format("#autoLOC_RM_1010") + SortIcon(column);
      if (GUILayout.Button(new GUIContent(hdrDisplay, buttonToolTip), hdrlabelStyle, GUILayout.Width(70)))		// #autoLOC_RM_1010 = Gender
        ChangeSorting(column);
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      if (RMLifeSpan.Instance.RMGameSettings.EnableAging)
      {
        column = SortColumn.Age;
        hdrlabelStyle = SortedBy == column ? RMStyle.LabelStyleHdrSort : RMStyle.LabelStyleHdr;
        hdrDisplay = Localizer.Format("#autoLOC_RM_1011") + SortIcon(column);
        if (GUILayout.Button(new GUIContent(hdrDisplay, Localizer.Format("#autoLOC_RM_1117")), hdrlabelStyle, GUILayout.Width(50)))   // #autoLOC_RM_1011 = Age		// #autoLOC_RM_1117 = Age of Kerbal
          ChangeSorting(column);
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      }

      column = SortColumn.Profession;
      hdrlabelStyle = SortedBy == column ? RMStyle.LabelStyleHdrSort : RMStyle.LabelStyleHdr;
      hdrDisplay = Localizer.Format("#autoLOC_RM_1012") + SortIcon(column);
      if (GUILayout.Button(new GUIContent(hdrDisplay, buttonToolTip), hdrlabelStyle, GUILayout.Width(95)))		// #autoLOC_RM_1012 = Profession
        ChangeSorting(column);
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      column = SortColumn.Skill;
      hdrlabelStyle = SortedBy == column ? RMStyle.LabelStyleHdrSort : RMStyle.LabelStyleHdr;
      hdrDisplay = Localizer.Format("#autoLOC_RM_1013") + SortIcon(column);
      if (GUILayout.Button(new GUIContent(hdrDisplay, buttonToolTip), hdrlabelStyle, GUILayout.Width(55)))		// #autoLOC_RM_1013 = Skill
        ChangeSorting(column);
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      column = SortColumn.Experience;
      hdrlabelStyle = SortedBy == column ? RMStyle.LabelStyleHdrSort : RMStyle.LabelStyleHdr;
      hdrDisplay = Localizer.Format("#autoLOC_RM_1014") + SortIcon(column);
      if (GUILayout.Button(new GUIContent(hdrDisplay, buttonToolTip), hdrlabelStyle, GUILayout.Width(95)))		// #autoLOC_RM_1014 = Experience
        ChangeSorting(column);
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      column = SortColumn.Status;
      hdrlabelStyle = SortedBy == column ? RMStyle.LabelStyleHdrSort : RMStyle.LabelStyleHdr;
      hdrDisplay = Localizer.Format("#autoLOC_RM_1015") + SortIcon(column);
      if (GUILayout.Button(new GUIContent(hdrDisplay, buttonToolTip), hdrlabelStyle))		// #autoLOC_RM_1015 = Status
        ChangeSorting(column);
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
    }

    private static void ChangeSorting(SortColumn column)
    {
      if (column == SortedBy) SortRosterList(true);
      else
      {
        SortedBy = column;
        OrderedBy = SortOrder.Ascending;
        SortRosterList();
      }
    }

    private static string SortIcon(SortColumn column)
    {
      const string sortAsc = " ▲";
      const string sortDesc = " ▼";
      return SortedBy == column
        ? OrderedBy == SortOrder.Ascending
          ? sortAsc
          : sortDesc
        : "";
    }

    internal static void DisplaySelectSuit(ref ProtoCrewMember.KerbalSuit suit)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(Localizer.Format("#autoLOC_RM_1146"), GUILayout.Width(85)); // "Suit:"

        // Always available
        bool isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Default, Localizer.Format("#autoLOC_RM_1147"), GUILayout.Width(90)); // "Default"
        if (isSet) suit = ProtoCrewMember.KerbalSuit.Default;

        if (Expansions.ExpansionsLoader.IsExpansionKerbalSuitInstalled(ProtoCrewMember.KerbalSuit.Vintage)) {
            isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Vintage, Localizer.Format("#autoLOC_RM_1148"), GUILayout.Width(90)); // "Vintage"
            if (isSet) suit = ProtoCrewMember.KerbalSuit.Vintage;
        }
        if (Expansions.ExpansionsLoader.IsExpansionKerbalSuitInstalled(ProtoCrewMember.KerbalSuit.Future)) {
            isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Future, Localizer.Format("#autoLOC_RM_1149"), GUILayout.Width(90)); // "Future"
            if (isSet) suit = ProtoCrewMember.KerbalSuit.Future;
        }
        if (Expansions.ExpansionsLoader.IsExpansionKerbalSuitInstalled(ProtoCrewMember.KerbalSuit.Slim)) {
            isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Slim, Localizer.Format("#autoLOC_RM_1150"), GUILayout.Width(90)); // "Slim"
            if (isSet) suit = ProtoCrewMember.KerbalSuit.Slim;
        }

        GUILayout.EndHorizontal();
        //DisplaySuitColor();
    }

    private static void DisplayRosterFilter()
    {
      // store old value for comparison
      FilterBy oldValue = FilteredBy;
      GUILayout.BeginHorizontal();
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1016"), GUILayout.Width(90));		// #autoLOC_RM_1016 = Kerbal Filter:
      bool isAll = FilteredBy == FilterBy.All;
      isAll = GUILayout.Toggle(isAll, Localizer.Format("#autoLOC_RM_1017"), GUILayout.Width(50));		// #autoLOC_RM_1017 = All
      if (isAll)
        FilteredBy = FilterBy.All;
      bool isAssign = FilteredBy == FilterBy.Assigned;
      isAssign = GUILayout.Toggle(isAssign, Localizer.Format("#autoLOC_RM_1018"), GUILayout.Width(80));		// #autoLOC_RM_1018 = Assigned
      if (isAssign)
        FilteredBy = FilterBy.Assigned;
      bool isAvail = FilteredBy == FilterBy.Available;
      isAvail = GUILayout.Toggle(isAvail, Localizer.Format("#autoLOC_RM_1019"), GUILayout.Width(80));		// #autoLOC_RM_1019 = Available
      if (isAvail)
        FilteredBy = FilterBy.Available;
      bool isDead = FilteredBy == FilterBy.Dead;
      isDead = GUILayout.Toggle(isDead, Localizer.Format("#autoLOC_RM_1020"), GUILayout.Width(100));		// #autoLOC_RM_1020 = Dead/Missing
      if (isDead)
        FilteredBy = FilterBy.Dead;
      bool isDispute = FilteredBy == FilterBy.Dispute;
      isDispute = GUILayout.Toggle(isDispute, Localizer.Format("#autoLOC_RM_1021"), GUILayout.Width(80));		// #autoLOC_RM_1021 = Dispute
      if (isDispute)
        FilteredBy = FilterBy.Dispute;
      if (InstalledMods.IsDfInstalled)
      {
        bool isFrozen = FilteredBy == FilterBy.Frozen;
        isFrozen = GUILayout.Toggle(isFrozen, Localizer.Format("#autoLOC_RM_1022"), GUILayout.Width(80));		// #autoLOC_RM_1022 = Frozen
        if (isFrozen)
          FilteredBy = FilterBy.Frozen;
      }
      if (oldValue != FilteredBy) FilterRosterList();
      GUILayout.EndHorizontal();
    }

    private static void DisplayRosterViewer()
    {
      try
      {
        // We have switched back to an event refreshed list, so I've moved filtering and sorting outof the routine.  
        // as we discover issues, we will refresh the list from the appropriate event.
        //FilterRosterList(FilteredBy);
        //if (RMAddon.SortColumn == "") SortRosterList(RMAddon.SortColumn);
        DisplayRosterHeader();

        _scrollViewerPosition = GUILayout.BeginScrollView(_scrollViewerPosition, RMStyle.ScrollStyle,
            GUILayout.Height(ViewerHeight + HeightScale), GUILayout.Width(ViewBox.width));

        //foreach (ProtoCrewMember kerbal in RMAddon.AllCrew
        List<KeyValuePair<string, RMKerbal>>.Enumerator crewList = FilteredCrew.GetEnumerator();
        while (crewList.MoveNext())
        {
          ProtoCrewMember kerbal = crewList.Current.Value.Kerbal;
          GUIStyle labelStyle = GetLabelStyle(kerbal, crewList.Current);

          // What vessel is this Kerbal Assigned to?
          string rosterDetails = GetRosterDetails(kerbal, crewList.Current, ref labelStyle);
          // now lets display the results for this kerbal
          GUILayout.BeginHorizontal();
          // Column Name (is a Button)
          string buttonText = kerbal.name;
          string buttonToolTip = Localizer.Format("#autoLOC_RM_1025");		// #autoLOC_RM_1025 = Opens the Edit section for this kerbal.
          GUIStyle btnStyle = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? RMStyle.ButtonStyleLeft : RMStyle.ButtonToggledStyleLeft;
          if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), btnStyle, GUILayout.Width(160)))
          {
            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
            {
              //Find the RMKerbal entry for the selected kerbal.
              SelectedKerbal = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.FirstOrDefault(a => a.Key == kerbal.name).Value;
              if (SelectedKerbal == null)
                //Didn't find the RMKerbal entry? Should never happen? Create a new one just in case.
              {
                SelectedKerbal = new RMKerbal(Planetarium.GetUniversalTime(), kerbal, true, false);
                RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Add(kerbal.name, SelectedKerbal);
              }
              else SelectedKerbal.IsNew = false;
              SetProfessionFlag();
              Gender = SelectedKerbal.Gender;
              DisplayMode = DisplayModes.Edit;
            }
            else
            {
              SelectedKerbal = null;
              DisplayMode = DisplayModes.None;
              Gender = ProtoCrewMember.Gender.Male;
            }
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

          // Column Gender
          GUILayout.Label(kerbal.gender.ToString(), labelStyle, GUILayout.Width(70));
          // Column Age
          if (RMLifeSpan.Instance.RMGameSettings.EnableAging)
          {
            KeyValuePair<string, RMKerbal> kerbalInfo = RMLifeSpan.Instance.RMKerbals.AllrmKerbals.FirstOrDefault(a => a.Key == kerbal.name);
            if (kerbalInfo.Key != null)
            {
              GUILayout.Label(kerbalInfo.Value.Age.ToString("##0"), labelStyle, GUILayout.Width(50));
            }
          }
          // Column Salary
          GUILayout.Label(crewList.Current.Value.SalaryContractDispute ? crewList.Current.Value.RealTrait : kerbal.trait, labelStyle, GUILayout.Width(95));
          // Column Skill level
          GUILayout.Label(kerbal.experienceLevel.ToString(), labelStyle, GUILayout.Width(55));
          // column Experience
          GUILayout.Label(kerbal.experience.ToString(CultureInfo.InvariantCulture), labelStyle, GUILayout.Width(95));
          // Column Status
          GUILayoutOption statusWidth = GUILayout.Width(240);
          if (RMLifeSpan.Instance.RMGameSettings.EnableAging) statusWidth = GUILayout.Width(200);
          GUILayout.Label(rosterDetails, labelStyle, statusWidth);
          GUILayout.EndHorizontal();
          GUI.enabled = true;
        }
        crewList.Dispose();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        RmUtils.LogMessage($" in RosterListViewer.  Error:  {ex.Message} \n\n{ex.StackTrace}", "Error", true);
      }
    }

    private static string GetRosterDetails(ProtoCrewMember kerbal, KeyValuePair<string, RMKerbal> rmkerbal, ref GUIStyle labelStyle)
    {
      string rosterDetails;
      if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
      {
        rosterDetails = GetRosterVesselDetails(rmkerbal.Value);
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
        rosterDetails = rmkerbal.Value.Trait == "Tourist"
          ? Localizer.Format("#autoLOC_RM_1023")
          : Localizer.Format(
            "#autoLOC_RM_1024"); // #autoLOC_RM_1023 = Contract Cancelled - Strike		// #autoLOC_RM_1024 = Contract Dispute
      }
      else
      {
        // Since the kerbal has no vessel assignment, lets show what their status...
        rosterDetails = kerbal.rosterStatus.ToString();
      }
      return rosterDetails;
    }

    internal static string GetRosterVesselDetails(RMKerbal rmkerbal)
    {
      string rosterDetails = "";
      List<Vessel>.Enumerator vessels = FlightGlobals.Vessels.GetEnumerator();
      while (vessels.MoveNext())
      {
        if (vessels.Current == null) continue;
        List<ProtoCrewMember>.Enumerator crew = vessels.Current.GetVesselCrew().GetEnumerator();
        while (crew.MoveNext())
        {
          if (crew.Current != rmkerbal.Kerbal) continue;
          if (rmkerbal.SalaryContractDispute)
            rosterDetails = rmkerbal.Trait == "Tourist" 
              ? $"{Localizer.Format("#autoLOC_RM_1026")} - {vessels.Current.GetName().Replace("(unloaded)", "")}" // Strike
              : $"{Localizer.Format("#autoLOC_RM_1027")} - {vessels.Current.GetName().Replace("(unloaded)", "")}";  // Dispute
          else
            rosterDetails =
              $"{Localizer.Format("#autoLOC_RM_1028")} - {vessels.Current.GetName().Replace("(unloaded)", "")}";		// #autoLOC_RM_1028 = Assigned
          break;
        }
        crew.Dispose();
      }
      vessels.Dispose();
      return rosterDetails;
    }

    internal static bool IsKerbalInDeepFreezePart(RMKerbal selectedKerbal)
    {
      bool result = false;
      List<Vessel>.Enumerator vessels = FlightGlobals.Vessels.GetEnumerator();
      while (vessels.MoveNext())
      {
        if (vessels.Current == null) continue;
        List<ProtoCrewMember>.Enumerator crew = vessels.Current.GetVesselCrew().GetEnumerator();
        while (crew.MoveNext())
        {
          if (crew.Current == null) continue;
          if (crew.Current != selectedKerbal.Kerbal) continue;
          if (crew.Current.KerbalRef.InPart != null)
          {
            if (crew.Current.KerbalRef.InPart.Modules.Contains("ModuleDeepFreeze"))
            result = true;
          }
          break;
        }
        crew.Dispose();
      }
      vessels.Dispose();
      return result;
    }

    private static void DisplayTabButtons()
    {
      GUILayout.BeginHorizontal();
      GUIStyle attributeStyle = SelectedTab == TabSelected.Attributes ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1029"), attributeStyle, GUILayout.Height(20)))		// #autoLOC_RM_1029 = Attributes
      {
        SelectedTab = TabSelected.Attributes;
      }
      GUIStyle trainingStyle = SelectedTab == TabSelected.Training ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1030"), trainingStyle, GUILayout.Height(20)))		// #autoLOC_RM_1030 = Training
      {
        SelectedTab = TabSelected.Training;
      }
      GUIStyle medicalStyle = SelectedTab == TabSelected.Medical ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1031"), medicalStyle, GUILayout.Height(20)))		// #autoLOC_RM_1031 = Medical
      {
        SelectedTab = TabSelected.Medical;
      }
      GUIStyle professionStyle = SelectedTab == TabSelected.Scheduling ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1032"), professionStyle, GUILayout.Height(20)))		// #autoLOC_RM_1032 = Scheduling
      {
        SelectedTab = TabSelected.Scheduling;
      }
      GUIStyle historyStyle = SelectedTab == TabSelected.History ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1033"), historyStyle, GUILayout.Height(20)))		// #autoLOC_RM_1033 = History
      {
        SelectedTab = TabSelected.History;
      }
      GUIStyle recordStyle = SelectedTab == TabSelected.Records ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1034"), recordStyle, GUILayout.Height(20)))		// #autoLOC_RM_1034 = Records
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
        case TabSelected.Scheduling:
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
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1035"), GUILayout.Width(80));		// #autoLOC_RM_1035 = Profession:

      bool isPilot = KerbalProfession == ProfessionType.Pilot.ToString();
      isPilot = GUILayout.Toggle(isPilot, Localizer.Format("#autoLOC_RM_1036"), GUILayout.Width(70));		// #autoLOC_RM_1036 = Pilot
      if (isPilot) _typeOfProfession = ProfessionType.Pilot;

      bool isEngineer = KerbalProfession == ProfessionType.Engineer.ToString();
      isEngineer = GUILayout.Toggle(isEngineer, Localizer.Format("#autoLOC_RM_1037"), GUILayout.Width(80));		// #autoLOC_RM_1037 = Engineer
      if (isEngineer) _typeOfProfession = ProfessionType.Engineer;

      bool isScientist = KerbalProfession == ProfessionType.Scientist.ToString();
      isScientist = GUILayout.Toggle(isScientist, Localizer.Format("#autoLOC_RM_1038"), GUILayout.Width(80));		// #autoLOC_RM_1038 = Scientist
      if (isScientist) _typeOfProfession = ProfessionType.Scientist;

      bool isTourist = KerbalProfession == ProfessionType.Tourist.ToString();
      isTourist = GUILayout.Toggle(isTourist, Localizer.Format("#autoLOC_RM_1039"), GUILayout.Width(80));		// #autoLOC_RM_1039 = Tourist
      if (isTourist) _typeOfProfession = ProfessionType.Tourist;

      GUI.enabled = true;
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectType()
    {
      GUILayout.BeginHorizontal();
      // at first initialization in Create Mode, Selected Kerbal is null...
      Kerbal_Type = SelectedKerbal?.Type ?? ProtoCrewMember.KerbalType.Crew;
      if (SelectedKerbal != null && SelectedKerbal.SalaryContractDispute) GUI.enabled = false;
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1040"), GUILayout.Width(80));		// #autoLOC_RM_1040 = KerbalType:

      bool isApplicant = KerbalType == ProtoCrewMember.KerbalType.Applicant.ToString();
      isApplicant = GUILayout.Toggle(isApplicant, ProtoCrewMember.KerbalType.Applicant.ToString(), GUILayout.Width(70));
      if (isApplicant) Kerbal_Type = ProtoCrewMember.KerbalType.Applicant;

      bool isCrew = KerbalType == ProtoCrewMember.KerbalType.Crew.ToString();
      isCrew = GUILayout.Toggle(isCrew, ProtoCrewMember.KerbalType.Crew.ToString(), GUILayout.Width(70));
      if (isCrew) Kerbal_Type = ProtoCrewMember.KerbalType.Crew;

      bool isTourist = KerbalType == ProtoCrewMember.KerbalType.Tourist.ToString();
      isTourist = GUILayout.Toggle(isTourist, ProtoCrewMember.KerbalType.Tourist.ToString(), GUILayout.Width(70));
      if (isTourist) Kerbal_Type = ProtoCrewMember.KerbalType.Tourist;

      bool isUnowned = KerbalType == ProtoCrewMember.KerbalType.Unowned.ToString();
      isUnowned = GUILayout.Toggle(isUnowned, ProtoCrewMember.KerbalType.Unowned.ToString(), GUILayout.Width(70));
      if (isUnowned) Kerbal_Type = ProtoCrewMember.KerbalType.Unowned;

      GUI.enabled = true;
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectState()
    {
      GUILayout.BeginHorizontal();
      // at first initialization in Create Mode, Selected Kerbal is null...
      Roster_Status = SelectedKerbal?.Status ?? ProtoCrewMember.RosterStatus.Available;
      if (SelectedKerbal != null && SelectedKerbal.SalaryContractDispute)
        GUI.enabled = false;
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1041"), GUILayout.Width(80));		// #autoLOC_RM_1041 = Status:

      bool isAvailable = RosterStatus == ProtoCrewMember.RosterStatus.Available.ToString();
      isAvailable = GUILayout.Toggle(isAvailable, ProtoCrewMember.RosterStatus.Available.ToString(), GUILayout.Width(70));
      if (isAvailable) Roster_Status = ProtoCrewMember.RosterStatus.Available;

      bool isAssigned = RosterStatus == ProtoCrewMember.RosterStatus.Assigned.ToString();
      isAssigned = GUILayout.Toggle(isAssigned, ProtoCrewMember.RosterStatus.Assigned.ToString(), GUILayout.Width(70));
      if (isAssigned) Roster_Status = ProtoCrewMember.RosterStatus.Assigned;

      bool isMissing = RosterStatus == ProtoCrewMember.RosterStatus.Missing.ToString();
      isMissing = GUILayout.Toggle(isMissing, ProtoCrewMember.RosterStatus.Missing.ToString(), GUILayout.Width(70));
      if (isMissing) Roster_Status = ProtoCrewMember.RosterStatus.Missing;

      bool isDead = RosterStatus == ProtoCrewMember.RosterStatus.Dead.ToString();
      isDead = GUILayout.Toggle(isDead, ProtoCrewMember.RosterStatus.Dead.ToString(), GUILayout.Width(70));
      if (isDead) Roster_Status = ProtoCrewMember.RosterStatus.Dead;

      GUI.enabled = true;
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectGender()
    {
      GUILayout.BeginHorizontal();
      bool isMale = ProtoCrewMember.Gender.Male == Gender;
      GUILayout.Label(Localizer.Format("#autoLOC_RM_1042"), GUILayout.Width(80));		// #autoLOC_RM_1042 = Gender:
      isMale = GUILayout.Toggle(isMale, ProtoCrewMember.Gender.Male.ToString(), GUILayout.Width(70));
      isMale = GUILayout.Toggle(!isMale, ProtoCrewMember.Gender.Female.ToString(), GUILayout.Width(80));
      Gender = isMale ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
      GUILayout.EndHorizontal();
    }

    private static void DisplayActionButtonsIndex()
    {
      GUILayout.BeginHorizontal();
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1043")))		// #autoLOC_RM_1043 = Create Kerbal
      {
        DisplayMode = DisplayModes.Create;
      }
      GUIStyle settingsStyle = WindowSettings.ShowWindow ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1044"), settingsStyle))		// #autoLOC_RM_1044 = Settings
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
          RmUtils.LogMessage($" opening Settings Window.  Error:  {ex.Message} \n\n{ex.StackTrace}", "Error", true);
        }
      }

      if (RMLifeSpan.Instance.RMGameSettings.EnableSalaries)
      {
        GUIStyle disputesStyle = WindowContracts.ShowWindow ? RMStyle.ButtonToggledStyle : RMStyle.ButtonStyle;
        if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1045"), disputesStyle))		// #autoLOC_RM_1045 = Salary Contracts
        {
          try
          {
            WindowContracts.ShowWindow = !WindowContracts.ShowWindow;
          }
          catch (Exception ex)
          {
            RmUtils.LogMessage($" opening Salary Contracts Window.  Error:  {ex.Message} \n\n{ex.StackTrace}", "Error", true);
          }
        }
      }

      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUILayout.EndHorizontal();
    }

    private static void DisplayActionButtonsCreate()
    {
      GUILayout.BeginHorizontal();
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1046"), GUILayout.MaxWidth(80)))		// #autoLOC_RM_1046 = Create
      {
        bool kerbalFound = false;
        while (!kerbalFound)
        {
          SelectedKerbal = RMKerbal.CreateKerbal(GetKerbalType(KerbalProfession));
          if (SelectedKerbal.Trait == KerbalProfession && SelectedKerbal.Gender == Gender)
            kerbalFound = true;
        }
        if (!RMLifeSpan.Instance.RMKerbals.AllrmKerbals.ContainsKey(SelectedKerbal.Name))
          RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Add(SelectedKerbal.Name, SelectedKerbal);
        DisplayMode = DisplayModes.Edit;
      }
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1047"), GUILayout.MaxWidth(80)))		// #autoLOC_RM_1047 = Cancel
      {
        SelectedKerbal = null;
        DisplayMode = DisplayModes.None;
      }
      GUILayout.EndHorizontal();
    }

    internal static void DisplayActionButtonsEdit()
    {
      GUILayout.BeginHorizontal();
      string label = Localizer.Format("#autoLOC_RM_1048");		// #autoLOC_RM_1048 = Apply
      string toolTip = Localizer.Format("#autoLOC_RM_1049");		// #autoLOC_RM_1049 = Applies the changes made to this Kerbal.\nDesired Name and Profession will be Retained after save.
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
          DisplayMode = DisplayModes.None;
        }
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && RMSettings.ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1050"), GUILayout.MaxWidth(50)))		// #autoLOC_RM_1050 = Cancel
      {
        SelectedKerbal = null;
        DisplayMode = DisplayModes.None;
      }
      GUILayout.EndHorizontal();
    }

    internal static void EditSuitViewer()
    {
        GUILayout.Label(SelectedKerbal.IsNew ? Localizer.Format("#autoLOC_RM_1007") : Localizer.Format("#autoLOC_RM_1007a"));
        SetKerbalSuit(SelectedKerbal);
    }

    internal static void UpdateRosterList()
    {
      RMAddon.RefreshCrew(HighLogic.LoadedScene);
      FilterRosterList();
      SortRosterList();
    }

    internal static void FilterRosterList()
    {
      FilteredCrew.Clear();
      switch (FilteredBy)
      {
        case FilterBy.All:
          FilteredCrew = (from k in RMAddon.AllCrew select k).ToList();
          break;
        case FilterBy.Assigned:
          FilteredCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Assigned || (k.Value.Status == ProtoCrewMember.RosterStatus.Dead && k.Value.Type == ProtoCrewMember.KerbalType.Unowned) select k).ToList();
          break;
        case FilterBy.Available:
          FilteredCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Available select k).ToList();
          break;
        case FilterBy.Dead:
          FilteredCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Missing || (k.Value.Status == ProtoCrewMember.RosterStatus.Dead && k.Value.Type != ProtoCrewMember.KerbalType.Unowned) select k).ToList();
          break;
        case FilterBy.Dispute:
          FilteredCrew = (from k in RMAddon.AllCrew where k.Value.Type == ProtoCrewMember.KerbalType.Tourist && k.Value.SalaryContractDispute select k).ToList();
          break;
        case FilterBy.Frozen:
          FilteredCrew = (from k in RMAddon.AllCrew where k.Value.Status == ProtoCrewMember.RosterStatus.Dead && k.Value.Type == ProtoCrewMember.KerbalType.Unowned select k).ToList();
          break;
        default:
          FilteredCrew = (from k in RMAddon.AllCrew select k).ToList();
          FilteredBy = FilterBy.All;
          break;
      }
    }

    internal static void SortRosterList(bool toggle = false)
    {
      // if allCrewSort is not null, then we want to force a sort by the value.
      switch (SortedBy)
      {
        case SortColumn.Name:
          if (OrderedBy == SortOrder.Ascending)
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Name descending select k).ToList();
              OrderedBy = SortOrder.Descending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Name select k).ToList();
          }
          else
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Ascending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Name descending select k).ToList();
          }
          break;
        case SortColumn.Gender:
          if (OrderedBy == SortOrder.Ascending)
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Gender descending, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Descending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Gender, k.Value.Name select k).ToList();
          }
          else
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Gender, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Ascending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Gender descending, k.Value.Name select k).ToList();
          }
          break;
        case SortColumn.Profession:
          if (OrderedBy == SortOrder.Ascending)
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Trait descending, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Descending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Trait, k.Value.Name select k).ToList();
          }
          else
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Trait, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Ascending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Trait descending, k.Value.Name select k).ToList();
          }
          break;
        case SortColumn.Skill:
          if (OrderedBy == SortOrder.Ascending)
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Skill descending, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Descending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Skill, k.Value.Name select k).ToList();
          }
          else
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Skill, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Ascending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Skill descending, k.Value.Name select k).ToList();
          }
          break;
        case SortColumn.Experience:
          if (OrderedBy == SortOrder.Ascending)
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Experience descending, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Descending;
            }
            else
              FilteredCrew = (from k in RMAddon.AllCrew orderby k.Value.Experience, k.Value.Name select k).ToList();
          }
          else
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Experience, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Ascending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Experience descending, k.Value.Name select k).ToList();
          }
          break;
        case SortColumn.Status:
          if (OrderedBy == SortOrder.Ascending)
          {
            if (toggle)
            {
              FilteredCrew = (from k in RMAddon.AllCrew orderby k.Value.Status descending, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Descending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Status, k.Value.Name select k).ToList();
          }
          else
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Status, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Ascending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Status descending, k.Value.Name select k).ToList();
          }
          break;
        case SortColumn.Age:
          if (OrderedBy == SortOrder.Ascending)
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Age descending, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Descending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Age, k.Value.Name select k).ToList();
          }
          else
          {
            if (toggle)
            {
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Age, k.Value.Name select k).ToList();
              OrderedBy = SortOrder.Ascending;
            }
            else
              FilteredCrew = (from k in FilteredCrew orderby k.Value.Age descending, k.Value.Name select k).ToList();
          }
          break;
        case SortColumn.None:
          FilteredCrew = (from k in FilteredCrew orderby k.Value.Name select k).ToList();
          SortedBy = SortColumn.Name;
          OrderedBy = SortOrder.Ascending;
          break;
        default:
          FilteredCrew = (from k in FilteredCrew orderby k.Value.Name select k).ToList();
          SortedBy = SortColumn.Name;
          OrderedBy = SortOrder.Ascending;
          break;

      }
    }

    internal static Dictionary<string, DfWrapper.KerbalInfo> GetFrozenKerbals()
    {
      if (!InstalledMods.IsDfInstalled) return new Dictionary<string, DfWrapper.KerbalInfo>();
      if (!DfWrapper.InstanceExists)
            {
                DfWrapper.InitDfWrapper();
            }
      if (!DfWrapper.InstanceExists || !DfWrapper.ApiReady) return new Dictionary<string, DfWrapper.KerbalInfo>();
      Dictionary<string, DfWrapper.KerbalInfo> idf = DfWrapper.DeepFreezeApi.FrozenKerbals;
      return idf;
    }

    private static string GetFrozenDetials(ProtoCrewMember kerbal)
    {
      if (RMAddon.FrozenKerbals.ContainsKey(kerbal.name) && RMAddon.FrozenKerbals[kerbal.name].VesselName != null)
        return
          $"{Localizer.Format("#autoLOC_RM_1051")} - {RMAddon.FrozenKerbals[kerbal.name].VesselName.Replace("(unloaded)", "")}";		// #autoLOC_RM_1051 = Frozen - {0}
      return "Frozen";
    }

    private static ProtoCrewMember.KerbalType GetKerbalType(string profession)
    {
      ProtoCrewMember.KerbalType kerbalType;
      switch (profession)
      {
        case "Pilot":
          kerbalType = ProtoCrewMember.KerbalType.Crew;
          break;
        case "Engineer":
          kerbalType = ProtoCrewMember.KerbalType.Crew;
          break;
        case "Scientist":
          kerbalType = ProtoCrewMember.KerbalType.Crew;
          break;
        case "Tourist":
          kerbalType = ProtoCrewMember.KerbalType.Tourist;
          break;
        default:
          kerbalType = ProtoCrewMember.KerbalType.Unowned;
            break;
      }
      return kerbalType;
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
        case "Scientist":
          _typeOfProfession = ProfessionType.Scientist;
          break;
        default:
          _typeOfProfession = ProfessionType.Tourist;
          break;
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

    private static void SetKerbalSuit(RMKerbal selectedKerbal)
    {
        DisplayMode = DisplayModes.Suit;
        DisplaySelectSuit(ref selectedKerbal.Suit);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1047"), GUILayout.MaxWidth(50))) // "Cancel"
        {
            SelectedKerbal = null;
            DisplayMode = DisplayModes.None;
        }

        if (GUILayout.Button(Localizer.Format("#autoLOC_RM_1048"), GUILayout.MaxWidth(50)))
        {
            if (SelectedKerbal != null)
            {
                RMAddon.SaveMessage = SelectedKerbal.SubmitChanges();
                UpdateRosterList();
                if (string.IsNullOrEmpty(RMAddon.SaveMessage))
                {
                    SelectedKerbal = null;
                }
                DisplayMode = DisplayModes.None;
            }
        }
        Rect rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
        GUILayout.EndHorizontal();
    }

    internal static float ActionHeight()
    {
        switch (DisplayMode)
        {
            case DisplayModes.Edit:
                return EditHeight;
            case DisplayModes.Suit:
                return SuitHeight;
            case DisplayModes.Create:
                return CreateHeight;
            case DisplayModes.None:
            default:
                return 0;
        }
    }


    #region Enums
    internal enum FilterBy
    {
      All,
      Assigned,
      Available,
      Dead,
      Dispute,
      Frozen
    }

    internal enum SortColumn
    {
      None,
      Name,
      Gender,
      Profession,
      Skill,
      Age,
      Experience,
      Status
    }

    internal enum SortOrder
    {
      Ascending,
      Descending,
    }

    internal enum ProfessionType
    {
      Pilot,
      Engineer,
      Scientist,
      Tourist
    }
    #endregion
  }
}