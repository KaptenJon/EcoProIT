//=============================================================================
//=  $Id: Condition.cs 184 2006-10-14 18:46:48Z eroe $
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
using React.Queue;
using React.Tasking;

namespace React
{
	/// <summary>
	/// A general-purpose blocking condition that can represent true/false
	/// situations.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="Condition"/> blocks <see cref="DesTask"/>s while it is in
	/// its false, or <em>reset</em> state.  When the <see cref="Condition"/>
	/// becomes true, or <em>signalled</em>, via a call to the
	/// <see cref="Signal"/> method, one or all blocked <see cref="DesTask"/>s are
	/// resumed.
	/// </para>
	/// <para>
	/// The <see cref="Condition"/> can be configured to automatically reset to
	/// the false state or remain true (signalled) using the
	/// <see cref="AutoReset"/> property.  In addition the
	/// <see cref="Condition"/> is configurable as to whether it will resume
	/// a single waiting <see cref="DesTask"/> or all waiting <see cref="DesTask"/>s
	/// when it is signalled via <see cref="ResumeAllOnSignal"/>.
	/// </para>
	/// <para>
	/// By default when resuming single <see cref="DesTask"/>s, the
	/// <see cref="DesTask"/>s are resumed in the order they blocked on the
	/// <see cref="Condition"/> (i.e. FIFO order).
	/// </para>
	/// </remarks>
	public class Condition : Blocking<DesTask>, ICondition
	{
		/// <summary>
		/// Flag indicating whether or not the <see cref="Condition"/>
		/// should automatically be reset after signalling.
		/// </summary>
		private bool _autoReset = true;
		/// <summary>
		/// Flag indicating if the <see cref="Condition"/> is signalled.
		/// </summary>
		private bool _signalled;
		/// <summary>
		/// Flag indicating whether all or just one <see cref="DesTask"/> will
		/// be resumed when the <see cref="Condition"/> is signalled.
		/// </summary>
		private bool _resumeAll;
		/// <summary>
		/// The wait queue used to block <see cref="DesTask"/> instances.
		/// </summary>
		private IQueue<DesTask> _waitQ;

		/// <overloads>Create and initialize a Condition.</overloads>
		/// <summary>
		/// Create a new, unnamed <see cref="Condition"/>.
		/// </summary>
		/// <remarks>
		/// The <see cref="Condition"/> is created in the reset state, the
		/// <see cref="AutoReset"/> and <see cref="ResumeAllOnSignal"/>
		/// properties are both <b>true</b>.
		/// </remarks>
		public Condition() : this(null, true)
		{
		}

		/// <summary>
		/// Create a new <see cref="Condition"/> having the given name.
		/// </summary>
		/// <remarks>
		/// The <see cref="Condition"/> is created in the reset state, the
		/// <see cref="AutoReset"/> and <see cref="ResumeAllOnSignal"/>
		/// properties are both <b>true</b>.
		/// </remarks>
		/// <param name="name">The name.</param>
		public Condition(string name) : this(name, true)
		{
		}

		/// <summary>
		/// Create an unnamed <see cref="Condition"/> specifying whether or not
		/// one or all blocked <see cref="DesTask"/>s should be resumed when
		/// signalled.
		/// </summary>
		/// <remarks>
		/// The <see cref="Condition"/> is created in the reset state and the
		/// <see cref="AutoReset"/> property is both <b>true</b>.
		/// </remarks>
		/// <param name="resumeAll">
		/// <b>true</b> if all blocked <see cref="DesTask"/>s are resumed; or
		/// <b>false</b> if the next blocked <see cref="DesTask"/> is resumed.
		/// </param>
		public Condition(bool resumeAll) : this(null, resumeAll)
		{
		}

		/// <summary>
		/// Create a <see cref="Condition"/> having the given name and
		/// specifying whether or not one or all blocked <see cref="DesTask"/>s
		/// should be resumed when signalled.
		/// </summary>
		/// <remarks>
		/// The <see cref="Condition"/> is created in the reset state and the
		/// <see cref="AutoReset"/> property is both <b>true</b>.
		/// </remarks>
		/// <param name="name">The name.</param>
		/// <param name="resumeAll">
		/// <b>true</b> if all blocked <see cref="DesTask"/>s are resumed; or
		/// <b>false</b> if the next blocked <see cref="DesTask"/> is resumed.
		/// </param>
		public Condition(string name, bool resumeAll) : base(name)
		{
			_resumeAll = resumeAll;
		}

		/// <summary>
		/// Gets or sets whether all blocked <see cref="DesTask"/>s are resumed
		/// when the <see cref="Condition"/> is signalled.
		/// </summary>
		/// <value>
		/// <b>true</b> if all blocked <see cref="DesTask"/>s are resumed; or
		/// <b>false</b> if the next blocked <see cref="DesTask"/> is resumed.
		/// </value>
		public bool ResumeAllOnSignal
		{
			get {return _resumeAll;}
			set {_resumeAll = value;}
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

		#region ICondition Members

		/// <summary>
		/// Block (wait) on the <see cref="Condition"/> until it becomes
		/// signalled.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="desTask"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> that will block on this
		/// <see cref="Condition"/> until it is signalled.  Note that
		/// <paramref name="desTask"/> does not <em>directly</em> block on
		/// the <see cref="Condition"/>, rather it should block on the
		/// <see cref="DesTask"/> returned by this method (which will actually
		/// block on the <see cref="Condition"/>).
		/// </param>
		/// <returns>
		/// The <see cref="DesTask"/> which will wait on this
		/// <see cref="Condition"/> on behalf of <paramref name="desTask"/>.
		/// </returns>
		[BlockingMethod]
		public DesTask Block(DesTask desTask)
		{
			if (desTask == null)
				throw new ArgumentNullException("desTask", "cannot be null");

			return new WaitForCondition(desTask.Simulation, this);
		}

		/// <summary>
		/// Gets whether or not the <see cref="Condition"/> automatically
		/// resets to an unsignalled state after invoking <see cref="Signal"/>.
		/// </summary>
		/// <remarks>
		/// <see cref="Condition"/> instances that do not auto-reset, will
		/// remain in the signalled state until the <see cref="Reset"/> method
		/// is invoked.  While signalled, the <see cref="Condition"/> will not
		/// block any <see cref="DesTask"/>s.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="Condition"/> automatically resets;
		/// or <b>false</b> if it must be manually reset by calling the
		/// <see cref="Reset"/> method.
		/// </value>
		public bool AutoReset
		{
			get {return _autoReset;}
			set {_autoReset = value;}
		}

		/// <summary>
		/// Gets whether or not the <see cref="Condition"/> is signalled.
		/// </summary>
		/// <remarks>
		/// When this property is <b>true</b>, the <see cref="Condition"/>
		/// will not block an <see cref="DesTask"/> during a call to
		/// <see cref="Block"/>.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="Condition"/> is signalled; or
		/// <b>false</b> if it is reset.
		/// </value>
		public bool Signalled
		{
			get {return _signalled;}
		}

		/// <summary>
		/// Place the <see cref="Condition"/> into a <em>signalled</em> state.
		/// </summary>
		/// <remarks>
		/// <para>
		/// One or more of the <see cref="DesTask"/>s blocking on the
		/// <see cref="Condition"/> are activated.  It is up to the actual
		/// implementation to decide how many of the blocked
		/// <see cref="DesTask"/>s to activate.
		/// </para>
		/// <para>
		/// If there are no <see cref="DesTask"/>s blocking on this
		/// <see cref="Condition"/> calling this method does nothing
		/// except set <see cref="Signalled"/> to <b>true</b>.  Even that
		/// change will be short-lived if <see cref="AutoReset"/> is
		/// <b>true</b>.
		/// </para>
		/// </remarks>
		public void Signal()
		{
			_signalled = !AutoReset;

			if (BlockCount > 0)
			{
				if (_resumeAll && BlockCount > 1)
					ResumeAll();
				else
					ResumeNext();
			}
		}

		/// <summary>
		/// Place the <see cref="Condition"/> into a <em>reset</em> state.
		/// </summary>
		/// <remarks>
		/// Subsequent calls to <see cref="Block"/> will block the
		/// <see cref="DesTask"/>.  Also, <see cref="Signalled"/> will be
		/// set to <b>false</b>.
		/// </remarks>
		public void Reset()
		{
			_signalled = false;
		}

		#endregion

		//====================================================================
		//====              Internal/Private Implementation               ====
		//====================================================================

		/// <summary>
		/// Invoked by the <see cref="Signal"/> method to resume the next
		/// waiting <see cref="DesTask"/>.
		/// </summary>
		private void ResumeNext()
		{
			System.Diagnostics.Debug.Assert(BlockCount > 0);
			bool canceled;
			do
			{
				DesTask desTask = _waitQ.Dequeue();
				canceled = desTask.Canceled;
				if (!canceled)
					desTask.Activate(this);
			} while (canceled && BlockCount > 0);
		}

		/// <summary>
		/// Invoked by the <see cref="Signal"/> method to resume all
		/// waiting <see cref="DesTask"/>s.
		/// </summary>
		private void ResumeAll()
		{
			System.Diagnostics.Debug.Assert(BlockCount > 1);
			int nWaiting = BlockCount;
			DesTask[] blockedDesTasks = new DesTask[nWaiting];
			_waitQ.CopyTo(blockedDesTasks, 0);
			_waitQ.Clear();
			for (int i = 0; i < nWaiting; i++)
			{
				DesTask desTask = blockedDesTasks[i];
				if (!desTask.Canceled)
					desTask.Activate(this);
			}
		}

		/// <summary>
		/// Called by a <see cref="WaitForCondition"/> DesTask to block on the
		/// <see cref="Condition"/>.
		/// </summary>
		/// <remarks>
		/// This method must be called only if the <see cref="Condition"/> is
		/// not signalled (e.g. <see cref="Signalled"/> is <b>false</b>).
		/// </remarks>
		/// <param name="desTask">
		/// The <see cref="WaitForCondition"/> DesTask to block.
		/// </param>
		internal void BlockTask(DesTask desTask)
		{
			System.Diagnostics.Debug.Assert(!Signalled);
			if (_waitQ == null)
				_waitQ = CreateBlockingQueue(DefaultQueue);
			_waitQ.Enqueue(desTask);
		}
	}
}
