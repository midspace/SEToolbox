﻿namespace SEToolbox.Interfaces
{
    using System.Collections.Generic;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;
    using VRageMath;

    public interface IMainView
    {
        bool IsModified { get; set; }

        bool IsBusy { get; set; }

        void CalcDistances();

        void OptimizeModel(params IStructureViewBase[] viewModels);

        string CreateUniqueVoxelStorageName(string originalFile, MyObjectBuilder_EntityBase[] additionalList);

        string CreateUniqueVoxelStorageName(string originalFile);

        List<IStructureBase> GetIntersectingEntities(BoundingBox box);

        StructureCharacterModel ThePlayerCharacter { get; }

        double Progress { get; set; }

        void ResetProgress(double initial, double maximumProgress);

        void IncrementProgress();

        void ClearProgress();

        MyObjectBuilder_Checkpoint Checkpoint { get; }

        int[] CreativeModeColors { get; set; }

        IStructureBase AddEntity(MyObjectBuilder_EntityBase entity);
    }
}
