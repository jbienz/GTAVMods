using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.UI;

public sealed class FriendAndFoe : Script
{
    private const float FeetToMeters = 0.3048f;
    private const float DefaultRadiusFeet = 30.0f;
    private const float DefaultThreatScanRadiusFeet = 300.0f;
    private const int DefaultMaximumCrewSize = 7;
    private const int DefaultCombatRefreshMilliseconds = 1000;
    private const WeaponHash DefaultRecruitWeapon = WeaponHash.UpNAtomizer;

    private readonly Keys recruitKey;
    private readonly Keys hostileKey;
    private readonly Keys dismissKey;
    private readonly float radiusMeters;
    private readonly float threatScanRadiusMeters;
    private readonly int maximumCrewSize;
    private readonly int combatRefreshMilliseconds;
    private readonly bool armUnarmedRecruits;
    private readonly WeaponHash recruitWeapon;
    private readonly bool includePolice;
    private readonly bool includeMissionPeds;
    private readonly bool includeAnimals;
    private readonly bool hostilesIncludeCrew;

    private readonly List<PedState> crew = new List<PedState>();
    private readonly List<PedState> hostiles = new List<PedState>();

    private RelationshipGroup hostileRelationshipGroup;
    private int nextCombatRefreshTime;

    public FriendAndFoe()
    {
        ScriptSettings settings = ScriptSettings.Load(@"scripts\FriendAndFoe\FriendAndFoe.ini");

        recruitKey = ReadKey(settings, "RecruitKey", Keys.F6);
        hostileKey = ReadKey(settings, "HostileKey", Keys.F7);
        dismissKey = ReadKey(settings, "DismissKey", Keys.F9);
        radiusMeters = Math.Max(1.0f, settings.GetValue("FriendAndFoe", "RadiusFeet", DefaultRadiusFeet)) * FeetToMeters;
        threatScanRadiusMeters = Math.Max(1.0f, settings.GetValue("FriendAndFoe", "ThreatScanRadiusFeet", DefaultThreatScanRadiusFeet)) * FeetToMeters;
        maximumCrewSize = Math.Min(7, Math.Max(1, settings.GetValue("FriendAndFoe", "MaximumCrewSize", DefaultMaximumCrewSize)));
        combatRefreshMilliseconds = Math.Max(250, settings.GetValue("FriendAndFoe", "CombatRefreshMilliseconds", DefaultCombatRefreshMilliseconds));
        armUnarmedRecruits = settings.GetValue("FriendAndFoe", "ArmUnarmedRecruits", true);
        recruitWeapon = ReadWeaponHash(settings, "RecruitWeapon", DefaultRecruitWeapon);
        includePolice = settings.GetValue("FriendAndFoe", "IncludePolice", false);
        includeMissionPeds = settings.GetValue("FriendAndFoe", "IncludeMissionPeds", false);
        includeAnimals = settings.GetValue("FriendAndFoe", "IncludeAnimals", false);
        hostilesIncludeCrew = settings.GetValue("FriendAndFoe", "HostilesIncludeCrew", false);

        hostileRelationshipGroup = World.AddRelationshipGroup("FRIEND_AND_FOE_HOSTILE");

        KeyDown += OnKeyDown;
        Tick += OnTick;
        Aborted += OnAborted;
    }

    private static Keys ReadKey(ScriptSettings settings, string settingName, Keys defaultKey)
    {
        string configuredKey = settings.GetValue("FriendAndFoe", settingName, defaultKey.ToString());
        Keys parsedKey;

        if (Enum.TryParse(configuredKey, true, out parsedKey))
        {
            return parsedKey;
        }

        return defaultKey;
    }

    private static WeaponHash ReadWeaponHash(ScriptSettings settings, string settingName, WeaponHash defaultWeapon)
    {
        string configuredWeapon = settings.GetValue("FriendAndFoe", settingName, defaultWeapon.ToString());
        WeaponHash parsedWeapon;

        if (Enum.TryParse(configuredWeapon, true, out parsedWeapon) && Enum.IsDefined(typeof(WeaponHash), parsedWeapon))
        {
            return parsedWeapon;
        }

        return defaultWeapon;
    }

    private void OnKeyDown(object sender, KeyEventArgs eventArgs)
    {
        Ped player = Game.LocalPlayerPed;
        if (!IsUsablePed(player))
        {
            return;
        }

        if (eventArgs.KeyCode == recruitKey)
        {
            RecruitNearbyPeds();
        }
        else if (eventArgs.KeyCode == hostileKey)
        {
            MakeNearbyPedsHostile();
        }
        else if (eventArgs.KeyCode == dismissKey)
        {
            DismissCrewAndResetNpcs();
        }
    }

    private void OnTick(object sender, EventArgs eventArgs)
    {
        RemoveInvalidPeds(crew);
        RemoveInvalidPeds(hostiles);

        if (Game.GameTime < nextCombatRefreshTime)
        {
            return;
        }

        nextCombatRefreshTime = Game.GameTime + combatRefreshMilliseconds;

        Ped player = Game.LocalPlayerPed;
        if (!IsUsablePed(player) || (hostiles.Count == 0 && crew.Count == 0))
        {
            return;
        }

        RefreshCombatTasks(player);
    }

    [Browsable(true)]
    [Category("NPC Commands")]
    [Description("Recruit Nearby NPCs")]
    private void RecruitNearbyPeds()
    {
        Ped player = Game.LocalPlayerPed;
        if (!IsUsablePed(player))
        {
            return;
        }

        RecruitNearbyPeds(player);
    }

    [Browsable(true)]
    [Category("NPC Commands")]
    [Description("Make Nearby NPCs Hostile")]
    private void MakeNearbyPedsHostile()
    {
        Ped player = Game.LocalPlayerPed;
        if (!IsUsablePed(player))
        {
            return;
        }

        EnrageNearbyPeds(player);
    }

    [Browsable(true)]
    [Category("NPC Commands")]
    [Description("Dismiss Crew and Reset NPCs")]
    private void DismissCrewAndResetNpcs()
    {
        int affectedCount = crew.Count + hostiles.Count;
        RestoreAllPeds();
        ShowMessage("FriendAndFoe reset " + affectedCount + " NPC(s).");
    }

    private void OnAborted(object sender, EventArgs eventArgs)
    {
        RestoreAllPeds();

        if (hostileRelationshipGroup != null)
        {
            Function.Call(Hash.REMOVE_RELATIONSHIP_GROUP, hostileRelationshipGroup.Hash);
            hostileRelationshipGroup = null;
        }
    }

    private void RecruitNearbyPeds(Ped player)
    {
        ConfigureRelationships(player);
        Ped[] nearbyPeds = World.GetNearbyPeds(player, radiusMeters);
        int playerGroup = Function.Call<int>(Hash.GET_PED_GROUP_INDEX, player.Handle);
        int recruitedCount = 0;

        for (int index = 0; index < nearbyPeds.Length && crew.Count < maximumCrewSize; index++)
        {
            Ped ped = nearbyPeds[index];
            if (!CanAffectPed(ped) || ContainsPed(crew, ped) || ContainsPed(hostiles, ped))
            {
                continue;
            }

            PedState state = CaptureState(ped);
            PreparePedForScriptedBehavior(ped);
            ped.RelationshipGroup = player.RelationshipGroup;
            Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped.Handle, playerGroup);

            if (!ped.IsInGroup || Function.Call<int>(Hash.GET_PED_GROUP_INDEX, ped.Handle) != playerGroup)
            {
                RestorePed(state);
                continue;
            }

            ped.NeverLeavesGroup = true;

            if (armUnarmedRecruits && ped.Weapons.Current.Hash == WeaponHash.Unarmed)
            {
                ped.Weapons.Give(recruitWeapon, 90, true, true);
            }

            crew.Add(state);
            recruitedCount++;
        }

        ShowMessage("FriendAndFoe recruited " + recruitedCount + " NPC(s). Crew: " + crew.Count + "/" + maximumCrewSize + ".");
    }

    private void EnrageNearbyPeds(Ped player)
    {
        ConfigureRelationships(player);
        Ped[] nearbyPeds = World.GetNearbyPeds(player, radiusMeters);
        int hostileCount = 0;

        for (int index = 0; index < nearbyPeds.Length; index++)
        {
            Ped ped = nearbyPeds[index];
            if (!CanAffectPed(ped) || ContainsPed(hostiles, ped))
            {
                continue;
            }

            PedState crewState = FindPedState(crew, ped);
            if (crewState != null)
            {
                if (!hostilesIncludeCrew)
                {
                    continue;
                }

                crew.Remove(crewState);
                RestorePed(crewState);
            }

            PedState state = CaptureState(ped);
            PreparePedForScriptedBehavior(ped);
            ped.RelationshipGroup = hostileRelationshipGroup;
            ped.Task.Combat(player);
            hostiles.Add(state);
            hostileCount++;
        }

        nextCombatRefreshTime = Game.GameTime;
        ShowMessage("FriendAndFoe enraged " + hostileCount + " NPC(s). Hostiles: " + hostiles.Count + ".");
    }

    private void ConfigureRelationships(Ped player)
    {
        if (hostileRelationshipGroup == null)
        {
            hostileRelationshipGroup = World.AddRelationshipGroup("FRIEND_AND_FOE_HOSTILE");
        }

        Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, (int)Relationship.Hate, hostileRelationshipGroup.Hash, player.RelationshipGroup.Hash);
        Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, (int)Relationship.Hate, player.RelationshipGroup.Hash, hostileRelationshipGroup.Hash);
    }

    private void RefreshCombatTasks(Ped player)
    {
        for (int index = 0; index < hostiles.Count; index++)
        {
            Ped hostile = hostiles[index].Ped;
            Ped target = FindClosestCrewTarget(hostile, player);
            if (IsUsablePed(target))
            {
                hostile.Task.Combat(target);
            }
        }

        for (int index = 0; index < crew.Count; index++)
        {
            Ped ally = crew[index].Ped;
            Ped target = FindClosestThreat(ally, player);
            if (IsUsablePed(target))
            {
                ally.Task.Combat(target);
            }
        }
    }

    private Ped FindClosestCrewTarget(Ped hostile, Ped player)
    {
        Ped closestTarget = player;
        float closestDistanceSquared = hostile.Position.DistanceToSquared(player.Position);

        for (int index = 0; index < crew.Count; index++)
        {
            Ped ally = crew[index].Ped;
            if (!IsUsablePed(ally))
            {
                continue;
            }

            float distanceSquared = hostile.Position.DistanceToSquared(ally.Position);
            if (distanceSquared < closestDistanceSquared)
            {
                closestTarget = ally;
                closestDistanceSquared = distanceSquared;
            }
        }

        return closestTarget;
    }

    private Ped FindClosestThreat(Ped ally, Ped player)
    {
        Ped closestTarget = null;
        float closestDistanceSquared = float.MaxValue;

        for (int index = 0; index < hostiles.Count; index++)
        {
            Ped hostile = hostiles[index].Ped;
            if (!IsUsablePed(hostile))
            {
                continue;
            }

            float distanceSquared = ally.Position.DistanceToSquared(hostile.Position);
            if (distanceSquared < closestDistanceSquared)
            {
                closestTarget = hostile;
                closestDistanceSquared = distanceSquared;
            }
        }

        Ped[] nearbyPeds = World.GetNearbyPeds(ally, threatScanRadiusMeters);
        for (int index = 0; index < nearbyPeds.Length; index++)
        {
            Ped candidate = nearbyPeds[index];
            if (!IsThreatToPlayer(candidate, player))
            {
                continue;
            }

            float distanceSquared = ally.Position.DistanceToSquared(candidate.Position);
            if (distanceSquared < closestDistanceSquared)
            {
                closestTarget = candidate;
                closestDistanceSquared = distanceSquared;
            }
        }

        return closestTarget;
    }

    private bool IsThreatToPlayer(Ped candidate, Ped player)
    {
        if (!IsUsablePed(candidate) || candidate.IsPlayer || ContainsPed(crew, candidate))
        {
            return false;
        }

        if (ContainsPed(hostiles, candidate) || Function.Call<bool>(Hash.IS_PED_IN_COMBAT, candidate.Handle, player.Handle))
        {
            return true;
        }

        Relationship relationship = candidate.GetRelationshipWithPed(player);
        return relationship == Relationship.Dislike || relationship == Relationship.Hate;
    }

    private bool CanAffectPed(Ped ped)
    {
        if (!IsUsablePed(ped) || ped.IsPlayer)
        {
            return false;
        }

        if (!includeAnimals && !ped.IsHuman)
        {
            return false;
        }

        if (!includeMissionPeds && Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, ped.Handle))
        {
            return false;
        }

        if (!includePolice && ped.RelationshipGroup.Hash == unchecked((int)RelationshipGroupHash.Cop))
        {
            return false;
        }

        return true;
    }

    private static bool IsUsablePed(Ped ped)
    {
        return ped != null && ped.Exists() && !ped.IsDead;
    }

    private static void PreparePedForScriptedBehavior(Ped ped)
    {
        ped.SetIsPersistentNoClearTask(true);
        ped.BlockPermanentEvents = true;
        ped.Task.ClearAll();
    }

    private static PedState CaptureState(Ped ped)
    {
        PedGroup pedGroup = ped.PedGroup;
        bool wasGroupLeader = false;

        if (pedGroup != null && pedGroup.Exists())
        {
            Ped groupLeader = Function.Call<Ped>(Hash.GET_PED_AS_GROUP_LEADER, pedGroup);
            wasGroupLeader = groupLeader != null && groupLeader.Exists() && groupLeader.Handle == ped.Handle;
        }

        return new PedState(
            ped,
            ped.RelationshipGroup,
            pedGroup,
            wasGroupLeader,
            ped.NeverLeavesGroup,
            ped.BlockPermanentEvents,
            ped.IsPersistent);
    }

    private static void RestorePed(PedState state)
    {
        Ped ped = state.Ped;
        if (ped == null || !ped.Exists())
        {
            return;
        }

        if (ped.IsInGroup)
        {
            Function.Call(Hash.REMOVE_PED_FROM_GROUP, ped.Handle);
        }

        if (state.PedGroup != null && state.PedGroup.Exists())
        {
            if (state.WasGroupLeader)
            {
                Function.Call(Hash.SET_PED_AS_GROUP_LEADER, ped.Handle, state.PedGroup);
            }
            else
            {
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped.Handle, state.PedGroup);
            }
        }

        ped.Task.ClearAll();
        ped.RelationshipGroup = state.RelationshipGroup;
        ped.NeverLeavesGroup = state.NeverLeavesGroup;
        ped.BlockPermanentEvents = state.BlockPermanentEvents;
        ped.SetIsPersistentNoClearTask(state.IsPersistent);
    }

    private void RestoreAllPeds()
    {
        for (int index = 0; index < crew.Count; index++)
        {
            RestorePed(crew[index]);
        }

        for (int index = 0; index < hostiles.Count; index++)
        {
            RestorePed(hostiles[index]);
        }

        crew.Clear();
        hostiles.Clear();
    }

    private static void RemoveInvalidPeds(List<PedState> states)
    {
        for (int index = states.Count - 1; index >= 0; index--)
        {
            if (!IsUsablePed(states[index].Ped))
            {
                states.RemoveAt(index);
            }
        }
    }

    private static bool ContainsPed(List<PedState> states, Ped ped)
    {
        return FindPedState(states, ped) != null;
    }

    private static PedState FindPedState(List<PedState> states, Ped ped)
    {
        for (int index = 0; index < states.Count; index++)
        {
            if (states[index].Ped.Handle == ped.Handle)
            {
                return states[index];
            }
        }

        return null;
    }

    private static void ShowMessage(string message)
    {
        Notification.PostTicker(message, false, false);
    }

    private sealed class PedState
    {
        public readonly Ped Ped;
        public readonly RelationshipGroup RelationshipGroup;
        public readonly PedGroup PedGroup;
        public readonly bool WasGroupLeader;
        public readonly bool NeverLeavesGroup;
        public readonly bool BlockPermanentEvents;
        public readonly bool IsPersistent;

        public PedState(
            Ped ped,
            RelationshipGroup relationshipGroup,
            PedGroup pedGroup,
            bool wasGroupLeader,
            bool neverLeavesGroup,
            bool blockPermanentEvents,
            bool isPersistent)
        {
            Ped = ped;
            RelationshipGroup = relationshipGroup;
            PedGroup = pedGroup;
            WasGroupLeader = wasGroupLeader;
            NeverLeavesGroup = neverLeavesGroup;
            BlockPermanentEvents = blockPermanentEvents;
            IsPersistent = isPersistent;
        }
    }
}