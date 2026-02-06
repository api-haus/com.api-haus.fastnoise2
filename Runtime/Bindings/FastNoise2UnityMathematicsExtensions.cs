using static FastNoise2.Bindings.FastNoise;

namespace FastNoise2.Bindings
{
    public static class FastNoise2UnsafeExtensions
    {
        public static unsafe void GenUniformGrid2D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            float xOffset,
            float yOffset,
            int xCount,
            int yCount,
            float xStepSize,
            float yStepSize,
            int seed
        ) =>
            fnGenUniformGrid2D(
                fn.mNodeHandle,
                noiseOut,
                xOffset,
                yOffset,
                xCount,
                yCount,
                xStepSize,
                yStepSize,
                seed,
                outputMinMax
            );

        public static unsafe void GenUniformGrid3D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            float xOffset,
            float yOffset,
            float zOffset,
            int xCount,
            int yCount,
            int zCount,
            float xStepSize,
            float yStepSize,
            float zStepSize,
            int seed
        ) =>
            fnGenUniformGrid3D(
                fn.mNodeHandle,
                noiseOut,
                xOffset,
                yOffset,
                zOffset,
                xCount,
                yCount,
                zCount,
                xStepSize,
                yStepSize,
                zStepSize,
                seed,
                outputMinMax
            );

        public static unsafe void GenUniformGrid4D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            float xOffset,
            float yOffset,
            float zOffset,
            float wOffset,
            int xCount,
            int yCount,
            int zCount,
            int wCount,
            float xStepSize,
            float yStepSize,
            float zStepSize,
            float wStepSize,
            int seed
        ) =>
            fnGenUniformGrid4D(
                fn.mNodeHandle,
                noiseOut,
                xOffset,
                yOffset,
                zOffset,
                wOffset,
                xCount,
                yCount,
                zCount,
                wCount,
                xStepSize,
                yStepSize,
                zStepSize,
                wStepSize,
                seed,
                outputMinMax
            );

        public static unsafe void GenTileable2D(
            this FastNoise fn,
            void* noiseOut,
            void* outputMinMax,
            int xSize,
            int ySize,
            float xStepSize,
            float yStepSize,
            int seed
        ) =>
            fnGenTileable2D(
                fn.mNodeHandle,
                noiseOut,
                xSize,
                ySize,
                xStepSize,
                yStepSize,
                seed,
                outputMinMax
            );

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
