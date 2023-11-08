/*******************************************************************************
Copyright ? 2015-2022 Pico Technology Co., Ltd.All rights reserved.  

NOTICE£ºAll information contained herein is, and remains the property of 
Pico Technology Co., Ltd. The intellectual and technical concepts 
contained hererin are proprietary to Pico Technology Co., Ltd. and may be 
covered by patents, patents in process, and are protected by trade secret or 
copyright law. Dissemination of this information or reproduction of this 
material is strictly forbidden unless prior written permission is obtained from
Pico Technology Co., Ltd. 
*******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.XR.PXR
{
    public class PXR_MixedReality
    {
        /// <summary>
        /// Create a spatial anchor
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="type"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static int CreateSpatialAnchor(Vector3 position, Quaternion rotation, PxrReferenceType type, ref ulong handle)
        {
            PxrSpatialAnchorCreateInfo info = new PxrSpatialAnchorCreateInfo()
            {
                referenceType = type,
                pose = new PxrPosef()
                {
                    orientation = new PxrVector4f()
                    {
                        x = rotation.x,
                        y = rotation.y,
                        z = -rotation.z,
                        w = -rotation.w
                    },
                    position = new PxrVector3f()
                    {
                        x = position.x,
                        y = position.y,
                        z = -position.z
                    }
                },
                time = PXR_Plugin.System.UPxr_GetPredictedDisplayTime()
            };

            int resultType = PXR_Plugin.MixedReality.UPxr_CreateSpatialAnchor(ref info, ref handle);
            Debug.Log("resultType : " + resultType);
            return resultType;
        }

        /// <summary>
        /// Destroy a spatial anchor in memory
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static int DestroySpatialAnchor(ulong handle)
        {
            return PXR_Plugin.MixedReality.UPxr_DestroySpatialAnchor(handle);
        }

        /// <summary>
        /// Save a spatial anchor to the persistence place
        /// </summary>
        /// <param name="info"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int SaveSpatialAnchor(PxrSpatialAnchorSaveInfo info, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_SaveSpatialAnchor(ref info, ref requestId);
        }

        /// <summary>
        /// Delete a spatial anchor from the persistence place
        /// </summary>
        /// <param name="info"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int DeleteSpatialAnchor(PxrSpatialAnchorDeleteInfo info, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_DeleteSpatialAnchor(ref info, ref requestId);
        }

        /// <summary>
        /// Load spatial anchor from persistence place
        /// </summary>
        /// <param name="info"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int LoadSpatialAnchorById(PxrSpatialInstanceLoadByIdInfo info, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_LoadSpatialAnchorById(ref info, ref requestId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public static int GetSpatialAnchorLoadResults(UInt64 requestId, out PxrSpatialAnchorLoadResult[] results)
        {
            results = null;
            
            PxrSpatialAnchorLoadResults resultsCount = new PxrSpatialAnchorLoadResults()
            {
                resultCapacityInput = 0,
                resultCountOutput = 0,
                results = new IntPtr(0),
            };
            PXR_Plugin.MixedReality.UPxr_GetSpatialAnchorLoadResults(requestId, ref resultsCount);
            int resultSize = Marshal.SizeOf(typeof(PxrSpatialAnchorLoadResult));
            int resultBytesSize = resultsCount.resultCountOutput * resultSize;
            PxrSpatialAnchorLoadResults anchorResults = new PxrSpatialAnchorLoadResults()
            {
                resultCapacityInput = resultsCount.resultCountOutput,
                resultCountOutput = 0,
                results = Marshal.AllocHGlobal(resultBytesSize),
            };
            int state = PXR_Plugin.MixedReality.UPxr_GetSpatialAnchorLoadResults(requestId, ref anchorResults);

            results = new PxrSpatialAnchorLoadResult[resultsCount.resultCountOutput];
            for (int i = 0; i < resultsCount.resultCountOutput; i++)
            {
                PxrSpatialAnchorLoadResult t = (PxrSpatialAnchorLoadResult)Marshal.PtrToStructure(anchorResults.results + (i * resultSize), typeof(PxrSpatialAnchorLoadResult));
                results[i] = t;
            }
            Marshal.FreeHGlobal(anchorResults.results);

            return state;
        }

        /// <summary>
        /// Get the pose of the spatial anchor
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="predictDisplayTime"></param>
        /// <param name="type"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        public static int GetSpatialAnchorPose(ulong handle, PxrReferenceType type, ref PxrPosef pose)
        {
            double predictDisplayTime = PXR_Plugin.System.UPxr_GetPredictedDisplayTime();
            int result = PXR_Plugin.MixedReality.UPxr_GetSpatialAnchorPose(handle, predictDisplayTime, type, ref pose);
            pose.position.z = -pose.position.z;
            pose.orientation.z = -pose.orientation.z;
            pose.orientation.w = -pose.orientation.w;
            return result;
        }

        
        /// <summary>
        /// Set a Property enabled/disabled of a spatial anchor
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="type"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static int SetSpatialAnchorProperty(ulong handle, PxrSpatialInstancePropertyType type, bool enable)
        {
            return PXR_Plugin.MixedReality.UPxr_SetSpatialAnchorProperty(handle, type, enable);
        }

        /// <summary>
        /// Get the specified property status of the spatial anchor
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int GetSpatialAnchorProperty(ulong handle, PxrSpatialInstancePropertyType type, ref bool status)
        {
            return PXR_Plugin.MixedReality.UPxr_GetSpatialAnchorProperty(handle, type,ref status);
        }

        /// <summary>
        /// Get the spatial anchor uuid
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public static int GetSpatialAnchorUuid(ulong handle, ref PxrSpatialInstanceUuid uuid)
        {
            return PXR_Plugin.MixedReality.UPxr_GetSpatialAnchorUuid(handle, ref uuid);
        }

        /// <summary>
        /// Get the spatial anchor tag
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static int GetSpatialAnchorTag(ulong handle, ref PxrSpatialInstanceTag tag)
        {
            return PXR_Plugin.MixedReality.UPxr_GetSpatialAnchorTag(handle, ref tag);
        }

        /// <summary>
        /// Set the spatial anchor tag
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static int SetSpatialAnchorTag(ulong handle, PxrSpatialInstanceTag tag)
        {
            return PXR_Plugin.MixedReality.UPxr_SetSpatialAnchorTag(handle, tag);
        }

        /// <summary>
        /// Export spatial instance files: Spatial Anchor + spatial model + SLAM Map 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int ExportSpatialInstance(PxrExportSpatialInstanceInfo info, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_ExportSpatialInstance(ref info, ref requestId);
        }

        /// <summary>
        /// Import spatial instance files: Spatial Anchor + spatial model + SLAM Map
        /// </summary>
        /// <param name="info"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int ImportSpatialInstance(PxrImportSpatialInstanceInfo info, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_ImportSpatialInstance(ref info, ref requestId);
        }

        /// <summary>
        /// Stop Spatial Recognition
        /// </summary>
        /// <returns></returns>
        public static int StopSpatialRecognition()
        {
            return PXR_Plugin.MixedReality.UPxr_StopSpatialRecognition();
        }

        public static uint humanMeshVerticesSize = 128 * 128 * 5;
        public static uint humanMeshFacetSize = 127 * 127 * 2 * 3;
        public static uint humanMeshMaskSize = 88 * 88;

        /// <summary>
        /// Get the human body segmentation handle
        /// </summary>
        /// <returns></returns>
        public static int CreateHumanOcclusionHandle()
        {
            PxrHumanMeshingConfig config2 = new PxrHumanMeshingConfig()
            {
                rgbCalibPath = "/mnt/vendor/persist/pvr/mav0/cam4/sensor.yaml",
                inferencerPath = "/mnt/vendor/persist/pvr/human_seg_infer",
                rgbImageWidth = 582,
                rgbImageHeight = 437,
            };
            var result = PXR_Plugin.MixedReality.UPxr_CreateMeshingHandle(ref config2, ref humanMeshVerticesSize, ref humanMeshFacetSize,ref humanMeshMaskSize);

            return result;
        }

        private static IntPtr vBufferPtr = IntPtr.Zero;
        private static IntPtr fBufferPtr = IntPtr.Zero;
        private static IntPtr mBufferPtr = IntPtr.Zero;
        /// <summary>
        /// Get the human mesh
        /// </summary>
        /// <param name="verticesBuffer">Vertex buffer start address</param>
        /// <param name="verticesSize">The number of vertices, note that vertex buffer size = 3 * verticesSize </param>
        /// <param name="facetsBuffer">Patch buffer start address</param>
        /// <param name="facetSize">The number of facets, note that the facet buffer size = 3 * facetSize</param>
        /// <param name="diffHumanFirstIdx">The starting id of different human face pieces</param>
        /// <param name="haveRead">Whether it has been read before, if it has been read before, do not update the buffer, otherwise update the buffer</param>
        /// <returns></returns>
        public static int AcquireHumanOcclusionInfo(ref byte[] maskBuffer, ref float[] verticesBuffer, ref uint verticesSize, ref int[] facetsBuffer, ref uint facetSize, ref bool haveRead)
        {
            if (mBufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mBufferPtr);
                mBufferPtr = IntPtr.Zero;
            }
            if (fBufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(fBufferPtr);
                fBufferPtr = IntPtr.Zero;
            }
            if (vBufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(vBufferPtr);
                vBufferPtr = IntPtr.Zero;
            }

            
            mBufferPtr = Marshal.AllocHGlobal((int)humanMeshMaskSize);
            vBufferPtr = Marshal.AllocHGlobal((int)humanMeshVerticesSize * Marshal.SizeOf(typeof(float)));
            fBufferPtr = Marshal.AllocHGlobal((int)humanMeshFacetSize * Marshal.SizeOf(typeof(int)));
            
            var state = PXR_Plugin.MixedReality.UPxr_AcquireMeshingInfo(mBufferPtr,vBufferPtr, ref verticesSize, fBufferPtr, ref facetSize, ref haveRead);
            Marshal.Copy(mBufferPtr, maskBuffer, 0, 88 * 88);
            Marshal.Copy(vBufferPtr, verticesBuffer, 0, (int)verticesSize * 5);
            Marshal.Copy(fBufferPtr, facetsBuffer, 0, (int)facetSize * 3);
            return state;
        }

        /// <summary>
        /// Destruction segmentation handle
        /// </summary>
        /// <returns></returns>
        public static int DestroyHumanOcclusionHandle()
        {
            return PXR_Plugin.MixedReality.UPxr_DestroyMeshingHandle();
        }

        /// <summary>
        /// Create Room Scene Data
        /// </summary>
        /// <param name="anchorUuid"></param>
        /// <param name="roomSceneData"></param>
        /// <param name="dataLen"></param>
        /// <param name="roomSceneDataHandle"></param>
        /// <returns></returns>
        public static int CreateRoomSceneData(PxrSpatialInstanceUuid anchorUuid, IntPtr roomSceneData, int dataLen,
            ref ulong roomSceneDataHandle)
        {
            return PXR_Plugin.MixedReality.UPxr_CreateRoomSceneData(anchorUuid, roomSceneData, dataLen,
                ref roomSceneDataHandle);
        }

        /// <summary>
        /// Destroy Room Scene Data
        /// </summary>
        /// <param name="roomSceneDataHandle"></param>
        /// <returns></returns>
        public static int DestroyRoomSceneData(ulong roomSceneDataHandle)
        {
            return PXR_Plugin.MixedReality.UPxr_DestroyRoomSceneData(roomSceneDataHandle);
        }

        /// <summary>
        /// Save Room Scene Data
        /// </summary>
        /// <param name="saveInfo"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int SaveRoomSceneData(PxrRoomSceneDataSaveInfo saveInfo, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_SaveRoomSceneData(ref saveInfo, ref requestId);
        }

        /// <summary>
        /// Delete Room Scene Data
        /// </summary>
        /// <param name="deleteInfo"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int DeleteRoomSceneData(PxrRoomSceneDataDeleteInfo deleteInfo, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_DeleteRoomSceneData(ref deleteInfo, ref requestId);
        }

        /// <summary>
        /// Load Room Scene
        /// </summary>
        /// <param name="loadInfo"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static int LoadRoomScene(PxrRoomSceneLoadInfo loadInfo, ref ulong requestId)
        {
            return PXR_Plugin.MixedReality.UPxr_LoadRoomScene(ref loadInfo, ref requestId);
        }

        /// <summary>
        /// Get Room Scene Load result
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public static int GetRoomSceneLoadResults(ulong requestId, out PxrRoomSceneLoadResult[] results)
        {
            results = null;
            PxrRoomSceneLoadResults resultsCount = new PxrRoomSceneLoadResults()
            {
                resultCapacityInput = 0,
                resultCountOutput = 0,
                results = new IntPtr(0),
            };
            PXR_Plugin.MixedReality.UPxr_GetRoomSceneLoadResults(requestId, ref resultsCount);
            
            int resultSize = Marshal.SizeOf(typeof(PxrRoomSceneLoadResult));
            int resultBytesSize = resultsCount.resultCountOutput * resultSize;
            PxrRoomSceneLoadResults sceneResults = new PxrRoomSceneLoadResults()
            {
                resultCapacityInput = resultsCount.resultCountOutput,
                resultCountOutput = 0,
                results = Marshal.AllocHGlobal(resultBytesSize),
            };
            var state = PXR_Plugin.MixedReality.UPxr_GetRoomSceneLoadResults(requestId, ref sceneResults);

            results = new PxrRoomSceneLoadResult[resultsCount.resultCountOutput];
            for (int i = 0; i < resultsCount.resultCountOutput; i++)
            {
                PxrRoomSceneLoadResult t = (PxrRoomSceneLoadResult)Marshal.PtrToStructure(sceneResults.results + (i * resultSize), typeof(PxrRoomSceneLoadResult));
                results[i] = t;
            }
            Marshal.FreeHGlobal(sceneResults.results);
            return state;
        }

        /// <summary>
        /// Start setting room Scene
        /// </summary>
        /// <returns></returns>
        public static int StartRoomCapture()
        {
            return PXR_Plugin.MixedReality.UPxr_StartRoomCapture();
        }

        /// <summary>
        /// Start MR mode
        /// </summary>
        /// <param name="startOptions"></param>
        /// <returns></returns>
        public static int StartMRMode(int startOptions)
        {
            return PXR_Plugin.MixedReality.UPxr_StartMRMode(startOptions);
        }

        /// <summary>
        /// Stop MR mode
        /// </summary>
        /// <returns></returns>
        public static int StopMRMode()
        {
            return PXR_Plugin.MixedReality.UPxr_StopMRMode();
        }
    }
}

