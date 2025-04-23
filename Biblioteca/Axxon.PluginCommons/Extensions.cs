using System;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Axxon.PluginCommons
{
    public static partial class Extensions
    {
        /// <summary>
        /// Adds the attributes from the given entity if they do not exist in the current Original
        /// code: https://github.com/daryllabar/DLaB.Xrm
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <param name="baseEntity"></param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static T CoalesceEntity<T>(this T baseEntity, Entity entity) where T : Entity
        {
            if (entity == null)
            {
                return baseEntity;
            }

            if (baseEntity.LogicalName == null)
            {
                baseEntity.LogicalName = entity.LogicalName;
            }

            if (baseEntity.Id == Guid.Empty && baseEntity.LogicalName == entity.LogicalName)
            {
                baseEntity.Id = entity.Id;
            }

            foreach (var attribute in entity.Attributes.Where(a => !baseEntity.Contains(a.Key)))
            {
                baseEntity[attribute.Key] = attribute.Value;
            }

            return baseEntity;
        }
    }
}