using System;
using System.Collections.Generic;
using UnityEngine;

namespace RosterManager
{
  [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION)]
  public class RMLifeSpan : ScenarioModule
  {
    // This ScenarioModule class stores the Roster Manager RMKerbals instance of all known kerbals.
    // The class is used to:-
    // Persist the RMKerbals data in the save game file.
    // Load the RMLifeSpanAddon class which processes Age and Salary processing.
    private static RMLifeSpan _instance;

    public static bool IsActive
    {
      get
      {
        return _instance != null;
      }
    }

    public static RMLifeSpan Instance
    {
      get
      {
        return _instance;
      }
    }

    internal RMKerbals RMKerbals { get; private set; }
    internal RMGameSettings RMGameSettings { get; set; }
    private readonly List<Component> _children = new List<Component>();

    public override void OnAwake()
    {
      RmUtils.LogMessage("RosterManagerLifeSpan.Awake Active...", "info", RMSettings.VerboseLogging);
      base.OnAwake();
      _instance = this;
      RMKerbals = new RMKerbals();
      RMGameSettings = new RMGameSettings();

      switch (HighLogic.LoadedScene)
      {
        case GameScenes.SPACECENTER:
        {
          RmUtils.LogMessage("RosterManagerLifeSpan.Awake adding SpaceCenterManager", "info", RMSettings.VerboseLogging);
          RMLifeSpanAddon klMem = gameObject.AddComponent<RMLifeSpanAddon>();
          _children.Add(klMem);
        }
          break;
        case GameScenes.FLIGHT:
        {
          RmUtils.LogMessage("RosterManagerLifeSpan.Awake adding FlightManager", "info", RMSettings.VerboseLogging);
          RMLifeSpanAddon klMem = gameObject.AddComponent<RMLifeSpanAddon>();
          _children.Add(klMem);
        }
          break;
        case GameScenes.EDITOR:
        {
          RmUtils.LogMessage("RosterManagerLifeSpan.Awake adding EditorManager", "info", RMSettings.VerboseLogging);
          RMLifeSpanAddon klMem = gameObject.AddComponent<RMLifeSpanAddon>();
          _children.Add(klMem);
        }
          break;
        case GameScenes.TRACKSTATION:
        {
          RmUtils.LogMessage("RosterManagerLifeSpan.Awake adding TrackingStationManager", "info", RMSettings.VerboseLogging);
          RMLifeSpanAddon klMem = gameObject.AddComponent<RMLifeSpanAddon>();
          _children.Add(klMem);
        }
          break;
        case GameScenes.LOADING:
          break;
        case GameScenes.LOADINGBUFFER:
          break;
        case GameScenes.MAINMENU:
          break;
        case GameScenes.SETTINGS:
          break;
        case GameScenes.CREDITS:
          break;
        case GameScenes.PSYSTEM:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public override void OnLoad(ConfigNode gameNode)
    {
      base.OnLoad(gameNode);
      RMGameSettings.Load(gameNode);
      RMKerbals.Load(gameNode);
      if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) return;
      //Salaries and Profession change charing are disabled in non-career mode games.
      RMGameSettings.EnableSalaries = false;
      RMGameSettings.ChangeProfessionCharge = false;
    }

    public override void OnSave(ConfigNode gameNode)
    {
      base.OnSave(gameNode);
      RMGameSettings.Save(gameNode);
      RMKerbals.Save(gameNode);
    }

    protected void OnDestroy()
    {
      RmUtils.LogMessage("RosterManagerLifeSpan.Awake OnDestroy...", "info", RMSettings.VerboseLogging);
      foreach (Component child in _children)
      {
        RmUtils.LogMessage($"RosterManagerLifeSpan.Awake Destroying {child.name}", "info", RMSettings.VerboseLogging);
        Destroy(child);
      }
      _children.Clear();
    }
  }
}