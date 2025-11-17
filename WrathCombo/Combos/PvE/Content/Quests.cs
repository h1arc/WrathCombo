#region

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using WrathCombo.CustomComboNS;
using WrathCombo.Extensions;
using WrathCombo.Services;

#endregion

namespace WrathCombo.Combos.PvE.Content;

public class Quests
{
    private static Job Job => Player.Job;

    private static IGameObject? Target => Player.Object.TargetObject;

    private static IGameObject? HealTarget =>
        Service.Configuration.RetargetHealingActionsToStack
            ? SimpleTarget.Stack.AllyToHeal
            : null;

    public static bool TryGetQuestActionFix(ref uint actionID)
    {
        if (TryGetCNJFix(ref actionID)) return true;

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
}