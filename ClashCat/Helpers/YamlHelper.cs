using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ClashCat.Helpers
{
    static class YamlHelper
    {
        private static readonly SerializerBuilder serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .WithTypeConverter(YamlEnumConverter.Create(HyphenatedNamingConvention.Instance))
            .WithTypeConverter(YamlAbstractConverter.Create(() => serializerBuilder!, () => deserializerBuilder!, HyphenatedNamingConvention.Instance));

        private static readonly DeserializerBuilder deserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .WithTypeConverter(YamlEnumConverter.Create(HyphenatedNamingConvention.Instance))
            .WithTypeConverter(YamlAbstractConverter.Create(() => serializerBuilder!, () => deserializerBuilder!, HyphenatedNamingConvention.Instance));


        private static readonly Lazy<ISerializer> laziedSerializer =
            new Lazy<ISerializer>(() => serializerBuilder.Build());

        private static readonly Lazy<IDeserializer> laziedDeserializer = 
            new Lazy<IDeserializer>(() => deserializerBuilder.Build());


        /// <summary>
        /// 全局静态 yaml 序列化器 (使用 - 连字符命名约定)
        /// </summary>
        public static ISerializer Serializer => laziedSerializer.Value;

        /// <summary>
        /// 全局静态 yaml 反序列化器 (从连字符转为大驼峰
        /// </summary>
        public static IDeserializer Deserializer = laziedDeserializer.Value;


        /// <summary>
        /// 用于 YAML 的枚举转换器
        /// </summary>
        private class YamlEnumConverter : IYamlTypeConverter
        {
            private YamlEnumConverter(INamingConvention? namingConvention)
            {
                NamingConvention = namingConvention;
            }

            public INamingConvention? NamingConvention { get; }

            public static YamlEnumConverter Create(INamingConvention? namingConvention) =>
                new YamlEnumConverter(namingConvention);

            public bool Accepts(Type type) => type.IsEnum;

            public object? ReadYaml(IParser parser, Type type)
            {
                var scalar =
                    parser.Consume<Scalar>();

                var enumValues =
                    Enum.GetValues(type);

                foreach (var enumValue in enumValues)
                {
                    var enumValueStr =
                        enumValue.ToString()!;

                    if (enumValueStr == scalar.Value ||
                        NamingConvention?.Apply(enumValueStr) == scalar.Value)
                        return enumValue;
                }

                throw new YamlException(scalar.Start, scalar.End, $"Value '{scalar.Value}' not found in Enum {type}");
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                if (value == null)
                {
                    emitter.Emit(new Scalar("null"));
                    return;
                }

                string write =
                    NamingConvention?.Apply($"{value}") ?? $"{value}";

                if (string.IsNullOrWhiteSpace(write))
                    throw new YamlException($"Cannot write {value} to yaml");

                emitter.Emit(new Scalar(write));
            }
        }

        private class YamlAbstractConverter : IYamlTypeConverter
        {
            private readonly Lazy<SerializerBuilder> laziedSerializerBuilderFactory;
            private readonly Lazy<DeserializerBuilder> laziedDeserializerBuilderFactory;

            private YamlAbstractConverter(Func<SerializerBuilder> serializerBuilderFactory, Func<DeserializerBuilder> deserializerBuilderFactory, INamingConvention? namingConvention)
            {
                laziedSerializerBuilderFactory = new Lazy<SerializerBuilder>(serializerBuilderFactory);
                laziedDeserializerBuilderFactory = new Lazy<DeserializerBuilder>(deserializerBuilderFactory);
                NamingConvention = namingConvention;
            }

            public INamingConvention? NamingConvention { get; }

            public static YamlAbstractConverter Create(Func<SerializerBuilder> serializerBuilderFactory, Func<DeserializerBuilder> deserializerBuilderFactory, INamingConvention? namingConvention) =>
                new YamlAbstractConverter(serializerBuilderFactory, deserializerBuilderFactory, namingConvention);

            public bool Accepts(Type type) => type.CustomAttributes.Any(attrData => attrData.AttributeType.Equals(typeof(YamlAbstractAttribute)));
            public object? ReadYaml(IParser parser, Type type)
            {
                var yamlAbsAttr =
                    type.GetCustomAttribute<YamlAbstractAttribute>();

                if (yamlAbsAttr == null)
                    throw new InvalidOperationException("This would never happen");

                var variants =
                    yamlAbsAttr.Variants;

                var serializer = laziedSerializerBuilderFactory.Value
                    .WithoutTypeConverter<YamlAbstractConverter>()
                    .Build();

                var deserializer = laziedDeserializerBuilderFactory.Value
                    .WithoutTypeConverter<YamlAbstractConverter>()
                    .Build();

                var yamlObject =
                    deserializer.Deserialize(parser);

                var yamlText =
                    serializer.Serialize(yamlObject!);

                if (yamlObject == null)
                    return null;

                if (yamlObject is not Dictionary<object, object> yamlDict)
                    throw new InvalidOperationException("This would never happen");

                foreach (var variant in variants)
                {
                    var signName =
                        variant.VariantAttribute.SignName;
                    var signRealName =
                        NamingConvention?.Apply(signName) ?? signName;
                    var signValue =
                        variant.VariantAttribute.SignValue;

                    if (yamlDict.TryGetValue(signName, out object? dictSignValue) && dictSignValue.Equals(signValue))
                        return deserializer.Deserialize(yamlText, variant.VariantType);
                    if (yamlDict.TryGetValue(signRealName, out dictSignValue) && dictSignValue.Equals(signValue))
                        return deserializer.Deserialize(yamlText, variant.VariantType);
                }

                throw new InvalidOperationException($"Could not find variant for type {type}")
                {
                    Data =
                    {
                        { "YamlText", yamlText }
                    }
                };
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                var serializer = laziedSerializerBuilderFactory.Value
                    .WithoutTypeConverter<YamlAbstractConverter>()
                    .Build();

                serializer.Serialize(emitter, value!, type);
            }
        }
    }


    public record struct YamlVariant(Type VariantType, YamlVariantAttribute VariantAttribute);

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class YamlAbstractAttribute : Attribute
    {
        public YamlAbstractAttribute(params Type[] variantTypes)
        {
            Variants = new YamlVariant[variantTypes.Length];

            for (int i = 0; i < variantTypes.Length; i++)
            {
                if (variantTypes[i].GetCustomAttribute<YamlVariantAttribute>() is not YamlVariantAttribute variantAttr)
                    throw new ArgumentException("Specified variants contain invalid variant", nameof(variantTypes));
                Variants[i] = new YamlVariant(variantTypes[i], variantAttr);
            }
        }

        public YamlVariant[] Variants { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class YamlVariantAttribute : Attribute
    {
        public YamlVariantAttribute(string signName, object? signValue)
        {
            SignName = signName;
            SignValue = signValue;
        }

        public string SignName { get; }
        public object? SignValue { get; }
    }
}
