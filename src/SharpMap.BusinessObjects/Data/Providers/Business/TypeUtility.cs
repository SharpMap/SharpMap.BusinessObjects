/*
 * Copyright © 2013 - Felix Obermaier, Ingenieurgruppe IVV GmbH & Co. KG
 * 
 * This file is part of SharpMap.BusinessObjects.
 *
 * SharpMap.BusinessObjects is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharpMap.BusinessObjects is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with SharpMap; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
 *
 * Code based on 
 * https://www.codeproject.com/articles/14560/fast-dynamic-property-field-accessors
 * 
 */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpMap.Data.Providers.Business
{
    /// <summary>
    /// A utility class to help with feature objects
    /// </summary>
    /// <typeparam name="TObjectType"></typeparam>
    internal static class TypeUtility<TObjectType>
    {
        public delegate TMemberType MemberGetDelegate<out TMemberType>(TObjectType obj);

        public delegate object MemberGetDelegate(TObjectType obj);

        public static MemberGetDelegate<TMemberType> GetMemberGetDelegate<TMemberType>(string memberName)
        {
            var objectType = typeof(TObjectType);
            
            var pi = objectType.GetProperty(memberName);
            var fi = objectType.GetField(memberName);
            if (pi != null)
            {
                // Member is a Property...

                var mi = pi.GetGetMethod();
                if (mi != null)
                {
                    // NOTE:  As reader J. Dunlap pointed out...
                    //  Calling a property's get accessor is faster/cleaner using
                    //  Delegate.CreateDelegate rather than Reflection.Emit 
                    return (MemberGetDelegate<TMemberType>)
                        Delegate.CreateDelegate(typeof(MemberGetDelegate<TMemberType>), mi);
                }
                throw new Exception(string.Format(
                    "Property: '{0}' of Type: '{1}' does" +
                    " not have a Public Get accessor",
                    memberName, objectType.Name));
            }

            if (fi != null)
            {
                // Member is a Field...

                var dm = new DynamicMethod("Get" + memberName,
                    typeof(TMemberType), new[] { objectType }, objectType);
                var il = dm.GetILGenerator();
                // Load the instance of the object (argument 0) onto the stack
                il.Emit(OpCodes.Ldarg_0);
                // Load the value of the object's field (fi) onto the stack
                il.Emit(OpCodes.Ldfld, fi);
                // return the value on the top of the stack
                il.Emit(OpCodes.Ret);

                return (MemberGetDelegate<TMemberType>)
                    dm.CreateDelegate(typeof(MemberGetDelegate<TMemberType>));
            }

            throw new Exception(String.Format(
                "Member: '{0}' is not a Public Property or Field of Type: '{1}'",
                memberName, objectType.Name));
        }

        internal static MemberGetDelegate<TMemberType> GetMemberGetDelegate<TMemberType>(Type attributeType )
        {
            var objectType = typeof (TObjectType);
            return GetMemberGetDelegate<TMemberType>(objectType, attributeType);
        }

        internal static MemberGetDelegate<TMemberType> GetMemberGetDelegate<TMemberType>(Type objectType, Type attributeType)
        {
            var pis = objectType.GetProperties(/*BindingFlags.GetProperty | BindingFlags.Public*/);
            foreach (var propertyInfo in pis)
            {
                var att = propertyInfo.GetCustomAttributes(attributeType, true);
                if (att.Length > 0)
                    return GetMemberGetDelegate<TMemberType>(propertyInfo.Name);
            }

            var fis = objectType.GetFields(BindingFlags.GetField | BindingFlags.Public);
            foreach (var fieldInfo in fis)
            {
                var att = fieldInfo.GetCustomAttributes(attributeType, true);
                if (att.Length > 0)
                    return GetMemberGetDelegate<TMemberType>(fieldInfo.Name);
            }

            if (objectType.BaseType != typeof (object))
                return GetMemberGetDelegate<TMemberType>(objectType.BaseType, attributeType);

            throw new ArgumentException("Attribute not declared on public field or property", "attributeType");
        }

        public static Type GetMemberType(string memberName)
        {
            return TypeUtility.GetMemberType(typeof (TObjectType), memberName);
        }
    }

    internal static class TypeUtility
    {
        public static Type GetMemberType(Type objectType, string memberName)
        {
            var pi = objectType.GetProperty(memberName);
            var fi = objectType.GetField(memberName);
            if (pi != null)
            {
                // Member is a Property...
                return pi.PropertyType;
            }
            if (fi != null)
            {
                // Member is a Field...
                return fi.FieldType;
            }
            if (objectType.BaseType != typeof (object))
                return GetMemberType(objectType.BaseType, memberName);

            throw new Exception("Member '" + memberName + "' not found in type '"+ objectType.Name + "'!");
        }
    }
}
