//=============================================================================
//=  $Id: InterruptDesTask.cs 172 2006-09-10 21:19:08Z eroe $
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

namespace React.Tasking
{
    /// <summary>
    /// A <see cref="DesTask"/> implementation that can be used to interrupt other
    /// <see cref="DesTask"/>s.
    /// </summary>
    public class InterruptDesTask : DesTask
    {
        /// <summary>
        /// The object given as the <see cref="DesTask"/> interruptor.
        /// </summary>
        private object _interruptor;

        /// <overloads>Create and initialize an IterruptTask.</overloads>
        /// <summary>
        /// Create a new <see cref="InterruptDesTask"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        public InterruptDesTask(Simulation sim) : this(sim, null)
        {
        }

        /// <summary>
        /// Create a new <see cref="InterruptDesTask"/> specifiying the
        /// interruptor object.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="interruptor">The interruptor object.</param>
        public InterruptDesTask(Simulation sim, object interruptor)
            : base(sim)
        {
            _interruptor = interruptor;
        }

        /// <summary>
        /// Gets or sets the interruptor object.
        /// </summary>
        /// <remarks>
        /// The interruptor object is passed to each blocked <see cref="DesTask"/>'s
        /// <see cref="DesTask.Interrupt"/> method when this <see cref="DesTask"/> is
        /// executed.  By default it is set to <c>this</c>.
        /// </remarks>
        /// <value>
        /// The interruptor as an <see cref="object"/>.  Setting this property
        /// to <see langword="null"/> will result in the property getter
        /// returning <c>this</c>.
        /// </value>
        public object Interruptor
        {
            get { return _interruptor != null ? _interruptor : this; }
            set { _interruptor = value; }
        }

        /// <summary>
        /// Interrupt all blocked <see cref="DesTask"/> instances.
        /// </summary>
        /// <param name="activator">Not used.</param>
        /// <param name="data">Not used.</param>
        protected override void ExecuteTask(object activator, object data)
        {
            ResumeAll(Interruptor, null);
        }

        /// <summary>
        /// Resume the given <see cref="DesTask"/> using an interrupt.
        /// </summary>
        /// <param name="desTask">The <see cref="DesTask"/> to interrupt.</param>
        /// <param name="activator">The interruptor object.</param>
        /// <param name="data">Not used.</param>
        protected override void ResumeTask(DesTask desTask, object activator,
            object data)
        {
            // DO NOT INVOKE THE BASE CLASS IMPLEMENTATION!

            System.Diagnostics.Debug.Assert(Interruptor == activator);
            desTask.Interrupt(activator);
        }
    }
}
