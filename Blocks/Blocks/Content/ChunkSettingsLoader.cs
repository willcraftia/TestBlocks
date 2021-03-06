﻿#region Using

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
                VertexBuildConcurrencyLevel = definition.VertexBuildConcurrencyLevel,
                UpdateBufferCountPerFrame = definition.UpdateBufferCountPerFrame,
                MinActiveVolume = definition.MinActiveRange,
                MaxActiveVolume = definition.MaxActiveRange,
                ChunkStoreType = definition.ChunkStoreType
            };

            settings.PartitionManager.ClusterSize = definition.ClusterSize;
            settings.PartitionManager.ActivationCapacity = definition.ActivationCapacity;
            settings.PartitionManager.PassivationCapacity = definition.PassivationCapacity;
            settings.PartitionManager.PassivationSearchCapacity = definition.PassivationSearchCapacity;
            settings.PartitionManager.PriorActiveDistance = definition.PriorActiveDistance;

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
                VertexBuildConcurrencyLevel = settings.VertexBuildConcurrencyLevel,
                UpdateBufferCountPerFrame = settings.UpdateBufferCountPerFrame,
                MinActiveRange = settings.MinActiveVolume,
                MaxActiveRange = settings.MaxActiveVolume,
                ClusterSize = settings.PartitionManager.ClusterSize,
                ActivationCapacity = settings.PartitionManager.ActivationCapacity,
                PassivationCapacity = settings.PartitionManager.PassivationCapacity,
                PassivationSearchCapacity = settings.PartitionManager.PassivationSearchCapacity,
                PriorActiveDistance = settings.PartitionManager.PriorActiveDistance,
                ChunkStoreType = settings.ChunkStoreType
            };

            serializer.Serialize(resource, definition);
        }
    }
}
