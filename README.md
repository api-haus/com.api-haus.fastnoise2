# FastNoise2 for Unity (UPM Package)

[FastNoise2 by Auburn](https://github.com/Auburn/FastNoise2) packaged for Unity Package Manager (UPM). This package provides efficient, high-performance noise generation capabilities directly integrated into Unity, leveraging native textures, Burst, Jobs, and IL2CPP compatibility.

---

## Features

- **NativeTextureXD<T>**: Efficient native texture wrappers for 2D and 3D textures.
- **Burst/Jobs/IL2CPP Compatible**: Optimized for Unity's high-performance systems.
- **Zero-Copy Texture Operations**: Directly operate on Unity's texture memory without additional copying overhead.
- **Noise Graph Editor Integration**: Visual editing and serialization of noise graphs within Unity.
- **Baked Noise Assets**: Precompute and store noise textures for quick previewing and runtime usage.

---

## Platform Support

- ✅ Linux x64, Android x64
- ✅ macOS Universal, macOS Intel
- ✅ Windows x64

---

## Getting Started

### Installation via Unity Package Manager (UPM)

1. Open Unity and navigate to `Window → Package Manager`.
2. Click the `+` button and select `Add package from git URL...`.
3. Paste the following URL and click `Add`:

```
https://github.com/api-haus/com.auburn.fastnoise2.git
```

Unity will automatically download and install the package.

---

## Usage Examples

### Generating 2D Noise Texture

```csharp
using UnityEngine;
using Unity.Collections;
using FastNoise2;
using FastNoise2.NativeTexture;

public class NoiseExample : MonoBehaviour
{
    public Texture2D texture;

    void Start()
    {
        var noise = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");
        using var nativeTexture = new NativeTexture2D<float>(texture, Allocator.TempJob);

        noise.GenUniformGrid2D(nativeTexture, 0, 0, texture.width, texture.height, 0.02f, 1337);
        nativeTexture.ApplyTo(texture);
    }
}
```

### Generating 3D Noise Texture

```csharp
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using FastNoise2;
using FastNoise2.NativeTexture;

public class Noise3DExample : MonoBehaviour
{
    public Texture3D texture3D;

    void Start()
    {
        var noise = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");
        int3 resolution = new int3(texture3D.width, texture3D.height, texture3D.depth);
        using var nativeTexture3D = new NativeTexture3D<float>(resolution, Allocator.TempJob);

        noise.GenUniformGrid3D(nativeTexture3D, 0, int3.zero, 0.02f);
        // Apply your 3D texture data as needed
    }
}
```

---

## Important Notes for macOS and Linux Users

### Handling Unsigned Libraries and Binaries

On macOS and Linux, you may encounter issues with unsigned native libraries. To resolve these issues, you can use the following commands:

**macOS:**

```bash
chmod +x path/to/library
xattr -dr com.apple.quarantine path/to/library
```

**Linux:**

```bash
chmod +x path/to/library
```

Replace `path/to/library` with the actual path to the native library file.

### ⚠️ Disclaimer

> **Perform these actions at your own risk.** Modifying file permissions and removing security attributes can expose your system to potential security risks. Ensure you trust the source of the libraries before proceeding. There is always a risk of repository being overtaken by malicious actor.

---

## Roadmap / TODO

- [ ] Update to NewFastSIMD branch
- [ ] Complete Noise Editor integration (serialization to/from Noise Tool)
- [ ] Support for all texture dimensions and job types
- [ ] Comprehensive documentation and additional examples
- [ ] macOS CodeSigning
- [ ] Continuous Integration (CI), Semantic Versioning (SemVer), and OpenUPM support

---

## Contributing

Contributions are welcome! Please submit pull requests or open issues on the [GitHub repository](https://github.com/Auburn/FastNoise2).

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
