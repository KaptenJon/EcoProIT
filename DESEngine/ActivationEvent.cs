//=============================================================================
//=  $Id: ActivationEvent.cs 180 2006-10-14 16:42:34Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2004, Eric K. Roe.  All rights reserved.
//=
//=  React.NET is free software; you can redistribute it and/or modify it
//=  under the terms of the GNU General Public License as published by the
//=  Free Software Foundation; either version 2 of the License, or (at your
//=  option) any later version.
//=
//=  React.NET is distributed in the hope that it will be useful, but WITHOUT
//=  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
//=  FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
//=  more details.
//=
//=  You should have received a copy of the GNU General Public License along
//=  with React.NET; if not, write to the Free Software Foundation, Inc.,
//=  51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//=============================================================================

using System;

namespace React
{
	/// <summary>
	/// Callback used to retrieve event data on a deferred (delayed) basis.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When an <see cref="ActivationEvent"/>'s
	/// <see cref="ActivationEvent.Data"/> property is set to a
	/// <see cref="DeferredDataCallback"/>, the delegate will be
	/// called at the time the event is fired by the
	/// <see cref="Simulation"/>.  The object returned by the delegate will
	/// then serve as the new value for the <see cref="ActivationEvent.Data"/>
	/// property.
	/// </para>
	/// <para>
	/// The callback should always check the
	/// <see cref="ActivationEvent.IsPending"/> property before it allocates
	/// any data to the <see cref="ActivationEvent"/>.  If
	/// <see cref="ActivationEvent.IsPending"/> is <b>true</b>, then the data
	/// should be allocated; otherwise it should not be allocated, and any
	/// steps needed to un-reserve or deallocate data already allocated on
	/// behalf of the requesting <see cref="ActivationEvent"/> should be taken.
	/// </para>
	/// </remarks>
	/// <param name="evt">
	/// The <see cref="ActivationEvent"/> that is requesting the data.
	/// </param>
	/// <returns>
	/// The event data object or <see langword="null"/> if no data is available
	/// for the event.
	/// </returns>
	public delegate object DeferredDataCallback(ActivationEvent evt);

	/// <summary>
	/// Object used by a <see cref="Simulation"/> to schedule and run
	/// <see cref="DesTask"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="ActivationEvent"/> is part of the React.NET tasking system.  Each
	/// <see cref="ActivationEvent"/> specifies <em>when</em> a particular
	/// <see cref="DesTask"/> should be executed.  Each <see cref="DesTask"/>, on
	/// the other hand, describes <em>what</em> happens.
	/// </para>
	/// <para>
	/// Normally <see cref="DesTask"/> and <see cref="Process"/> implementors will
	/// not need to work directly with <see cref="ActivationEvent"/>s. Instead,
	/// they can simply use the various <b>Activate</b> methods of the
	/// <see cref="DesTask"/> class, which will create and schedule an
	/// <see cref="ActivationEvent"/> on behalf of the <see cref="DesTask"/>.
	/// </para>
	/// </remarks>
	public class ActivationEvent
	{
		/// <summary>
		/// Time which indicates the event is not scheduled.
		/// </summary>
		public const ulong NotScheduled = 0L;

		/// <summary>
		/// The time the event is scheduled to occur.
		/// </summary>
		private ulong _evtTime = NotScheduled;
		/// <summary>
		/// Client-specific event data.
		/// </summary>
		private object _evtData;
		/// <summary>
		/// Flag indicating whether or not the event is canceled.
		/// </summary>
		private bool _cancelFlag;

		/// <summary>The <see cref="DesTask"/> to execute.</summary>
		private DesTask _desTask;
		/// <summary>
		/// The priority of <see cref="_desTask"/> at the time this event was created.
		/// </summary>
		private int _priority = TaskPriority.Normal;
		/// <summary>The object that activated <see cref="_desTask"/>.</summary>
		private object _activator;

		/// <summary>
		/// Creates a new <see cref="ActivationEvent"/> that will run the specified
		/// <see cref="DesTask"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="desTask"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> this <see cref="ActivationEvent"/> will run
		/// when it is fired.
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current simulation time when the
		/// <see cref="ActivationEvent"/> should be fired.
		/// </param>
		public ActivationEvent(DesTask desTask, ulong relTime)
			: this(desTask, null, relTime)
		{
		}

		/// <summary>
		/// Create a new <see cref="ActivationEvent"/> that will run the
		/// specified <see cref="DesTask"/> on behalf of the given activator.
		/// </summary>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> this <see cref="ActivationEvent"/> will run
		/// when it is fired.
		/// </param>
		/// <param name="activator">
		/// The object which is activating <paramref name="desTask"/>.
		/// </param>
		/// <param name="relTime">
		/// The relative time when <paramref name="desTask"/> should be executed.
		/// </param>
		public ActivationEvent(DesTask desTask, object activator, ulong relTime)
		{
			if (desTask == null)
				throw new ArgumentNullException("desTask");
			if (desTask.Canceled)
				throw new ArgumentException("Cannot be canceled.", "desTask");
			if (relTime == 0)
			{
				;
			}
			_evtTime = desTask.Simulation.Now + relTime;
			_desTask = desTask;
			_activator = activator;
			_priority = desTask.Priority;
		}

		/// <summary>
		/// Gets the <see cref="DesTask"/> this event will execute.
		/// </summary>
		/// <value>
		/// The <see cref="DesTask"/> to execute.
		/// </value>
		public DesTask DesTask
		{
			get {return _desTask;}
		}

		/// <summary>
		/// Gets the object that is activating the <see cref="DesTask"/>
		/// associated with this <see cref="ActivationEvent"/>.
		/// </summary>
		/// <value>
		/// The DesTask activator as an <see cref="object"/> or
		/// <see langword="null"/> if the activation is anonymous (i.e. it
		/// did not specify an activator).
		/// </value>
		public object Activator
		{
			get {return _activator;}
		}

		/// <summary>
		/// Gets the simulation time the event should occur.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Note that the event will not actually occur unless it is first
		/// scheduled with the simulation using the
		/// <see cref="Simulation.ScheduleEvent"/> method.
		/// </para>
		/// <para>
		/// <see cref="ActivationEvent"/>s with lower <see cref="Time"/> values
		/// always get fired before those with greater <see cref="Time"/>s.
		/// For <see cref="ActivationEvent"/> occurring at the same
		/// <see cref="Time"/>, their <see cref="Priority"/> values are used to
		/// determine which <see cref="ActivationEvent"/> should be fired
		/// first.
		/// </para>
		/// </remarks>
		/// <value>
		/// The simulation time the event should occur as an
		/// <see cref="ulong"/>.
		/// </value>
		public ulong Time
		{
			get { return _evtTime; }
			set { _evtTime = value; }
		}

		/// <summary>
		/// Gets the <see cref="ActivationEvent"/>'s priority.
		/// </summary>
		/// <remarks>
		/// <para>
		/// For <see cref="ActivationEvent"/>s scheduled to occur at the same
		/// <see cref="Time"/>, the <see cref="Priority"/> can be used as a
		/// tie-breaker with those <see cref="ActivationEvent"/>s having higher
		/// priorities getting fired before those of lower priorities.
		/// </para>
		/// <para>
		/// <see cref="Priority"/> is not used to compare
		/// <see cref="ActivationEvent"/>s occurring at different times.  In
		/// those cases, the earlier <see cref="ActivationEvent"/> always takes
		/// place before the later <see cref="ActivationEvent"/>.
		/// </para>
		/// </remarks>
		/// <value>
		/// The priority as an <see cref="int"/>.
		/// </value>
		public int Priority
		{
			get {return _priority;}
		}

		/// <summary>
		/// Gets whether the <see cref="ActivationEvent"/> is pending.
		/// </summary>
		/// <remarks>
		/// This property will be <b>true</b> if the event is currently
		/// pending (waiting to be fired).  It is false if:
		/// <list type="bullet">
		///     <item><description>
		///         The <see cref="ActivationEvent"/> was never scheduled with
		///         a <see cref="Simulation"/>; or
		///     </description></item>
		///     <item><description>
		///         It was cancelled via the <see cref="Cancel"/> method; or
		///     </description></item>
		///     <item><description>
		///         Its associated <see cref="DesTask"/> was cancelled; or
		///     </description></item>
		///     <item><description>
		///         It has already been fired.
		///     </description></item>
		/// </list>
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="ActivationEvent"/> is pending.
		/// </value>
		public bool IsPending
		{
			get { return _evtTime >= 0L && !_cancelFlag && !DesTask.Canceled; }
		}
		/// <summary>
		/// Gets any optional data associated with this
		/// <see cref="ActivationEvent"/>.
		/// </summary>
		/// <value>
		/// The event data or <see langword="null"/> if there is no optional
		/// data.
		/// </value>
		public virtual object Data
		{
			get { return _evtData; }
			set { _evtData = value; }
		}

		/// <summary>
		/// Cancels the <see cref="ActivationEvent"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// After cancelling the <see cref="ActivationEvent"/>,
		/// <see cref="IsPending"/> will be <b>false</b> and the event will
		/// not be executed (fired) by the <see cref="Simulation"/>.
		/// </para>
		/// <para>
		/// Note that cancelling an <see cref="ActivationEvent"/> does not cancel
		/// its associated <see cref="DesTask"/>.
		/// </para>
		/// </remarks>
		public void Cancel()
		{
			_cancelFlag = true;
			DesTask.CancelPending(this);
			PrepareDeferredData();
		}

		/// <summary>
		/// Invoked by the <see cref="Simulation"/> to fire the
		/// <see cref="ActivationEvent"/>.
		/// </summary>
		/// <remarks>
		/// Only a <see cref="Simulation"/> should invoke this method and then
		/// only after scheduling the <see cref="ActivationEvent"/>.
		/// </remarks>
		/// <param name="sim">
		/// The <see cref="Simulation"/> firing the
		/// <see cref="ActivationEvent"/>.
		/// </param>
		internal void Fire(Simulation sim)
		{
			if (!IsPending)
			{
				throw new InvalidOperationException("Event was not pending.");
			}

			// Obtained the deferred data.  This call must come before the
			// call to base.Fire(sim), otherwise the DeferredDataCallback
			// will treat this event as canceled.
			PrepareDeferredData();

			_evtTime = NotScheduled;
			DesTask.RunFromActivationEvent(this);
		}

		/// <summary>
		/// Obtains the deferred activation data if any.
		/// </summary>
		/// <remarks>
		/// After this method is invoked, the <see cref="Data"/> property
		/// will reflect the data obtained from the
		/// <see cref="DeferredDataCallback"/> delegate.
		/// </remarks>
		private void PrepareDeferredData()
		{
			DeferredDataCallback callback = Data as DeferredDataCallback;
			if (callback != null)
			{
				Data = callback(this);
			}
		}
	}
}
