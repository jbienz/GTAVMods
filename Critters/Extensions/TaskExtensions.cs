using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	static public class TaskExtensions
	{
		static public void PlayAnimation(this Tasks task, string animSet, string animName)
		{
			task.PlayAnimation(animSet, animName, Constants.SPEED_DEFAULT, Constants.DURATION_DEFAULT, false, Constants.RATE_DEFAULT);
		}
	}
}
