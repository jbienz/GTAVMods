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
		static public int GetPlayerGroup(this Player player)
		{
			return Function.Call<int>(Hash.GET_PLAYER_GROUP, player);
		}

	}
}
