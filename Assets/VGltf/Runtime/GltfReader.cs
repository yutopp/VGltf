//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using VJson;

namespace VGltf
{
    public class GltfReader
    {
        public static Types.Gltf Read(Stream s)
        {
            using (var r = new JsonReader(s))
            {
                var node = r.Read();

                RepairKnownInvalidFormat(node);

                var jd = new JsonDeserializer(typeof(Types.Gltf));
                return (Types.Gltf)jd.Deserialize(node);
            }
        }

        public static void RepairKnownInvalidFormat(INode node)
        {
            RepairUniGLTFInvalidNamesForImages(node);
            RepairUniGLTFInvalidTargets(node);
        }

        public static void RepairUniGLTFInvalidNamesForImages(INode node)
        {
            // If (Root)["images"][i]["extra"]["name"] exists,
            // re-assign them to (Root)["images"][i]["name"].
            var images = node["images"] as ArrayNode;
            if (images == null || images.Elems == null)
            {
                return;
            }

            foreach (var image in images.Elems)
            {
                var tValue = image as ObjectNode;
                if (tValue == null)
                {
                    continue;
                }

                var name = image["extra"]["name"] as StringNode;
                if (name == null)
                {
                    continue;
                }

                var imageNameRaw = image["name"];
                if (!(imageNameRaw is UndefinedNode)) {
                    // Do not overwrite...
                    continue;
                }

                tValue.AddElement("name", name);
            }
        }

        public static void RepairUniGLTFInvalidTargets(INode node)
        {
            // If (Root)["meshes"][i]["primitives"][j]["targets"][k]["extra"]["name"] exists,
            // re-assign them to (Root)["meshes"][i]["primitives"]["extras"]["targetNames"].
            var meshes = node["meshes"] as ArrayNode;
            if (meshes == null || meshes.Elems == null)
            {
                return;
            }

            foreach (var mesh in meshes.Elems)
            {
                var primitives = mesh["primitives"] as ArrayNode;
                if (primitives == null || primitives.Elems == null)
                {
                    continue;
                }

                foreach (var primitive in primitives.Elems)
                {
                    var targets = primitive["targets"] as ArrayNode;
                    if (targets == null || targets.Elems == null)
                    {
                        continue;
                    }

                    var extrasRaw = primitive["extras"];
                    if (extrasRaw is UndefinedNode)
                    {
                        extrasRaw = new ObjectNode();
                        ((ObjectNode)primitive).AddElement("extras", extrasRaw);
                    }

                    ArrayNode targetNames = null;
                    var extras = extrasRaw as ObjectNode;
                    if (extras != null)
                    {
                        var targetNamesRaw = extras["targetNames"];
                        if (targetNamesRaw is UndefinedNode)
                        {
                            targetNamesRaw = new ArrayNode();
                            extras.AddElement("targetNames", targetNamesRaw);
                        }

                        targetNames = targetNamesRaw as ArrayNode;
                    }

                    if (targetNames == null)
                    {
                        continue;
                    }

                    var deletionKeys = new List<string>();
                    foreach (var target in targets.Elems)
                    {
                        var tValue = target as ObjectNode;
                        if (tValue == null || tValue.Elems == null)
                        {
                            continue;
                        }

                        foreach (var key in tValue.Elems.Keys)
                        {
                            // Invalid extension
                            if (key == "extra")
                            {
                                deletionKeys.Add(key);

                                var name = target[key]["name"] as StringNode;
                                if (name == null)
                                {
                                    continue;
                                }

                                // Re-assign to primitive.extras.targetName
                                targetNames.AddElement(name);
                            }

                            // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/mesh.primitive.schema.json#L72
                            // Only `POSITION`, `NORMAL`, and `TANGENT` supported
                            if (key != "POSITION" && key != "NORMAL" && key != "TANGENT")
                            {
                                deletionKeys.Add(key);
                            }
                        }

                        foreach(var key in deletionKeys)
                        {
                            tValue.Elems.Remove(key);
                        }
                    }
                }
            }
        }
    }
}
