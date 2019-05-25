//=============================================================================
//=  $Id: AcquireResource.cs 184 2006-10-14 18:46:48Z eroe $
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
	/// A <see cref="DesTask"/> used to acquire a DESResource item from a
	/// <see cref="DESResource"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Normally client code should not have to instantiate objects of this
	/// class.  Rather, they should use the <see cref="IResource.Acquire"/>
	/// method which will return the appropriate <see cref="DesTask"/> for
	/// acquiring a DESResource item on behalf of a client <see cref="DesTask"/>.
	/// </para>
	/// <para>
	/// This class is declared public to allow third parties to create
	/// their own derivatives of <see cref="DESResource"/>.
	/// </para>
	/// </remarks>
	public class AcquireResource : ProxyDesTask<DESResource>
	{
		/// <summary>
		/// Set to <b>true</b> if the <see cref="AcquireResource"/> DesTask
		/// has made the request from a <see cref="DESResource"/>.
		/// </summary>
		private bool _requestMade;
		/// <summary>
		/// The maximum wait time before giving up waiting on the
		/// <see cref="DESResource"/>.
		/// </summary>
		private ulong _maxwait;

		/// <summary>
		/// Create a new <see cref="AcquireResource"/> DesTask that will acquire
		/// from the specified <see cref="DESResource"/>.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="desResource">
		/// The <see cref="DESResource"/> from which to acquire an item.
		/// </param>
		public AcquireResource(Simulation sim, DESResource desResource)
			: this(sim, desResource, 0L)
		{
		}

		/// <overloads>
		/// Create and initialize an AcquireResource DesTask.
		/// </overloads>
		/// <summary>
		/// Create a new <see cref="AcquireResource"/> DesTask that will acquire
		/// from the specfied <see cref="DESResource"/> with a timeout.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="desResource">
		/// The <see cref="DESResource"/> from which to acquire an item.
		/// </param>
		/// <param name="maxWait">
		/// The maximum time to wait for <paramref name="desResource"/>.
		/// </param>
		public AcquireResource(Simulation sim, DESResource desResource, ulong maxWait)
			: base(sim, desResource)
		{
			if (maxWait < 0L)
			{
				throw new ArgumentException("'maxWait' cannot be negative.");
			}

			_maxwait = maxWait;
		}

		/// <summary>
		/// Gets the maximum wait time.
		/// </summary>
		/// <remarks>
		/// The maximum wait time is relative to the current simulation time.
		/// If <see cref="MaxWait"/> is zero (0L), the
		/// <see cref="AcquireResource"/> DesTask will wait for a DESResource
		/// indefinitely.
		/// </remarks>
		/// <value>
		/// The maximum wait time as an <see cref="ulong"/>.
		/// </value>
		public ulong MaxWait
		{
			get {return _maxwait;}
		}

		/// <summary>
		/// Attempt to acquire an item from a <see cref="DESResource"/> on behalf
		/// of some client <see cref="DesTask"/>.
		/// </summary>
		/// <param name="activator">
		/// The object that activated this <see cref="AcquireResource"/> DesTask.
		/// </param>
		/// <param name="data">
		/// Optional data for the <see cref="AcquireResource"/> DesTask.  This will
		/// normally be <see langword="null"/> or the DESResource item.
		/// </param>
		protected override void ExecuteTask(object activator, object data)
		{
			System.Diagnostics.Debug.Assert(activator != Blocker);
			if (!_requestMade)
			{
				if (Client != null)
				{
					_requestMade = true;
					if (!Blocker.RequestResource(this) && MaxWait > 0)
					{
						Activate(null, MaxWait, null, Priority);
					}
				}
			}
			else
			{
				ResumeAll();
			}
		}
	}
}
