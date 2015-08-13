using GTA;
using GTA.Native;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;

namespace Critters
{
	/// <remarks>
	/// <seealso href="https://github.com/crosire/scripthookvdotnet/wiki/Getting-Started">Getting Started</seealso> 
	/// <seealso href="https://github.com/crosire/scripthookvdotnet/wiki/How-Tos">How Tos</seealso> 
	/// <seealso href="https://github.com/crosire/scripthookvdotnet/wiki/Code-Snippets">Code Snippets</seealso> 
	/// <seealso href="http://www.gta5-mystery-busters.onet.domains/tools/anims.php">List of Animations</seealso> 
	/// <seealso href="http://www.dev-c.com/nativedb/">Native DB</seealso>
	/// </remarks>
	public class CrittersMod : Script
	{
		private CritterManager critterManager;
		public CrittersMod()
		{
			critterManager = new CritterManager();
			this.KeyUp += OnKeyUp;
			this.Tick += CrittersMod_Tick;
		}

		private void SpawnVehicle()
		{
			Vehicle vehicle = World.CreateVehicle(VehicleHash.Adder, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 3.0f, Game.Player.Character.Heading + 90);
			vehicle.CanTiresBurst = false;
			vehicle.CustomPrimaryColor = Color.FromArgb(38, 38, 38);
			vehicle.CustomSecondaryColor = Color.DarkOrange;
			vehicle.PlaceOnGround();
			vehicle.NumberPlate = "SHVDN";
		}

		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Alt)
			{
				switch (e.KeyCode)
				{
					case Keys.S:
						SpawnVehicle();
						break;
					case Keys.K:
						critterManager.FindCritters();
						break;
				}
			}
		}

		private void CrittersMod_Tick(object sender, EventArgs e)
		{
			critterManager.Update();
		}
	}



	#region Notes
	/* Working */
	// Audio.PlaySoundFromEntity("Franklin_Whistle_For_Chop", Game.Player.Character, "SPEECH_RELATED_SOUNDS");
	// Audio.PlaySoundFrontEnd("WEAPON_AMMO_PURCHASE", "HUD_AMMO_SHOP_SOUNDSET");
	// Audio.PlaySoundFrontEnd("PIN_BUTTON", "ATM_SOUNDS");
	// Audio.PlaySoundFromEntity("PIN_BUTTON", Game.Player.Character, "ATM_SOUNDS");

	/* Not Working*/
	// Game.PlaySound("hunting_2_elk_calls", "SCRIPT");
	// Audio.PlaySoundFromEntity("PLAYER_CALLS_ELK_MASTER", Game.Player.Character, null);
	// Audio.PlaySoundFromEntity("unlocked_bleep", Game.Player.Character, "HACKING_DOOR_UNLOCK_SOUNDS");
	// Audio.PlaySoundFromEntity("Birds", Game.Player.Character, "ARM_1_SOUNDSET");

	// missheist_agency3aig_18 -> say_hurry_up_a

	// facials@p_m_zero@variations@elkcall -> mood_elkcal_1
	// Game.Player.Character.Task.PlayAnimation("missheist_agency3aig_18", "say_hurry_up_a", DEFAULT_SPEED, DEFAULT_DURATION, true, DEFAULT_RATE);

	// MANAGED CALL: PlayAnimation, animSet, animName, speed, duration, lastAnimation, playbackRate);
	// NATIVE CALL: TASK_PLAY_ANIM, this->mPed->Handle, animSet, animName, speed, -8.0f, duration, lastAnimation, playbackRate, 0, 0, 0);
	#endregion // Notes
}
