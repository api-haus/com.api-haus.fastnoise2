using static FastNoise2.Bindings.FastNoise;

namespace FastNoise2.Bindings
{
    public static class FastNoise2UnsafeExtensions
    {
        public static unsafe void GenUniformGrid2D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int xStart,
            int yStart,
            int xSize,
            int ySize,
            float frequency,
            int seed
        ) =>
            fnGenUniformGrid2D(
                fn.mNodeHandle,
                noiseOut,
                xStart,
                yStart,
                xSize,
                ySize,
                seed,
                outputMinMax
            );

        public static unsafe void GenUniformGrid3D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int xStart,
            int yStart,
            int zStart,
            int xSize,
            int ySize,
            int zSize,
            float frequency,
            int seed
        ) =>
            fnGenUniformGrid3D(
                fn.mNodeHandle,
                noiseOut,
                xStart,
                yStart,
                zStart,
                xSize,
                ySize,
                zSize,
                seed,
                outputMinMax
            );

        public static unsafe void GenUniformGrid4D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int xStart,
            int yStart,
            int zStart,
            int wStart,
            int xSize,
            int ySize,
            int zSize,
            int wSize,
            float frequency,
            int seed
        ) =>
            fnGenUniformGrid4D(
                fn.mNodeHandle,
                noiseOut,
                xStart,
                yStart,
                zStart,
                wStart,
                xSize,
                ySize,
                zSize,
                wSize,
                seed,
                outputMinMax
            );

        public static unsafe void GenTileable2D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int xSize,
            int ySize,
            float frequency,
            int seed
        ) => fnGenTileable2D(fn.mNodeHandle, noiseOut, xSize, ySize, seed, outputMinMax);

        public static unsafe void GenPositionArray2D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int positionCount,
            void* xPosArray,
            void* yPosArray,
            float xOffset,
            float yOffset,
            int seed
        ) =>
            fnGenPositionArray2D(
                fn.mNodeHandle,
                noiseOut,
                positionCount,
                xPosArray,
                yPosArray,
                xOffset,
                yOffset,
                seed,
                outputMinMax
            );

        public static unsafe void GenPositionArray3D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int positionCount,
            void* xPosArray,
            void* yPosArray,
            void* zPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            int seed
        ) =>
            fnGenPositionArray3D(
                fn.mNodeHandle,
                noiseOut,
                positionCount,
                xPosArray,
                yPosArray,
                zPosArray,
                xOffset,
                yOffset,
                zOffset,
                seed,
                outputMinMax
            );

        public static unsafe void GenPositionArray4D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int positionCount,
            void* xPosArray,
            void* yPosArray,
            void* zPosArray,
            void* wPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            float wOffset,
            int seed
        ) =>
            fnGenPositionArray4D(
                fn.mNodeHandle,
                noiseOut,
                positionCount,
                xPosArray,
                yPosArray,
                zPosArray,
                wPosArray,
                xOffset,
                yOffset,
                zOffset,
                wOffset,
                seed,
                outputMinMax
            );
    }
}
