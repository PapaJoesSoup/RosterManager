using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterManager
{
    internal class RMGameSettings
    {
        // This class stores the RosterManager Gamesettings config node, which are settings stored for each saved game.
        // which includes Settings for Aging of kerbals, charging for chaing a kerbals profession, and Salaries settings.        

        public const string configNodeName = "RMGameSettings";
        //Aging vars        
        internal bool EnableAging { get; set; }
        internal int Minimum_Age { get; set; }
        internal int Maximum_Age { get; set; }
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
            Minimum_Age = 25;
            Maximum_Age = 75;
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
            if (node.HasNode(configNodeName))
            {
                ConfigNode RMGamesettingsNode = node.GetNode(configNodeName);
                EnableAging = GetNodes.GetNodeValue(RMGamesettingsNode, "EnableAging", EnableAging);
                Minimum_Age = GetNodes.GetNodeValue(RMGamesettingsNode, "Minimum_Age", Minimum_Age);
                Maximum_Age = GetNodes.GetNodeValue(RMGamesettingsNode, "Maximum_Age", Maximum_Age);
                MaxContractDisputePeriods = GetNodes.GetNodeValue(RMGamesettingsNode, "MaxContractDisputePeriods", MaxContractDisputePeriods);
                ChangeProfessionCharge = GetNodes.GetNodeValue(RMGamesettingsNode, "ChangeProfessionCharge", ChangeProfessionCharge);
                ChangeProfessionCost = GetNodes.GetNodeValue(RMGamesettingsNode, "ChangeProfessionCost", ChangeProfessionCost);
                EnableSalaries = GetNodes.GetNodeValue(RMGamesettingsNode, "EnableSalaries", EnableSalaries);
                DefaultSalary = GetNodes.GetNodeValue(RMGamesettingsNode, "DefaultSalary", DefaultSalary);                
                SalaryPeriod = GetNodes.GetNodeValue(RMGamesettingsNode, "SalaryPeriod", SalaryPeriod);
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
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }

            settingsNode.AddValue("EnableAging", EnableAging);
            settingsNode.AddValue("Minimum_Age", Minimum_Age);
            settingsNode.AddValue("Maximum_Age", Maximum_Age);
            settingsNode.AddValue("MaxContractDisputePeriods", MaxContractDisputePeriods);
            settingsNode.AddValue("ChangeProfessionCharge", ChangeProfessionCharge);
            settingsNode.AddValue("ChangeProfessionCost", ChangeProfessionCost);
            settingsNode.AddValue("EnableSalaries", EnableSalaries);
            settingsNode.AddValue("DefaultSalary", DefaultSalary);            
            settingsNode.AddValue("SalaryPeriod", SalaryPeriod);
        }        
    }
}
