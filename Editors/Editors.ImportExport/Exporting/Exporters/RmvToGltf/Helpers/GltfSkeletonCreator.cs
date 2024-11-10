﻿using Editors.ImportExport.Common;
using Editors.ImportExport.Exporting.Exporters.GltfSkeleton;
using Shared.Core.PackFiles;
using Shared.GameFormats.Animation;
using SharpGLTF.Schema2;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Editors.ImportExport.Exporting.Exporters.RmvToGltf.Helpers
{
    public class ProcessedGltfSkeleton
    {
        public required List<(Node, Matrix4x4)> Data { get; set; }
    }

    public class GltfSkeletonCreator
    {
        private readonly PackFileService _packFileService;

        public GltfSkeletonCreator(PackFileService packFileService)
        {
            _packFileService = packFileService;
        }



        public ProcessedGltfSkeleton Create(ModelRoot outputScene, AnimationFile animSkeletonFil, bool doMirror)
        {
            var framePoseMatrixCalculator = new FramePoseMatrixCalculator(animSkeletonFil);
            var invMatrices = framePoseMatrixCalculator.GetInverseBindPoseMatrices(doMirror);

            var outputGltfBindings = new List<(Node node, Matrix4x4 invMatrix)>();

            var scene = outputScene.UseScene("default");
            var parentIdToGltfNode = new Dictionary<int, Node>();
            var frame = animSkeletonFil.AnimationParts[0].DynamicFrames[0];
            parentIdToGltfNode[-1] = scene.CreateNode(""); // bones with not parent will be children of the scene

            for (var boneIndex = 0; boneIndex < animSkeletonFil.Bones.Length; boneIndex++)
            {
                var parentNode = parentIdToGltfNode[animSkeletonFil.Bones[boneIndex].ParentId];
                if (parentNode == null)
                    throw new Exception($"Parent Node not found for boneIndex={boneIndex}");

                parentIdToGltfNode[boneIndex] = parentNode.CreateNode(animSkeletonFil.Bones[boneIndex].Name);

                parentIdToGltfNode[boneIndex].
                    WithLocalTranslation(VecConv.GetSys(GlobalSceneTransforms.FlipVector(frame.Transforms[boneIndex].ToVector3(), doMirror))).
                    WithLocalRotation(VecConv.GetSys(GlobalSceneTransforms.FlipQuaternion(frame.Quaternion[boneIndex].ToQuaternion(), doMirror))).
                    WithLocalScale(new System.Numerics.Vector3(1, 1, 1));

                var invBindPoseMatrix4x4 = VecConv.GetSys(invMatrices[boneIndex]);

                outputGltfBindings.Add((parentIdToGltfNode[boneIndex], invBindPoseMatrix4x4));
            }

            return new ProcessedGltfSkeleton() { Data = outputGltfBindings };
        }
    }
}
