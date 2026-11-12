using CloudGameShare.BuildingBlocks.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudGameShare.Catalog.Domain.Games
{

    public sealed record GameRequirements
    {
        public int CpuCores { get; private init; }
        public int MemoryGb { get; private init; }
        public int GpuCount { get; private init; }
        public int StorageGb { get; private init; }

        private GameRequirements() { }

        public GameRequirements(
            int cpuCores,
            int memoryGb,
            int gpuCount,
            int storageGb)
        {
            if (cpuCores <= 0)
                throw new DomainException("CPU cores must be greater than zero.");

            if (memoryGb <= 0)
                throw new DomainException("Memory must be greater than zero.");

            if (gpuCount <= 0)
                throw new DomainException("GPU count must be greater than zero.");

            if (storageGb <= 0)
                throw new DomainException("Storage must be greater than zero.");

            CpuCores = cpuCores;
            MemoryGb = memoryGb;
            GpuCount = gpuCount;
            StorageGb = storageGb;
        }
    }

}
