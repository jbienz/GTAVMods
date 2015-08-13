using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	static public class EntityExtensions
	{
		static public void TaskGoToEntity(this Entity entity, Entity target, float distance, float speed)
		{
			Function.Call(Hash.TASK_GO_TO_ENTITY, entity.Handle, target.Handle, -1, distance, speed, 1073741824, 0);
		}

		static public void TaskGoToEntity(this Entity entity, Entity target)
		{
			TaskGoToEntity(entity, target, 3.0f, 1.0f);
		}
		static public void TaskHurryToEntity(this Entity entity, Entity target)
		{
			TaskGoToEntity(entity, target, 3.0f, 4.0f);
		}

	}
}
