using System;

namespace Hazelcast.Security
{
	public interface Credentials
	{
		/**
		 * Returns IP address of endpoint. 
		 * @return endpoint address 
		 */
		String getEndpoint();
	
		/**
		 * Sets IP address of endpoint.
		 * @param endpoint address
		 */
		void setEndpoint(String endpoint) ;
		
		/**
		 * Returns principal of endpoint.
		 * @return endpoint principal
		 */
		String getPrincipal() ;
		
	}
}

