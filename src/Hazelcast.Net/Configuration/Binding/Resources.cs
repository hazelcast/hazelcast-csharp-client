// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System.Globalization;

namespace Hazelcast.Configuration.Binding
{
    // no idea how building Microsoft's runtime generates these,
    // so adding them by hand here
    // make sure to mark the class 'partial' in Resources.Designer.cs

    internal partial class Resources
    {
        internal static string FormatError_CannotActivateAbstractOrInterface(params object[] args)
            => string.Format(CultureInfo.InvariantCulture, Error_CannotActivateAbstractOrInterface, args);

        internal static string FormatError_FailedBinding(params object[] args)
            => string.Format(CultureInfo.InvariantCulture, Error_FailedBinding, args);

        internal static string FormatError_FailedToActivate(params object[] args)
            => string.Format(CultureInfo.InvariantCulture, Error_FailedToActivate, args);

        internal static string FormatError_MissingParameterlessConstructor(params object[] args)
            => string.Format(CultureInfo.InvariantCulture, Error_MissingParameterlessConstructor, args);

        internal static string FormatError_UnsupportedMultidimensionalArray(params object[] args)
            => string.Format(CultureInfo.InvariantCulture, Error_UnsupportedMultidimensionalArray, args);
    }
}
