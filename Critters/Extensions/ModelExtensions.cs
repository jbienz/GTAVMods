using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	/// <summary>
	/// Classes of animals
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


	static public class ModelExtensions
	{
		static public AnimalClass GetAnimalClass(this Model model)
		{
			switch ((uint)model.Hash)
			{
				case AnimalHash.Boar:
				case AnimalHash.Chimp:
				case AnimalHash.Coyote:
				case AnimalHash.Deer:
				case AnimalHash.MountainLion:
				case AnimalHash.Rat:
				case AnimalHash.Rhesus:
				case AnimalHash.Rabit:
					return AnimalClass.Wild;
				case AnimalHash.Cat:
				case AnimalHash.Chop:
				case AnimalHash.ChopClone:
				case AnimalHash.Husky:
				case AnimalHash.Retriever:
				case AnimalHash.Shepherd:
					return AnimalClass.Pet;
				case AnimalHash.ChickenHawk:
				case AnimalHash.Cormorant:
				case AnimalHash.Crow:
				case AnimalHash.Pigeon:
				case AnimalHash.Seagull:
					return AnimalClass.Flying;
				case AnimalHash.Cow:
				case AnimalHash.Hen:
				case AnimalHash.Pig:
					return AnimalClass.Farm;
				case AnimalHash.Dolphin:
				case AnimalHash.Fish:
				case AnimalHash.HammerShark:
				case AnimalHash.Humpback:
				case AnimalHash.KillerWhale:
				case AnimalHash.TigerShark:
					return AnimalClass.Swimming;
				default:
					return AnimalClass.Unknown;
			}
		}

		static public string GetAnimalName(this Model model)
		{
			switch ((uint)model.Hash)
			{
				case AnimalHash.Boar:
					return "Boar";
				case AnimalHash.Cat:
					return "Cat";
				case AnimalHash.Chimp:
					return "Chimp";
				case AnimalHash.Chop:
				case AnimalHash.ChopClone:
					return "Rottweiler";
				case AnimalHash.ChickenHawk:
					return "Chicken Hawk";
				case AnimalHash.Cormorant:
					return "Cormorant";
				case AnimalHash.Cow:
					return "Cow";
				case AnimalHash.Coyote:
					return "Coyote";
				case AnimalHash.Crow:
					return "Crow";
				case AnimalHash.Deer:
					return "Deer";
				case AnimalHash.Dolphin:
					return "Dolphin";
				case AnimalHash.Fish:
					return "Fish";
				case AnimalHash.HammerShark:
					return "Hammer Shark";
				case AnimalHash.Hen:
					return "Hen";
				case AnimalHash.Humpback:
					return "Humpback Whale";
				case AnimalHash.Husky:
					return "Husky";
				case AnimalHash.KillerWhale:
					return "Killer Whale";
				case AnimalHash.MountainLion:
					return "Mountain Lion";
				case AnimalHash.Pig:
					return "Pig";
				case AnimalHash.Pigeon:
					return "Pigeon";
				case AnimalHash.Rabit:
					return "Rabit";
				case AnimalHash.Rat:
					return "Rat";
				case AnimalHash.Retriever:
					return "Retriever";
				case AnimalHash.Rhesus:
					return "Rhesus";
				case AnimalHash.Seagull:
					return "Seagull";
				case AnimalHash.Shepherd:
					return "Shepherd";
				case AnimalHash.TigerShark:
					return "Tiger Shark";
				default:
					return "Unknown";
			}
		}

		static public int GetAnimalStrength(this Model model)
		{
			switch ((uint)model.Hash)
			{
				case AnimalHash.Chop:
				case AnimalHash.ChopClone:
					return 100;

				case AnimalHash.MountainLion:
					return 95;

				case AnimalHash.Humpback:
				case AnimalHash.KillerWhale:
					return 90;

				case AnimalHash.TigerShark:
				case AnimalHash.HammerShark:
					return 85;

				case AnimalHash.Boar:
				case AnimalHash.Coyote:
					return 70;

				case AnimalHash.Deer:
					return 60;

				case AnimalHash.Cow:
					return 50;

				case AnimalHash.Chimp:
				case AnimalHash.Rhesus:
					return 40;

				case AnimalHash.Husky:
				case AnimalHash.Retriever:
				case AnimalHash.Shepherd:
					return 35;

				case AnimalHash.Pig:
					return 30;

				case AnimalHash.Dolphin:
					return 28;

				case AnimalHash.Cat:
					return 25;

				case AnimalHash.Hen:
					return 20;

				case AnimalHash.ChickenHawk:
				case AnimalHash.Cormorant:
				case AnimalHash.Crow:
					return 15;

				case AnimalHash.Rabit:
					return 10;

				case AnimalHash.Pigeon:
				case AnimalHash.Seagull:
					return 8;

				case AnimalHash.Rat:
				case AnimalHash.Fish:
					return 5;

				default:
					return 23; // If unknown, treat slightly less than cat
			}
		}
	}
}
