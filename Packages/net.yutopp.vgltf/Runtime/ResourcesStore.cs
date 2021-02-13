//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;

namespace VGltf
{
    // TODO: Rename...
    public class ResourcesStore
    {
        // TODO: Consider concurrent accesses
        private Dictionary<int, Resource> _bufferResources = new Dictionary<int, Resource>();
        private Dictionary<int, Resource> _imageResources = new Dictionary<int, Resource>();

        public Types.Gltf Gltf { get; private set; }
        public Glb.StoredBuffer Buffer { get; private set; }

        public IResourceLoader Loader { get; private set; }

        public ResourcesStore(Types.Gltf gltf, Glb.StoredBuffer buffer, IResourceLoader loader)
        {
            Gltf = gltf;
            Buffer = buffer;

            Loader = loader;
        }

        // TODO: Provide async version....
        public Resource GetOrLoadBufferResourceAt(int index)
        {
            Resource r;
            if (GetBufferResourceAt(index, out r))
            {
                return r;
            }

            var buffer = Gltf.Buffers[index];
            if (buffer.Uri != null)
            {
                r = Loader.Load(buffer.Uri);

                // TODO: Check length
                r.Data = new ArraySegment<byte>(r.Data.Array, r.Data.Offset, buffer.ByteLength);
            }
            else
            {
                // References binaryBuffer.
                if (index != 0)
                {
                    throw new InvalidOperationException("When referencing binaryBuffer, index must be 0");
                }

                if (Buffer == null)
                {
                    throw new InvalidOperationException("GLB stored buffer is null");
                }

                r = new Resource
                {
                    // TODO: Check length
                    Data = new ArraySegment<byte>(Buffer.Payload.Array, Buffer.Payload.Offset, buffer.ByteLength),
                };
            }

            _bufferResources.Add(index, r);

            return r;
        }

        public bool GetBufferResourceAt(int index, out Resource resource)
        {
            return _bufferResources.TryGetValue(index, out resource);
        }

        public Resource GetOrLoadBufferViewResourceAt(int index)
        {
            var bufferView = Gltf.BufferViews[index];
            var bufferResource = GetOrLoadBufferResourceAt(bufferView.Buffer);

            var data = new ArraySegment<byte>(
                bufferResource.Data.Array,
                bufferResource.Data.Offset + bufferView.ByteOffset,
                bufferView.ByteLength
                ); // TODO: Check length
            return new Resource
            {
                Data = data,
            };
        }

        // TODO: Provide async version....
        public Resource GetOrLoadImageResourceAt(int index)
        {
            Resource r;
            if (GetImageResourceAt(index, out r))
            {
                return r;
            }

            var image = Gltf.Images[index];
            if (image.Uri != null)
            {
                r = Loader.Load(image.Uri);
            }
            else
            {
                r = GetOrLoadBufferViewResourceAt(image.BufferView.Value);
            }

            _imageResources.Add(index, r);

            return r;
        }

        public bool GetImageResourceAt(int index, out Resource resource)
        {
            return _imageResources.TryGetValue(index, out resource);
        }

        public TypedBuffer GetOrLoadTypedBufferByAccessorIndex(int index)
        {
            var accessor = Gltf.Accessors[index];
            return new TypedBuffer(this, accessor);
        }
    }
}
