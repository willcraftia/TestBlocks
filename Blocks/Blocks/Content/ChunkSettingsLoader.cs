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
                MinActiveVolume = definition.MinActiveRange,
                MaxActiveVolume = definition.MaxActiveRange,
            };

            settings.PartitionManager.PartitionPoolMaxCapacity = definition.ChunkPoolMaxCapacity;
            settings.PartitionManager.ClusterSize = definition.ClusterSize;
            settings.PartitionManager.ActiveClusterCapacity = definition.ActiveClusterCapacity;
            settings.PartitionManager.ActivationCapacity = definition.ActivationCapacity;
            settings.PartitionManager.PassivationCapacity = definition.PassivationCapacity;
            settings.PartitionManager.PassivationSearchCapacity = definition.PassivationSearchCapacity;

            settings.PartitionManager.PartitionSize = definition.ChunkSize.ToVector3();
            settings.PartitionManager.MinActiveVolume = new DefaultActiveVolume(settings.MinActiveVolume);
            settings.PartitionManager.MaxActiveVolume = new DefaultActiveVolume(settings.MaxActiveVolume);

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
                MinActiveRange = settings.MinActiveVolume,
                MaxActiveRange = settings.MaxActiveVolume,
                ChunkPoolMaxCapacity = settings.PartitionManager.PartitionPoolMaxCapacity,
                ClusterSize = settings.PartitionManager.ClusterSize,
                ActiveClusterCapacity = settings.PartitionManager.ActiveClusterCapacity,
                ActivationCapacity = settings.PartitionManager.ActivationCapacity,
                PassivationCapacity = settings.PartitionManager.PassivationCapacity,
                PassivationSearchCapacity = settings.PartitionManager.PassivationSearchCapacity
            };

            serializer.Serialize(resource, definition);
        }
    }
}
