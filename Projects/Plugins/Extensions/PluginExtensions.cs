using Crm.CommunitySupport.Plugins;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Text;

namespace Crm.CommunitySupport.Extensions
{
    /// Extension methods that are useful in plugin development
    public static partial class PluginExtensions
    {
        /// <summary>
        /// Generic method to beautify the use of GetService()
        /// </summary>
        public static TService GetService<TService>(this IServiceProvider service) where TService : class
        {
            return (TService)service.GetService(typeof(TService));
        }

        /// Get an EntityImage based on imageName. If no imageName is provided, assume that there is exactly 1 image registered.
        public static TEntity GetImage<TEntity>(this EntityImageCollection images, string imageName = "") where TEntity : Entity
        {
            if (string.IsNullOrEmpty(imageName))
            {
                imageName = images.Keys.Single();
            }

            images.TryGetValue(imageName, out var image);

            return image?.ToEntity<TEntity>();
        }

        /// <summary>
        /// Get a traceable string representation of an EntityReference 
        /// </summary>
        public static string ToTraceable(this EntityReference entityReference)
        {
            if (entityReference == null)
            {
                return "(EntityReference)null";
            }
            else
            {
                return string.Format("(EntityReference){0}({1})", entityReference.LogicalName, entityReference.Id.ToString());
            }
        }

        /// <summary>
        /// Get a traceable string representation of an OptionSetValue 
        /// </summary>
        public static string ToTraceable(this OptionSetValue optionSetValue)
        {
            return string.Format("(OptionSetValue){0}", optionSetValue == null ? "null" : optionSetValue.Value.ToString());
        }

        /// <summary>
        /// Get a traceable string representation of a Money 
        /// </summary>
        public static string ToTraceable(this Money money)
        {
            return string.Format("(Money){0}", money == null ? "null" : money.Value.ToString());
        }

        /// <summary>
        /// Get a deep, traceable string representation of an Entity
        /// </summary>
        public static string ToTraceable(this Entity entity)
        {
            if (entity == null)
            {
                return "(Entity) null";
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendFormat("(Entity<{0}>)", entity.LogicalName).AppendLine();

                if (entity.Attributes.Any())
                {
                    foreach (var attributeName in entity.Attributes.Keys)
                    {
                        var attributeValue = entity[attributeName];
                        string typeAndValue;

                        try
                        {
                            if (attributeValue == null)
                            {
                                typeAndValue = string.Format("(null)");
                            }
                            else
                            {

                                if (attributeValue is OptionSetValue)
                                {
                                    typeAndValue = ((OptionSetValue)attributeValue).ToTraceable();
                                }
                                else if (attributeValue is Money)
                                {
                                    typeAndValue = ((Money)attributeValue).ToTraceable();
                                }
                                else if (attributeValue is EntityReference)
                                {
                                    typeAndValue = ((EntityReference)attributeValue).ToTraceable();
                                }
                                else
                                {
                                    typeAndValue = string.Format(
                                        "({0}){1}",
                                        attributeValue.GetType().FullName,
                                        attributeValue);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            typeAndValue = "<Error serializing>" + ex.Message;
                        }

                        sb.AppendFormat("{0}: {1}", attributeName, typeAndValue).AppendLine();

                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Get a traceable string representation of a DataCollection
        /// </summary>
        public static string ToTraceable<TKey, TValue>(this DataCollection<TKey, TValue> collection)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("(DataCollection<{0},{1}>)", typeof(TKey).FullName, typeof(TValue).FullName).AppendLine();

            foreach (var kvp in collection)
            {
                sb.AppendFormat(
                    "{0}: ({1}) {2},",
                    kvp.Key,
                    kvp.Value.GetType().FullName,
                    kvp.Value.ToString().IndentNewLines()
                    ).AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get a traceable string representation of the SdkMessage info that triggered the plugin
        /// </summary>
        public static string ToTraceableMessage(this IPluginExecutionContext context)
        {
            return string.Format(
                "{0} {1} of {2}{3}.{4}{5}",
                (ExecutionMode)context.Mode,
                (PluginStage)context.Stage,
                string.IsNullOrEmpty(context.PrimaryEntityName)
                    ? "<global>"
                    : context.PrimaryEntityName,
                context.PrimaryEntityId == null || context.PrimaryEntityId == Guid.Empty
                    ? string.Empty
                    : string.Format("({0})", context.PrimaryEntityId),
                context.MessageName,
                string.IsNullOrEmpty(context.SecondaryEntityName)
                    ? string.Empty
                    : string.Format("/{0}", context.SecondaryEntityName)
                );
        }
    }
}