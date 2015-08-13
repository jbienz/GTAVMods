using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	public class CritterManager : Updatable
	{
		#region Constants
		private const float AroundPlayerRange = 3f;
		private const string CritterEnemyRName = "CritterEnemy";
		private const int CritterHealth = 200;
		private const float SeparationRange = 150f;
		private readonly Vector3 SeparationRange3D = new Vector3(SeparationRange, SeparationRange, SeparationRange);
		private readonly TimeSpan BirdReleaseTime = TimeSpan.FromSeconds(10);
		#endregion // Constants

		#region Member Variables
		// Collections
		private List<Ped> critters;
		private List<Ped> enemies;
		
		// Groups
		private int playerGroup;
		
		// Relationship Groups
		private int enemyRGroup;
		private int playerRGroup;
		
		// Misc
		private DateTime lastCleanTime = DateTime.Now;
		private DateTime lastCallTime = DateTime.Now;
		#endregion // Member Variables

		#region Internal Methods
		/*
				var pos = player.Position.Around( 5f );
				var ped = World.CreatePed( PedHash.Beach01AFM, pos );
				var seq = new TaskSequence();
				seq.AddTask.GoTo( player.Position );
				seq.AddTask.PlayAnimation( "missheist_agency3aig_18", "say_hurry_up_a", 8f, 3000, true, 8f );
				seq.AddTask.GoTo( pos );
				seq.Close();
				ped.Task.PerformSequence( seq ); 
			 */

		private void BefriendCritter(Ped critter)
		{
			// Shortcut to player
			var player = Game.Player.Character;

			// Get the animal class
			var animalClass = critter.Model.GetAnimalClass();
			bool isBird = (animalClass == AnimalClass.Flying);
			if (animalClass == AnimalClass.Unknown)
			{
				UI.Notify(string.Format("Unknown Animal: {0}", critter.Model));
				critter.CurrentBlip.IsFlashing = true;
			}
			else
			{
				UI.Notify(string.Format("Animal Class: {0}", animalClass));
			}

			// Turn on friendly blip
			if (critter.CurrentBlip == null)
			{
				critter.AddBlip();
			}
			critter.CurrentBlip.IsFriendly = true;
			critter.CurrentBlip.Scale = 0.7f;

			// Make the critter run (or fly) to the player
			critter.TaskHurryToEntity(player);

			// Only add to collection once
			if (!critters.Contains(critter)) { critters.Add(critter); }

			// We don't add birds to the actual group because they can't fight
			if (isBird) { return; }

			// Super health the critter
			critter.MaxHealth = CritterHealth;
			critter.Health = CritterHealth;

			// Set the critter into the players relationship grip
			critter.RelationshipGroup = playerRGroup;

			// Set the critter into players group
			critter.SetAsGroupMember(playerGroup);

			// Make sure the critter will never leave the group (unless told to do so)
			critter.SetNeverLeavesGroup(true);

			// Allow switch weapons (and fight?)
			critter.CanSwitchWeapons = true;

			// Block critter non-temporary events
			Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, critter, true);

			// Critter never flees
			Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, critter, 0, 0);

			// Critter fights to the death
			Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, critter, 46, true);
			// Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, critter, 17, true);

			// Critter fights hated targets
			critter.Task.FightAgainstHatedTargets(50000f);
			
			// Critter never stops fighting
			critter.AlwaysKeepTask = true;
		}

		private void Cleanup()
		{
			// Get current time
			var now = DateTime.Now;

			// Only do this every 3 seconds
			if ((now - lastCleanTime) < TimeSpan.FromSeconds(3))
			{
				return;
			}

			// Update last clean time
			lastCleanTime = now;

			// Have we passed bird release time?
			var shouldReleaseBirds = ((now - lastCallTime) >= BirdReleaseTime);

			// Shortcut
			var player = Game.Player.Character;

			// Thread safe
			lock (critters)
			{
				for (int i = critters.Count - 1; i >= 0; i--)
				{
					// Get critter object
					var critter = critters[i];

					// Is it a bird?
					bool isBird = (critter.Model.GetAnimalClass() == AnimalClass.Flying);
					
					// If bird, dead or too far away, let it go
					if ((isBird && shouldReleaseBirds) || (!isBird && critter.Health < 1) || (!critter.IsNearEntity(player, SeparationRange3D)))
					{
						if (critter.CurrentBlip != null)
						{
							critter.CurrentBlip.Remove();
						}
						critter.RemoveFromGroup();
						critters.RemoveAt(i);
						critter.MarkAsNoLongerNeeded();
					}
					else
					{
						var msg = string.Format("Re: {0} He: {1} G: {2} A: {3} D: {4}", critter.GetRelationshipWithPed(player), critter.Health, critter.IsInGroup(), critter.IsAlive, critter.IsDead);
						UI.Notify(msg);
					}
				}
			}
		}

		private void CreateCollections()
		{
			if (critters == null)
			{
				critters = new List<Ped>();
			}
			if (enemies == null)
			{
				enemies = new List<Ped>();
			}
		}

		private void CreateGroupsAndRelationships()
		{
			// Shortcut to player character
			var player = Game.Player.Character;
			UI.Notify("Before GetPlayerGroup");
			// Get the player group
			playerGroup = Game.Player.GetPlayerGroup();

			UI.Notify("Before Cougar");
			// Get the player relationship group
			// playerRGroup = player.RelationshipGroup;
			int playerRGroup = Function.Call<int>(Hash.GET_HASH_KEY, "COUGAR");
			player.RelationshipGroup = playerRGroup;
			UI.Notify(string.Format("Cougar Group: {0}", playerRGroup));

			// Create the enemy relationship group
			enemyRGroup = World.AddRelationshipGroup(CritterEnemyRName);
			
			//// Set the relationship between both groups (hint: it's not good)
			World.SetRelationshipBetweenGroups(Relationship.Hate, enemyRGroup, playerRGroup);
			World.SetRelationshipBetweenGroups(Relationship.Hate, playerRGroup, enemyRGroup);
		}
		#endregion // Internal Methods

		#region Overrides / Event Handlers
		protected override void OnActiveUpdate(int activeTicks, int totalTicks)
		{
			base.OnActiveUpdate(activeTicks, totalTicks);

			// See if player is targeting a ped
			var target = Game.Player.GetTargetedEntity() as Ped;

			// If we have a target and it's not already an enemy, make it one
			if ((target != null) && (target.RelationshipGroup != enemyRGroup))
			{
				MakeEnemy(target);
			}

			// Ignore if not at least 60 ticks
			if (activeTicks % 60 == 0)
			{
				Cleanup();
			}
		}

		protected override void OnFirstActiveUpdate(int totalTicks)
		{
			// Call base first
			base.OnFirstActiveUpdate(totalTicks);

			// Create collections
			CreateCollections();

			// Create groups and relationships
			CreateGroupsAndRelationships();
		}
		#endregion // Overrides / Event Handlers

		#region Public Methods
		/// <summary>
		/// Finds nearby critters and makes them friends
		/// </summary>
		public void FindCritters()
		{
			// Update last call time
			lastCallTime = DateTime.Now;

			// Shortcut
			var player = Game.Player.Character;

			// Play call animation
			player.Task.PlayAnimation("facials@p_m_zero@variations@elkcall", "mood_elkcal_1");

			// Play call sound
			Audio.PlaySoundFromEntity("Franklin_Whistle_For_Chop", player, "SPEECH_RELATED_SOUNDS");

			// Find nearby animals that aren't dead
			List<Ped> newCritters = null;
			try
			{
				newCritters = World.GetNearbyPeds(player, SeparationRange).Where(p => p.GetPedType() == PedType.Animal && !p.IsDead).ToList();
				// newCritters = World.GetNearbyPeds(player, SeparationRange).Where(p => !p.IsDead).ToList();
			}
			catch (Exception) { }

			// If we didn't find any just notify and bail
			if ((newCritters == null) || (newCritters.Count < 1))
			{
				UI.ShowSubtitle("No critters found. :(");
			}

			// Found some! make them our friends.
			var msg = string.Format("Found {0} critters!", newCritters.Count);
			UI.ShowSubtitle(msg);

			// Lock master list
			lock (critters)
			{
				// Befriend each critter
				foreach (var critter in newCritters)
				{
					BefriendCritter(critter);
				}
			}
		}

		public void MakeEnemy(Ped ped)
		{
			// Notify player
			UI.Notify(string.Format("Making an enemy out of {0}", ped.GetPedType()));

			// Set ped into enemy relationship group
			ped.RelationshipGroup = enemyRGroup;
		}
		#endregion // Public Methods
	}
}
