# 📘 HƯỚNG DẪN TRIỂN KHAI CẢI TIẾN SQLServerServices

## 🎯 TÓM TẮT

Cải tiến `SQLServerServices` mà **KHÔNG thay đổi Propertyy pattern**, tập trung vào:
1. ✅ Fix N+1 queries với batch loading
2. ✅ Add async methods
3. ✅ Fix SQL injection với parameterized queries
4. ✅ Add caching layer
5. ✅ Extract helper methods (DRY)

---

## 📁 CẤU TRÚC FILE MỚI

```
ProcessManagement/Services/SQLServer/
├── SQLServerServices.cs                    # Main service (existing - sẽ refactor)
├── Helpers/                                # NEW
│   ├── PropertyMapper.cs                  # Map Propertyy từ reader
│   ├── QueryBuilder.cs                    # Build parameterized queries
│   └── BatchLoader.cs                     # Batch load related data
└── SQLServerServices_IMPROVED_EXAMPLE.cs   # Examples (reference)
```

---

## 🚀 BƯỚC 1: Thêm Helper Classes

### 1.1. Tạo thư mục Helpers
```bash
mkdir ProcessManagement\Services\SQLServer\Helpers
```

### 1.2. Copy các file đã tạo:
- ✅ `PropertyMapper.cs` - Helper để map Propertyy
- ✅ `QueryBuilder.cs` - Helper để build queries an toàn
- ✅ `BatchLoader.cs` - Helper để batch load data

---

## 🚀 BƯỚC 2: Update SQLServerServices.cs

### 2.1. Thêm using statements

Thêm vào đầu file:
```csharp
using ProcessManagement.Services.SQLServer.Helpers;
using Microsoft.Extensions.Caching.Memory;
```

### 2.2. Update Constructor

```csharp
public class SQLServerServices
{
    private readonly string? connectionString;
    private readonly IMemoryCache? _cache;

    // ✅ IMPROVED: Constructor với optional cache
    public SQLServerServices(IMemoryCache? cache = null)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        connectionString = configuration["ConnectionStrings:DBConnectionString"];
        _cache = cache; // Optional - sẽ inject từ DI
    }

    // Giữ constructor cũ để backward compatibility
    public SQLServerServices() : this(null) { }
}
```

### 2.3. Add MemoryCache vào DI

```csharp
// Program.cs
builder.Services.AddMemoryCache(); // Add này
builder.Services.AddSingleton<SQLServerServices>(sp => 
    new SQLServerServices(sp.GetService<IMemoryCache>()));
```

---

## 🚀 BƯỚC 3: Refactor Methods (Ưu tiên)

### 3.1. **FIX NGAY**: GetKHSXbyID - Fix N+1 + Add Async

**Thêm async version** (không thay thế sync method):

```csharp
// Thêm method mới (giữ method cũ)
public async Task<KHSX> GetKHSXbyIDAsync(object? khsxID)
{
    if (khsxID == null) return new KHSX();

    // Check cache
    var cacheKey = $"KHSX_{khsxID}";
    if (_cache?.TryGetValue(cacheKey, out KHSX? cached) == true && cached != null)
    {
        return cached;
    }

    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    // ✅ Parameterized query
    var command = QueryBuilder.BuildSelectQuery(
        connection,
        Common.TableKHSX,
        new Dictionary<string, object?> { { Common.KHSXID, khsxID } });

    using var reader = await command.ExecuteReaderAsync();

    KHSX khsx = new();
    if (await reader.ReadAsync())
    {
        PropertyMapper.MapPropertiesFromReader(khsx, reader);
    }

    // ✅ Batch load related data
    await LoadKHSXRelatedDataBatchAsync(khsx);

    // Cache
    if (khsx.KHSXID.Value != null)
    {
        _cache?.Set(cacheKey, khsx, TimeSpan.FromMinutes(5));
    }

    return khsx;
}

// Helper để batch load
private async Task LoadKHSXRelatedDataBatchAsync(KHSX khsx)
{
    // Implement như trong SQLServerServices_IMPROVED_EXAMPLE.cs
    // ...
}
```

### 3.2. **FIX NGAY**: GetDanhSachSanPham - Fix N+1

```csharp
// Thêm async version
public async Task<List<SanPham>> GetDanhSachSanPhamAsync()
{
    // Check cache
    const string cacheKey = "SanPham_List";
    if (_cache?.TryGetValue(cacheKey, out List<SanPham>? cached) == true && cached != null)
    {
        return cached;
    }

    var danhSachSanPham = new List<SanPham>();
    
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $"SELECT * FROM [{Common.Table_SanPham}]";

    using var reader = await command.ExecuteReaderAsync();
    var spIds = new List<object>();

    // Load all SanPham
    while (await reader.ReadAsync())
    {
        var sanPham = new SanPham();
        PropertyMapper.MapPropertiesFromReader(sanPham, reader);
        danhSachSanPham.Add(sanPham);
        
        if (sanPham.SP_SPID.Value != null)
            spIds.Add(sanPham.SP_SPID.Value);
    }

    if (spIds.Any())
    {
        // ✅ Batch load related data
        var allThongTin = await BatchLoader.BatchLoadThongTinSanPhamAsync(connectionString, spIds);
        var allNVLs = await BatchLoader.BatchLoadNVLwithSanPhamAsync(connectionString, spIds);

        // Map related data
        foreach (var sanPham in danhSachSanPham)
        {
            var spid = sanPham.SP_SPID.Value;
            sanPham.DSThongTin = allThongTin
                .Where(t => t.SPID.Value?.Equals(spid) == true)
                .ToList();
            sanPham.DanhSachNVLs = allNVLs
                .Where(nvl => nvl.SPID.Value?.Equals(spid) == true)
                .ToList();
        }
    }

    // Cache
    _cache?.Set(cacheKey, danhSachSanPham, TimeSpan.FromMinutes(2));

    return danhSachSanPham;
}
```

### 3.3. **FIX NGAY**: Fix SQL Injection trong tất cả methods

Thay đổi từ:
```csharp
// ❌ TRƯỚC
command.CommandText = $"SELECT * FROM [...] WHERE [{Common.KHSXID}] = '{khsxID}'";
```

Sang:
```csharp
// ✅ SAU
var command = QueryBuilder.BuildSelectQuery(
    connection,
    Common.TableKHSX,
    new Dictionary<string, object?> { { Common.KHSXID, khsxID } });
```

---

## 🚀 BƯỚC 4: Update Pages/Components

### 4.1. Update Pages để dùng async methods

```csharp
// ❌ TRƯỚC
@code {
    private async Task LoadData()
    {
        DSKHSXs = SQLServerServices.GetListKHSXsByAnyParmeters(...).kHSXes;
    }
}

// ✅ SAU
@code {
    private async Task LoadData()
    {
        var result = await SQLServerServices.GetListKHSXsByAnyParmetersAsync(...);
        DSKHSXs = result.kHSXes;
    }
}
```

---

## 📊 KẾT QUẢ MONG ĐỢI

### Performance:
- ⚡ **70-90% giảm** số queries (fix N+1)
- ⚡ **50-70% nhanh hơn** với async
- ⚡ **80% giảm** DB calls với caching

### Code Quality:
- ✅ An toàn hơn (parameterized queries)
- ✅ Dễ maintain hơn (helper methods)
- ✅ Reusable code (DRY)

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **Giữ backward compatibility**: Thêm async methods, không xóa sync methods ngay
2. **Test từng method**: Refactor từng method một, test kỹ trước khi tiếp tục
3. **Monitor performance**: Đo performance trước/sau để verify improvements
4. **Gradual migration**: Migrate Pages/Components dần dần sang async

---

## 📝 CHECKLIST

### Phase 1 - Critical Fixes:
- [ ] Add MemoryCache vào DI
- [ ] Tạo Helper classes (PropertyMapper, QueryBuilder, BatchLoader)
- [ ] Fix SQL injection trong GetKHSXbyID
- [ ] Add GetKHSXbyIDAsync với batch loading
- [ ] Fix GetDanhSachSanPham N+1 queries
- [ ] Test và verify

### Phase 2 - More Methods:
- [ ] Add async cho GetNguyenVatLieuByID
- [ ] Add async cho GetPhieuNhapKho, GetPhieuXuatKho
- [ ] Add caching cho frequently accessed data
- [ ] Extract common patterns

### Phase 3 - Update UI:
- [ ] Update Pages để dùng async methods
- [ ] Monitor performance improvements
- [ ] Remove old sync methods nếu không dùng

