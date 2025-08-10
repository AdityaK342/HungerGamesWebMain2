using DongUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Media3D;

namespace VisualizerControl
{
    /// <summary>
    /// The crucial interface between the WPF and the C++ code
    /// </summary>
    public partial class Visualizer3DCoreInterface : HwndHost
    {
        internal const int
                    WsChild = 0x40000000,
                    WsVisible = 0x10000000,
                    LbsNotify = 0x00000001,
                    HostId = 0x00000002,
                    ListboxId = 0x00000001,
                    WsVscroll = 0x00200000,
                    WsBorder = 0x00800000;

        public int HostHeight { get; set; }
        public int HostWidth { get; set; }
        private IntPtr hwndHost;

        public Visualizer3DCoreInterface()
        { }

        public Visualizer3DCoreInterface(double windowWidth, double windowHeight)
        {
            SetWindowDimensions(windowWidth, windowHeight);
        }

        public IntPtr HwndListBox { get; private set; }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            HwndListBox = IntPtr.Zero;
            hwndHost = IntPtr.Zero;

            string windowName = "internalWindow";
            //string curDir = Directory.GetCurrentDirectory();
            RegisterWindow(windowName);

            double fourKScaleFactor = 1.5;
            /*
             * On 4k screens you have to have UI scaling in order for things to not look super tiny.
             * Unfortunately it seems that the internal win32 apps don't get automatically scaled while the wpf does so we have to
             * manually scale here.
             */

            double debugInternalScale = 1;

            hwndHost = CreateWindowEx(0, "static", "",
                WsChild | WsVisible,
                0, 0,
                (int)(HostHeight * fourKScaleFactor), (int)(HostWidth * fourKScaleFactor),
                hwndParent.Handle,
                (IntPtr)HostId,
                IntPtr.Zero,
                0);

            HwndListBox = MakeWindow(windowName,
                WsChild | WsVisible | LbsNotify | WsBorder,
                (int)(HostHeight * fourKScaleFactor * debugInternalScale),
                (int)(HostWidth * fourKScaleFactor * debugInternalScale),
                hwndHost);

            return new HandleRef(this, hwndHost);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        private readonly Dictionary<string, int> shapeCodes = [];
        private readonly Dictionary<string, int> materialCodes = [];
        private readonly Dictionary<int, int> externalIndexToInternalIndex = [];

        internal void AddObject(Object3D obj, int externalIndex)
        {
            // Only add the shape if it is not currently in the dictionary
            var shape = obj.Shape;
            if (!shapeCodes.ContainsKey(shape.ShapeName))
            {
                AddShape(shape);
                // So each shape is added only once
            }

            var material = obj.Material;

            if (!materialCodes.ContainsKey(material.Name))
            {
                AddMaterial(material);
            }

            float[] position = ConvertVector(obj.Position);
            float[] scale = ConvertVector(obj.Scale);
            float[] rotation = ConvertMatrix(obj.Rotation.Value);
            int internalIndex = AddObjectX(scale, rotation, position,
                shapeCodes[shape.ShapeName], materialCodes[material.Name]);
            externalIndexToInternalIndex.Add(externalIndex, internalIndex);
        }

        private static float[] ConvertVector(Vector3D vec)
        {
            float[] response = [(float)vec.X, (float)vec.Y, (float)vec.Z];
            return response;
        }

        private static float[] ConvertMatrix(Matrix3D mat)
        {
            float[] response =
            [
                (float)mat.M11,
                (float)mat.M21,
                (float)mat.M31,
                (float)mat.M12,
                (float)mat.M22,
                (float)mat.M32,
                (float)mat.M13,
                (float)mat.M23,
                (float)mat.M33,
            ];
            return response;
        }

        private int AddMaterial(BasicMaterial material)
        {
            var color = material.Color;
            float r = (float)color.R / 255;
            float g = (float)color.G / 255;
            float b = (float)color.B / 255;
            float a = (float)color.A / 255;
            int index = materialCodes.Count;
            materialCodes.Add(material.Name, index);
            AddMaterialX(index, r, g, b, a, (float)material.Fresnel, (float)material.Roughness);

            return index;
        }

        /// <summary>
        /// Changes the color of a material based on the change already taken place
        /// in the BasicMaterial object
        /// </summary>
        public void ChangeMaterialColor(BasicMaterial material)
        {
            var color = material.Color;
            float r = (float)color.R / 255;
            float g = (float)color.G / 255;
            float b = (float)color.B / 255;
            float a = (float)color.A / 255;

            int internalIndex = materialCodes[material.Name];
            ChangeMaterialColorX(internalIndex, r, g, b, a);
        }

        private void AddShape(Shapes.Shape3D shape)
        {
            var mesh = shape.Mesh;
            int nVertices = mesh.Positions.Count;
            // Vectors are packed into flat arrays
            // So there are 3 * nVertices points in vertices[] and normals[]
            int size = nVertices * 3;
            float[] vertices = new float[size];
            float[] normals = new float[size];
            for (int i = 0; i < nVertices; ++i)
            {
                var vertex = mesh.Positions[i];
                int index = i * 3;
                vertices[index] = (float)(vertex.X);
                vertices[index + 1] = (float)(vertex.Y);
                vertices[index + 2] = (float)(vertex.Z);
                var normal = mesh.Normals[i];
                normals[index] = (float)(normal.X);
                normals[index + 1] = (float)(normal.Y);
                normals[index + 2] = (float)(normal.Z);
            }

            int nTriangleIndices = mesh.TriangleIndices.Count;
            UInt32[] triangles = new UInt32[nTriangleIndices];
            for (int i = 0; i < nTriangleIndices; ++i)
            {
                triangles[i] = (UInt32)(mesh.TriangleIndices[i]);
            }

            shapeCodes.Add(shape.ShapeName, shapeCodes.Count);
            AddShapeX(shapeCodes[shape.ShapeName], nVertices, vertices, normals, nTriangleIndices,
                triangles);
        }

        internal void MoveObject(int externalIndex, Vector3D newPosition)
        {
            int internalIndex = externalIndexToInternalIndex[externalIndex];

            MoveObjectX(internalIndex, ConvertVector(newPosition));
        }

        internal void TransformObject(int externalIndex, Vector3D newScale,
            Matrix3D newRotation, Vector3D newPosition)
        {
            int internalIndex = externalIndexToInternalIndex[externalIndex];
            float[] position = ConvertVector(newPosition);
            float[] scale = ConvertVector(newScale);
            float[] rotation = ConvertMatrix(newRotation);
            TransformObjectX(internalIndex, scale, rotation, position);
        }

        internal void RemoveObject(int externalIndex)
        {
            int internalIndex = externalIndexToInternalIndex[externalIndex];
            RemoveObjectX(internalIndex);
            externalIndexToInternalIndex.Remove(externalIndex);
            // Redo internal indices
            foreach (var key in externalIndexToInternalIndex.Keys.ToList())
            {
                if (externalIndexToInternalIndex[key] > internalIndex)
                    --externalIndexToInternalIndex[key];
            }
        }

        internal void Clear()
        {
            ClearX();
            externalIndexToInternalIndex.Clear();
            shapeCodes.Clear();
            materialCodes.Clear();
        }

        internal static void SetAutoCamera(bool value) => SetAutoCameraX(value);

        internal static void AutoCameraAdjust() => AutoCameraAdjustX();

        internal static void MoveCamera(Point3D newPosition)
        {
            float[] position = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(newPosition));
            MoveCameraX(position);
        }

        internal static void MoveCamera(Point3D newPosition, Vector3D newLookDirection, Vector3D newUpDirection)
        {
            float[] position = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(newPosition));
            float[] lookDirection = ConvertVector(newLookDirection);
            float[] upDirection = ConvertVector(newUpDirection);
            MoveAndTurnCameraX(position, lookDirection, upDirection);
        }

        internal static void LookAt(Point3D newPosition, Point3D target, Vector3D newUpDirection)
        {
            float[] position = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(newPosition));
            float[] newTarget = ConvertVector(WPFUtility.UtilityFunctions.ConvertToVector3D(target));
            float[] upDirection = ConvertVector(newUpDirection);
            LookAtX(position, newTarget, upDirection);
        }

        internal static void AdjustLens(double fieldOfViewY, double aspectRatio, double nearZ, double farZ)
        {
            AdjustLensX((float)fieldOfViewY, (float)aspectRatio, (float)nearZ, (float)farZ);
        }

        internal static Vector CameraPosition => GetCameraInfo().Item1;
        internal static Vector CameraLookDirection => GetCameraInfo().Item2;
        internal static Vector CameraUpDirection => GetCameraInfo().Item3;

        private static Tuple<Vector, Vector, Vector> GetCameraInfo()
        {
            var position = new float[3];
            var lookDirection = new float[3];
            var upDirection = new float[3];
            GetCameraPositionX(position, lookDirection, upDirection);
            var positionVec = new Vector(position[0], position[1], position[2]);
            var lookVec = new Vector(lookDirection[0], lookDirection[1], lookDirection[2]);
            var upVec = new Vector(upDirection[0], upDirection[1], upDirection[2]);
            return new Tuple<Vector, Vector, Vector>(positionVec, lookVec, upVec);
        }

        private bool paused = true;
        internal bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
                SetPauseDrawingStateX(value);
            }
        }

        private const string dllName = @"..\..\..\..\VisualizerControl\Visualizer3DCore.dll";
        //private const string dllName = @"C:\Users\pdong\Documents\Visual Studio Repositories\Computational Science Spring 2021\VisualizerControl\Visualizer3DCore.dll";

        [LibraryImport(dllName, EntryPoint = "RegisterWindow", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool RegisterWindow(string ClassName);

        [LibraryImport(dllName, EntryPoint = "MakeWindow", StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial IntPtr MakeWindow(string ClassName, int style, int height, int width, IntPtr parent);

        [LibraryImport(dllName, EntryPoint = "SetupDirectX")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void SetupDirectX();

        [LibraryImport(dllName, EntryPoint = "AddShape")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AddShapeX(int index, int nVertices, [In] float[] vertices,
        [In] float[] normals, int nTriangleIndices, [In] UInt32[] triangles);

        [LibraryImport(dllName, EntryPoint = "AddMaterial")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void AddMaterialX(int index, float colorR,
            float colorG, float colorB, float alpha, float fresnel, float roughness);

        [DllImport(dllName, EntryPoint = "ChangeMaterialColor", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ChangeMaterialColorX(int index, float colorR,
            float colorG, float colorB, float alpha);

        [DllImport(dllName, EntryPoint = "AddObject", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddObjectX(float[] scale, float[] rotation,
            float[] position, int shape, int material);

        [DllImport(dllName, EntryPoint = "MoveObject", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MoveObjectX(int index, float[] newPosition);

        [DllImport(dllName, EntryPoint = "TransformObject", CallingConvention = CallingConvention.Cdecl)]
        private static extern void TransformObjectX(int index, float[] scale,
            float[] rotation, float[] position);

        [DllImport(dllName, EntryPoint = "RemoveObject", CallingConvention = CallingConvention.Cdecl)]
        private static extern void RemoveObjectX(int index);

        [DllImport(dllName, EntryPoint = "Clear", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ClearX();

        [DllImport(dllName, EntryPoint = "SetAutoCamera", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetAutoCameraX(bool value);

        [DllImport(dllName, EntryPoint = "AutoCameraAdjust", CallingConvention = CallingConvention.Cdecl)]
        private static extern void AutoCameraAdjustX();

        [DllImport(dllName, EntryPoint = "MoveCamera", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MoveCameraX(float[] newPosition);

        [DllImport(dllName, EntryPoint = "MoveAndTurnCamera", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MoveAndTurnCameraX(float[] newPosition, float[] lookDirection, float[] upDirection);

        [DllImport(dllName, EntryPoint = "LookAt", CallingConvention = CallingConvention.Cdecl)]
        private static extern void LookAtX(float[] newPosition, float[] target, float[] upDirection);

        [DllImport(dllName, EntryPoint = "AdjustLens", CallingConvention = CallingConvention.Cdecl)]
        private static extern void AdjustLensX(float fieldOfViewY, float aspectRatio, float nearZ, float farZ);

        [DllImport(dllName, EntryPoint = "SetPauseDrawingState", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPauseDrawingStateX(bool value);

        [DllImport(dllName, EntryPoint = "GetCameraPosition", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetCameraPositionX(float[] position, float[] lookDirection, float[] upDirection);

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(int dwExStyle,
            string lpszClassName,
            string lpszWindowName,
            int style,
            int x, int y,
            int width, int height,
            IntPtr hwndParent,
            IntPtr hMenu,
            IntPtr hInst,
            [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        private static extern bool DestroyWindow(IntPtr hwnd);

        public void SetWindowDimensions(double windowWidth, double windowHeight)
        {
            HostHeight = (int)windowHeight;
            HostWidth = (int)windowWidth;
        }
    }
}
