//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using VJson;
using VJson.Schema;

// Reference: https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/*
namespace VGltf.Types
{
    public class GltfProperty
    {
        [JsonField(Name = "extensions"), JsonFieldIgnorable]
        public Dictionary<string, INode> Extensions;

        [JsonField(Name = "extras"), JsonFieldIgnorable]
        public object Extras;

        //

        public void AddExtension<T>(string name, T value)
        {
            if (Extensions == null)
            {
                Extensions = new Dictionary<string, INode>();
            }

            var s = new JsonSerializer(typeof(T));
            var node = s.SerializeToNode(value);
            Extensions.Add(name, node);
        }

        [Obsolete]
        public bool GetExtension<T>(string name, out T value)
        {
            return TryGetExtension<T>(name, out value);
        }

        public bool TryGetExtension<T>(string name, out T value)
        {
            if (Extensions == null) {
                value = default(T);
                return false;
            }

            INode node;
            if (!Extensions.TryGetValue(name, out node)) {
                value = default(T);
                return false;
            }

            var v = JsonSchemaAttribute.CreateFromClass<T>();
            var ex = v.Validate(node);
            if (ex != null)
            {
                // TODO: 
                throw ex;
            }

            var d = new JsonDeserializer(typeof(T));
            var dv = d.DeserializeFromNode(node);
            value = (T)dv;

            return true;
        }

        internal class ExtensionsResolverTag { }

        public static void RegisterExtension(string name, Type type)
        {
            DynamicResolver.Register<ExtensionsResolverTag>(name, type);
        }
    }
}
