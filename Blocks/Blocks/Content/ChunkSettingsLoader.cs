#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class ChunkSettingsLoader : IAssetLoader
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(ChunkSettingsDefinition));

        public object Load(IResource resource)
        {
            var definition = (ChunkSettingsDefinition) serializer.Deserialize(resource);

            var settings = new ChunkSettings
            {
                ChunkSize = definition.ChunkSize,
                MeshUpdateSearchCapacity = definition.MeshUpdateSearchCapacity,
                VerticesBuilderCount = definition.VerticesBuilderCount,
                MinActiveRange = definition.MinActiveRange,
                MaxActiveRange = definition.MaxActiveRange,
            };

            settings.PartitionManager.PartitionPoolMaxCapacity = definition.ChunkPoolMaxCapacity;
            settings.PartitionManager.ClusterExtent = definition.ClusterExtent;
            settings.PartitionManager.InitialActivePartitionCapacity = definition.InitialActiveChunkCapacity;
            settings.PartitionManager.InitialActiveClusterCapacity = definition.InitialActiveClusterCapacity;
            settings.PartitionManager.InitialActivationCapacity = definition.InitialActivationCapacity;
            settings.PartitionManager.InitialPassivationCapacity = definition.InitialPassivationCapacity;
            settings.PartitionManager.ActivationTaskQueueSlotCount = definition.ActivationTaskQueueSlotCount;
            settings.PartitionManager.PassivationTaskQueueSlotCount = definition.PassivationTaskQueueSlotCount;
            settings.PartitionManager.ActivationSearchCapacity = definition.ActivationSearchCapacity;
            settings.PartitionManager.PassivationSearchCapacity = definition.PassivationSearchCapacity;

            settings.PartitionManager.PartitionSize = definition.ChunkSize.ToVector3();
            settings.PartitionManager.MinLandscapeVolume = new DefaultLandscapeVolume(VectorI3.Zero, settings.MinActiveRange);
            settings.PartitionManager.MaxLandscapeVolume = new DefaultLandscapeVolume(VectorI3.Zero, settings.MaxActiveRange);

            return settings;
        }

        public void Save(IResource resource, object asset)
        {
            var settings = asset as ChunkSettings;

            var definition = new ChunkSettingsDefinition
            {
                ChunkSize = settings.ChunkSize,
                MeshUpdateSearchCapacity = settings.MeshUpdateSearchCapacity,
                VerticesBuilderCount = settings.VerticesBuilderCount,
                MinActiveRange = settings.MinActiveRange,
                MaxActiveRange = settings.MaxActiveRange,
                ChunkPoolMaxCapacity = settings.PartitionManager.PartitionPoolMaxCapacity,
                ClusterExtent = settings.PartitionManager.ClusterExtent,
                InitialActiveChunkCapacity = settings.PartitionManager.InitialActivePartitionCapacity,
                InitialActiveClusterCapacity = settings.PartitionManager.InitialActiveClusterCapacity,
                InitialActivationCapacity = settings.PartitionManager.InitialActivationCapacity,
                InitialPassivationCapacity = settings.PartitionManager.InitialPassivationCapacity,
                ActivationTaskQueueSlotCount = settings.PartitionManager.ActivationTaskQueueSlotCount,
                PassivationTaskQueueSlotCount = settings.PartitionManager.PassivationTaskQueueSlotCount,
                ActivationSearchCapacity = settings.PartitionManager.ActivationSearchCapacity,
                PassivationSearchCapacity = settings.PartitionManager.PassivationSearchCapacity
            };

            serializer.Serialize(resource, definition);
        }
    }
}
