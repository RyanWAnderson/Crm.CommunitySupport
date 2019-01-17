using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.CommunitySupport.Extensions
{
    public static partial class XrmExtensions
    {
        /// <summary>
        /// Make a comparable, but not equatable copy of an Entity.
        /// </summary>
        public static Entity Copy(this Entity sourceEntity)
        {
            var copy = new Entity(sourceEntity.LogicalName, sourceEntity.Id)
            {
                KeyAttributes = sourceEntity.KeyAttributes.Copy(),
                //clone.EntityState = EntityState.Unchanged;
                //clone.RowVersion = e.RowVersion;
                Attributes = sourceEntity.Attributes.Copy()
            };
            return copy;
        }

        /// <summary>
        /// Make a comparable, but not equatable copy of a TDataCollection.
        /// </summary>
        public static TDataCollection Copy<TDataCollection>(this TDataCollection source, TDataCollection target = null) where TDataCollection : DataCollection<string, object>, new()
        {
            var copy = target ?? new TDataCollection();

            // Copy any items in the collection
            foreach (var attribute in source)
            {
                var attributeValue = attribute.Value;

                switch (attributeValue)
                {
                    case OptionSetValue optionSetValue:
                        attributeValue = optionSetValue.Copy();
                        break;
                    case Money money:
                        attributeValue = money.Copy();
                        break;
                    case EntityReference entityReference:
                        attributeValue = entityReference.Copy();
                        break;
                }

                copy[attribute.Key] = attributeValue;
            }

            return copy;
        }

        /// <summary>
        /// Make a comparable, but not equatable copy of an OptionSetValue
        /// </summary>
        public static OptionSetValue Copy(this OptionSetValue source)
        {
            return new OptionSetValue(source.Value);
        }

        /// <summary>
        /// Make a comparable, but not equatable copy of a Money
        /// </summary>
        public static Money Copy(this Money source)
        {
            return new Money(source.Value);
        }

        /// <summary>
        /// Make a comparable, but not equatable copy of an EntityReference
        /// </summary>
        public static EntityReference Copy(this EntityReference source)
        {
            return new EntityReference(source.LogicalName, source.Id);
        }

        public static bool IsReferenceTo(this EntityReference entityReference, Entity entity)
        {
            return (string.Compare(entityReference.LogicalName, entity.LogicalName) == 0 && entityReference.Id.CompareTo(entity.Id) == 0);
        }
        /// <summary>
        /// Apply changes from a delta to an Entity
        /// </summary>
        public static void ApplyDelta(this Entity entity, Entity delta)
        {
            if (delta == null)
            {
                return;
            }

            if (!delta.ToEntityReference().IsReferenceTo(entity))
            {
                throw new ArgumentException(string.Format(
                    "Argument 'delta' refers to {0}, not {1}.",
                    delta.ToEntityReference().ToTraceable(),
                    entity.ToEntityReference().ToTraceable()));
            }

            Copy(delta.Attributes, entity.Attributes);
        }

        /// <summary>
        /// Remove redundant attributes based on a PreEntityImage
        /// Returns: The logical names of fields that were removed.
        /// </summary>
        public static IEnumerable<string> ReduceToDelta(this Entity currentImage, Entity previousImage, params string[] fieldsToPreserve)
        {
            var removedFields = new List<string>();

            if (previousImage == null)
            {
                return removedFields;
            }

            IEnumerable<string> fieldsToConsider = currentImage.Attributes.Keys;

            if (fieldsToPreserve != null)
            {
                fieldsToConsider = fieldsToConsider.Except(fieldsToPreserve);
            }

            foreach (var attributeName in fieldsToConsider)
            {
                if (!IsAttributeNeededInTarget(currentImage, previousImage, attributeName))
                {
                    currentImage.Attributes.Remove(attributeName);
                    removedFields.Add(attributeName);
                }
            }

            return removedFields;
        }

        /// <summary>
        /// Compare one Entity to another, returning a copy of the Entity with only new/changed attributes
        /// </summary>
        /// <remarks>Returns an Entity so that the result can be sent to IOrganizationService.Update()</remarks>
        public static Entity GetDeltaFrom(this Entity currentImage, Entity previousImage)
        {
            var deltaEntity = currentImage.Copy();
            deltaEntity.ReduceToDelta(previousImage);
            return deltaEntity;
        }

        /// <summary>
        /// Determine if the given attribute is needed when creating a delta
        /// </summary>
        private static bool IsAttributeNeededInTarget(Entity target, Entity preImage, string attributeName)
        {
            var targetValue = target[attributeName];
            var preImageValue = preImage.Contains(attributeName) ? preImage[attributeName] : null;

            if (!preImage.Contains(attributeName))
            {
                return true;
            }
            if (targetValue == null)
            {
                return preImageValue != null;
            }
            if (targetValue == preImageValue ||
                (targetValue is IComparable && ((IComparable)targetValue).CompareTo(preImageValue) == 0))
            {
                return false;
            }

            // specific cases
            if (targetValue is Money
                && ((Money)targetValue).Value == ((Money)preImageValue).Value)
            {
                return false;
            }

            if (targetValue is OptionSetValue
                && ((OptionSetValue)targetValue).Value == ((OptionSetValue)preImageValue).Value)
            {
                return false;
            }

            if (targetValue is EntityReference)
            {
                var targetRef = targetValue as EntityReference;
                var preImageRef = preImageValue as EntityReference;
                if (targetRef.LogicalName == preImageRef.LogicalName && targetRef.Id == preImageRef.Id)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Copies the given attributes to a new entity.
        /// </summary>
        /// <param name="source">The entity from which to copy values.</param>
        /// <param name="attributeMap">A dictionary of {source, target} attribute names.</param>
        /// <param name="targetEntityName">Optional. Logical name of the entity. Defaults to the source entity's name.</param>
        /// <returns></returns>
        public static Entity MapAttributes(this Entity source, IDictionary<string, string> attributeMap, string targetEntityName = null)
        {
            var target = new Entity(targetEntityName ?? source.LogicalName);

            foreach (var attribute in attributeMap)
            {
                if (source.Attributes.TryGetValue(attribute.Key, out var attributeValue))
                {
                    target[attribute.Value] = attributeValue;
                }
            }

            return target;
        }
    }
}
