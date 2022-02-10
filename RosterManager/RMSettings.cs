using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RosterManager.Windows;
using UnityEngine;

namespace RosterManager
{
  internal static class RMSettings
  {
    #region Properties

    internal static bool Loaded;

    internal static ConfigNode Settings;
    private static readonly string SettingsPath = KSPUtil.ApplicationRootPath + "GameData/RosterManager/Plugins/PluginData";
    private static readonly string SettingsFile = SettingsPath + "/RMSettings.dat";
    internal static Rect DefaultPosition = new Rect(50, 50, 0, 0);

    // This value is assigned from AssemblyInfo.cs
    internal static string CurVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

    // Persisted properties
    // Realism Options
    internal static bool RealismMode = true;

    internal static bool LockSettings;

    // Tooltip Options
    internal static bool ShowToolTips = true;

    //Configuration options
    internal static bool EnableBlizzyToolbar; // off by default

    internal static bool VerboseLogging;
    internal static string ErrorLogLength = "1000";
    internal static bool EnableKerbalRename;
    internal static bool AutoDebug;
    internal static bool SaveLogOnExit;

    internal static double LifeInfoUpdatePeriod = 60;

    // Non user managed Internal options
    internal static Color DefaultColor = new Color(0.478f, 0.698f, 0.478f, 0.698f);

    internal static string DebugLogPath = "\\Plugins\\PluginData\\";
    // End Persisted Properties

    // Settings Window Option undo properties (Cancel button support)

    // Realism settings
    internal static bool PrevRealismMode;

    internal static bool PrevLockSettings;

    // Tooltip Settings
    internal static bool PrevShowToolTips = true;

    internal static bool PrevSettingsToolTips = true;
    internal static bool PrevRosterToolTips = true;
    internal static bool PrevDebuggerToolTips = true;
    internal static bool PrevContractDisputeToolTips = true;

    // Configuration Settings
    internal static bool PrevEnableBlizzyToolbar;

    internal static bool PrevShowDebugger;
    internal static bool PrevVerboseLogging;
    internal static string PrevErrorLogLength = "1000";
    internal static bool PrevSaveLogOnExit = true;
    internal static bool PrevEnableKerbalRename;
    internal static bool PrevEnableSalaries;
    internal static string PrevSalaryPeriod = "Month";
    internal static bool PrevEnableAging;
    internal static bool PrevChangeProfessionCharge;
    internal static double PrevDefaultSalary = 10000d;
    internal static double PrevChangeProfessionCost = 10000d;
    internal static int PrevMinimumAge = 25;
    internal static int PrevMaximumAge = 75;
    internal static int PrevMaxContractDisputePeriods = 3;

    // Internal properties for plugin management.  Not persisted, not user managed.
    internal static Dictionary<string, Color> Colors;

    #endregion Properties

    #region Methods

    internal static void LoadColors()
    {
      Colors = new Dictionary<string, Color>
      {
            {"black", Color.black},
            {"blue", Color.blue},
            {"clea", Color.clear},
            {"cyan", Color.cyan},
            {"gray", Color.gray},
            {"green", Color.green},
            {"magenta", Color.magenta},
            {"red", Color.red},
            {"white", Color.white},
            {"yellow", Color.yellow}
          };

    }

    internal static void StoreTempSettings()
    {
      PrevRealismMode = RealismMode;
      PrevShowDebugger = WindowDebugger.ShowWindow;
      PrevVerboseLogging = VerboseLogging;
      PrevEnableKerbalRename = EnableKerbalRename;
      PrevEnableSalaries = RMLifeSpan.Instance.RMGameSettings.EnableSalaries;
      PrevSalaryPeriod = RMLifeSpan.Instance.RMGameSettings.SalaryPeriod;
      PrevEnableAging = RMLifeSpan.Instance.RMGameSettings.EnableAging;
      PrevLockSettings = LockSettings;
      PrevEnableBlizzyToolbar = EnableBlizzyToolbar;
      PrevSaveLogOnExit = SaveLogOnExit;
      PrevShowToolTips = ShowToolTips;
      PrevSettingsToolTips = WindowSettings.ShowToolTips;
      PrevRosterToolTips = WindowRoster.ShowToolTips;
      PrevDebuggerToolTips = WindowDebugger.ShowToolTips;
      PrevContractDisputeToolTips = WindowContracts.ShowToolTips;
      PrevChangeProfessionCharge = RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge;
      PrevDefaultSalary = RMLifeSpan.Instance.RMGameSettings.DefaultSalary;
      PrevChangeProfessionCost = RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCost;
      PrevMinimumAge = RMLifeSpan.Instance.RMGameSettings.MinimumAge;
      PrevMaximumAge = RMLifeSpan.Instance.RMGameSettings.MaximumAge;
      PrevMaxContractDisputePeriods = RMLifeSpan.Instance.RMGameSettings.MaxContractDisputePeriods;

      // sounds

      //debugger Settings
      PrevErrorLogLength = ErrorLogLength;
    }

    internal static void RestoreTempSettings()
    {
      RealismMode = PrevRealismMode;
      WindowDebugger.ShowWindow = PrevShowDebugger;
      VerboseLogging = PrevVerboseLogging;
      EnableKerbalRename = PrevEnableKerbalRename;
      RMLifeSpan.Instance.RMGameSettings.EnableSalaries = PrevEnableSalaries;
      RMLifeSpan.Instance.RMGameSettings.EnableAging = PrevEnableAging;
      RMLifeSpan.Instance.RMGameSettings.SalaryPeriod = PrevSalaryPeriod;
      LockSettings = PrevLockSettings;
      EnableBlizzyToolbar = PrevEnableBlizzyToolbar;
      SaveLogOnExit = PrevSaveLogOnExit;
      ShowToolTips = PrevShowToolTips;
      WindowSettings.ShowToolTips = PrevSettingsToolTips;
      WindowRoster.ShowToolTips = PrevRosterToolTips;
      WindowDebugger.ShowToolTips = PrevDebuggerToolTips;
      WindowContracts.ShowToolTips = PrevContractDisputeToolTips;
      RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCharge = PrevChangeProfessionCharge;
      RMLifeSpan.Instance.RMGameSettings.DefaultSalary = PrevDefaultSalary;
      RMLifeSpan.Instance.RMGameSettings.ChangeProfessionCost = PrevChangeProfessionCost;
      RMLifeSpan.Instance.RMGameSettings.MinimumAge = PrevMinimumAge;
      RMLifeSpan.Instance.RMGameSettings.MaximumAge = PrevMaximumAge;
      RMLifeSpan.Instance.RMGameSettings.MaxContractDisputePeriods = PrevMaxContractDisputePeriods;
      if (RMLifeSpan.Instance.RMGameSettings.SalaryPeriod == "Yearly")
      {
        RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly = false;
        RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly = true;
      }
      else
      {
        RMLifeSpan.Instance.RMGameSettings.SalaryPeriod = "Monthly";
        RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisMonthly = true;
        RMLifeSpan.Instance.RMGameSettings.SalaryPeriodisYearly = false;
      }

      //debugger Settings
      PrevErrorLogLength = ErrorLogLength;
    }

    internal static ConfigNode LoadSettingsFile()
    {
      return Settings ?? (Settings = ConfigNode.Load(SettingsFile) ?? new ConfigNode());
    }

    internal static void ApplySettings()
    {
      LoadColors();

      if (Settings == null)
        LoadSettingsFile();
      if (Settings == null) return;
      ConfigNode windowsNode = Settings.HasNode("RM_Windows") ? Settings.GetNode("RM_Windows") : Settings.AddNode("RM_Windows");
      ConfigNode settingsNode = Settings.HasNode("RM_Settings") ? Settings.GetNode("RM_Settings") : Settings.AddNode("RM_Settings");
      ConfigNode hiddenNode = Settings.HasNode("RM_Hidden") ? Settings.GetNode("RM_Hidden") : Settings.AddNode("RM_Hidden");

      // Lets get our window rectangles...
      DefaultPosition = GetRectangle(windowsNode, "DefaultPosition", DefaultPosition);
      WindowDebugger.Position = GetRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
      WindowSettings.Position = GetRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
      WindowRoster.Position = GetRectangle(windowsNode, "RosterPosition", WindowRoster.Position);
      WindowContracts.Position = GetRectangle(windowsNode, "ContractDisputePosition", WindowContracts.Position);

      // now the settings

      // Realism Settings
      RealismMode = settingsNode.HasValue("RealismMode") ? bool.Parse(settingsNode.GetValue("RealismMode")) : RealismMode;
      LockSettings = settingsNode.HasValue("LockSettings") ? bool.Parse(settingsNode.GetValue("LockSettings")) : LockSettings;

      // ToolTip Settings
      ShowToolTips = settingsNode.HasValue("ShowToolTips") ? bool.Parse(settingsNode.GetValue("ShowToolTips")) : ShowToolTips;
      WindowSettings.ShowToolTips = settingsNode.HasValue("SettingsToolTips") ? bool.Parse(settingsNode.GetValue("SettingsToolTips")) : WindowSettings.ShowToolTips;
      WindowRoster.ShowToolTips = settingsNode.HasValue("RosterToolTips") ? bool.Parse(settingsNode.GetValue("RosterToolTips")) : WindowRoster.ShowToolTips;
      WindowDebugger.ShowToolTips = settingsNode.HasValue("DebuggerToolTips") ? bool.Parse(settingsNode.GetValue("DebuggerToolTips")) : WindowDebugger.ShowToolTips;
      WindowContracts.ShowToolTips = settingsNode.HasValue("ContractDisputeToolTips") ? bool.Parse(settingsNode.GetValue("ContractDisputeToolTips")) : WindowContracts.ShowToolTips;

      // Config Settings
      EnableBlizzyToolbar = settingsNode.HasValue("EnableBlizzyToolbar") ? bool.Parse(settingsNode.GetValue("EnableBlizzyToolbar")) : EnableBlizzyToolbar;
      WindowDebugger.ShowWindow = settingsNode.HasValue("ShowDebugger") ? bool.Parse(settingsNode.GetValue("ShowDebugger")) : WindowDebugger.ShowWindow;
      VerboseLogging = settingsNode.HasValue("VerboseLogging") ? bool.Parse(settingsNode.GetValue("VerboseLogging")) : VerboseLogging;
      AutoDebug = settingsNode.HasValue("AutoDebug") ? bool.Parse(settingsNode.GetValue("AutoDebug")) : AutoDebug;
      DebugLogPath = settingsNode.HasValue("DebugLogPath") ? settingsNode.GetValue("DebugLogPath") : DebugLogPath;
      ErrorLogLength = settingsNode.HasValue("ErrorLogLength") ? settingsNode.GetValue("ErrorLogLength") : ErrorLogLength;
      SaveLogOnExit = settingsNode.HasValue("SaveLogOnExit") ? bool.Parse(settingsNode.GetValue("SaveLogOnExit")) : SaveLogOnExit;
      EnableKerbalRename = settingsNode.HasValue("EnableKerbalRename") ? bool.Parse(settingsNode.GetValue("EnableKerbalRename")) : EnableKerbalRename;

      // Hidden Settings
      LifeInfoUpdatePeriod = hiddenNode.HasValue("LifeInfoUpdatePeriod") ? double.Parse(hiddenNode.GetValue("LifeInfoUpdatePeriod")) : LifeInfoUpdatePeriod;

      //Hidden sound

      // Okay, set the Settings loaded flag
      Loaded = true;

      // Lets make sure that the windows can be seen on the screen. (supports different resolutions)
      RepositionWindows();
    }

    internal static void SaveSettings()
    {
      //If EnableAging has been turned ON when it was previously OFF, we reset age processing, otherwise they could all die instantly.
      if (RMLifeSpan.Instance.RMGameSettings.EnableAging && PrevEnableAging == false)
      {
        RmUtils.LogMessage("RosterManagerWindowSettings.Display Save settings, aging has been enabled. Reset all birthdays.", "info", RMSettings.VerboseLogging);
        double currentTime = Planetarium.GetUniversalTime();
        foreach (KeyValuePair<string, RMKerbal> rmkerbal in RMLifeSpan.Instance.RMKerbals.AllrmKerbals)
        {
          rmkerbal.Value.TimelastBirthday = currentTime;
          rmkerbal.Value.TimeNextBirthday = RMKerbal.BirthdayNextDue(currentTime);
        }
      }
      //If EnableSalaries has been turned OFF when it was previously ON, reset any kerbals from tourist back to active.
      if (!RMLifeSpan.Instance.RMGameSettings.EnableSalaries && PrevEnableSalaries)
      {
        RmUtils.LogMessage("RosterManagerWindowSettings.Display Save settings, salaries have been turned off. Reset all salary related fields for all kerbals.", "info", RMSettings.VerboseLogging);
        foreach (KeyValuePair<string, RMKerbal> rmkerbal in RMLifeSpan.Instance.RMKerbals.AllrmKerbals)
        {
          if (rmkerbal.Value.Type == ProtoCrewMember.KerbalType.Tourist && rmkerbal.Value.Kerbal.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
          {
            rmkerbal.Value.Type = ProtoCrewMember.KerbalType.Crew;
            rmkerbal.Value.Kerbal.type = ProtoCrewMember.KerbalType.Crew;
            rmkerbal.Value.Trait = rmkerbal.Value.RealTrait;
            rmkerbal.Value.Kerbal.trait = rmkerbal.Value.RealTrait;
            KerbalRoster.SetExperienceTrait(rmkerbal.Value.Kerbal, rmkerbal.Value.Trait);
            RMKerbal.RegisterExperienceTrait(rmkerbal.Value);
          }
          rmkerbal.Value.SalaryContractDispute = false;
          rmkerbal.Value.SalaryContractDisputePeriods = 0;
          rmkerbal.Value.SalaryContractDisputeProcessed = true;
        }
      }
      //If EnableSalaries has been turned ON when it was previously OFF, reset all kerbals salary time to now.
      if (RMLifeSpan.Instance.RMGameSettings.EnableSalaries && PrevEnableSalaries == false)
      {
        RmUtils.LogMessage("RosterManagerWindowSettings.Display Save settings, salaries have been turned on. Reset all salary related fields for all kerbals.", "info", RMSettings.VerboseLogging);
        double currentTime = Planetarium.GetUniversalTime();
        foreach (KeyValuePair<string, RMKerbal> rmkerbal in RMLifeSpan.Instance.RMKerbals.AllrmKerbals)
        {
          rmkerbal.Value.Timelastsalary = currentTime;
          rmkerbal.Value.TimeSalaryDue = RMKerbal.SalaryNextDue(currentTime);
        }
      }
      if (Settings == null)
        Settings = LoadSettingsFile();

      ConfigNode windowsNode = Settings.HasNode("RM_Windows") ? Settings.GetNode("RM_Windows") : Settings.AddNode("RM_Windows");
      ConfigNode settingsNode = Settings.HasNode("RM_Settings") ? Settings.GetNode("RM_Settings") : Settings.AddNode("RM_Settings");
      ConfigNode hiddenNode = Settings.HasNode("RM_Hidden") ? Settings.GetNode("RM_Hidden") : Settings.AddNode("RM_Hidden");

      // Write window positions
      WriteRectangle(windowsNode, "DefaultPosition", DefaultPosition);
      WriteRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
      WriteRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
      WriteRectangle(windowsNode, "RosterPosition", WindowRoster.Position);
      WriteRectangle(windowsNode, "ContractDisputePosition", WindowContracts.Position);

      //Write settings...
      // Realism Settings
      WriteValue(settingsNode, "LockSettings", LockSettings);

      // ToolTip Settings
      WriteValue(settingsNode, "ShowToolTips", ShowToolTips);
      WriteValue(settingsNode, "SettingsToolTips", WindowSettings.ShowToolTips);
      WriteValue(settingsNode, "RosterToolTips", WindowRoster.ShowToolTips);
      WriteValue(settingsNode, "DebuggerToolTips", WindowDebugger.ShowToolTips);
      WriteValue(settingsNode, "ContractDisputeToolTips", WindowContracts.ShowToolTips);

      // Config Settings
      WriteValue(settingsNode, "ShowDebugger", WindowDebugger.ShowWindow);
      WriteValue(settingsNode, "EnableBlizzyToolbar", EnableBlizzyToolbar);
      WriteValue(settingsNode, "VerboseLogging", VerboseLogging);
      WriteValue(settingsNode, "AutoDebug", AutoDebug);
      WriteValue(settingsNode, "DebugLogPath", DebugLogPath);
      WriteValue(settingsNode, "ErrorLogLength", ErrorLogLength);
      WriteValue(settingsNode, "SaveLogOnExit", SaveLogOnExit);
      WriteValue(settingsNode, "EnableKerbalRename", EnableKerbalRename);

      // Hidden Settings
      WriteValue(hiddenNode, "LifeInfoUpdatePeriod", LifeInfoUpdatePeriod);

      if (!Directory.Exists(SettingsPath))
        Directory.CreateDirectory(SettingsPath);
      Settings.Save(SettingsFile);
    }

    private static Rect GetRectangle(ConfigNode windowsNode, string rectName, Rect defaultvalue)
    {
      Rect thisRect = new Rect();
      ConfigNode rectNode = windowsNode.HasNode(rectName) ? windowsNode.GetNode(rectName) : windowsNode.AddNode(rectName);
      thisRect.x = rectNode.HasValue("x") ? int.Parse(rectNode.GetValue("x")) : defaultvalue.x;
      thisRect.y = rectNode.HasValue("y") ? int.Parse(rectNode.GetValue("y")) : defaultvalue.y;
      thisRect.width = rectNode.HasValue("width") ? int.Parse(rectNode.GetValue("width")) : defaultvalue.width;
      thisRect.height = rectNode.HasValue("height") ? int.Parse(rectNode.GetValue("height")) : defaultvalue.height;

      return thisRect;
    }

    private static void WriteRectangle(ConfigNode windowsNode, string rectName, Rect rectValue)
    {
      ConfigNode rectNode = windowsNode.HasNode(rectName) ? windowsNode.GetNode(rectName) : windowsNode.AddNode(rectName);
      WriteValue(rectNode, "x", rectValue.x);
      WriteValue(rectNode, "y", rectValue.y);
      WriteValue(rectNode, "width", rectValue.width);
      WriteValue(rectNode, "height", rectValue.height);
    }

    private static void WriteValue(ConfigNode configNode, string valueName, object value)
    {
      if (configNode.HasValue(valueName))
        configNode.RemoveValue(valueName);
      configNode.AddValue(valueName, value.ToString());
    }

    internal static void RepositionWindows()
    {
      RepositionWindow(ref WindowDebugger.Position);
      RepositionWindow(ref WindowSettings.Position);
      RepositionWindow(ref WindowContracts.Position);
      RepositionWindow(ref WindowRoster.Position);
    }

    internal static void RepositionWindow(ref Rect windowPosition)
    {
      // This method uses Gui point system.
      if (windowPosition.x < 0) windowPosition.x = 0;
      if (windowPosition.y < 0) windowPosition.y = 0;

      if (windowPosition.xMax > Screen.width)
        windowPosition.x = Screen.width - windowPosition.width;
      if (windowPosition.yMax > Screen.height)
        windowPosition.y = Screen.height - windowPosition.height;
    }

    #endregion Methods
  }
}