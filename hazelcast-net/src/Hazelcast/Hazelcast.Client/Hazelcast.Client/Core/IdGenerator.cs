using System;

namespace Hazelcast.Core
{
	public interface IdGenerator: Instance
	{
		/**
	     * Returns the name of this Id Generator instance.
	     *
	     * @return name of this id generator instance
	     */
	    String getName();
	
	    /**
	     * Generates and returns cluster-wide unique id.
	     * Generated ids are guaranteed to be unique for the entire cluster
	     * as long as the cluster is live. If the cluster restarts then
	     * id generation will start from 0.
	     *
	     * @return cluster-wide new unique id
	     */
	    long newId();
	}
}

