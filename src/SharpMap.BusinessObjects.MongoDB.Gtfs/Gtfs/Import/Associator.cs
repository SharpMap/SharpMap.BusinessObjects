using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Common.Logging;
using MongoDB.Bson.Serialization.Attributes;

namespace SharpMap.Data.Providers.Business.MongoDB.Gtfs.Import
{
    internal class Associator<T> where T:class,new()
    {
        private readonly Dictionary<string, Tuple<MethodInfo, Func<string, object>>> _association;

        public Associator(string csvHeader)
        {
            _association = new Dictionary<string, Tuple<MethodInfo,Func<string,object>>>();
            var t = typeof(T);
            var propertyInfos = t.GetProperties();
            foreach (var match in ImportFromFolder.Csv.Matches(csvHeader))
            {
                var matchString = match.ToString();
                var propertyInfo = GetPropertyInfo(propertyInfos, matchString);
                if (propertyInfo != null)
                    _association.Add(matchString, Tuple.Create(propertyInfo.GetSetMethod(), GetConversion(propertyInfo.PropertyType)));
                else
                    _association.Add(matchString, null);
            }
        }

        public CultureInfo CultureInfo { get; set; }

        private static PropertyInfo GetPropertyInfo(PropertyInfo[] propertyInfos, string matchString)
        {
            foreach (var propertyInfo in propertyInfos)
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(BsonElementAttribute), false);
                if (attributes.Length > 0)
                {
                    var tmpBsonElementAttribute = (BsonElementAttribute)attributes[0];
                    if (tmpBsonElementAttribute.ElementName == matchString)
                        return propertyInfo;
                }
            }
            return null;
        }

        private static readonly DateTimeFormatInfo _dateTimeFormatInfo = new DateTimeFormatInfo() {FullDateTimePattern = "yyyyMMdd"} ;

        /// <summary>
        /// Function to get an object conversion
        /// </summary>
        /// <param name="t">The type to convert to</param>
        /// <returns>A function delegate to convert to the specified type</returns>
        private Func<string, object> GetConversion(Type t)
        {
            if (t.IsEnum)
                return s => string.IsNullOrWhiteSpace(s) ? null : Enum.ToObject(t, int.Parse(s, NumberFormatInfo.InvariantInfo));

            if (t == typeof(short))
                return s => string.IsNullOrWhiteSpace(s) ? 0 : short.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(bool))
                return s => string.IsNullOrWhiteSpace(s)
                    ? false
                    : s == "-1" || string.Equals(s, "true", StringComparison.InvariantCultureIgnoreCase);

            if (t == typeof(int))
                return s => string.IsNullOrWhiteSpace(s) ? 0 : int.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(Nullable<int>))
                return s => string.IsNullOrWhiteSpace(s) ? null : (object)int.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(uint))
                return s => string.IsNullOrWhiteSpace(s) ? 0 : uint.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(uint?))
                return s => string.IsNullOrWhiteSpace(s) ? null : (object)uint.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(float))
                return s => string.IsNullOrWhiteSpace(s) ? 0f : float.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(double))
                return s => string.IsNullOrWhiteSpace(s) ? 0d : double.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(double?))
                return s => string.IsNullOrWhiteSpace(s) ? null : (object)double.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(long))
                return s => string.IsNullOrWhiteSpace(s) ? 0L : long.Parse(s, NumberFormatInfo.InvariantInfo);

            if (t == typeof(string))
                return s => s;

            if (t == typeof(DateTime))
                return s => string.IsNullOrWhiteSpace(s)
                    ? DateTime.MinValue
                    : new DateTime(int.Parse(s.Substring(0, 4)), 
                        int.Parse(s.Substring(4, 2)),
                        int.Parse(s.Substring(6, 2)));

                //return delegate(string s)
                //{
                //    if (string.IsNullOrWhiteSpace(s))
                //        return DateTime.MinValue;
                //    var parts = s.Split(':');
                //    var result = DateTime.MinValue;
                //    result = result.AddHours(double.Parse(parts[0], NumberFormatInfo.InvariantInfo));
                //    result = result.AddMinutes(double.Parse(parts[1], NumberFormatInfo.InvariantInfo));
                //    return result.AddSeconds(double.Parse(parts[2], NumberFormatInfo.InvariantInfo));
                //};

            if (t == typeof (TimeSpan))
                return delegate(string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return new TimeSpan();
                    var parts = s.Split(':');
                    var days = 0;
                    var hours = int.Parse(parts[0]);
                    while (hours > 23)
                    {
                        days++;
                        hours -= 24;
                    }
                    var minutes = int.Parse(parts[1]);
                    var seconds = int.Parse(parts[2]);
                    return new TimeSpan(days, hours, minutes, seconds);
                };

                //return s => string.IsNullOrWhiteSpace(s)
                //    ? new TimeSpan(0)
                //    : TimeSpan.ParseExact(s, @"%h\:mm\:ss", NumberFormatInfo.InvariantInfo);
                

            throw new NotImplementedException(string.Format("GetConversion for '{0}' not implemented", t.FullName));
        }

        public T Associate(T item, string line)
        {
            var i = 0;
            var elements = ImportFromFolder.Csv.Matches(line);
            foreach (var kvp in _association)
            {
                if (kvp.Value != null)
                {
                    var method = kvp.Value.Item1;
                    var conversion = kvp.Value.Item2;
                    method.Invoke(item, new[] { conversion(elements[i].ToString()) });
                }
                i++;
            }
            return item;
        }

        public static IEnumerable<T> Read(StreamReader reader)
        {
            var log = LogManager.GetCurrentClassLogger();
            log.DebugFormat("Thread {0}: Start Importing {1}", Thread.CurrentThread.ManagedThreadId, typeof(T).Name);
            var associator = new Associator<T>(reader.ReadLine());
            var items = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                yield return associator.Associate(new T(), line);
                items++;
                if (items % 250 == 0)
                    log.DebugFormat("Thread {0}: Importing {1}: {2} items imported.", Thread.CurrentThread.ManagedThreadId, typeof(T).Name, items);
            }
            log.DebugFormat("Thread {0}: End import of {1}. {2} items imported.", Thread.CurrentThread.ManagedThreadId, typeof(T).Name, items);
        }
    }
}