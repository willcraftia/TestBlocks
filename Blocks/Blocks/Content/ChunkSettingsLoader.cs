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
            settings.PartitionManager.ClusterSize = definition.ClusterSize;
            settings.PartitionManager.ActivePartitionCapacity = definition.ActiveChunkCapacity;
            settings.PartitionManager.ActiveClusterCapacity = definition.ActiveClusterCapacity;
            settings.PartitionManager.ActivationCapacity = definition.ActivationCapacity;
            settings.PartitionManager.PassivationCapacity = definition.PassivationCapacity;
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
                ClusterSize = settings.PartitionManager.ClusterSize,
                ActiveChunkCapacity = settings.PartitionManager.ActivePartitionCapacity,
                ActiveClusterCapacity = settings.PartitionManager.ActiveClusterCapacity,
                ActivationCapacity = settings.PartitionManager.ActivationCapacity,
                PassivationCapacity = settings.PartitionManager.PassivationCapacity,
                ActivationSearchCapacity = settings.PartitionManager.ActivationSearchCapacity,
                PassivationSearchCapacity = settings.PartitionManager.PassivationSearchCapacity
            };

            serializer.Serialize(resource, definition);
        }
    }
}
