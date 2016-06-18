using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Crm.CommunitySupport.Extensions {
    public static partial class XrmExtensions {
        public static Entity Clone(this Entity source) {
            Entity clone = new Entity(source.LogicalName, source.Id);
            clone.KeyAttributes = source.KeyAttributes.Clone();
            //clone.EntityState = EntityState.Unchanged;
            //clone.RowVersion = e.RowVersion;
            clone.ApplyDelta(source.Attributes);
            return clone;
        }

        public static KeyAttributeCollection Clone(this KeyAttributeCollection source) {
            KeyAttributeCollection clone = new KeyAttributeCollection();

            // Copy any items in the collection
            foreach (KeyValuePair<string, object> currentItem in source) {
                object value = currentItem.Value;

                if (value is OptionSetValue) {
                    value = ((OptionSetValue)value).Clone();
                } else if (value is Money) {
                    value = ((Money)value).Clone();
                } else if (value is EntityReference) {
                    value = ((EntityReference)value).Clone();
                }

                clone[currentItem.Key] = value;
            }

            return clone;
        }

        /// <summary>
        /// Make a comparable, but not equatable copy of an OptionSetValue
        /// </summary>
        public static OptionSetValue Clone(this OptionSetValue source) {
            return new OptionSetValue(source.Value);
        }

        /// <summary>
        /// Make a comparable, but not equatable copy of a Money
        /// </summary>
        public static Money Clone(this Money source) {
            return new Money(source.Value);
        }

        /// <summary>
        /// Make a comparable, but not equatable copy of an EntityReference
        /// </summary>
        public static EntityReference Clone(this EntityReference source) {
            return new EntityReference(source.LogicalName, source.Id);
        }

        /// <summary>
        /// Apply changes from a delta to an Entity
        /// </summary>
        public static void ApplyDelta(this Entity entity, AttributeCollection patch) {
            foreach (KeyValuePair<string, object> kvp in patch) {
                object value = kvp.Value;

                if (value is OptionSetValue) {
                    value = ((OptionSetValue)value).Clone();

                } else if (kvp.Value is Money) {
                    value = ((Money)value).Clone();

                } else if (kvp.Value is EntityReference) {
                    value = ((EntityReference)value).Clone();

                }

                entity.Attributes[kvp.Key] = value;
            }
        }

        /// <summary>
        /// Remove redundant attributes based on a PreEntityImage
        /// </summary>
        /// <param name="currentImage"></param>
        /// <param name="previousImage"></param>
        public static void ReduceToDelta(this Entity currentImage, Entity previousImage) {
            // REMINDER: Don't modify the collection we are iterating over
            foreach (string attributeName in currentImage.Attributes.Keys) {
                if (!IsAttributeNeededInTarget(currentImage, previousImage, attributeName)) {
                    currentImage.Attributes.Remove(attributeName);
                }
            }
        }
        /// <summary>
        /// Compare one Entity to another, returning only new/changed attributes
        /// </summary>
        public static AttributeCollection GetDeltaFrom(this Entity currentImage, Entity previousImage) {
            Entity deltaEntity = currentImage.Clone();
            deltaEntity.ReduceToDelta(previousImage);
            return deltaEntity.Attributes;
        }

        /// <summary>
        /// Determine if the given attribute is needed when creating a delta
        /// </summary>
        private static bool IsAttributeNeededInTarget(Entity target, Entity preImage, string attributeName) {
            object targetValue = target[attributeName];
            object preImageValue = preImage.Contains(attributeName) ? preImage[attributeName] : null;

            if (!preImage.Contains(attributeName)) {
                return true;
            }
            if (targetValue == null) {
                return preImageValue != null;
            }
            if (targetValue == preImageValue ||
                (targetValue is IComparable && ((IComparable)targetValue).CompareTo(preImageValue) == 0)) {
                return false;
            }

            // specific cases
            if (targetValue is Money
                && ((Money)targetValue).Value == ((Money)preImageValue).Value) {
                return false;
            }

            if (targetValue is OptionSetValue
                && ((OptionSetValue)targetValue).Value == ((OptionSetValue)preImageValue).Value) {
                return false;
            }

            if (targetValue is EntityReference) {
                EntityReference targetRef = targetValue as EntityReference;
                EntityReference preImageRef = preImageValue as EntityReference;
                if (targetRef.LogicalName == preImageRef.LogicalName && targetRef.Id == preImageRef.Id) {
                    return false;
                }
            }

            return true;
        }
    }
}
