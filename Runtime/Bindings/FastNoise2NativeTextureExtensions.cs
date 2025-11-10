using NativeTexture;

namespace FastNoise2.Bindings
{
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using static FastNoise;

    public static class FastNoise2NativeTextureExtensions
    {
        public static unsafe void GenUniformGrid2D<T>(
            this FastNoise fn,
            NativeTexture2D<T> nativeTexture,
            NativeReference<ValueBounds> boundsRef,
            int xStart,
            int yStart,
            int xSize,
            int ySize,
            float frequency,
            int seed
        )
            where T : unmanaged
        {
            boundsRef.Reset();

            fnGenUniformGrid2D(
                fn.mNodeHandle,
                nativeTexture.GetUnsafePtr(),
                xStart,
                yStart,
                xSize,
                ySize,
                frequency,
                seed,
                boundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenUniformGrid3D<T>(
            this FastNoise fn,
            NativeTexture3D<T> nativeTexture,
            NativeReference<ValueBounds> boundsRef,
            int xStart,
            int yStart,
            int zStart,
            int xSize,
            int ySize,
            int zSize,
            float frequency,
            int seed
        )
            where T : unmanaged
        {
            boundsRef.Reset();

            fnGenUniformGrid3D(
                fn.mNodeHandle,
                nativeTexture.GetUnsafePtr(),
                xStart,
                yStart,
                zStart,
                xSize,
                ySize,
                zSize,
                frequency,
                seed,
                boundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenTileable2D<T>(
            this FastNoise fn,
            NativeTexture2D<T> nativeTexture,
            NativeReference<ValueBounds> boundsRef,
            int xSize,
            int ySize,
            float frequency,
            int seed
        )
            where T : unmanaged
        {
            boundsRef.Reset();

            fnGenTileable2D(
                fn.mNodeHandle,
                nativeTexture.GetUnsafePtr(),
                xSize,
                ySize,
                frequency,
                seed,
                boundsRef.GetUnsafePtr()
            );
        }
    }
}
