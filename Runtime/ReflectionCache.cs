using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace Weariness.Util.CSV
{
    public static class ReflectionCache
    {
        public delegate void NonRefSetter<T>(T target, object value);
        
        public struct NonRefTypeSetter<T>
        {
            public Type Type;
            public NonRefSetter<T> Setter;
        }

        public static Dictionary<string, NonRefTypeSetter<T>> ClassTypeSetters<T>()
        {
            var typeSetters = new Dictionary<string, NonRefTypeSetter<T>>(StringComparer.OrdinalIgnoreCase);

            // 필드 처리
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var paramTarget = Expression.Parameter(typeof(T), "target");
                var paramValue = Expression.Parameter(typeof(object), "value");

                var assign = Expression.Assign(
                    Expression.Field(paramTarget, field),
                    Expression.Convert(paramValue, field.FieldType)
                );

                var valueType = field.FieldType;
                var lambda = Expression.Lambda<NonRefSetter<T>>(assign, paramTarget, paramValue).Compile();
                typeSetters[field.Name.ToLower()] = new NonRefTypeSetter<T>
                {
                    Type = valueType,
                    Setter = lambda
                };
            }

            // 프로퍼티 처리
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite) continue;

                var paramTarget = Expression.Parameter(typeof(T), "target");
                var paramValue = Expression.Parameter(typeof(object), "value");

                var assign = Expression.Assign(
                    Expression.Property(paramTarget, prop),
                    Expression.Convert(paramValue, prop.PropertyType)
                );

                var valueType = prop.PropertyType;
                var lambda = Expression.Lambda<NonRefSetter<T>>(assign, paramTarget, paramValue).Compile();
                typeSetters[prop.Name.ToLower()] = new NonRefTypeSetter<T>
                {
                    Type = valueType,
                    Setter = lambda
                };
            }

            return typeSetters;
        }
        
        public delegate void RefSetter<T>(ref T target, object value);

        public struct RefTypeSetter<T>
        {
            public Type Type;
            public RefSetter<T> Setter;
        };

        public static Dictionary<string, RefTypeSetter<T>> RefTypeSetters<T>()
        {
            var type = typeof(T);
            var typeSetters = new Dictionary<string, RefTypeSetter<T>>(StringComparer.OrdinalIgnoreCase);

            // 필드 처리
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var paramTarget = Expression.Parameter(typeof(T).MakeByRefType(), "target");
                var paramValue = Expression.Parameter(typeof(object), "value");

                var assign = Expression.Assign(
                    Expression.Field(paramTarget, field),
                    Expression.Convert(paramValue, field.FieldType)
                );

                var valueType = field.FieldType;
                var lambda = Expression.Lambda<RefSetter<T>>(assign, paramTarget, paramValue).Compile();
                typeSetters[field.Name.ToLower()] = new RefTypeSetter<T>
                {
                    Type = valueType,
                    Setter = lambda
                };
            }

            // 프로퍼티 처리
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite) continue;

                var paramTarget = Expression.Parameter(typeof(T).MakeByRefType(), "target");
                var paramValue = Expression.Parameter(typeof(object), "value");

                var assign = Expression.Assign(
                    Expression.Property(paramTarget, prop),
                    Expression.Convert(paramValue, prop.PropertyType)
                );

                var valueType = prop.PropertyType;
                var lambda = Expression.Lambda<RefSetter<T>>(assign, paramTarget, paramValue).Compile();
                typeSetters[prop.Name.ToLower()] = new RefTypeSetter<T>
                {
                    Type = valueType,
                    Setter = lambda
                };
            }

            return typeSetters;
        }

        public struct TypeSetter<T>
        {
            public Type Type;
            public bool IsRef;
            public NonRefSetter<T> NonRefSetter;
            public RefSetter<T> RefSetter;
        }

        public static Dictionary<string, TypeSetter<T>> TypeSetters<T>()
        {
            var type = typeof(T);
            var typeSetters = new Dictionary<string, TypeSetter<T>>(StringComparer.OrdinalIgnoreCase);

            // 필드 처리
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<CSVIgnoreAttribute>() != null) continue;
                
                ParameterExpression paramTarget;
                if(type.IsClass && !field.FieldType.IsArray)
                    paramTarget = Expression.Parameter(typeof(T), "target");
                else
                    paramTarget = Expression.Parameter(typeof(T).MakeByRefType(), "target");
                var paramValue = Expression.Parameter(typeof(object), "value");

                var assign = Expression.Assign(
                    Expression.Field(paramTarget, field),
                    Expression.Convert(paramValue, field.FieldType)
                );

                var fieldName = CSVReader.GetFieldByCSVName(field).ToLower();
                var valueType = field.FieldType;
                if (type.IsClass && !field.FieldType.IsArray)
                {
                    var lambda = Expression.Lambda<NonRefSetter<T>>(assign, paramTarget, paramValue).Compile();
                    typeSetters[fieldName] = new TypeSetter<T>
                    {
                        Type = valueType,
                        IsRef = false,
                        NonRefSetter = lambda
                    };
                }
                else
                {
                    var lambda = Expression.Lambda<RefSetter<T>>(assign, paramTarget, paramValue).Compile();
                    typeSetters[fieldName] = new TypeSetter<T>
                    {
                        Type = valueType,
                        IsRef = true,
                        RefSetter = lambda
                    };
                }
            }

            // 프로퍼티 처리
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite) continue;
                if (prop.GetCustomAttribute<CSVIgnoreAttribute>() != null) continue;

                // 타잎에 맞는 Expression 생성
                ParameterExpression paramTarget;
                if(type.IsClass && !prop.PropertyType.IsArray)
                    paramTarget = Expression.Parameter(typeof(T), "target");
                else
                    paramTarget = Expression.Parameter(typeof(T).MakeByRefType(), "target");
                var paramValue = Expression.Parameter(typeof(object), "value");

                // 결합
                var assign = Expression.Assign(
                    Expression.Property(paramTarget, prop),
                    Expression.Convert(paramValue, prop.PropertyType)
                );
                
                var propertyName = CSVReader.GetPropertyByCSVName(prop).ToLower();
                var valueType = prop.PropertyType;
                if (type.IsClass && !prop.PropertyType.IsArray)
                {
                    var lambda = Expression.Lambda<NonRefSetter<T>>(assign, paramTarget, paramValue).Compile();
                    typeSetters[propertyName] = new TypeSetter<T>
                    {
                        Type = valueType,
                        NonRefSetter = lambda
                    };
                }
                else
                {
                    var lambda = Expression.Lambda<RefSetter<T>>(assign, paramTarget, paramValue).Compile();
                    typeSetters[propertyName] = new TypeSetter<T>
                    {
                        Type = valueType,
                        RefSetter = lambda
                    };
                }
            }

            return typeSetters;
        }
    }
}
