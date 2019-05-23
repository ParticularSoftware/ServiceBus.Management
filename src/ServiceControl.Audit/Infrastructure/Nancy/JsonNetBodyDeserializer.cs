﻿namespace ServiceBus.Management.Infrastructure.Nancy
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using global::Nancy.Extensions;
    using global::Nancy.ModelBinding;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using ServiceControl.Audit.Infrastructure;

    class JsonNetBodyDeserializer : IBodyDeserializer
    {
        /// <summary>
        /// Empty constructor if no converters are needed
        /// </summary>
        public JsonNetBodyDeserializer()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new UnderscoreMappingResolver(),
                Converters =
                {
                    new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.RoundtripKind },
                    new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() }
                }
            };
            serializer = JsonSerializer.Create(serializerSettings);
        }

        /// <summary>
        /// Constructor to use when a custom serializer are needed.
        /// </summary>
        /// <param name="serializer">Json serializer used when deserializing.</param>
        public JsonNetBodyDeserializer(JsonSerializer serializer)
        {
            this.serializer = serializer;
        }

        public bool CanDeserialize(string contentType, BindingContext context)
        {
            return Helpers.IsJsonType(contentType);
        }

        /// <summary>
        /// Deserialize the request body to a model
        /// </summary>
        /// <param name="contentType">Content type to deserialize</param>
        /// <param name="bodyStream">Request body stream</param>
        /// <param name="context">Current context</param>
        /// <returns>Model instance</returns>
        public object Deserialize(string contentType, Stream bodyStream, BindingContext context)
        {
            var deserializedObject =
                serializer.Deserialize(new StreamReader(bodyStream), context.DestinationType);

            var properties =
                context.DestinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new BindingMemberInfo(p));

            var fields =
                context.DestinationType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Select(f => new BindingMemberInfo(f));

            if (properties.Concat(fields).Except(context.ValidModelBindingMembers).Any())
            {
                return CreateObjectWithBlacklistExcluded(context, deserializedObject);
            }

            return deserializedObject;
        }

        private static object ConvertCollection(object items, Type destinationType, BindingContext context)
        {
            var returnCollection = Activator.CreateInstance(destinationType);

            var collectionAddMethod =
                destinationType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

            foreach (var item in (IEnumerable)items)
            {
                collectionAddMethod.Invoke(returnCollection, new[] {item});
            }

            return returnCollection;
        }

        private static object CreateObjectWithBlacklistExcluded(BindingContext context, object deserializedObject)
        {
            var returnObject = Activator.CreateInstance(context.DestinationType, true);

            if (context.DestinationType.IsCollection())
            {
                return ConvertCollection(deserializedObject, context.DestinationType, context);
            }

            foreach (var property in context.ValidModelBindingMembers)
            {
                CopyPropertyValue(property, deserializedObject, returnObject);
            }

            return returnObject;
        }

        private static void CopyPropertyValue(BindingMemberInfo property, object sourceObject, object destinationObject)
        {
            property.SetValue(destinationObject, property.GetValue(sourceObject));
        }

        readonly JsonSerializer serializer;
    }
}