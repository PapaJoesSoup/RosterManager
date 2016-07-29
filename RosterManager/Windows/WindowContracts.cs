using System.Collections.Generic;
using System.Linq;
using RosterManager.Api;
using RosterManager.InternalObjects;
using UnityEngine;

namespace RosterManager.Windows
{
  internal static class WindowContracts
  {
    // This class creates a GUI for ContractDispute data and processes user inputs for this data.

    #region Contracts Window (GUI)

    internal static float WindowWidth = 700;
    internal static float WindowHeight = 330;
    internal static Rect Position = new Rect(0, 0, 700, 330);
    internal static bool ShowWindow;
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    private static Vector2 _scrollViewerPosition = Vector2.zero;
    private static bool _recontractFlag;

    //Kerbal List Filter vars
    internal static bool IsAll = true;

    internal static bool IsDispute;
    internal static bool IsStrike;
    internal static bool IsContracted;

    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      var rect = new Rect(680, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
      {
        ToolTip = "";
        ShowWindow = false;
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.BeginVertical();
      DisplayRosterFilter();

      DisplayHeadings();

      _scrollViewerPosition = GUILayout.BeginScrollView(_scrollViewerPosition, RMStyle.ScrollStyle, GUILayout.Height(280), GUILayout.Width(680));
      GUILayout.BeginVertical();
      if (IsAll || IsDispute || IsStrike)
        DisplayDisputes();
      if (IsAll || IsContracted)
        DisplayContracted();

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      var disputekerbals = new List<RMKerbal>(RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Values.Where(a => a.SalaryContractDispute).ToList());
      string buttonToolTip;
      if (disputekerbals.Any())
      {
        buttonToolTip = "Accept All Payrises. Kerbals will continue working.";
        if (GUILayout.Button(new GUIContent("Accept All", buttonToolTip), RMStyle.ButtonStyle))
        {
          // Accept all disputes.
          foreach (var disputekerbal in disputekerbals.Where(disputekerbal => !disputekerbal.SalaryContractDisputeProcessed))
          {
            AcceptDispute(disputekerbal);
          }
          ToolTip = "";
          ShowWindow = false;
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

        buttonToolTip = "Decline All Payrises, All listed kerbals will become tourists until you can pay them.";
        if (GUILayout.Button(new GUIContent("Decline All", buttonToolTip), RMStyle.ButtonStyle))
        {
          // Decline all disputes.
          foreach (var disputekerbal in disputekerbals.Where(disputekerbal => !disputekerbal.SalaryContractDisputeProcessed))
          {
            DeclineDispute(disputekerbal);
          }
          ToolTip = "";
          ShowWindow = false;
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      }
      if (_recontractFlag)
      {
        buttonToolTip = "ReContract All Disputed Kerbals.";
        if (GUILayout.Button(new GUIContent("Recontract All", buttonToolTip), RMStyle.ButtonStyle))
        {
          // ReContract all disputes.
          foreach (var disputekerbal in disputekerbals.Where(disputekerbal => disputekerbal.SalaryContractDisputeProcessed))
          {
            ReContract(disputekerbal);
          }
          ToolTip = "";
          ShowWindow = false;
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      }
      GUILayout.EndHorizontal();

      GUILayout.EndVertical();

      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      RMSettings.RepositionWindows("WindowContracts");
    }

    private static void DisplayRosterFilter()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Kerbal Filter:", GUILayout.Width(90));
      IsAll = GUILayout.Toggle(IsAll, "All", GUILayout.Width(50));
      if (IsAll)
        IsDispute = IsContracted = IsStrike = false;
      else
      {
        if (!IsDispute && !IsContracted && !IsStrike)
          IsAll = true;
      }
      IsDispute = GUILayout.Toggle(IsDispute, "Dispute", GUILayout.Width(80));
      if (IsDispute)
        IsAll = IsContracted = IsStrike = false;
      else
      {
        if (!IsAll && !IsContracted && !IsStrike)
          IsDispute = true;
      }
      IsStrike = GUILayout.Toggle(IsStrike, "On Strike", GUILayout.Width(80));
      if (IsStrike)
        IsAll = IsDispute = IsContracted = false;
      else
      {
        if (!IsAll && !IsDispute && !IsContracted)
          IsStrike = true;
      }
      IsContracted = GUILayout.Toggle(IsContracted, "Contracted", GUILayout.Width(80));
      if (IsContracted)
        IsAll = IsDispute = IsStrike = false;
      else
      {
        if (!IsAll && !IsDispute && !IsStrike)
          IsContracted = true;
      }

      GUILayout.EndHorizontal();
    }

    private static void DisplayDisputes()
    {
      _recontractFlag = false;
      var disputekerbals = new List<RMKerbal>(RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Values.Where(a => a.SalaryContractDispute).ToList());
      foreach (var disputekerbal in disputekerbals.Where(disputekerbal => IsAll || (IsDispute && disputekerbal.Trait != "Tourist") || (IsStrike && disputekerbal.Trait == "Tourist")))
      {
        GUILayout.BeginHorizontal();
        DisplayKerbal(disputekerbal);
        string buttonToolTip;
        Rect rect;
        if (disputekerbal.SalaryContractDisputeProcessed || disputekerbal.Trait == "Tourist")
        {
          _recontractFlag = true;
          if (Funding.CanAfford((float)disputekerbal.Salary + (float)disputekerbal.OwedSalary))
          {
            GUI.enabled = true;
            buttonToolTip = "Re-Instate kerbal and pay all owed funds.";
          }
          else
          {
            GUI.enabled = false;
            buttonToolTip = "Not enough funds to Re-Instate kerbal and pay all owed funds.";
          }

          if (GUILayout.Button(new GUIContent("Re-Contract", buttonToolTip), RMStyle.ButtonStyle, GUILayout.Width(100)))
          {
            ReContract(disputekerbal);
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
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
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

          buttonToolTip = "Decline Payrise. Kerbal will become a tourist until paid.";
          if (GUILayout.Button(new GUIContent("Decline", buttonToolTip), RMStyle.ButtonStyle, GUILayout.Width(80)))
          {
            //Decline payrise
            DeclineDispute(disputekerbal);
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
        }
        GUILayout.EndHorizontal();
      }
    }

    private static void DisplayContracted()
    {
      var contractedkerbals = new List<RMKerbal>(RMLifeSpan.Instance.RMKerbals.AllrmKerbals.Values.Where(a => a.SalaryContractDispute == false).ToList());

      foreach (var kerbal in contractedkerbals)
      {
        GUILayout.BeginHorizontal();

        DisplayKerbal(kerbal);

        GUILayout.EndHorizontal();
      }
    }

    private static void DisplayHeadings()
    {
      GUILayout.BeginHorizontal();
      GUI.enabled = true;
      var buttonToolTip = "Crew Member Name.";
      GUILayout.Label(new GUIContent("Crew Name", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(130));
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Crew Member's Current Salary.";
      GUILayout.Label(new GUIContent("Salary", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(55));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Crew Member's Current Outstanding/Owing Salary.";
      GUILayout.Label(new GUIContent("BackPay", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(60));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "How many Pay periods salary has been in dispute.";
      GUILayout.Label(new GUIContent("#", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(15));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Payrise Requested.";
      GUILayout.Label(new GUIContent("PayRise", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(55));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "kerbals usual profession when working.";
      GUILayout.Label(new GUIContent("Profession", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(75));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Next Salary payment is due at.";
      GUILayout.Label(new GUIContent("Salary Due", buttonToolTip), RMStyle.LabelStyleBold, GUILayout.Width(65));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
    }

    private static void DisplayKerbal(RMKerbal kerbal)
    {
      GUIStyle labelStyle;
      if (kerbal.Status == ProtoCrewMember.RosterStatus.Dead || kerbal.Status == ProtoCrewMember.RosterStatus.Missing
          || (kerbal.SalaryContractDispute && kerbal.Trait == "Tourist"))
        labelStyle = RMStyle.LabelStyleRed;
      else if (kerbal.SalaryContractDispute && kerbal.Trait != "Tourist")
        labelStyle = RMStyle.LabelStyleMagenta;
      else if (kerbal.Status == ProtoCrewMember.RosterStatus.Assigned)
        labelStyle = RMStyle.LabelStyleYellow;
      else
        labelStyle = RMStyle.LabelStyle;
      if (InstalledMods.IsDfInstalled && kerbal.Type == ProtoCrewMember.KerbalType.Unowned)
      {
        labelStyle = RMStyle.LabelStyleCyan;
      }

      var buttonToolTip = "Crew Member Name.";
      GUILayout.Label(new GUIContent(kerbal.Name, buttonToolTip), labelStyle, GUILayout.Width(130));
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Crew Member's Current Salary.";
      GUILayout.Label(new GUIContent(kerbal.Salary.ToString("###,##0"), buttonToolTip), labelStyle, GUILayout.Width(55));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Crew Member's Current Outstanding/Owing Salary.";
      GUILayout.Label(new GUIContent(kerbal.OwedSalary.ToString("###,##0"), buttonToolTip), labelStyle, GUILayout.Width(60));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "How many Pay periods salary has been in dispute.";
      GUILayout.Label(new GUIContent(kerbal.SalaryContractDisputePeriods.ToString("#0"), buttonToolTip), labelStyle, GUILayout.Width(15));
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Payrise Requested.";
      GUILayout.Label(new GUIContent(kerbal.PayriseRequired.ToString("###,##0"), buttonToolTip), labelStyle, GUILayout.Width(55));
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Kerbals usual Profession.";
      GUILayout.Label(new GUIContent(kerbal.RealTrait, buttonToolTip), labelStyle, GUILayout.Width(75));
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      buttonToolTip = "Next Salary payment is due on:";
      GUILayout.Label(new GUIContent(KSPUtil.PrintDate((int)kerbal.TimeSalaryDue, false), buttonToolTip), labelStyle, GUILayout.Width(65));
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = RMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
    }

    private static void DeclineDispute(RMKerbal disputekerbal)
    {
      var kerbal = new KeyValuePair<string, RMKerbal>(disputekerbal.Name, disputekerbal);
      RMLifeSpanAddon.Instance.ResignKerbal(disputekerbal.Kerbal, kerbal);
      disputekerbal.SalaryContractDisputeProcessed = true;
    }

    private static void AcceptDispute(RMKerbal disputekerbal)
    {
      // Add requested payrise amount to their regular salary
      disputekerbal.Salary += disputekerbal.PayriseRequired;
      disputekerbal.SalaryContractDispute = true;
      // Calculate and store their backpay.
      disputekerbal.OwedSalary += disputekerbal.Salary;
      disputekerbal.SalaryContractDisputeProcessed = true;
      disputekerbal.PayriseRequired = 0;
    }

    private static void ReContract(RMKerbal disputekerbal)
    {
      if (Funding.CanAfford((float)disputekerbal.Salary + (float)disputekerbal.OwedSalary))
      {
        Funding.Instance.AddFunds(-(disputekerbal.Salary + disputekerbal.OwedSalary), TransactionReasons.CrewRecruited);
        disputekerbal.Timelastsalary = Planetarium.GetUniversalTime();
        disputekerbal.SalaryContractDispute = false;
        disputekerbal.SalaryContractDisputeProcessed = true;
        disputekerbal.SalaryContractDisputePeriods = 0;
        disputekerbal.PayriseRequired = 0d;
        disputekerbal.OwedSalary = 0d;
        //If they are a tourist (dispute) and not dead (DeepFreeze frozen/comatose) set them back to crew
        if (disputekerbal.Type == ProtoCrewMember.KerbalType.Tourist && disputekerbal.Status != ProtoCrewMember.RosterStatus.Dead)
        {
          disputekerbal.Type = ProtoCrewMember.KerbalType.Crew;
          disputekerbal.Kerbal.type = ProtoCrewMember.KerbalType.Crew;
          disputekerbal.Trait = disputekerbal.RealTrait;
          disputekerbal.Kerbal.trait = disputekerbal.RealTrait;
          KerbalRoster.SetExperienceTrait(disputekerbal.Kerbal, disputekerbal.Kerbal.trait);
          RMKerbal.RegisterExperienceTrait(disputekerbal);
        }
      }
    }

    #endregion Contracts Window (GUI)
  }
}