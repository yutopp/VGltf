//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading.Tasks;
using System.IO;
using VJson;
using UnityEngine;

namespace VGltf.Unity
{
    public class Importer
    {
        private ResourcesOnMemory _res;
        private ResourcesStore _store;

        public Importer(ResourcesOnMemory res, ResourcesStore store)
        {
            _res = res;
            _store = store;
        }

        public async Task<GameObject> ImportAsync()
        {
            var gltf = _store.Gltf;

            if (gltf.Scene == null) {
                return;
            }
            var sceneIndex = gltf.Scene.Value;

            return await ImportSceneAsync(sceneIndex);
        }

        public async Task<GameObject> ImportSceneAsync(int sceneIndex)
        {
            var gltf = _store.Gltf;

            var scene = gltf.Scenes[sceneIndex];

            var name = string.IsNullOrEmpty(scene.Name) ? "Scene" : scene.Name;
            var go = new GameObject(name);

            foreach(var nodeIndex in scene.Nodes) {
                var goNode = await ImportNodeAsync(nodeIndex);
            }

            return go;
        }

        public async Task<GameObject> ImportNodeAsync(int nodeIndex)
        {
            var gltf = _store.Gltf;

            var node = gltf.Nodes[nodeIndex];

            var name = string.IsNullOrEmpty(node.Name) ? $"Node.${nodeIndex}" : node.Name;
            var go = new GameObject(name);

            return go;
        }
    }
}
