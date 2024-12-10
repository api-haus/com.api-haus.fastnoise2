> [FastNoise2 by Auburn](https://github.com/Auburn/FastNoise2) packaged in UPM

Platform support:

* Linux x64, Android x64
* macos Universal, macos Intel
* Windows x64

Feature support:

* `NativeTextureXD<T>`
* Burst/Jobs/IL2CPP compatible FastNoise2 bindings
* Ability to operate directly on Unity's texture memory (zero copying between NativeTexture/Texture2D)
* Noise Graphs Editor support - in properties, scriptable objects, etc.
* Baked Noise Assets - useful when previewing noise in editor, or generating a hefty 3D texture for fun

TODO:

* Complete Noise Editor integration - serializing to/from Noise Tool
* All Texture dimensions, All Job types
* Documentation, examples. See [Tests](./Tests) for usage examples.
* NewSIMD branch migration
* MacOS CodeSigning
* CI, SemVer, OpenUPM
