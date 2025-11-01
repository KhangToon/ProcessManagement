# 🚀 ĐỀ XUẤT CẢI TIẾN SQLServerServices (GIỮ NGUYÊN PROPERTYY PATTERN)

## 📊 PHÂN TÍCH VẤN ĐỀ HIỆN TẠI

### ❌ Vấn đề nghiêm trọng đã phát hiện:

#### 1. **N+1 Query Problem** (Cực kỳ nghiêm trọng)
```csharp
// Ví dụ: GetDanhSachSanPham() - Line 7828
while (reader.Read()) {
    sanPham.DSThongTin = GetDanhSachThongTinSanPham(spid); // 1 query per row!
    sanPham.DanhSachNVLs = GetDSachNVLwithSanPham_bySPID(spid); // 1 query per row!
    foreach (var ncid in ...) {
        sanPham.DSNguyenCongs.Add(GetNguyenCong(ncid)); // N queries per row!
    }
}
// Nếu có 100 SanPham → 100 + 100 + (100*N) queries!

// Ví dụ: GetKHSXbyID() - Line 299
khsx.TargetSanPham = GetSanpham(...); // 1 query
khsx.LoaiNVL = GetLoaiNVLbyID(...); // 1 query
khsx.DSachNVLofKHSXs = GetListNVLofKHSXbyID(...); // 1 query
khsx.DSachCongDoans = GetlistCongdoans(...); // 1 query
foreach (var lotkhsx in khsx.DSLOT_KHSXs) {
    lotkhsx.TargetNVL = GetMaNguyenVatLieuByID(...); // N queries trong loop!
}
```

#### 2. **Synchronous Blocking**
- Hầu hết methods là `public KHSX GetKHSXbyID()` - block thread
- Không có async versions
- `Task.Run()` được dùng nhưng không đúng cách (line 1927-1937)

#### 3. **SQL Injection Risk**
```csharp
// ❌ NGUY HIỂM: String interpolation
command.CommandText = $"SELECT * FROM [...] WHERE [{Common.KHSXID}] = '{khsxID}'";

// ✅ AN TOÀN: Parameterized (đã có ở một số nơi)
command.Parameters.AddWithValue("@khsxID", khsxID);
```

#### 4. **No Caching**
- Mọi query đều hit database
- Không cache kết quả thường xuyên truy cập

#### 5. **Connection Management**
- Mỗi method tạo connection mới (OK vì có connection pooling)
- Nhưng nested calls tạo nhiều connections không cần thiết
- Không có transaction sharing

#### 6. **Singleton Service**
```csharp
builder.Services.AddSingleton<SQLServerServices>();
```
- Singleton với connection string có thể OK
- Nhưng không thread-safe nếu có shared state

---

## ✅ GIẢI PHÁP ĐỀ XUẤT

### **Strategy: Refactor từng bước, giữ nguyên interface**

---

## 🎯 PHASE 1: FIX IMMEDIATE ISSUES (Ưu tiên cao)

### 1.1. **Add Async Methods** (Không breaking changes)

```csharp
// Thêm async versions bên cạnh sync methods
public async Task<KHSX> GetKHSXbyIDAsync(object? khsxID)
{
    if (khsxID == null) return new KHSX();

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.KHSXID}] = @khsxID";
    command.Parameters.AddWithValue("@khsxID", khsxID);

    using var reader = await command.ExecuteReaderAsync();
    
    KHSX khsx = new();
    if (await reader.ReadAsync())
    {
        // Map properties...
        MapPropertiesFromReader(khsx, reader);
    }

    // Load related data in parallel
    await LoadKHSXRelatedDataAsync(khsx);

    return khsx;
}

// Helper để load related data song song
private async Task LoadKHSXRelatedDataAsync(KHSX khsx)
{
    var tasks = new List<Task>
    {
        Task.Run(async () => khsx.TargetSanPham = await GetSanphamAsync(...)),
        Task.Run(async () => khsx.LoaiNVL = await GetLoaiNVLbyIDAsync(...)),
        Task.Run(async () => khsx.DSachNVLofKHSXs = await GetListNVLofKHSXbyIDAsync(...)),
        Task.Run(async () => khsx.DSachCongDoans = await GetlistCongdoansAsync(...))
    };

    await Task.WhenAll(tasks);
}
```

### 1.2. **Fix N+1 Queries với Batch Loading**

```csharp
// ❌ TRƯỚC: N+1 queries
public List<SanPham> GetDanhSachSanPham()
{
    // ... load sanPham list
    foreach (var sanPham in danhSachSanPham)
    {
        sanPham.DSThongTin = GetDanhSachThongTinSanPham(spid); // N queries!
    }
}

// ✅ SAU: Batch load
public async Task<List<SanPham>> GetDanhSachSanPhamAsync()
{
    var danhSachSanPham = new List<SanPham>();
    
    // 1. Load all SanPham
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    
    var command = connection.CreateCommand();
    command.CommandText = $"SELECT * FROM [{Common.Table_SanPham}]";
    
    using var reader = await command.ExecuteReaderAsync();
    var spIds = new List<object>();
    
    while (await reader.ReadAsync())
    {
        var sanPham = new SanPham();
        MapPropertiesFromReader(sanPham, reader);
        danhSachSanPham.Add(sanPham);
        spIds.Add(sanPham.SP_SPID.Value);
    }

    // 2. Batch load all related data in ONE query
    var allThongTin = await BatchLoadThongTinSanPhamAsync(spIds);
    var allNVLs = await BatchLoadNVLwithSanPhamAsync(spIds);
    var allNguyenCongs = await BatchLoadNguyenCongsAsync(spIds);

    // 3. Map related data
    foreach (var sanPham in danhSachSanPham)
    {
        sanPham.DSThongTin = allThongTin
            .Where(t => t.SPID.Value?.Equals(sanPham.SP_SPID.Value) == true)
            .ToList();
        // ... similar for other related data
    }

    return danhSachSanPham;
}

// Batch load helper
private async Task<List<ThongTinSanPham>> BatchLoadThongTinSanPhamAsync(List<object> spIds)
{
    if (!spIds.Any()) return new List<ThongTinSanPham>();

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    // Use IN clause instead of N queries
    var spIdsStr = string.Join(",", spIds);
    var command = connection.CreateCommand();
    command.CommandText = $@"
        SELECT * FROM [{Common.Table_ThongTinSanPham}] 
        WHERE [{Common.SP_SPID}] IN ({spIdsStr})";

    using var reader = await command.ExecuteReaderAsync();
    var results = new List<ThongTinSanPham>();

    while (await reader.ReadAsync())
    {
        var tt = new ThongTinSanPham();
        MapPropertiesFromReader(tt, reader);
        results.Add(tt);
    }

    return results;
}
```

### 1.3. **Fix SQL Injection - Parameterized Queries**

```csharp
// ❌ TRƯỚC: SQL Injection risk
command.CommandText = $"SELECT * FROM [...] WHERE [{Common.KHSXID}] = '{khsxID}'";

// ✅ SAU: Parameterized
public KHSX GetKHSXbyID(object? khsxID)
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.KHSXID}] = @khsxID";
    command.Parameters.AddWithValue("@khsxID", khsxID ?? DBNull.Value); // Handle null safely

    // ... rest of code
}
```

### 1.4. **Add Caching Layer**

```csharp
public class SQLServerServices
{
    private readonly string? connectionString;
    private readonly IMemoryCache _cache; // Inject from DI
    private static readonly MemoryCacheEntryOptions DefaultCacheOptions = 
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

    public SQLServerServices(IMemoryCache? cache = null)
    {
        // ... existing constructor code
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
    }

    public KHSX GetKHSXbyID(object? khsxID)
    {
        if (khsxID == null) return new KHSX();

        // Check cache first
        var cacheKey = $"KHSX_{khsxID}";
        if (_cache.TryGetValue(cacheKey, out KHSX? cached))
        {
            return cached ?? new KHSX();
        }

        // Load from DB
        var khsx = GetKHSXbyID_Internal(khsxID);

        // Cache result
        if (khsx?.KHSXID.Value != null)
        {
            _cache.Set(cacheKey, khsx, DefaultCacheOptions);
        }

        return khsx;
    }

    // Invalidate cache when update
    public (int, string) UpdateKHSXProperty(object? khsxid, string propertyName, object? value)
    {
        var result = UpdateKHSXProperty_Internal(khsxid, propertyName, value);
        
        // Invalidate cache
        if (khsxid != null)
        {
            _cache.Remove($"KHSX_{khsxid}");
        }

        return result;
    }
}
```

---

## 🎯 PHASE 2: REFACTORING STRUCTURE

### 2.1. **Extract Helper Methods** (DRY Principle)

```csharp
// Helper method để map properties từ reader
private void MapPropertiesFromReader<T>(T entity, SqlDataReader reader) where T : new()
{
    if (entity == null) return;

    List<Propertyy> properties = GetPropertiesValues(entity);

    foreach (var property in properties)
    {
        string? columnName = property.DBName;
        if (string.IsNullOrEmpty(columnName)) continue;

        try
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (ordinal >= 0)
            {
                object columnValue = reader[ordinal];
                property.Value = columnValue == DBNull.Value ? null : columnValue;
            }
        }
        catch
        {
            // Column doesn't exist, skip
        }
    }
}

// Helper để get properties (generic)
private List<Propertyy> GetPropertiesValues<T>(T entity)
{
    if (entity == null) return new List<Propertyy>();

    // Use reflection or method if entity has GetPropertiesValues()
    if (entity is Models.KHSX khsx)
        return khsx.GetPropertiesValues();
    if (entity is Models.KHO_NVL.NguyenVatLieu nvl)
        return nvl.GetPropertiesValues();
    // ... other types

    return new List<Propertyy>();
}

// Helper để tạo parameterized query
private SqlCommand CreateParameterizedCommand(
    SqlConnection connection, 
    string tableName, 
    Dictionary<string, object?> parameters)
{
    var command = connection.CreateCommand();
    var conditions = new List<string>();

    foreach (var param in parameters)
    {
        conditions.Add($"[{param.Key}] = @{param.Key}");
        command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
    }

    command.CommandText = $"SELECT * FROM [{tableName}]";
    if (conditions.Any())
    {
        command.CommandText += " WHERE " + string.Join(" AND ", conditions);
    }

    return command;
}
```

### 2.2. **Extract Common Query Patterns**

```csharp
// Base method cho GetById pattern
private async Task<T> GetByIdAsync<T>(
    string tableName,
    string idColumnName,
    object? id,
    Func<T> createEntity) where T : class
{
    if (id == null) return createEntity();

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $"SELECT * FROM [{tableName}] WHERE [{idColumnName}] = @id";
    command.Parameters.AddWithValue("@id", id);

    using var reader = await command.ExecuteReaderAsync();
    
    T entity = createEntity();
    if (await reader.ReadAsync())
    {
        MapPropertiesFromReader(entity, reader);
    }

    return entity;
}

// Usage:
public async Task<KHSX> GetKHSXbyIDAsync(object? khsxID)
{
    return await GetByIdAsync(
        Common.TableKHSX,
        Common.KHSXID,
        khsxID,
        () => new KHSX());
}
```

### 2.3. **Connection Reuse trong Transaction**

```csharp
// Helper để execute multiple operations trong 1 transaction
private async Task<T> ExecuteInTransactionAsync<T>(
    Func<SqlConnection, SqlTransaction, Task<T>> operation)
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    using var transaction = connection.BeginTransaction();
    try
    {
        var result = await operation(connection, transaction);
        await transaction.CommitAsync();
        return result;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

// Usage:
public async Task<(int, string)> InsertKHSXWithDetailsAsync(KHSX khsx, List<NguyenCongofKHSX> congDoans)
{
    return await ExecuteInTransactionAsync(async (connection, transaction) =>
    {
        // Insert KHSX
        int khsxId = await InsertKHSXAsync(connection, transaction, khsx);

        // Insert all cong doans in same transaction
        foreach (var cd in congDoans)
        {
            await InsertCongdoanAsync(connection, transaction, cd);
        }

        return (khsxId, string.Empty);
    });
}
```

---

## 🎯 PHASE 3: PERFORMANCE OPTIMIZATIONS

### 3.1. **Add Connection String Optimization**

```csharp
// Program.cs - Optimize connection string
var connectionString = builder.Configuration.GetConnectionString("DBConnectionString");

// Add connection pooling options
var builder = new SqlConnectionStringBuilder(connectionString)
{
    MaxPoolSize = 100, // Default is 100
    MinPoolSize = 0,
    ConnectionTimeout = 30, // seconds
    CommandTimeout = 30, // seconds
    Pooling = true, // Enable connection pooling
    MultipleActiveResultSets = true // For MARS
};

builder.Services.AddSingleton<SQLServerServices>(sp => 
    new SQLServerServices(builder.ConnectionString, sp.GetService<IMemoryCache>()));
```

### 3.2. **Add Query Result Caching**

```csharp
// Cache frequently accessed data
private const string CACHE_KEY_KHSX_LIST = "KHSX_List";
private const string CACHE_KEY_SANPHAM_LIST = "SanPham_List";
private static readonly TimeSpan LIST_CACHE_DURATION = TimeSpan.FromMinutes(2);

public async Task<List<KHSX>> GetListKHSXsAsync()
{
    // Check cache
    if (_cache.TryGetValue(CACHE_KEY_KHSX_LIST, out List<KHSX>? cached))
    {
        return cached ?? new List<KHSX>();
    }

    // Load from DB
    var list = await GetListKHSXs_InternalAsync();

    // Cache
    _cache.Set(CACHE_KEY_KHSX_LIST, list, LIST_CACHE_DURATION);

    return list;
}

// Invalidate khi có update
public async Task<(int, string)> InsertNewKHSXAsync(KHSX newKHSX)
{
    var result = await InsertNewKHSX_InternalAsync(newKHSX);
    
    // Invalidate list cache
    _cache.Remove(CACHE_KEY_KHSX_LIST);
    
    return result;
}
```

### 3.3. **Add Prepared Statements**

```csharp
// Cache prepared commands for frequently used queries
private readonly Dictionary<string, string> _preparedQueries = new();

private string GetPreparedQuery(string key, Func<string> factory)
{
    if (!_preparedQueries.ContainsKey(key))
    {
        _preparedQueries[key] = factory();
    }
    return _preparedQueries[key];
}

// Usage:
public async Task<KHSX> GetKHSXbyIDAsync(object? khsxID)
{
    var query = GetPreparedQuery("KHSX_GetById", () => 
        $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.KHSXID}] = @khsxID");

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = query;
    command.Parameters.AddWithValue("@khsxID", khsxID);

    // ... execute
}
```

---

## 📦 CẤU TRÚC ĐỀ XUẤT

```
ProcessManagement/Services/SQLServer/
├── SQLServerServices.cs          # Main service (refactored)
├── Helpers/
│   ├── PropertyMapper.cs         # Map Propertyy từ reader
│   ├── QueryBuilder.cs           # Build parameterized queries
│   └── BatchLoader.cs           # Batch load related data
├── Caching/
│   └── QueryCacheManager.cs     # Cache management
└── Extensions/
    └── SQLServerServicesExtensions.cs
```

---

## 🔄 MIGRATION PLAN

### Week 1: Fix Critical Issues
1. ✅ Add async methods cho hot paths (GetKHSXbyID, GetNguyenVatLieuByID...)
2. ✅ Fix SQL injection với parameterized queries
3. ✅ Add basic caching cho frequently accessed data

### Week 2: Refactor Structure
4. ✅ Extract helper methods (DRY)
5. ✅ Add batch loading methods
6. ✅ Extract common query patterns

### Week 3: Performance Tuning
7. ✅ Optimize connection string
8. ✅ Add prepared statements
9. ✅ Add transaction helpers
10. ✅ Performance testing

### Week 4: Testing & Cleanup
11. ✅ Test all methods
12. ✅ Update Pages/Components to use async methods
13. ✅ Monitor performance improvements

---

## 📈 KẾT QUẢ MONG ĐỢI

### Performance Improvements:
- ⚡ **70-90% reduction** trong database queries (fix N+1)
- ⚡ **50-70% faster** response times với async
- ⚡ **80% reduction** trong DB calls với caching
- ⚡ **Better scalability** với async/await

### Code Quality:
- 📝 **Smaller methods**: Extract helpers
- 📝 **Reusable code**: Common patterns
- 📝 **Better error handling**: Consistent approach
- 📝 **Type safety**: Less runtime errors

---

## 💡 IMPLEMENTATION ORDER (Ưu tiên)

### **High Priority (Do ngay)**:
1. Fix SQL Injection - parameterized queries
2. Fix N+1 queries trong GetDanhSachSanPham, GetKHSXbyID
3. Add async methods cho hot paths

### **Medium Priority**:
4. Add caching layer
5. Extract helper methods
6. Add batch loading

### **Low Priority**:
7. Prepared statements
8. Transaction helpers
9. Connection string optimization

