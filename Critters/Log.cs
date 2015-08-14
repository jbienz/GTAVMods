using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	static public class Log
	{
		static public void Debug(string msg)
		{
			#if DEBUG
			UI.Notify(msg);
			#endif
		}

		static public void Info(string msg)
		{
			UI.Notify(msg);
		}

		static public void Warn(string msg)
		{
			UI.Notify(msg);
		}
	}
}
