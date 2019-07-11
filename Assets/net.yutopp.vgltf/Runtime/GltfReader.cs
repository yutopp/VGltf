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
using VJson.Schema;

namespace VGltf
{
    public class GltfReader
    {
        public static Types.Gltf Read(Stream s, bool withRepairment = true, bool withValidation = true)
        {
            using (var r = new JsonReader(s))
            {
                var node = r.Read();

                if (withRepairment)
                {
                    RepairKnownInvalidFormat(node);
                }

                var jd = new JsonDeserializer(typeof(Types.Gltf));
                var gltf = (Types.Gltf)jd.DeserializeFromNode(node);

                if (withValidation)
                {
                    var schema = JsonSchemaAttribute.CreateFromClass<Types.Gltf>();
                    var ex = schema.Validate(gltf);
                    if (ex != null)
                    {
                        throw ex;
                    }
                }

                return gltf;
            }
        }

        public static Types.Gltf ReadWithoutValidation(Stream s, bool withRepairment = true)
        {
            return Read(s, withRepairment, false);
        }

        public static void RepairKnownInvalidFormat(INode node)
        {
            RepairUniGLTFInvalidNamesForImages(node);
            RepairUniGLTFInvalidTargets(node);
            RepairUniGLTFInvalidIndexValues(node);
            RepairUniGLTFInvalidScene(node);
        }

        public static void RepairUniGLTFInvalidNamesForImages(INode node)
        {
            // If (Root)["images"][i]["extra"]["name"] exists,
            // re-assign them to (Root)["images"][i]["name"].
            var images = node["images"] as ArrayNode;
            if (images == null)
            {
                return;
            }

            foreach (var image in images)
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
                if (!(imageNameRaw is UndefinedNode))
                {
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
            if (meshes == null)
            {
                return;
            }

            foreach (var mesh in meshes)
            {
                var primitives = mesh["primitives"] as ArrayNode;
                if (primitives == null)
                {
                    continue;
                }

                foreach (var primitive in primitives)
                {
                    var targets = primitive["targets"] as ArrayNode;
                    if (targets == null)
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


                    foreach (var target in targets)
                    {
                        var tValue = target as ObjectNode;
                        if (tValue == null)
                        {
                            continue;
                        }

                        var deletionKeys = new List<string>();
                        foreach (var kv in tValue)
                        {
                            var key = kv.Key;

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

                        foreach (var key in deletionKeys)
                        {
                            tValue.RemoveElement(key);
                        }
                    }
                }
            }
        }

        public static void RepairUniGLTFInvalidIndexValues(INode node)
        {
            // TODO: Support animations
            RepairUniGLTFInvalidIndexValuesForMeshes(node);
        }

        public static void RepairUniGLTFInvalidIndexValuesForMeshes(INode node)
        {
            var meshes = node["meshes"] as ArrayNode;
            if (meshes == null)
            {
                return;
            }

            foreach (var mesh in meshes)
            {
                var primitives = mesh["primitives"] as ArrayNode;
                if (primitives == null)
                {
                    continue;
                }

                foreach (var primitive in primitives)
                {
                    var tPrimitive = primitive as ObjectNode;
                    if (tPrimitive == null)
                    {
                        continue;
                    }

                    var indices = primitive["indices"] as IntegerNode;
                    if (indices != null && indices.Value == -1)
                    {
                        tPrimitive.RemoveElement("indices");
                    }

                    var material = primitive["material"] as IntegerNode;
                    if (material != null && material.Value == -1)
                    {
                        tPrimitive.RemoveElement("material");
                    }

                    var mode = primitive["mode"] as IntegerNode;
                    if (mode != null && mode.Value == -1)
                    {
                        tPrimitive.RemoveElement("mode");
                    }

                    var targets = primitive["targets"] as ArrayNode;
                    if (targets == null)
                    {
                        continue;
                    }

                    foreach (var target in targets)
                    {
                        var tTarget = target as ObjectNode;
                        if (tTarget == null)
                        {
                            continue;
                        }

                        var deletionKeys = new List<string>();
                        foreach (var kv in tTarget)
                        {
                            var index = kv.Value as IntegerNode;
                            if (index != null && index.Value == -1)
                            {
                                deletionKeys.Add(kv.Key);
                            }
                        }

                        foreach (var key in deletionKeys)
                        {
                            tTarget.RemoveElement(key);
                        }
                    }
                }
            }
        }

        public static void RepairUniGLTFInvalidScene(INode node)
        {
            var tNode = node as ObjectNode;
            if (tNode == null)
            {
                return;
            }

            var generator = node["asset"]["generator"] as StringNode;
            if (generator == null)
            {
                return;
            }

            if (generator.Value != "UniGLTF")
            {
                return;
            }

            // If `scene` is not defined, it should be treated as `0` in UniGLTF.
            var scene = node["scene"];
            if (scene is UndefinedNode)
            {
                tNode.AddElement("scene", new IntegerNode(0));
            }
        }
    }
}
