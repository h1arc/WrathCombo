#region

using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using Lumina.Excel.Sheets;
using WrathCombo.CustomComboNS;
using WrathCombo.Extensions;
using WrathCombo.Services;

#endregion

namespace WrathCombo.Combos.PvE.Content;

public class Quests
{
    private static Job Job => Player.Job;

    private static IGameObject? Target => SimpleTarget.HardTarget;

    private static IGameObject? HealTarget =>
        Service.Configuration.RetargetHealingActionsToStack
            ? SimpleTarget.Stack.AllyToHeal
            : null;

    public static bool TryGetQuestActionFix(ref uint actionID)
    {
        if (TryGetCNJFix(ref actionID)) return true;
        if (TryGetARCFix(ref actionID)) return true;

        return false;
    }

    private static bool TryGetCNJFix(ref uint actionID)
    {
        if (Job != Job.CNJ)
            return false;

        #region Level 30 CNJ Quest Fix

        var target = Target.IfFriendly() ?? HealTarget;

        if (Player.Level > 29 &&
            target?.ObjectKind is ObjectKind.EventNpc &&
            target.BaseId is 1008174)
        {
            actionID = WHM.Cure;
            return true;
        }

        #endregion

        return false;
    }
    
    private static bool TryGetARCFix(ref uint actionID)
    {
        if (Job != Job.ARC)
            return false;

        #region Level 5-15 ARC Quest Fix

        var archeryButt = Svc.Data.GetExcelSheet<EObjName>()[2000925]
            .Singular.ToString();

        if (Player.Level < 30 &&
            (Target.Name.TextValue.Equals(archeryButt,
                 StringComparison.InvariantCultureIgnoreCase) ||
             SimpleTarget.NearestEnemyTarget.Name.TextValue.Equals(archeryButt,
                 StringComparison.InvariantCultureIgnoreCase) ||
             Svc.Objects.Any(x => x.Name.TextValue.Equals(archeryButt,
                 StringComparison.InvariantCultureIgnoreCase) && x.IsTargetable)))
        {
            actionID = BRD.HeavyShot;
            return true;
        }

        #endregion

        return false;
    }
}