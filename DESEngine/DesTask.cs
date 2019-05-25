//=============================================================================
//=  $Id: DesTask.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Collections.Generic;
using React.Queue;

namespace React
{
	/// <summary> 
	/// An object that carries out some processing during the course of
	/// running a <see cref="Simulation"/>.
	/// <seealso cref="Process"/>
	/// </summary>
	public abstract class DesTask : Blocking<DesTask> //IComparable<DesTask>, IComparable
	{
		/// <summary>
		/// Schedule time returned when the <see cref="DesTask"/> is not
		/// scheduled to execute.
		/// </summary>
		/// <remarks>
		/// This value is identical to
		/// <see cref="ActivationEvent.NotScheduled"/>.
		/// </remarks>
		public const ulong NotScheduled = ActivationEvent.NotScheduled;

		/// <summary>
		/// The simulation context under which the <see cref="DesTask"/> runs.
		/// </summary>
		private Simulation _sim;
		/// <summary>
		/// The wait queue used to block <see cref="DesTask"/>s.
		/// </summary>
		private IQueue<DesTask> _waitQ;
		/// <summary>
		/// The DesTask instances upon which this DesTask is blocked.
		/// </summary>
		private IList<DesTask> _blockedOn;
		/// <summary>
		/// The DesTask priority.
		/// </summary>
		private int _priority = TaskPriority.Normal;
		/// <summary>
		/// The temporary elevated DesTask priority.
		/// </summary>
		private int _elevated = TaskPriority.Normal;
		/// <summary>
		/// Flag indicating the DesTask has been canceled.
		/// </summary>
		private bool _cancelFlag;
		/// <summary>
		/// Flag indicating the DesTask has been interrupted.
		/// </summary>
		private bool _intFlag;
		/// <summary>
		/// The <see cref="ActivationEvent"/> which invoked the DesTask.
		/// </summary>
		private ActivationEvent _actevt;

		/// <summary>
		/// Create a new <see cref="DesTask"/> instance that will run under under
		/// the given simulation context.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="sim"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="sim">The simulation context.</param>
		protected DesTask( Simulation sim)
		{
			if (sim == null)
			{
				throw new ArgumentNullException("sim");
			}

			this._sim = sim;
		}

		/// <summary>
		/// Gets the simulation context under which the <see cref="DesTask"/> is
		/// running.
		/// </summary>
		/// <value>
		/// The simulation context as a <see cref="Simulation"/>.
		/// </value>
		public Simulation Simulation
		{
			get {return _sim;}
		}

		/// <summary>
		/// Gets the current simulation time.
		/// </summary>
		/// <remarks>
		/// This is really just a shortcut for <c>DesTask.Simulation.Now</c>.
		/// </remarks>
		/// <value>
		/// The current simulation time as an <see cref="ulong"/>.
		/// </value>
		public ulong Now
		{
			get {return _sim.Now;}
		}

		/// <summary>
		/// Gets whether or not the <see cref="DesTask"/> has been scheduled to
		/// run.
		/// </summary>
		/// <remarks>
		/// A <see cref="DesTask"/> that has been activated using one of the
		/// <b>Activate</b> methods will be scheduled to run.  Therefore after
		/// calling <b>Activate</b>, <see cref="IsScheduled"/> should always
		/// return <b>true</b>.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="DesTask"/> has been scheduled.
		/// </value>
		public bool IsScheduled
		{
			get {return _actevt != null && _actevt.IsPending;}
		}

		/// <summary>
		/// Gets the time the <see cref="DesTask"/> is scheduled to run.
		/// </summary>
		/// <remarks>
		/// If the <see cref="DesTask"/> is not scheduled to run, this
		/// property will be <see cref="DesTask.NotScheduled"/>.
		/// </remarks>
		/// <value>
		/// The simulation time the <see cref="DesTask"/> will run as an
		/// <see cref="ulong"/>.
		/// </value>
		public ulong ScheduledTime
		{
			get 
			{
				if (IsScheduled)
					return _actevt.Time;
				else
					return DesTask.NotScheduled;
			}
		}

		/// <summary>
		/// Gets whether or not the <see cref="DesTask"/> is blocked (that is,
		/// waiting on other <see cref="DesTask"/>s)
		/// </summary>
		/// <remarks>
		/// <para>
		/// Immediately after a call to one of the <b>Activate</b> methods,
		/// this property will normally be <b>false</b> as <b>Activate</b>
		/// invokes <see cref="ClearBlocks"/>.  Subsequent calls to
		/// <see cref="WaitOnTask(DesTask)"/> or <see cref="WaitOnTask(DesTask,int)"/>
		/// will cause this property to be <b>true</b>.
		/// </para>
		/// <para>
		/// Remember <see cref="IsBlocked"/> is used to check if this
		/// <see cref="DesTask"/> is waiting on other <see cref="DesTask"/>s
		/// <b>not</b> to check if this <see cref="DesTask"/> is blocking other
		/// <see cref="DesTask"/>s (e.g. other <see cref="DesTask"/>s are waiting on
		/// this <see cref="DesTask"/>).
		/// </para>
		/// </remarks>
		/// <value>
		/// <b>true</b> if this <see cref="DesTask"/> is blocking on one or more
		/// <see cref="DesTask"/>s.
		/// </value>
		public bool IsBlocked
		{
			get { return _blockedOn != null && _blockedOn.Count > 0; }
		}

		/// <summary>
		/// Gets the <see cref="IQueue&lt;DesTask&gt;"/> that contains all the
		/// <see cref="DesTask"/>s which are blocking on this <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// The wait queue is created on demand when this property is first
		/// accessed. The <see cref="Blocking&lt;DesTask&gt;.CreateBlockingQueue"/>
		/// method is used to create the wait queue.
		/// </remarks>
		/// <value>
		/// The <see cref="IQueue&lt;DesTask&gt;"/> that contains the
		/// <see cref="DesTask"/>s blocking on this <see cref="DesTask"/>.
		/// </value>
		protected IQueue<DesTask> WaitQueue
		{
			get
			{
				if (_waitQ == null)
					_waitQ = CreateBlockingQueue(DefaultQueue);
				return _waitQ;
			}
		}

		/// <summary>
		/// Get the number of <see cref="DesTask"/> instances blocked on the
		/// specified wait queue.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// If <paramref name="queueId"/> is not a valid queue identifier.
		/// </exception>
		/// <param name="queueId">The queue identifier.</param>
		/// <returns>
		/// The number of <see cref="DesTask"/> instances blocked on the
		/// queue identified by <paramref name="queueId"/>.
		/// </returns>
		public override int GetBlockCount(int queueId)
		{
			if (queueId == DefaultQueue || queueId == AllQueues)
				return _waitQ != null ? _waitQ.Count : 0;

			throw new ArgumentException("Invalid queue id: " + queueId);
		}

		/// <summary>
		/// Gets the <see cref="DesTask"/> instances blocking on the
		/// wait queue identified by a queue id.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// If <paramref name="queueId"/> is not a valid queue identifier.
		/// </exception>
		/// <param name="queueId">The queue identifier.</param>
		/// <returns>
		/// An array of <see cref="DesTask"/> instances that are currently
		/// contained in the wait queue identified by
		/// <paramref name="queueId"/>.  The returned array will never
		/// by <see langword="null"/>.
		/// </returns>
		public override DesTask[] GetBlockedTasks(int queueId)
		{
			if (queueId == DefaultQueue || queueId == AllQueues)
				return GetBlockedTasks(_waitQ);

			throw new ArgumentException("Invalid queue id: " + queueId);
		}

		/// <summary>
		/// Clear the interrupt state.
		/// </summary>
		/// <remarks>
		/// The <see cref="DesTask"/> will automatically invoke this method after
		/// it's <see cref="ExecuteTask"/> method runs.
		/// </remarks>
		public void ClearInterrupt()
		{
			_intFlag = false;
		}

		/// <summary>
		/// Temporarily elevate the <see cref="DesTask"/>'s priority.
		/// </summary>
		/// <remarks>
		/// This method can both elevate (raise) the priority or reduce (lower)
		/// the priority.  If <paramref name="newPriority"/> is greater than
		/// <see cref="Priority"/>, the DesTask prioritiy is raised; if
		/// <paramref name="newPriority"/> is lower than
		/// <see cref="Priority"/>, the DesTask prioritiy is lowered.
		/// </remarks>
		/// <param name="newPriority">
		/// The new DesTask priority.
		/// </param>
		public void ElevatePriority(int newPriority)
		{
			_elevated = newPriority;
		}

		/// <summary>
		/// Restores the <see cref="DesTask"/>'s priority to its non-elevated
		/// level.
		/// </summary>
		/// <returns>
		/// The non-elevated DesTask priority.
		/// </returns>
		public int RestorePriority()
		{
			_elevated = _priority;
			return _priority;
		}

		/// <summary>
		/// Gets the current DesTask priority.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the priority was elevated using <see cref="ElevatePriority"/>,
		/// then <see cref="Priority"/> will return the elevated DesTask priority.
		/// The only way to get the DesTask's default (non-elevated) priority is
		/// as follows.
		/// </para>
		/// <para>
		/// <code>
		/// // Get the current (possibly elevated) priority.
		/// int currpriority = DesTask.Priority;
		/// // Restore the default priority which also returns the default priority.
		/// int defpriority = DesTask.RestorePriority();
		/// // Return the priority to it's possibly elevated level.
		/// DesTask.ElevatePriority(currpriority);</code></para>
		/// </remarks>
		/// <value>
		/// The current DesTask priority as an <see cref="int"/>.
		/// </value>
		public int Priority
		{
			get {return _elevated;}
		}

		/// <summary>
		/// Wait the given <see cref="DesTask"/> while it executes.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <paramref name="desTask"/> must not already be scheduled because this
		/// method will invoke its <see cref="DesTask.Activate(object, ulong)"/>
		/// method.
		/// </para>
		/// <para>
		/// The method is simply shorthand for
		/// <code>
		/// DesTask.Activate(this, 0L);
		/// DesTask.Block(this);</code>
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="desTask"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> to wait upon while it runs.
		/// </param>
		public void WaitOnTask(DesTask desTask)
		{
			if (desTask == null)
				throw new ArgumentNullException("desTask", "cannot be null");

			desTask.Activate(this, 0L);
			desTask.Block(this);
		}

		/// <summary>
		/// Wait the given <see cref="DesTask"/> while it executes at the
		/// specified priority.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <paramref name="desTask"/> must not already be scheduled because this
		/// method will invoke its <see cref="DesTask.Activate(object, ulong, int)"/>
		/// method.
		/// </para>
		/// <para>
		/// The method is simply shorthand for
		/// <code>
		/// DesTask.Activate(this, 0L, priority);
		/// DesTask.Block(this);</code>
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="desTask"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> to wait upon while it runs.
		/// </param>
		/// <param name="priority">
		/// The priority to activate <paramref name="desTask"/>.
		/// </param>
		public void WaitOnTask(DesTask desTask, int priority)
		{
			if (desTask == null)
				throw new ArgumentNullException("desTask", "cannot be null");
			
			desTask.Activate(this, 0L, priority);
			desTask.Block(this);
		}

		/// <overloads>Activates (schedules) the DesTask to run.</overloads>
		/// <summary>
		/// Activates the <see cref="DesTask"/> at the current simulation time.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="DesTask"/>.  May be
		/// <see langword="null"/>
		/// </param>
		public void Activate(object activator)
		{
			Activate(activator, 0L, null, Priority);
		}

		/// <summary>
		/// Activates the <see cref="DesTask"/> at some time in the future.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="DesTask"/>.  May be
		/// <see langword="null"/>
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="DesTask"/>
		/// should be scheduled to run.  If this value is zero (0), this
		/// method is the same as <see cref="Activate(object)"/>.
		/// </param>
		public void Activate(object activator, ulong relTime)
		{
			Activate(activator, relTime, null, Priority);
		}

		/// <summary>
		/// Activates the <see cref="DesTask"/> at some time in the future and
		/// with the given priority.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="DesTask"/>.  May be
		/// <see langword="null"/>.
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="DesTask"/>
		/// should be scheduled to run. 
		/// </param>
		/// <param name="priority">
		/// The DesTask priority.  Higher values indicate higher priorities.
		/// </param>
		public void Activate(object activator, ulong relTime, int priority)
		{
			Activate(activator, relTime, null, priority);
		}

		/// <summary>
		/// Activates the <see cref="DesTask"/> at some time in the future and
		/// with the given client-specific data.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="DesTask"/>.  May be
		/// <see langword="null"/>
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="DesTask"/>
		/// should be scheduled to run.
		/// </param>
		/// <param name="data">
		/// An object containing client-specific data for the
		/// <see cref="DesTask"/>.
		/// </param>
		public void Activate(object activator, ulong relTime, object data)
		{
			Activate(activator, relTime, data, Priority);
		}

		/// <summary>
		/// Activates the <see cref="DesTask"/> at some time in the future and
		/// specifying the DesTask priority and client-specific DesTask data.
		/// </summary>
		/// <remarks>
		/// <see cref="DesTask"/> implementations can normally treat this method
		/// as the "designated" version of the <b>Activate</b> method, which
		/// all other versions of <b>Activate</b> invoke.  That, in fact, is
		/// how the <see cref="DesTask"/> class implements <b>Activate</b>.
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// If <see cref="Interrupted"/> is <b>true</b>.  Before calling
		/// this method, ensure that the <see cref="DesTask"/> is no longer
		/// in an interrupted state.
		/// </exception>
		/// <param name="activator">
		/// The object that is activating the <see cref="DesTask"/>.  May be
		/// <see langword="null"/>.
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="DesTask"/>
		/// should be scheduled to run.
		/// </param>
		/// <param name="data">
		/// An object containing client-specific data for the
		/// <see cref="DesTask"/>.
		/// </param>
		/// <param name="priority">
		/// The DesTask priority.  Higher values indicate higher priorities.
		/// </param>
		public virtual void Activate(object activator, ulong relTime,
									 object data, int priority)
		{
			if (Interrupted)
			{
				throw new InvalidOperationException(
					"DesTask is in an interrupted state.  Clear interrupt flag.");
			}
			else
			{
				CancelPending(_actevt);
				ClearBlocks();
				ElevatePriority(priority);
				ActivationEvent evt = new ActivationEvent(this, activator, relTime);
				evt.Data = data;
				Simulation.ScheduleEvent(evt);
				_actevt = evt;
			}
		}

		/// <summary>
		/// Resume the next waiting <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The next waiting <see cref="DesTask"/> is resumed with <c>this</c>
		/// as the activator and <see langword="null"/> for the activation
		/// data.
		/// </para>
		/// <para>
		/// Calling this method is identical to calling
		/// <code>ResumeNext(this, null);</code>
		/// </para>
		/// </remarks>
		protected void ResumeNext()
		{
			ResumeNext(this, null);
		}

		/// <summary>
		/// Resume the next waiting <see cref="DesTask"/> with the specified
		/// activation data.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The next waiting <see cref="DesTask"/> is resume with <c>this</c>
		/// as the activator.
		/// </para>
		/// <para>
		/// Calling this method is identical to calling
		/// <code>ResumeNext(this, data);</code>
		/// </para>
		/// </remarks>
		/// <param name="data">The activation data.</param>
		protected void ResumeNext(object data)
		{
			ResumeNext(this, data);
		}

		/// <summary>
		/// Resume the next waiting <see cref="DesTask"/> specifying the
		/// activator and activation data.
		/// </summary>
		/// <param name="activator">The activator.</param>
		/// <param name="data">The activation data.</param>
		protected virtual void ResumeNext(object activator, object data)
		{
			if (BlockCount > 0)
			{
				bool canceled;
				do
				{
					DesTask desTask = WaitQueue.Dequeue();
					canceled = desTask.Canceled;
					if (!canceled)
						ResumeTask(desTask, activator, data);
				} while (canceled && BlockCount > 0);
			}
		}

		/// <summary>
		/// Resume all waiting <see cref="DesTask"/>s.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All waiting <see cref="DesTask"/>s are resumed with <c>this</c> as the
		/// activator and <see langword="null"/>as the activation data.
		/// </para>
		/// <para>
		/// Calling this method is identical to calling
		/// <code>ResumeAll(this, null);</code>
		/// </para>
		/// </remarks>
		protected void ResumeAll()
		{
			ResumeAll(this, null);
		}

		/// <summary>
		/// Resume all waiting <see cref="DesTask"/>s passing each the specified
		/// activation data.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All waiting <see cref="DesTask"/>s are resumed with <c>this</c> as the
		/// activator.
		/// </para>
		/// <para>
		/// Calling this method is identical to calling
		/// <code>ResumeAll(this, data);</code>
		/// </para>
		/// </remarks>
		/// <param name="data">The activation data.</param>
		protected void ResumeAll(object data)
		{
			ResumeAll(this, data);
		}

		/// <summary>
		/// Resume all waiting <see cref="DesTask"/>s specifying the activator and
		/// the activation data.
		/// </summary>
		/// <param name="activator">The activator.</param>
		/// <param name="data">The activation data.</param>
		protected virtual void ResumeAll(object activator, object data)
		{
			int nWaiting = BlockCount;

			if (nWaiting > 1)
			{
				DesTask[] blockedDesTasks = new DesTask[nWaiting];
				WaitQueue.CopyTo(blockedDesTasks, 0);
				WaitQueue.Clear();
				for (int i = 0; i < nWaiting; i++)
				{
					DesTask desTask = blockedDesTasks[i];
					if (!desTask.Canceled)
						ResumeTask(desTask, activator, data);
				}
			}
			else if (nWaiting == 1)
			{
				ResumeNext(activator, data);
			}
		}

		/// <summary>
		/// Resume the specified <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is invoked for each blocked <see cref="DesTask"/> that is
		/// to be resumed by calling one of the <b>ResumeNext</b> or
		/// <b>ResumeAll</b> methods.  Subclasses may override this method to
		/// alter the way <paramref name="desTask"/> is activated.
		/// </para>
		/// <para>
		/// The default implementation simply performs
		/// </para>
		/// <para><code>DesTask.Activate(activator, 0L, data);</code></para>
		/// <para>
		/// Client code should normally never need to invoke this method
		/// directly.
		/// </para>
		/// <para>
		/// <b>By the time this method is called, <paramref name="desTask"/> has
		/// already been removed from <see cref="WaitQueue"/>.</b>
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="desTask"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> to resume (activate).
		/// </param>
		/// <param name="activator">
		/// The activator that will be passed to <paramref name="desTask"/> upon
		/// its activation.
		/// </param>
		/// <param name="data">
		/// Optional activation data passed to <paramref name="desTask"/>.
		/// </param>
		protected virtual void ResumeTask(DesTask desTask, object activator,
			object data)
		{
			if (desTask == null)
				throw new ArgumentNullException("desTask");
			desTask.Activate(activator, 0L, data);
		}

		/// <summary>
		/// Block the specified <see cref="DesTask"/> instance.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="desTask"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If <paramref name="desTask"/> attempts to block itself.  For
		/// example, if code like <c>this.Block(this);</c> is executed.
		/// </exception>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> to block.
		/// </param>
		public virtual void Block(DesTask desTask)
		{
			if (desTask == null)
			{
				throw new ArgumentNullException("'DesTask' cannot be null.");
			}
			if (desTask == this)
			{
				throw new ArgumentException("DesTask cannot block on itself.");
			}
			desTask.UpdateBlockingLinks(this, true);
			WaitQueue.Enqueue(desTask);
		}

		/// <summary>
		/// Unblock, but do not resume, the specified <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is used to remove <paramref name="desTask"/> from the
		/// <see cref="DesTask"/> instance's wait list without resuming the
		/// execution of <paramref name="desTask"/>.  The most common use for
		/// invoking <see cref="Unblock"/> is to stop a <see cref="DesTask"/>
		/// from waiting after it has been resumed by another means (e.g. a
		/// different simulation object has resumed <paramref name="desTask"/>).
		/// </para>
		/// <para>
		/// Again, it's very important to realize that <see cref="Unblock"/>
		/// does <b>not</b> activate <paramref name="desTask"/>.
		/// </para>
		/// <para>
		/// This method does nothing if <paramref name="desTask"/> equals
		/// <c>this</c> or is <see langword="null"/>.
		/// </para>
		/// </remarks>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> which will stop blocking on this
		/// <see cref="DesTask"/> instance.
		/// </param>
		public virtual void Unblock(DesTask desTask)
		{
			if (desTask != null && desTask != this && BlockCount > 0)
			{
				WaitQueue.Remove(desTask);
				desTask.UpdateBlockingLinks(this, false);
			}
		}

		/// <summary>
		/// Stop blocking on all <see cref="DesTask"/>s currently being blocked
		/// upon.
		/// </summary>
		protected virtual void ClearBlocks()
		{
			if (_blockedOn != null)
			{
				IList<DesTask> list = _blockedOn;
				_blockedOn = null;
				foreach (DesTask task in list)
				{
					task.Unblock(this);
				}
				if (_blockedOn == null)
				{
					list.Clear();
					_blockedOn = list;
				}
			}
		}

		/// <summary>
		/// Update the association between this <see cref="DesTask"/> and the
		/// <see cref="DesTask"/> upon which it is blocking.
		/// </summary>
		/// <param name="blocker">
		/// The <see cref="DesTask"/> upon which this <see cref="DesTask"/> is
		/// blocking.
		/// </param>
		/// <param name="blocked">
		/// <b>true</b> if <paramref name="blocker"/> is blocking this
		/// <see cref="DesTask"/>; or <b>false</b> if <paramref name="blocker"/>
		/// is unblocking this <see cref="DesTask"/>.
		/// </param>
		private void UpdateBlockingLinks(DesTask blocker, bool blocked)
		{
			if (blocked)
			{
				if (_blockedOn == null)
				{
					_blockedOn = new List<DesTask>();
				}
				if (_blockedOn.Contains(blocker))
				{
					throw new InvalidOperationException(
						"Already blocking on specified DesTask.");
				}
				_blockedOn.Add(blocker);
			}
			else if (_blockedOn != null)
			{
				_blockedOn.Remove(blocker);
			}
		}

		/// <summary>
		/// Cancel the <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A canceled DesTask will not be executed.  The associated
		/// <see cref="ActivationEvent"/> (if any) is also canceled.
		/// </para>
		/// <para>
		/// Callers should note that once a <see cref="DesTask"/> is canceled
		/// it cannot be un-canceled, and therefore can never be
		/// re-activated.
		/// </para>
		/// </remarks>
		public void Cancel()
		{
			_cancelFlag = true;
			CancelPending(_actevt);
		}

		/// <summary>
		/// Gets whether or not the <see cref="DesTask"/> was canceled.
		/// </summary>
		/// <remarks>
		/// This property will be <b>true</b> after the <see cref="Cancel"/>
		/// method is invoked.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="DesTask"/> was canceled.
		/// </value>
		public bool Canceled
		{
			get {return _cancelFlag;}
		}

		/// <summary>
		/// Interrupt a blocked <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When an blocked <see cref="DesTask"/> is interrupted, it should be
		/// activated at <see cref="React.Simulation.Now"/>.  When the
		/// <see cref="DesTask"/> resumes running, it can check the 
		/// <see cref="Interrupted"/> property to determine how to proceed.
		/// The <paramref name="interruptor"/> is available to the
		/// <see cref="DesTask"/> as the <em>activator</em> parameter when
		/// <see cref="ExecuteTask"/> is invoked.
		/// </para>
		/// <para>
		/// The <see cref="DesTask"/> must handle the interrupt and clear the
		/// interrupt flag by calling <see cref="ClearInterrupt"/> before
		/// <see cref="Interrupt"/> or
		/// <see cref="Activate(object,ulong,object,int)"/> (or any of the
		/// other <b>Activate</b> methods) may be called again.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If an <see cref="DesTask"/> attempts to interrupt itself.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="interruptor"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="interruptor">
		/// The object which caused the interrupt.  This should normally be
		/// the object that is invoking this method.
		/// </param>
		public void Interrupt(object interruptor)
		{
			if (interruptor == this)
			{
				throw new ArgumentException(
					"DesTask cannot interrupt itself.", "interruptor");
			}
			if (interruptor == null)
			{
				throw new ArgumentNullException("interruptor");
			}
			Activate(interruptor, 0L);
			_intFlag = true;
		}

		/// <summary>
		/// Gets whether or not the <see cref="DesTask"/> was interrupted.
		/// </summary>
		/// <remarks>
		/// This value is automatically reset to <b>false</b> after the
		/// <see cref="DesTask"/> executes.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="DesTask"/> was interrupted.
		/// </value>
		public bool Interrupted
		{
			get {return _intFlag;}
		}

		/// <summary>
		/// Perform the DesTask actions.
		/// </summary>
		/// <remarks>
		/// This method is invoked by the <see cref="DesTask"/>'s associated
		/// <see cref="ActivationEvent"/> when the
		/// <see cref="ActivationEvent"/> is fired.  Normally this method
		/// should not be called by client code.
		/// </remarks>
		/// <param name="activator">
		/// The object that activated this <see cref="DesTask"/>.
		/// </param>
		/// <param name="data">
		/// Optional data for the <see cref="DesTask"/>.
		/// </param>
		protected abstract void ExecuteTask(object activator, object data);

		//=====================================================================
		//====                  INTERNAL IMPLEMENTATION                    ====
		//=====================================================================

		/// <summary>
		/// Cancel the pending <see cref="ActivationEvent"/>.
		/// </summary>
		/// <param name="evt">
		/// The <see cref="ActivationEvent"/> to cancel.
		/// </param>
		internal void CancelPending(ActivationEvent evt)
		{
			if (_actevt != null)
			{
				// Internal consistency check
				if (_actevt != evt)
				{
					throw new InvalidOperationException("Event mis-match.");
				}
				_actevt = null;
				evt.Cancel();
			}
		}

		/// <summary>
		/// Invoked by an <see cref="ActivationEvent"/> to execute the
		/// <see cref="DesTask"/>.
		/// </summary>
		/// <param name="evt">
		/// The <see cref="ActivationEvent"/> that triggered this
		/// <see cref="DesTask"/> to execute.
		/// </param>
		internal void RunFromActivationEvent(ActivationEvent evt)
		{
			// Internal consistency check
			if (_actevt != evt)
			{
				throw new InvalidOperationException("Event mis-match.");
			}
			RestorePriority();
			_actevt = null;
			ExecuteTask(evt.Activator, evt.Data);
			ClearInterrupt();
		}

		//public int CompareTo(DesTask other)
		//{
		//    if (Name == null && other.Name == null)
		//        return 0;
		//    return Name.CompareTo(other);
		//}

		//public int CompareTo(object obj)
		//{
		//    var p = obj as DesTask;
		//    if (p == null)
		//        return -1;
		//    if (Name == null && p.Name == null)
		//        return 0;
		//    return Name.CompareTo(p.Name);
		//}
		//public override bool Equals(object obj)
		//{
		//    if (this == obj)
		//        return true;
		//    var p = obj as DesTask;
		//    if (p == null)
		//        return false;
		//    return Name.Equals(p.Name);
		//}

		//public override int GetHashCode()
		//{
		//    return Name.GetHashCode();
		//}
	}
}
