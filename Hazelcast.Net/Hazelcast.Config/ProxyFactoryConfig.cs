// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Config
{
    public class ProxyFactoryConfig
    {
        private string _className;
        private string _service;

        public ProxyFactoryConfig()
        {
        }

        public ProxyFactoryConfig(string className, string service)
        {
            _className = className;
            _service = service;
        }

        public virtual string GetClassName()
        {
            return _className;
        }

        public virtual string GetService()
        {
            return _service;
        }

        public virtual void SetClassName(string className)
        {
            _className = className;
        }

        public virtual void SetService(string service)
        {
            _service = service;
        }
    }
}