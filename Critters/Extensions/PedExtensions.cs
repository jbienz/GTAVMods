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
	/// Types of peds
	/// </summary>
	public enum PedType
	{
		Michael = 0,
		Franklin = 1,
		Trevor = 2,
		Male = 4,
		Female = 5 ,
		Cop = 6,
		Paramedic = 20,
		LSFD = 21,
		Human = 26,
		SWAT = 27,
		Animal = 28,
		Army = 29,
	}
	
	/// <summary>
	/// Class of animals
	/// </summary>
	public enum AnimalClass
	{
		Unknown, 
		Flying,		// Birds
		Farm,		// Cows, Pigs, Hens, etc.
		Pet,		// Dogs and Cats
		Swimming,	// Fish, Skars, Whales, etc.
		Wild,		// Boars, Chimps, Deer, Mountain Lions, Rats, etc.
	}

	static public class PedExtensions
	{
		/*
		static public Ped GetRandomPedAtCoord(Vector3 position, Vector3 radius, PedType pedType)
		{
			return Function.Call<Ped>(Hash.GET_RANDOM_PED_AT_COORD, position.X, position.Y, position.Z, 200, 200, 200, 26);
			//if (handle != 0)
			//{
			//	return new Ped(handle);
			//}
			//else
			//{
			//	return null;
			//}
		}
		 */

		//static public void FlyTo(this Ped ped, Vector3 position, float speed)
		//{
		//	Function.Call(Hash.TASK_FOLLOW_NAV_MESH_TO_COORD, ped.Handle, position.X, position.Y, position.Z, 4.0f, -1, 0.0f, 0, 0.0f);
		//}

		//static public void FlyTo(this Ped ped, Vector3 position)
		//{
		//	FlyTo(ped, position, 10f);
		//}


		static public PedType GetPedType(this Ped ped)
		{
			return (PedType)Function.Call<int>(Hash.GET_PED_TYPE, ped);
		}

		static public AnimalClass GetAnimalClass(this Model model)
		{
			switch ((uint)model.Hash)
			{
				case 0xCE5FF074: // Boar
				case 0xA8683715: // Chimp
				case 0x644AC75E: // Coyote
				case 0xD86B5A95: // Deer
				case 0x1250D7BA: // MountainLion
				case 0xC3B52966: // Rat
				case 0xC2D06F53: // Rhesus
				case 0xDFB55C81: // Rabits
					return AnimalClass.Wild;
				case 0x573201B8: // Cat
				case 0x14EC17EA: // Chop
				case 0x9563221D: // Chop Clone
				case 0x4E8F95A2: // Husky
				case 0x349F33E1: // Retriever
				case 0x431FC24C: // Shepherd
					return AnimalClass.Pet;
				case 0xAAB71F62: // ChickenHawk
				case 0x56E29962: // Cormorant
				case 0x18012A9F: // Crow
				case 0x6A20728:  // Pigeon
				case 0xD3939DFD: // Seagull
					return AnimalClass.Flying;
				case 0xFCFA9E1E: // Cow
				case 0x6AF51FAF: // Hen
				case 0xB11BAB56: // Pig
					return AnimalClass.Farm;
				case 0x8BBAB455: // Dolphin
				case 0x2FD800B7: // Fish
				case 0x3C831724: // HammerShark
				case 0x471BE4B2: // Humpback
				case 0x8D8AC8B9: // KillerWhale
				case 0x6C3F072:  // TigerShark
					return AnimalClass.Swimming;
				default:
					return AnimalClass.Unknown;
			}
		}

		static public int GetRelationship(this Ped ped, Ped other)
		{
			return Function.Call<int>(Hash.GET_RELATIONSHIP_BETWEEN_PEDS, ped, other);
		}

		static public bool IsGroupMember(this Ped ped, int groupId)
		{
			return Function.Call<bool>(Hash.IS_PED_GROUP_MEMBER, ped, groupId);
		}

		static public bool IsInGroup(this Ped ped)
		{
			return Function.Call<bool>(Hash.IS_PED_IN_GROUP, ped);
		}

		static public void RemoveFromGroup(this Ped ped)
		{
			Function.Call(Hash.REMOVE_PED_FROM_GROUP, ped);
		}

		static public void SetAsGroupLeader(this Ped ped, int groupId)
		{
			Function.Call(Hash.SET_PED_AS_GROUP_LEADER, ped, groupId);
		}

		static public void SetAsGroupMember(this Ped ped, int groupId)
		{
			Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped, groupId);
		}

		static public void SetNeverLeavesGroup(this Ped ped, bool neverLeaves)
		{
			Function.Call(Hash.SET_PED_NEVER_LEAVES_GROUP, ped, neverLeaves);
		}
	}
}
