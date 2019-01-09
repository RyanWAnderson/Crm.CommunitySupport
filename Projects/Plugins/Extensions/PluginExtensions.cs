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
                return $"(EntityReference){entityReference.LogicalName}({entityReference.Id})";
            }
        }

        /// <summary>
        /// Get a traceable string representation of an OptionSetValue 
        /// </summary>
        public static string ToTraceable(this OptionSetValue optionSetValue)
        {
            return $"(OptionSetValue){ optionSetValue?.Value.ToString() ?? "null"}";
        }

        /// <summary>
        /// Get a traceable string representation of a Money 
        /// </summary>
        public static string ToTraceable(this Money money)
        {
            return $"(Money){ money.Value.ToString() ?? "null"}";
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
                sb.AppendLine($"(Entity<{entity.LogicalName}>)");

                foreach (var attributeName in entity.Attributes.Keys)
                {
                    var attributeValue = entity[attributeName];
                    string typeAndValue;

                    try
                    {
                        if (attributeValue == null)
                        {
                            typeAndValue = "(null)";
                        }
                        else
                        {
                            switch (attributeValue)
                            {
                                case OptionSetValue optionSetValue:
                                    typeAndValue = optionSetValue.ToTraceable();
                                    break;

                                case Money moneyValue:
                                    typeAndValue = moneyValue.ToTraceable();
                                    break;

                                case EntityReference entityReference:
                                    typeAndValue = entityReference.ToTraceable();
                                    break;

                                default:
                                    typeAndValue = $"({attributeValue.GetType().FullName}){attributeValue}";
                                    break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        typeAndValue = "<Error serializing>" + ex.Message;
                    }

                    sb.AppendLine($"{attributeName}: {typeAndValue}");
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

            sb.AppendLine($"(DataCollection<{typeof(TKey).FullName},{typeof(TValue).FullName}>)");

            foreach (var kvp in collection)
            {
                var keyName = kvp.Key;
                var valueTypeName = kvp.Value.GetType().FullName;
                var formattedValue = kvp.Value.ToString().IndentNewLines();
                sb.AppendLine($"{keyName}: ({valueTypeName}) {formattedValue},");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get a traceable string representation of the SdkMessage info that triggered the plugin
        /// </summary>
        public static string ToTraceableMessage(this IPluginExecutionContext context)
        {
            var mode = ((ExecutionMode)context.Mode);
            var stage = (PluginStage)context.Stage;
            var primaryEntityName = string.IsNullOrEmpty(context.PrimaryEntityName) ? "any entity" : context.PrimaryEntityName;
            var primaryEntityId = (context.PrimaryEntityId == null || context.PrimaryEntityId == Guid.Empty) ? string.Empty : $"({context.PrimaryEntityId})";
            var messageName = context.MessageName;

            return $"{mode} {stage} of {primaryEntityName}{primaryEntityId}.{messageName}";
        }
    }
}