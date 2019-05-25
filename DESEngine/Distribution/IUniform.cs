//=============================================================================
//=  $Id: IUniform.cs 128 2005-12-04 20:12:00Z Eric Roe $
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

namespace React.Distribution
{
    /// <summary>
    /// A producer of pseudo-random numbers distributed uniformly.
    /// </summary>
    public interface IUniform : IRandom
    {
        /// <summary>
        /// Generate and return the next uniformly distributed
        /// <see cref="int"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as an <see cref="int"/>.
        /// </returns>
        int NextInteger();

        /// <summary>
        /// Generate and return the next uniformly distributed
        /// <see cref="ulong"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as an <see cref="ulong"/>.
        /// </returns>
        long NextLong();

    }
}
