using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ProcessManagement.Commons;
using ProcessManagement.Models;
using ProcessManagement.Models.KHSXs;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.KHO_NVL.XuatKho;
using ProcessManagement.Models.SANPHAM;
using ProcessManagement.Services.SQLServer.Helpers;
using System.Data;

namespace ProcessManagement.Services.SQLServer
{
    /// <summary>
    /// SQLServerServicesV2 - Optimized version với batch loading, async, và caching
    /// Tập trung vào các methods được PageDSachKHSXs sử dụng
    /// </summary>
    public class SQLServerServicesV2
    {
        private readonly string? connectionString;
        private readonly IMemoryCache? _cache;
        private readonly SQLServerServices _originalService; // Fallback cho các methods chưa được migrate

        public SQLServerServicesV2(IMemoryCache cache, SQLServerServices originalService, IConfiguration configuration)
        {
            System.Console.WriteLine($"[SQLServerServicesV2] Constructor called - Cache: {cache != null}, OriginalService: {originalService != null}, Configuration: {configuration != null}");
            
            // Try both keys for compatibility
            if (configuration != null)
            {
                connectionString = configuration.GetConnectionString("DBConnectionString") 
                                ?? configuration["ConnectionStrings:DBConnectionString"]; 
            }
            else
            {
                connectionString = null;
                System.Console.WriteLine($"[SQLServerServicesV2] ERROR - IConfiguration is NULL!");
            }
            
            _cache = cache;
            _originalService = originalService ?? throw new ArgumentNullException(nameof(originalService));
            
            System.Console.WriteLine($"[SQLServerServicesV2] Initialized - ConnectionString: {(!string.IsNullOrEmpty(connectionString) ? $"OK (length: {connectionString.Length})" : "NULL")}");
            if (string.IsNullOrEmpty(connectionString))
            {
                System.Console.WriteLine($"[SQLServerServicesV2] WARNING - ConnectionString is NULL! Check appsettings.json");
            }
        }

        #region Batch Loading Helpers

        /// <summary>
        /// Batch load KetQuaGC cho nhiều (NCID, KHSXID) pairs - TRÁNH N+1 QUERIES
        /// Thay vì gọi GetListKetQuaGC nhiều lần, chỉ cần 1 query
        /// </summary>
        public async Task<Dictionary<(object? ncid, object? khsxid), List<KetQuaGC>>> BatchLoadKetQuaGCAsync(
            List<(object? ncid, object? khsxid)> pairs)
        {
            var result = new Dictionary<(object?, object?), List<KetQuaGC>>();

            if (!pairs.Any()) return result;

            // Initialize dictionary
            foreach (var pair in pairs)
            {
                result[pair] = new List<KetQuaGC>();
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Tạo IN clause cho NCID và KHSXID
            var ncIds = pairs.Select(p => p.ncid).Where(id => id != null).Distinct().ToList();
            var khsxIds = pairs.Select(p => p.khsxid).Where(id => id != null).Distinct().ToList();

            if (!ncIds.Any() || !khsxIds.Any()) return result;

            var command = connection.CreateCommand();

            // Build IN clause cho NCID
            var ncParams = new List<string>();
            for (int i = 0; i < ncIds.Count; i++)
            {
                string paramName = $"@ncid{i}";
                ncParams.Add(paramName);
                command.Parameters.AddWithValue(paramName, ncIds[i] ?? DBNull.Value);
            }

            // Build IN clause cho KHSXID
            var khsxParams = new List<string>();
            for (int i = 0; i < khsxIds.Count; i++)
            {
                string paramName = $"@khsxid{i}";
                khsxParams.Add(paramName);
                command.Parameters.AddWithValue(paramName, khsxIds[i] ?? DBNull.Value);
            }

            // OPTIMIZED: Chỉ select columns cần thiết (dynamic list) thay vì SELECT *
            // Dynamic: Columns được định nghĩa từ constants, không hardcode số lượng
            var requiredColumns = new List<string>
            {
                KetQuaGC.KQGCDBName.NCID,
                KetQuaGC.KQGCDBName.KHSXID,
                KetQuaGC.KQGCDBName.SLOK,
                KetQuaGC.KQGCDBName.SLNG
            };
            
            var selectClause = string.Join(", ", requiredColumns.Select(col => $"[{col}]"));
            command.CommandText = $@"
                SELECT {selectClause}
                FROM [{KetQuaGC.KQGCDBName.Table_KetQuaGC}] 
                WHERE [{KetQuaGC.KQGCDBName.NCID}] IN ({string.Join(", ", ncParams)})
                  AND [{KetQuaGC.KQGCDBName.KHSXID}] IN ({string.Join(", ", khsxParams)})";

            using var reader = await command.ExecuteReaderAsync();
            var allKQGCs = PropertyMapper.MapEntitiesFromReader(reader, () => new KetQuaGC());

            // Group by (NCID, KHSXID)
            foreach (var kqgc in allKQGCs)
            {
                var key = (kqgc.NCID.Value, kqgc.KHSXID.Value);
                if (result.ContainsKey(key))
                {
                    result[key].Add(kqgc);
                }
            }

            return result;
        }

        /// <summary>
        /// Batch load PhieuXuatKho cho nhiều KHSXIDs
        /// </summary>
        public async Task<Dictionary<object, List<PhieuXuatKho>>> BatchLoadPhieuXuatKhosAsync(
            List<object?> khsxIds,
            bool isPhieuBoSung = false)
        {
            var result = new Dictionary<object, List<PhieuXuatKho>>();

            if (!khsxIds.Any()) return result;

            // Initialize dictionary
            foreach (var id in khsxIds.Where(id => id != null))
            {
                result[id!] = new List<PhieuXuatKho>();
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.Table_PhieuXuatKho,
                Common.KHSXID,
                khsxIds.Where(id => id != null).Cast<object>().ToList());

            // Thêm điều kiện IsPhieuBoSungNVL nếu cần
            if (isPhieuBoSung)
            {
                command.CommandText += $" AND [{Common.IsPhieuBoSungNVL}] = @isPhieuBoSung";
                command.Parameters.AddWithValue("@isPhieuBoSung", 1);
            }

            using var reader = await command.ExecuteReaderAsync();
            var allPXKs = PropertyMapper.MapEntitiesFromReader(reader, () => new PhieuXuatKho()).ToList();

            // FIX: Load DSNVLofPXKs cho mỗi PXK (giống V1 GetListPhieuXuatKhos line 5245)
            // Collect all PXKIDs để batch load DSNVLofPXKs
            var allPxkIds = allPXKs
                .Where(pxk => pxk.PXKID.Value != null)
                .Select(pxk => pxk.PXKID.Value)
                .Distinct()
                .ToList();

            // Batch load DSNVLofPXKs cho tất cả PXK
            Dictionary<object, List<NVLofPhieuXuatKho>> nvlOfPxksData = new();
            if (allPxkIds.Any())
            {
                using var nvlConnection = new SqlConnection(connectionString);
                await nvlConnection.OpenAsync();
                
                var nvlCommand = QueryBuilder.BuildInQuery(
                    nvlConnection,
                    Common.Table_NVLofPhieuXuatKho,
                    Common.PXKID,
                    allPxkIds.Cast<object>().ToList());

                using var nvlReader = await nvlCommand.ExecuteReaderAsync();
                var allNVLofPXKs = PropertyMapper.MapEntitiesFromReader(nvlReader, () => new NVLofPhieuXuatKho());

                foreach (var nvl in allNVLofPXKs)
                {
                    if (nvl.PXKID.Value != null)
                    {
                        if (!nvlOfPxksData.ContainsKey(nvl.PXKID.Value))
                        {
                            nvlOfPxksData[nvl.PXKID.Value] = new List<NVLofPhieuXuatKho>();
                        }
                        nvlOfPxksData[nvl.PXKID.Value].Add(nvl);
                    }
                }
            }

            // Assign DSNVLofPXKs và group by KHSXID
            foreach (var pxk in allPXKs)
            {
                var khsxId = pxk.KHSXID.Value;
                if (khsxId != null && result.ContainsKey(khsxId))
                {
                    // Assign DSNVLofPXKs
                    if (pxk.PXKID.Value != null && nvlOfPxksData.ContainsKey(pxk.PXKID.Value))
                    {
                        pxk.DSNVLofPXKs = nvlOfPxksData[pxk.PXKID.Value];
                    }
                    else
                    {
                        pxk.DSNVLofPXKs = new List<NVLofPhieuXuatKho>();
                    }
                    
                    result[khsxId].Add(pxk);
                }
            }

            return result;
        }

        #endregion

        #region Optimized KHSX Methods

        /// <summary>
        /// Cải tiến GetKHSXbyIDRuduceTime - Async với caching và batch loading
        /// </summary>
        public async Task<KHSX> GetKHSXbyIDRuduceTimeAsync(object? khsxID)
        {
            if (khsxID == null) return new KHSX();

            // Check cache
            string cacheKey = $"KHSX_{khsxID}_ReduceTime";
            if (_cache != null && _cache.TryGetValue(cacheKey, out KHSX? cachedKHSX) && cachedKHSX != null)
            {
                return cachedKHSX;
            }

            KHSX khsx = new();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.KHSXID}] = @khsxid";
                command.Parameters.AddWithValue("@khsxid", khsxID);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    PropertyMapper.MapPropertiesFromReader(khsx, reader);
                }
            }

            if (khsx.KHSXID.Value == null) return khsx;

            // Batch load related data (có thể optimize thêm)
            var spid = int.TryParse(khsx.SPID.Value?.ToString(), out int spidParsed) ? spidParsed : 0;
            if (spid > 0)
            {
                khsx.TargetSanPham = await Task.Run(() => _originalService.GetSanpham(spid, false));
            }

            khsx.DSachNVLofKHSXs = await Task.Run(() => _originalService.GetListNVLofKHSXbyID(khsx.KHSXID.Value));
            khsx.DSachCongDoans = await Task.Run(() => _originalService.GetlistCongdoans(khsx.KHSXID.Value, false));

            var fistCDoanID = khsx.DSachCongDoans.FirstOrDefault()?.NCID.Value ?? 0;

            var resultdslots = await Task.Run(() => 
                _originalService.GetListLOT_khsx(new() { 
                    { KHSX_LOT.DBName.KHSXID, khsx.KHSXID.Value }, 
                    { KHSX_LOT.DBName.NCID, fistCDoanID } 
                }).Item1);

            if (resultdslots != null && resultdslots.Any())
            {
                khsx.DSLOT_KHSXs = resultdslots;

                // Batch load TargetNVL cho tất cả lots
                var nvlIds = resultdslots.Select(lot => lot.NVLID.Value).Where(id => id != null).Distinct().ToList();
                foreach (var lotkhsx in khsx.DSLOT_KHSXs)
                {
                    lotkhsx.TargetNVL = await Task.Run(() => _originalService.GetMaNguyenVatLieuByID(lotkhsx.NVLID.Value));
                }
            }

            // Get trang thai xuat kho NVL
            var colIsDonePXK = await Task.Run(() => 
                _originalService.GetPXK_AnyColValuebyAnyParameters(
                    new Dictionary<string, object?>() { { Common.KHSXID, khsx.KHSXID.Value } }, 
                    Common.IsDonePXK).columnValues.FirstOrDefault());
            
            if (int.TryParse(colIsDonePXK?.ToString(), out int ispxkdone))
            {
                khsx.isDonePXK = ispxkdone == 1;
            }

            // Get PNKID
            var colpnkid = await Task.Run(() => 
                _originalService.GetPXK_AnyColValuebyAnyParameters(
                    new Dictionary<string, object?>() { { Common.PXKID, khsx.PXKID.Value } }, 
                    Common.PNKID).columnValues.FirstOrDefault());
            
            if (int.TryParse(colpnkid?.ToString(), out int pnkid))
            {
                var colIsReturnedNVL = await Task.Run(() => 
                    _originalService.GetPNK_AnyColValuebyAnyParameters(
                        new Dictionary<string, object?>() { { Common.PNKID, pnkid } }, 
                        Common.IsDonePNK).columnValues.FirstOrDefault());
                
                if (int.TryParse(colIsReturnedNVL?.ToString(), out int isPNKdone))
                {
                    khsx.isReturnedNVL = isPNKdone == 1;
                }
            }

            // Get collapsed KHSX
            if (int.TryParse(khsx.IsCollapsed.Value?.ToString(), out int isCollapsed))
            {
                khsx.isCollapsed = isCollapsed == 1;
            }

            // Get ischartrunning KHSX
            if (int.TryParse(khsx.IsChartRunning.Value?.ToString(), out int isChartRunning))
            {
                khsx.isChartRunning = isChartRunning == 1;
            }

            // Cache result (5 minutes)
            if (_cache != null)
            {
                _cache.Set(cacheKey, khsx, TimeSpan.FromMinutes(5));
            }

            return khsx;
        }

        /// <summary>
        /// Cải tiến GetKHSXbyMaKHSXRuduceTime - Async version
        /// </summary>
        public async Task<KHSX> GetKHSXbyMaKHSXRuduceTimeAsync(object? maKHSX)
        {
            if (maKHSX == null) return new KHSX();

            // Check cache
            string cacheKey = $"KHSX_Ma_{maKHSX}_ReduceTime";
            if (_cache != null && _cache.TryGetValue(cacheKey, out KHSX? cachedKHSX) && cachedKHSX != null)
            {
                return cachedKHSX;
            }

            KHSX khsx = new();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.MaLSX}] = @maKHSX";
                command.Parameters.AddWithValue("@maKHSX", maKHSX);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    PropertyMapper.MapPropertiesFromReader(khsx, reader);
                }
            }

            if (khsx.KHSXID.Value == null) return khsx;

            // Load related data (tương tự GetKHSXbyIDRuduceTimeAsync)
            var spid = int.TryParse(khsx.SPID.Value?.ToString(), out int spidParsed) ? spidParsed : 0;
            if (spid > 0)
            {
                khsx.TargetSanPham = await Task.Run(() => _originalService.GetSanpham(spid, false));
            }

            khsx.DSachNVLofKHSXs = await Task.Run(() => _originalService.GetListNVLofKHSXbyID(khsx.KHSXID.Value));
            khsx.DSachCongDoans = await Task.Run(() => _originalService.GetlistCongdoans(khsx.KHSXID.Value, false));

            var fistCDoanID = khsx.DSachCongDoans.FirstOrDefault()?.NCID.Value ?? 0;

            var resultdslots = await Task.Run(() => 
                _originalService.GetListLOT_khsx(new() { 
                    { KHSX_LOT.DBName.KHSXID, khsx.KHSXID.Value }, 
                    { KHSX_LOT.DBName.NCID, fistCDoanID } 
                }).Item1);

            if (resultdslots != null && resultdslots.Any())
            {
                khsx.DSLOT_KHSXs = resultdslots;

                foreach (var lotkhsx in khsx.DSLOT_KHSXs)
                {
                    lotkhsx.TargetNVL = await Task.Run(() => _originalService.GetMaNguyenVatLieuByID(lotkhsx.NVLID.Value));
                }
            }

            // FIX: Load PXK/PNK status, flags (giống V1 GetKHSXbyMaKHSXRuduceTime line 526-555)
            // Get trang thai xuat kho NVL
            var colIsDonePXK = await Task.Run(() => 
                _originalService.GetPXK_AnyColValuebyAnyParameters(
                    new Dictionary<string, object?>() { { Common.KHSXID, khsx.KHSXID.Value } }, 
                    Common.IsDonePXK).columnValues.FirstOrDefault());
            
            if (int.TryParse(colIsDonePXK?.ToString(), out int ispxkdone))
            {
                khsx.isDonePXK = ispxkdone == 1;
            }

            // Get PNKID
            var colpnkid = await Task.Run(() => 
                _originalService.GetPXK_AnyColValuebyAnyParameters(
                    new Dictionary<string, object?>() { { Common.PXKID, khsx.PXKID.Value } }, 
                    Common.PNKID).columnValues.FirstOrDefault());
            
            if (int.TryParse(colpnkid?.ToString(), out int pnkid))
            {
                var colIsReturnedNVL = await Task.Run(() => 
                    _originalService.GetPNK_AnyColValuebyAnyParameters(
                        new Dictionary<string, object?>() { { Common.PNKID, pnkid } }, 
                        Common.IsDonePNK).columnValues.FirstOrDefault());
                
                if (int.TryParse(colIsReturnedNVL?.ToString(), out int isPNKdone))
                {
                    khsx.isReturnedNVL = isPNKdone == 1;
                }
            }

            // Get collapsed KHSX
            if (int.TryParse(khsx.IsCollapsed.Value?.ToString(), out int isCollapsed))
            {
                khsx.isCollapsed = isCollapsed == 1;
            }

            // Get ischartrunning KHSX
            if (int.TryParse(khsx.IsChartRunning.Value?.ToString(), out int isChartRunning))
            {
                khsx.isChartRunning = isChartRunning == 1;
            }

            // Cache result
            if (_cache != null)
            {
                _cache.Set(cacheKey, khsx, TimeSpan.FromMinutes(5));
            }

            return khsx;
        }

        /// <summary>
        /// Async version của GetListKHSXsByAnyParmeters_AsignColumn
        /// OPTIMIZED: Chỉ load các columns cần thiết, không load related data
        /// </summary>
        public async Task<(List<KHSX> kHSXes, string errorMessage)> GetListKHSXsByAnyParmeters_AsignColumnAsync(
            Dictionary<string, object?> parameters, 
            List<string> requiredColumns, 
            bool isgetAll = false)
        {
            // Check cache nếu chỉ cần basic columns và không có filter
            string cacheKey = $"KHSX_List_{string.Join("_", requiredColumns.OrderBy(c => c))}_{isgetAll}";
            if (_cache != null && isgetAll && !parameters.Any())
            {
                if (_cache.TryGetValue(cacheKey, out List<KHSX>? cachedList) && cachedList != null)
                {
                    return (cachedList, string.Empty);
                }
            }

            List<KHSX> khsxs = new();
            string errorMessage = string.Empty;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // OPTIMIZED: Chỉ select các columns cần thiết thay vì SELECT *
                var columnList = requiredColumns != null && requiredColumns.Any() 
                    ? requiredColumns.Select(col => $"[{col}]").ToList()
                    : new List<string> { "*" };

                var selectClause = string.Join(", ", columnList);
                
                var command = connection.CreateCommand();
                var query = new System.Text.StringBuilder($"SELECT {selectClause} FROM [{Common.TableKHSX}]");

                // Build WHERE clause nếu có parameters và không phải getAll
                if (!isgetAll && parameters != null && parameters.Any())
                {
                    var conditions = new List<string>();
                    foreach (var param in parameters)
                    {
                        string paramName = $"@param_{System.Text.RegularExpressions.Regex.Replace(param.Key, @"[^\w]", "")}";
                        conditions.Add($"[{param.Key}] = {paramName}");
                        command.Parameters.AddWithValue(paramName, param.Value ?? DBNull.Value);
                    }

                    if (conditions.Any())
                    {
                        query.Append(" WHERE ").Append(string.Join(" AND ", conditions));
                    }
                }

                command.CommandText = query.ToString();

                System.Console.WriteLine($"[SQL] Query: {command.CommandText}");
                System.Console.WriteLine($"[SQL] Parameters: {string.Join(", ", command.Parameters.Cast<Microsoft.Data.SqlClient.SqlParameter>().Select(p => $"{p.ParameterName}={p.Value}"))}");

                var querySw = System.Diagnostics.Stopwatch.StartNew();
                using var reader = await command.ExecuteReaderAsync();
                querySw.Stop();
                System.Console.WriteLine($"[TIMING] SQL Query execution: {querySw.ElapsedMilliseconds}ms");

                var mapSw = System.Diagnostics.Stopwatch.StartNew();
                khsxs = PropertyMapper.MapEntitiesFromReader(reader, () => new KHSX());
                mapSw.Stop();
                System.Console.WriteLine($"[TIMING] Map entities from reader: {mapSw.ElapsedMilliseconds}ms");

                // Cache result nếu getAll và không có filter
                if (_cache != null && isgetAll && (parameters == null || !parameters.Any()))
                {
                    _cache.Set(cacheKey, khsxs, TimeSpan.FromMinutes(2)); // Cache 2 phút cho list
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
                khsxs.Clear();
                System.Console.WriteLine($"[ERROR] GetListKHSXsByAnyParmeters_AsignColumnAsync: {ex.Message}");
            }
            finally
            {
                sw.Stop();
                System.Console.WriteLine($"[TIMING] GetListKHSXsByAnyParmeters_AsignColumnAsync TOTAL: {sw.ElapsedMilliseconds}ms ({khsxs.Count} items)");
            }

            return (khsxs, errorMessage);
        }

        #endregion

        #region Batch Load Multiple KHSX with Related Data

        /// <summary>
        /// OPTIMIZED: Batch load nhiều KHSX cùng lúc với tất cả related data
        /// Thay vì load từng KHSX → N queries, chỉ cần vài queries batch
        /// </summary>
        public async Task<Dictionary<object, KHSX>> BatchLoadKHSXsWithRelatedDataAsync(
            List<object> khsxIds)
        {
            var result = new Dictionary<object, KHSX>();

            if (!khsxIds.Any()) return result;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Step 1: Batch load base KHSX data
            var step1Sw = System.Diagnostics.Stopwatch.StartNew();
            var baseKHSXs = await BatchLoadKHSXsBaseAsync(khsxIds);
            step1Sw.Stop();
            System.Console.WriteLine($"[BATCH] Step 1 - Load {baseKHSXs.Count} KHSX base: {step1Sw.ElapsedMilliseconds}ms");

            // Initialize result dictionary
            foreach (var kvp in baseKHSXs)
            {
                result[kvp.Key] = kvp.Value;
            }

            // Step 2: Collect all IDs for batch loading
            var allSPIds = baseKHSXs.Values
                .Select(k => int.TryParse(k.SPID.Value?.ToString(), out int spid) ? spid : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var step2Sw = System.Diagnostics.Stopwatch.StartNew();
            // Step 3: Batch load SanPham
            var sanPhams = await BatchLoadSanPhamsAsync(allSPIds);
            foreach (var kvp in baseKHSXs)
            {
                var khsx = kvp.Value;
                var spid = int.TryParse(khsx.SPID.Value?.ToString(), out int spidParsed) ? spidParsed : 0;
                if (spid > 0 && sanPhams.ContainsKey(spid))
                {
                    khsx.TargetSanPham = sanPhams[spid];
                }
            }
            step2Sw.Stop();
            System.Console.WriteLine($"[BATCH] Step 2 - Load {sanPhams.Count} SanPham: {step2Sw.ElapsedMilliseconds}ms");

            // Step 4: Batch load NVL, CongDoan, LOT cho tất cả KHSX - OPTIMIZED: Parallel execution
            var step3Sw = System.Diagnostics.Stopwatch.StartNew();
            
            // OPTIMIZED: Parallel load NVL và CongDoan (độc lập nhau) để tăng tốc
            var loadNvlTask = BatchLoadNVLofKHSXAsync(khsxIds);
            var loadCongDoanTask = BatchLoadCongDoanAsync(khsxIds);
            
            // Wait for both to complete
            await Task.WhenAll(loadNvlTask, loadCongDoanTask);
            var nvlData = await loadNvlTask;
            var congDoanData = await loadCongDoanTask;
            
            // Load LOT sau khi có CongDoan (vì LOT cần NCID từ CongDoan)
            var lotData = await BatchLoadLOTAsync(congDoanData);
            
            // Assign related data
            foreach (var khsxId in khsxIds)
            {
                if (!result.ContainsKey(khsxId)) continue;

                var khsx = result[khsxId];

                // Assign NVL
                if (nvlData.ContainsKey(khsxId))
                {
                    khsx.DSachNVLofKHSXs = nvlData[khsxId];
                }

                // Assign CongDoan
                if (congDoanData.ContainsKey(khsxId))
                {
                    khsx.DSachCongDoans = congDoanData[khsxId];
                }

                // Assign LOT
                var firstCDoanID = khsx.DSachCongDoans.FirstOrDefault()?.NCID.Value ?? 0;
                if (lotData.ContainsKey((khsxId, firstCDoanID)))
                {
                    khsx.DSLOT_KHSXs = lotData[(khsxId, firstCDoanID)];
                }
            }
            step3Sw.Stop();
            System.Console.WriteLine($"[BATCH] Step 3 - Load related data (parallel): {step3Sw.ElapsedMilliseconds}ms");

            // Step 5: Batch load TargetNVL cho tất cả LOT
            var step4Sw = System.Diagnostics.Stopwatch.StartNew();
            var allLotNvlIds = result.Values
                .SelectMany(k => k.DSLOT_KHSXs)
                .Select(lot => lot.NVLID.Value)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var targetNVLs = await BatchLoadNguyenVatLieuAsync(allLotNvlIds);
            
            foreach (var khsx in result.Values)
            {
                foreach (var lot in khsx.DSLOT_KHSXs)
                {
                    if (lot.NVLID.Value != null && targetNVLs.ContainsKey(lot.NVLID.Value))
                    {
                        lot.TargetNVL = targetNVLs[lot.NVLID.Value];
                    }
                }
            }
            step4Sw.Stop();
            System.Console.WriteLine($"[BATCH] Step 4 - Load {targetNVLs.Count} TargetNVL: {step4Sw.ElapsedMilliseconds}ms");

            // Step 6: Batch load PXK/PNK status
            var step5Sw = System.Diagnostics.Stopwatch.StartNew();
            var pxkStatuses = await BatchLoadPXKStatusAsync(khsxIds);
            var pnkStatuses = await BatchLoadPNKStatusAsync(
                result.Values
                    .Select(k => k.PXKID.Value)
                    .Where(id => id != null)
                    .ToList());

            foreach (var khsxId in khsxIds)
            {
                if (!result.ContainsKey(khsxId)) continue;
                var khsx = result[khsxId];

                // PXK status
                if (pxkStatuses.ContainsKey(khsxId))
                {
                    khsx.isDonePXK = pxkStatuses[khsxId];
                }

                // PNK status
                var pxkId = khsx.PXKID.Value;
                if (pxkId != null && pnkStatuses.ContainsKey(pxkId))
                {
                    khsx.isReturnedNVL = pnkStatuses[pxkId];
                }

                // FIX: Set isCollapsed và isChartRunning (giống V1 line 372-381)
                if (int.TryParse(khsx.IsCollapsed.Value?.ToString(), out int isCollapsed))
                {
                    khsx.isCollapsed = isCollapsed == 1;
                }

                if (int.TryParse(khsx.IsChartRunning.Value?.ToString(), out int isChartRunning))
                {
                    khsx.isChartRunning = isChartRunning == 1;
                }

                // FIX: Load LoaiNVL (giống V1 line 331)
                var loaiNvlId = int.TryParse(khsx.LOAINVLID.Value?.ToString(), out int loainvlid) ? loainvlid : 0;
                if (loaiNvlId > 0)
                {
                    khsx.LoaiNVL = await Task.Run(() => _originalService.GetLoaiNVLbyID(loaiNvlId));
                }
            }
            step5Sw.Stop();
            System.Console.WriteLine($"[BATCH] Step 5 - Load PXK/PNK status + flags: {step5Sw.ElapsedMilliseconds}ms");

            sw.Stop();
            System.Console.WriteLine($"[BATCH] BatchLoadKHSXsWithRelatedDataAsync TOTAL: {sw.ElapsedMilliseconds}ms ({khsxIds.Count} KHSX)");

            return result;
        }

        /// <summary>
        /// Batch load base KHSX data (chỉ properties, không có related data)
        /// </summary>
        private async Task<Dictionary<object, KHSX>> BatchLoadKHSXsBaseAsync(List<object> khsxIds)
        {
            var result = new Dictionary<object, KHSX>();

            if (!khsxIds.Any()) return result;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.TableKHSX,
                Common.KHSXID,
                khsxIds.Cast<object>().ToList());

            using var reader = await command.ExecuteReaderAsync();
            var khsxs = PropertyMapper.MapEntitiesFromReader(reader, () => new KHSX());

            foreach (var khsx in khsxs)
            {
                if (khsx.KHSXID.Value != null)
                {
                    result[khsx.KHSXID.Value] = khsx;
                }
            }

            return result;
        }

        /// <summary>
        /// OPTIMIZED: Batch load SanPham cho nhiều SPID - Direct SQL thay vì N queries
        /// </summary>
        private async Task<Dictionary<int, SanPham>> BatchLoadSanPhamsAsync(List<int> spIds)
        {
            var result = new Dictionary<int, SanPham>();

            if (!spIds.Any()) return result;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // OPTIMIZED: Batch load tất cả SanPham trong 1 query
            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.Table_SanPham,
                Common.SP_SPID,
                spIds.Cast<object>().ToList());

            using var reader = await command.ExecuteReaderAsync();
            var allSanPhams = PropertyMapper.MapEntitiesFromReader(reader, () => new SanPham());

            foreach (var sp in allSanPhams)
            {
                if (sp.SP_SPID.Value != null && int.TryParse(sp.SP_SPID.Value.ToString(), out int spid))
                {
                    result[spid] = sp;
                }
            }

            return result;
        }

        /// <summary>
        /// OPTIMIZED: Batch load NVL của KHSX - Direct SQL
        /// </summary>
        private async Task<Dictionary<object, List<NVLofKHSX>>> BatchLoadNVLofKHSXAsync(List<object> khsxIds)
        {
            var nvlData = new Dictionary<object, List<NVLofKHSX>>();

            if (!khsxIds.Any()) return nvlData;

            // Initialize
            foreach (var id in khsxIds)
            {
                nvlData[id] = new List<NVLofKHSX>();
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // OPTIMIZED: Batch load NVL - 1 query cho tất cả KHSX
            var nvlCommand = QueryBuilder.BuildInQuery(
                connection,
                Common.TableNVLofKHSX,
                Common.KHSXID,
                khsxIds.Cast<object>().ToList());

            using (var nvlReader = await nvlCommand.ExecuteReaderAsync())
            {
                var allNVLs = PropertyMapper.MapEntitiesFromReader(nvlReader, () => new NVLofKHSX());
                
                // Collect all unique NVLIDs để batch load TenNVL
                var uniqueNvlIds = allNVLs
                    .Select(nvl => nvl.NVLID.Value)
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                // Batch load NVL entities để lấy MaNVL (TenNVL)
                var nvlEntities = await BatchLoadNguyenVatLieuAsync(uniqueNvlIds);
                var nvlIdToMaNVL = nvlEntities
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.MaNVL.Value?.ToString() ?? string.Empty);

                foreach (var nvl in allNVLs)
                {
                    if (nvl.KHSXID.Value != null)
                    {
                        // Map TenNVL từ batch loaded data
                        if (nvl.NVLID.Value != null && nvlIdToMaNVL.ContainsKey(nvl.NVLID.Value))
                        {
                            nvl.TenNVL = nvlIdToMaNVL[nvl.NVLID.Value];
                        }

                        nvlData[nvl.KHSXID.Value].Add(nvl);
                    }
                }
            }

            return nvlData;
        }

        /// <summary>
        /// OPTIMIZED: Batch load CongDoan của KHSX - Direct SQL
        /// Bao gồm batch load TenCongDoan từ bảng NguyenCong
        /// </summary>
        private async Task<Dictionary<object, List<NguyenCongofKHSX>>> BatchLoadCongDoanAsync(List<object> khsxIds)
        {
            var congDoanData = new Dictionary<object, List<NguyenCongofKHSX>>();

            if (!khsxIds.Any()) return congDoanData;

            // Initialize
            foreach (var id in khsxIds)
            {
                congDoanData[id] = new List<NguyenCongofKHSX>();
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // OPTIMIZED: Batch load CongDoan - 1 query cho tất cả KHSX
            var congDoanCommand = QueryBuilder.BuildInQuery(
                connection,
                Common.TableCongDoan,
                Common.KHSXID,
                khsxIds.Cast<object>().ToList());

            // FIX: Load tất cả data vào memory TRƯỚC KHI đóng reader để tránh lỗi "open DataReader"
            List<NguyenCongofKHSX> allCongDoans;
            using (var cdReader = await congDoanCommand.ExecuteReaderAsync())
            {
                allCongDoans = PropertyMapper.MapEntitiesFromReader(cdReader, () => new NguyenCongofKHSX()).ToList();
            } // Reader đã đóng ở đây
            
            // Collect all unique NCIDs để batch load TenCongDoan
            var uniqueNcIds = allCongDoans
                .Select(cd => cd.NCID.Value)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            // Batch load NguyenCong entities để lấy TenNguyenCong
            // FIX: Dùng connection đã có (reader đã đóng rồi) hoặc có thể dùng lại connection
            Dictionary<object, string> ncIdToTenCongDoan = new();
            if (uniqueNcIds.Any())
            {
                var nguyenCongCommand = QueryBuilder.BuildInQuery(
                    connection,
                    NguyenCong.DBName.Table_NguyenCong,
                    NguyenCong.DBName.NCID,
                    uniqueNcIds.Cast<object>().ToList());

                using (var ncReader = await nguyenCongCommand.ExecuteReaderAsync())
                {
                    var nguyenCongs = PropertyMapper.MapEntitiesFromReader(ncReader, () => new NguyenCong()).ToList();
                    
                    // Build mapping: NCID -> TenNguyenCong
                    foreach (var nc in nguyenCongs)
                    {
                        if (nc.NCID.Value != null)
                        {
                            ncIdToTenCongDoan[nc.NCID.Value] = nc.TenNguyenCong.Value?.ToString() ?? string.Empty;
                        }
                    }
                }
            }
            
            // Map TenCongDoan cho mỗi NguyenCongofKHSX
            foreach (var cd in allCongDoans)
            {
                if (cd.KHSXID.Value != null)
                {
                    // FIX: Reset TenCongDoan trước (giống V1 logic)
                    cd.TenCongDoan.Value = null;
                    
                    // Map TenCongDoan từ batch loaded data
                    if (cd.NCID.Value != null && ncIdToTenCongDoan.ContainsKey(cd.NCID.Value))
                    {
                        cd.TenCongDoan.Value = ncIdToTenCongDoan[cd.NCID.Value];
                    }

                    congDoanData[cd.KHSXID.Value].Add(cd);
                }
            }

            // FIX: SORT by STTNguyencong - QUAN TRỌNG! (giống V1 logic line 830-836)
            foreach (var kvp in congDoanData)
            {
                var list = kvp.Value;
                if (list.All(cd => string.IsNullOrEmpty(cd.STTNguyencong.Value?.ToString()?.Trim()) == false))
                {
                    // Sort listCongdoans by STTNguyencong
                    var sortedList = list
                        .OrderBy(congdoan => congdoan.STTNguyencong?.Value ?? default(int))
                        .ToList();
                    congDoanData[kvp.Key] = sortedList;
                }
            }

            // FIX: Set IsUsing = true cho tất cả (giống V1 logic line 840-845)
            // Note: DSachNVLCongDoans sẽ được load sau nếu cần (có thể batch load)
            foreach (var kvp in congDoanData)
            {
                foreach (var cd in kvp.Value)
                {
                    cd.IsUsing = true;
                    // TODO: Batch load DSachNVLCongDoans nếu cần
                    // cd.DSachNVLCongDoans = ... (có thể thêm batch method)
                }
            }

            return congDoanData;
        }

        /// <summary>
        /// OPTIMIZED: Batch load LOT của KHSX - Direct SQL
        /// </summary>
        private async Task<Dictionary<(object khsxId, object ncid), List<KHSX_LOT>>> BatchLoadLOTAsync(
            Dictionary<object, List<NguyenCongofKHSX>> congDoanData)
        {
            var lotData = new Dictionary<(object, object), List<KHSX_LOT>>();

            if (!congDoanData.Any()) return lotData;

            // Collect all (KHSXID, NCID) pairs from CongDoan
            var lotPairs = new List<(object khsxId, object ncid)>();
            foreach (var kvp in congDoanData)
            {
                var firstCDoan = kvp.Value.FirstOrDefault();
                if (firstCDoan?.NCID.Value != null)
                {
                    lotPairs.Add((kvp.Key, firstCDoan.NCID.Value));
                }
            }

            if (!lotPairs.Any()) return lotData;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Batch load LOT cho tất cả pairs
            var khsxIdsForLot = lotPairs.Select(p => p.khsxId).Distinct().ToList();
            var ncIdsForLot = lotPairs.Select(p => p.ncid).Distinct().ToList();

            var lotCommand = connection.CreateCommand();
            var khsxParams = new List<string>();
            for (int i = 0; i < khsxIdsForLot.Count; i++)
            {
                string paramName = $"@khsxid_lot{i}";
                khsxParams.Add(paramName);
                lotCommand.Parameters.AddWithValue(paramName, khsxIdsForLot[i] ?? DBNull.Value);
            }

            var ncParams = new List<string>();
            for (int i = 0; i < ncIdsForLot.Count; i++)
            {
                string paramName = $"@ncid_lot{i}";
                ncParams.Add(paramName);
                lotCommand.Parameters.AddWithValue(paramName, ncIdsForLot[i] ?? DBNull.Value);
            }

            // Dynamic: Columns không hardcode
            lotCommand.CommandText = $@"
                SELECT * FROM [{KHSX_LOT.DBName.Table_KHSXLOT}] 
                WHERE [{KHSX_LOT.DBName.KHSXID}] IN ({string.Join(", ", khsxParams)})
                  AND [{KHSX_LOT.DBName.NCID}] IN ({string.Join(", ", ncParams)})";

            using (var lotReader = await lotCommand.ExecuteReaderAsync())
            {
                var allLots = PropertyMapper.MapEntitiesFromReader(lotReader, () => new KHSX_LOT());

                foreach (var lot in allLots)
                {
                    if (lot.KHSXID.Value != null && lot.NCID.Value != null)
                    {
                        var key = (lot.KHSXID.Value, lot.NCID.Value);
                        if (!lotData.ContainsKey(key))
                        {
                            lotData[key] = new List<KHSX_LOT>();
                        }
                        lotData[key].Add(lot);
                    }
                }
            }

            return lotData;
        }

        /// <summary>
        /// OPTIMIZED: Batch load NVL, CongDoan, LOT cho nhiều KHSX - Direct SQL thay vì gọi original service
        /// LEGACY: Giữ lại để backward compatibility, nhưng sẽ dùng các methods riêng
        /// </summary>
        private async Task<(
            Dictionary<object, List<NVLofKHSX>> nvlData,
            Dictionary<object, List<NguyenCongofKHSX>> congDoanData,
            Dictionary<(object khsxId, object ncid), List<KHSX_LOT>> lotData)> 
            BatchLoadKHSXRelatedDataAsync(List<object> khsxIds)
        {
            var nvlData = new Dictionary<object, List<NVLofKHSX>>();
            var congDoanData = new Dictionary<object, List<NguyenCongofKHSX>>();
            var lotData = new Dictionary<(object, object), List<KHSX_LOT>>();

            if (!khsxIds.Any()) return (nvlData, congDoanData, lotData);

            // Initialize
            foreach (var id in khsxIds)
            {
                nvlData[id] = new List<NVLofKHSX>();
                congDoanData[id] = new List<NguyenCongofKHSX>();
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // OPTIMIZED: Batch load NVL - 1 query cho tất cả KHSX
            var nvlSw = System.Diagnostics.Stopwatch.StartNew();
            var nvlCommand = QueryBuilder.BuildInQuery(
                connection,
                Common.TableNVLofKHSX,
                Common.KHSXID,
                khsxIds.Cast<object>().ToList());

            using (var nvlReader = await nvlCommand.ExecuteReaderAsync())
            {
                var allNVLs = PropertyMapper.MapEntitiesFromReader(nvlReader, () => new NVLofKHSX());
                
                // Collect all unique NVLIDs để batch load TenNVL
                var uniqueNvlIds = allNVLs
                    .Select(nvl => nvl.NVLID.Value)
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                // Batch load NVL entities để lấy MaNVL (TenNVL)
                var nvlEntities = await BatchLoadNguyenVatLieuAsync(uniqueNvlIds);
                var nvlIdToMaNVL = nvlEntities
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.MaNVL.Value?.ToString() ?? string.Empty);

                foreach (var nvl in allNVLs)
                {
                    if (nvl.KHSXID.Value != null)
                    {
                        // Map TenNVL từ batch loaded data
                        if (nvl.NVLID.Value != null && nvlIdToMaNVL.ContainsKey(nvl.NVLID.Value))
                        {
                            nvl.TenNVL = nvlIdToMaNVL[nvl.NVLID.Value];
                        }

                        nvlData[nvl.KHSXID.Value].Add(nvl);
                    }
                }
            }
            nvlSw.Stop();
            System.Console.WriteLine($"[BATCH] Load NVL: {nvlSw.ElapsedMilliseconds}ms");

            // OPTIMIZED: Batch load CongDoan - 1 query cho tất cả KHSX
            var congDoanSw = System.Diagnostics.Stopwatch.StartNew();
            var congDoanCommand = QueryBuilder.BuildInQuery(
                connection,
                Common.TableCongDoan,
                Common.KHSXID,
                khsxIds.Cast<object>().ToList());

            using (var cdReader = await congDoanCommand.ExecuteReaderAsync())
            {
                var allCongDoans = PropertyMapper.MapEntitiesFromReader(cdReader, () => new NguyenCongofKHSX());
                
                foreach (var cd in allCongDoans)
                {
                    if (cd.KHSXID.Value != null)
                    {
                        congDoanData[cd.KHSXID.Value].Add(cd);
                    }
                }
            }
            congDoanSw.Stop();
            System.Console.WriteLine($"[BATCH] Load CongDoan: {congDoanSw.ElapsedMilliseconds}ms");

            // OPTIMIZED: Batch load LOT - Collect all (KHSXID, NCID) pairs first, then batch load
            var lotSw = System.Diagnostics.Stopwatch.StartNew();
            var lotPairs = new List<(object khsxId, object ncid)>();
            foreach (var kvp in congDoanData)
            {
                var firstCDoan = kvp.Value.FirstOrDefault();
                if (firstCDoan?.NCID.Value != null)
                {
                    lotPairs.Add((kvp.Key, firstCDoan.NCID.Value));
                }
            }

            if (lotPairs.Any())
            {
                // Batch load LOT cho tất cả pairs
                var khsxIdsForLot = lotPairs.Select(p => p.khsxId).Distinct().ToList();
                var ncIdsForLot = lotPairs.Select(p => p.ncid).Distinct().ToList();

                var lotCommand = connection.CreateCommand();
                var khsxParams = new List<string>();
                for (int i = 0; i < khsxIdsForLot.Count; i++)
                {
                    string paramName = $"@khsxid_lot{i}";
                    khsxParams.Add(paramName);
                    lotCommand.Parameters.AddWithValue(paramName, khsxIdsForLot[i] ?? DBNull.Value);
                }

                var ncParams = new List<string>();
                for (int i = 0; i < ncIdsForLot.Count; i++)
                {
                    string paramName = $"@ncid_lot{i}";
                    ncParams.Add(paramName);
                    lotCommand.Parameters.AddWithValue(paramName, ncIdsForLot[i] ?? DBNull.Value);
                }

                lotCommand.CommandText = $@"
                    SELECT * FROM [{KHSX_LOT.DBName.Table_KHSXLOT}] 
                    WHERE [{KHSX_LOT.DBName.KHSXID}] IN ({string.Join(", ", khsxParams)})
                      AND [{KHSX_LOT.DBName.NCID}] IN ({string.Join(", ", ncParams)})";

                using (var lotReader = await lotCommand.ExecuteReaderAsync())
                {
                    var allLots = PropertyMapper.MapEntitiesFromReader(lotReader, () => new KHSX_LOT());

                    foreach (var lot in allLots)
                    {
                        if (lot.KHSXID.Value != null && lot.NCID.Value != null)
                        {
                            var key = (lot.KHSXID.Value, lot.NCID.Value);
                            if (!lotData.ContainsKey(key))
                            {
                                lotData[key] = new List<KHSX_LOT>();
                            }
                            lotData[key].Add(lot);
                        }
                    }
                }
            }
            lotSw.Stop();
            System.Console.WriteLine($"[BATCH] Load LOT: {lotSw.ElapsedMilliseconds}ms");

            return (nvlData, congDoanData, lotData);
        }

        /// <summary>
        /// OPTIMIZED: Batch load NguyenVatLieu cho nhiều NVLID - Direct SQL thay vì N queries
        /// </summary>
        private async Task<Dictionary<object, NguyenVatLieu>> BatchLoadNguyenVatLieuAsync(List<object?> nvlIds)
        {
            var result = new Dictionary<object, NguyenVatLieu>();

            if (!nvlIds.Any()) return result;

            var validNvlIds = nvlIds.Where(id => id != null).Cast<object>().ToList();
            if (!validNvlIds.Any()) return result;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.Table_NguyenVatLieu,
                Common.NVLID,
                validNvlIds);

            using var reader = await command.ExecuteReaderAsync();
            var allNVLs = PropertyMapper.MapEntitiesFromReader(reader, () => new NguyenVatLieu());

            foreach (var nvl in allNVLs)
            {
                if (nvl.NVLID.Value != null)
                {
                    result[nvl.NVLID.Value] = nvl;
                }
            }

            return result;
        }

        /// <summary>
        /// Batch load PXK status (IsDonePXK) cho nhiều KHSX
        /// </summary>
        private async Task<Dictionary<object, bool>> BatchLoadPXKStatusAsync(List<object> khsxIds)
        {
            var result = new Dictionary<object, bool>();

            if (!khsxIds.Any()) return result;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();

            // Build IN clause - Dynamic parameter list
            var paramsList = new List<string>();
            for (int i = 0; i < khsxIds.Count; i++)
            {
                string paramName = $"@khsxid{i}";
                paramsList.Add(paramName);
                command.Parameters.AddWithValue(paramName, khsxIds[i] ?? DBNull.Value);
            }

            // Dynamic: Columns được định nghĩa từ constants, không hardcode số lượng
            var requiredColumns = new List<string>
            {
                Common.KHSXID,
                Common.IsDonePXK
            };
            
            var selectClause = string.Join(", ", requiredColumns.Select(col => $"[{col}]"));
            command.CommandText = $"SELECT {selectClause} FROM [{Common.Table_PhieuXuatKho}] WHERE [{Common.KHSXID}] IN ({string.Join(", ", paramsList)})";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var khsxId = reader[Common.KHSXID];
                var isDone = reader[Common.IsDonePXK];
                if (khsxId != null && isDone != DBNull.Value)
                {
                    result[khsxId] = Convert.ToInt32(isDone) == 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Batch load PNK status (IsDonePNK) cho nhiều PXKID
        /// </summary>
        private async Task<Dictionary<object, bool>> BatchLoadPNKStatusAsync(List<object?> pxkIds)
        {
            var result = new Dictionary<object, bool>();

            if (!pxkIds.Any()) return result;

            // Tạm thời fallback về original service
            var tasks = pxkIds.Where(id => id != null).Select(pxkId => Task.Run(() =>
            {
                var pnkIdCol = _originalService.GetPXK_AnyColValuebyAnyParameters(
                    new Dictionary<string, object?>() { { Common.PXKID, pxkId } },
                    Common.PNKID).columnValues.FirstOrDefault();

                if (int.TryParse(pnkIdCol?.ToString(), out int pnkId) && pnkId > 0)
                {
                    var isDoneCol = _originalService.GetPNK_AnyColValuebyAnyParameters(
                        new Dictionary<string, object?>() { { Common.PNKID, pnkId } },
                        Common.IsDonePNK).columnValues.FirstOrDefault();

                    if (int.TryParse(isDoneCol?.ToString(), out int isDone))
                    {
                        result[pxkId!] = isDone == 1;
                    }
                }
            }));

            await Task.WhenAll(tasks);

            return result;
        }

        #endregion

        #region Wrapper Methods for PageDSachKHSXs

        /// <summary>
        /// Optimized version của GetResultsKQGCperCDoanAllLots - Batch load thay vì N queries
        /// </summary>
        public async Task<(int sumok, int sumng, int totalokng)> GetResultsKQGCperCDoanAllLotsAsync(
            object? cdid, 
            object? khsxid)
        {
            // OPTIMIZED: Use batch aggregation method thay vì load tất cả rows
            var pairs = new List<(object?, object?)> { (cdid, khsxid) };
            var batchResults = await BatchGetResultsKQGCperCDoanAllLotsAsync(pairs);
            
            return batchResults.ContainsKey((cdid, khsxid)) 
                ? batchResults[(cdid, khsxid)] 
                : (0, 0, 0);
        }

        /// <summary>
        /// OPTIMIZED: Batch version - Tính kết quả cho nhiều (NCID, KHSXID) pairs cùng lúc
        /// 
        /// Tối ưu: Dùng SQL aggregation (SUM) thay vì load tất cả rows và SUM trong C#
        /// - Giảm data transfer: 43k rows → vài trăm grouped rows
        /// - Tính toán ở DB nhanh hơn
        /// - Giảm memory và PropertyMapper overhead
        /// </summary>
        public async Task<Dictionary<(object? cdid, object? khsxid), (int sumok, int sumng, int totalokng)>> 
            BatchGetResultsKQGCperCDoanAllLotsAsync(
                List<(object? cdid, object? khsxid)> pairs)
        {
            var result = new Dictionary<(object?, object?), (int, int, int)>();

            if (!pairs.Any())
            {
                return result;
            }

            // Initialize result cho tất cả pairs
            foreach (var pair in pairs)
            {
                result[pair] = (0, 0, 0);
            }

            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    return result;
                }

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Extract unique IDs
                var ncIds = pairs.Select(p => p.cdid).Where(id => id != null).Distinct().ToList();
                var khsxIds = pairs.Select(p => p.khsxid).Where(id => id != null).Distinct().ToList();

                if (!ncIds.Any() || !khsxIds.Any())
                {
                    return result;
                }

                var command = connection.CreateCommand();

                // Build IN clause cho NCID
                var ncParams = new List<string>();
                for (int i = 0; i < ncIds.Count; i++)
                {
                    string paramName = $"@ncid{i}";
                    ncParams.Add(paramName);
                    command.Parameters.AddWithValue(paramName, ncIds[i] ?? DBNull.Value);
                }

                // Build IN clause cho KHSXID
                var khsxParams = new List<string>();
                for (int i = 0; i < khsxIds.Count; i++)
                {
                    string paramName = $"@khsxid{i}";
                    khsxParams.Add(paramName);
                    command.Parameters.AddWithValue(paramName, khsxIds[i] ?? DBNull.Value);
                }

                // OPTIMIZED: SQL aggregation - SUM ở DB thay vì load tất cả rows
                // Dynamic: Columns từ constants, không hardcode
                var ncidCol = KetQuaGC.KQGCDBName.NCID;
                var khsxidCol = KetQuaGC.KQGCDBName.KHSXID;
                var slokCol = KetQuaGC.KQGCDBName.SLOK;
                var slngCol = KetQuaGC.KQGCDBName.SLNG;
                
                command.CommandText = $@"
                    SELECT 
                        [{ncidCol}] as NCID,
                        [{khsxidCol}] as KHSXID,
                        SUM(CAST([{slokCol}] as int)) as SumOK,
                        SUM(CAST([{slngCol}] as int)) as SumNG,
                        SUM(CAST([{slokCol}] as int) + CAST([{slngCol}] as int)) as TotalOKNG
                    FROM [{KetQuaGC.KQGCDBName.Table_KetQuaGC}] 
                    WHERE [{ncidCol}] IN ({string.Join(", ", ncParams)})
                      AND [{khsxidCol}] IN ({string.Join(", ", khsxParams)})
                    GROUP BY [{ncidCol}], [{khsxidCol}]";

                using var reader = await command.ExecuteReaderAsync();
                int matchedCount = 0;
                
                while (await reader.ReadAsync())
                {
                    var ncid = reader["NCID"];
                    var khsxid = reader["KHSXID"];
                    
                    var sumok = reader["SumOK"] != DBNull.Value ? Convert.ToInt32(reader["SumOK"]) : 0;
                    var sumng = reader["SumNG"] != DBNull.Value ? Convert.ToInt32(reader["SumNG"]) : 0;
                    var total = reader["TotalOKNG"] != DBNull.Value ? Convert.ToInt32(reader["TotalOKNG"]) : 0;

                    // Try to find matching key trong result dictionary - normalize để match
                    var matchingKey = result.Keys.FirstOrDefault(k => 
                        (k.Item1?.ToString() == ncid?.ToString()) && 
                        (k.Item2?.ToString() == khsxid?.ToString()));
                    
                    if (matchingKey != default)
                    {
                        result[matchingKey] = (sumok, sumng, total);
                        matchedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Return default values for all pairs on error
                foreach (var pair in pairs)
                {
                    result[pair] = (0, 0, 0);
                }
            }

            return result;
        }

        #endregion

        #region Update Methods (Wrapper)

        /// <summary>
        /// Wrapper cho UpdateKHSXProperty - có thể thêm caching invalidation sau
        /// </summary>
        public (int result, string error) UpdateKHSXProperty(object? khsxid, string propertyName, object? value)
        {
            // Invalidate cache nếu có
            if (_cache != null && khsxid != null)
            {
                _cache.Remove($"KHSX_{khsxid}_ReduceTime");
                _cache.Remove($"KHSX_Ma_*_ReduceTime"); // Có thể cache theo pattern
            }

            return _originalService.UpdateKHSXProperty(khsxid, propertyName, value);
        }

        /// <summary>
        /// Wrapper cho GetListPhieuXuatKhos - có thể optimize thêm sau
        /// </summary>
        public (List<PhieuXuatKho> phieuxuatkhos, string errorMessage) GetListPhieuXuatKhos(Dictionary<string, object?> parameters)
        {
            return _originalService.GetListPhieuXuatKhos(parameters);
        }

        #endregion
    }
}

