﻿using BanBrick.TypeScript.CodeGenerator.Enums;
using BanBrick.TypeScript.CodeGenerator.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BanBrick.TypeScript.CodeGenerator.Generators
{
    public class ValueCodeGenerator
    {
        private readonly TypeHelper _typeHelper;
        private readonly PropertyHelper _propertyHelper;
        private readonly StringHelper _stringHelper;

        public ValueCodeGenerator()
        {
            _typeHelper = new TypeHelper();
            _propertyHelper = new PropertyHelper();
            _stringHelper = new StringHelper();
        }

        /// <summary>
        /// Generate value code for typescript in including value check
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GenerateValueCode(Type type, object value)
        {
            var typeCategory = _typeHelper.GetTypeCategory(type);

            if (typeCategory == TypeCategory.Primitive)
            {
                if (_typeHelper.IsPrimitiveType(type) && value == null)
                    return null;

                return GetPrimitiveValueCode(value);
            }

            if (typeCategory == TypeCategory.Enum)
            {
                return GetEnumValueCode(_typeHelper.GetTypeScriptName(type), value);
            }

            if (typeCategory == TypeCategory.Collection)
            {
                return GenerateArrayValueCode(value);
            }

            if (typeCategory == TypeCategory.Dictionary)
            {
                return GenerateDictionaryCode(value);
            }

            if (typeCategory == TypeCategory.Object)
            {
                return GenerateObjectCode(value);
            }

            return "";
        }
        
        public string GetPrimitiveValueCode(object value)
        {
            if (value == null) return "null";

            var type = value.GetType();

            if (type == typeof(bool))
                return (bool)value ? "true" : "false";

            if (_typeHelper.IsNumericType(type))
                return value.ToString();

            if (type == typeof(string))
                return GetStringValueCode((string)value);

            return null;
        }
        
        public string GenerateArrayValueCode(object value)
        {
            if (value == null)
                return "[]";

            var arrayValues = value as IEnumerable;
            var arrayValuesCode = new List<string>();

            foreach (var arrayValue in arrayValues)
            {
                arrayValuesCode.Add(GenerateValueCode(arrayValue.GetType(), arrayValue));
            }

            return $"[ {string.Join(", ", arrayValuesCode)} ]";
        }

        public string GenerateDictionaryCode(object value)
        {
            if (value == null)
                return "{}";

            var dictionaryValue = value as IDictionary;
            var dictionaryValuesCode = new List<string>();

            foreach (var key in dictionaryValue.Keys)
            {
                var keyValue = dictionaryValue[key];
                var keyType = key.GetType();
                var keyValueType = keyValue.GetType();

                dictionaryValuesCode.Add($"{GenerateValueCode(keyType, key)}: {GenerateValueCode(keyValueType, keyValue)}");
            }

            return $"{{ {string.Join(", ", dictionaryValuesCode)} }}";
        }

        public string GenerateObjectCode(object value)
        {
            if (value == null)
                return "";

            var objectType = value.GetType();
            var properties = TypeExtensions.GetProperties(objectType);
            var objectPropertiesCode = new List<string>();

            foreach (var property in properties)
            {
                if (_propertyHelper.IsTypeScriptIgnored(property))
                    continue;

                var propertyType = property.PropertyType;
                var propertyName = _typeHelper.GetTypeScriptName(propertyType);
                var valueCode = GenerateValueCode(propertyType, property.GetValue(value));

                objectPropertiesCode.Add($"{_stringHelper.ToCamelCase(property.Name)}: {valueCode}");
            }
            return $"{{ {string.Join(",\n", objectPropertiesCode)} }}";
        }
        
        public string GetStringValueCode(string value)
        {
            return "'" + (value?.Replace("'", "\'") ?? "") + "'";
        }

        public string GetEnumValueCode(string enumName, object value)
        {
            return $"{enumName}.{value.ToString()}";
        }
    }
}
