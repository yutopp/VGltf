//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;

namespace VGltf
{
    public class Resource
    {
        // TODO
        // public string MimeType;
        // public string Charset;

        public ArraySegment<byte> Data;
    }

    public interface IResourceLoader
    {
        Resource Load(string uri);
        string PathOf(string uri);
    }

    public class ResourceLoaderFromFileStorage : IResourceLoader
    {
        private string _baseDir;

        public ResourceLoaderFromFileStorage(string baseDir)
        {
            _baseDir = baseDir;
        }

        public Resource Load(string uri)
        {
            if (DataUriUtil.IsData(uri))
            {
                return DataUriUtil.Extract(uri);
            }

            return LoadFromFile(_baseDir, uri);
        }

        public string PathOf(string uri)
        {
            throw new NotImplementedException(uri);
        }

        public static string EnsureCleanedPath(string baseDir, string uri)
        {
            var combined = Path.Combine(baseDir, uri);
            var fullPath = Path.GetFullPath(combined);
            if (!fullPath.StartsWith(baseDir))
            {
                throw new ArgumentException("Path must be a child of baseDir: Uri = " + uri);
            }

            return fullPath;
        }

        public static Resource LoadFromFile(string baseDir, string uri)
        {
            var path = EnsureCleanedPath(baseDir, uri);

            var data = File.ReadAllBytes(path);
            return new Resource
            {
                Data = new ArraySegment<byte>(data),
            };
        }
    }

    public class ResourceLoaderFromEmbedOnly : IResourceLoader
    {
        public Resource Load(string uri)
        {
            if (!DataUriUtil.IsData(uri))
            {
                throw new InvalidOperationException("uri is not data form");
            }

            return DataUriUtil.Extract(uri);
        }

        public string PathOf(string uri)
        {
            throw new NotImplementedException(uri);
        }
    }

    public class DataUriUtil
    {
        public static bool IsData(string uri)
        {
            return uri.StartsWith("data:");
        }

        public static Resource Extract(string uri)
        {
            // TODO: Read MIME-type
            // TODO: Read Charset
            // TODO: Read Base64 specifier

            var delimPos = uri.IndexOf(',');
            if (delimPos == -1)
            {
                throw new ArgumentException("Invalid DataURI format (',' is missing)");
            }

            return new Resource
            {
                Data = new ArraySegment<byte>(Convert.FromBase64String(uri.Substring(delimPos + 1))),
            };
        }
    }
}
