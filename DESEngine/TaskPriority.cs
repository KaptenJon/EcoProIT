//=============================================================================
//=  $Id: TaskPriority.cs 166 2006-09-04 16:53:32Z eroe $
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
	/// The built-in DesTask priorities.
	/// </summary>
	/// <remarks>
	/// Defines the built-in DesTask priorities used by <see cref="DesTask"/>s,
	/// <see cref="ActivationEvent"/>s and <see cref="Simulation"/>.  Because
	/// other priority values are possible, <see cref="TaskPriority"/> is not
	/// an <c>enum</c>, as that would limit the priorities to a well defined
	/// set.
	/// </remarks>
	public sealed class TaskPriority
	{
		/// <summary>Immediate priority.</summary>
		private const int _immediate	= System.Int32.MaxValue;
		/// <summary>Maximum priority.</summary>
		private const int _maximum		= _immediate - 1;
		/// <summary>Elevated priority.</summary>
		private const int _elevated		= _normal + 100;
		/// <summary>Normal (default) priority.</summary>
		private const int _normal		= 0;
		/// <summary>Reduced priority.</summary>
		private const int _reduced		= _normal - 100;
		/// <summary>Discardable priority.</summary>
		private const int _discardable	= System.Int32.MinValue;

		/// <summary>Private constructor to prevent instantiation.</summary>
		private TaskPriority() {}

		/// <summary>
		/// Gets the immediate DesTask priority.
		/// </summary>
		/// <remarks>
		/// <see cref="DesTask"/>s which are activated with this priority are
		/// guaranteed to be executed before all other pending
		/// <see cref="DesTask"/>s.  To use the <see cref="Immediate"/> DesTask
		/// priority, the <see cref="DesTask"/> must be scheduled at the current
		/// simulation time.  It is an error to schedule an <see cref="DesTask"/>
		/// in the future with <see cref="Immediate"/> priority.
		/// </remarks>
		/// <value>
		/// The immediate priority as an <see cref="int"/>.  The immediate
		/// priority is defined as <see cref="System.Int32.MaxValue"/>.
		/// </value>
		public static int Immediate
		{
			get {return _immediate;}
		}

		/// <summary>
		/// Get the maximum DesTask priority.
		/// </summary>
		/// <remarks>
		/// This is the highest allowable priority that can be used to activate
		/// an <see cref="DesTask"/> beyond the current simulation time (i.e. in
		/// the future).
		/// </remarks>
		/// <value>
		/// The maximum DesTask priority as an <see cref="int"/>.  The maximum
		/// priority is defined as <c>TaskPriority.Immediate - 1</c>.
		/// </value>
		public static int Maximum
		{
			get {return _maximum;}
		}

		/// <summary>
		/// Get the elevated DesTask priority.
		/// </summary>
		/// <value>
		/// The elevated DesTask priority as an <see cref="int"/>.  The elevated
		/// priority is defined as <c>TaskPriority.Normal + 100</c>.
		/// </value>
		public static int Elevated
		{
			get {return _elevated;}
		}

		/// <summary>
		/// Gets the normal (default) DesTask priority.
		/// </summary>
		/// <remarks>
		/// Unless otherwise activated with a different priority, all
		/// <see cref="DesTask"/> instances should use the <see cref="Normal"/>
		/// DesTask priority.
		/// </remarks>
		/// <value>
		/// The normal (default) DesTask priority as an <see cref="int"/>.  The
		/// normal priority is defined as zero (0).
		/// </value>
		public static int Normal
		{
			get {return _normal;}
		}

		/// <summary>
		/// Get the reduced DesTask priority.
		/// </summary>
		/// <value>
		/// The reduced DesTask priority as an <see cref="int"/>.  The reduced
		/// priority is defined as <c>TaskPriority.Normal - 100</c>.
		/// </value>
		public static int Reduced
		{
			get {return _reduced;}
		}

		/// <summary>
		/// Gets the discardable DesTask priority.
		/// </summary>
		/// <remarks>
		/// <see cref="DesTask"/>s that are activated with the
		/// <see cref="Discardable"/> priority can be discarded by the
		/// <see cref="Simulation"/> if no higher priority desTasks are pending.
		/// Put simply, if all <see cref="DesTask"/>s waiting to run have a
		/// <see cref="DesTask.Priority"/> of <see cref="Discardable"/>, then
		/// they are all thrown away by the <see cref="Simulation"/>, which
		/// will cause the <see cref="Simulation"/> to end.  Discardable
		/// desTasks are useful for interval-based data collection where the
		/// data collector desTasks should stop when there are no more desTasks
		/// of significance pending.
		/// </remarks>
		/// <value>
		/// The discardable DesTask priority as an <see cref="int"/>.  The
		/// discardable priority is defined as
		/// <see cref="System.Int32.MinValue"/>.
		/// </value>
		public static int Discardable
		{
			get {return _discardable;}
		}
	}
}
