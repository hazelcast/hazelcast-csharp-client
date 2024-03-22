// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Hazelcast.Networking;

/// <summary>
/// Represents the TPC options.
/// </summary>
/// <remarks>
/// <para>TPC is a BETA feature. It is not supported, and can change anytime.</para>
/// </remarks>
public class TpcOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TpcOptions"/> class.
    /// </summary>
    public TpcOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TpcOptions"/> class.
    /// </summary>
    private TpcOptions(TpcOptions other)
    {
        Enabled = other.Enabled;
        Required = other.Required;
    }

    /// <summary>
    /// Whether TPC is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether TPC is required.
    /// </summary>
    /// <remarks>
    /// <para>When TPC is required, a connection to a member will be terminated if it fails
    /// to connect all its TPC channels. When TPC is not required, and one or more TPC channels
    /// cannot be connected, the connection falls back to the classic connection.</para>
    /// </remarks>
    public bool Required { get; set; }

    /// <summary>
    /// Clones the options.
    /// </summary>
    internal TpcOptions Clone() => new(this);
}
