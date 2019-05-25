

using System;

namespace React
{
	/// <summary>
	/// A general condition upon which <see cref="DesTask"/>s may block.
	/// <seealso cref="Condition"/>
	/// </summary>
	public interface ICondition
	{
		/// <summary>
		/// Block (wait) on the <see cref="ICondition"/> until it becomes
		/// signalled.
		/// </summary>
		/// <param name="desTask">
		/// The <see cref="DesTask"/> that will block on this
		/// <see cref="ICondition"/> until it is signalled.
		/// </param>
		/// <returns>
		/// The <see cref="DesTask"/> which will waits on the condition on
		/// behalf of <paramref name="desTask"/>.
		/// </returns>
		[BlockingMethod]
		DesTask Block(DesTask desTask);

		/// <summary>
		/// Gets whether or not the <see cref="ICondition"/> automatically
		/// resets to an unsignalled state after invoking <see cref="Signal"/>.
		/// </summary>
		/// <remarks>
		/// <see cref="ICondition"/> instances that do not auto-reset, will
		/// remain in the signalled state until the <see cref="Reset"/> method
		/// is invoked.  While signalled, the <see cref="ICondition"/> will not
		/// block any <see cref="DesTask"/>s.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="ICondition"/> automatically resets;
		/// or <b>false</b> if it must be manually reset by calling the
		/// <see cref="Reset"/> method.
		/// </value>
		bool AutoReset {get;}

		/// <summary>
		/// Gets whether or not the <see cref="ICondition"/> is signalled.
		/// </summary>
		/// <remarks>
		/// When this property is <b>true</b>, the <see cref="ICondition"/>
		/// will not block an <see cref="DesTask"/> during a call to
		/// <see cref="Block"/>.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="ICondition"/> is signalled; or
		/// <b>false</b> if it is reset.
		/// </value>
		bool Signalled {get;}

		/// <summary>
		/// Place the <see cref="ICondition"/> into a <em>signalled</em> state.
		/// </summary>
		/// <remarks>
		/// <para>
		/// One or more of the <see cref="DesTask"/>s blocking on the
		/// <see cref="ICondition"/> are activated.  It is up to the actual
		/// implementation to decide how many of the blocked
		/// <see cref="DesTask"/>s to activate.
		/// </para>
		/// <para>
		/// If there are no <see cref="DesTask"/>s blocking on this
		/// <see cref="ICondition"/> calling this method does nothing
		/// except set <see cref="Signalled"/> to <b>true</b>.  Even that
		/// change will be short-lived if <see cref="AutoReset"/> is
		/// <b>true</b>.
		/// </para>
		/// </remarks>
		void Signal();

		/// <summary>
		/// Place the <see cref="ICondition"/> into a <em>reset</em> state.
		/// </summary>
		/// <remarks>
		/// Subsequent calls to <see cref="Block"/> will block the
		/// <see cref="DesTask"/>.  Also, <see cref="Signalled"/> will be
		/// set to <b>false</b>.
		/// </remarks>
		void Reset();
	}
}
