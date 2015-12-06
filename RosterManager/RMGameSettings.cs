namespace RosterManager
{
  internal class RMGameSettings
  {
    // This class stores the RosterManager Gamesettings config node, which are settings stored for each saved game.
    // which includes Settings for Aging of kerbals, charging for chaing a kerbals profession, and Salaries settings.

    public const string ConfigNodeName = "RMGameSettings";

    //Aging vars
    internal bool EnableAging { get; set; }

    internal int MinimumAge { get; set; }
    internal int MaximumAge { get; set; }
    internal int MaxContractDisputePeriods { get; set; }

    //Profession change vars
    internal bool ChangeProfessionCharge { get; set; }

    internal double ChangeProfessionCost { get; set; }

    //SalaryPeriod vars
    internal bool EnableSalaries { get; set; }

    internal double DefaultSalary { get; set; }
    internal bool SalaryPeriodisMonthly { get; set; }
    internal bool SalaryPeriodisYearly { get; set; }
    internal string SalaryPeriod { get; set; }

    internal RMGameSettings()
    {
      EnableAging = false;
      MinimumAge = 25;
      MaximumAge = 75;
      MaxContractDisputePeriods = 3;
      ChangeProfessionCharge = false;
      ChangeProfessionCost = 5000;
      EnableSalaries = false;
      DefaultSalary = 5000;
      SalaryPeriodisMonthly = true;
      SalaryPeriodisYearly = false;
      SalaryPeriod = "Monthly";
    }

    internal void Load(ConfigNode node)
    {
      if (node.HasNode(ConfigNodeName))
      {
        var rmGamesettingsNode = node.GetNode(ConfigNodeName);
        EnableAging = GetNodes.GetNodeValue(rmGamesettingsNode, "EnableAging", EnableAging);
        MinimumAge = GetNodes.GetNodeValue(rmGamesettingsNode, "Minimum_Age", MinimumAge);
        MaximumAge = GetNodes.GetNodeValue(rmGamesettingsNode, "Maximum_Age", MaximumAge);
        MaxContractDisputePeriods = GetNodes.GetNodeValue(rmGamesettingsNode, "MaxContractDisputePeriods", MaxContractDisputePeriods);
        ChangeProfessionCharge = GetNodes.GetNodeValue(rmGamesettingsNode, "ChangeProfessionCharge", ChangeProfessionCharge);
        ChangeProfessionCost = GetNodes.GetNodeValue(rmGamesettingsNode, "ChangeProfessionCost", ChangeProfessionCost);
        EnableSalaries = GetNodes.GetNodeValue(rmGamesettingsNode, "EnableSalaries", EnableSalaries);
        DefaultSalary = GetNodes.GetNodeValue(rmGamesettingsNode, "DefaultSalary", DefaultSalary);
        SalaryPeriod = GetNodes.GetNodeValue(rmGamesettingsNode, "SalaryPeriod", SalaryPeriod);
        if (SalaryPeriod == "Yearly")
        {
          SalaryPeriodisMonthly = false;
          SalaryPeriodisYearly = true;
        }
        else
        {
          SalaryPeriod = "Monthly";
          SalaryPeriodisMonthly = true;
          SalaryPeriodisYearly = false;
        }
      }
    }

    internal void Save(ConfigNode node)
    {
      var settingsNode = node.HasNode(ConfigNodeName) ? node.GetNode(ConfigNodeName) : node.AddNode(ConfigNodeName);

      settingsNode.AddValue("EnableAging", EnableAging);
      settingsNode.AddValue("Minimum_Age", MinimumAge);
      settingsNode.AddValue("Maximum_Age", MaximumAge);
      settingsNode.AddValue("MaxContractDisputePeriods", MaxContractDisputePeriods);
      settingsNode.AddValue("ChangeProfessionCharge", ChangeProfessionCharge);
      settingsNode.AddValue("ChangeProfessionCost", ChangeProfessionCost);
      settingsNode.AddValue("EnableSalaries", EnableSalaries);
      settingsNode.AddValue("DefaultSalary", DefaultSalary);
      settingsNode.AddValue("SalaryPeriod", SalaryPeriod);
    }
  }
}