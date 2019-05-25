//=============================================================================
//=  $Id: ResourceEntry.cs 184 2006-10-14 18:46:48Z eroe $
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

namespace React
{
	/// <summary>
	/// Internal class used by <see cref="DESResource"/> to help keep track of
	/// which <see cref="DesTask"/>s own resources belonging to the pool.
	/// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="ResourceEntry"/> instance keep track of either the
    /// DESResource items issued by an <see cref="AnonymousDesResource"/> or a
    /// <see cref="TrackedDesResource"/> but <b>not</b> both.  When used with
    /// an <see cref="AnonymousDesResource"/>, the <see cref="ResourceEntry"/>
    /// tracks allocated items via a simple <see cref="int"/> counter.
    /// When used with a <see cref="TrackedDesResource"/>, the
    /// <see cref="ResourceEntry"/> tracks allocated items by their
    /// actual references.
    /// </para>
    /// <para>
    /// The type of DESResource which the <see cref="ResourceEntry"/> will track
    /// is determined by which constructor is called.
    /// </para>
    /// </remarks>
	internal class ResourceEntry
	{
        /// <summary>
        /// The number of DESResource items acquired.
        /// </summary>
		private int _count = 1;
        /// <summary>
        /// Reference to a single owned DESResource item.
        /// </summary>
		private object _single;
        /// <summary>
        /// Reference to multiple owned DESResource items.
        /// </summary>
		private IList _multiple;

        /// <overloads>Create and initialize a ResourceEntry.</overloads>
        /// <summary>
        /// Create a new <see cref="ResourceEntry"/> that helps keep track
        /// DESResource owners of an <see cref="AnonymousDesResource"/>.
        /// </summary>
		internal ResourceEntry() 
		{
		}

        /// <summary>
        /// Create a new <see cref="ResourceEntry"/> that helps keep track
        /// of DESResource owners of a <see cref="TrackedDesResource"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="item"/> is <see langword="null"/>.</exception>
        /// <param name="item">The DESResource item to track.</param>
		internal ResourceEntry(object item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("'item' cannot be null.");
			}
			_single = item;
		}

        /// <summary>
        /// Gets the number of DESResource items owned by a single
        /// <see cref="DesTask"/>.
        /// </summary>
        /// <value>
        /// The number of DESResource items owned by a single
        /// <see cref="DesTask"/> as an <see cref="int"/>/
        /// </value>
		internal int Count
		{
			get
			{
				return _multiple != null ? _multiple.Count : _count;
			}
		}

        /// <summary>
        /// Gets an immutable <see cref="IList"/> of the DESResource items
        /// tracked by this <see cref="ResourceEntry"/>.
        /// </summary>
        /// <value>
        /// An immutable <see cref="IList"/> of tracked DESResource items or
        /// <see langword="null"/> if the <see cref="ResourceEntry"/> is not
        /// tracking individual DESResource items.
        /// </value>
        internal IList Items
        {
            get
            {
                IList list;

                if (_multiple != null)
                    list = ArrayList.ReadOnly(_multiple);
                else if (_single != null)
                {
                    list = new ArrayList(1);
                    list.Add(_single);
                    list = ArrayList.ReadOnly(list);
                }
                else
                    list = null;

                return list;
            }
        }

        /// <summary>
        /// Adds one to the ownership count of an
        /// <see cref="AnonymousDesResource"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If this <see cref="ResourceEntry"/> is tracking DESResource item
        /// references rather than simply counting anonymous DESResource items.
        /// </exception>
		internal void Add() 
		{
			if (_single != null ||_multiple != null)
			{
				throw new InvalidOperationException();
			}
			_count++;
		}

        /// <summary>
        /// Adds the given DESResource item to the set of items owned by
        /// a single <see cref="DesTask"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If this <see cref="ResourceEntry"/> is counting anonymous
        /// DESResource items rather than tracking DESResource item references.
        /// </exception>
        /// <param name="item">The DESResource item to add.</param>
		internal void Add(object item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("'item' cannot be null.");
			}
			if (_single != null && _multiple == null)
			{
				_multiple = new ArrayList();
				_multiple.Add(_single);
				_multiple.Add(item);
				_count = -1;
				_single = null;
			}
			else if (_single == null && _multiple != null)
			{
				_multiple.Add(item);
			}
			else 
			{
				throw new InvalidOperationException();
			}
		}

        /// <summary>
        /// Removes one from the ownership count of an
        /// <see cref="AnonymousDesResource"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <see cref="Count"/> is less than one (1) or this
        /// <see cref="ResourceEntry"/> is tracking multiple DESResource item
        /// references (as opposed to counting anonymous DESResource items).
        /// </exception>
        internal void Remove()
		{
			if (Count < 1)
			{
				throw new InvalidOperationException();
			}
			else if (_single != null)
			{
				System.Diagnostics.Debug.Assert(_count == 1);
				_count = 0;
				_single = null;
			}
			else if (Count == 1 && _multiple != null)
			{
				_multiple.Clear();
			}
			else if (_single == null && _multiple == null)
			{
				_count--;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

        /// <summary>
        /// Removes the given DESResource item from the set of items owned by
        /// a single <see cref="DesTask"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <see cref="Count"/> is less than one (1).
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="item"/> is not a reference being tracked by
        /// this <see cref="ResourceEntry"/>.
        /// </exception>
        /// <param name="item">The DESResource item to remove.</param>
        internal void Remove(object item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("'item' cannot be null.");
			}

			if (Count < 1)
			{
				throw new InvalidOperationException();
			}

			if (_single == item)
			{
				Remove();
			}
			else if (_multiple != null && _multiple.Contains(item))
			{
				_multiple.Remove(item);
			}
			else 
			{
				throw new ArgumentException("'item' is not present.");
			}
		}
	}
}
