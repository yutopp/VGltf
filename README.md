# VGltf 🗿

[![ci](https://github.com/yutopp/VGltf/actions/workflows/ci.yml/badge.svg)](https://github.com/yutopp/VGltf/actions/workflows/ci.yml)
[![NuGet Badge](https://buildstats.info/nuget/vgltf)](https://www.nuget.org/packages/VGltf/)
[![codecov](https://codecov.io/gh/yutopp/VGltf/branch/master/graph/badge.svg)](https://codecov.io/gh/yutopp/VGltf)
[![license](https://img.shields.io/github/license/yutopp/VGltf.svg)](https://github.com/yutopp/VGltf/blob/master/LICENSE_1_0.txt)
[![gltf-2.0](https://camo.githubusercontent.com/4a2bc1263a5da1ed3190e23186521ffd9a2d51b0/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f676c54462d32253245302d677265656e2e7376673f7374796c653d666c6174)](https://github.com/KhronosGroup/glTF/tree/master/specification/2.0)
![unity](https://img.shields.io/badge/unity-2019.4%2B-blue.svg)

![layer](/docs/layer.png)

> `VGltf` is a library importing/exporting `glTF 2.0` assets aiming for **extensibility**, **readability**, and **stability**.

Supported .NET versions are `.NET Standard 2.0` or higher.

The following glTF extensions are also supported as independent libraries.

- [VRM 0.x](https://github.com/vrm-c/vrm-specification)

## Description for Unity users

Supported Unity versions are `Unity 2019.4` or higher.

As for importing and exporting resources, you can use it in the following situations.

- [x] Runtime import
- [x] Runtime export
- [ ] Editor import
- [x] Editor export

## Installation

### For standard C# projects

You can use [Nuget/VGltf](https://www.nuget.org/packages/VGltf/).

```bash
dotnet add package VGltf
```

### For Unity projects

#### Nightly version

Add a url for VGltf git repository to your `Packages/manifest.json` like below.

```json
{
  "dependencies": {
    "net.yutopp.vgltf": "https://github.com/yutopp/VGltf.git?path=Packages/net.yutopp.vgltf",
    "net.yutopp.vgltf.unity": "https://github.com/yutopp/VGltf.git?path=Packages/net.yutopp.vgltf.unity"
  }
}
```

#### Stable version

Add scoped registry information shown below to your `Packages/manifest.json` if not exists.

```json
{
  "scopedRegistries": [
    {
      "name": "yutopp.net",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "net.yutopp"
      ]
    }
  ]
}
```

And add `net.yutopp.vgltf.*` to your `Packages/manifest.json` like below.

```json
{
  "dependencies": {
    "net.yutopp.vgltf": "*",
    "net.yutopp.vgltf.unity": "*"
  }
}
```

#### Dependencies

- [yutopp/VJson](https://github.com/yutopp/VJson)

## Usage examples

See [Assets/VGltfExamples](./Assets/VGltfExamples).

## TODO

- [ ] Performance tuning

## License

[Boost Software License - Version 1.0](./LICENSE_1_0.txt)

## References

- [glTF 2.0 Specification](https://github.com/KhronosGroup/glTF/tree/master/specification/2.0)

## Author

- [@yutopp](https://github.com/yutopp)
