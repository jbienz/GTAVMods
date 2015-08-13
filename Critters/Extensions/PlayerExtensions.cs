using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	static public class PlayerExtensions
	{
		static public Group GetPlayerGroup(this Player player)
		{
			var groupId = Function.Call<int>(Hash.GET_PLAYER_GROUP, player.Handle);
			return new Group(groupId);
		}

	}
}
