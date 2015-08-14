using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	public enum CritterState
	{
		NotFriend,
		Following,
		Attacking,
	}

	public class Critter
	{
		#region Constants
		static private readonly TimeSpan REFRESH_ATTACK = TimeSpan.FromSeconds(20);
		static private readonly TimeSpan REFRESH_FOLLOW = TimeSpan.FromSeconds(15);
		private const int SEARCH_COUNT = 10;	// How many peds do we consider when looking for a target
		private const float SEARCH_RANGE = 300f; // How far the critter can "see" new targets
		private const int SUPER_HEALTH = 200;
		#endregion // Constants

		#region Member Variables
		private AnimalClass animalClass;
		private string animalName;
		private Ped critterPed;
		private DateTime lastAttackTime = DateTime.Now - REFRESH_ATTACK;
		private DateTime lastFollowTime = DateTime.Now - REFRESH_FOLLOW;
		private int originalMaxHealth;
		private int originalRGroup;
		private CritterState state;
		private int strenth;
		private Ped target;
		#endregion // Member Variables

		#region Constructors
		public Critter(Ped ped)
		{
			// Store
			this.critterPed = ped;

			// Load Attributes
			LoadAttributes();
		}
		#endregion // Constructors

		#region Internal Methods
		private void AttackTarget()
		{
			// If not a friend, ignore
			if (state == CritterState.NotFriend) { return; }

			// Attacking
			if (state != CritterState.Attacking)
			{
				state = CritterState.Attacking;
				Log.Debug("Critter is now attacking");
			}

			//// Stop the critter from doing whatever it's doing
			//critterPed.Task.ClearAllImmediately();

			// Make sure we still have a target
			if (target != null)
			{
				// Run to target
				critterPed.TaskHurryToEntity(target);

				// Critter fights specific target
				critterPed.Task.FightAgainst(target);

				// Critter never stops fighting
				critterPed.AlwaysKeepTask = true;
			}
		}

		private void Follow()
		{
			// If not a friend, ignore
			if (state == CritterState.NotFriend) { return; }

			// Following
			if (state != CritterState.Following)
			{
				state = CritterState.Following;
				Log.Debug("Critter is now following");
			}

			//// Stop the critter from doing whatever it's doing
			//critterPed.Task.ClearAllImmediately();

			// Run to target
			critterPed.TaskHurryToEntity(Game.Player.Character);

			// Critter fights hated targets (but for animals they really just follow(
			critterPed.Task.FightAgainstHatedTargets(50000f);

			// Critter keeps doing above
			critterPed.AlwaysKeepTask = true;
		}

		private bool InnerCheckOnTarget(bool findNew, FindTargetMethod method)
		{
			// If not allowed in our group, ignore
			if (!IsAllowedInGroup) { return false; }

			// If not friend, warn and bail
			if (state == CritterState.NotFriend)
			{
				Log.Warn("Attempted to check on target with critter that is not a friend.");
				return false;
			}

			// If there is a target, check to see if it's alive and has health
			if (target != null)
			{
				// Is target still alive?
				if ((!target.IsAlive) || (target.Health <= 0))
				{
					// Target has died and is no longer needed
					Log.Debug("Target has died");
					target.MarkAsNoLongerNeeded();
					target = null;
				}
			}

			// If don't have a target now, see if we should try and find one
			if (target == null)
			{
				// Try to find a new one?
				if (findNew)
				{
					// Yes, try to find a new one
					FindTarget(method, true);
				}
			}

			// What time is it?
			var now = DateTime.Now;

			// If we have a target, see if we need to do a refresh attack
			if (target != null)
			{
				if ((now - lastAttackTime) > REFRESH_ATTACK)
				{
					Log.Debug("Refresh Attack");
					lastAttackTime = now;
					AttackTarget();
				}
			}
			// If we don't have a target, see if we need to do a refresh follow
			else
			{
				if ((now - lastFollowTime) > REFRESH_FOLLOW)
				{
					Log.Debug("Refresh Follow");
					lastFollowTime = now;
					Follow();
				}
			}

			// This method returns true if it completed with a valid target
			return (target != null);
		}

		private void LoadAttributes()
		{
			animalClass = critterPed.Model.GetAnimalClass();
			animalName = critterPed.Model.GetAnimalName();
			originalMaxHealth = critterPed.MaxHealth;
			originalRGroup = critterPed.RelationshipGroup;
			strenth = critterPed.Model.GetAnimalStrength();
		}
		#endregion // Internal Methods

		#region Public Methods
		/// <summary>
		/// Befriend the critter.
		/// </summary>
		/// <param name="group">
		/// The group the critter should join.
		/// </param>
		/// <param name="relationshipGroup">
		/// The relationship group the critter should join.
		/// </param>
		public void Befriend(int group, int relationshipGroup)
		{
			// Only proceed if not already a friend
			if (state != CritterState.NotFriend) { return; }

			// Now it's a friend
			state = CritterState.Following;

			// Shortcut to player
			var player = Game.Player.Character;

			// Turn on friendly blip
			if (critterPed.CurrentBlip == null)
			{
				critterPed.AddBlip();
			}
			critterPed.CurrentBlip.IsFriendly = true;
			critterPed.CurrentBlip.Scale = 0.7f;

			// Warn for unknown animal, otherwise notify what's found
			if (animalClass != AnimalClass.Unknown)
			{
				Log.Info(string.Format("Found {0}", animalName));
			}
			#if DEBUG
			else
			{
				Log.Debug(string.Format("Unknown Animal: {0}", critterPed.Model));
				critterPed.CurrentBlip.IsFlashing = true;
			}
			#endif

			// Make the critter run (or fly) to the player
			critterPed.TaskHurryToEntity(player);

			// If the critter is not allowed in our group, we're done
			if (!IsAllowedInGroup) { return; }

			// Give the critter super health
			critterPed.MaxHealth = SUPER_HEALTH;
			critterPed.Health = SUPER_HEALTH;

			// Set the critter into the relationship group
			critterPed.RelationshipGroup = relationshipGroup;

			// Set the critter into the group?
			if (group != 0)
			{
				critterPed.SetAsGroupMember(group);
			}

			// Make sure the critter will never leave the group (unless told to do so)
			critterPed.SetNeverLeavesGroup(true);

			// Allow switch weapons (and fight?)
			critterPed.CanSwitchWeapons = true;

			// Block non-temporary events
			Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, critterPed, true);

			// Critter never flees
			Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, critterPed, 0, 0);

			// Critter fights to the death
			Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, critterPed, 46, true);

			// Start following the player
			Follow();
		}

		/// <summary>
		/// Checks to see if the critter has a target and the target is still alive.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the critter has a target and the target is still alive; otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// If the current target has died <see cref="Target"/> will be set to <see langword="null"/> 
		/// and <see cref="State"/> will return to <see cref="CritterState.Following"/>.
		/// </remarks>
		public bool CheckOnTarget()
		{
			return InnerCheckOnTarget(false, FindTargetMethod.CombatingPlayer);
		}

		/// <summary>
		/// Checks to see if the critter has a live target and attempts to find a new one if not.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the critter has an existing valid target or found a new one; otherwise <c>false</c>.
		/// </returns>
		public bool CheckOnTarget(FindTargetMethod findNewMethod)
		{
			return InnerCheckOnTarget(true, findNewMethod);
		}

		/// <summary>
		/// Finds a target
		/// </summary>
		/// <param name="method">
		/// The method for finding the target.
		/// </param>
		/// <param name="replaceExisting">
		/// <c>true</c> if any current target should be abandoned; otherwise <c>false</c>.
		/// </param>
		/// <returns>
		/// <c>true</c> if critter ends up with a valid target.
		/// </returns>
		public bool FindTarget(FindTargetMethod method, bool replaceExisting = false)
		{
			// If not replacing and already have one, ignore
			if ((!replaceExisting) && (target != null)) { return true; }

			Log.Debug(string.Format("Searching for new target using {0}", method));

			// Shortcut to player
			var player = Game.Player.Character;

			// Placeholder
			Ped[] peds = null;

			// Which method?
			switch (method)
			{
				case FindTargetMethod.CombatingPlayer:
					// Get peds close to critter
					peds = World.GetNearbyPeds(critterPed, SEARCH_RANGE, SEARCH_COUNT);

					// Find first targeting player
					Target = peds.Where(p => p.IsInCombatWith(player)).FirstOrDefault();
					break;

				case FindTargetMethod.PlayerIsTargeting:
					// Whatever the player is targeting
					Target = Game.Player.GetTargetedEntity() as Ped;
					break;

				case FindTargetMethod.HatesPlayer:
					// Get peds close to critter
					peds = World.GetNearbyPeds(critterPed, SEARCH_RANGE, SEARCH_COUNT);

					// Find first that hates player
					Target = peds.Where(p => p.GetRelationshipWithPed(player) == Relationship.Hate).FirstOrDefault();
					break;

				case FindTargetMethod.ClosestPlayer:
					// Get peds close to critter
					peds = World.GetNearbyPeds(critterPed, SEARCH_RANGE, SEARCH_COUNT);

					// Find first that doesn't respect the player (friend) and isn't a bird (since we don't let them join the group)
					Target = peds.Where(p => (p.GetRelationshipWithPed(player) != Relationship.Respect) && (p.Model.GetAnimalClass() != Critters.AnimalClass.Flying)).FirstOrDefault();
					break;
				default:
					Log.Debug(string.Format("Unknown Target Method {0}", method));
					Target = null;
					break;
			}

			// Did we find a target?
			bool found = (target != null);
			if (found)
			{
				Log.Debug("Critter found a target");
			}
			else
			{
				Log.Debug("Critter could not find a target");
			}
			return found;
		}

		/// <summary>
		/// Unfriends the critter, causing him to leave the group
		/// </summary>
		public void Unfriend()
		{
			// If not a friend, ignore
			if (state == CritterState.NotFriend) { return; }

			// Set as no longer friend
			state = CritterState.NotFriend;

			// Remove from group
			critterPed.RemoveFromGroup();

			// Set back to original relationship
			critterPed.RelationshipGroup = originalRGroup;
			
			// Remove the target
			target = null;

			// Turn off blip
			if (critterPed.CurrentBlip != null)
			{
				critterPed.CurrentBlip.Remove();
			}

			// Reset the health
			critterPed.Health = Math.Min(critterPed.Health, originalMaxHealth);
			critterPed.MaxHealth = originalMaxHealth;

			// Mark it as no longer needed
			critterPed.MarkAsNoLongerNeeded();
		}
		#endregion // Public Methods

		#region Public Properties
		public AnimalClass AnimalClass
		{
			get { return animalClass; }
		}

		public string AnimalName
		{
			get { return animalName; }
		}

		public bool HasTarget
		{
			get
			{
				return (target != null);
			}
		}

		/// <summary>
		/// Gets a value that indicates if the critter is alive.
		/// </summary>
		/// <remarks>
		/// <see cref="Ped.IsAlive"/> it seems is unreliable with animals.
		/// </remarks>
		public bool IsAlive
		{
			get
			{
				// It's not a bird and it has health
				if ((animalClass != Critters.AnimalClass.Flying) && (critterPed.Health > 0)) { return true; }

				// It's a bird and the system thinks it's alive
				return critterPed.IsAlive;
			}
		}

		/// <summary>
		/// Gets a value that indicates if the critter is allowed to join the players group.
		/// </summary>
		public bool IsAllowedInGroup
		{
			get
			{
				return strenth >= 20;  // animalClass != Critters.AnimalClass.Flying;
			}
		}

		public bool IsFlyingAnimal
		{
			get
			{
				return animalClass == Critters.AnimalClass.Flying;
			}
		}

		public bool IsSwimmingAnimal
		{
			get
			{
				return animalClass == Critters.AnimalClass.Swimming;
			}
		}

		public Ped Ped
		{
			get { return critterPed; }
		}

		public CritterState State
		{
			get { return state; }
		}

		public int Strenth
		{
			get { return strenth; }
		}

		public Ped Target
		{
			get { return target; }
			set
			{
				// Make sure changing
				if (value != target)
				{
					// Store
					target = value; 

					// If target, attack; otherwise follow.
					if (target != null)
					{
						AttackTarget();
					}
					else
					{
						Follow();
					}
				}
			}
		}
		#endregion // Public Properties
	}
}
