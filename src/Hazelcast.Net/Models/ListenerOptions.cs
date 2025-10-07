// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration for a listener.
/// </summary>
/// <remarks>
/// <para>The .NET implementation of listener configuration does not support registering
/// an actual listener implementation, as that implementation would need to be written
/// in Java. The only thing that is supported here is a Java class name, which must
/// be already available on the cluster.</para>
/// </remarks>
public class ListenerOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    private string _className;
    //private EventListener _implementation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenerOptions"/> class.
    /// </summary>
    public ListenerOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenerOptions"/> class.
    /// </summary>
    /// <param name="className">The class name.</param>
    public ListenerOptions(string className)
    {
        ClassName = className;
    }

    ///// <summary>
    ///// Initializes a new instance of the <see cref="ListenerConfig"/> class.
    ///// </summary>
    ///// <param name="implementation">The listener implementation.</param>
    //public ListenerConfig(EventListener implementation)
    //{
    //    Implementation = implementation;
    //}

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenerOptions"/> class.
    /// </summary>
    public ListenerOptions(ListenerOptions config)
    {
        //Implementation = config.Implementation;
        ClassName = config.ClassName;
    }

    /// <summary>
    /// Gets or sets the class name of the event listener.
    /// </summary>
    public string ClassName
    {
        get => _className;
        set
        {
            _className = value.ThrowIfNullNorWhiteSpace();
            //_implementation = null;
        }
    }

    ///// <summary>
    ///// Gets or sets the listener implementation.
    ///// </summary>
    //public EventListener Implementation
    //{
    //    get => _implementation;
    //    set
    //    {
    //        _implementation = value.EnsureIsNotNull();
    //        _className = null;
    //    }
    //}

    /// <summary>
    /// Gets or sets a value indicating whether to include the value of the event.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public virtual bool IncludeValue
    {
        get => true;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the listener is local.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public virtual bool Local
    {
        get => false;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override string ToString()
        => $"ListenerConfig [className={_className}, includeValue={IncludeValue}, local={Local}]";

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public virtual int ClassId => ConfigurationDataSerializerHook.ListenerConfig;

    /// <inheritdoc />
    public virtual void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_className);
        //output.WriteObject(implementation);
        output.WriteObject(null); // still, be compatible with Java!
    }

    /// <inheritdoc />
    public virtual void ReadData(IObjectDataInput input)
    {
        ClassName = input.ReadString();
        //Implementation = input.ReadObject();
        _ = input.ReadObject<object>(); // still, be compatible with Java!
    }
}
