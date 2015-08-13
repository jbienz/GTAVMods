using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Critters
{
	/// <summary>
	/// The base class for an entity that can be updated.
	/// </summary>
	public class Updatable
	{
		#region Member Variables
		private Dictionary<int, Action> actionQueue = new Dictionary<int, Action>();
		private int activeTicks;
		private int inactiveTicks;
		private bool isActive = true;
		private readonly int startTick;
		private int ticks;
		#endregion // Member Variables

		#region Constructors
		protected Updatable(int startTick = 0)
		{
			ticks = startTick;
			this.startTick = startTick;
		}
		#endregion // Constructors

		#region Overrides / Event Handlers
		/// <summary>
		/// Called on every update when the entity is active.
		/// </summary>
		/// <param name="activeTicks">
		/// The number of active ticks for this entity.
		/// </param>
		/// <param name="totalTicks">
		/// The total number of ticks for this entity.
		/// </param>
		protected virtual void OnActiveUpdate(int activeTicks, int totalTicks) { }

		/// <summary>
		/// Called once whenever an entity becomes active.
		/// </summary>
		/// <param name="totalTicks">
		/// The total number of ticks for this entity.
		/// </param>
		protected virtual void OnFirstActiveUpdate(int totalTicks) { }

		/// <summary>
		/// Called once whenever an entity becomes inactive.
		/// </summary>
		/// <param name="totalTicks">
		/// The total number of ticks for this entity.
		/// </param>
		protected virtual void OnFirstInactiveUpdate(int totalTicks) { }

		/// <summary>
		/// Called on the very first update for the entity.
		/// </summary>
		protected virtual void OnFirstUpdate() { }

		/// <summary>
		/// Called on every update when the entity is inactive.
		/// </summary>
		/// <param name="inactiveTicks">
		/// The number of inactive ticks for this entity.
		/// </param>
		/// <param name="totalTicks">
		/// The total number of ticks for this entity.
		/// </param>
		protected virtual void OnInactiveUpdate(int inactiveTicks, int totalTicks) { }

		/// <summary>
		/// Called on every update whether the entity is active or not.
		/// </summary>
		/// <param name="totalTicks">
		/// The total number of ticks for this entity.
		/// </param>
		protected virtual void OnUpdate(int totalTicks) { }
		#endregion // Overrides / Event Handlers

		#region Public Methods
		/// <summary>
		/// Performs an update on the entity, adding another tick.
		/// </summary>
		public void Update()
		{
			OnUpdate(ticks);

			if (actionQueue.ContainsKey(ticks))
			{
				actionQueue[ticks].Invoke();
			}

			if (ticks == 0 || ticks == startTick)
			{
				OnFirstUpdate();
			}

			if (isActive)
			{
				inactiveTicks = 0;

				if (activeTicks == 0)
				{
					OnFirstActiveUpdate(ticks);
				}

				OnActiveUpdate(activeTicks, ticks);

				activeTicks++;
			}
			else
			{
				activeTicks = 0;

				if (inactiveTicks == 0)
				{
					OnFirstInactiveUpdate(ticks);
				}

				OnInactiveUpdate(inactiveTicks, ticks);

				inactiveTicks++;
			}

			ticks++;
		}
		#endregion // Public Methods

		#region Public Properties
		/// <summary>
		/// Gets or sets the action queue for the entity.
		/// </summary>
		public Dictionary<int, Action> ActionQueue
		{
			get
			{
				return actionQueue;
			}
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				actionQueue = value;
			}
		}

		/// <summary>
		/// If active, returns how many times it has been ticked during its session.
		/// </summary>
		public int ActiveTicks { get { return activeTicks; } }

		/// <summary>
		/// If inactive, returns how many times it has been ticked during its session.
		/// </summary>
		public int InactiveTicks { get { return inactiveTicks; } }

		/// <summary>
		/// Gets or sets a value that indicates whether this entity is active.
		/// </summary>
		public bool IsActive
		{
			get { return isActive; }
			set { isActive = value; }
		}

		/// <summary>
		/// Returns the tick at which updating started.
		/// </summary>
		/// <remarks>
		/// This isn't necessarily 0 since StartTick can be specified in the constructor.
		/// </remarks>
		public int StartTick { get { return startTick; } }

		/// <summary>
		/// Returns how many times this Updating instance has ticked.
		/// </summary>
		public int Ticks { get { return ticks; } }
		#endregion // Public Properties
	}
}
