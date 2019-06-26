//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using UnityEngine;

namespace VGltf.Unity
{
    public class IndexedResource<T>
    {
        public int Index;
        public T Value;
    }

    public class ResourcesCache
    {
        public Dictionary<string, IndexedResource<Texture2D>> Textures = new Dictionary<string, IndexedResource<Texture2D>>();
        public Dictionary<string, IndexedResource<Material>> Materials = new Dictionary<string, IndexedResource<Material>>();
        public Dictionary<string, IndexedResource<Mesh>> Meshes = new Dictionary<string, IndexedResource<Mesh>>();
        public Dictionary<string, IndexedResource<Avatar>> Avatars = new Dictionary<string, IndexedResource<Avatar>>();

        public delegate IndexedResource<T> Gerenator<T, U>(U obj);

        public IndexedResource<T> CacheObjectIfNotExists<T, U>(string name, U obj, Dictionary<string, IndexedResource<T>> dic, Gerenator<T, U> generator) where T : UnityEngine.Object
        {
            IndexedResource<T> res;
            if (!string.IsNullOrEmpty(name) && dic.TryGetValue(name, out res))
            {
                return res;
            }

            res = generator(obj);
            if (!string.IsNullOrEmpty(name))
            {
                dic.Add(name, res);
            }

            return res;
        }
    }
}
