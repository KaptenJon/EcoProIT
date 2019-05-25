//=============================================================================
//=  $Id: TransferResource.cs 184 2006-10-14 18:46:48Z eroe $
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
	/// A <see cref="DesTask"/> used to tranfer a DESResource item from its current
	/// owning <see cref="DesTask"/> to another <see cref="DesTask"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Normally client code should not have to instantiate objects of this
	/// class.  Rather, they should use the <see cref="IResource.Transfer"/>
	/// method which will return the appropriate <see cref="DesTask"/> for
	/// transfering a DESResource item on behalf of a client <see cref="DesTask"/>.
	/// </para>
	/// <para>
	/// This class is declared public to allow third parties to create
	/// their own derivatives of <see cref="DESResource"/>.
	/// </para>
	/// </remarks>
	public class TransferResource : ProxyDesTask<DESResource>
	{
		/// <summary>
		/// The <see cref="DesTask"/> which will become the new DESResource item
		/// owner.
		/// </summary>
		private DesTask _receiver;
		/// <summary>
		/// The item to transfer to <see cref="_receiver"/>.
		/// </summary>
		/// <remarks>
		/// This will be <see langword="null"/> for un-tracked (anonymous)
		/// resources.
		/// </remarks>
		private object _itemToTransfer;


		/// <summary>
		/// Create a new <see cref="TransferResource"/> DesTask that will transfer
		/// a DESResource item to the specified receiver.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="desResource">
		/// The <see cref="DESResource"/> containing the item to transfer to
		/// <paramref name="receiver"/>.
		/// </param>
		/// <param name="receiver">
		/// The <see cref="DesTask"/> which will become the new owner of an item
		/// contained in <paramref name="desResource"/>.
		/// </param>
		public TransferResource(Simulation sim, DESResource desResource, DesTask receiver)
			: this(sim, desResource, receiver, null)
		{
		}

		/// <summary>
		/// Create a new <see cref="TransferResource"/> DesTask that will transfer
		/// a specific DESResource item to the given receiver.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="desResource">
		/// The <see cref="DESResource"/> containing the item to transfer to
		/// <paramref name="receiver"/>.
		/// </param>
		/// <param name="receiver">
		/// The <see cref="DesTask"/> which will become the new owner of an item
		/// contained in <paramref name="desResource"/>.
		/// </param>
		/// <param name="item">
		/// The DESResource item to transfer.  This should be <see langword="null"/>
		/// for anonymous resources.
		/// </param>
		public TransferResource(Simulation sim, DESResource desResource,
			DesTask receiver, object item) : base(sim, desResource)
		{
			_receiver = receiver;
			_itemToTransfer = item;
		}

		/// <summary>
		/// Transfers ownership of a DESResource item to another <see cref="DesTask"/>.
		/// </summary>
		/// <param name="activator">
		/// The current owner of the DESResource item.
		/// </param>
		/// <param name="data">Not used.</param>
		protected override void ExecuteTask(object activator, object data)
		{
			DesTask owner = activator as DesTask;
			if (owner == null)
			{
				throw new ArgumentException("'activator' not a DesTask instance.");
			}
			Blocker.TransferResource(owner, _receiver, _itemToTransfer);
			ResumeAll(Blocker, null);
		}
	}
}
