//=============================================================================
//=  $Id: ProxyDesTask.cs 184 2006-10-14 18:46:48Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2005, Eric K. Roe.  All rights reserved.
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

namespace React.Tasking
{
	/// <summary>
	/// A <see cref="DesTask"/> that acts on behalf of another <see cref="DesTask"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="ProxyDesTaskDesTask{T}"/> acts as a "middle-man" between a
	/// <see cref="Client"/> and another object, the <see cref="Blocker"/>.
	/// Normally the <see cref="Client"/> blocks on the
	/// <see cref="ProxyDesTaskDesTask{T}"/> and the
	/// <see cref="ProxyDesTaskDesTask{T}"/> will request some service, which may
	/// block, from the <see cref="Blocker"/>.
	/// </para>
	/// <para>
	/// It's important to note that a <see cref="ProxyDesTaskDesTask{T}"/> can have
	/// only one client at a time.  This means that only one <see cref="DesTask"/>
	/// may block on the <see cref="ProxyDesTaskDesTask{T}"/> at any time.
	/// </para>
	/// <para>
	/// If, when the <see cref="ProxyDesTaskDesTask{T}"/> is executed, it has no
	/// client, it does nothing.
	/// </para>
	/// </remarks>
	/// <typeparam name="T">
	/// The class type upon which the <see cref="ProxyDesTaskDesTask{T}"/>
	/// will block.  This will be the <see cref="Blocker"/> property's type.
	/// </typeparam>
	public abstract class ProxyDesTask<T> : DesTask where T : class
	{
		/// <summary>
		/// The blocking object that is providing some service.
		/// </summary>
		private T _blocker;
		/// <summary>
		/// Auto-activate <see cref="Client"/> when activated by
		/// <see cref="Blocker"/>.
		/// </summary>
		private bool _autoActivateClient = true;

		/// <summary>
		/// Create a new <see cref="ProxyDesTaskDesTask{T}"/> that will obtain
		/// service from the specified object.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="blocker">
		/// The blocking object that will provide some service required by
		/// <see cref="Client"/>.
		/// </param>
		protected ProxyDesTask(Simulation sim, T blocker) : base(sim)
		{
			if (blocker == null)
			{
				throw new ArgumentNullException("'blocker' cannot be null.");
			}
			_blocker = blocker;
		}

		/// <summary>
		/// Gets the object that is providing some service to the
		/// <see cref="Client"/> DesTask through this
		/// <see cref="ProxyDesTaskDesTask{T}"/>.
		/// </summary>
		/// <value>
		/// The object providing service to <see cref="Client"/>.
		/// </value>
		public T Blocker
		{
			get {return _blocker;}
		}

		/// <summary>
		/// Gets the client <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// The client DesTask is set by having a <see cref="DesTask"/> block on this
		/// <see cref="ProxyDesTaskDesTask{T}"/>.  If no <see cref="DesTask"/> blocks
		/// on this DesTask, then <see cref="Client"/> will be
		/// <see langword="null"/>.
		/// </remarks>
		/// <value>
		/// The client <see cref="DesTask"/> or <see langword="null"/> if there
		/// is no client.
		/// </value>
		public DesTask Client
		{
			get
			{
				return BlockCount > 0 ? WaitQueue.Peek() : null;
			}
		}

		/// <summary>
		/// Gets or sets whether the <see cref="Client"/> is activated instead
		/// of this <see cref="ProxyDesTaskDesTask{T}"/> when this
		/// <see cref="ProxyDesTaskDesTask{T}"/> is activated by
		/// <see cref="Blocker"/>.
		/// </summary>
		/// <value>
		/// <b>true</b> if the <see cref="Client"/> is activated when this
		/// <see cref="ProxyDesTaskDesTask{T}"/> is activated by
		/// <see cref="Blocker"/>.
		/// </value>
		public bool AutoActivateClient
		{
			get { return _autoActivateClient; }
			protected set { _autoActivateClient = value; }
		}

		/// <summary>
		/// Activates the <see cref="ProxyDesTaskDesTask{T}"/>.
		/// </summary>
		/// <remarks>
		/// If <paramref name="activator"/> equals <see cref="Blocker"/> and
		/// <see cref="AutoActivateClient"/> is <b>true</b>, this method will
		/// activate <see cref="Client"/> rather than this
		/// <see cref="ProxyDesTaskDesTask{T}"/>.
		/// </remarks>
		/// <param name="activator">
		/// The object that is activating the <see cref="ProxyDesTaskDesTask{T}"/>.
		/// May be <see langword="null"/>
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the
		/// <see cref="ProxyDesTaskDesTask{T}"/> should be scheduled to run.
		/// </param>
		/// <param name="data">
		/// An object containing client-specific data for the
		/// <see cref="ProxyDesTaskDesTask{T}"/>.
		/// </param>
		/// <param name="priority">
		/// The DesTask priority.  Higher values indicate higher priorities.
		/// </param>
		public override void Activate(object activator, ulong relTime,
			object data, int priority)
		{
			if (activator == Blocker && AutoActivateClient)
			{
				System.Diagnostics.Debug.Assert(Client != null);
				System.Diagnostics.Debug.Assert(!Client.Canceled);
				Client.Activate(activator, relTime, data, priority);
				Cancel();
			}
			else
			{
				base.Activate(activator, relTime, data, priority);
			}
		}
	}
}
