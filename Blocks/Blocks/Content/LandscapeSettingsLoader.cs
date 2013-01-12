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
    public sealed class LandscapeSettingsLoader : IAssetLoader
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(LandscapeSettingsDefinition));

        public object Load(IResource resource)
        {
            var definition = (LandscapeSettingsDefinition) serializer.Deserialize(resource);

            var settings = new LandscapeSettings
            {
                MinActiveRange = definition.MinActiveRange,
                MaxActiveRange = definition.MaxActiveRange,
            };

            settings.PartitionManager.PartitionPoolMaxCapacity = definition.PartitionPoolMaxCapacity;
            settings.PartitionManager.ClusterExtent = definition.ClusterExtent;
            settings.PartitionManager.InitialActivePartitionCapacity = definition.InitialActivePartitionCapacity;
            settings.PartitionManager.InitialActiveClusterCapacity = definition.InitialActiveClusterCapacity;
            settings.PartitionManager.InitialActivationCapacity = definition.InitialActivationCapacity;
            settings.PartitionManager.InitialPassivationCapacity = definition.InitialPassivationCapacity;
            settings.PartitionManager.ActivationTaskQueueSlotCount = definition.ActivationTaskQueueSlotCount;
            settings.PartitionManager.PassivationTaskQueueSlotCount = definition.PassivationTaskQueueSlotCount;
            settings.PartitionManager.ActivationSearchCapacity = definition.ActivationSearchCapacity;
            settings.PartitionManager.PassivationSearchCapacity = definition.PassivationSearchCapacity;

            settings.PartitionManager.PartitionSize = Chunk.Size.ToVector3();
            settings.PartitionManager.MinLandscapeVolume = new DefaultLandscapeVolume(VectorI3.Zero, settings.MinActiveRange);
            settings.PartitionManager.MaxLandscapeVolume = new DefaultLandscapeVolume(VectorI3.Zero, settings.MaxActiveRange);

            return settings;
        }

        public void Save(IResource resource, object asset)
        {
            var settings = asset as LandscapeSettings;

            var definition = new LandscapeSettingsDefinition
            {
                MinActiveRange = settings.MinActiveRange,
                MaxActiveRange = settings.MaxActiveRange,
                PartitionPoolMaxCapacity = settings.PartitionManager.PartitionPoolMaxCapacity,
                ClusterExtent = settings.PartitionManager.ClusterExtent,
                InitialActivePartitionCapacity = settings.PartitionManager.InitialActivePartitionCapacity,
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
