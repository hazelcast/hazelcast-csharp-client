using Hazelcast.IO.Serialization;
using Hazelcast.Security;


namespace Hazelcast.Security
{
	/// <summary>
	/// ICredentials is a container object for endpoint (Members and Clients)
	/// security attributes.
	/// </summary>
	/// <remarks>
	/// ICredentials is a container object for endpoint (Members and Clients)
	/// security attributes.
	/// <p/>
	/// It is used on authentication process by
	/// </remarks>
	public interface ICredentials : IPortable
	{
		/// <summary>Returns IP address of endpoint.</summary>
		/// <remarks>Returns IP address of endpoint.</remarks>
		/// <returns>endpoint address</returns>
		string GetEndpoint();

		/// <summary>Sets IP address of endpoint.</summary>
		/// <remarks>Sets IP address of endpoint.</remarks>
		/// <param name="endpoint">address</param>
		void SetEndpoint(string endpoint);

		/// <summary>Returns principal of endpoint.</summary>
		/// <remarks>Returns principal of endpoint.</remarks>
		/// <returns>endpoint principal</returns>
		string GetPrincipal();
	}
}
