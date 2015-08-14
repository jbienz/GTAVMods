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
	/// <summary>
	/// Methods for finding a target
	/// </summary>
	public enum FindTargetMethod
	{
		/// <summary>
		/// Any ped that is fighting with the player
		/// </summary>
		/// <remarks>
		/// This has the effect of deffending the player.
		/// </remarks>
		CombatingPlayer,
		
		/// <summary>
		/// Any ped that the player is targeting.
		/// </summary>
		/// <remarks>
		/// This has the effect of going on the offense but only when the player starts it.
		/// </remarks>
		PlayerIsTargeting,

		/// <summary>
		/// Any ped that hates the player
		/// </summary>
		/// <remarks>
		/// This has the effect of going on the offense all the time.
		/// </remarks>
		HatesPlayer,

		/// <summary>
		/// Ped closest to the player regardless of relationship
		/// </summary>
		/// <remarks>
		/// This should be used one-shot.
		/// </remarks>
		ClosestPlayer
	}

	public class CritterManager : Updatable
	{
		#region Constants
		private const float AroundPlayerRange = 3f;
		private const string CritterEnemyRName = "CritterEnemy";
		private const int CritterHealth = 200;
		private const int PrimaryCritterCount = 8;
		private const float SeparationRange = 150f;
		private readonly Vector3 SeparationRange3D = new Vector3(SeparationRange, SeparationRange, SeparationRange);
		private readonly TimeSpan InvalidReleaseTime = TimeSpan.FromSeconds(10);
		private readonly TimeSpan CleanTime = TimeSpan.FromSeconds(5);
		private readonly TimeSpan TargetTime = TimeSpan.FromSeconds(3);
		#endregion // Constants

		#region Member Variables
		// Collections
		private List<int> critterIds;
		private List<Critter> primaryCritters;
		private List<Critter> secondaryCritters;
		private List<Ped> enemies;
		
		// Groups
		private int playerGroup;
		
		// Relationship Groups
		private int enemyRGroup;
		private int playerRGroup;
		
		// Misc
		private DateTime lastCleanTime = DateTime.Now;
		private DateTime lastCallTime = DateTime.Now;
		private DateTime lastTargetTime = DateTime.Now;
		private FindTargetMethod targetMethod = FindTargetMethod.CombatingPlayer;
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

		private void Cleanup(List<Critter> list, bool shouldReleaseInvalids, Ped player)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				// Get critter objects
				var critter = list[i];
				var critterPed = critter.Ped;

				// If it's a bird that needs to be released, or it's dead, or it's too far away, unfriend it
				if ((!critter.IsAllowedInGroup && shouldReleaseInvalids) || (!critter.IsAlive) || (!critterPed.IsNearEntity(player, SeparationRange3D)))
				{
					// Died?
					if (!critter.IsAlive)
					{
						Log.Info("A critter has died :(");
					}

					// Unfriend
					critter.Unfriend();

					// Remove from lists
					critterIds.Remove(critter.Ped.Handle);
					list.RemoveAt(i);

					// If this is the primary list, see if we need to promote
					if (list == primaryCritters)
					{
						// Find the most powerful critter
						var strongest = secondaryCritters.OrderBy(c => c.Strenth).FirstOrDefault();

						// If we found one, promote it
						if (strongest != null)
						{
							Log.Debug(string.Format("Space available, promoting {0}", strongest.AnimalName));
							Promote(strongest);
						}
					}
				}
				#if DEBUG
				else
				{
					var msg = string.Format("Re: {0} He: {1} G: {2} A: {3} H: {4}", critterPed.GetRelationshipWithPed(player), critterPed.Health, critterPed.IsInGroup(), critter.IsAlive, critter.Ped.Model);
					Log.Debug(msg);
				}
				#endif
			}
		}

		private void Cleanup(bool shouldReleaseInvalids)
		{
			Log.Debug("Clean: " + shouldReleaseInvalids);
			// Shortcut
			var player = Game.Player.Character;

			// Thread safe
			lock (primaryCritters)
			{
				lock (secondaryCritters)
				{
					Cleanup(primaryCritters, shouldReleaseInvalids, player);
					Cleanup(secondaryCritters, shouldReleaseInvalids, player);
				}
			}
		}

		private void CreateCollections()
		{
			if (critterIds == null)
			{
				critterIds = new List<int>();
			}
			if (primaryCritters == null)
			{
				primaryCritters = new List<Critter>();
			}
			if (secondaryCritters == null)
			{
				secondaryCritters = new List<Critter>();
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

			// Get the player group
			playerGroup = Game.Player.GetPlayerGroup();

			// Get the player relationship group
			playerRGroup = player.RelationshipGroup;

			// Create the enemy relationship group
			enemyRGroup = World.AddRelationshipGroup(CritterEnemyRName);
			
			// Set the relationship between both groups (hint: it's not good)
			World.SetRelationshipBetweenGroups(Relationship.Hate, enemyRGroup, playerRGroup);
			World.SetRelationshipBetweenGroups(Relationship.Hate, playerRGroup, enemyRGroup);
		}
		
		private void Demote(Critter weaker)
		{
			// Remove the weaker critter from the primary group
			weaker.Ped.RemoveFromGroup();

			// Reassign relationship group
			weaker.Ped.RelationshipGroup = playerRGroup;

			// Remove from primary collection
			primaryCritters.Remove(weaker);

			// Add to secondary collection
			secondaryCritters.Add(weaker);
		}

		private void FindTargets()
		{
			// Thread safe
			lock (primaryCritters)
			{
				for (int i=0; i < primaryCritters.Count; i++)
				{
					// Check on targets and find new
					primaryCritters[i].CheckOnTarget(targetMethod);
				}
			}
			lock (secondaryCritters)
			{
				for (int i = 0; i < secondaryCritters.Count; i++)
				{
					// Check on targets and find new
					secondaryCritters[i].CheckOnTarget(targetMethod);
				}
			}
		}

		private void Promote(Critter critter)
		{
			// Add the critter to the primary group
			critter.Ped.SetAsGroupMember(playerGroup);

			// Reassign relationship group
			critter.Ped.RelationshipGroup = playerRGroup;

			// Remove from secondary collection
			secondaryCritters.Remove(critter);

			// Add to primary collection
			primaryCritters.Add(critter);
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
			if (activeTicks % 60 != 0) { return; }

			// Get current time
			var now = DateTime.Now;

			// Time to cleanup?
			if ((now - lastCleanTime) > CleanTime)
			{
				// Update last clean time
				lastCleanTime = now;

				// Have we passed invalid release time?
				var shouldReleaseInvalids = ((now - lastCallTime) >= InvalidReleaseTime);

				// Clean
				Cleanup(shouldReleaseInvalids);
			}

			// Time to retarget?
			if ((now - lastTargetTime) > TargetTime)
			{
				// Update last target time
				lastTargetTime = now;

				// Target
				FindTargets();
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

			// Lock master lists
			lock (primaryCritters)
			{
				lock (secondaryCritters)
				{
					// Befriend each critter
					foreach (var critterPed in newCritters)
					{
						// If we already know about it, skip
						if (critterIds.Contains(critterPed.Handle))
						{
							// Log.Debug("Skipping known critter");
							continue;
						}

						// Now we know about it
						critterIds.Add(critterPed.Handle);

						// Create critter object
						var critter = new Critter(critterPed);

						// Default to secondary collection and group
						List<Critter> addList = secondaryCritters;
						int addGroup = 0;

						Log.Debug(string.Format("Critters in Group P: {0} S: {1}", primaryCritters.Count, secondaryCritters.Count));

						// If primary group isn't full and critters is allowed in group, switch to primary
						if ((primaryCritters.Count < PrimaryCritterCount) && (critter.IsAllowedInGroup))
						{
							Log.Debug("Room available in priamry");
							addList = primaryCritters;
							addGroup = playerGroup;
						}
						else
						{
							// If critter is allowed in primary group at this point in time then we don't have enough slots
							// See if we can demote find a weaker one
							if (critter.IsAllowedInGroup)
							{
								// Find weaker critter in primary list
								var weaker = primaryCritters.Where(c => c.Strenth < critter.Strenth).FirstOrDefault();

								// If we found a weaker critter, demote it.
								if (weaker != null)
								{
									// Notify of demotion
									Log.Debug(string.Format("{0} being demoted for {1}", weaker.AnimalName, critter.AnimalName));

									// Demote the critter
									Demote(weaker);

									// Switch to primary
									addList = primaryCritters;
									addGroup = playerGroup;
								}
							}
							else
							{
								Log.Debug(string.Format("Critter not allowed in primary group: {0}", critter.AnimalName));
							}
						}

						// Add to target list
						addList.Add(critter);

						// Befriend the critter
						critter.Befriend(addGroup, playerRGroup);
					}
				}
			}
		}

		public void MakeEnemy(Ped ped)
		{
			// Notify player
			Log.Debug(string.Format("Hating on {0}", ped.GetPedType()));

			// Set ped into enemy relationship group
			ped.RelationshipGroup = enemyRGroup;
		}
		#endregion // Public Methods
	}
}
