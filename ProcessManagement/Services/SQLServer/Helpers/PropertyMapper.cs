using ProcessManagement.Commons;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace ProcessManagement.Services.SQLServer.Helpers
{
    /// <summary>
    /// Helper class để map Propertyy từ SqlDataReader
    /// OPTIMIZED: Cache property list và column ordinals để giảm reflection overhead
    /// 
    /// TÍNH LINH ĐỘNG:
    /// - Tự động detect số columns từ IDataReader (không hardcode)
    /// - Map bất kỳ entity type nào có Propertyy fields
    /// - Hoạt động với bất kỳ SQL query nào (SELECT *, SELECT specific columns, etc.)
    /// - Tự động skip columns không có trong entity
    /// </summary>
    public static class PropertyMapper
    {
        // Cache property DB name -> FieldInfo mapping cho mỗi type - chỉ scan 1 lần
        private static readonly ConcurrentDictionary<Type, Dictionary<string, FieldInfo>> _propertyFieldCache = new();
        
        // Cache property instances cho mỗi type (để tránh GetValue() mỗi row)
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, Propertyy?>>> _propertyGetterCache = new();

        /// <summary>
        /// OPTIMIZED: Map properties với caching - CHỈ map các columns có trong reader
        /// 
        /// Tính linh động:
        /// - Tự động detect số columns từ reader.FieldCount (dynamic)
        /// - Đọc tên columns từ reader.GetName(i) (dynamic)
        /// - Chỉ map columns có trong cả reader VÀ entity (auto-matching)
        /// - Tự động skip columns không match
        /// </summary>
        public static void MapPropertiesFromReader<T>(T entity, IDataReader reader) where T : class
        {
            if (entity == null || reader == null) return;

            Type entityType = typeof(T);
            
            // Get cached property field mappings (chỉ scan 1 lần per type)
            var propertyFields = GetCachedPropertyFields<T>();

            // CRITICAL OPTIMIZATION: Chỉ map các columns có trong reader, không loop tất cả properties
            // Dynamic: Đọc số columns từ reader (không hardcode)
            var fieldCount = reader.FieldCount;

            // Build mapping: ordinal -> FieldInfo cho các columns có trong reader
            // Dynamic: Kích thước dictionary = số columns thực tế trong reader
            var ordinalToFieldMap = new Dictionary<int, FieldInfo>(fieldCount);
            
            // Dynamic loop: Iterate qua TẤT CẢ columns có trong reader (không hardcode số lượng)
            for (int i = 0; i < fieldCount; i++)
            {
                try
                {
                    // Dynamic: Đọc tên column từ reader (không hardcode tên)
                    string columnName = reader.GetName(i);
                    
                    // Auto-matching: Chỉ map nếu column tồn tại trong entity
                    if (propertyFields.TryGetValue(columnName, out FieldInfo? fieldInfo) && fieldInfo != null)
                    {
                        ordinalToFieldMap[i] = fieldInfo;
                    }
                    // Note: Columns không có trong entity sẽ tự động bị skip (flexible)
                }
                catch (Exception)
                {
                    // Skip invalid columns - ensures flexibility with any reader structure
                    continue;
                }
            }

            // Now map only the columns that exist in reader
            foreach (var kvp in ordinalToFieldMap)
            {
                int ordinal = kvp.Key;
                FieldInfo fieldInfo = kvp.Value;

                try
                {
                    object columnValue = reader[ordinal];
                    Propertyy? property = fieldInfo.GetValue(entity) as Propertyy;
                    if (property != null)
                    {
                        property.Value = columnValue == DBNull.Value ? null : columnValue;
                    }
                }
                catch (Exception)
                {
                    // Skip on error
                    continue;
                }
            }
        }

        /// <summary>
        /// OPTIMIZED: Cache property FieldInfo per type - chỉ scan 1 lần, không gọi GetPropertiesValues
        /// </summary>
        private static Dictionary<string, FieldInfo> GetCachedPropertyFields<T>()
        {
            Type entityType = typeof(T);

            return _propertyFieldCache.GetOrAdd(entityType, type =>
            {
                var result = new Dictionary<string, FieldInfo>();

                // Use reflection để tìm Propertyy fields trực tiếp - nhanh hơn GetPropertiesValues
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(Propertyy))
                    {
                        // Create temp instance để get DBName
                        var tempInstance = Activator.CreateInstance(type);
                        if (tempInstance != null)
                        {
                            Propertyy? fieldValue = field.GetValue(tempInstance) as Propertyy;
                            if (fieldValue != null && !string.IsNullOrEmpty(fieldValue.DBName))
                            {
                                result[fieldValue.DBName] = field;
                            }
                        }
                    }
                }

                return result;
            });
        }

        /// <summary>
        /// OPTIMIZED: Map multiple entities - Build ordinal mapping ONCE, reuse for all rows
        /// </summary>
        public static List<T> MapEntitiesFromReader<T>(IDataReader reader, Func<T> createEntity) where T : class
        {
            var results = new List<T>();
            var rowCount = 0;
            var totalMapTime = 0L;
            var totalCreateTime = 0L;

            // CRITICAL: Build ordinal -> FieldInfo mapping ONCE for this reader
            var buildMappingSw = System.Diagnostics.Stopwatch.StartNew();
            var ordinalToFieldMap = BuildOrdinalToFieldMapping<T>(reader);
            buildMappingSw.Stop();
            System.Console.WriteLine($"[TIMING] Build ordinal mapping: {buildMappingSw.ElapsedMilliseconds}ms ({ordinalToFieldMap.Count} columns)");

            var readSw = System.Diagnostics.Stopwatch.StartNew();
            while (reader.Read())
            {
                rowCount++;
                var createSw = System.Diagnostics.Stopwatch.StartNew();
                T entity = createEntity();
                createSw.Stop();
                totalCreateTime += createSw.ElapsedMilliseconds;

                var mapSw = System.Diagnostics.Stopwatch.StartNew();
                // OPTIMIZED: Map using pre-built ordinal mapping
                MapPropertiesFromReaderFast(entity, reader, ordinalToFieldMap);
                mapSw.Stop();
                totalMapTime += mapSw.ElapsedMilliseconds;

                results.Add(entity);

                // Log mỗi 50 rows để không spam
                if (rowCount % 50 == 0)
                {
                    System.Console.WriteLine($"[TIMING] Mapped {rowCount} rows - Create avg: {totalCreateTime / rowCount:F2}ms, Map avg: {totalMapTime / rowCount:F2}ms");
                }
            }
            readSw.Stop();

            if (rowCount > 0)
            {
                System.Console.WriteLine($"[TIMING] PropertyMapper - Total rows: {rowCount}, Read time: {readSw.ElapsedMilliseconds}ms");
                System.Console.WriteLine($"[TIMING] PropertyMapper - Create avg: {totalCreateTime / (double)rowCount:F2}ms total: {totalCreateTime}ms");
                System.Console.WriteLine($"[TIMING] PropertyMapper - Map avg: {totalMapTime / (double)rowCount:F2}ms total: {totalMapTime}ms");
            }

            return results;
        }

        /// <summary>
        /// OPTIMIZED: Build ordinal -> Propertyy instance mapping ONCE per reader
        /// Returns list of (ordinal, Propertyy instance) để set value trực tiếp
        /// </summary>
        private static List<(int ordinal, Propertyy property)> BuildOrdinalToPropertyMapping<T>(T sampleEntity, IDataReader reader)
        {
            var propertyFields = GetCachedPropertyFields<T>();
            var mapping = new List<(int, Propertyy)>(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    string columnName = reader.GetName(i);
                    
                    if (propertyFields.TryGetValue(columnName, out FieldInfo? fieldInfo) && fieldInfo != null)
                    {
                        // Get Propertyy instance từ sample entity (chỉ 1 lần)
                        Propertyy? property = fieldInfo.GetValue(sampleEntity) as Propertyy;
                        if (property != null)
                        {
                            mapping.Add((i, property));
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip invalid columns
                    continue;
                }
            }

            return mapping;
        }

        /// <summary>
        /// OPTIMIZED: Build ordinal -> FieldInfo mapping ONCE per reader
        /// 
        /// Tính linh động:
        /// - Dynamic: Đọc số columns từ reader.FieldCount (không hardcode)
        /// - Dynamic: Đọc tên columns từ reader.GetName(i) (không hardcode tên)
        /// - Hoạt động với bất kỳ số lượng columns nào (1, 3, 10, 50+, SELECT *, etc.)
        /// </summary>
        private static Dictionary<int, FieldInfo> BuildOrdinalToFieldMapping<T>(IDataReader reader)
        {
            var propertyFields = GetCachedPropertyFields<T>();
            
            // Dynamic capacity: Dựa trên số columns thực tế trong reader
            var ordinalToFieldMap = new Dictionary<int, FieldInfo>(reader.FieldCount);

            // Dynamic loop: Iterate qua TẤT CẢ columns có trong reader
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    // Dynamic: Đọc tên column từ reader (không hardcode)
                    string columnName = reader.GetName(i);
                    
                    // Auto-matching: Chỉ map nếu column tồn tại trong entity
                    if (propertyFields.TryGetValue(columnName, out FieldInfo? fieldInfo) && fieldInfo != null)
                    {
                        ordinalToFieldMap[i] = fieldInfo;
                    }
                    // Note: Columns không match sẽ tự động skip (flexible)
                }
                catch (Exception)
                {
                    // Skip invalid columns - ensures flexibility với bất kỳ reader structure nào
                    continue;
                }
            }

            return ordinalToFieldMap;
        }

        /// <summary>
        /// OPTIMIZED: Fast mapping using pre-built ordinal mapping - cache property instances
        /// </summary>
        private static void MapPropertiesFromReaderFast<T>(T entity, IDataReader reader, Dictionary<int, FieldInfo> ordinalToFieldMap)
        {
            if (entity == null || reader == null) return;

            // Pre-fetch property instances để tránh GetValue() nhiều lần
            // Chỉ gọi GetValue() 1 lần per property, cache trong array
            var propertyInstances = new Propertyy?[ordinalToFieldMap.Count];
            int index = 0;
            foreach (var kvp in ordinalToFieldMap)
            {
                propertyInstances[index] = kvp.Value.GetValue(entity) as Propertyy;
                index++;
            }

            // Now set values - dynamic iterations based on reader columns, không có GetValue() overhead
            index = 0;
            foreach (var kvp in ordinalToFieldMap)
            {
                int ordinal = kvp.Key;
                Propertyy? property = propertyInstances[index];
                index++;

                if (property != null)
                {
                    try
                    {
                        property.Value = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
                    }
                    catch (Exception)
                    {
                        // Skip on error - ensures flexibility with any reader structure
                        continue;
                    }
                }
            }
        }
    }
}

