namespace FastNoise2.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Jobs;
    using Unity.Mathematics;

    public struct FastNoise : INativeDisposable, IEquatable<FastNoise>
    {
        #region IEquatable

        public readonly override int GetHashCode() => HashCode.Combine(mNodeHandle, m_MetadataId);

        public readonly bool Equals(FastNoise other) =>
            mNodeHandle == other.mNodeHandle && m_MetadataId == other.m_MetadataId;

        public override readonly bool Equals(object obj) =>
            obj != null && obj is FastNoise other && Equals(other);

        public static bool operator ==(FastNoise left, FastNoise right) => left.Equals(right);

        public static bool operator !=(FastNoise left, FastNoise right) => !left.Equals(right);

        #endregion

        public static FastNoise Invalid = new() { mNodeHandle = IntPtr.Zero, m_MetadataId = -1 };

        [StructLayout(LayoutKind.Sequential)]
        public struct OutputMinMax
        {
            public OutputMinMax(
                float minValue = float.PositiveInfinity,
                float maxValue = float.NegativeInfinity
            )
            {
                min = minValue;
                max = maxValue;
            }

            public OutputMinMax(float[] nativeOutputMinMax)
            {
                min = nativeOutputMinMax[0];
                max = nativeOutputMinMax[1];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Merge(OutputMinMax other)
            {
                min = math.min(min, other.min);
                max = math.max(max, other.max);
            }

            public float min;
            public float max;
        }

        public FastNoise(string metadataName)
        {
            if (!s_metadataNameLookup.TryGetValue(FormatLookup(metadataName), out m_MetadataId))
            {
                throw new ArgumentException("Failed to find metadata name: " + metadataName);
            }

            m_IsDisposed = false;
            mNodeHandle = fnNewFromMetadata(m_MetadataId);
        }

        private FastNoise(IntPtr nodeHandle)
        {
            mNodeHandle = nodeHandle;
            m_IsDisposed = false;
            m_MetadataId = fnGetMetadataID(nodeHandle);
        }

        private struct DisposeNoiseJob : IJob
        {
            private FastNoise m_Noise;

            public DisposeNoiseJob(FastNoise fastNoise)
            {
                m_Noise = fastNoise;
            }

            public void Execute() => m_Noise.Dispose();
        }

        public void Dispose()
        {
            if (m_IsDisposed || mNodeHandle == IntPtr.Zero || this == Invalid)
            {
                return;
            }

            fnDeleteNodeRef(mNodeHandle);
            m_IsDisposed = true;
        }

        public JobHandle Dispose(JobHandle inputDeps) =>
            IsCreated //
                ? new DisposeNoiseJob(this).Schedule(inputDeps)
                : inputDeps;

        public static FastNoise FromEncodedNodeTree(string encodedNodeTree)
        {
            IntPtr nodeHandle = fnNewFromEncodedNodeTree(encodedNodeTree);

            if (nodeHandle == IntPtr.Zero)
            {
                return Invalid;
            }

            return new FastNoise(nodeHandle);
        }

        public readonly uint GetSIMDLevel() => fnGetSIMDLevel(mNodeHandle);

        public readonly void Set(string memberName, float value)
        {
            Metadata.Member member;
            if (
                !s_nodeMetadata[m_MetadataId]
                    .members.TryGetValue(FormatLookup(memberName), out member)
            )
            {
                throw new ArgumentException("Failed to find member name: " + memberName);
            }

            switch (member.type)
            {
                case Metadata.Member.Type.Float:
                    if (!fnSetVariableFloat(mNodeHandle, member.index, value))
                    {
                        throw new ExternalException("Failed to set float value");
                    }

                    break;

                case Metadata.Member.Type.Hybrid:
                    if (!fnSetHybridFloat(mNodeHandle, member.index, value))
                    {
                        throw new ExternalException("Failed to set float value");
                    }

                    break;

                default:
                    throw new ArgumentException(memberName + " cannot be set to a float value");
            }
        }

        public readonly void Set(string memberName, int value)
        {
            Metadata.Member member;
            if (
                !s_nodeMetadata[m_MetadataId]
                    .members.TryGetValue(FormatLookup(memberName), out member)
            )
            {
                throw new ArgumentException("Failed to find member name: " + memberName);
            }

            if (member.type != Metadata.Member.Type.Int)
            {
                throw new ArgumentException(memberName + " cannot be set to an int value");
            }

            if (!fnSetVariableIntEnum(mNodeHandle, member.index, value))
            {
                throw new ExternalException("Failed to set int value");
            }
        }

        public readonly void Set(string memberName, string enumValue)
        {
            Metadata.Member member;
            if (
                !s_nodeMetadata[m_MetadataId]
                    .members.TryGetValue(FormatLookup(memberName), out member)
            )
            {
                throw new ArgumentException("Failed to find member name: " + memberName);
            }

            if (member.type != Metadata.Member.Type.Enum)
            {
                throw new ArgumentException(memberName + " cannot be set to an enum value");
            }

            int enumIdx;
            if (!member.enumNames.TryGetValue(FormatLookup(enumValue), out enumIdx))
            {
                throw new ArgumentException("Failed to find enum value: " + enumValue);
            }

            if (!fnSetVariableIntEnum(mNodeHandle, member.index, enumIdx))
            {
                throw new ExternalException("Failed to set enum value");
            }
        }

        public readonly void Set(string memberName, FastNoise nodeLookup)
        {
            Metadata.Member member;
            if (
                !s_nodeMetadata[m_MetadataId]
                    .members.TryGetValue(FormatLookup(memberName), out member)
            )
            {
                throw new ArgumentException("Failed to find member name: " + memberName);
            }

            switch (member.type)
            {
                case Metadata.Member.Type.NodeLookup:
                    if (!fnSetNodeLookup(mNodeHandle, member.index, nodeLookup.mNodeHandle))
                    {
                        throw new ExternalException("Failed to set node lookup");
                    }

                    break;

                case Metadata.Member.Type.Hybrid:
                    if (!fnSetHybridNodeLookup(mNodeHandle, member.index, nodeLookup.mNodeHandle))
                    {
                        throw new ExternalException("Failed to set node lookup");
                    }

                    break;

                default:
                    throw new ArgumentException(memberName + " cannot be set to a node lookup");
            }
        }

        public readonly OutputMinMax GenUniformGrid2D(
            float[] noiseOut,
            int xStart,
            int yStart,
            int xSize,
            int ySize,
            float frequency,
            int seed
        )
        {
            float[] minMax = new float[2];
            fnGenUniformGrid2D(mNodeHandle, noiseOut, xStart, yStart, xSize, ySize, seed, minMax);
            return new OutputMinMax(minMax);
        }

        public readonly OutputMinMax GenUniformGrid3D(
            float[] noiseOut,
            int xStart,
            int yStart,
            int zStart,
            int xSize,
            int ySize,
            int zSize,
            float frequency,
            int seed
        )
        {
            float[] minMax = new float[2];
            fnGenUniformGrid3D(
                mNodeHandle,
                noiseOut,
                xStart,
                yStart,
                zStart,
                xSize,
                ySize,
                zSize,
                seed,
                minMax
            );
            return new OutputMinMax(minMax);
        }

        public readonly OutputMinMax GenUniformGrid4D(
            float[] noiseOut,
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
        )
        {
            float[] minMax = new float[2];
            fnGenUniformGrid4D(
                mNodeHandle,
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
                minMax
            );
            return new OutputMinMax(minMax);
        }

        public readonly OutputMinMax GenTileable2D(
            float[] noiseOut,
            int xSize,
            int ySize,
            float frequency,
            int seed
        )
        {
            float[] minMax = new float[2];
            fnGenTileable2D(mNodeHandle, noiseOut, xSize, ySize, seed, minMax);
            return new OutputMinMax(minMax);
        }

        public readonly OutputMinMax GenPositionArray2D(
            float[] noiseOut,
            float[] xPosArray,
            float[] yPosArray,
            float xOffset,
            float yOffset,
            int seed
        )
        {
            float[] minMax = new float[2];
            fnGenPositionArray2D(
                mNodeHandle,
                noiseOut,
                xPosArray.Length,
                xPosArray,
                yPosArray,
                xOffset,
                yOffset,
                seed,
                minMax
            );
            return new OutputMinMax(minMax);
        }

        public readonly OutputMinMax GenPositionArray3D(
            float[] noiseOut,
            float[] xPosArray,
            float[] yPosArray,
            float[] zPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            int seed
        )
        {
            float[] minMax = new float[2];
            fnGenPositionArray3D(
                mNodeHandle,
                noiseOut,
                xPosArray.Length,
                xPosArray,
                yPosArray,
                zPosArray,
                xOffset,
                yOffset,
                zOffset,
                seed,
                minMax
            );
            return new OutputMinMax(minMax);
        }

        public readonly OutputMinMax GenPositionArray4D(
            float[] noiseOut,
            float[] xPosArray,
            float[] yPosArray,
            float[] zPosArray,
            float[] wPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            float wOffset,
            int seed
        )
        {
            float[] minMax = new float[2];
            fnGenPositionArray4D(
                mNodeHandle,
                noiseOut,
                xPosArray.Length,
                xPosArray,
                yPosArray,
                zPosArray,
                wPosArray,
                xOffset,
                yOffset,
                zOffset,
                wOffset,
                seed,
                minMax
            );
            return new OutputMinMax(minMax);
        }

        public readonly float GenSingle2D(float x, float y, int seed) =>
            fnGenSingle2D(mNodeHandle, x, y, seed);

        public readonly float GenSingle3D(float x, float y, float z, int seed) =>
            fnGenSingle3D(mNodeHandle, x, y, z, seed);

        public readonly float GenSingle4D(float x, float y, float z, float w, int seed) =>
            fnGenSingle4D(mNodeHandle, x, y, z, w, seed);

        [NativeDisableUnsafePtrRestriction]
        internal IntPtr mNodeHandle;

        private int m_MetadataId;

        public class Metadata
        {
            public int id;
            public Dictionary<string, Member> members;
            public string name;

            public struct Member
            {
                public enum Type
                {
                    Float,
                    Int,
                    Enum,
                    NodeLookup,
                    Hybrid,
                }

                public string name;
                public Type type;
                public int index;
                public Dictionary<string, int> enumNames;
            }
        }

        static FastNoise()
        {
            int metadataCount = fnGetMetadataCount();

            s_nodeMetadata = new Metadata[metadataCount];
            s_metadataNameLookup = new Dictionary<string, int>(metadataCount);

            // Collect metadata for all FastNoise node classes
            for (int id = 0; id < metadataCount; id++)
            {
                Metadata metadata = new();

                metadata.id = id;
                metadata.name = FormatLookup(Marshal.PtrToStringAnsi(fnGetMetadataName(id)));
                //Console.WriteLine(id + " - " + metadata.name);
                s_metadataNameLookup.Add(metadata.name, id);

                int variableCount = fnGetMetadataVariableCount(id);
                int nodeLookupCount = fnGetMetadataNodeLookupCount(id);
                int hybridCount = fnGetMetadataHybridCount(id);
                metadata.members = new Dictionary<string, Metadata.Member>(
                    variableCount + nodeLookupCount + hybridCount
                );

                // Init variables
                for (int variableIdx = 0; variableIdx < variableCount; variableIdx++)
                {
                    Metadata.Member member = new();

                    member.name = FormatLookup(
                        Marshal.PtrToStringAnsi(fnGetMetadataVariableName(id, variableIdx))
                    );
                    member.type = (Metadata.Member.Type)fnGetMetadataVariableType(id, variableIdx);
                    member.index = variableIdx;

                    member.name = FormatDimensionMember(
                        member.name,
                        fnGetMetadataVariableDimensionIdx(id, variableIdx)
                    );

                    // Get enum names
                    if (member.type == Metadata.Member.Type.Enum)
                    {
                        int enumCount = fnGetMetadataEnumCount(id, variableIdx);
                        member.enumNames = new Dictionary<string, int>(enumCount);

                        for (int enumIdx = 0; enumIdx < enumCount; enumIdx++)
                        {
                            member.enumNames.Add(
                                FormatLookup(
                                    Marshal.PtrToStringAnsi(
                                        fnGetMetadataEnumName(id, variableIdx, enumIdx)
                                    )
                                ),
                                enumIdx
                            );
                        }
                    }

                    metadata.members.Add(member.name, member);
                }

                // Init node lookups
                for (int nodeLookupIdx = 0; nodeLookupIdx < nodeLookupCount; nodeLookupIdx++)
                {
                    Metadata.Member member = new();

                    member.name = FormatLookup(
                        Marshal.PtrToStringAnsi(fnGetMetadataNodeLookupName(id, nodeLookupIdx))
                    );
                    member.type = Metadata.Member.Type.NodeLookup;
                    member.index = nodeLookupIdx;

                    member.name = FormatDimensionMember(
                        member.name,
                        fnGetMetadataNodeLookupDimensionIdx(id, nodeLookupIdx)
                    );

                    metadata.members.Add(member.name, member);
                }

                // Init hybrids
                for (int hybridIdx = 0; hybridIdx < hybridCount; hybridIdx++)
                {
                    Metadata.Member member = new();

                    member.name = FormatLookup(
                        Marshal.PtrToStringAnsi(fnGetMetadataHybridName(id, hybridIdx))
                    );
                    member.type = Metadata.Member.Type.Hybrid;
                    member.index = hybridIdx;

                    member.name = FormatDimensionMember(
                        member.name,
                        fnGetMetadataHybridDimensionIdx(id, hybridIdx)
                    );

                    metadata.members.Add(member.name, member);
                }

                s_nodeMetadata[id] = metadata;
            }
        }

        // Append dimension char where neccessary
        private static string FormatDimensionMember(string name, int dimIdx)
        {
            if (dimIdx >= 0)
            {
                char[] dimSuffix = { 'x', 'y', 'z', 'w' };
                name += dimSuffix[dimIdx];
            }

            return name;
        }

        // Ignores spaces and caps, harder to mistype strings
        private static string FormatLookup(string s) => s.Replace(" ", "").ToLower();

        private static readonly Dictionary<string, int> s_metadataNameLookup;
        private static readonly Metadata[] s_nodeMetadata;

#if UNITY_IOS && !UNITY_EDITOR
        // On iOS plugins are statically linked into
        // the executable, so we have to use __Internal as the
        // library name.
        private const string NATIVE_LIB = "__Internal";
#else
        private const string NATIVE_LIB = "FastNoise";
#endif

        [DllImport(NATIVE_LIB)]
        internal static extern IntPtr fnNewFromMetadata(int id, uint simdLevel = 0);

        [DllImport(NATIVE_LIB)]
        internal static extern IntPtr fnNewFromEncodedNodeTree(
            [MarshalAs(UnmanagedType.LPStr)] string encodedNodeTree,
            uint simdLevel = 0
        );

        [DllImport(NATIVE_LIB)]
        internal static extern void fnDeleteNodeRef(IntPtr nodeHandle);

        [DllImport(NATIVE_LIB)]
        internal static extern uint fnGetSIMDLevel(IntPtr nodeHandle);

        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataID(IntPtr nodeHandle);

        [DllImport(NATIVE_LIB)]
        internal static extern void fnGenUniformGrid2D(
            IntPtr nodeHandle,
            float[] noiseOut,
            int xStart,
            int yStart,
            int xSize,
            int ySize,
            int seed,
            float[] outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern void fnGenUniformGrid3D(
            IntPtr nodeHandle,
            float[] noiseOut,
            int xStart,
            int yStart,
            int zStart,
            int xSize,
            int ySize,
            int zSize,
            int seed,
            float[] outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern void fnGenUniformGrid4D(
            IntPtr nodeHandle,
            float[] noiseOut,
            int xStart,
            int yStart,
            int zStart,
            int wStart,
            int xSize,
            int ySize,
            int zSize,
            int wSize,
            int seed,
            float[] outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern void fnGenTileable2D(
            IntPtr node,
            float[] noiseOut,
            int xSize,
            int ySize,
            int seed,
            float[] outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern void fnGenPositionArray2D(
            IntPtr node,
            float[] noiseOut,
            int count,
            float[] xPosArray,
            float[] yPosArray,
            float xOffset,
            float yOffset,
            int seed,
            float[] outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern void fnGenPositionArray3D(
            IntPtr node,
            float[] noiseOut,
            int count,
            float[] xPosArray,
            float[] yPosArray,
            float[] zPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            int seed,
            float[] outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern void fnGenPositionArray4D(
            IntPtr node,
            float[] noiseOut,
            int count,
            float[] xPosArray,
            float[] yPosArray,
            float[] zPosArray,
            float[] wPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            float wOffset,
            int seed,
            float[] outputMinMax
        );

        #region Unsafe versions

        [DllImport(NATIVE_LIB)]
        internal static extern unsafe void fnGenUniformGrid2D(
            IntPtr nodeHandle,
            void* noiseOut,
            int xStart,
            int yStart,
            int xSize,
            int ySize,
            int seed,
            void* outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern unsafe void fnGenUniformGrid3D(
            IntPtr nodeHandle,
            void* noiseOut,
            int xStart,
            int yStart,
            int zStart,
            int xSize,
            int ySize,
            int zSize,
            int seed,
            void* outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern unsafe void fnGenUniformGrid4D(
            IntPtr nodeHandle,
            void* noiseOut,
            int xStart,
            int yStart,
            int zStart,
            int wStart,
            int xSize,
            int ySize,
            int zSize,
            int wSize,
            int seed,
            void* outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern unsafe void fnGenTileable2D(
            IntPtr node,
            void* noiseOut,
            int xSize,
            int ySize,
            int seed,
            void* outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern unsafe void fnGenPositionArray2D(
            IntPtr node,
            void* noiseOut,
            int count,
            void* xPosArray,
            void* yPosArray,
            float xOffset,
            float yOffset,
            int seed,
            void* outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern unsafe void fnGenPositionArray3D(
            IntPtr node,
            void* noiseOut,
            int count,
            void* xPosArray,
            void* yPosArray,
            void* zPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            int seed,
            void* outputMinMax
        );

        [DllImport(NATIVE_LIB)]
        internal static extern unsafe void fnGenPositionArray4D(
            IntPtr node,
            void* noiseOut,
            int count,
            void* xPosArray,
            void* yPosArray,
            void* zPosArray,
            void* wPosArray,
            float xOffset,
            float yOffset,
            float zOffset,
            float wOffset,
            int seed,
            void* outputMinMax
        );

        #endregion

        [DllImport(NATIVE_LIB)]
        internal static extern float fnGenSingle2D(IntPtr node, float x, float y, int seed);

        [DllImport(NATIVE_LIB)]
        internal static extern float fnGenSingle3D(
            IntPtr node,
            float x,
            float y,
            float z,
            int seed
        );

        [DllImport(NATIVE_LIB)]
        internal static extern float fnGenSingle4D(
            IntPtr node,
            float x,
            float y,
            float z,
            float w,
            int seed
        );

        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataCount();

        [DllImport(NATIVE_LIB)]
        internal static extern IntPtr fnGetMetadataName(int id);

        // Variable
        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataVariableCount(int id);

        [DllImport(NATIVE_LIB)]
        internal static extern IntPtr fnGetMetadataVariableName(int id, int variableIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataVariableType(int id, int variableIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataVariableDimensionIdx(int id, int variableIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataEnumCount(int id, int variableIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern IntPtr fnGetMetadataEnumName(
            int id,
            int variableIndex,
            int enumIndex
        );

        [DllImport(NATIVE_LIB)]
        internal static extern bool fnSetVariableFloat(
            IntPtr nodeHandle,
            int variableIndex,
            float value
        );

        [DllImport(NATIVE_LIB)]
        internal static extern bool fnSetVariableIntEnum(
            IntPtr nodeHandle,
            int variableIndex,
            int value
        );

        // Node Lookup
        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataNodeLookupCount(int id);

        [DllImport(NATIVE_LIB)]
        internal static extern IntPtr fnGetMetadataNodeLookupName(int id, int nodeLookupIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataNodeLookupDimensionIdx(int id, int nodeLookupIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern bool fnSetNodeLookup(
            IntPtr nodeHandle,
            int nodeLookupIndex,
            IntPtr nodeLookupHandle
        );

        // Hybrid
        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataHybridCount(int id);

        [DllImport(NATIVE_LIB)]
        internal static extern IntPtr fnGetMetadataHybridName(int id, int nodeLookupIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern int fnGetMetadataHybridDimensionIdx(int id, int nodeLookupIndex);

        [DllImport(NATIVE_LIB)]
        internal static extern bool fnSetHybridNodeLookup(
            IntPtr nodeHandle,
            int nodeLookupIndex,
            IntPtr nodeLookupHandle
        );

        [DllImport(NATIVE_LIB)]
        internal static extern bool fnSetHybridFloat(
            IntPtr nodeHandle,
            int nodeLookupIndex,
            float value
        );

        private bool m_IsDisposed;
        public readonly bool IsCreated => !m_IsDisposed && Invalid != this;
    }
}
