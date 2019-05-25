//=============================================================================
//=  $Id: NoOperation.cs 128 2005-12-04 20:12:00Z Eric Roe $
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
	/// A <see cref="DesTask"/> that does nothing.
	/// </summary>
    /// <remarks>
    /// A <see cref="NoOperation"/> DesTask can be set to either: 1) resume
    /// all blocked <see cref="DesTask"/>s; or 2) end and resume nothing.  In
    /// the second case, the caller must have an alternate means of
    /// resuming blocked desTasks if they want those desTasks activated again during
    /// the simulation run.
    /// </remarks>
	public class NoOperation : DesTask
	{
        /// <summary>
        /// Flag to control whether blocked <see cref="DesTask"/>s are resumed
        /// when this DesTask executes.
        /// </summary>
        private bool _resumeBlocked;

        /// <summary>
        /// Create a new <see cref="NoOperation"/> DesTask.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
		public NoOperation(Simulation sim) : this(sim, true)
		{
		}

        /// <summary>
        /// Create a new <see cref="NoOperation"/> DesTask specifying whether or
        /// not to resume blocked <see cref="DesTask"/>s.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="resumeBlocked">
        /// <b>true</b> to resume all <see cref="DesTask"/>s blocked on this DesTask;
        /// or <b>false</b> to simply do nothing.
        /// </param>
        public NoOperation(Simulation sim, bool resumeBlocked)
            : base(sim)
        {
            _resumeBlocked = resumeBlocked;
        }

        /// <summary>
        /// Gets or sets whether or not the <see cref="NoOperation"/> DesTask
        /// will resume all desTasks which may be blocking upon it.
        /// </summary>
        /// <value>
        /// <b>true</b> to resume all <see cref="DesTask"/>s blocked on this DesTask;
        /// or <b>false</b> to simply do nothing.
        /// </value>
        public bool ResumeBlocked
        {
            get { return _resumeBlocked; }
            set { _resumeBlocked = value; }
        }

        /// <summary>
        /// Perform a no-operation.
        /// </summary>
        /// <remarks>
        /// Depending upon the value of <see cref="ResumeBlocked"/>, execution
        /// of this DesTask may or may not resume any <see cref="DesTask"/>s which
        /// are blocking upon it.
        /// </remarks>
        /// <param name="activator">Not used.</param>
        /// <param name="data">Not used.</param>
		protected override void ExecuteTask(object activator, object data)
		{
            if (_resumeBlocked)
                ResumeAll();
		}
	}
}
