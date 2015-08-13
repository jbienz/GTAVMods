using GTA;
using GTA.Math;
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
		private const float SeparationRange = 150f;
		private readonly Vector3 SeparationRange3D = new Vector3(SeparationRange, SeparationRange, SeparationRange);
		#endregion // Constants

		#region Member Variables
		private List<Ped> critters;
		// private Group critterGroup;
		private List<Ped> enemies;
		private Group enemyGroup;
		private DateTime lastCleanTime = DateTime.Now;
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
			// Only add to critters list once
			if (critters.Contains(critter)) { return; }

			// Add to critters list
			critters.Add(critter);

			// Shortcut to player
			var player = Game.Player.Character;

			// Turn on friendly blip
			if (critter.CurrentBlip == null)
			{
				critter.AddBlip();
			}
			critter.CurrentBlip.IsFriendly = true;

			// Get the animal class
			var animalClass = critter.Model.GetAnimalClass();
			if (animalClass == AnimalClass.Unknown)
			{
				UI.Notify(string.Format("Unknown Animal: {0}", critter.Model));
				critter.CurrentBlip.IsFlashing = true;
			}
			else
			{
				UI.Notify(string.Format("Animal Class: {0}", animalClass));
			}


			// Get the player group
			var playerGroup = Game.Player.GetPlayerGroup();

			UI.Notify("Player In Group: " + playerGroup.Handle);

				
			// Add critter to player group
			critter.SetAsGroupMember(playerGroup);

			// Find a place close to the player for the critter to run / land
			var posNear = player.Position.Around(AroundPlayerRange);

			// critter.Weapons.Give(GTA.Native.WeaponHash.Pistol, 100, true, true);
			critter.TaskHurryToEntity(player);
			critter.Task.FightAgainstHatedTargets(5000f);
			critter.AlwaysKeepTask = true;
			/*
			// The following needs to be done as a task sequence
			var seq = new TaskSequence();
			seq.AddTask.RunTo(posNear);
			seq.AddTask.FightAgainstHatedTargets(5000f);
			seq.Close();
			critter.AlwaysKeepTask = true;
			critter.Task.PerformSequence(seq);
			 */
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

			// Long enough
			lastCleanTime = now;

			// Shortcut
			var player = Game.Player.Character;
			// UI.Notify("Player Group: ")

			// Thread safe
			lock (critters)
			{
				for (int i = critters.Count - 1; i >= 0; i--)
				{
					// Get critter object
					var critter = critters[i];

					// If dead or too far away, let it go
					if ((critter.Health < 1) || (!critter.IsNearEntity(player, SeparationRange3D)))
					{
						if (critter.CurrentBlip != null)
						{
							critter.CurrentBlip.Remove();
						}
						critter.RemoveFromGroup();
						critters.RemoveAt(i);
					}
					else
					{
						var msg = string.Format("Re: {0} He: {1} G: {2} A: {3} D: {4}", critter.GetRelationshipWithPed(player), critter.Health, critter.IsInGroup(), critter.IsAlive, critter.IsDead);
						UI.Notify(msg);
					}
				}
			}
		}
		#endregion // Internal Methods

		#region Overrides / Event Handlers
		protected override void OnActiveUpdate(int activeTicks, int totalTicks)
		{
			base.OnActiveUpdate(activeTicks, totalTicks);

			// Ignore if not at least 60 ticks
			if (activeTicks % 60 == 0)
			{
				Cleanup();
			}
		}

		protected override void OnFirstActiveUpdate(int totalTicks)
		{
			base.OnFirstActiveUpdate(totalTicks);

			// Create collections and groups
			if (critters == null)
			{
				critters = new List<Ped>();
			}
			if (enemies == null)
			{
				enemies = new List<Ped>();
			}
			if (enemyGroup == null)
			{
				enemyGroup = WorldExtensions.CreateGroup();
			}
		}
		#endregion // Overrides / Event Handlers

		#region Public Methods
		/// <summary>
		/// Finds nearby critters and makes them friends
		/// </summary>
		public void FindCritters()
		{
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
		#endregion // Public Methods
	}
}
