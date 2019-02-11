/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hazelcast.IO;
using Hazelcast.Client.Protocol;
using Hazelcast.Logging;

using TimeStampIList = System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<string, long>>;

namespace Hazelcast.Client.Proxy
{
    /// <summary>
    /// Client proxy implementation for a <see cref="IPNCounter" />.
    /// </summary>
    internal class ClientPNCounterProxy : ClientProxy, IPNCounter
    {
        internal static readonly HashSet<Address> _emptyAddressList = new HashSet<Address>();
        internal volatile Address _currentTargetReplicaAddress;
        private volatile int _maxConfiguredReplicaCount;

        // Sync object to protect _currentTargetReplicaAddress against race conditions
        private readonly object _targetAddressGuard = new object();

        // The last vector clock observed by this proxy. It is used for maintaining
        // session consistency guarantees when reading from different replicas.
        internal volatile VectorClock _observedClock;

        /// <summary>
        /// Creates a client <see cref="IPNCounter" /> proxy
        /// </summary>
        /// <param name="serviceName">the service name</param>
        /// <param name="objectId">the PNCounter name</param>
        public ClientPNCounterProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
            _observedClock = new VectorClock();
        }

        public override string ToString()
        {
            return "PNCounter{name='" + GetName() + "\'}";
        }

        public long Get()
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeGetInternal(_emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterGetCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long GetAndAdd(long delta)
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(delta, true, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long AddAndGet(long delta)
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(delta, false, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long GetAndSubtract(long delta)
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(-delta, true, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long SubtractAndGet(long delta)
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(-delta, false, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long DecrementAndGet()
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(-1, false, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long IncrementAndGet()
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(1, false, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long GetAndDecrement()
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(-1, true, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public long GetAndIncrement()
        {
            var targetAddress = GetCRDTOperationTarget(_emptyAddressList);
            var response = InvokeAddInternal(1, true, _emptyAddressList, null, targetAddress);
            var decodedResponse = PNCounterAddCodec.DecodeResponse(response);

            UpdateObservedReplicaTimestamps(decodedResponse.replicaTimestamps);

            return decodedResponse.value;
        }

        public void Reset()
        {
            _observedClock = new VectorClock();
        }

        /// <summary>
        /// Adds the delta and returns the value of the counter before the update
        /// if getBeforeUpdate" is true or the value after the update if it is false.
        /// It will invoke client messages recursively on viable replica addresses
        /// until successful or the list of viable replicas is exhausted.
        /// Replicas with addresses contained in the excludedAddresses are skipped.
        /// If there are no viable replicas, this method will throw the lastException if not null
        /// or a NoDataMemberInClusterException if the lastException is null.
        /// </summary>
        /// <param name="delta">the delta to add to the counter value, can be negative</param>
        /// <param name="getBeforeUpdate">true if the operation should return the counter value before the addition,
        /// false if it should return the value after the addition</param>
        /// <param name="excludedAddresses">the addresses to exclude when choosing a replica address, must not be null</param>
        /// <param name="lastException">the exception thrown from the last invocation of the request on a replica, may be null</param>
        /// <param name="targetAddress">the target address</param>
        /// <returns>the result of the request invocation on a replica</returns>
        /// <exception cref="NoDataMemberInClusterException">if there are no replicas and the lastException is null</exception>
        internal IClientMessage InvokeAddInternal(long delta, bool getBeforeUpdate, HashSet<Address> excludedAddresses, Exception lastException, Address targetAddress)
        {
            if (targetAddress == null)
            {
                if (lastException != null)
                    throw lastException;

                throw new NoDataMemberInClusterException("Cannot invoke operations on a CRDT because the cluster does not contain any data members");
            }

            try
            {
                var request = PNCounterAddCodec.EncodeRequest(GetName(), delta, getBeforeUpdate, _observedClock.EntrySet(), targetAddress);
                return InvokeOnTarget(request, targetAddress);
            }
            catch (Exception ex)
            {
                Logger.GetLogger(GetType()).Finest("Unable to provide session guarantees when sending operations to " +
                                                   targetAddress.ToString() + ", choosing different target. Cause: " +
                                                   ex.ToString());

                // Make sure that this only affects the local variable of the method
                if (excludedAddresses == _emptyAddressList)
                    excludedAddresses = new HashSet<Address>();

                // Add current/failed address to exclusion list
                excludedAddresses.Add(targetAddress);

                // Look for the new target address (taking into account exclusion list)
                var newTarget = GetCRDTOperationTarget(excludedAddresses);

                // Send null target address in case it's uninitialized instance
                return InvokeAddInternal(delta, getBeforeUpdate, excludedAddresses, ex, newTarget);
            }
        }

        /// <summary>
        /// Returns the current value of the counter.
        /// It will invoke client messages recursively on viable replica addresses
        /// until successful or the list of viable replicas is exhausted.
        /// Replicas with addresses contained in the excludedAddresses are skipped.
        /// If there are no viable replicas, this method will throw the lastException
        /// if not null or a NoDataMemberInClusterException if the lastException is null.
        /// </summary>
        /// <param name="excludedAddresses">the addresses to exclude when choosing a replica address, must not be null</param>
        /// <param name="lastException">the exception thrown from the last invocation of the request on a replica, may be null</param>
        /// <param name="targetAddress">the target address</param>
        /// <returns>the result of the request invocation on a replica</returns>
        /// <exception cref="NoDataMemberInClusterException">if there are no replicas and the lastException is null</exception>
        internal IClientMessage InvokeGetInternal(HashSet<Address> excludedAddresses, Exception lastException, Address targetAddress)
        {
            if (targetAddress == null)
            {
                if (lastException != null)
                    throw lastException;

                throw new NoDataMemberInClusterException("Cannot invoke operations on a CRDT because the cluster does not contain any data members");
            }

            try
            {
                var request = PNCounterGetCodec.EncodeRequest(GetName(), _observedClock.EntrySet(), targetAddress);
                return InvokeOnTarget(request, targetAddress);
            }
            catch (Exception ex)
            {
                Logger.GetLogger(GetType()).Finest("Unable to provide session guarantees when sending operations to " +
                                                   targetAddress.ToString() + ", choosing different target. Cause: " +
                                                   ex.ToString());

                // Make sure that this only affects the local variable of the method
                if (excludedAddresses == _emptyAddressList)
                    excludedAddresses = new HashSet<Address>();

                // Add current/failed address to exclusion list
                excludedAddresses.Add(targetAddress);

                // Look for the new target address (taking into account exclusion list)
                var newTarget = GetCRDTOperationTarget(excludedAddresses);

                // Send null target address in case it's uninitialized instance
                return InvokeGetInternal(excludedAddresses, ex, newTarget);
            }
        }

        /// <summary>
        /// Returns the target on which this proxy should invoke a CRDT operation. On first invocation of this method,
        /// the method will choose a target address and return that address on future invocations.
        /// Replicas with addresses contained in the excludedAddresses list are excluded and if the chosen replica is in this list,
        /// a new replica is chosen and returned on future invocations.
        /// The method may return null if there are no viable target addresses.
        /// </summary>
        /// <param name="excludedAddresses">the addresses to exclude when choosing a replica address, must not be null</param>
        /// <returns>a CRDT replica address or null if there are no viable addresses</returns>
        private Address GetCRDTOperationTarget(HashSet<Address> excludedAddresses)
        {
            // Ensure the current address is not on excluded addresses list
            if (_currentTargetReplicaAddress != null && !excludedAddresses.Contains(_currentTargetReplicaAddress))
                return _currentTargetReplicaAddress;

            // If address has not been provided or is on exclusion list
            lock (_targetAddressGuard)
            {
                if (_currentTargetReplicaAddress == null || excludedAddresses.Contains(_currentTargetReplicaAddress))
                    _currentTargetReplicaAddress = ChooseTargetReplica(excludedAddresses);
            }

            return _currentTargetReplicaAddress;
        }

        /// <summary>
        /// Chooses and returns a CRDT replica address. Replicas with addresses contained in the excludedAddresses list are excluded
        /// and the method chooses randomly between the collection of viable target addresses. he method may return null if there are no viable addresses.
        /// </summary>
        /// <param name="excludedAddresses">the addresses to exclude when choosing a replica address, must not be null</param>
        /// <returns>a CRDT replica address or {@code null} if there are no viable addresses</returns>
        private Address ChooseTargetReplica(HashSet<Address> excludedAddresses)
        {
            var replicaAddresses = GetReplicaAddresses(excludedAddresses);
            if (replicaAddresses.Count == 0)
                return null;

            // Choose random replica
            int randomReplicaIndex = new Random().Next(replicaAddresses.Count);
            return replicaAddresses[randomReplicaIndex];
        }

        /// <summary>
        /// Returns the addresses of the CRDT replicas from the current state of the local membership list.
        /// Addresses contained in the excludedAddresses collection are excluded.
        /// </summary>
        /// <param name="excludedAddresses">the addresses to exclude when choosing a replica address, must not be null</param>
        /// <returns>list of possible CRDT replica addresses</returns>
        private List<Address> GetReplicaAddresses(HashSet<Address> excludedAddresses)
        {
            var dataMembers = GetContext().GetClusterService().GetMemberList().Where(x => !x.IsLiteMember).ToList();
            var maxConfiguredReplicaCount = GetMaxConfiguredReplicaCount();
            int currentReplicaCount = Math.Min(maxConfiguredReplicaCount, dataMembers.Count);
            var replicaAddresses = dataMembers
                .Select(x => x.GetAddress())
                .Where(x => excludedAddresses.Contains(x) == false)
                .Take(currentReplicaCount)
                .ToList();

            return replicaAddresses;
        }

        /// <summary>
        /// Returns the max configured replica count. When invoked for the first time,
        /// this method will fetch the configuration from a cluster member.
        /// </summary>
        /// <returns>the maximum configured replica count</returns>
        private int GetMaxConfiguredReplicaCount()
        {
            if (_maxConfiguredReplicaCount > 0)
                return _maxConfiguredReplicaCount;

            var request = PNCounterGetConfiguredReplicaCountCodec.EncodeRequest(GetName());
            var response = Invoke(request);
            var decodedResult = PNCounterGetConfiguredReplicaCountCodec.DecodeResponse(response);

            _maxConfiguredReplicaCount = decodedResult.response;
            return _maxConfiguredReplicaCount;
        }

        /// <summary>
        /// Updates the locally observed CRDT vector clock atomically. This method is thread safe and can be called concurrently.
        /// The method will only update the clock if the receivedLogicalTimestamps is higher than the currently observed vector clock.
        /// </summary>
        /// <param name="timeStamps">logical timestamps received from a replica state read</param>
        public void UpdateObservedReplicaTimestamps(TimeStampIList timeStamps)
        {
            var newVectorClock = new VectorClock(timeStamps);

            while (true)
            {
                // Store the original value just to avoid issue with data capture order
                var originalValue = _observedClock;

                if (originalValue.IsAfter(newVectorClock))
                    break;
               
                if (Interlocked.CompareExchange(ref _observedClock, newVectorClock, originalValue) == originalValue)
                    break;
            }
        }
    }
}
