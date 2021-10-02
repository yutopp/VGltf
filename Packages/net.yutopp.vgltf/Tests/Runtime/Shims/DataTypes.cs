//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

namespace VGltf.UnitTests.Shims
{
    // For testing purpose
    struct Vec2<T> where T : struct
    {
        T x;
        T y;

        public Vec2(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return string.Format("{{{0}, {1}}}", x, y);
        }
    }

    struct Vec3<T> where T : struct
    {
        T x;
        T y;
        T z;

        public Vec3(T x, T y, T z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return string.Format("{{{0}, {1}, {2}}}", x, y, z);
        }
    }

    struct Vec4<T> where T : struct
    {
        public T x;
        public T y;
        public T z;
        public T w;

        public Vec4(T x, T y, T z, T w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString()
        {
            return string.Format("{{{0}, {1}, {2}, {3}}}", x, y, z, w);
        }
    }
}
