//=============================================================================
//=  $Id: DESResource.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Collections;
using System.Collections.Generic;
using React.Queue;
using React.Tasking;

namespace React
{
	/// <summary>
	/// Abstract class used as a base for creating <see cref="IResource"/>
	/// implementations.
	/// </summary>
	/// <remarks>
	/// In most cases, <see cref="IResource"/> implementators should use this
	/// class as a base.  There are already two concrete <see cref="DESResource"/>
	/// subclasses available: <see cref="AnonymousDesResource"/> and
	/// <see cref="TrackedDesResource"/>.  In addition, <see cref="DESResource"/>
	/// offers several factory methods that can be used to instantiate
	/// <see cref="IResource"/> objects.
	/// </remarks>
	public abstract class DESResource : Blocking<AcquireResource>, IResource
	{
		/// <summary>
		/// Whether or not a <see cref="DesTask"/> may own more than one DESResource
		/// item.
		/// </summary>
		private bool _ownMany;
		/// <summary>
		/// An <see cref="IDictionary"/> used to track DESResource item owners.
		/// </summary>
		private IDictionary<DesTask, ResourceEntry> _owners;
		/// <summary>
		/// The wait queue used to block <see cref="AcquireResource"/> desTasks.
		/// </summary>
		private IQueue<AcquireResource> _waitQ;
		/// <summary>
		/// The number of reserved resources.
		/// </summary>
		private int _nReserved;

		/// <overloads>
		/// Create and initialize a new DESResource instance.
		/// </overloads>
		/// <summary>
		/// Create a new <see cref="DESResource"/> instance that has no name.
		/// </summary>
		protected DESResource() : this(null)
		{
		}

		/// <summary>
		/// Create a new <see cref="DESResource"/> instance with the specified
		/// name.
		/// </summary>
		/// <param name="name">The name of the <see cref="DESResource"/>.</param>
		protected DESResource(string name) : base(name)
		{
		}

		/// <summary>
		/// Gets or sets whether multiple resources from the pool may be
		/// owned by the same <see cref="DesTask"/>.
		/// </summary>
		/// <remarks>
		/// If a <see cref="DESResource"/> does not allow ownership of multiple
		/// resources by a single <see cref="DesTask"/>, an exception will be
		/// thrown if an owning <see cref="DesTask"/> calls <see cref="Acquire"/>
		/// before first calling <see cref="Release"/>.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the same <see cref="DesTask"/> may own multiple
		/// DESResource from the pool; or <b>false</b> if at most one DESResource
		/// from the pool may be owned by each <see cref="DesTask"/>. 
		/// </value>
		public bool AllowOwnMany
		{
			get {return _ownMany;}
			set {_ownMany = value;}
		}

		/// <summary>
		/// Gets the number of resources that have been reserved for use by
		/// requesting <see cref="DesTask"/>s but not yet actually allocated to
		/// those <see cref="DesTask"/>s.
		/// </summary>
		/// <value>
		/// The number of reserved resources as an <see cref="int"/>.
		/// </value>
		public int Reserved
		{
			get { return _nReserved; }
		}

		#region IResource Implementation

		/// <summary>
		/// Gets the total number of resources in the pool.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The total number of resources in the pool is defined as
		/// </para>
		/// <code>Count = Free + InUse + OutOfService</code>
		/// </remarks>
		/// <value>
		/// The total number of resources in the pool as an <see cref="int"/>.
		/// </value>
		public virtual int Count
		{
			get {return Free + InUse + OutOfService;}
		}

		/// <summary>
		/// Gets the number of resources that are not currently in use.
		/// </summary>
		/// <value>
		/// The number of resources that are not currently in use as an
		/// <see cref="int"/>.
		/// </value>
		public abstract int Free { get; }

		/// <summary>
		/// Gets the number of resources that are currently in use.
		/// </summary>
		/// <value>
		/// The number of resources that are currently in use as an
		/// <see cref="int"/>.
		/// </value>
		public abstract int InUse { get; }

		/// <summary>
		/// Gets or sets the number of resources that are out of service.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Out-of-service resources may not be acquired from the pool.  If
		/// <b>OutOfService</b> is set to a value greater or equal to
		/// <see cref="Count"/>, then all resources are out of service and
		/// all subsequent calls to <see cref="Acquire"/> will block.
		/// </para>
		/// <para>
		/// Decreasing the number of out-of-service resources has the potential
		/// side-effect of resuming one or more waiting <see cref="DesTask"/>s.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If an attempt is made to set this property to a value less than
		/// zero (0).
		/// </exception>
		/// <value>
		/// The number of resources currently out of service as an
		/// <see cref="int"/>.
		/// </value>
		public abstract int OutOfService { get; set; }

		/// <summary>
		/// Attempt to acquire a DESResource from the pool.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="requestor"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If <paramref name="requestor"/> is already an owner of a DESResource
		/// from this pool and <see cref="AllowOwnMany"/> is <b>false</b>.
		/// </exception>
		/// <param name="requestor">
		/// The <see cref="DesTask"/> that is requesting to acquire a DESResource
		/// from the pool.
		/// </param>
		/// <returns>
		/// The <see cref="DesTask"/> which will acquire a DESResource from the pool
		/// on behalf of <paramref name="requestor"/>.
		/// </returns>
		[BlockingMethod]
		public virtual DesTask Acquire(DesTask requestor)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor", "cannot be null");

			if (!AllowOwnMany && IsOwner(requestor))
			{
				throw new InvalidOperationException(
					"'requestor' already owns a DESResource from this pool.");
			}
			return new AcquireResource(requestor.Simulation, this);
		}
		public int nrofcurrent = 0;
		/// <summary>
		/// Releases a previously acquired DESResource back to the pool.
		/// </summary>
		/// <remarks>
		/// This method invokes <see cref="SelectItemToRelease"/> to
		/// select a DESResource item to release.  For
		/// <see cref="AnonymousDesResource"/> instances, the item will be
		/// <see langword="null"/>.  For <see cref="TrackedDesResource"/>
		/// instances, <see cref="SelectItemToRelease"/> <b>may</b>
		/// return a <see langword="null"/> or a reference to a DESResource
		/// item owned by <paramref name="owner"/> that should be released.
		/// This allows <see cref="Release"/> to be used to release a
		/// <see cref="TrackedDesResource"/> item without requiring the calling
		/// program to explicitly specify the item to release.  This
		/// behavior can be disabled on <see cref="TrackedDesResource"/>
		/// instance through the <see cref="TrackedDesResource.AutoSelect"/>
		/// property.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="owner"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If <paramref name="owner"/> is not an owner of one of the
		/// resources in this pool.
		/// </exception>
		/// <param name="owner">
		/// The <see cref="DesTask"/> that is releasing a DESResource.
		/// </param>
		/// <returns>
		/// The <see cref="DesTask"/> which will release the DESResource on behalf
		/// of <paramref name="owner"/>.
		/// </returns>
		[BlockingMethod]
		public virtual DesTask Release(DesTask owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner", "cannot be null");

			if (!IsOwner(owner))
			{
				throw new InvalidOperationException(
					"DesTask is not a DESResource owner.");
			}

			ResourceEntry entry = _owners[owner];
			object item = SelectItemToRelease(owner, entry.Items);
			return new ReleaseResource(owner.Simulation, this, item);
		}

		/// <summary>
		/// Transfer ownership of a previously acquired DESResource from one
		/// <see cref="DesTask"/> to another.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Ownership of a DESResource must be transferred from one
		/// <see cref="DesTask"/> to another if one DesTask is supposed to acquire
		/// the DESResource, but another DesTask will release it.
		/// </para>
		/// <para>
		/// It is important to note that <paramref name="receiver"/> is
		/// <b>not</b> resumed if it was waiting to acquire the DESResource.
		/// In the case where <paramref name="receiver"/> is blocking on
		/// the DESResource, <paramref name="owner"/> should interrupt
		/// <paramref name="receiver"/> after making the transfer.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If <paramref name="owner"/> is the same as
		/// <paramref name="receiver"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="owner"/> or <paramref name="receiver"/> is
		/// <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If <paramref name="owner"/> does not own a DESResource from this
		/// pool; or <paramref name="receiver"/> already owns a DESResource from
		/// this pool and <see cref="AllowOwnMany"/> is <b>false</b>.
		/// </exception>
		/// <param name="owner">
		/// The <see cref="DesTask"/> that owns the DESResource.
		/// </param>
		/// <param name="receiver">
		/// The <see cref="DesTask"/> which will receive the DESResource from
		/// <paramref name="owner"/>.
		/// </param>
		/// <returns>
		/// The <see cref="DesTask"/> which will transfer the DESResource to
		/// <paramref name="receiver"/> on behalf of <paramref name="owner"/>.
		/// </returns>
		[BlockingMethod]
		public DesTask Transfer(DesTask owner, DesTask receiver)
		{
			if (owner == receiver)
			{
				throw new ArgumentException(
					"'owner' was the same as 'receiver'.");
			}
			if (owner == null)
			{
				throw new ArgumentNullException("'owner' was null.");
			}
			if (receiver == null)
			{
				throw new ArgumentNullException("'receiver' was null.");
			}
			if (!IsOwner(owner))
			{
				throw new InvalidOperationException(
					"DesTask is not a DESResource owner.");
			}
			if (!AllowOwnMany && IsOwner(receiver))
			{
				throw new InvalidOperationException(
					"'receiver' already owns a DESResource from this pool.");
			}

			ResourceEntry entry = _owners[owner];
			object item = SelectItemToRelease(owner, entry.Items);
			return new TransferResource(owner.Simulation, this, receiver, item);
		}

		#endregion
	
		/// <summary>
		/// Checks if the given <see cref="DesTask"/> owns any resources in the
		/// pool.
		/// </summary>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> which may own one or more resources in the
		/// DESResource pool.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="desTask"/> owns at least one DESResource
		/// from this DESResource pool.
		/// </returns>
		public bool IsOwner(DesTask desTask)
		{
			return _owners != null ? _owners.ContainsKey(desTask) : false;
		}
	
		/// <summary>
		/// Gets the number of resources owned by the specified
		/// <see cref="DesTask"/> (if any).
		/// </summary>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> whose DESResource ownership count will be
		/// computed.
		/// </param>
		/// <returns>
		/// The number of DESResource from the pool owned by
		/// <paramref name="desTask"/>.  If <paramref name="desTask"/> does not
		/// own any resources from the pool, the returned value will be zero
		/// (0).
		/// </returns>
		public int OwnershipCount(DesTask desTask)
		{
			int count;
			if (IsOwner(desTask))
			{
				ResourceEntry entry = _owners[desTask];
				count = entry.Count;
				System.Diagnostics.Debug.Assert(count > 0);
			}
			else
			{
				count = 0;
			}
			return count;
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
		public override AcquireResource[] GetBlockedTasks(int queueId)
		{
			if (queueId == DefaultQueue || queueId == AllQueues)
				return GetBlockedTasks(_waitQ);

			throw new ArgumentException("Invalid queue id: " + queueId);
		}

		/// <summary>
		/// Immediately allocate a DESResource from the pool and return the
		/// associated DESResource item.
		/// </summary>
		/// <remarks>
		/// Anonymous <see cref="DESResource"/> implementations will always
		/// return <see langword="null"/>.  Tracked <see cref="DESResource"/>
		/// implementation must return a valid, non-null <see cref="object"/>.
		/// </remarks>
		/// <returns>
		/// The DESResource item associated with the allocated DESResource or
		/// <see langword="null"/> if resources are not assocatiated with
		/// arbitrary objects.
		/// </returns>
		protected abstract object AllocateResource();

		/// <summary>
		/// Immediately free (deallocate) a DESResource and return it to the
		/// pool.
		/// </summary>
		/// <param name="item">
		/// The DESResource item being freed.  This will be <see langword="null"/>
		/// for anonymous <see cref="DESResource"/> implementations.
		/// </param>
		protected abstract void DeallocateResource(object item);

		/// <summary>
		/// Resume as many blocked <see cref="DesTask"/>s as there are free
		/// DESResource items.
		/// </summary>
		protected virtual void ResumeWaiting()
		{
			while (Free > 0 && BlockCount > 0)
			{
				AcquireResource task = _waitQ.Dequeue();
				if (!task.Canceled && task.Client != null)
				{
					RequestResource(task);
				}
			}
		}

		/// <summary>
		/// Select and return a particular DESResource item to release.
		/// </summary>
		/// <remarks>
		/// This method may simply return <see langword="null"/> if the
		/// <see cref="DESResource"/> implementation does not track individual
		/// DESResource items; otherwise it should select an item from the
		/// <see cref="IList"/> to release.
		/// </remarks>
		/// <param name="owner">
		/// The <see cref="DesTask"/> that is the actual DESResource owner.
		/// </param>
		/// <param name="items">
		/// An immutable <see cref="IList"/> of DESResource items owned by
		/// <paramref name="owner"/>.  This will be <see langword="null"/>
		/// if the <see cref="DESResource"/> is not tracking individual items.
		/// </param>
		/// <returns>A DESResource item to release.</returns>
		protected abstract object SelectItemToRelease(DesTask owner, IList items);

		//====================================================================
		//====                 Static Factory Methods                     ====
		//====================================================================

		/// <summary>
		/// Create an anonymous <see cref="DESResource"/> containing the given
		/// number of in-service resources.
		/// </summary>
		/// <param name="count">
		/// The total number of resources in the pool, all of which are 
		/// in-service.
		/// </param>
		/// <returns>
		/// An anonymous <see cref="DESResource"/> containing
		/// <paramref name="count"/> resources, all of which are in-service.
		/// </returns>
		public static DESResource Create(int count)
		{
			return DESResource.Create(count, 0);
		}

		/// <summary>
		/// Create an anonymous <see cref="DESResource"/> containing the
		/// specified number of in-service and out-of-service resources.
		/// </summary>
		/// <remarks>
		/// The total number of resources in the pool is given by
		/// <c>inService + outOfService</c>.
		/// </remarks>
		/// <param name="inService">
		/// The number of resources in the pool that are in-service.
		/// </param>
		/// <param name="outOfService">
		/// The number of resources in the pool that are out-of-service.
		/// </param>
		/// <returns>
		/// An anonymous <see cref="DESResource"/> containing
		/// <paramref name="inService"/> in-service resources and
		/// <paramref name="outOfService"/> out-of-service resources.
		/// </returns>
		public static DESResource Create(int inService, int outOfService)
		{
			int count = inService + outOfService;
			DESResource desResource = new AnonymousDesResource(count);
			desResource.OutOfService = outOfService;
			return desResource;
		}

		/// <summary>
		/// Create a tracked <see cref="DESResource"/> containing the given
		/// objects.
		/// </summary>
		/// <remarks>
		/// Each object in <paramref name="items"/> must be of the same
		/// type.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If <paramref name="items"/> contains objects having differing
		/// types or is empty.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="items"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="items">
		/// An <see cref="IEnumerable"/> that contains one or more objects
		/// that will be dispensed as resources.
		/// </param>
		/// <returns>
		/// A tracked <see cref="DESResource"/> containing the given items.
		/// </returns>
		public static DESResource Create(IEnumerable items)
		{
			return new TrackedDesResource(items);
		}

		//====================================================================
		//====              Internal/Private Implementation               ====
		//====================================================================

		/// <summary>
		/// Gets or frees a reserved DESResource item.
		/// </summary>
		/// <remarks>
		/// This method decrements the reserve count and, if 
		/// <paramref name="evt"/> is pending it will actually allocate a
		/// DESResource item to the <see cref="DesTask"/> which will be run by
		/// <paramref name="evt"/>.
		/// </remarks>
		/// <param name="evt">
		/// The <see cref="ActivationEvent"/> making the data request.
		/// </param>
		/// <returns>
		/// The DESResource item associated with the allocated DESResource or
		/// <see langword="null"/> if resources are not assocatiated with
		/// arbitrary objects.  Also returns <see langword="null"/> if
		/// <paramref name="evt"/> is not pending (i.e. the
		/// <see cref="ActivationEvent.IsPending"/> method is <b>false</b>).
		/// </returns>
		private object GetOrFreeReserved(ActivationEvent evt)
		{
			object obj;
			_nReserved--;
			if (evt.IsPending && Free + Reserved > 0)
			{ 
				System.Diagnostics.Debug.Assert(evt.DesTask != null);
				obj = AllocateResource();
				SetOwner(evt.DesTask, obj);
			}
			else
			{
				obj = null;
			}
			return obj;
		}

		/// <summary>
		/// Invoked by a <see cref="AcquireResource"/> DesTask to request
		/// allocation of a DESResource.
		/// </summary>
		/// <remarks>
		/// If there are no free resources, <paramref name="task"/> will be
		/// placed on the wait queue.
		/// </remarks>
		/// <param name="task">
		/// The <see cref="AcquireResource"/> DesTask making the request.
		/// </param>
		/// <returns>
		/// <b>true</b> if a DESResource was allocated; or <b>false</b> if no
		/// DESResource was available and <paramref name="task"/> was blocked.
		/// </returns>
		internal bool RequestResource(AcquireResource task)
		{
			bool mustWait = Free < 1;
			if (mustWait)
			{
				if (_waitQ == null)
					_waitQ = CreateBlockingQueue(DefaultQueue);
				_waitQ.Enqueue(task);
			}
			else
			{
				_nReserved++;
				task.Activate(this, 0L,
					new DeferredDataCallback(GetOrFreeReserved));
			}

			return !mustWait;
		}

		/// <summary>
		/// Invoked by a <see cref="ReleaseResource"/> DesTask to return a
		/// DESResource to the pool.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="DesTask"/> that is the actual DESResource owner.  This
		/// is <b>not</b> the <see cref="ReleaseResource"/> DesTask.
		/// </param>
		/// <param name="item">
		/// The DESResource item.  This will be <see langword="null"/> for
		/// anonymous resources.
		/// </param>
		internal void ReturnResource(DesTask owner, object item)
		{
			DeallocateResource(item);
			ClearOwner(owner, item);
			ResumeWaiting();
		}

		/// <summary>
		/// Invoked by a <see cref="TransferResource"/> DesTask to transfer
		/// ownership of a DESResource from one <see cref="DesTask"/> to another.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="DesTask"/> that is the actual DESResource owner.  This
		/// is <b>not</b> the <see cref="TransferResource"/> DesTask.
		/// </param>
		/// <param name="receiver">
		/// The <see cref="DesTask"/> that will be granted ownership of the
		/// DESResource.
		/// </param>
		/// <param name="item">
		/// The DESResource item.  This will be <see langword="null"/> for
		/// anonymous resources.
		/// </param>
		internal void TransferResource(DesTask owner, DesTask receiver, object item)
		{
			if (receiver != null)
			{
				ClearOwner(owner, item);
				SetOwner(receiver, item);
			}
			else
			{
				ReturnResource(owner, item);
			}
		}

		/// <summary>
		/// Record the given <see cref="DesTask"/> as a DESResource item owner.
		/// </summary>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> that owns <paramref name="item"/>.
		/// </param>
		/// <param name="item">
		/// The reference to a DESResource item for tracked resources; or
		/// <see langword="null"/> for anonymous resources.
		/// </param>
		internal void SetOwner(DesTask desTask, object item)
		{
			if (desTask != null)
			{
				ResourceEntry entry;
				if (_owners == null)
					_owners = new Dictionary<DesTask, ResourceEntry>();

				if (!IsOwner(desTask))
				{
					if (item == null)
						entry = new ResourceEntry();
					else
						entry = new ResourceEntry(item);
					_owners.Add(desTask, entry);
				}
				else
				{
					entry = _owners[desTask];
					if (item == null)
						entry.Add();
					else
						entry.Add(item);
				}
			}
		}

		/// <summary>
		/// Removes the specified <see cref="DesTask"/> as a DESResource item owner.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="DesTask"/> that owns <paramref name="item"/>.
		/// </param>
		/// <param name="item">
		/// A reference to the DESResource item previously obtained via a call to
		/// <see cref="AllocateResource"/> (for tracked resources); or
		/// <see langword="null"/> (for anonymous resources).
		/// </param>
		private void ClearOwner(DesTask owner, object item)
		{
			if (owner != null && _owners != null)
			{
				ResourceEntry entry = _owners[owner];
				if (item == null)
					entry.Remove();
				else
					entry.Remove(item);
			
				if (entry.Count < 1)
					_owners.Remove(owner);
			}
		}
	}
}
