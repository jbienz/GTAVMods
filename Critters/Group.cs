using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	public enum GroupFormation
	{
		Default = 0,
		CircleAroundLeader = 1,
		AlternativeCircleAroundLeader = 2,
		LineWithLeaderAtCenter = 3,
	}

	/// <summary>
	/// Represents a Ped group.
	/// </summary>
	public class Group
	{
		private int handle;

		#region Constructors
		/// <summary>
		/// Initializes a new group.
		/// </summary>
		/// <param name="handle">
		/// The group handle.
		/// </param>
		public Group(int handle)
		{
			this.handle = handle;
		}
		#endregion // Constructors

		#region Public Methods
		public void ClearRelationship(Relationship relationship, Group other)
		{
			// Must be called once for each group
			Function.Call(Hash.CLEAR_RELATIONSHIP_BETWEEN_GROUPS, (int)relationship, handle, other.handle);
			Function.Call(Hash.CLEAR_RELATIONSHIP_BETWEEN_GROUPS, (int)relationship, other.handle, handle);
		}

		public Relationship GetRelationship(Group other)
		{
			return (Relationship)Function.Call<int>(Hash.GET_RELATIONSHIP_BETWEEN_GROUPS, handle, other.handle);
		}

		public void SetFormation(GroupFormation formation)
		{
			Function.Call(Hash.SET_GROUP_FORMATION, handle, (int)formation);
		}

		public void SetRelationship(Relationship relationship, Group other)
		{
			// Must be called once for each group
			Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, (int)relationship, handle, other.handle);
			Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, (int)relationship, other.handle, handle);
		}

		public void SetSeparationRange(float separationRange)
		{
			Function.Call(Hash.SET_GROUP_SEPARATION_RANGE, handle, separationRange);
		}
		#endregion // Public Methods

		#region Public Properties
		/// <summary>
		/// The native handle of the group.
		/// </summary>
		public int Handle { get { return handle; } }
		#endregion // Public Properties
	}
}
