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

    public class Skin
    {
    }

    public class ResourcesCache<Key>
    {
        public Dictionary<object, IndexedResource<Transform>> Nodes = new Dictionary<object, IndexedResource<Transform>>();

        public Dictionary<Key, IndexedResource<Texture2D>> Textures = new Dictionary<Key, IndexedResource<Texture2D>>();
        public Dictionary<Key, IndexedResource<Material>> Materials = new Dictionary<Key, IndexedResource<Material>>();
        public Dictionary<Key, IndexedResource<Mesh>> Meshes = new Dictionary<Key, IndexedResource<Mesh>>();
        public Dictionary<Key, IndexedResource<Skin>> Skins = new Dictionary<Key, IndexedResource<Skin>>();
        public Dictionary<Key, IndexedResource<Avatar>> Avatars = new Dictionary<Key, IndexedResource<Avatar>>();

        public delegate IndexedResource<T> Gerenator<T, U>(U obj);

        public IndexedResource<Transform> CacheObjectIfNotExists(Transform obj, Dictionary<object, IndexedResource<Transform>> dic, Gerenator<Transform, Transform> generator)
        {
            IndexedResource<Transform> res;
            // Cached by reference
            if (dic.TryGetValue(obj, out res))
            {
                return res;
            }

            res = generator(obj);
            dic.Add(obj, res);

            return res;
        }

        public IndexedResource<T> CacheObjectIfNotExists<T, U>(Key key, U obj, Dictionary<Key, IndexedResource<T>> dic, Gerenator<T, U> generator)
        {
            IndexedResource<T> res;

            var preCond = true;
            if (key is string)
            {
                preCond = !string.IsNullOrEmpty(key as string);
            } else
            {
                preCond = key != null;
            }

            if (preCond && dic.TryGetValue(key, out res))
            {
                return res;
            }

            res = generator(obj);
            if (preCond)
            {
                dic.Add(key, res);
            }

            return res;
        }
    }
}
