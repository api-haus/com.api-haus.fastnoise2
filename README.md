# FastNoise2 for Unity (UPM Package)

[FastNoise2 by Auburn](https://github.com/Auburn/FastNoise2) packaged for Unity Package Manager (UPM). This package provides efficient, high-performance noise generation capabilities directly integrated into Unity, leveraging native textures, Burst, Jobs, and IL2CPP compatibility.

---

## Features

- **Native Texture Support**:
  - `NativeTexture2D<T>` and `NativeTexture3D<T>`: Efficient native texture wrappers for 2D and 3D textures
  - Zero-copy operations with Unity textures for maximum performance
  - Support for multiple texture formats (byte2/3/4, ushort2/3/4, float)
  - Value bounds and normalization utilities

- **Comprehensive Sampling Extensions**:
  - Bilinear sampling for smooth interpolation
  - Read pixel functionality for precise texture access
  - Normalized sampling with configurable value ranges

- **Job System Integration**:
  - Full Burst/Jobs/IL2CPP compatibility
  - Specialized noise generation jobs:
    - Uniform grid generation (2D/3D)
    - Tileable noise generation
    - Texture normalization jobs
  
- **Authoring Tools**:
  - `BakedNoiseTextureAsset` for creating and configuring baked noise textures
  - `FastNoiseGraph` for storing and serializing noise node configurations
  - Integration with FastNoise Tool for visual noise design

- **Editor Extensions**:
  - Property drawers for noise graph assets
  - Menu items for launching the FastNoise Tool
  - Custom editors for noise assets

- **Cross-Platform Native Support**:
  - Optimized native libraries for Windows, macOS, and Linux
  - Universal architecture support (x64, arm64)

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

### Using Jobs for Noise Generation

```csharp
public class NoiseJobsExample : MonoBehaviour
{
    void Start()
    {
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			Texture2D texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> noiseTexture2D = new NativeTexture2D<float>(512, Allocator.TempJob);

			// Create a bounds reference for tracking min/max values
			using NativeReference<ValueBounds> boundsRef = new NativeReference<ValueBounds>(Allocator.Temp);

			// Generate noise directly into the native texture with built-in bounds tracking
			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				boundsRef,
				0, 0,
				noiseTexture2D.Width, noiseTexture2D.Height,
				0.02f, 1337);

			// Log the bounds for verification
			Debug.Log($"Noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNative.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
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
- [ ] Support for all texture dimensions (4D) and job types
- [ ] Comprehensive documentation and additional examples
- [ ] macOS CodeSigning
- [ ] Continuous Integration (CI), Semantic Versioning (SemVer), and OpenUPM support

---

## Contributing

Contributions are welcome! Please submit pull requests or open issues on the [GitHub repository](https://github.com/Auburn/FastNoise2).

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
