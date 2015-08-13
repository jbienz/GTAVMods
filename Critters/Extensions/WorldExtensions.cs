using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	static public class WorldExtensions
	{
		///// <summary>
		///// Creates a new ped group.
		///// </summary>
		///// <returns>
		///// The new group if created; otherwise <see langword="null"/>.
		///// </returns>
		///// <remarks>
		///// Groups can contain up to 8 peds.
		///// </remarks>
		//static public Group CreateGroup()
		//{
		//	var handle = Function.Call<int>(Hash.CREATE_GROUP, 0);
		//	return (handle == 0 ? null : new Group(handle));
		//}

		///// <summary>
		///// Removes a ped group.
		///// </summary>
		//static public void RemoveGroup(Group group)
		//{
		//	Function.Call(Hash.REMOVE_GROUP, group.Handle);
		//}
	}
}
