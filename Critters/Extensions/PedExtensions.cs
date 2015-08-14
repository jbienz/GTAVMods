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

		static public int GetRelationship(this Ped ped, Ped other)
		{
			return Function.Call<int>(Hash.GET_RELATIONSHIP_BETWEEN_PEDS, ped, other);
		}

		static public bool IsGroupMember(this Ped ped, int groupId)
		{
			return Function.Call<bool>(Hash.IS_PED_GROUP_MEMBER, ped, groupId);
		}

		static public bool IsInCombatWith(this Ped ped, Ped target)
		{
			return Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped, target);
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

		static public void SetCanBeTargetedByPlayer(this Ped ped, bool canBeTargeted)
		{
			Function.Call(Hash.SET_PED_CAN_BE_TARGETTED_BY_PLAYER, ped, canBeTargeted);
		}

		static public void SetNeverLeavesGroup(this Ped ped, bool neverLeaves)
		{
			Function.Call(Hash.SET_PED_NEVER_LEAVES_GROUP, ped, neverLeaves);
		}
	}
}
