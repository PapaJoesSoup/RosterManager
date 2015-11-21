using System.Collections.Generic;
using UnityEngine;

namespace RosterManager
{
    internal static class WindowContractDispute
    {
        #region Settings Window (GUI)
        internal static float windowWidth = 700;
        internal static float windowHeight = 330;
        internal static Rect Position = new Rect(0, 0, 700, 330);
        internal static bool ShowWindow = false;
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;
        internal static string ToolTip = "";

        private static Vector2 ScrollViewerPosition = Vector2.zero; 
        private static List<disputeKerbal> processeddisputekerbals = new List<disputeKerbal>();

        internal static void Display(int windowId)
        {            
            // Reset Tooltip active flag...            
            ToolTipActive = false;

            Rect rect = new Rect(680, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                ToolTip = "";
                WindowContractDispute.ShowWindow = false;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 0, 0);

            
            GUILayout.BeginVertical();
            ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, GUILayout.Height(280), GUILayout.Width(680));
            GUILayout.BeginVertical();

            DisplayDisputes();
                       
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            string buttonToolTip = string.Empty;
            buttonToolTip = "Accept All Payrises. Kerbals will continue working.";
            if (GUILayout.Button("Accept"))
            {
                // Accept all disputes.
                processeddisputekerbals.Clear();
                foreach (disputeKerbal disputekerbal in LifeSpanAddon.Instance.ContractDisputeKerbals)
                {
                    AcceptDispute(disputekerbal);
                }
                processeddisputekerbals.ForEach(id => LifeSpanAddon.Instance.ContractDisputeKerbals.Remove(id));
                WindowContractDispute.ShowWindow = false;                                
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

            buttonToolTip = "Decline All Payrises, All listed kerbals will become tourists until you can pay them.";
            if (GUILayout.Button("Decline"))
            {
                // Decline all disputes.    
                processeddisputekerbals.Clear();
                foreach (disputeKerbal disputekerbal in LifeSpanAddon.Instance.ContractDisputeKerbals)
                {
                    DeclineDispute(disputekerbal);
                }
                processeddisputekerbals.ForEach(id => LifeSpanAddon.Instance.ContractDisputeKerbals.Remove(id));
                WindowContractDispute.ShowWindow = false;
                
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            RMSettings.RepositionWindows("Contract Disputes");
        }
              
        private static void DisplayDisputes()
        {
            GUILayout.BeginHorizontal();
            Rect rect = new Rect();
            GUI.enabled = true;
            string buttonToolTip = string.Empty;
            buttonToolTip = "Crew Member Name.";
            GUILayout.Label(new GUIContent("Crew Name", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(150));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

            buttonToolTip = "Crew Member's Current Salary.";
            GUILayout.Label(new GUIContent("Salary", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(80));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

            buttonToolTip = "Crew Member's Current Outstanding/Owing Salary.";
            GUILayout.Label(new GUIContent("BackPay", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(80));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

            buttonToolTip = "How many Pay periods salary has been in dispute.";
            GUILayout.Label(new GUIContent("#", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(30));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

            buttonToolTip = "Payrise Requested.";
            GUILayout.Label(new GUIContent("PayRise Amt", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(80));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

            GUILayout.EndHorizontal();

            processeddisputekerbals.Clear();

            foreach (disputeKerbal disputekerbal in LifeSpanAddon.Instance.ContractDisputeKerbals)
            {
                buttonToolTip = string.Empty;
                GUILayout.BeginHorizontal();
                buttonToolTip = "Crew Member Name.";
                GUILayout.Label(disputekerbal.crew.name, RMStyle.LabelStyle, GUILayout.Width(150));
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                buttonToolTip = "Crew Member's Current Salary.";
                GUILayout.Label(disputekerbal.kerbal.Value.salary.ToString("###,##0"), RMStyle.LabelStyle, GUILayout.Width(80));
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                buttonToolTip = "Crew Member's Current Outstanding/Owing Salary.";
                GUILayout.Label(disputekerbal.kerbal.Value.owedSalary.ToString("###,##0"), RMStyle.LabelStyle, GUILayout.Width(80));
                rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                buttonToolTip = "How many Pay periods salary has been in dispute.";
                GUILayout.Label(disputekerbal.kerbal.Value.salaryContractDisputePeriods.ToString(), RMStyle.LabelStyle, GUILayout.Width(30));
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                buttonToolTip = "Payrise Requested.";
                GUILayout.Label(disputekerbal.payriseRequired.ToString(), RMStyle.LabelStyle, GUILayout.Width(30));
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                if (disputekerbal.extended)
                {
                    if (Funding.CanAfford((float)disputekerbal.kerbal.Value.salary + (float)disputekerbal.kerbal.Value.owedSalary))
                    {
                        GUI.enabled = true;
                        buttonToolTip = "Re-Instate kerbal and pay all owed funds.";
                    }
                    else
                    {
                        GUI.enabled = false;
                        buttonToolTip = "Not enough funds to Re-Instate kerbal and pay all owed funds.";
                    }

                    if (GUILayout.Button(new GUIContent("Re-Instate", buttonToolTip), RMStyle.ButtonStyle, GUILayout.Width(100)))
                    {
                        //Accept payrise
                        Funding.Instance.AddFunds(-(disputekerbal.kerbal.Value.salary + disputekerbal.kerbal.Value.owedSalary), TransactionReasons.CrewRecruited);
                        disputekerbal.kerbal.Value.timelastsalary = Planetarium.GetUniversalTime();
                        disputekerbal.kerbal.Value.salaryContractDispute = false;
                        //If they are a tourist (dispute) and not dead (DeepFreeze frozen/comatose) set them back to crew                   
                        if (disputekerbal.crew.type == ProtoCrewMember.KerbalType.Tourist && disputekerbal.crew.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
                        {
                            disputekerbal.kerbal.Value.type = ProtoCrewMember.KerbalType.Crew;
                            disputekerbal.crew.type = ProtoCrewMember.KerbalType.Crew;
                        }
                        processeddisputekerbals.Add(disputekerbal);                                               
                    }
                    GUI.enabled = true;
                }
                else
                {
                    buttonToolTip = "Accept Payrise.";
                    if (GUILayout.Button(new GUIContent("Accept", buttonToolTip), RMStyle.ButtonStyle, GUILayout.Width(80)))
                    {
                        //Accept payrise
                        AcceptDispute(disputekerbal);
                    }
                    rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);

                    buttonToolTip = "Decline Payrise. Kerbal will become a tourist until paid.";
                    if (GUILayout.Button(new GUIContent("Decline", buttonToolTip), RMStyle.ButtonStyle, GUILayout.Width(80)))
                    {
                        //Decline payrise
                        DeclineDispute(disputekerbal);
                    }
                    rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 30, 5 - ScrollViewerPosition.y);
                }
                
                GUILayout.EndHorizontal();
            }

            processeddisputekerbals.ForEach(id => LifeSpanAddon.Instance.ContractDisputeKerbals.Remove(id));
        }

        private static void DeclineDispute(disputeKerbal disputekerbal)
        {
            LifeSpanAddon.Instance.resignKerbal(disputekerbal.crew, disputekerbal.kerbal);
            disputekerbal.extended = true;
            disputekerbal.payriseRequired = 0;
        }

        private static void AcceptDispute(disputeKerbal disputekerbal)
        {
            disputekerbal.kerbal.Value.salary += disputekerbal.payriseRequired;
            disputekerbal.kerbal.Value.salaryContractDispute = true;
            // Calculate and store their backpay.
            if (RMSettings.SalaryPeriodisYearly)
            {
                disputekerbal.kerbal.Value.owedSalary += disputekerbal.kerbal.Value.salary * 12;
            }
            else
            {
                disputekerbal.kerbal.Value.owedSalary += disputekerbal.kerbal.Value.salary;
            }
            disputekerbal.extended = true;
            disputekerbal.payriseRequired = 0;
        }

        #endregion Settings Window (GUI)
    }
}