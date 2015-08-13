using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;

namespace Critters
{
	static public class Audio
	{
		static public int GetSoundID()
		{
			return Function.Call<int>(Hash.GET_SOUND_ID);
		}

		static public bool HasSoundFinished(int id)
		{
			return Function.Call<bool>(Hash.HAS_SOUND_FINISHED, id);
		}

		static public void PlaySoundFromEntity(string sound, Entity entity, string set)
		{
			Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, sound, entity, set, 0, 0);
		}

		static public void PlaySoundFromEntity(string sound, Entity entity)
		{
			Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, sound, entity, 0, 0, 0);
		}

		static public void PlaySoundFrontEnd(string sound, string set)
		{
			/*
			Function.Call(Hash.REQUEST_S::REQUEST_ANIM_DICT, animSet);

			const System::DateTime endtime = System::DateTime::Now + System::TimeSpan(0, 0, 0, 0, 1000);

			while (!Native::Function::Call<bool>(Native::Hash::HAS_ANIM_DICT_LOADED, animSet))
			{
				Script::Yield();

				if (System::DateTime::Now >= endtime)
				{
					return;
				}
			}
			*/
			Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, sound, set, 0);
		}

		static public void ReleaseSoundId(int id)
		{
			Function.Call(Hash.RELEASE_SOUND_ID, id);
		}
	}
}
