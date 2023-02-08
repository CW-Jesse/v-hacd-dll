using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Reflection;

namespace VHACD {
    public class VHACD : IDisposable {
        private const string dllName = "libvhacd";
        private IntPtr vhacd;

        private Parameters m_parameters = new Parameters();

        [DllImport(dllName, EntryPoint = "CreateVHACD")]
        private static extern IntPtr DllCreateVHACD();

        [DllImport(dllName, EntryPoint = "CreateVHACD_ASYNC")]
        private static extern IntPtr DllCreateVHACD_ASYNC();

        [DllImport(dllName, EntryPoint = "DestroyVHACD")]
        private static extern void DllDestroyVHACD(IntPtr vhacd);

        [DllImport(dllName, EntryPoint = "ComputeFloat")]
        private static extern bool DllComputeFloat(
            IntPtr pVHACD,
            float[] points,
            uint countPoints,
            uint[] triangles,
            uint countTriangles,
            IntPtr parameters);

        [DllImport(dllName, EntryPoint = "ComputeDouble")]
        private static extern bool DllComputeDouble(
            IntPtr pVHACD,
            double[] points,
            uint countPoints,
            uint[] triangles,
            uint countTriangles,
            IntPtr parameters);

        [DllImport(dllName, EntryPoint = "Cancel")]
        private static extern void DllCancel(IntPtr vhacd);

        [DllImport(dllName, EntryPoint = "IsReady")]
        private static extern bool DllIsReady(IntPtr vhacd);

        [DllImport(dllName, EntryPoint = "findNearestConvexHull")]
        private static extern uint DllfindNearestConvexHull(
            IntPtr vhacd,
            double[] pos,
            out double distanceToHull);

        [DllImport(dllName, EntryPoint = "GetNConvexHulls")]
        private static extern uint DllGetNConvexHulls(IntPtr vhacd);

        [DllImport(dllName, EntryPoint = "GetConvexHull")]
        private static extern bool DllGetConvexHull(
            IntPtr vhacd,
            uint index,
            out ConvexHull ch);

        public Parameters parameters {
            get { return m_parameters; }
            set { m_parameters = parameters; }
        }

        public VHACD() {
            vhacd = DllCreateVHACD_ASYNC();
        }
        public void Dispose() {
            DllDestroyVHACD(vhacd);
        }

        public bool Compute(
            float[] points,
            uint countPoints,
            uint[] triangles,
            uint countTriangles) {

            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(m_parameters));
            Marshal.StructureToPtr(m_parameters, p, false);

            return DllComputeFloat(vhacd, points, countPoints, triangles, countTriangles, p);
        }

        public bool Compute(
            double[] points,
            uint countPoints,
            uint[] triangles,
            uint countTriangles) {

            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(m_parameters));
            Marshal.StructureToPtr(m_parameters, p, false);

            return DllComputeDouble(vhacd, points, countPoints, triangles, countTriangles, p);
        }

        public void Cancel() {
            DllCancel(vhacd);
        }

        public void IsReady() {
            DllIsReady(vhacd);
        }

        public uint findNearestConvexHull(
            IntPtr vhacd,
            double[] pos,
            out double distanceToHull) {
            return DllfindNearestConvexHull(vhacd, pos, out distanceToHull);
        }

        public bool GetConvexHull(
            IntPtr vhacd,
            uint index,
            out ConvexHull ch) {
            return DllGetConvexHull(vhacd, index, out ch);
        }
    }

    [Serializable]
    public class Parameters {
        public uint m_maxConvexHulls = 64;         // The maximum number of convex hulls to produce
        public uint m_resolution = 400000;         // The voxel resolution to use
        public double m_minimumVolumePercentErrorAllowed = 1; // if the voxels are within 1% of the volume of the hull, we consider this a close enough approximation
        public uint m_maxRecursionDepth = 10;        // The maximum recursion depth
        public bool m_shrinkWrap = true;             // Whether or not to shrinkwrap the voxel positions to the source mesh on output
        public FillMode m_fillMode = FillMode.FLOOD_FILL; // How to fill the interior of the voxelized mesh
        public uint m_maxNumVerticesPerCH = 64;    // The maximum number of vertices allowed in any output convex hull
        public bool m_asyncACD = true;             // Whether or not to run asynchronously, taking advantage of additional cores
        public uint m_minEdgeLength = 2;           // Once a voxel patch has an edge length of less than 4 on all 3 sides, we don't keep recursing
        public bool m_findBestPlane = false;       // Whether or not to attempt to split planes along the best location. Experimental feature. False by default.
    }

    public enum FillMode {
        FLOOD_FILL, // This is the default behavior, after the voxelization step it uses a flood fill to determine 'inside'
                    // from 'outside'. However, meshes with holes can fail and create hollow results.
        SURFACE_ONLY, // Only consider the 'surface', will create 'skins' with hollow centers.
        RAYCAST_FILL, // Uses raycasting to determine inside from outside.
    }

    [Serializable]
    public class ConvexHull {
        public double[] m_points;
        public uint[] m_triangles;

        public double m_volume = 0;          // The volume of the convex hull
        public double[] m_center = { 0, 0, 0 };    // The centroid of the convex hull
        public uint m_meshId = 0;          // A unique id for this convex hull
        public double[] mBmin;                  // Bounding box minimum of the AABB
        public double[] mBmax;                  // Bounding box maximum of the AABB
    }
}