using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ProcessManagement.Commons;
using ProcessManagement.Models;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.KHO_NVL.KiemKe;
using ProcessManagement.Models.KHO_NVL.NhapKho;
using ProcessManagement.Models.KHO_NVL.Tracking;
using ProcessManagement.Models.KHO_NVL.XuatKho;
using ProcessManagement.Models.KHSXs;
using ProcessManagement.Models.MAYMOC;
using ProcessManagement.Models.NHANVIEN;
using ProcessManagement.Models.SANPHAM;
using ProcessManagement.Models.TienDoGCs;
using System.Data;
using System.Text.RegularExpressions;
using static ProcessManagement.Models.KHSXs.KetQuaGC;
using ProcessManagement.Models.KHO_TPHAM;
using ProcessManagement.Models.KHSXs.MQL_Template;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace ProcessManagement.Services.SQLServer
{
    public class SQLServerServices
    {
        private readonly string? connectionString;

        public SQLServerServices()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            string? configConstring = configuration["ConnectionStrings:DBConnectionstring"];

            connectionString = configConstring;

            // kieu du lieu de khong bi loi font tieng Viet la nvarchar hoac nchar
        }

        // ------------------------------------------------------------------------------------- //
        #region Table_KHSX

        // Them ke hoach san xuat moi
        public (int, string) InsertNewKHSX(KHSX newKHSX)
        {
            List<Propertyy> newkhsxItems = newKHSX.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newkhsxItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newkhsxItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableKHSX}] ({columnNames}) OUTPUT INSERTED.{Common.KHSXID} VALUES ({parameterNames})";

                foreach (var item in newkhsxItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Load danh sach ke hoach san xuat ID
        public List<int> GetlistKHSXid()
        {
            List<int> listKHSXid = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.KHSXID}] FROM [{Common.TableKHSX}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int khsxid = int.Parse(reader[Common.KHSXID].ToString() ?? "0");

                    if (khsxid > 0)
                    {
                        listKHSXid.Add(khsxid);
                    }
                }
            }

            return listKHSXid;
        }

        public (List<KHSX> kHSXes, string) GetListKHSXsByAnyParmeters(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<KHSX> khsxs = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{Common.TableKHSX}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        KHSX khsx = new();

                        List<Propertyy> rowItems = khsx.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        khsxs.Add(khsx);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    khsxs.Clear(); // Clear the list in case of error
                }
            }
            return (khsxs, errorMessage);
        }

        public (List<KHSX> kHSXes, string) GetListKHSXsByAnyParmeters_AsignColumn(Dictionary<string, object?> parameters, List<string> requiredColumns, bool isgetAll = false)
        {
            List<KHSX> khsxs = new();
            string errorMessage = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var conditions = new List<string>();
                    var command = connection.CreateCommand();

                    // Construct the SELECT clause with only required columns
                    string columnList = requiredColumns.Any()
                        ? string.Join(", ", requiredColumns.Select(col => $"[{col}]"))
                        : "*";
                    command.CommandText = $"SELECT {columnList} FROM [{Common.TableKHSX}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }
                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        KHSX khsx = new();
                        List<Propertyy> rowItems = khsx.GetPropertiesValues()
                            .Where(item => requiredColumns.Contains(item.DBName ?? string.Empty))
                            .ToList();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;
                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];
                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }
                        khsxs.Add(khsx);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    khsxs.Clear(); // Clear the list in case of error
                }
            }
            return (khsxs, errorMessage);
        }

        // Get last ke hoach san xuat
        public KHSX GetLastKHSX()
        {
            KHSX khsx = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT TOP 1 * FROM [{Common.TableKHSX}] ORDER BY [{Common.KHSXID}] DESC";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowSPitems = khsx.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            // Get trang thai xuat kho NVL
            var colIsDonePXK = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.KHSXID, khsx.KHSXID.Value } }, Common.IsDonePXK).columnValues.FirstOrDefault();
            if (int.TryParse(colIsDonePXK?.ToString(), out int ispxkdone))
            {
                khsx.isDonePXK = ispxkdone == 1;
            }

            // Get PNKID
            var colpnkid = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PXKID, khsx.PXKID.Value } }, Common.PNKID).columnValues.FirstOrDefault();
            if (int.TryParse(colpnkid?.ToString(), out int pnkid))
            {
                // Get trang thai tra NVL
                var colIsReturnedNVL = GetPNK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PNKID, pnkid } }, Common.IsDonePNK).columnValues.FirstOrDefault();
                if (int.TryParse(colIsReturnedNVL?.ToString(), out int isPNKdone))
                {
                    khsx.isReturnedNVL = isPNKdone == 1;
                }
            }

            return khsx;
        }

        public KHSX GetKHSXbyID(object? khsxID)
        {
            KHSX khsx = new();

            if (khsxID == null) { return khsx; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.KHSXID}] = '{khsxID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowSPitems = khsx.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }

                khsx.TargetSanPham = GetSanpham(int.TryParse(khsx.SPID.Value?.ToString(), out int spid) ? spid : 0);

                khsx.LoaiNVL = GetLoaiNVLbyID(int.TryParse(khsx.LOAINVLID.Value?.ToString(), out int loainvlid) ? loainvlid : 0);

                khsx.DSachNVLofKHSXs = GetListNVLofKHSXbyID(khsx.KHSXID.Value);

                khsx.DSachCongDoans = GetlistCongdoans(khsx.KHSXID.Value);

                var fistCDoanID = khsx.DSachCongDoans.FirstOrDefault()?.NCID.Value ?? 0;

                var resultdslots = GetListLOT_khsx(new() { { KHSX_LOT.DBName.KHSXID, khsx.KHSXID.Value }, { KHSX_LOT.DBName.NCID, fistCDoanID } }).Item1;

                if (resultdslots != null && resultdslots.Any())
                {
                    khsx.DSLOT_KHSXs = resultdslots;

                    // Get maNVL of KHSX_LOT
                    foreach (var lotkhsx in khsx.DSLOT_KHSXs)
                    {
                        lotkhsx.TargetNVL = GetMaNguyenVatLieuByID(lotkhsx.NVLID.Value);
                    }
                }

                // Get trang thai xuat kho NVL
                var colIsDonePXK = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.KHSXID, khsx.KHSXID.Value } }, Common.IsDonePXK).columnValues.FirstOrDefault();
                if (int.TryParse(colIsDonePXK?.ToString(), out int ispxkdone))
                {
                    khsx.isDonePXK = ispxkdone == 1;
                }

                // Get PNKID
                var colpnkid = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PXKID, khsx.PXKID.Value } }, Common.PNKID).columnValues.FirstOrDefault();
                if (int.TryParse(colpnkid?.ToString(), out int pnkid))
                {
                    // Get trang thai tra NVL
                    var colIsReturnedNVL = GetPNK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PNKID, pnkid } }, Common.IsDonePNK).columnValues.FirstOrDefault();
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
            }

            return khsx;
        }

        public KHSX GetKHSXbyIDRuduceTime(object? khsxID)
        {
            KHSX khsx = new();

            if (khsxID == null) { return khsx; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.KHSXID}] = '{khsxID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowSPitems = khsx.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }

                khsx.TargetSanPham = GetSanpham(int.TryParse(khsx.SPID.Value?.ToString(), out int spid) ? spid : 0, false);

                //khsx.LoaiNVL = GetLoaiNVLbyID(int.TryParse(khsx.LOAINVLID.Value?.ToString(), out int loainvlid) ? loainvlid : 0);

                khsx.DSachNVLofKHSXs = GetListNVLofKHSXbyID(khsx.KHSXID.Value);

                khsx.DSachCongDoans = GetlistCongdoans(khsx.KHSXID.Value, false);

                var fistCDoanID = khsx.DSachCongDoans.FirstOrDefault()?.NCID.Value ?? 0;

                var resultdslots = GetListLOT_khsx(new() { { KHSX_LOT.DBName.KHSXID, khsx.KHSXID.Value }, { KHSX_LOT.DBName.NCID, fistCDoanID } }).Item1;

                if (resultdslots != null && resultdslots.Any())
                {
                    khsx.DSLOT_KHSXs = resultdslots;

                    // Get maNVL of KHSX_LOT
                    foreach (var lotkhsx in khsx.DSLOT_KHSXs)
                    {
                        lotkhsx.TargetNVL = GetMaNguyenVatLieuByID(lotkhsx.NVLID.Value);
                    }
                }

                // Get trang thai xuat kho NVL
                var colIsDonePXK = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.KHSXID, khsx.KHSXID.Value } }, Common.IsDonePXK).columnValues.FirstOrDefault();
                if (int.TryParse(colIsDonePXK?.ToString(), out int ispxkdone))
                {
                    khsx.isDonePXK = ispxkdone == 1;
                }

                // Get PNKID
                var colpnkid = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PXKID, khsx.PXKID.Value } }, Common.PNKID).columnValues.FirstOrDefault();
                if (int.TryParse(colpnkid?.ToString(), out int pnkid))
                {
                    // Get trang thai tra NVL
                    var colIsReturnedNVL = GetPNK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PNKID, pnkid } }, Common.IsDonePNK).columnValues.FirstOrDefault();
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
            }

            return khsx;
        }

        public KHSX GetKHSXbyMaKHSXRuduceTime(object? maKHSX)
        {
            KHSX khsx = new();

            if (maKHSX == null) { return khsx; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.MaLSX}] = '{maKHSX}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowSPitems = khsx.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }

                khsx.TargetSanPham = GetSanpham(int.TryParse(khsx.SPID.Value?.ToString(), out int spid) ? spid : 0, false);

                khsx.DSachNVLofKHSXs = GetListNVLofKHSXbyID(khsx.KHSXID.Value);

                khsx.DSachCongDoans = GetlistCongdoans(khsx.KHSXID.Value, false);

                var fistCDoanID = khsx.DSachCongDoans.FirstOrDefault()?.NCID.Value ?? 0;

                var resultdslots = GetListLOT_khsx(new() { { KHSX_LOT.DBName.KHSXID, khsx.KHSXID.Value }, { KHSX_LOT.DBName.NCID, fistCDoanID } }).Item1;

                if (resultdslots != null && resultdslots.Any())
                {
                    khsx.DSLOT_KHSXs = resultdslots;

                    // Get maNVL of KHSX_LOT
                    foreach (var lotkhsx in khsx.DSLOT_KHSXs)
                    {
                        lotkhsx.TargetNVL = GetMaNguyenVatLieuByID(lotkhsx.NVLID.Value);
                    }
                }

                // Get trang thai xuat kho NVL
                var colIsDonePXK = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.KHSXID, khsx.KHSXID.Value } }, Common.IsDonePXK).columnValues.FirstOrDefault();
                if (int.TryParse(colIsDonePXK?.ToString(), out int ispxkdone))
                {
                    khsx.isDonePXK = ispxkdone == 1;
                }

                // Get PNKID
                var colpnkid = GetPXK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PXKID, khsx.PXKID.Value } }, Common.PNKID).columnValues.FirstOrDefault();
                if (int.TryParse(colpnkid?.ToString(), out int pnkid))
                {
                    // Get trang thai tra NVL
                    var colIsReturnedNVL = GetPNK_AnyColValuebyAnyParameters(new Dictionary<string, object?>() { { Common.PNKID, pnkid } }, Common.IsDonePNK).columnValues.FirstOrDefault();
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
            }

            return khsx;
        }

        // Get KHSXID by maKHSX
        public int GetKHSXIDbyMaKHSX(object? maKHSX)
        {
            int khsxid = -1;

            if (maKHSX == null) { return khsxid; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.KHSXID}] FROM [{Common.TableKHSX}] WHERE [{Common.MaLSX}] = '{maKHSX}'";

                object result = command.ExecuteScalar();

                if (result != null)
                {
                    khsxid = int.Parse(result.ToString() ?? "-1");
                }
                else khsxid = -1;
            }

            return khsxid;
        }

        public int GetSPIDbyMaKHSX(object? maKHSX)
        {
            int spid = -1;

            if (maKHSX == null) { return spid; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.SP_SPID}] FROM [{Common.TableKHSX}] WHERE [{Common.MaLSX}] = '{maKHSX}'";

                object result = command.ExecuteScalar();

                if (result != null)
                {
                    spid = int.Parse(result.ToString() ?? "-1");
                }
                else spid = -1;
            }

            return spid;
        }

        // Get any columns of KHSX by any paramaters
        public (List<object?> columnValues, string errorMessage) GetAnyColValuebyAnyParameters_KHSX(Dictionary<string, object?> parameters, string? returnColumnName = null, bool isGetAll = false)
        {
            List<object?> columnValues = new();
            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var conditions = new List<string>();
                    var command = connection.CreateCommand();

                    // If a specific column is requested, select only that column
                    string selectClause = returnColumnName != null
                        ? $"SELECT [{returnColumnName}]"
                        : "SELECT *";

                    command.CommandText = $"{selectClause} FROM [{Common.TableKHSX}]";

                    if (!isGetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }
                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // If no specific column is requested, read entire object
                        if (returnColumnName == null)
                        {
                            KHSX khsx = new();
                            List<Propertyy> rowItems = khsx.GetPropertiesValues();
                            foreach (var item in rowItems)
                            {
                                string? columnName = item.DBName;
                                if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                                {
                                    object columnValue = reader[columnName];
                                    item.Value = columnValue == DBNull.Value ? null : columnValue;
                                }
                                columnValues.Add(khsx);
                            }
                        }
                        else
                        {
                            // If a specific column is requested, read only that column
                            object columnValue = reader[returnColumnName];
                            columnValues.Add(columnValue == DBNull.Value ? null : columnValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    columnValues.Clear(); // Clear the list in case of error
                }
            }
            return (columnValues, errorMessage);
        }


        // Delete Ke hoach san xuat
        public (int, string) DeleteKehoachSanxuat(KHSX? removeKHSX)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeKHSX == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.TableKHSX} WHERE [{Common.KHSXID}] = '{removeKHSX.KHSXID.Value}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Insert new cong doan into KHSX
        public (int, string) InsertCongdoantoKehoachSanxuat(NguyenCongofKHSX newCongdoan)
        {
            List<Propertyy> newcdItems = newCongdoan.GetPropertiesValues().Where(po => (po.AlowDatabase == true && po.Value != null)).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newcdItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newcdItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableCongDoan}] ({columnNames}) OUTPUT INSERTED.{Common.NCIDofKHSX} VALUES ({parameterNames})";

                foreach (var item in newcdItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Update KHSX property
        public (int, string) UpdateKHSXProperty(object? khsxid, string propertyName, object? value)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.TableKHSX} SET [{propertyName}] = N'{value}' WHERE [{Common.KHSXID}] = '{khsxid}' ";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_NguyenCongofKHSX
        // Load danh sach cong doan
        public List<NguyenCongofKHSX> GetlistCongdoans(object? khsxID, bool isloaddata = true)
        {
            List<NguyenCongofKHSX> listCongdoans = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableCongDoan}] WHERE [{Common.KHSXID}] = '{khsxID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenCongofKHSX rowSP = new();

                    List<Propertyy> rowSPitems = rowSP.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    rowSP.TenCongDoan.Value = null;

                    // Update TenCongDoan
                    rowSP.TenCongDoan.Value = GetNguyenCongByID(rowSP.NCID.Value);

                    listCongdoans.Add(rowSP);
                }
            }

            if (isloaddata)
            {
                Parallel.ForEach(listCongdoans, congdoan =>
                {
                    congdoan.IsUsing = true;

                    congdoan.DSachNVLCongDoans = GetlistNVLmoiCongdoans(congdoan.NCIDofKHSX.Value ?? 0, khsxID);
                });
            }

            return listCongdoans;
        }
        public (int, string) UpdateNguyenCongofKHSX(NguyenCongofKHSX nguyencong)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (nguyencong == null) return (result, "Error: is null");

            List<Propertyy> properties = nguyencong.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{Common.TableCongDoan}] SET {updateSet} WHERE [{Common.NCIDofKHSX}] = '{nguyencong.NCIDofKHSX.Value}'";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion Table_NguyenCongofKHSX

        // ------------------------------------------------------------------------------------- //
        #region Table_NVLmoiNguyenCong
        // Them nvl moi cong doan
        public (int, string) InsertNVLmoiCongdoan(NVLmoiNguyenCong nvlmoicd)
        {
            List<Propertyy> Items = nvlmoicd.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", Items.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", Items.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableNVLmoiCongDoan}] ({columnNames}) OUTPUT INSERTED.{Common.NVLMCDID} VALUES ({parameterNames})";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Them nvl moi cong doan ca lam viec
        public (int, string) InsertNVLmoiCongdoanCalamviec(CaLamViec calamviec)
        {
            List<Propertyy> Items = calamviec.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", Items.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", Items.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableCalamviec}] ({columnNames}) OUTPUT INSERTED.{Common.CLVID} VALUES ({parameterNames})";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Load ca lam viec moi nvl moi cong doan
        public List<CaLamViec> GetlistNVLmoiCongdoanCalamviecs(object? nvlmcdID)
        {
            List<CaLamViec> listCalamviecs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableCalamviec}] WHERE [{Common.NVLMCDID}] = '{nvlmcdID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    CaLamViec rowSP = new();

                    List<Propertyy> rowSPitems = rowSP.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listCalamviecs.Add(rowSP);
                }
            }

            return listCalamviecs;
        }

        // Load danh sach NVL moi cong doan
        public List<NVLmoiNguyenCong> GetlistNVLmoiCongdoans(object congdoanID, object? khsxID)
        {
            List<NVLmoiNguyenCong> listNVLmoiCongdoans = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVLmoiCongDoan}] WHERE [{Common.KHSXID}] = '{khsxID}' AND [{Common.NCIDofKHSX}] = '{congdoanID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLmoiNguyenCong rowSP = new();

                    List<Propertyy> rowSPitems = rowSP.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listNVLmoiCongdoans.Add(rowSP);
                }
            }

            Parallel.ForEach(listNVLmoiCongdoans, nvl =>
            {
                List<CaLamViec> listCaLamviec = GetlistNVLmoiCongdoanCalamviecs(nvl.NVLMCDID.Value);

                nvl.CaNgay = listCaLamviec.FirstOrDefault(ca => ca.Ca?.Value?.ToString() == Common.Cangay) ?? new CaLamViec();

                nvl.CaDem = listCaLamviec.FirstOrDefault(ca => ca.Ca?.Value?.ToString() == Common.Cadem) ?? new CaLamViec();
            });

            return listNVLmoiCongdoans;
        }
        public async Task<List<NVLmoiNguyenCong>> GetlistNVLmoiCongdoansAsync(object congdoanID, object? khsxID)
        {
            List<NVLmoiNguyenCong> listNVLmoiCongdoans = new();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{Common.TableNVLmoiCongDoan}] WHERE [{Common.KHSXID}] = @khsxID AND [{Common.NCIDofKHSX}] = @congdoanID";
                command.Parameters.AddWithValue("@khsxID", khsxID);
                command.Parameters.AddWithValue("@congdoanID", congdoanID);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    NVLmoiNguyenCong rowSP = new();
                    List<Propertyy> rowSPitems = rowSP.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;
                        object? columnValue = await reader.IsDBNullAsync(reader.GetOrdinal(columnName)) ? null : reader[columnName];
                        item.Value = columnValue;
                    }

                    listNVLmoiCongdoans.Add(rowSP);
                }
            }

            // Use Task.Run to run the following code in parallel
            await Task.Run(() =>
            {
                Parallel.ForEach(listNVLmoiCongdoans, nvl =>
                {
                    List<CaLamViec> listCaLamviec = GetlistNVLmoiCongdoanCalamviecs(nvl.NVLMCDID.Value);
                    nvl.CaNgay = listCaLamviec.FirstOrDefault(ca => ca.Ca?.Value?.ToString() == Common.Cangay) ?? new CaLamViec();
                    nvl.CaDem = listCaLamviec.FirstOrDefault(ca => ca.Ca?.Value?.ToString() == Common.Cadem) ?? new CaLamViec();
                });
            });

            return listNVLmoiCongdoans;
        }

        // Update calam viec
        public (int, string) UpdateCalamviec(NVLmoiNguyenCong? nVLmoiCong, TemNVLMCDValues temNVLMCD)
        {
            int result = -1; string errorMess = string.Empty;

            if (nVLmoiCong == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.TableCalamviec} SET [{Common.OK}] = '{temNVLMCD.slOK}', [{Common.NG}] = '{temNVLMCD.slNG}' ,[{Common.NgayGiaCong}] = '{temNVLMCD.ngayGC}', [{Common.NhanVien}] = '{temNVLMCD.tenNV}' " +
                                    $", [{Common.IsUpdated}] = '{1}' WHERE [{Common.NVLMCDID}] = '{nVLmoiCong.NVLMCDID.Value}' AND [{Common.Ca}] = N'{temNVLMCD.Ca}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        // Update so luong sau gia cong moi cong doan
        public (int, string) UpdateSLsaugiacongNVLMCD(object? nvlmcdid, int slsaugiacong, int tongNG, int isupdated)
        {
            int result = -1; string errorMess = string.Empty;

            if (nvlmcdid == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.TableNVLmoiCongDoan} SET [{Common.SLSauGiaCong}] = '{slsaugiacong}', [{Common.TongOK}] = '{slsaugiacong}' " +
                        $", [{Common.TongNG}] = '{tongNG}', [{Common.IsUpdated}] = '{isupdated}' WHERE [{Common.NVLMCDID}] = '{nvlmcdid}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        // Update sltruocgiacong cua nguyen cong tiep theo
        public (int, string) UpdateSLTruocgiacongNextNguyenCong(int cdid, string maquanly, int sltruocgiacong)
        {
            int result = -1; string errorMess = string.Empty;

            if (cdid <= 0) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.TableNVLmoiCongDoan} SET [{Common.SLTruocGiaCong}] = '{sltruocgiacong}' WHERE [{Common.NCIDofKHSX}] = '{cdid}' AND [{Common.MaQuanLy}] = '{maquanly}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        #endregion Table_NVLmoiNguyenCong

        // ------------------------------------------------------------------------------------- //
        #region Table_NguyenCong

        // Insert
        public (int, string) InsertNguyenCong(NguyenCong newNC)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (newNC == null) return (result, "Error: NguyenCong is null");

            List<Propertyy> properties = newNC.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{NguyenCong.DBName.Table_NguyenCong}] ({columns}) OUTPUT INSERTED.{NguyenCong.DBName.NCID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get nguyen cong
        public NguyenCong GetNguyenCong(object ncid)
        {
            NguyenCong nguyencong = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenCong}] WHERE [{Common.NCID}] = '{ncid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> NCitems = nguyencong.GetPropertiesValues();

                    foreach (var item in NCitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }
            }

            if (int.TryParse(nguyencong.IsHide.Value?.ToString(), out int ishiding))
            {
                nguyencong.isHiding = (ishiding == 1);
            }

            return nguyencong;
        }
        // Get nguyen cong by ID
        public string GetNguyenCongByID(object? id)
        {
            string result = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.NguyenCong}] FROM [{Common.Table_NguyenCong}] WHERE [{Common.NCID}] = @ID";
                command.Parameters.AddWithValue("@ID", id ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    result = reader[Common.NguyenCong].ToString()?.Trim() ?? string.Empty;
                }
            }
            return result;
        }

        // Load danh sach nguyen cong
        public List<NguyenCong> GetListNguyenCongs()
        {
            List<NguyenCong> lisNCs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenCong}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenCong nguyencong = new();

                    List<Propertyy> NCitems = nguyencong.GetPropertiesValues();

                    foreach (var item in NCitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    if (int.TryParse(nguyencong.IsHide.Value?.ToString(), out int ishiding))
                    {
                        nguyencong.isHiding = (ishiding == 1);
                    }

                    lisNCs.Add(nguyencong);
                }
            }

            return lisNCs;
        }

        // Get
        public (List<NguyenCong> nguyencongs, string error) GetListNguyenCongs(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<NguyenCong> listNguyenCongs = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{NguyenCong.DBName.Table_NguyenCong}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{Common.RemoveDiacriticsAndSpaces(param.Key)}");

                            command.Parameters.AddWithValue($"@{Common.RemoveDiacriticsAndSpaces(param.Key)}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        NguyenCong nguyencong = new();

                        List<Propertyy> rowItems = nguyencong.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        foreach (var ncid in Common.GetListNCIDs(nguyencong.NGIDs.Value))
                        {
                            var ngtype = GetDanhSachNGType(ncid).Item1.FirstOrDefault();

                            if (ngtype != null && ngtype.NGID.Value != null)
                            {
                                nguyencong.DSNGTypes.Add(ngtype);
                            }
                        }

                        if (int.TryParse(nguyencong.IsHide.Value?.ToString(), out int ishiding))
                        {
                            nguyencong.isHiding = (ishiding == 1);
                        }

                        listNguyenCongs.Add(nguyencong);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listNguyenCongs.Clear(); // Clear the list in case of error
                }
            }
            return (listNguyenCongs, errorMessage);
        }

        // Delete
        public (bool, string) DeleteNguyenCong(object? ncid)
        {
            // Check for valid ID
            if (ncid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{NguyenCong.DBName.Table_NguyenCong}] WHERE [{NguyenCong.DBName.NCID}] = @NCID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@NCID", ncid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        // Update
        public (int, string) UpdateNguyenCongMainDetails(NguyenCong nguyencong)
        {
            int result = -1; string errorMess = string.Empty;

            if (nguyencong == null) return (result, errorMess);

            List<Propertyy> Items = nguyencong.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{NguyenCong.DBName.Table_NguyenCong}] SET {setClause} WHERE [{NguyenCong.DBName.NCID}] = '{nguyencong.NCID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value ?? DBNull.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Check gia tri truong thong tin
        public bool DefaultThongTinNguyenCong_ValueIsExisting(string? proValue, string proName)
        {
            using var connection = new SqlConnection(connectionString);

            string query = $"SELECT COUNT(*) FROM [{NguyenCong.DBName.Table_NguyenCong}] WHERE [{proName}] = N'{proValue?.Trim()}'";

            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();

                int count = (int)command.ExecuteScalar();

                return count > 0;
            }
        }

        #endregion Table_NguyenCong

        // ------------------------------------------------------------------------------------- //
        #region Table_NVLwithSanPham
        // Get danh sach NVL cua san pham
        public List<NVLwithSanPham> GetDSachNVLwithSanPham_bySPID(object? spid)
        {
            List<NVLwithSanPham> listNVLofSanphams = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NVLwithSanPham}] WHERE [{Common.SP_SPID}] = '{spid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLwithSanPham nvlofsp = new();

                    List<Propertyy> rowitems = nvlofsp.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    // Load target nguyen vat lieu
                    nvlofsp.TargetNgLieu = GetNguyenVatLieuByID_MultipleTask(nvlofsp.NVLID.Value);

                    listNVLofSanphams.Add(nvlofsp);
                }
            }

            return listNVLofSanphams;
        }

        // Get danh sach SP cua NVL
        public List<NVLwithSanPham> GetDSachNVLwithSanPham_byNVLID(object? nvlid)
        {
            List<NVLwithSanPham> listNVLofSanphams = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NVLwithSanPham}] WHERE [{Common.NVLID}] = '{nvlid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLwithSanPham nvlofsp = new();

                    List<Propertyy> rowitems = nvlofsp.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    // Khong load TargetSP - tranh deadloop

                    listNVLofSanphams.Add(nvlofsp);
                }
            }

            return listNVLofSanphams;
        }


        // Kiem tra da ton tai NVL trong ds NVL cua san pham // Table Table_NVLofSanPham //
        public bool IsNVLwithSanPhamExisting(NVLwithSanPham nVLofSanPham)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_NVLwithSanPham}] WHERE [{Common.SP_SPID}] = '{nVLofSanPham.SPID.Value}' AND [{Common.NVLID}] = '{nVLofSanPham.NVLID.Value}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Them loai NVL cho san pham // Table Table_NVLofSanPham //
        public (int, string) InsertNewNVLwithSanPham(NVLwithSanPham newnvl)
        {
            List<Propertyy> newNVLItems = newnvl.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newNVLItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newNVLItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_NVLwithSanPham}] ({columnNames}) OUTPUT INSERTED.{Common.NVLID} VALUES ({parameterNames})";

                foreach (var item in newNVLItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update NVL cho san pham
        public (int, string) UpdateNVLwithSanPham(NVLwithSanPham updateNVLofSP)
        {
            int result = -1; string errorMess = string.Empty;

            if (updateNVLofSP == null) return (result, errorMess);

            List<Propertyy> Items = updateNVLofSP.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_NVLwithSanPham}] SET {setClause} WHERE [{Common.SP_NVLSPID}] = '{updateNVLofSP.NVLSPID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Xoa NVL cua SP // Table Table_NVLofSanPham //
        public (int, string) DeleteNVLwithSanpham(NVLwithSanPham? removeNVLofSP)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeNVLofSP == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_NVLwithSanPham} WHERE [{Common.SP_NVLSPID}] = '{removeNVLofSP.NVLSPID.Value}'";

                object rs = command.ExecuteNonQuery();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_NVLofKHSX
        // Insert NVLofKHSX
        public (int, string) InsertNVLofKHSX(NVLofKHSX nvlofkhsx)
        {
            List<Propertyy> newNVLItems = nvlofkhsx.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newNVLItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newNVLItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableNVLofKHSX}] ({columnNames}) OUTPUT INSERTED.{Common.NVLKHSXID} VALUES ({parameterNames})";

                foreach (var item in newNVLItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get list NVLofKHSX by ID
        public List<NVLofKHSX> GetListNVLofKHSXbyID(object? khsxid)
        {
            List<NVLofKHSX> listnvls = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVLofKHSX}] WHERE [{Common.KHSXID}] = '{khsxid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLofKHSX nvl = new();

                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    nvl.TenNVL = GetMaNguyenVatLieuByID(nvl.NVLID.Value).MaNVL.Value?.ToString() ?? string.Empty;

                    listnvls.Add(nvl);
                }
            }

            return listnvls;
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table KHO_NguyenVatLieu

        // Get Nguyen Vat Lieu by ID using multitask
        public NguyenVatLieu GetNguyenVatLieuByID_MultipleTask(object? nvlID)
        {
            NguyenVatLieu nvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenVatLieu}] WHERE [{Common.NVLID}] = '{nvlID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                }
            }

            if (nvl.NVLID.Value != null)
            {
                // Use Task.WhenAll for parallel loading of dependent data
                var tasks = new List<Task>
                {
                    Task.Run(() => nvl.DSThongTin = GetNguyenVatLieuDetails(nvl.NVLID.Value)),
                    Task.Run(() => nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value)),
                    Task.Run(() => nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value)),
                    Task.Run(() => nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value)),
                    Task.Run(() => nvl.DSachSPofNVLs = GetDSachNVLwithSanPham_byNVLID(nvl.NVLID.Value))
                };

                // Wait for all tasks to complete
                Task.WhenAll(tasks).Wait();

                // Compute TonKho after parallel loading
                nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);
            }

            //// Load Details NVL 
            //nvl.DSThongTin = GetNguyenVatLieuDetails(nvl.NVLID.Value);
            //// Load Danh muc
            //nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
            //// Load Loai NVL
            //nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
            //// Load list vi tri 
            //nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
            //// Tinh so luong ton kho
            //nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

            //// Load danh sach SPofNVL
            //nvl.DSachSPofNVLs = GetDSachNVLwithSanPham_byNVLID(nvl.NVLID.Value);

            return nvl;
        }

        // Get nguyen vat lieu by TenNVL
        public NguyenVatLieu GetNguyenVatLieuByTenNVL(object? tenNVL)
        {
            NguyenVatLieu nvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenVatLieu}] WHERE [{Common.MaNVL}] = '{tenNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                }
            }

            // Load Details NVL 
            nvl.DSThongTin = GetNguyenVatLieuDetails(nvl.NVLID.Value);
            // Load Danh muc
            nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
            // Load Loai NVL
            nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
            // Load list vi tri 
            nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
            // Tinh so luong ton kho
            nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

            // Load danh sach SPofNVL
            nvl.DSachSPofNVLs = GetDSachNVLwithSanPham_byNVLID(nvl.NVLID.Value);

            return nvl;
        }

        public NguyenVatLieu GetMaNguyenVatLieuByID(object? nvlid)
        {
            NguyenVatLieu nvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenVatLieu}] WHERE [{Common.NVLID}] = '{nvlid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }
                }
            }

            return nvl;
        }

        // Get list NguyenVatLieu by loai nvl ID
        public List<NguyenVatLieu> GetListNguyenVatLieuByLoaiNvlID(object? loainvlID)
        {
            List<NguyenVatLieu> nguyenvatlieus = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenVatLieu}] WHERE [{Common.LOAINVLID}] = '{loainvlID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenVatLieu nvl = new();

                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Load Details NVL 
                    nvl.DSThongTin = GetNguyenVatLieuDetails(nvl.NVLID.Value);
                    // Load Danh muc
                    nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
                    // Load Loai NVL
                    nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
                    // Load list vi tri 
                    nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
                    // Tinh so luong ton kho
                    nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

                    // Load danh sach SPofNVL
                    nvl.DSachSPofNVLs = GetDSachNVLwithSanPham_byNVLID(nvl.NVLID.Value);

                    nguyenvatlieus.Add(nvl);
                }
            }

            return nguyenvatlieus;
        }

        // Get list Nguyenvatlieu by DanhmucID
        public List<NguyenVatLieu> GetListNgVatLieuByDanhmucID(object? dmucID)
        {
            List<NguyenVatLieu> nguyenvatlieus = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenVatLieu}] WHERE [{Common.DMID}] = '{dmucID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenVatLieu nvl = new();

                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Load Details NVL 
                    nvl.DSThongTin = GetNguyenVatLieuDetails(nvl.NVLID.Value);
                    // Load Danh muc
                    nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
                    // Load Loai NVL
                    nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
                    // Load list vi tri 
                    nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
                    // Tinh so luong ton kho
                    nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

                    // Load danh sach SPofNVL
                    nvl.DSachSPofNVLs = GetDSachNVLwithSanPham_byNVLID(nvl.NVLID.Value);

                    nguyenvatlieus.Add(nvl);
                }
            }

            return nguyenvatlieus;
        }

        // Get list ngvatlieu ID
        public List<int> GetListNVLIds(object? tenNVL)
        {
            List<int> listNVLids = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.NVLID}] FROM [{Common.Table_NguyenVatLieu}] WHERE [{Common.MaNVL}] = '{tenNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    _ = int.TryParse(reader[Common.NVLID]?.ToString(), out int nvlid) ? nvlid : 0;

                    if (nvlid > 0)
                    {
                        listNVLids.Add(nvlid);
                    }
                }
            }

            return listNVLids;
        }

        // Lấy danh sách ID của danh sách nguyên vật liệu
        public List<int> GetlistNgVatLieuId()
        {
            List<int> nvlIDs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.NVLID}] FROM [{Common.Table_NguyenVatLieu}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int nvlid = int.Parse(reader[Common.NVLID].ToString() ?? "0");

                    if (nvlid > 0)
                    {
                        nvlIDs.Add(nvlid);
                    }
                }
            }

            return nvlIDs;
        }

        // Cập nhật giá trị các thông tin chính của NVL
        public (int, string) UpdateNguyenVatLieuMainDetails(NguyenVatLieu ngVatLieu)
        {
            int result = -1; string errorMess = string.Empty;

            if (ngVatLieu == null) return (result, errorMess);

            List<Propertyy> Items = ngVatLieu.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_NguyenVatLieu}] SET {setClause} WHERE [{Common.NVLID}] = '{ngVatLieu.NVLID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Check gia tri truong thong tin mac dinh nvl is exsting? 
        public bool DefaultThongTinNguyenVatLieu_ValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_NguyenVatLieu}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Insert new nguyen vat lieu
        public (int, string) InsertNewNguyenVatLieu(NguyenVatLieu? newNguyenVatLieu)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (newNguyenVatLieu == null) return (result, "Error");

            List<Propertyy> newNVLItems = newNguyenVatLieu.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newNVLItems.Select(key => $"[{key.DBName}]"));
                string parameterNames = string.Join(",", newNVLItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_NguyenVatLieu}] ({columnNames}) OUTPUT INSERTED.{Common.NVLID} VALUES ({parameterNames})";

                foreach (var item in newNVLItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete nguyen vat lieu by nvlid
        public (int, string) DeleteNguyenVatLieu(object? nvlID)
        {
            int result = -1;
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM {Common.Table_NguyenVatLieu} WHERE [{Common.NVLID}] = '{nvlID}'";

                object rs = command.ExecuteScalar();
                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }

            if (result > 0)
            {
                // Delete danh sach VitriofNVL 
            }

            return (result, errorMess);
        }
        #endregion

        #region Table_KHO_ThongTinNVL
        // Delete thong tin nguyen vat lieu
        public (int, string) DeleteThongTinNguyenVatLieu(object? thongtinNVLid)
        {
            int result = -1;
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM {Common.Table_ThongTinNVL} WHERE [{Common.TTNVLID}] = '{thongtinNVLid}'";

                object rs = command.ExecuteScalar();
                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Insert thong tin nguyen vat lieu
        public (int, string) InsertThongTinNguyenVatLieu(ThongTinNVL? ttNVL)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (ttNVL == null) return (result, "Error");

            List<Propertyy> items = ttNVL.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", items.Select(key => $"[{key.DBName}]"));
                string parameterNames = string.Join(",", items.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_ThongTinNVL}] ({columnNames}) OUTPUT INSERTED.{Common.TTNVLID} VALUES ({parameterNames})";

                foreach (var item in items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update danh sach thong tin nguyen vat lieu
        public (int, string) UpdateListExtraNguyenVatLieuDetails(List<ThongTinNVL> thongtinNguyenVatLieus)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (thongtinNguyenVatLieus == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                foreach (var thongtin in thongtinNguyenVatLieus)
                {
                    List<Propertyy> items = thongtin.GetPropertiesValues().Where(pro => pro.DBName == Common.GiaTri).ToList();
                    var command = connection.CreateCommand();

                    string setClause = string.Join(",", items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));
                    command.CommandText = $"UPDATE [{Common.Table_ThongTinNVL}] SET {setClause} WHERE [{Common.TTNVLID}] = '{thongtin.TTNVLID.Value}'";

                    foreach (var item in items)
                    {
                        string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";
                        object? parameterValue = item.Value;

                        command.Parameters.AddWithValue(parameterName, parameterValue);
                    }

                    result = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get danh sach thong tin nguyen vat lieu by nvlid
        public List<ThongTinNVL> GetDanhSachThongTinNguyenVatLieu(object? nvlID)
        {
            List<ThongTinNVL> danhSachThongTinNguyenVatLieu = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ThongTinNVL}] WHERE [{Common.NVLID}] = @NVLID";

                command.Parameters.AddWithValue("@NVLID", nvlID);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ThongTinNVL thongTinNguyenVatLieu = new();

                    List<Propertyy> rowItems = thongTinNguyenVatLieu.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Get loai thong tin nguyen vat lieu
                    thongTinNguyenVatLieu.LoaiThongTin = GetLoaiThongTinNguyenVatLieu(thongTinNguyenVatLieu.LoaiTTNVLID.Value);

                    danhSachThongTinNguyenVatLieu.Add(thongTinNguyenVatLieu);
                }
            }

            return danhSachThongTinNguyenVatLieu;
        }

        #endregion

        #region Table_KHO_LoaiThongTinNVL
        // Thêm mới loại thông tin nguyên vật liệu
        public (int, string) InsertNewLoaiThongTinNguyenVatLieu(LoaiThongTinNVL loaithongtinnvl)
        {
            List<Propertyy> newItems = loaithongtinnvl.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1;
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));
                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LoaiThongTinNVL}] ({columnNames}) OUTPUT INSERTED.{Common.LoaiTTNVLID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Get loai thong tin nguyen vat lieu by loaditnvlid
        public LoaiThongTinNVL GetLoaiThongTinNguyenVatLieu(object? loaditnvlid)
        {
            LoaiThongTinNVL loaiThongTinNguyenVatLieu = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNVL}] WHERE [{Common.LoaiTTNVLID}] = @LoaiTTNVLID";

                command.Parameters.AddWithValue("@LoaiTTNVLID", loaditnvlid);

                using var reader = command.ExecuteReader();

                if (reader.Read()) // Use if since we're expecting a single result
                {
                    List<Propertyy> rowItems = loaiThongTinNguyenVatLieu.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            return loaiThongTinNguyenVatLieu;
        }
        // Get danh sach loai thong tin nguyen vat lieu
        public List<LoaiThongTinNVL> GetDanhSachLoaiThongTinNguyenVatLieu()
        {
            List<LoaiThongTinNVL> danhSachLoaiThongTinNguyenVatLieu = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNVL}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinNVL loaiThongTinNguyenVatLieu = new();

                    List<Propertyy> rowItems = loaiThongTinNguyenVatLieu.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinNguyenVatLieu.Add(loaiThongTinNguyenVatLieu);
                }
            }

            return danhSachLoaiThongTinNguyenVatLieu;
        }
        // Get danh sach loai thong tin nguyen vat lieu - mac dinh
        public List<LoaiThongTinNVL> GetDanhSachLoaiThongTinNguyenVatLieu(object isDefault)
        {
            List<LoaiThongTinNVL> danhSachLoaiThongTinNguyenVatLieu = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNVL}] WHERE [{Common.IsDefault}] = '{isDefault}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinNVL loaiThongTinNguyenVatLieu = new();

                    List<Propertyy> rowItems = loaiThongTinNguyenVatLieu.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinNguyenVatLieu.Add(loaiThongTinNguyenVatLieu);
                }
            }

            return danhSachLoaiThongTinNguyenVatLieu;
        }
        // Kiem tra ten loai thong tin nguyen vat lieu da ton tai
        public bool IsExisting_LoaiThongTinNguyenVatLieu_Name(string? proValue)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_LoaiThongTinNVL}] WHERE [{Common.TenTruyXuat}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }
        // Thay đổi tên của loại thông tin nguyên vật liệu
        public (int, string) UpdateLoaiThongTinNguyenVatLieuName(object? loaittnvlid, string newName, string tentruyxuat)
        {
            int result = -1;
            string errorMess = string.Empty;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_LoaiThongTinNVL} SET [{Common.TenLoaiThongTin}] = N'{newName}', [{Common.TenTruyXuat}] = '{tentruyxuat}' WHERE [{Common.LoaiTTNVLID}] = '{loaittnvlid}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }
        // Xóa loại thông tin trong bảng danh sách thông tin NVL
        public (int, string) DeleteLoaiThongTinNVL(object? tenttID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_LoaiThongTinNVL} WHERE [{Common.LoaiTTNVLID}] = '{tenttID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_NguyenLieuDetails

        // Kiểm tra thông tin đã tồn tại trong danh sách nvl details
        public bool IsDetailExistingInNVLDetailsList(object? tenttid, object? nvlid)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_ThongTinNVL}] WHERE [{Common.LoaiTTNVLID}] = N'{tenttid}' AND [{Common.NVLID}] = {nvlid}";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Load thong tin of nguyen vat lieu
        public List<ThongTinNVL> GetNguyenVatLieuDetails(object? nvlid)
        {
            List<ThongTinNVL> nvlDetails = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ThongTinNVL}] WHERE [{Common.NVLID}] = '{nvlid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ThongTinNVL detail = new();

                    List<Propertyy> rowitems = detail.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Load ten thong tin 
                    detail.LoaiThongTin = GetNVLdetailsNamebyID(detail.LoaiTTNVLID.Value);

                    nvlDetails.Add(detail);
                }
            }

            return nvlDetails;
        }

        // Thêm trường thông tin chi tiết mới cho nguyên vật liệu
        public (int, string) InsertNewNguyenVatLieuDetail(ThongTinNVL newnvldetail)
        {
            List<Propertyy> newItems = newnvldetail.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_ThongTinNVL}] ({columnNames}) OUTPUT INSERTED.{Common.TTNVLID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }


        // Xóa trường thông tin ở các nguyên vật liệu khác
        public (int, string) DeleteThongTinNgVatLieuByTenTTID(object? tenttID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_ThongTinNVL} WHERE [{Common.LoaiTTNVLID}] = '{tenttID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Cập nhật danh sách thông tin phụ của nguyên vật liệu
        public (int, string) UpdateListNguyenVatLieuDetails(List<ThongTinNVL> ngvatlieudetails)
        {
            int result = -1; string errorMess = string.Empty;

            if (ngvatlieudetails == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                foreach (var nvldetail in ngvatlieudetails)
                {
                    List<Propertyy> items = nvldetail.GetPropertiesValues().Where(pro => pro.DBName == Common.GiaTri).ToList();

                    var command = connection.CreateCommand();

                    string setClause = string.Join(",", items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                    command.CommandText = $"UPDATE [{Common.Table_ThongTinNVL}] SET {setClause} WHERE [{Common.TTNVLID}] = '{nvldetail.TTNVLID.Value}'";

                    foreach (var item in items)
                    {
                        string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                        object? parameterValue = item.Value;

                        command.Parameters.AddWithValue(parameterName, parameterValue);
                    }

                    result = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_NVLDetailsListName
        // Load list name of thong tin nguyen vat lieu 
        public List<LoaiThongTinNVL> GetListNVLdetailsName()
        {
            List<LoaiThongTinNVL> names = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNVL}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinNVL name = new();

                    List<Propertyy> rowitems = name.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    names.Add(name);
                }
            }

            return names;
        }

        // Get name of nguyen vat lieu detail column 
        public LoaiThongTinNVL GetNVLdetailsNamebyID(object? tenttID)
        {
            LoaiThongTinNVL detailname = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNVL}] WHERE [{Common.LoaiTTNVLID}] = '{tenttID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = detailname.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            return detailname;
        }

        // Check ten nvl details da ton tai
        public bool IsNVLDetailExisting(string? tenNVLDetail)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_LoaiThongTinNVL}] WHERE [{Common.TenLoaiThongTin}] = N'{tenNVLDetail}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Them NVL detail name moi
        public (int, string) InsertNewNVLDetailName(LoaiThongTinNVL newnvldetailName)
        {
            List<Propertyy> newItems = newnvldetailName.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LoaiThongTinNVL}] ({columnNames}) OUTPUT INSERTED.{Common.LoaiTTNVLID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }



        // Thay đổi tên của trường thông tin nguyên vật liệu
        public (int, string) UpdateNVLDetailName(object? tenttid, string newName, string tentruyxuat)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_LoaiThongTinNVL} SET [{Common.TenLoaiThongTin}] = N'{newName}', [{Common.TenTruyXuat}] = '{tentruyxuat}' WHERE [{Common.LoaiTTNVLID}] = '{tenttid}' ";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_ViTriLuuTru

        // Insert
        public (int, string) InsertViTriLuuTru(VitriLuuTru vitriluutru)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (vitriluutru == null) return (result, "Error: log is null");

            List<Propertyy> properties = vitriluutru.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{Common.Table_ViTriLuuTru}] ({columns}) OUTPUT INSERTED.{Common.VTID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Check gia tri truong thong tin mac dinh is exsting? 
        public bool DefaultThongTinViTriLuuTru_ValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_ViTriLuuTru}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Tính số lượng trống còn lại của vị trí
        public (int, string) GetViTriLuuTruSoLuongTrong(object? vtltID)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = $"SELECT SUM(CAST([{Common.SoLuong}] AS INT)) AS TongSoLuong FROM [{Common.Table_VitriOfNVL}] WHERE [{Common.VTID}] = '{vtltID}'";

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int tongSoLuong = Convert.ToInt32(result);

                        return (tongSoLuong, string.Empty);
                    }
                    else
                    {
                        return (0, "Null value");
                    }
                }
            }
            catch (SqlException ex)
            {
                return (-1, ex.Message);
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        // Get list vitriluutru ID
        public List<int> GetListVTriIds(object? mavitri)
        {
            List<int> listVitriIds = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.VTID}] FROM [{Common.Table_ViTriLuuTru}] WHERE [{Common.MaViTri}] = '{mavitri}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    _ = int.TryParse(reader[Common.VTID]?.ToString(), out int vitriID) ? vitriID : 0;

                    if (vitriID > 0)
                    {
                        listVitriIds.Add(vitriID);
                    }
                }
            }

            return listVitriIds;
        }

        // Lấy thông tin vị trí lưu trữ NVL
        public VitriLuuTru GetViTriLuuTruByID(object? vtltID)
        {
            VitriLuuTru vitri = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ViTriLuuTru}] WHERE [{Common.VTID}] = '{vtltID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                }
            }

            int suchua = int.TryParse(vitri.VTSucChua.Value?.ToString(), out int sc) ? sc : 0;
            // Get so luong con trong cua vi tri
            vitri.SLConTrong = suchua - GetViTriLuuTruSoLuongTrong(vitri.VTID.Value).Item1;

            return vitri;
        }

        // Get vị trí lưu trữ by mã vị trí
        public VitriLuuTru GetViTriLuuTruByMaVitri(object? mavitri)
        {
            VitriLuuTru vitri = new();

            if (mavitri == null) return vitri;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ViTriLuuTru}] WHERE [{Common.MaViTri}] = '{mavitri}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }
                }
            }

            int suchua = int.TryParse(vitri.VTSucChua.Value?.ToString(), out int sc) ? sc : 0;
            // Get so luong con trong cua vi tri
            vitri.SLConTrong = suchua - GetViTriLuuTruSoLuongTrong(vitri.VTID.Value).Item1;

            return vitri;
        }

        // Lấy danh sách tất cả vị trí lưu trữ
        public List<VitriLuuTru> GetAllViTriLuuTru()
        {
            List<VitriLuuTru> vitris = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ViTriLuuTru}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    VitriLuuTru vitri = new();

                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    int suchua = int.TryParse(vitri.VTSucChua.Value?.ToString(), out int sc) ? sc : 0;
                    // Get so luong con trong cua vi tri
                    vitri.SLConTrong = suchua - GetViTriLuuTruSoLuongTrong(vitri.VTID.Value).Item1;

                    vitris.Add(vitri);
                }
            }

            return vitris;
        }

        // Update thông tin vị trí lưu trữ
        public (int, string) UpdateViTriLuuTru(object? slnhapkho, object? vtid)
        {
            int result = -1; string errorMess = string.Empty;

            if (slnhapkho == null || vtid == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_ViTriLuuTru} SET [{Common.VTSLTrong}] = ([{Common.VTSLTrong}] - '{slnhapkho}') WHERE [{Common.VTID}] = '{vtid}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        // Delete
        public (bool, string) DeleteViTriLuuTru(object? vtid)
        {
            // Check for valid ID
            if (vtid == null)
            {
                return (false, "Can not delete!");
            }

            using var connection = new SqlConnection(connectionString);

            connection.Open();

            string query = $"DELETE FROM [{Common.Table_ViTriLuuTru}] WHERE [{Common.VTID}] = @VTID";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@VTID", vtid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();

                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_ViTriOfNVL

        // Lấy vitriofNVL by NVLid và VTid
        public ViTriofNVL GetViTriOfNgVatLieuByNVLid_VTid(object? nvlid, object? vtid)
        {
            ViTriofNVL vitriofnvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_VitriOfNVL}] WHERE [{Common.NVLID}] = '{nvlid}' AND [{Common.VTID}] = '{vtid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = vitriofnvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }
                }
            }

            // Get vitriluutru infor
            vitriofnvl.VitriInfor = GetViTriLuuTruByID(vitriofnvl.VTID.Value);
            vitriofnvl.NgLieuInfor = GetNguyenVatLieuByID_MultipleTask(vitriofnvl.NVLID.Value);

            return vitriofnvl;
        }
        public List<ViTriofNVL> GetViTriOfNgVatLieuByAnyParameters(object? vtid = null, object? nvlid = null, object? qridlot = null, object? vtofnvlid = null)
        {
            List<ViTriofNVL> vitriofnvls = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var conditions = new List<string>();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_VitriOfNVL}]";

                // Helper function to add condition if value is not null
                void AddCondition(object? value, string fieldName, string paramName)
                {
                    if (value != null)
                    {
                        conditions.Add($"[{fieldName}] = @{paramName}");

                        command.Parameters.AddWithValue($"@{paramName}", value);
                    }
                }

                // Add all potential conditions
                AddCondition(vtid, Common.VTID, "VTID");
                AddCondition(vtofnvlid, Common.VTofNVLID, "VTofNVLID");
                AddCondition(nvlid, Common.NVLID, "NVLID");
                AddCondition(qridlot, Common.QRIDLOT, "QRIDLOT");
                // Add more conditions as needed...

                // Combine all conditions with AND
                if (conditions.Any())
                {
                    command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                }

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ViTriofNVL vitriofnvl = new();

                    List<Propertyy> rowitems = vitriofnvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    if (vitriofnvl.VTofNVLID.Value != null)
                    {
                        // Get vitriluutru infor
                        vitriofnvl.VitriInfor = GetViTriLuuTruByID(vitriofnvl.VTID.Value);
                        vitriofnvl.NgLieuInfor = GetNguyenVatLieuByID_MultipleTask(vitriofnvl.NVLID.Value);
                    }

                    vitriofnvls.Add(vitriofnvl);
                }
            }

            return vitriofnvls;
        }
        public ViTriofNVL GetViTriOfNgVatLieuBy_VTid_LotVitri(object? vtid = null, object? nvlid = null, object? lotvitri = null)
        {
            ViTriofNVL vitriofnvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var conditions = new List<string>();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_VitriOfNVL}]";

                // Helper function to add condition if value is not null
                void AddCondition(object? value, string fieldName, string paramName)
                {
                    if (value != null)
                    {
                        conditions.Add($"[{fieldName}] = @{paramName}");

                        command.Parameters.AddWithValue($"@{paramName}", value);
                    }
                }

                // Add all potential conditions
                AddCondition(vtid, Common.VTID, "VTID");
                AddCondition(nvlid, Common.NVLID, "NVLID");
                AddCondition(lotvitri, Common.LotViTri, "LOTVITRI");
                // Add more conditions as needed...

                // Combine all conditions with AND
                if (conditions.Any())
                {
                    command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                }

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = vitriofnvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }
                }
            }

            if (vitriofnvl.VTofNVLID.Value != null)
            {
                // Get vitriluutru infor
                vitriofnvl.VitriInfor = GetViTriLuuTruByID(vitriofnvl.VTID.Value);
                vitriofnvl.NgLieuInfor = GetNguyenVatLieuByID_MultipleTask(vitriofnvl.NVLID.Value);
            }

            return vitriofnvl;
        }

        // Lấy danh sách vị trí của nguyên vật liệu by ID
        public List<ViTriofNVL> GetListViTriOfNgVatLieuByNVLid(object? nvlID)
        {
            List<ViTriofNVL> vitriOfnvls = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_VitriOfNVL}] WHERE [{Common.NVLID}] = '{nvlID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ViTriofNVL vitri = new();

                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Get vitriluutru infor
                    vitri.VitriInfor = GetViTriLuuTruByID(vitri.VTID.Value);

                    vitriOfnvls.Add(vitri);
                }
            }

            return vitriOfnvls;
        }

        public List<ViTriofNVL> GetListViTriOfNgVatLieuByVTid(object? vtID)
        {
            List<ViTriofNVL> vitriOfnvls = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_VitriOfNVL}] WHERE [{Common.VTID}] = '{vtID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ViTriofNVL vitri = new();

                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Get vitriluutru infor
                    vitri.VitriInfor = GetViTriLuuTruByID(vitri.VTID.Value);

                    vitriOfnvls.Add(vitri);
                }
            }

            return vitriOfnvls;
        }

        // Insert new ViTriofNVL
        public (int, string) InsertNewViTriOfNgVatLieu(ViTriofNVL newVTofNVL)
        {
            List<Propertyy> newItems = newVTofNVL.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_VitriOfNVL}] ({columnNames}) OUTPUT INSERTED.{Common.VTofNVLID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update so luong ViTriofNVL
        public (int, string) UpdateViTriOfNgVatLieu(ViTriofNVL vitriofNVL)
        {
            int result = -1; string errorMess = string.Empty;

            if (vitriofNVL == null) return (result, errorMess);

            List<Propertyy> Items = vitriofNVL.GetPropertiesValues().Where(pro => pro.AlowDatabase == true && pro.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_VitriOfNVL}] SET {setClause} WHERE [{Common.VTofNVLID}] = '{vitriofNVL.VTofNVLID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();

                if (result <= 0)
                {
                    return (-1, errorMess);
                }
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update so luong nguyen lieu by ID
        public (int, string) UpdateSoluongNgVatLieuById(object? vitriofnvlId, object? tonkho)
        {
            int result = -1; string errorMess = string.Empty;

            if (vitriofnvlId == null || tonkho == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_VitriOfNVL} SET [{Common.SoLuong}] = '{tonkho}' WHERE [{Common.VTofNVLID}] = '{vitriofnvlId}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        // Remove VitriofNVL
        public (int, string) DeleteViTriOfNgVatLieu(object? vtofnvllid)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_VitriOfNVL} WHERE [{Common.VTofNVLID}] = '{vtofnvllid}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }
            return (result, errorMess);
        }


        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table KHO_DanhMucNguyenVatLieu
        // Get list danh muc NVL
        public List<DanhMucNVL> GetListDanhMucNVLs()
        {
            List<DanhMucNVL> listDanhmucNVLs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_DanhMucNVL}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DanhMucNVL danhmuc = new();

                    List<Propertyy> rowitems = danhmuc.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listDanhmucNVLs.Add(danhmuc);
                }
            }

            return listDanhmucNVLs;
        }
        // Get danh muc by ID
        public DanhMucNVL GetDanhMucbyID(object? danhmucID)
        {
            DanhMucNVL dmnvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_DanhMucNVL}] WHERE [{Common.DMID}] = '{danhmucID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = dmnvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                }
            }

            return dmnvl;
        }
        // Them new danh muc NVL // Table KHO_DanhMucNguyenVatLieu //
        public (int, string) InsertNewDanhMucNguyenVatLieu(DanhMucNVL newdmnvl)
        {
            List<Propertyy> newItems = newdmnvl.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_DanhMucNVL}] ({columnNames}) OUTPUT INSERTED.{Common.DMID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Check ten danh muc da ton tai // Table KHO_DanhMucNguyenVatLieu //
        public bool IsTenDanhmucNVLExists(string? tendmnvl)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_DanhMucNVL}] WHERE [{Common.TenDanhMuc}] = N'{tendmnvl}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table KHO_LoaiNguyenVatLieu

        // Get loai NVL by maloai NVL // Table KHO_LoaiNguyenVatLieu //
        public LoaiNVL GetLoaiNVLbyID(object? maloaiNVL)
        {
            LoaiNVL loainvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiNVL}] WHERE [{Common.LOAINVLID}] = '{maloaiNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = loainvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                }
            }

            return loainvl;
        }

        // Get list loai nguyen vat lieu (get all) // Table KHO_LoaiNguyenVatLieu //
        public List<LoaiNVL> GetListLoaiNVLs()
        {
            List<LoaiNVL> listloaiNVLs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiNVL}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiNVL loaiNVL = new();

                    List<Propertyy> rowitems = loaiNVL.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listloaiNVLs.Add(loaiNVL);
                }
            }

            return listloaiNVLs;
        }

        // Get list loai nguyen vat lieu (order by madanhmuc) // Table KHO_LoaiNguyenVatLieu //
        public List<LoaiNVL> GetListLoaiNVLs(object? dmid)
        {
            List<LoaiNVL> listloaiNVLs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiNVL}] WHERE [{Common.DMID}] = '{dmid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiNVL loaiNVL = new();

                    List<Propertyy> rowitems = loaiNVL.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listloaiNVLs.Add(loaiNVL);
                }
            }

            return listloaiNVLs;
        }

        // Check ten loai nvl da ton tai // Table KHO_LoaiNguyenVatLieu //
        public bool IsTenLoaiNVLExists(string? tenloainvl)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_LoaiNVL}] WHERE [{Common.TenLoaiNVL}] = N'{tenloainvl}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Them new loai NVL // Table KHO_LoaiNguyenVatLieu //
        public (int, string) InsertNewLoaiNguyenVatLieu(LoaiNVL newloainvl)
        {
            List<Propertyy> newItems = newloainvl.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LoaiNVL}] ({columnNames}) OUTPUT INSERTED.{Common.LOAINVLID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete Loai NVL // Table KHO_LoaiNguyenVatLieu //
        public (int, string) DeleteLoaiNVL(LoaiNVL? removeloainvl)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeloainvl == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_LoaiNVL} WHERE [{Common.LOAINVLID}] = '{removeloainvl.LOAINVLID.Value}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_PhieuNhapKho

        // Thêm mới phiếu nhập kho nguyên vật liệu
        public (int, string) InsertNewPhieuNhapKho(PhieuNhapKho? newpnk)
        {
            int result = -1; string errorMess = string.Empty;

            if (newpnk == null) return (result, "Error");

            List<Propertyy> pnkItems = newpnk.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", pnkItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", pnkItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_PhieuNhapKho}] ({columnNames}) OUTPUT INSERTED.{Common.PNKID} VALUES ({parameterNames})";

                foreach (var item in pnkItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Xóa phiếu nhập kho by ID
        public (int, string) DeletePhieuNhapKho(object? pnkID)
        {
            int result = -1; string errorMess = string.Empty;

            if (pnkID == null) return (-1, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_PhieuNhapKho} WHERE [{Common.PNKID}] = '{pnkID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Load Phieu nhap kho by ID
        public PhieuNhapKho GetPhieuNhapKhoByID(object? pnkid)
        {
            PhieuNhapKho phieunhapkho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuNhapKho}] WHERE [{Common.PNKID}] = '{pnkid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = phieunhapkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                }
            }

            // Load danh sach nguyen vat lieu pnk
            phieunhapkho.DSNVLofPNKs = GetListNVLofPNKs(pnkid);

            phieunhapkho.isPNKDoneNhapKho = (int.TryParse(phieunhapkho.IsDonePNK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

            return phieunhapkho;
        }

        // Get nguoi nhap kho by pnkid ID
        public string GetNguoiTaoPhieuNhapKhoByID(object? pnkid)
        {
            string nguoilapphieu = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.NguoiLapPNK}] FROM [{Common.Table_PhieuNhapKho}] WHERE [{Common.PNKID}] = @PNKid";
                command.Parameters.AddWithValue("@PNKid", pnkid ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    nguoilapphieu = reader[Common.NguoiLapPNK].ToString()?.Trim() ?? string.Empty;
                }
            }
            return nguoilapphieu;
        }

        // Get ma phieu nhap kho by ID
        public string GetMaPhieuNhapKhoByID(object? pnkid)
        {
            string maPhieuNhapKho = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.MaPhieuNhapKho}] FROM [{Common.Table_PhieuNhapKho}] WHERE [{Common.PNKID}] = @PNKid";
                command.Parameters.AddWithValue("@PNKid", pnkid ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    maPhieuNhapKho = reader[Common.MaPhieuNhapKho].ToString()?.Trim() ?? string.Empty;
                }
            }
            return maPhieuNhapKho;
        }

        // Get phiếu nhập kho by Mã phiếu 
        public PhieuNhapKho GetPhieuNhapKhoByMaPhieu(object? maPNK)
        {
            PhieuNhapKho phieunhapkho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuNhapKho}] WHERE [{Common.MaPhieuNhapKho}] = '{maPNK}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = phieunhapkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                }
            }

            // Load danh sach nguyen vat lieu pnk
            phieunhapkho.DSNVLofPNKs = GetListNVLofPNKs(phieunhapkho.PNKID.Value);

            phieunhapkho.isPNKDoneNhapKho = (int.TryParse(phieunhapkho.IsDonePNK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

            return phieunhapkho;
        }

        // Get list phieu nhap kho
        public List<PhieuNhapKho> GetListPhieuNhapKho()
        {
            List<PhieuNhapKho> dsPNKho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuNhapKho}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PhieuNhapKho phieunhapkho = new();

                    List<Propertyy> rowitems = phieunhapkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Load danh sach nguyen vat lieu pnk
                    phieunhapkho.DSNVLofPNKs = GetListNVLofPNKs(phieunhapkho.PNKID.Value);

                    phieunhapkho.isPNKDoneNhapKho = (int.TryParse(phieunhapkho.IsDonePNK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

                    // Check PNK isdone
                    //foreach (var nvlofpnk in phieunhapkho.DSNVLofPNKs)
                    //{
                    //    if (!nvlofpnk.IsNhapKhoDone)
                    //    {
                    //        phieunhapkho.IsPXKDoneNhapKho = false; break;
                    //    }
                    //    else if (nvlofpnk.IsNhapKhoDone)
                    //    {
                    //        phieunhapkho.IsPXKDoneNhapKho = true;
                    //    }
                    //}

                    dsPNKho.Add(phieunhapkho);
                }
            }

            return dsPNKho;
        }

        // Get phieu nhap kho ID by maPNK
        public List<int> GetListPNKIds(object? maPNK)
        {
            List<int> listPNKids = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.PNKID}] FROM [{Common.Table_PhieuNhapKho}] WHERE [{Common.MaPhieuNhapKho}] = '{maPNK}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {

                    _ = int.TryParse(reader[Common.PNKID]?.ToString(), out int pnkid) ? pnkid : 0;

                    if (pnkid > 0)
                    {
                        listPNKids.Add(pnkid);
                    }
                }
            }

            return listPNKids;
        }

        // Get any columns of PNK by any paramaters
        public (List<object?> columnValues, string errorMessage) GetPNK_AnyColValuebyAnyParameters(Dictionary<string, object?> parameters, string? returnColumnName = null, bool isGetAll = false)
        {
            List<object?> columnValues = new();
            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var conditions = new List<string>();
                    var command = connection.CreateCommand();

                    // If a specific column is requested, select only that column
                    string selectClause = returnColumnName != null
                        ? $"SELECT [{returnColumnName}]"
                        : "SELECT *";

                    command.CommandText = $"{selectClause} FROM [{Common.Table_PhieuNhapKho}]";

                    if (!isGetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }
                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // If no specific column is requested, read entire object
                        if (returnColumnName == null)
                        {
                            PhieuNhapKho phieunhapkho = new();
                            List<Propertyy> rowItems = phieunhapkho.GetPropertiesValues();
                            foreach (var item in rowItems)
                            {
                                string? columnName = item.DBName;
                                if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                                {
                                    object columnValue = reader[columnName];
                                    item.Value = columnValue == DBNull.Value ? null : columnValue;
                                }
                                columnValues.Add(phieunhapkho);
                            }
                        }
                        else
                        {
                            // If a specific column is requested, read only that column
                            object columnValue = reader[returnColumnName];
                            columnValues.Add(columnValue == DBNull.Value ? null : columnValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    columnValues.Clear(); // Clear the list in case of error
                }
            }
            return (columnValues, errorMessage);
        }

        // Update thong tin Phieu nhap kho
        public (int, string) UpdatePhieuNhapKhoInfor(PhieuNhapKho phieuNK)
        {
            int result = -1; string errorMess = string.Empty;

            if (phieuNK == null) return (result, errorMess);

            List<Propertyy> Items = phieuNK.GetPropertiesValues().Where(pro => pro.AlowDatabase == true && pro.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_PhieuNhapKho}] SET {setClause} WHERE [{Common.PNKID}] = '{phieuNK.PNKID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_NVLofPhieuNhapKho

        // Thêm NVL của phiếu nhập kho
        public (int, string) InsertNVLofPhieuNhapKho(NVLofPhieuNhapKho? newnvlpnk)
        {
            int result = -1; string errorMess = string.Empty;

            if (newnvlpnk == null) return (result, "Error");

            List<Propertyy> nvlpnkItems = newnvlpnk.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", nvlpnkItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", nvlpnkItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_NVLofPhieuNhapKho}] ({columnNames}) OUTPUT INSERTED.{Common.NVLPNKID} VALUES ({parameterNames})";

                foreach (var item in nvlpnkItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Remove nvlof Phieu nhap kho
        public (int, string) DeleteNVLofPhieuNhapKho(object? nvlpnkid)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_NVLofPhieuNhapKho} WHERE [{Common.NVLPNKID}] = '{nvlpnkid}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Load danh sach NVLofPNK by PNKid
        public List<NVLofPhieuNhapKho> GetListNVLofPNKs(object? pnkId)
        {
            List<NVLofPhieuNhapKho> listNvlofPnk = new();

            if (pnkId == null) return listNvlofPnk;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NVLofPhieuNhapKho}] WHERE [{Common.PNKID}] = '{pnkId}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLofPhieuNhapKho nvlpnk = new();

                    List<Propertyy> rowitems = nvlpnk.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Get danh sach lenh nhap kho
                    nvlpnk.DSLenhNKs = GetDSLenhNhapKho(nvlpnk.NVLPNKID.Value, pnkId);

                    // Check nhap kho status of NVLofPNK
                    foreach (var lenh in nvlpnk.DSLenhNKs)
                    {
                        int islenhdone = int.TryParse(lenh.LNKIsDone.Value?.ToString(), out int isd) ? isd : -1;
                        if (islenhdone == 0)
                        {
                            nvlpnk.IsNhapKhoDone = false; break;
                        }
                        else if (islenhdone == 1) { nvlpnk.IsNhapKhoDone = true; }
                    }
                    // Load target Nguyenvatlieu
                    nvlpnk.TargetNgLieu = GetNguyenVatLieuByID_MultipleTask(nvlpnk.NVLID.Value);

                    listNvlofPnk.Add(nvlpnk);
                }
            }

            return listNvlofPnk;
        }

        // Update NVLofPXK
        public (int, string) UpdateNVLofPNK(NVLofPhieuNhapKho nvlofpnk)
        {
            int result = -1; string errorMess = string.Empty;

            if (nvlofpnk == null) return (result, errorMess);

            List<Propertyy> Items = nvlofpnk.GetPropertiesValues().Where(pro => pro.AlowDatabase == true && pro.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_NVLofPhieuNhapKho}] SET {setClause} WHERE [{Common.NVLPNKID}] = '{nvlofpnk.NVLPNKID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_LenhNhapKho

        // Get lệnh nhập kho by LNKID
        public LenhNhapKho GetLenhNhapKhoByID(object? lnkid)
        {
            LenhNhapKho lnkho = new();

            if (lnkid == null) return lnkho;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhNhapKho}] WHERE [{Common.LenhNKID}] = '{lnkid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = lnkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }
            }

            // Get vi tri luu tru
            lnkho.TargertVitri = GetViTriLuuTruByID(lnkho.VTID.Value);

            // Get ng vat lieu nhap kho
            lnkho.TargetNgLieu = GetNguyenVatLieuByID_MultipleTask(lnkho.NVLID.Value);

            return lnkho;
        }

        public LenhNhapKho GetLenhNhapKhoByQRIDLOT(object? qridlot)
        {
            LenhNhapKho lnkho = new();

            if (qridlot == null) return lnkho;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhNhapKho}] WHERE [{Common.QRIDLOT}] = '{qridlot}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = lnkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }
            }

            // Get vi tri luu tru
            lnkho.TargertVitri = GetViTriLuuTruByID(lnkho.VTID.Value);

            // Get ng vat lieu nhap kho
            lnkho.TargetNgLieu = GetNguyenVatLieuByID_MultipleTask(lnkho.NVLID.Value);

            return lnkho;
        }


        public LenhNhapKho GetLenhNhapKho(LenhNhapKho inputLNK)
        {
            LenhNhapKho lnkho = new();

            if (inputLNK == null) return lnkho;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhNhapKho}] WHERE [{Common.PNKID}] = '{inputLNK.PNKID.Value}' AND [{Common.NVLID}] = '{inputLNK.NVLID.Value}' AND [{Common.VTID}] = '{inputLNK.VTID.Value}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = lnkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }
            }

            return lnkho;
        }


        // Get danh sách lệnh nhập kho by NVLPNKID and PNKID
        public List<LenhNhapKho> GetDSLenhNhapKho(object? nvlpnkId, object? pnkId)
        {
            List<LenhNhapKho> lenhNKhos = new();

            if (nvlpnkId == null || pnkId == null) return lenhNKhos;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhNhapKho}] WHERE [{Common.NVLPNKID}] = '{nvlpnkId}' AND [{Common.PNKID}] = '{pnkId}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LenhNhapKho lenhnk = new();

                    List<Propertyy> rowitems = lenhnk.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Get vitriluutru infor
                    lenhnk.TargertVitri = GetViTriLuuTruByID(lenhnk.VTID.Value);

                    // Get ng vat lieu nhap kho
                    lenhnk.TargetNgLieu = GetNguyenVatLieuByID_MultipleTask(lenhnk.NVLID.Value);

                    lenhNKhos.Add(lenhnk);
                }
            }

            return lenhNKhos;
        }

        // Thêm lệnh nhập kho
        public (int, string) InsertLenhNhapKho(LenhNhapKho? lenhnk)
        {
            int result = -1; string errorMess = string.Empty;

            if (lenhnk == null) return (result, "Error");

            List<Propertyy> lnkItems = lenhnk.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", lnkItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", lnkItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LenhNhapKho}] ({columnNames}) OUTPUT INSERTED.{Common.LenhNKID} VALUES ({parameterNames})";

                foreach (var item in lnkItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Xóa lệnh nhập kho
        public (int, string) DeleteLenhNhapKho(object? lenhnkId)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_LenhNhapKho} WHERE [{Common.LenhNKID}] = '{lenhnkId}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Update lệnh nhập kho
        public (int, string) UpdateLenhNhapKho(LenhNhapKho lenhnhapkho)
        {
            int result = -1; string errorMess = string.Empty;

            if (lenhnhapkho == null) return (result, errorMess);

            List<Propertyy> Items = lenhnhapkho.GetPropertiesValues().Where(pro => pro.AlowDatabase == true && pro.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_LenhNhapKho}] SET {setClause} WHERE [{Common.LenhNKID}] = '{lenhnhapkho.LenhNKID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        public (int, string) UpdateLenhNhapKhoStatus(object? lenhnkId, object lnkstatus)
        {
            int result = -1; string errorMess = string.Empty;

            if (lenhnkId == null || lnkstatus == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_LenhNhapKho} SET [{Common.LNKIsDone}] = '{lnkstatus}' WHERE [{Common.LenhNKID}] = '{lenhnkId}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_PhieuXuatKho
        // Thêm mới phiếu xuất kho nguyên vật liệu
        public (int, string) InsertNewPhieuXuatKho(PhieuXuatKho? newpxk)
        {
            int result = -1; string errorMess = string.Empty;

            if (newpxk == null) return (result, "Error");

            List<Propertyy> pxkItems = newpxk.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", pxkItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", pxkItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_PhieuXuatKho}] ({columnNames}) OUTPUT INSERTED.{Common.PXKID} VALUES ({parameterNames})";

                foreach (var item in pxkItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }
            return (result, errorMess);
        }

        // Xóa phiếu xuất kho by ID
        public (int, string) DeletePhieuXuatKho(object? pxkID)
        {
            int result = -1; string errorMess = string.Empty;

            if (pxkID == null) return (-1, errorMess);
            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_PhieuXuatKho} WHERE [{Common.PXKID}] = '{pxkID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                return (result, errorMess);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

        }

        public (List<PhieuXuatKho> phieuxuatkhos, string error) GetListPhieuXuatKhos(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<PhieuXuatKho> listPhieuXuatKhos = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{Common.Table_PhieuXuatKho}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PhieuXuatKho phieuxuatkho = new();

                        List<Propertyy> rowItems = phieuxuatkho.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        if (phieuxuatkho.PXKID.Value != null)
                        {
                            // Load danh sach nguyen vat lieu pxk
                            phieuxuatkho.DSNVLofPXKs = GetListNVLofPXKs(phieuxuatkho.PXKID.Value);

                            // Kiem tra phieu xuat kho da chi dinh du NVL chua
                            phieuxuatkho.isChiDinhDuSLXuatKho = !(phieuxuatkho.DSNVLofPXKs.Any(nvlofpxk => IsChidinhDuSoLuongXuatKho(nvlofpxk) == false));

                            // Check PXK isdone
                            phieuxuatkho.isPXKDoneXuatKho = (int.TryParse(phieuxuatkho.IsDonePXK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

                            // Check IsPhieuBoSungNVL
                            phieuxuatkho.isPhieuBoSungNVL = (int.TryParse(phieuxuatkho.IsPhieuBoSungNVL.Value?.ToString(), out int isbsnvl) ? isbsnvl : 0) == 1;

                            // Check IsPhieuBSungAddedLOTNVL
                            phieuxuatkho.isPhieuBSungAddedLOTNVL = (int.TryParse(phieuxuatkho.IsPhieuBSungAddedLOTNVL.Value?.ToString(), out int isaddedlot) ? isaddedlot : 0) == 1;

                            // Kiem tra trang thai tra NVL neu phieu tra kho da duoc tao
                            _ = int.TryParse(phieuxuatkho.PNKID.Value?.ToString(), out int pnkid_trakho);
                            _ = int.TryParse(phieuxuatkho.KHSXID.Value?.ToString(), out int khsxid_trakho);

                            if (pnkid_trakho > 0 && khsxid_trakho > 0)
                            {
                                PhieuNhapKho phieuNhapKho = GetPhieuNhapKhoByID(phieuxuatkho.PNKID.Value);

                                phieuxuatkho.PhieuTraKho = phieuNhapKho;

                                phieuxuatkho.maPNKreturnNVL = phieuNhapKho.MaPhieuNK.Value?.ToString() ?? string.Empty;

                                phieuxuatkho.isReturnedNVL = phieuNhapKho.isPNKDoneNhapKho;
                            }
                        }

                        listPhieuXuatKhos.Add(phieuxuatkho);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listPhieuXuatKhos.Clear(); // Clear the list in case of error
                }
            }
            return (listPhieuXuatKhos, errorMessage);
        }


        // Get phiếu xuất kho by ID
        public PhieuXuatKho GetPhieuXuatKhoByID(object? pxkid)
        {
            PhieuXuatKho phieuxuatkho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuXuatKho}] WHERE [{Common.PXKID}] = '{pxkid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = phieuxuatkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }
                }
            }

            // Load danh sach nguyen vat lieu pxk
            phieuxuatkho.DSNVLofPXKs = GetListNVLofPXKs(pxkid);

            // Kiem tra phieu xuat kho da chi dinh du NVL chua
            phieuxuatkho.isChiDinhDuSLXuatKho = !(phieuxuatkho.DSNVLofPXKs.Any(nvlofpxk => IsChidinhDuSoLuongXuatKho(nvlofpxk) == false));

            // Check PXK isdone
            phieuxuatkho.isPXKDoneXuatKho = (int.TryParse(phieuxuatkho.IsDonePXK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

            // Check IsPhieuBoSungNVL
            phieuxuatkho.isPhieuBoSungNVL = (int.TryParse(phieuxuatkho.IsPhieuBoSungNVL.Value?.ToString(), out int isbsnvl) ? isbsnvl : 0) == 1;

            // Check IsPhieuBSungAddedLOTNVL
            phieuxuatkho.isPhieuBSungAddedLOTNVL = (int.TryParse(phieuxuatkho.IsPhieuBSungAddedLOTNVL.Value?.ToString(), out int isaddedlot) ? isaddedlot : 0) == 1;


            // Kiem tra trang thai tra NVL neu phieu tra kho da duoc tao
            _ = int.TryParse(phieuxuatkho.PNKID.Value?.ToString(), out int pnkid_trakho);
            _ = int.TryParse(phieuxuatkho.KHSXID.Value?.ToString(), out int khsxid_trakho);

            if (pnkid_trakho > 0 && khsxid_trakho > 0)
            {
                PhieuNhapKho phieuNhapKho = GetPhieuNhapKhoByID(phieuxuatkho.PNKID.Value);

                phieuxuatkho.PhieuTraKho = phieuNhapKho;

                phieuxuatkho.maPNKreturnNVL = phieuNhapKho.MaPhieuNK.Value?.ToString() ?? string.Empty;

                phieuxuatkho.isReturnedNVL = phieuNhapKho.isPNKDoneNhapKho;
            }

            return phieuxuatkho;
        }

        // Get phiếu xuất kho by Mã phiếu 
        public PhieuXuatKho GetPhieuXuatKhoByMaPhieu(object? maPXK)
        {
            PhieuXuatKho phieuxuatkho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuXuatKho}] WHERE [{Common.MaPhieuXuatKho}] = '{maPXK}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = phieuxuatkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }
                }
            }
            // Load danh sach nguyen vat lieu pxk
            phieuxuatkho.DSNVLofPXKs = GetListNVLofPXKs(phieuxuatkho.PXKID.Value);

            // Kiem tra phieu xuat kho da chi dinh du NVL chua
            phieuxuatkho.isChiDinhDuSLXuatKho = !(phieuxuatkho.DSNVLofPXKs.Any(nvlofpxk => IsChidinhDuSoLuongXuatKho(nvlofpxk) == false));

            // Check PXK isdone
            //phieuxuatkho.isPXKDoneXuatKho = !(phieuxuatkho.DSNVLofPXKs.Any(nvlofpxk => nvlofpxk.IsXuatKhoDone == false));

            phieuxuatkho.isPXKDoneXuatKho = (int.TryParse(phieuxuatkho.IsDonePXK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

            // Check IsPhieuBoSungNVL
            phieuxuatkho.isPhieuBoSungNVL = (int.TryParse(phieuxuatkho.IsPhieuBoSungNVL.Value?.ToString(), out int isbsnvl) ? isbsnvl : 0) == 1;

            // Check IsPhieuBSungAddedLOTNVL
            phieuxuatkho.isPhieuBSungAddedLOTNVL = (int.TryParse(phieuxuatkho.IsPhieuBSungAddedLOTNVL.Value?.ToString(), out int isaddedlot) ? isaddedlot : 0) == 1;

            // Kiem tra trang thai tra NVL neu phieu tra kho da duoc tao
            _ = int.TryParse(phieuxuatkho.PNKID.Value?.ToString(), out int pnkid_trakho);
            _ = int.TryParse(phieuxuatkho.KHSXID.Value?.ToString(), out int khsxid_trakho);

            if (pnkid_trakho > 0 && khsxid_trakho > 0)
            {
                PhieuNhapKho phieuNhapKho = GetPhieuNhapKhoByID(phieuxuatkho.PNKID.Value);

                phieuxuatkho.PhieuTraKho = phieuNhapKho;

                phieuxuatkho.maPNKreturnNVL = phieuNhapKho.MaPhieuNK.Value?.ToString() ?? string.Empty;

                phieuxuatkho.isReturnedNVL = phieuNhapKho.isPNKDoneNhapKho;
            }



            return phieuxuatkho;
        }

        // Load danh sach phieu xuat kho
        public List<PhieuXuatKho> GetListPhieuXuatKho()
        {
            List<PhieuXuatKho> dsPXKho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuXuatKho}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PhieuXuatKho phieuxuatkho = new();

                    List<Propertyy> rowitems = phieuxuatkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Load danh sach nguyen vat lieu pxk
                    phieuxuatkho.DSNVLofPXKs = GetListNVLofPXKs(phieuxuatkho.PXKID.Value);

                    // Kiem tra phieu xuat kho da chi dinh du NVL chua
                    phieuxuatkho.isChiDinhDuSLXuatKho = !(phieuxuatkho.DSNVLofPXKs.Any(nvlofpxk => IsChidinhDuSoLuongXuatKho(nvlofpxk) == false));

                    // Check PXK isdone
                    //phieuxuatkho.isPXKDoneXuatKho = !(phieuxuatkho.DSNVLofPXKs.Any(nvlofpxk => nvlofpxk.IsXuatKhoDone == false));

                    phieuxuatkho.isPXKDoneXuatKho = (int.TryParse(phieuxuatkho.IsDonePXK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

                    // Check IsPhieuBoSungNVL
                    phieuxuatkho.isPhieuBoSungNVL = (int.TryParse(phieuxuatkho.IsPhieuBoSungNVL.Value?.ToString(), out int isbsnvl) ? isbsnvl : 0) == 1;

                    // Check IsPhieuBSungAddedLOTNVL
                    phieuxuatkho.isPhieuBSungAddedLOTNVL = (int.TryParse(phieuxuatkho.IsPhieuBSungAddedLOTNVL.Value?.ToString(), out int isaddedlot) ? isaddedlot : 0) == 1;


                    // Kiem tra trang thai tra NVL neu phieu tra kho da duoc tao
                    _ = int.TryParse(phieuxuatkho.PNKID.Value?.ToString(), out int pnkid_trakho);
                    _ = int.TryParse(phieuxuatkho.KHSXID.Value?.ToString(), out int khsxid_trakho);

                    if (pnkid_trakho > 0 && khsxid_trakho > 0)
                    {
                        PhieuNhapKho phieuNhapKho = GetPhieuNhapKhoByID(phieuxuatkho.PNKID.Value);

                        phieuxuatkho.PhieuTraKho = phieuNhapKho;

                        phieuxuatkho.maPNKreturnNVL = phieuNhapKho.MaPhieuNK.Value?.ToString() ?? string.Empty;

                        phieuxuatkho.isReturnedNVL = phieuNhapKho.isPNKDoneNhapKho;
                    }



                    dsPXKho.Add(phieuxuatkho);
                }
            }

            return dsPXKho;
        }

        // Kiem tra tong so luong chi dinh bang tong so luong can lay
        private bool IsChidinhDuSoLuongXuatKho(NVLofPhieuXuatKho nvlofpxk)
        {
            bool isSlgAsigned = false;

            int sumsoluongtake = nvlofpxk.DSLenhXKs.Sum(lenh => int.TryParse(lenh.LXKSoLuong.Value?.ToString(), out int slt) ? slt : 0);

            int slallNVLxuatkho = int.TryParse(nvlofpxk.NVLXKSoLuongAll.Value?.ToString(), out int slall) ? slall : 0;

            if (slallNVLxuatkho > 0 && sumsoluongtake == slallNVLxuatkho)
            {
                isSlgAsigned = true;
            }

            return isSlgAsigned;
        }

        // Get ma phieu xuat kho by ID
        public string GetMaPhieuXuatKhoByID(object? pxkid)
        {
            string maPhieuXuatKho = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.MaPhieuXuatKho}] FROM [{Common.Table_PhieuXuatKho}] WHERE [{Common.PXKID}] = @PXKid";
                command.Parameters.AddWithValue("@PXKid", pxkid ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    maPhieuXuatKho = reader[Common.MaPhieuXuatKho].ToString()?.Trim() ?? string.Empty;
                }
            }
            return maPhieuXuatKho;
        }

        // Get nguoi xuat kho by pxkid ID
        public string GetNguoiTaoPhieuXuatKhoByID(object? pxkid)
        {
            string nguoilapphieu = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.NguoiLapPXK}] FROM [{Common.Table_PhieuXuatKho}] WHERE [{Common.PXKID}] = @PXKid";
                command.Parameters.AddWithValue("@PXKid", pxkid ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    nguoilapphieu = reader[Common.NguoiLapPXK].ToString()?.Trim() ?? string.Empty;
                }
            }
            return nguoilapphieu;
        }

        // Get phieu xuat kho ID by maPXK
        public List<int> GetListPXKIds(object? maPXK)
        {
            List<int> listPXKids = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{Common.PXKID}] FROM [{Common.Table_PhieuXuatKho}] WHERE [{Common.MaPhieuXuatKho}] = '{maPXK}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {

                    _ = int.TryParse(reader[Common.PXKID]?.ToString(), out int pxkid) ? pxkid : 0;

                    if (pxkid > 0)
                    {
                        listPXKids.Add(pxkid);
                    }
                }
            }

            return listPXKids;
        }

        // Get any columns of PXK by any paramaters
        public (List<object?> columnValues, string errorMessage) GetPXK_AnyColValuebyAnyParameters(Dictionary<string, object?> parameters, string? returnColumnName = null, bool isGetAll = false)
        {
            List<object?> columnValues = new();
            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var conditions = new List<string>();
                    var command = connection.CreateCommand();

                    // If a specific column is requested, select only that column
                    string selectClause = returnColumnName != null
                        ? $"SELECT [{returnColumnName}]"
                        : "SELECT *";

                    command.CommandText = $"{selectClause} FROM [{Common.Table_PhieuXuatKho}]";

                    if (!isGetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }
                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // If no specific column is requested, read entire object
                        if (returnColumnName == null)
                        {
                            PhieuXuatKho phieuxuatkho = new();
                            List<Propertyy> rowItems = phieuxuatkho.GetPropertiesValues();
                            foreach (var item in rowItems)
                            {
                                string? columnName = item.DBName;
                                if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                                {
                                    object columnValue = reader[columnName];
                                    item.Value = columnValue == DBNull.Value ? null : columnValue;
                                }
                                columnValues.Add(phieuxuatkho);
                            }
                        }
                        else
                        {
                            // If a specific column is requested, read only that column
                            object columnValue = reader[returnColumnName];
                            columnValues.Add(columnValue == DBNull.Value ? null : columnValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    columnValues.Clear(); // Clear the list in case of error
                }
            }
            return (columnValues, errorMessage);
        }


        // Update thong tin Phieu xuat kho
        public (int, string) UpdatePhieuXuatKhoInfor(PhieuXuatKho phieuXK)
        {
            int result = -1; string errorMess = string.Empty;

            if (phieuXK == null) return (result, errorMess);

            List<Propertyy> Items = phieuXK.GetPropertiesValues().Where(pro => pro.AlowDatabase == true && pro.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_PhieuXuatKho}] SET {setClause} WHERE [{Common.PXKID}] = '{phieuXK.PXKID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }
            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_NVLofPhieuXuatKho
        // Thêm NVL của phiếu xuất kho
        public (int, string) InsertNVLofPhieuXuatKho(NVLofPhieuXuatKho? newnvlpxk)
        {
            int result = -1; string errorMess = string.Empty;

            if (newnvlpxk == null) return (result, "Error");

            List<Propertyy> nvlpxkItems = newnvlpxk.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();
                var command = connection.CreateCommand();

                string columnNames = string.Join(",", nvlpxkItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", nvlpxkItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_NVLofPhieuXuatKho}] ({columnNames}) OUTPUT INSERTED.{Common.NVLPXKID} VALUES ({parameterNames})";

                foreach (var item in nvlpxkItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }
            return (result, errorMess);
        }

        // Remove nvlof Phieu xuat kho
        public (int, string) DeleteNVLofPhieuXuatKho(object? nvlpxkid)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_NVLofPhieuXuatKho} WHERE [{Common.NVLPXKID}] = '{nvlpxkid}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }
            return (result, errorMess);
        }

        // Load danh sach NVLofPXK by PXKid
        public List<NVLofPhieuXuatKho> GetListNVLofPXKs(object? pxkId)
        {
            List<NVLofPhieuXuatKho> listNvlofPxk = new();

            if (pxkId == null) return listNvlofPxk;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NVLofPhieuXuatKho}] WHERE [{Common.PXKID}] = '{pxkId}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLofPhieuXuatKho nvlpxk = new();

                    List<Propertyy> rowitems = nvlpxk.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Get danh sach lenh xuat kho
                    nvlpxk.DSLenhXKs = GetDSLenhXuatKho(nvlpxk.NVLPXKID.Value, pxkId);

                    // Check xuat kho status of NVLofPXK
                    foreach (var lenh in nvlpxk.DSLenhXKs)
                    {
                        int islenhdone = int.TryParse(lenh.LXKIsDone.Value?.ToString(), out int isd) ? isd : -1;

                        if (islenhdone == 0)
                        {
                            nvlpxk.IsXuatKhoDone = false; break;
                        }
                        else if (islenhdone == 1)
                        {
                            nvlpxk.IsXuatKhoDone = true;
                        }
                    }

                    // Load target Nguyenvatlieu
                    nvlpxk.TargetNgLieu = GetNguyenVatLieuByID_MultipleTask(nvlpxk.NVLID.Value);

                    listNvlofPxk.Add(nvlpxk);
                }
            }
            return listNvlofPxk;
        }

        // Update NVLofPXK
        public (int, string) UpdateNVLofPXK(NVLofPhieuXuatKho nvlofpxk)
        {
            int result = -1; string errorMess = string.Empty;

            if (nvlofpxk == null) return (result, errorMess);

            List<Propertyy> Items = nvlofpxk.GetPropertiesValues().Where(pro => pro.AlowDatabase == true && pro.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_NVLofPhieuXuatKho}] SET {setClause} WHERE [{Common.NVLPXKID}] = '{nvlofpxk.NVLPXKID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_LenhXuatKho

        // Kiem tra ton tai lenh xuat kho
        public LenhXuatKho GetLenhXuatKho(LenhXuatKho inputLXK)
        {
            LenhXuatKho lxkho = new();

            if (inputLXK == null) return lxkho;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhXuatKho}] WHERE [{Common.PXKID}] = '{inputLXK.PXKID.Value}' AND [{Common.NVLID}] = '{inputLXK.NVLID.Value}' AND [{Common.VTID}] = '{inputLXK.VTID.Value}' AND [{Common.LotViTri}] = '{inputLXK.LotVitri.Value}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = lxkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }
            }

            return lxkho;
        }

        public LenhXuatKho GetLenhXuatKhoByID(object? lxkID)
        {
            LenhXuatKho lxkho = new();

            if (lxkID == null) return lxkho;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhXuatKho}] WHERE [{Common.LenhXKID}] = '{lxkID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = lxkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }

                // Get vitriofNVL infor
                lxkho.ViTriofNVL = GetViTriOfNgVatLieuByNVLid_VTid(lxkho.NVLID.Value, lxkho.VTID.Value);

            }

            return lxkho;
        }

        // Get danh sách lệnh xuất kho by NVLPXKID and PXKID
        public List<LenhXuatKho> GetDSLenhXuatKho(object? nvlpxkId, object? pxkId)
        {
            List<LenhXuatKho> lenhXKhos = new();

            if (nvlpxkId == null || pxkId == null) return lenhXKhos;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhXuatKho}] WHERE [{Common.NVLPXKID}] = '{nvlpxkId}' AND [{Common.PXKID}] = '{pxkId}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LenhXuatKho lenhxk = new();

                    List<Propertyy> rowitems = lenhxk.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    // Get vitriluutru infor
                    //lenhxk.TagertVitri = GetViTriLuuTruByID(lenhxk.VTID.Value);

                    // Get vitriofNVL infor
                    lenhxk.ViTriofNVL = GetViTriOfNgVatLieuBy_VTid_LotVitri(vtid: lenhxk.VTID.Value, lotvitri: lenhxk.LotVitri.Value);

                    lenhXKhos.Add(lenhxk);
                }
            }

            return lenhXKhos;
        }

        // Thêm lệnh xuất kho
        public (int, string) InsertLenhXuatKho(LenhXuatKho? lenhxk)
        {
            int result = -1; string errorMess = string.Empty;

            if (lenhxk?.PXKID.Value == null) return (result, "Error");

            List<Propertyy> lxkItems = lenhxk.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", lxkItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", lxkItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LenhXuatKho}] ({columnNames}) OUTPUT INSERTED.{Common.LenhXKID} VALUES ({parameterNames})";

                foreach (var item in lxkItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Xóa lệnh xuất kho
        public (int, string) DeleteLenhXuatKho(object? lenhxkId)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_LenhXuatKho} WHERE [{Common.LenhXKID}] = '{lenhxkId}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update lệnh xuất kho
        public (int, string) UpdateLenhXuatKho(LenhXuatKho lenhxuatkho)
        {
            int result = -1; string errorMess = string.Empty;

            if (lenhxuatkho == null) return (result, errorMess);

            List<Propertyy> Items = lenhxuatkho.GetPropertiesValues().Where(pro => pro.AlowDatabase == true && pro.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_LenhXuatKho}] SET {setClause} WHERE [{Common.LenhXKID}] = '{lenhxuatkho.LenhXKID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update lenh xuat kho isdone status
        public (int, string) UpdateLenhXuatKhoStatus(object? lenhxkId, object lxkstatus)
        {
            int result = -1; string errorMess = string.Empty;

            if (lenhxkId == null || lxkstatus == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_LenhXuatKho} SET [{Common.LXKIsDone}] = '{lxkstatus}' WHERE [{Common.LenhXKID}] = '{lenhxkId}'";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_HistoryXuatNhapKho

        // Insert logging xuat-nhap kho
        public (int, string) InsertLogingXNKho(HistoryXNKho? logging)
        {
            int result = -1; string errorMess = string.Empty;

            if (logging == null) return (result, "Error");

            List<Propertyy> logginItems = logging.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", logginItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", logginItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_HistoryXNKho}] ({columnNames}) OUTPUT INSERTED.{Common.LogXNKID} VALUES ({parameterNames})";

                foreach (var item in logginItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get danh sach logging xuat-nhap kho
        public List<HistoryXNKho> GetDSLoggingXNKho()
        {
            List<HistoryXNKho> logs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_HistoryXNKho}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    HistoryXNKho log = new();

                    List<Propertyy> rowitems = log.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue == DBNull.Value ? null : columnValue;
                    }

                    logs.Add(log);
                }
            }

            return logs;
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_MAY_MayMoc
        // Get may moc by mmid
        public MayMoc GetMayMocbyID(object? mmID)
        {
            MayMoc mayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_MayMoc}] WHERE [{Common.MM_MMID}] = @MMID";

                command.Parameters.AddWithValue("@MMID", mmID);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = mayMoc.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            // Get danh sach thong tin may moc
            mayMoc.DSThongTin = GetDanhSachThongTinMayMoc(mayMoc.MMID.Value);

            return mayMoc;
        }

        // Get may moc by ID
        public string GetMaMayMocbyID(object? id)
        {
            string result = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.MM_MaMay}] FROM [{Common.Table_MayMoc}] WHERE [{Common.MM_MMID}] = @ID";
                command.Parameters.AddWithValue("@ID", id ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    result = reader[Common.MM_MaMay].ToString()?.Trim() ?? string.Empty;
                }
            }
            return result;
        }

        public MayMoc GetMayMocbyMaMay(string? mamay)
        {
            MayMoc mayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_MayMoc}] WHERE [{Common.MM_MaMay}] = @MAMAY";

                command.Parameters.AddWithValue("@MAMAY", mamay?.Trim());

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = mayMoc.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            if (mayMoc.MMID.Value != null)
            {
                // Get danh sach thong tin may moc
                mayMoc.DSThongTin = GetDanhSachThongTinMayMoc(mayMoc.MMID.Value);
            }

            return mayMoc;
        }

        // Check gia tri truong thong tin mac dinh may moc is exsting? 
        public bool DefaultThongTinMayMoc_ValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_MayMoc}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }
        // Insert new may moc
        public (int, string) InsertNewMayMoc(MayMoc? newmaymoc)
        {
            int result = -1; string errorMess = string.Empty;

            if (newmaymoc == null) return (result, "Error");

            List<Propertyy> newMMItems = newmaymoc.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newMMItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newMMItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_MayMoc}] ({columnNames}) OUTPUT INSERTED.{Common.MM_MMID} VALUES ({parameterNames})";

                foreach (var item in newMMItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Delete maymoc by mmid
        public (int, string) DeleteMayMoc(object? mmID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_MayMoc} WHERE [{Common.MM_MMID}] = '{mmID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Load danh sach maymoc by keySearch
        public List<MayMoc> GetDanhSachMayMoc(string column, object? keySearch)
        {
            List<MayMoc> danhSachMayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_MayMoc}] WHERE [{column}] LIKE @SearchTerm OR LOWER([{column}]) LIKE LOWER(@SearchTerm)";

                command.Parameters.AddWithValue("@SearchTerm", $"%{keySearch}%");

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MayMoc mayMoc = new();

                    List<Propertyy> rowItems = mayMoc.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Load danh sach thong tin may moc
                    mayMoc.DSThongTin = GetDanhSachThongTinMayMoc(mayMoc.MMID.Value);

                    danhSachMayMoc.Add(mayMoc);
                }
            }

            return danhSachMayMoc;
        }

        // Get all ds may moc
        public List<MayMoc> GetDanhSachMayMoc()
        {
            List<MayMoc> danhSachMayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_MayMoc}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MayMoc mayMoc = new();

                    List<Propertyy> rowItems = mayMoc.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Load danh sach thong tin may moc
                    mayMoc.DSThongTin = GetDanhSachThongTinMayMoc(mayMoc.MMID.Value);

                    danhSachMayMoc.Add(mayMoc);
                }
            }

            return danhSachMayMoc;
        }
        // Upadate truong thong tin may moc
        public (int, string) UpdateMayMocMainDetails(MayMoc mayMoc)
        {
            int result = -1; string errorMess = string.Empty;

            if (mayMoc == null) return (result, errorMess);

            List<Propertyy> Items = mayMoc.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_MayMoc}] SET {setClause} WHERE [{Common.MM_MMID}] = '{mayMoc.MMID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_MAY_ThongTinMayMoc

        // Get danh sach thong tin may moc by mmid
        public List<ThongTinMayMoc> GetDanhSachThongTinMayMoc(object? mmID)
        {
            List<ThongTinMayMoc> danhSachThongTinMayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ThongTinMayMoc}] WHERE [{Common.MM_MMID}] = @MMID";

                command.Parameters.AddWithValue("@MMID", mmID);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ThongTinMayMoc thongTinMayMoc = new();

                    List<Propertyy> rowItems = thongTinMayMoc.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Get loai thong tin may moc 
                    thongTinMayMoc.LoaiThongTin = GetLoaiThongTinMayMoc(thongTinMayMoc.LoaiTTMMID.Value);

                    danhSachThongTinMayMoc.Add(thongTinMayMoc);
                }
            }

            return danhSachThongTinMayMoc;
        }

        // Get thong tin may moc by mmid
        public ThongTinMayMoc GetThongTinMayMocByID(object? ttmmID)
        {
            ThongTinMayMoc thongTinMayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ThongTinMayMoc}] WHERE [{Common.MM_TTMMID}] = @TTMMID";

                command.Parameters.AddWithValue("@TTMMID", ttmmID);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowItems = thongTinMayMoc.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            // Get loai thong tin may moc 
            thongTinMayMoc.LoaiThongTin = GetLoaiThongTinMayMoc(thongTinMayMoc.LoaiTTMMID.Value);

            return thongTinMayMoc;
        }
        // Insert thong tin may moc
        public (int, string) InsertThongTinMayMoc(ThongTinMayMoc? ttmm)
        {
            int result = -1; string errorMess = string.Empty;

            if (ttmm == null) return (result, "Error");

            List<Propertyy> items = ttmm.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", items.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", items.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_ThongTinMayMoc}] ({columnNames}) OUTPUT INSERTED.{Common.MM_TTMMID} VALUES ({parameterNames})";

                foreach (var item in items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Delete thong tin may moc
        public (int, string) DeleteThongTinMayMoc(object? thongtinMMid)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_ThongTinMayMoc} WHERE [{Common.MM_TTMMID}] = '{thongtinMMid}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Update danh sach thong tin may moc
        public (int, string) UpdateListExtraMayMocDetails(List<ThongTinMayMoc> thongtinmaymocs)
        {
            int result = -1; string errorMess = string.Empty;

            if (thongtinmaymocs == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                foreach (var thongtin in thongtinmaymocs)
                {
                    List<Propertyy> items = thongtin.GetPropertiesValues().Where(pro => pro.DBName == Common.GiaTri).ToList();

                    var command = connection.CreateCommand();

                    string setClause = string.Join(",", items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                    command.CommandText = $"UPDATE [{Common.Table_ThongTinMayMoc}] SET {setClause} WHERE [{Common.MM_TTMMID}] = '{thongtin.TTMMID.Value}'";

                    foreach (var item in items)
                    {
                        string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                        object? parameterValue = item.Value;

                        command.Parameters.AddWithValue(parameterName, parameterValue);
                    }

                    result = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_MAY_LoaiThongTinMayMoc
        // Get loai thong tin may moc by loadittmmid
        public LoaiThongTinMayMoc GetLoaiThongTinMayMoc(object? loadittmmid)
        {
            LoaiThongTinMayMoc loaiThongTinMayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinMayMoc}] WHERE [{Common.MM_LoaiTTMMID}] = @LoaiTTMMID";

                command.Parameters.AddWithValue("@LoaiTTMMID", loadittmmid);

                using var reader = command.ExecuteReader();

                if (reader.Read()) // Use if since we're expecting a single result
                {
                    List<Propertyy> rowItems = loaiThongTinMayMoc.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }


            return loaiThongTinMayMoc;
        }

        // Get danh sach loai thong tin may moc
        public List<LoaiThongTinMayMoc> GetDanhSachLoaiThongTinMayMoc()
        {
            List<LoaiThongTinMayMoc> danhSachLoaiThongTinMayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinMayMoc}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinMayMoc loaithongTinMayMoc = new();

                    List<Propertyy> rowItems = loaithongTinMayMoc.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinMayMoc.Add(loaithongTinMayMoc);
                }
            }

            return danhSachLoaiThongTinMayMoc;
        }

        // Get danh sach loai thong tin may moc - mac dinh
        public List<LoaiThongTinMayMoc> GetDanhSachLoaiThongTinMayMoc(object isDefault)
        {
            List<LoaiThongTinMayMoc> danhSachLoaiThongTinMayMoc = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinMayMoc}] WHERE [{Common.MM_IsDefault}] = '{isDefault}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinMayMoc loaithongTinMayMoc = new();

                    List<Propertyy> rowItems = loaithongTinMayMoc.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinMayMoc.Add(loaithongTinMayMoc);
                }
            }

            return danhSachLoaiThongTinMayMoc;
        }

        // Them moi loai thong tin may moc
        public (int, string) InsertNewLoaiThongTinMayMoc(LoaiThongTinMayMoc loaithongtinmm)
        {
            List<Propertyy> newItems = loaithongtinmm.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LoaiThongTinMayMoc}] ({columnNames}) OUTPUT INSERTED.{Common.MM_LoaiTTMMID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete loai thong tin may moc
        public (int, string) DeleteLoaiThongTinMayMoc(object? loaiTTMMid)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_LoaiThongTinMayMoc} WHERE [{Common.MM_LoaiTTMMID}] = '{loaiTTMMid}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Kiem tra ten loai thong tin may moc da ton tai
        public bool IsExisting_LoaiThongTinMayMoc_Name(string? proValue)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_LoaiThongTinMayMoc}] WHERE [{Common.MM_TenTruyXuat}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }
        // Thay đổi tên của loại thông tin máy móc
        public (int, string) UpdateLoaiThongTinMayMocName(object? loaittmmid, string newName, string tentruyxuat)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_LoaiThongTinMayMoc} SET [{Common.MM_TenLoaiThongTin}] = N'{newName}', [{Common.MM_TenTruyXuat}] = '{tentruyxuat}' WHERE [{Common.MM_LoaiTTMMID}] = '{loaittmmid}' ";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_NV_NhanVien

        // Get nhan vien by ma nhan vien
        public NhanVien GetNhanVienbyMaNhanVien(object? maNV)
        {
            NhanVien nhanVien = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NhanVien}] WHERE [{Common.NV_MaNhanVien}] = @MANHANVIEN";

                command.Parameters.AddWithValue("@MANHANVIEN", maNV);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nhanVien.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            if (nhanVien.NVID.Value != null)
            {
                // Get danh sach thong tin nhan vien
                nhanVien.DSThongTin = GetDanhSachThongTinNhanVien(nhanVien.NVID.Value);
            }

            return nhanVien;
        }

        // Get nhan vien by ma nhan vien
        public NhanVien GetNhanVienbyID(object? id)
        {
            NhanVien nhanVien = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NhanVien}] WHERE [{Common.NV_NVID}] = @ID";

                command.Parameters.AddWithValue("@ID", id);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nhanVien.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            if (nhanVien.NVID.Value != null)
            {
                // Get danh sach thong tin nhan vien
                nhanVien.DSThongTin = GetDanhSachThongTinNhanVien(nhanVien.NVID.Value);
            }

            return nhanVien;
        }

        // Get ma nhan vien by ID
        public string GetMaNhanVienbyID(object? id)
        {
            string result = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.NV_MaNhanVien}] FROM [{Common.Table_NhanVien}] WHERE [{Common.NV_NVID}] = @ID";
                command.Parameters.AddWithValue("@ID", id ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    result = reader[Common.NV_MaNhanVien].ToString()?.Trim() ?? string.Empty;
                }
            }
            return result;
        }

        // Check gia tri truong thong tin nhan vien 
        public bool DefaultThongTinNhanVien_ValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_NhanVien}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Insert new nhan vien
        public (int, string) InsertNewNhanVien(NhanVien? newnhanvien)
        {
            int result = -1; string errorMess = string.Empty;

            if (newnhanvien == null) return (result, "Error");

            List<Propertyy> newNVItems = newnhanvien.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newNVItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newNVItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_NhanVien}] ({columnNames}) OUTPUT INSERTED.{Common.NV_NVID} VALUES ({parameterNames})";

                foreach (var item in newNVItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete nhan vien by ID
        public (int, string) DeleteNhanVien(object? nvID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_NhanVien} WHERE [{Common.NV_NVID}] = '{nvID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Load danh sach nhan vien
        public List<NhanVien> GetDanhSachNhanVien()
        {
            List<NhanVien> danhSachNhanVien = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NhanVien}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NhanVien nhanVien = new();

                    List<Propertyy> rowItems = nhanVien.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Load danh sach thong tin nhan vien

                    nhanVien.DSThongTin = GetDanhSachThongTinNhanVien(nhanVien.NVID.Value);

                    danhSachNhanVien.Add(nhanVien);
                }
            }

            return danhSachNhanVien;
        }

        // Update thong tin nhan vien
        public (int, string) UpdateNhanVienMainDetails(NhanVien nhanVien)
        {
            int result = -1; string errorMess = string.Empty;

            if (nhanVien == null) return (result, errorMess);

            List<Propertyy> Items = nhanVien.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_NhanVien}] SET {setClause} WHERE [{Common.NV_NVID}] = '{nhanVien.NVID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_LoaiThongTinNhanVien

        // Get loai thong tin nhan vien by ID
        public LoaiThongTinNhanVien GetLoaiThongTinNhanVien(object? loadittmmid)
        {
            LoaiThongTinNhanVien loaiThongTinNhanVien = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNhanVien}] WHERE [{Common.NV_LoaiTTNVID}] = @LoaiTTNVID";

                command.Parameters.AddWithValue("@LoaiTTNVID", loadittmmid);

                using var reader = command.ExecuteReader();

                if (reader.Read()) // Use if since we're expecting a single result
                {
                    List<Propertyy> rowItems = loaiThongTinNhanVien.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            return loaiThongTinNhanVien;
        }

        // Load danh sach loai thong tin nhan vien
        public List<LoaiThongTinNhanVien> GetDanhSachLoaiThongTinNhanVien()
        {
            List<LoaiThongTinNhanVien> danhSachLoaiThongTinNhanVien = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNhanVien}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinNhanVien loaithongTinNhanVien = new();

                    List<Propertyy> rowItems = loaithongTinNhanVien.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinNhanVien.Add(loaithongTinNhanVien);
                }
            }

            return danhSachLoaiThongTinNhanVien;
        }

        // Load danh sach loai thong tin nhan vien - macdinh 
        public List<LoaiThongTinNhanVien> GetDanhSachLoaiThongTinNhanVien(object isDefault)
        {
            List<LoaiThongTinNhanVien> danhSachLoaiThongTinNhanVien = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinNhanVien}] WHERE [{Common.NV_IsDefault}] = '{isDefault}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinNhanVien loaithongTinNhanVien = new();

                    List<Propertyy> rowItems = loaithongTinNhanVien.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinNhanVien.Add(loaithongTinNhanVien);
                }
            }

            return danhSachLoaiThongTinNhanVien;
        }

        // Insert loai thong tin nhan vien
        public (int, string) InsertNewLoaiThongTinNhanVien(LoaiThongTinNhanVien loaithongtin)
        {
            List<Propertyy> newItems = loaithongtin.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LoaiThongTinNhanVien}] ({columnNames}) OUTPUT INSERTED.{Common.NV_LoaiTTNVID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete loai thong tin nhan vien
        public (int, string) DeleteLoaiThongTinNhanVien(object? loaiTTNVID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_LoaiThongTinNhanVien} WHERE [{Common.NV_LoaiTTNVID}] = '{loaiTTNVID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Check ten loai thong tin nhan vien isExisting 
        public bool IsExisting_LoaiThongTinNhanVien_Name(string? tentruyxuat)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_LoaiThongTinNhanVien}] WHERE [{Common.NV_TenTruyXuat}] = N'{tentruyxuat?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Update loai thong tin nhan vien name
        public (int, string) UpdateLoaiThongTinNhanVienName(object? loaittnvid, string newName, string tentruyxuat)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_LoaiThongTinNhanVien} SET [{Common.NV_TenLoaiThongTin}] = N'{newName}', [{Common.NV_TenTruyXuat}] = '{tentruyxuat}' WHERE [{Common.NV_LoaiTTNVID}] = '{loaittnvid}' ";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_ThongTinNhanVien

        // Load danh sach thong tin nhan vien
        public List<ThongTinNhanVien> GetDanhSachThongTinNhanVien(object? nvID)
        {
            List<ThongTinNhanVien> danhSachThongTinNhanVien = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ThongTinNhanVien}] WHERE [{Common.NV_NVID}] = @NVID";

                command.Parameters.AddWithValue("@NVID", nvID);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ThongTinNhanVien thongTinNhanVien = new();

                    List<Propertyy> rowItems = thongTinNhanVien.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Get loai thong tin nhan vien
                    thongTinNhanVien.LoaiThongTin = GetLoaiThongTinNhanVien(thongTinNhanVien.LoaiTTNVID.Value);

                    danhSachThongTinNhanVien.Add(thongTinNhanVien);
                }
            }

            return danhSachThongTinNhanVien;
        }

        // Insert thong tin nhan vien
        public (int, string) InsertThongTinNhanVien(ThongTinNhanVien? ttNhanVien)
        {
            int result = -1; string errorMess = string.Empty;

            if (ttNhanVien == null) return (result, "Error");

            List<Propertyy> items = ttNhanVien.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", items.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", items.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_ThongTinNhanVien}] ({columnNames}) OUTPUT INSERTED.{Common.NV_TTNVID} VALUES ({parameterNames})";

                foreach (var item in items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete thong tin nhan vien
        public (int, string) DeleteThongTinNhanVien(object? thongtinNVID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_ThongTinNhanVien} WHERE [{Common.NV_TTNVID}] = '{thongtinNVID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update danh sach thong tin nhan vien
        public (int, string) UpdateListExtraNhanVienDetails(List<ThongTinNhanVien> thongtinnhanviens)
        {
            int result = -1; string errorMess = string.Empty;

            if (thongtinnhanviens == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                foreach (var thongtin in thongtinnhanviens)
                {
                    List<Propertyy> items = thongtin.GetPropertiesValues().Where(pro => pro.DBName == Common.NV_GiaTriThongTin).ToList();

                    var command = connection.CreateCommand();

                    string setClause = string.Join(",", items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                    command.CommandText = $"UPDATE [{Common.Table_ThongTinNhanVien}] SET {setClause} WHERE [{Common.NV_TTNVID}] = '{thongtin.TTNVID.Value}'";

                    foreach (var item in items)
                    {
                        string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                        object? parameterValue = item.Value;

                        command.Parameters.AddWithValue(parameterName, parameterValue);
                    }

                    result = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_SanPham
        // Check gia tri truong thong tin san pham 
        public bool DefaultThongTinSanPham_ValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_SanPham}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Insert new san pham
        public (int, string) InsertNewSanPham(SanPham? newsanpham)
        {
            int result = -1; string errorMess = string.Empty;

            if (newsanpham == null) return (result, "Error");

            List<Propertyy> newSPItems = newsanpham.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {

                    connection.Open();

                    var command = connection.CreateCommand();

                    string columnNames = string.Join(",", newSPItems.Select(key => $"[{key.DBName}]"));

                    string parameterNames = string.Join(",", newSPItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                    command.CommandText = $"INSERT INTO [{Common.Table_SanPham}] ({columnNames}) OUTPUT INSERTED.{Common.SP_SPID} VALUES ({parameterNames})";

                    foreach (var item in newSPItems)
                    {
                        string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                        object? parameterValue = item.Value;

                        command.Parameters.AddWithValue(parameterName, parameterValue);
                    }

                    object? rs = command.ExecuteScalar();

                    result = Convert.ToInt32(rs);

                    if (result == 0) result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete san pham by ID
        public (int, string) DeleteSanPham(object? spID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_SanPham} WHERE [{Common.SP_SPID}] = '{spID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Load danh sach san pham
        public List<SanPham> GetDanhSachSanPham()
        {
            List<SanPham> danhSachSanPham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_SanPham}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    SanPham sanPham = new();

                    List<Propertyy> rowItems = sanPham.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Load danh sach thong tin san pham
                    sanPham.DSThongTin = GetDanhSachThongTinSanPham(sanPham.SP_SPID.Value);
                    // Load danh sach NVL of san pham
                    sanPham.DanhSachNVLs = GetDSachNVLwithSanPham_bySPID(sanPham.SP_SPID.Value);
                    // Load danh sach NCong of san pham
                    foreach (var ncid in Common.GetListNCIDs(sanPham.NCIDs.Value))
                    {
                        sanPham.DSNguyenCongs.Add(GetNguyenCong(ncid));
                    }

                    danhSachSanPham.Add(sanPham);
                }
            }

            return danhSachSanPham;
        }

        public List<SanPham> GetDanhSachSanPham_ID_Name()
        {
            List<SanPham> danhSachSanPham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_SanPham}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    SanPham sanPham = new();

                    List<Propertyy> rowItems = sanPham.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    //// Load danh sach thong tin san pham
                    //sanPham.DSThongTin = GetDanhSachThongTinSanPham(sanPham.SP_SPID.Value);
                    //// Load danh sach NVL of san pham
                    //sanPham.DanhSachNVLs = GetDSachNVLofSanPham(sanPham.SP_SPID.Value);

                    danhSachSanPham.Add(sanPham);
                }
            }

            return danhSachSanPham;
        }

        // Lay san pham by sp id
        public SanPham GetSanpham(int spid, bool isLoadData = true) // Lay danh sach san pham
        {
            SanPham sanpham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_SanPham}] WHERE [{Common.SP_SPID}] = '{spid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowSPitems = sanpham.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            if (isLoadData)
            {
                sanpham.DanhSachNVLs = GetDSachNVLwithSanPham_bySPID(sanpham.SP_SPID.Value);
            }

            return sanpham;
        }

        // Get ma sanpham by ID
        public string GetMaSanphamByID(object? id)
        {
            string result = string.Empty;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT [{Common.SP_MaSP}] FROM [{Common.Table_SanPham}] WHERE [{Common.SP_SPID}] = @ID";
                command.Parameters.AddWithValue("@ID", id ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    result = reader[Common.SP_MaSP].ToString()?.Trim() ?? string.Empty;
                }
            }
            return result;
        }

        // Update thong tin san pham
        public (int, string) UpdateSanPhamMainDetails(SanPham sanPham)
        {
            int result = -1; string errorMess = string.Empty;

            if (sanPham == null) return (result, errorMess);

            List<Propertyy> Items = sanPham.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{Common.Table_SanPham}] SET {setClause} WHERE [{Common.SP_SPID}] = '{sanPham.SP_SPID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_LoaiThongTinSanPham

        // Get loai thong tin san pham by ID
        public LoaiThongTinSanPham GetLoaiThongTinSanPham(object? loaiTTSPID)
        {
            LoaiThongTinSanPham loaiThongTinSanPham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinSanPham}] WHERE [{Common.SP_LoaiTTSPID}] = @LoaiTTSPID";

                command.Parameters.AddWithValue("@LoaiTTSPID", loaiTTSPID);

                using var reader = command.ExecuteReader();

                if (reader.Read()) // Use if since we're expecting a single result
                {
                    List<Propertyy> rowItems = loaiThongTinSanPham.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }
                }
            }

            return loaiThongTinSanPham;
        }

        // Load danh sach loai thong tin san pham
        public List<LoaiThongTinSanPham> GetDanhSachLoaiThongTinSanPham()
        {
            List<LoaiThongTinSanPham> danhSachLoaiThongTinSanPham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinSanPham}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinSanPham loaithongTinSanPham = new();

                    List<Propertyy> rowItems = loaithongTinSanPham.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinSanPham.Add(loaithongTinSanPham);
                }
            }

            return danhSachLoaiThongTinSanPham;
        }

        // Load danh sach loai thong tin san pham - macdinh 
        public List<LoaiThongTinSanPham> GetDanhSachLoaiThongTinSanPham(object isDefault)
        {
            List<LoaiThongTinSanPham> danhSachLoaiThongTinSanPham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_LoaiThongTinSanPham}] WHERE [{Common.SP_IsDefault}] = '{isDefault}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    LoaiThongTinSanPham loaithongTinSanPham = new();

                    List<Propertyy> rowItems = loaithongTinSanPham.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    danhSachLoaiThongTinSanPham.Add(loaithongTinSanPham);
                }
            }

            return danhSachLoaiThongTinSanPham;
        }

        // Insert loai thong tin san pham
        public (int, string) InsertNewLoaiThongTinSanPham(LoaiThongTinSanPham loaithongtin)
        {
            List<Propertyy> newItems = loaithongtin.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_LoaiThongTinSanPham}] ({columnNames}) OUTPUT INSERTED.{Common.SP_LoaiTTSPID} VALUES ({parameterNames})";

                foreach (var item in newItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete loai thong tin san pham
        public (int, string) DeleteLoaiThongTinSanPham(object? loaiTTSPID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_LoaiThongTinSanPham} WHERE [{Common.SP_LoaiTTSPID}] = '{loaiTTSPID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Check ten loai thong tin san pham isExisting 
        public bool IsExisting_LoaiThongTinSanPham_Name(string? tentruyxuat)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_LoaiThongTinSanPham}] WHERE [{Common.SP_TenTruyXuat}] = N'{tentruyxuat?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Update loai thong tin san pham name
        public (int, string) UpdateLoaiThongTinSanPhamName(object? loaittnvid, string newName, string tentruyxuat)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_LoaiThongTinSanPham} SET [{Common.SP_TenLoaiThongTin}] = N'{newName}', [{Common.SP_TenTruyXuat}] = '{tentruyxuat}' WHERE [{Common.SP_LoaiTTSPID}] = '{loaittnvid}' ";

                    var command = new SqlCommand(sqlQuery, connection);

                    result = command.ExecuteNonQuery();

                    connection.Close();

                    return (result, string.Empty);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

                return (-1, err);
            }
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_ThongTinSanPham
        public List<ThongTinSanPham> GetDanhSachThongTinSanPham(object? spID)
        {
            List<ThongTinSanPham> danhSachThongTinSanPham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_ThongTinSanPham}] WHERE [{Common.SP_SPID}] = @SPID";

                command.Parameters.AddWithValue("@SPID", spID);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ThongTinSanPham thongTinSanPham = new();

                    List<Propertyy> rowItems = thongTinSanPham.GetPropertiesValues();

                    foreach (var item in rowItems)
                    {
                        string? columnName = item.DBName;

                        if (reader.GetOrdinal(columnName) != -1) // Check if the column exists
                        {
                            object columnValue = reader[columnName];

                            item.Value = columnValue == DBNull.Value ? null : columnValue;
                        }
                    }

                    // Get loai thong tin san pham
                    thongTinSanPham.LoaiThongTin = GetLoaiThongTinSanPham(thongTinSanPham.LoaiTTSPID.Value);

                    danhSachThongTinSanPham.Add(thongTinSanPham);
                }
            }
            return danhSachThongTinSanPham;
        }

        // Insert thong tin san pham
        public (int, string) InsertThongTinSanPham(ThongTinSanPham? ttSanPham)
        {
            int result = -1; string errorMess = string.Empty;

            if (ttSanPham == null) return (result, "Error");

            List<Propertyy> items = ttSanPham.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", items.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", items.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_ThongTinSanPham}] ({columnNames}) OUTPUT INSERTED.{Common.SP_TTSPID} VALUES ({parameterNames})";

                foreach (var item in items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }
            return (result, errorMess);
        }

        // Delete thong tin san pham
        public (int, string) DeleteThongTinSanPham(object? thongtinSPID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_ThongTinSanPham} WHERE [{Common.SP_TTSPID}] = @ThongTinSPID";

                command.Parameters.AddWithValue("@ThongTinSPID", thongtinSPID);

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }
            return (result, errorMess);
        }

        // Update thong tin san pham
        public (int, string) UpdateListExtraSanPhamDetails(List<ThongTinSanPham> thongtinsanphams)
        {
            int result = -1; string errorMess = string.Empty;

            if (thongtinsanphams == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                foreach (var thongtin in thongtinsanphams)
                {
                    List<Propertyy> items = thongtin.GetPropertiesValues().Where(pro => pro.DBName == Common.SP_GiaTriThongTin).ToList();

                    var command = connection.CreateCommand();

                    string setClause = string.Join(",", items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                    command.CommandText = $"UPDATE [{Common.Table_ThongTinSanPham}] SET {setClause} WHERE [{Common.SP_TTSPID}] = @TTSPID";

                    command.Parameters.AddWithValue("@TTSPID", thongtin.TTSPID.Value);

                    foreach (var item in items)
                    {
                        string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                        object? parameterValue = item.Value;

                        command.Parameters.AddWithValue(parameterName, parameterValue);
                    }

                    result = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;
                return (-1, errorMess);
            }
            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KetQuaGC
        // Insert 
        public (int, string) InsertKetQuaGC(KetQuaGC ketquaGC)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (ketquaGC == null) return (result, "Error: KetQuaGC is null");

            List<Propertyy> properties = ketquaGC.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"INSERT INTO [{KQGCDBName.Table_KetQuaGC}] ({columns}) OUTPUT INSERTED.{KQGCDBName.KQGCID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute and handle potential null result
                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    // Successfully inserted; proceed with DongThung insertion if necessary
                    //if (ketquaGC.DSDongThung != null && ketquaGC.DSDongThung.Any())
                    //{
                    //    foreach (var dongThung in ketquaGC.DSDongThung)
                    //    {
                    //        var (dongThungResult, dongThungError) = InsertDongThung(connection, result, dongThung);
                    //        if (dongThungResult == -1)
                    //        {
                    //            // If there's an error inserting DongThung, rollback and return the error
                    //            transaction.Rollback();
                    //            return (-1, $"Error inserting DongThung: {dongThungError}");
                    //        }
                    //    }
                    //}

                    // Commit transaction if all operations were successful
                    transaction.Commit();
                }
                else
                {
                    result = -1; // Set to -1 if insertion was not successful
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                // Rollback transaction in case of error
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Delete 
        public (bool, string) DeleteKetQuaGC(object? kqgcId)
        {
            // Check for valid ID
            if (kqgcId == null)
            {
                return (false, "Error: Invalid KQGCID.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{KQGCDBName.Table_KetQuaGC}] WHERE [{KQGCDBName.KQGCID}] = @KQGCID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@KQGCID", kqgcId);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }
        // Update 
        public (int, string) UpdateKetQuaGC(KetQuaGC ketquaGC, int ketquaGCId)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (ketquaGC == null) return (result, "Error: KetQuaGC is null");

            List<Propertyy> properties = ketquaGC.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"UPDATE [{KQGCDBName.Table_KetQuaGC}] SET {updateSet} WHERE {KQGCDBName.KQGCID} = @KetQuaGCId";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }
                command.Parameters.AddWithValue("@KetQuaGCId", ketquaGCId);

                // Execute update command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated; proceed with DongThung update if necessary
                    //if (ketquaGC.DSDongThung != null && ketquaGC.DSDongThung.Any())
                    //{
                    //    foreach (var dongThung in ketquaGC.DSDongThung)
                    //    {
                    //        var (dongThungResult, dongThungError) = UpdateDongThung(connection, ketquaGCId, dongThung);
                    //        if (dongThungResult == -1)
                    //        {
                    //            // If there's an error updating DongThung, rollback and return the error
                    //            transaction.Rollback();
                    //            return (-1, $"Error updating DongThung: {dongThungError}");
                    //        }
                    //    }
                    //}
                    // Commit transaction if all operations were successful
                    transaction.Commit();
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified KetQuaGC may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                // Rollback transaction in case of error
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Get  
        public (List<KetQuaGC>, string) GetListKetQuaGC(object? kqgcId = null)
        {
            List<KetQuaGC> listKetQuaGC = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using var command = connection.CreateCommand();

                    command.CommandText = $"SELECT * FROM [{KQGCDBName.Table_KetQuaGC}]";

                    if (kqgcId != null)
                    {
                        command.CommandText += $" WHERE [{KQGCDBName.KQGCID}] = @KQGCID";

                        command.Parameters.AddWithValue("@KQGCID", kqgcId);
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        KetQuaGC ketQuaGC = new();

                        List<Propertyy> rowItems = ketQuaGC.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }
                        // Get DongThung for this KetQuaGC
                        if (ketQuaGC.KQGCID.Value != null)
                        {
                            //ketQuaGC.DSDongThung = GetListDongThung(ketQuaGC.KQGCID.Value);
                        }

                        listKetQuaGC.Add(ketQuaGC);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";

                    listKetQuaGC.Clear(); // Clear the list in case of error
                }
            }
            return (listKetQuaGC, errorMessage);
        }

        public (List<KetQuaGC>, string) GetListKetQuaGC(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<KetQuaGC> listKetQuaGC = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();

                    using var command = connection.CreateCommand();

                    command.CommandText = $"SELECT * FROM [{KQGCDBName.Table_KetQuaGC}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        KetQuaGC ketQuaGC = new();

                        List<Propertyy> rowItems = ketQuaGC.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }
                        // Get DongThung for this KetQuaGC
                        if (ketQuaGC.KQGCID.Value != null)
                        {
                            //ketQuaGC.DSDongThung = GetListDongThung(ketQuaGC.KQGCID.Value);
                        }

                        listKetQuaGC.Add(ketQuaGC);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";

                    listKetQuaGC.Clear(); // Clear the list in case of error
                }
            }
            return (listKetQuaGC, errorMessage);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region KHSX_PartOfThungTPham
        // Insert 
        public (int, string) InsertPartOfThungTPham(PartOfThungTPham thungtpham)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (thungtpham == null) return (result, "Error: PartOfThungTPham is null");

            List<Propertyy> properties = thungtpham.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction 

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"INSERT INTO [{PartOfThungTPham.DBName.Table_PartOfThungTP}] ({columns}) OUTPUT INSERTED.{PartOfThungTPham.DBName.POTTPID} VALUES ({parameters})";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    // Successfully inserted
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if insertion was not successful
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update 
        public (int, string) UpdatePartOfThungTPham(PartOfThungTPham thungtpham)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (thungtpham == null) return (result, "Error: ThungTPham is null");

            List<Propertyy> properties = thungtpham.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{PartOfThungTPham.DBName.Table_PartOfThungTP}] SET {updateSet} WHERE [{PartOfThungTPham.DBName.POTTPID}] = '{thungtpham.POTTPID.Value}'";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified DongThung may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get
        public (List<PartOfThungTPham> thungTPhams, string error) GetListPartOfThungTPs(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<PartOfThungTPham> listThungTPhams = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{PartOfThungTPham.DBName.Table_PartOfThungTP}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PartOfThungTPham thungtpham = new();

                        List<Propertyy> rowItems = thungtpham.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        listThungTPhams.Add(thungtpham);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listThungTPhams.Clear(); // Clear the list in case of error
                }
            }
            return (listThungTPhams, errorMessage);
        }

        // Delete
        public (bool, string) DeletePartOfThungTPham(object? ttpid)
        {
            // Check for valid ID
            if (ttpid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{PartOfThungTPham.DBName.Table_PartOfThungTP}] WHERE [{PartOfThungTPham.DBName.POTTPID}] = @TTPID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TTPID", ttpid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }


        #endregion

        // ------------------------------------------------------------------------------------- //
        #region KHSX_ThungTPham
        // Insert 
        public (int, string) InsertThungTPham(ThungTPham thungtpham)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (thungtpham == null) return (result, "Error: ThungTPham is null");

            List<Propertyy> properties = thungtpham.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction 

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"INSERT INTO [{ThungTPham.DBName.Table_ThungTPham}] ({columns}) OUTPUT INSERTED.{ThungTPham.DBName.TTPID} VALUES ({parameters})";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    // Successfully inserted
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if insertion was not successful
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update 
        public (int, string) UpdateThungTPham(ThungTPham thungtpham)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (thungtpham == null) return (result, "Error: ThungTPham is null");

            List<Propertyy> properties = thungtpham.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{ThungTPham.DBName.Table_ThungTPham}] SET {updateSet} WHERE [{ThungTPham.DBName.TTPID}] = '{thungtpham.TTPID.Value}'";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get
        public (List<ThungTPham> thungTPhams, string error) GetListThungTPs(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<ThungTPham> listThungTPhams = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{ThungTPham.DBName.Table_ThungTPham}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ThungTPham thungtpham = new();

                        List<Propertyy> rowItems = thungtpham.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        if (int.TryParse(thungtpham.InStock.Value?.ToString(), out int instock))
                        {
                            thungtpham.DaXuatKho = thungtpham.PXKTPID.Value != null && instock == 0;

                            if (thungtpham.DaXuatKho)
                            {
                                thungtpham.DaNhapKho = true;
                            }
                            else
                            {
                                thungtpham.DaNhapKho = thungtpham.VTofTPID.Value != null && thungtpham.PNKTPID.Value != null && instock == 1;
                            }
                        }

                        // Load partofthungTPs
                        thungtpham.PartOfThungTPhams = GetListPartOfThungTPs(new() { { PartOfThungTPham.DBName.MaQuanLyThung, thungtpham.MaQuanLyThung.Value } }).thungTPhams;

                        listThungTPhams.Add(thungtpham);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listThungTPhams.Clear(); // Clear the list in case of error
                }
            }
            return (listThungTPhams, errorMessage);
        }

        // Delete
        public (bool, string) DeleteThungTP(object? ttpid)
        {
            // Check for valid ID
            if (ttpid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{ThungTPham.DBName.Table_ThungTPham}] WHERE [{ThungTPham.DBName.TTPID}] = @TTPID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TTPID", ttpid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        public (bool, string) DeleteThungTPbyMQLThung(object? mqlthung)
        {
            // Check for valid ID
            if (mqlthung == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{ThungTPham.DBName.Table_ThungTPham}] WHERE [{ThungTPham.DBName.MaQuanLyThung}] = @MQLT";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MQLT", mqlthung);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region TDGC_TienDoGC
        // Insert
        public (int, string) InsertTienDoGC(TienDoGC tiendoGC)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (tiendoGC == null) return (result, "Error: TienDoGC is null");

            List<Propertyy> properties = tiendoGC.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{TienDoGC.DBName.Table_TienDoGC}] ({columns}) OUTPUT INSERTED.{TienDoGC.DBName.TDGCID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    if (tiendoGC.DSachTienDoRows != null && tiendoGC.DSachTienDoRows.Any())
                    {
                        foreach (var tiendorow in tiendoGC.DSachTienDoRows)
                        {
                            // asign tdgcid
                            tiendorow.TDGCID.Value = result;
                            // Pass the transaction here
                            var (tiendorowResult, tiendogcrowError) = InsertTienDoGCRow(connection, transaction, tiendorow);
                            if (tiendorowResult == -1)
                            {
                                transaction.Rollback();
                                return (-1, $"Error inserting TienDoGCRow: {tiendogcrowError}");
                            }
                        }
                    }
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Delete
        public (bool, string) DeleteTienDoGC(object? tdgcid)
        {
            // Check for valid ID
            if (tdgcid == null)
            {
                return (false, "Error: Invalid TDGCID.");
            }

            using var connection = new SqlConnection(connectionString);

            connection.Open();

            string query = $"DELETE FROM [{TienDoGC.DBName.Table_TienDoGC}] WHERE [{TienDoGC.DBName.TDGCID}] = @TDGCID";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@TDGCID", tdgcid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();

                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }
        // Update
        public (int, string) UpdateTienDoGC(TienDoGC tiendoGC)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (tiendoGC == null) return (result, "Error: TienDoGC is null");

            List<Propertyy> properties = tiendoGC.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                // Update main TienDoGC record
                string updateSet = string.Join(", ", properties.Select(p =>
                    $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{TienDoGC.DBName.Table_TienDoGC}] 
                                        SET {updateSet} 
                                        WHERE [{TienDoGC.DBName.TDGCID}] = @TDGCID";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                command.Parameters.AddWithValue("@TDGCID", tiendoGC.TDGCID.Value);

                result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                    errorMess = "No rows were updated. The specified TienDoGC may not exist.";
                    transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Get 
        public (List<TienDoGC>, string) GetListTienDoGC(object? tdgcid = null, object? ncid = null, object? khsxid = null)
        {
            List<TienDoGC> listTienDoGC = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{TienDoGC.DBName.Table_TienDoGC}]";

                    // Helper function to add condition if value is not null
                    void AddCondition(object? value, string fieldName, string paramName)
                    {
                        if (value != null)
                        {
                            conditions.Add($"[{fieldName}] = @{paramName}");
                            command.Parameters.AddWithValue($"@{paramName}", value);
                        }
                    }

                    // Add all potential conditions
                    AddCondition(tdgcid, TienDoGC.DBName.TDGCID, "TDGCID");
                    AddCondition(khsxid, TienDoGC.DBName.KHSXID, "KHSXID");
                    AddCondition(ncid, TienDoGC.DBName.NCID, "NCID");
                    // Add more conditions as needed...

                    // Combine all conditions with AND
                    if (conditions.Any())
                    {
                        command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TienDoGC tiendoGC = new();

                        List<Propertyy> rowItems = tiendoGC.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }
                        // Get danh sach TienDoGCRow
                        if (tiendoGC.TDGCID.Value != null)
                        {
                            // Load ma san pham
                            tiendoGC.MaSanPham = GetMaSanphamByID(tiendoGC.SPID.Value);
                            // Load cong doan
                            tiendoGC.TenCongDoan = GetNguyenCongByID(tiendoGC.NCID.Value);

                            tiendoGC.DSachTienDoRows = GetListTienDoGCRow(tiendoGC.TDGCID.Value);
                        }
                        listTienDoGC.Add(tiendoGC);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listTienDoGC.Clear(); // Clear the list in case of error
                }
            }
            return (listTienDoGC, errorMessage);
        }

        public (List<TienDoGC>, string) GetListTienDoGC(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<TienDoGC> listTienDoGC = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{TienDoGC.DBName.Table_TienDoGC}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TienDoGC tiendoGC = new();

                        List<Propertyy> rowItems = tiendoGC.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }
                        // Get danh sach TienDoGCRow
                        if (tiendoGC.TDGCID.Value != null)
                        {
                            // Load ma san pham
                            tiendoGC.MaSanPham = GetMaSanphamByID(tiendoGC.SPID.Value);
                            // Load cong doan
                            tiendoGC.TenCongDoan = GetNguyenCongByID(tiendoGC.NCID.Value);

                            tiendoGC.DSachTienDoRows = GetListTienDoGCRow(tiendoGC.TDGCID.Value);
                        }
                        listTienDoGC.Add(tiendoGC);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listTienDoGC.Clear(); // Clear the list in case of error
                }
            }
            return (listTienDoGC, errorMessage);
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region TDGC_TienDoGCRow
        // Insert
        public (int, string) InsertTienDoGCRow(SqlConnection connection, SqlTransaction transaction, TienDoGCRow tiendogcRow)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (tiendogcRow == null) return (result, "Error: TienDoGCRow is null");

            List<Propertyy> properties = tiendogcRow.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Set the transaction from parent

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{TienDoGCRow.DBName.Table_TienDoGCRow}] ({columns}) OUTPUT INSERTED.{TienDoGCRow.DBName.TDGCRowID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? tdgcrowid = command.ExecuteScalar();
                if (tdgcrowid != null && int.TryParse(tdgcrowid.ToString(), out result) && result > 0)
                {

                }
                else result = -1;

            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Delete
        public (bool, string) DeleteTienDoGCRow(object? tdgcrowid)
        {
            // Check for valid ID
            if (tdgcrowid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{TienDoGCRow.DBName.Table_TienDoGCRow}] WHERE [{TienDoGCRow.DBName.TDGCRowID}] = @TDGCRowID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TDGCRowID", tdgcrowid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        public (int, string) InsertSingleTienDoGCRow(int tdgcid, TienDoGCRow tiendoGCRow)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (tiendoGCRow == null)
                return (result, "Error: TienDoGCRow is null");

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var (tdgcrowID, rowError) = InsertTienDoGCRow(connection, transaction, tiendoGCRow);
                if (tdgcrowID == -1)
                {
                    transaction.Rollback();
                    return (-1, $"Error inserting TienDoGCRow: {rowError}");
                }

                transaction.Commit();
                result = tdgcid;
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Update
        public (int, string) UpdateTienDoGCRow(SqlConnection connection, SqlTransaction transaction, object? tiendogcID, TienDoGCRow tiendogcRow)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (tiendogcRow == null) return (result, "Error: TienDoGCRow is null");

            List<Propertyy> properties = tiendogcRow.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Use the transaction passed from parent

                string updateSet = string.Join(", ", properties.Select(p =>
                    $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{TienDoGCRow.DBName.Table_TienDoGCRow}] 
                                        SET {updateSet} 
                                        WHERE [{TienDoGCRow.DBName.TDGCRowID}] = '{tiendogcRow.TDGCRowID.Value}' 
                                        AND [{TienDoGCRow.DBName.TDGCID}] = @TDGCID";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Add TDGCID parameter
                command.Parameters.AddWithValue("@TDGCID", tiendogcID);

                // Execute command
                result = command.ExecuteNonQuery();
                if (result <= 0)
                {
                    result = -1;
                    errorMess = "No rows were updated. The specified TienDoGCRow may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Get 
        private List<TienDoGCRow> GetListTienDoGCRow(object tdgcID)
        {
            List<TienDoGCRow> listTienDoGCRow = new();

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using var command = connection.CreateCommand();

                    command.CommandText = $"SELECT * FROM [{TienDoGCRow.DBName.Table_TienDoGCRow}] WHERE [{TienDoGCRow.DBName.TDGCID}] = @TDGCID";

                    command.Parameters.AddWithValue("@TDGCID", tdgcID);

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TienDoGCRow tiendogcrow = new();

                        List<Propertyy> rowItems = tiendogcrow.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        listTienDoGCRow.Add(tiendogcrow);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as appropriate
                    Console.WriteLine($"Error in GetListDongThung: {ex.Message}");
                }
            }
            return listTienDoGCRow;
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_NGType
        // Insert new NGType
        public (int, string) InsertNGType(NGType NGtype)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (NGtype == null) return (result, "Error: NGType is null");

            List<Propertyy> properties = NGtype.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"INSERT INTO [{NGType.NGTypeDBName.Table_NGType}] ({columns}) OUTPUT INSERTED.{NGType.NGTypeDBName.NGID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute and handle potential null result
                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    // Commit transaction if all operations were successful
                    transaction.Commit();
                }
                else
                {
                    result = -1; // Set to -1 if insertion was not successful
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                // Rollback transaction in case of error
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Get list all NGType
        public (List<NGType>, string) GetDanhSachNGType(object? ngtypeId = null)
        {
            List<NGType> listNGType = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using var command = connection.CreateCommand();

                    command.CommandText = $"SELECT * FROM [{NGType.NGTypeDBName.Table_NGType}]";

                    if (ngtypeId != null)
                    {
                        command.CommandText += $" WHERE [{NGType.NGTypeDBName.NGID}] = @ID";

                        command.Parameters.AddWithValue("@ID", ngtypeId);
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        NGType ngType = new();

                        List<Propertyy> rowItems = ngType.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        listNGType.Add(ngType);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";

                    listNGType.Clear(); // Clear the list in case of error
                }
            }
            return (listNGType, errorMessage);
        }
        // Get noi dung NG by ID
        public string GetNoiDungNGbyID(object? id)
        {
            string result = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT [{NGType.NGTypeDBName.NoiDungNG}] FROM [{NGType.NGTypeDBName.Table_NGType}] WHERE [{NGType.NGTypeDBName.NGID}] = @ID";

                command.Parameters.AddWithValue("@ID", id ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    result = reader[NGType.NGTypeDBName.NoiDungNG].ToString()?.Trim() ?? string.Empty;
                }
            }
            return result;
        }

        // Delete
        public (bool, string) DeleteNGType(object? ngid)
        {
            // Check for valid ID
            if (ngid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{NGType.NGTypeDBName.Table_NGType}] WHERE [{NGType.NGTypeDBName.NGID}] = @NGID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@NGID", ngid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        // Update
        public (int, string) UpdateNGType(NGType ngid)
        {
            int result = -1; string errorMess = string.Empty;

            if (ngid == null) return (result, errorMess);

            List<Propertyy> Items = ngid.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string setClause = string.Join(",", Items.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"UPDATE [{NGType.NGTypeDBName.Table_NGType}] SET {setClause} WHERE [{NGType.NGTypeDBName.NGID}] = '{ngid.NGID.Value}'";

                foreach (var item in Items)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Check Is exsting
        public bool DefaultThongTinNGTypeValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{NGType.NGTypeDBName.Table_NGType}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHSX_LOT
        // Insert
        public (int, string) InsertLOT_khsx(KHSX_LOT lotkhsx)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (lotkhsx == null) return (result, "Error: LOT_khsx is null");

            List<Propertyy> properties = lotkhsx.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{KHSX_LOT.DBName.Table_KHSXLOT}] ({columns}) OUTPUT INSERTED.{KHSX_LOT.DBName.KHSXLOTID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Get
        public (List<KHSX_LOT>, string) GetListLOT_khsx(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<KHSX_LOT> listLOT_khsx = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{KHSX_LOT.DBName.Table_KHSXLOT}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    // Set odery by KHSXLOTID ( unique column (KHSXID, MQLLOT, NCID) not follow KHSXLOTID ascending)
                    command.CommandText += $" ORDER BY {KHSX_LOT.DBName.KHSXLOTID}";

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        KHSX_LOT lotkhsx = new();

                        List<Propertyy> rowItems = lotkhsx.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        listLOT_khsx.Add(lotkhsx);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listLOT_khsx.Clear(); // Clear the list in case of error
                }
            }
            return (listLOT_khsx, errorMessage);
        }

        public (List<object?> columnValues, string errorMessage) GetListLOT_khsxColumnValues(Dictionary<string, object?> parameters, string? returnColumnName = null, bool isGetAll = false)
        {
            List<object?> columnValues = new();
            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var conditions = new List<string>();
                    var command = connection.CreateCommand();

                    // If a specific column is requested, select only that column
                    string selectClause = returnColumnName != null
                        ? $"SELECT [{returnColumnName}]"
                        : "SELECT *";

                    command.CommandText = $"{selectClause} FROM [{KHSX_LOT.DBName.Table_KHSXLOT}]";

                    if (!isGetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }
                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // If no specific column is requested, read entire object
                        if (returnColumnName == null)
                        {
                            KHSX_LOT lotkhsx = new();
                            List<Propertyy> rowItems = lotkhsx.GetPropertiesValues();
                            foreach (var item in rowItems)
                            {
                                string? columnName = item.DBName;
                                if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                                {
                                    object columnValue = reader[columnName];
                                    item.Value = columnValue == DBNull.Value ? null : columnValue;
                                }
                                columnValues.Add(lotkhsx);
                            }
                        }
                        else
                        {
                            // If a specific column is requested, read only that column
                            object columnValue = reader[returnColumnName];
                            columnValues.Add(columnValue == DBNull.Value ? null : columnValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    columnValues.Clear(); // Clear the list in case of error
                }
            }
            return (columnValues, errorMessage);
        }

        // Update
        public (int, string) UpdateLOT_khsx(KHSX_LOT lotkhsx)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (lotkhsx == null) return (result, "Error: LOT_khsx is null");

            List<Propertyy> properties = lotkhsx.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                // Update main LOT_khsx record
                string updateSet = string.Join(", ", properties.Select(p =>
                    $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{KHSX_LOT.DBName.Table_KHSXLOT}] 
                                        SET {updateSet} 
                                        WHERE [{KHSX_LOT.DBName.KHSXLOTID}] = @KHSXLOTID";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                command.Parameters.AddWithValue("@KHSXLOTID", lotkhsx.KHSXLOTID.Value);

                result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                    errorMess = "No rows were updated. The specified LOT_khsx may not exist.";
                    transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_LogKiemKe
        // Insert
        public (int, string) InsertLogKiemKe(LogKiemKe logkiemke)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (logkiemke == null) return (result, "Error: log is null");

            List<Propertyy> properties = logkiemke.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{LogKiemKe.DBName.Table_LogKiemKe}] ({columns}) OUTPUT INSERTED.{LogKiemKe.DBName.LOGKKEID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get list
        public (List<LogKiemKe>, string) GetListLogKiemKes(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<LogKiemKe> listLogkiemkes = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{LogKiemKe.DBName.Table_LogKiemKe}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        LogKiemKe logkiemke = new();

                        List<Propertyy> rowItems = logkiemke.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        listLogkiemkes.Add(logkiemke);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listLogkiemkes.Clear(); // Clear the list in case of error
                }
            }
            return (listLogkiemkes, errorMessage);
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_KHO_ViTriTPham

        // Insert
        public (int, string) InsertViTriTPham(ViTriTPham vitrithpham)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (vitrithpham == null) return (result, "Error: ViTriTPham is null");

            List<Propertyy> properties = vitrithpham.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{ViTriTPham.DBName.Table_ViTriTPham}] ({columns}) OUTPUT INSERTED.{ViTriTPham.DBName.VTTPID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update 
        public (int, string) UpdateViTriTPham(ViTriTPham vitrithpham)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (vitrithpham == null) return (result, "Error: is null");

            List<Propertyy> properties = vitrithpham.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{ViTriTPham.DBName.Table_ViTriTPham}] SET {updateSet} WHERE [{ViTriTPham.DBName.VTTPID}] = '{vitrithpham.VTTPID.Value}'";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get
        public (List<ViTriTPham> viTriTPhams, string error) GetListViTriTPhams(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<ViTriTPham> listViTriTPhams = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{ViTriTPham.DBName.Table_ViTriTPham}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ViTriTPham vitrithpham = new();

                        List<Propertyy> rowItems = vitrithpham.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        _ = int.TryParse(vitrithpham.VTSucChua.Value?.ToString(), out int suchua) ? suchua : 0;

                        vitrithpham.SLConTrong = suchua - GetViTriTPhamSoLuongTrong(vitrithpham.VTTPID.Value).soluong;

                        listViTriTPhams.Add(vitrithpham);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listViTriTPhams.Clear(); // Clear the list in case of error
                }
            }
            return (listViTriTPhams, errorMessage);
        }

        // Get 
        public ViTriTPham GetViTriTPhamByVTofTPID(object? vtoftpid)
        {
            ViTriTPham viTriTPham = new();

            var viTriofTPham = GetListViTriofTPhams(new Dictionary<string, object?>() { { ViTriofTPham.DBName.VTofTPID, vtoftpid } }).viTriofTPhams.FirstOrDefault();

            if (viTriofTPham != null)
            {
                viTriTPham = GetListViTriTPhams(new Dictionary<string, object?>() { { ViTriTPham.DBName.VTTPID, viTriofTPham.VTTPID.Value } }).viTriTPhams.FirstOrDefault() ?? new();
            }

            return viTriTPham;
        }

        // Delete
        public (bool, string) DeleteViTriTPham(object? vttpid)
        {
            // Check for valid ID
            if (vttpid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{ViTriTPham.DBName.Table_ViTriTPham}] WHERE [{ViTriTPham.DBName.VTTPID}] = @VTTPID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VTTPID", vttpid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        // Tính số lượng trống còn lại của vị trí
        public (int soluong, string) GetViTriTPhamSoLuongTrong(object? vttpid)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = $"SELECT SUM(CAST([{ViTriofTPham.DBName.VTTPSoLuong}] AS INT)) AS TongSoLuong FROM [{ViTriofTPham.DBName.Table_ViTriofTPham}] WHERE [{ViTriofTPham.DBName.VTTPID}] = '{vttpid}'";

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int tongSoLuong = Convert.ToInt32(result);

                        return (tongSoLuong, string.Empty);
                    }
                    else
                    {
                        return (0, "Null value");
                    }
                }
            }
            catch (SqlException ex)
            {
                return (-1, ex.Message);
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        // Check gia tri truong thong tin mac dinh is exsting? 
        public bool DefaultThongTinViTriThanhPham_ValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{ViTriTPham.DBName.Table_ViTriTPham}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }



        #endregion

        #region API_ViTriTPham
        public async Task<(List<ViTriTPham> viTriTPhams, string error)> APIGetListViTriTPhamsAsync(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<ViTriTPham> listViTriTPhams = new();
            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{ViTriTPham.DBName.Table_ViTriTPham}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        ViTriTPham vitrithpham = new();
                        List<Propertyy> rowItems = vitrithpham.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];
                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        _ = int.TryParse(vitrithpham.VTSucChua.Value?.ToString(), out int suchua) ? suchua : 0;

                        vitrithpham.SLConTrong = suchua - GetViTriTPhamSoLuongTrong(vitrithpham.VTTPID.Value).soluong;

                        listViTriTPhams.Add(vitrithpham);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listViTriTPhams.Clear(); // Clear the list in case of error
                }
            }
            return (listViTriTPhams, errorMessage);
        }
        #endregion

        #region Table_KHO_ViTriofTPham

        // Insert
        public (int, string) InsertViTriofTPham(ViTriofTPham vitrioftp)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (vitrioftp == null) return (result, "Error: is null");

            List<Propertyy> properties = vitrioftp.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{ViTriofTPham.DBName.Table_ViTriofTPham}] ({columns}) OUTPUT INSERTED.{ViTriofTPham.DBName.VTofTPID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update 
        public (int, string) UpdateViTriofTPham(ViTriofTPham vitrioftp)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (vitrioftp == null) return (result, "Error: is null");

            List<Propertyy> properties = vitrioftp.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{ViTriofTPham.DBName.Table_ViTriofTPham}] SET {updateSet} WHERE [{ViTriofTPham.DBName.VTofTPID}] = '{vitrioftp.VTofTPID.Value}'";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get
        public (List<ViTriofTPham> viTriofTPhams, string error) GetListViTriofTPhams(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<ViTriofTPham> listViTriofTPhams = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{ViTriofTPham.DBName.Table_ViTriofTPham}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ViTriofTPham vitrioftp = new();

                        List<Propertyy> rowItems = vitrioftp.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        listViTriofTPhams.Add(vitrioftp);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listViTriofTPhams.Clear(); // Clear the list in case of error
                }
            }
            return (listViTriofTPhams, errorMessage);
        }

        // Delete
        public (bool, string) DeleteViTriofTPham(object? vtoftpid)
        {
            // Check for valid ID
            if (vtoftpid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{ViTriofTPham.DBName.Table_ViTriofTPham}] WHERE [{ViTriofTPham.DBName.VTofTPID}] = @VTofTPID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VTofTPID", vtoftpid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_MQLTemplateProperty
        // Insert
        public (int, string) InsertMQLTemplateProperty(MQLTemplateProperty mqltemplateProperty)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (mqltemplateProperty == null) return (result, "Error: is null");

            List<Propertyy> properties = mqltemplateProperty.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{MQLTemplateProperty.DBName.Table_MQLTLProperty}] ({columns}) OUTPUT INSERTED.{MQLTemplateProperty.DBName.MQLTLpropertyID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Update 
        public (int, string) UpdateMQLTemplateProperty(MQLTemplateProperty mqltemplateProperty)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (mqltemplateProperty == null) return (result, "Error: is null");

            List<Propertyy> properties = mqltemplateProperty.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{MQLTemplateProperty.DBName.Table_MQLTLProperty}] SET {updateSet} WHERE [{MQLTemplateProperty.DBName.MQLTLpropertyID}] = '{mqltemplateProperty.MQLTLpropertyID.Value}'";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }
        // Get
        public (List<MQLTemplateProperty> mQLTemplates, string error) GetListMQLTLPropertiess(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<MQLTemplateProperty> listMQLTLproperties = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{MQLTemplateProperty.DBName.Table_MQLTLProperty}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        MQLTemplateProperty mqltemplateproperty = new();

                        List<Propertyy> rowItems = mqltemplateproperty.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        listMQLTLproperties.Add(mqltemplateproperty);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listMQLTLproperties.Clear(); // Clear the list in case of error
                }
            }
            return (listMQLTLproperties, errorMessage);
        }
        // Delete
        public (bool, string) DeleteMQLTemplateProperty(object? mqltemplateProperty)
        {
            // Check for valid ID
            if (mqltemplateProperty == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{MQLTemplateProperty.DBName.Table_MQLTLProperty}] WHERE [{MQLTemplateProperty.DBName.MQLTLpropertyID}] = @MQLTLpropertyID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MQLTLpropertyID", mqltemplateProperty);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        public bool IsThongTinMQLTemplateProperty_ValueIsExisting(string? proValue, string proName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{MQLTemplateProperty.DBName.Table_MQLTLProperty}] WHERE [{proName}] = N'{proValue?.Trim()}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }
        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_PhieuNhapKhoThanhPham
        public (int, string) InsertNewPNKThanhPham(PhieuNhapKhoTPham? newpnk)
        {
            int result = -1; string errorMess = string.Empty;

            if (newpnk == null) return (result, "Error");

            List<Propertyy> pnkItems = newpnk.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", pnkItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", pnkItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.Table_PhieuNhapKhoTPham}] ({columnNames}) OUTPUT INSERTED.{Common.PNKTPID} VALUES ({parameterNames})";

                foreach (var item in pnkItems)
                {
                    string parameterName = $"@{Regex.Replace(item.DBName ?? string.Empty, @"[^\w]+", "")}";

                    object? parameterValue = item.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);

                if (result == 0) result = -1;
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        public List<PhieuNhapKhoTPham> GetListPhieuNhapKhoTPham()
        {
            List<PhieuNhapKhoTPham> dsPNKho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuNhapKhoTPham}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PhieuNhapKhoTPham phieuxuatkho = new();

                    List<Propertyy> rowitems = phieuxuatkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    phieuxuatkho.ListKhoThungTPham = GetListThungTPs(new() { { ThungTPham.DBName.PNKTPID, phieuxuatkho.PNKTPID.Value } }, false).thungTPhams;

                    phieuxuatkho.MaViTri = GetListViTriTPhams(new Dictionary<string, object?>() { { ViTriTPham.DBName.VTTPID, phieuxuatkho.VTTPID.Value } }).viTriTPhams.FirstOrDefault()?.MaViTri.Value ?? string.Empty;

                    //foreach (var thung in phieuxuatkho.ListKhoThungTPham)
                    //{
                    //    if (thung.MaQuanLyThung.Value != null && int.TryParse(thung.VTofTPID.Value?.ToString(), out int vtid))
                    //    {
                    //        thung.DaNhapKho = vtid > 0;
                    //    }
                    //}

                    phieuxuatkho.IsDonePNK.Value = (phieuxuatkho.ListKhoThungTPham.All(ttp => ttp.DaNhapKho) && phieuxuatkho.ListKhoThungTPham.Count > 0) ? 1 : 0;

                    phieuxuatkho.isPNKDoneNhapKho = (int.TryParse(phieuxuatkho.IsDonePNK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

                    dsPNKho.Add(phieuxuatkho);
                }
            }

            return dsPNKho;
        }

        public (int, string) DeletePhieuNhapKhoTPham(object? pnktpID)
        {
            int result = -1; string errorMess = string.Empty;

            if (pnktpID == null) return (-1, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_PhieuNhapKhoTPham} WHERE [{Common.PNKTPID}] = '{pnktpID}'";

                object rs = command.ExecuteScalar();

                result = Convert.ToInt32(rs);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        public PhieuNhapKhoTPham GetPhieuNhapKhoTPhamByID(object? pnktpid)
        {
            PhieuNhapKhoTPham phieuxuatkho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuNhapKhoTPham}] WHERE [{Common.PNKTPID}] = '{pnktpid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = phieuxuatkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            phieuxuatkho.ListKhoThungTPham = GetListThungTPs(new() { { ThungTPham.DBName.PNKTPID, phieuxuatkho.PNKTPID.Value } }, false).thungTPhams;

            phieuxuatkho.MaViTri = GetListViTriTPhams(new Dictionary<string, object?>() { { ViTriTPham.DBName.VTTPID, phieuxuatkho.VTTPID.Value } }).viTriTPhams.FirstOrDefault()?.MaViTri.Value ?? string.Empty;

            //foreach (var thung in phieuxuatkho.ListKhoThungTPham)
            //{
            //    if (thung.MaQuanLyThung.Value != null && int.TryParse(thung.VTofTPID.Value?.ToString(), out int vtid))
            //    {
            //        thung.DaNhapKho = vtid > 0;
            //    }
            //}

            phieuxuatkho.IsDonePNK.Value = (phieuxuatkho.ListKhoThungTPham.All(ttp => ttp.DaNhapKho) && phieuxuatkho.ListKhoThungTPham.Count > 0) ? 1 : 0;

            phieuxuatkho.isPNKDoneNhapKho = (int.TryParse(phieuxuatkho.IsDonePNK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

            return phieuxuatkho;
        }

        public PhieuNhapKhoTPham GetPhieuNhapKhoTPhamByPalletKey(object? palletkey)
        {
            PhieuNhapKhoTPham phieuxuatkho = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_PhieuNhapKhoTPham}] WHERE [{Common.CodePallet}] = '{palletkey}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = phieuxuatkho.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            phieuxuatkho.ListKhoThungTPham = GetListThungTPs(new() { { ThungTPham.DBName.PNKTPID, phieuxuatkho.PNKTPID.Value } }, false).thungTPhams;

            phieuxuatkho.MaViTri = GetListViTriTPhams(new Dictionary<string, object?>() { { ViTriTPham.DBName.VTTPID, phieuxuatkho.VTTPID.Value } }).viTriTPhams.FirstOrDefault()?.MaViTri.Value ?? string.Empty;

            //foreach (var thung in phieuxuatkho.ListKhoThungTPham)
            //{
            //    if (thung.MaQuanLyThung.Value != null && int.TryParse(thung.VTofTPID.Value?.ToString(), out int vtid))
            //    {
            //        thung.DaNhapKho = vtid > 0;
            //    }
            //}

            phieuxuatkho.IsDonePNK.Value = (phieuxuatkho.ListKhoThungTPham.All(ttp => ttp.DaNhapKho) && phieuxuatkho.ListKhoThungTPham.Count > 0) ? 1 : 0;

            phieuxuatkho.isPNKDoneNhapKho = (int.TryParse(phieuxuatkho.IsDonePNK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

            return phieuxuatkho;
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_PhieuXuatKhoThanhPham
        // Insert
        public (int, string) InsertPhieuXuatKhoTPham(PhieuXuatKhoTPham phieuxuatkhotp)
        {
            int result = -1;
            string errorMess = string.Empty;

            if (phieuxuatkhotp == null) return (result, "Error: is null");

            List<Propertyy> properties = phieuxuatkhotp.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to insert.");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                string columns = string.Join(", ", properties.Select(p => $"[{p.DBName}]"));
                string parameters = string.Join(", ", properties.Select(p => $"@{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));
                command.CommandText = $@"INSERT INTO [{PhieuXuatKhoTPham.DBName.Table_PhieuXuatKhoTPham}] ({columns}) OUTPUT INSERTED.{PhieuXuatKhoTPham.DBName.PXKTPID} VALUES ({parameters})";

                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                object? rs = command.ExecuteScalar();
                if (rs != null && int.TryParse(rs.ToString(), out result) && result > 0)
                {
                    transaction.Commit();
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Update 
        public (int, string) UpdatePhieuXuatKhoTPhamAnyProperty(PhieuXuatKhoTPham phieuxuatkhotp, object? targetproperty = null)
        {
            int result = -1;
            string errorMess = string.Empty;

            // Check for null input
            if (phieuxuatkhotp == null) return (result, "Error: is null");

            List<Propertyy> properties = phieuxuatkhotp.GetPropertiesValues()
                .Where(po => po.AlowDatabase == true && po.Value != null)
                .ToList();

            // Validate properties before proceeding
            if (properties.Count == 0)
            {
                return (result, "Error: No valid properties to update.");
            }

            if (targetproperty != null)
            {
                properties = properties.Where(pro => pro.DBName == targetproperty?.ToString()?.Trim()).ToList();
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction(); // Start transaction

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction; // Associate command with the transaction

                string updateSet = string.Join(", ", properties.Select(p => $"[{p.DBName}] = @{Regex.Replace(p.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $@"UPDATE [{PhieuXuatKhoTPham.DBName.Table_PhieuXuatKhoTPham}] SET {updateSet} WHERE [{PhieuXuatKhoTPham.DBName.PXKTPID}] = '{phieuxuatkhotp.PXKTPID.Value}'";

                // Add parameters
                foreach (var prop in properties)
                {
                    string parameterName = $"@{Regex.Replace(prop.DBName ?? string.Empty, @"[^\w]+", "")}";
                    object? parameterValue = prop.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }

                // Execute command
                result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    // Successfully updated
                    transaction.Commit(); // Commit the transaction
                }
                else
                {
                    result = -1; // Set to -1 if update was not successful
                    errorMess = "No rows were updated. The specified may not exist.";
                }
            }
            catch (Exception ex)
            {
                errorMess = $"Error: {ex.Message}";
                try
                {
                    transaction.Rollback(); // Rollback transaction in case of error
                }
                catch (Exception rollbackEx)
                {
                    errorMess += $" | Rollback Error: {rollbackEx.Message}";
                }
                return (-1, errorMess);
            }

            return (result, errorMess);
        }

        // Get
        public (List<PhieuXuatKhoTPham> phieuXuatKhos, string error) GetListPhieuXuatKhoTPs(Dictionary<string, object?> parameters, bool isgetAll = false)
        {
            List<PhieuXuatKhoTPham> listphieuxuatkhos = new();

            string errorMessage = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var conditions = new List<string>();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM [{PhieuXuatKhoTPham.DBName.Table_PhieuXuatKhoTPham}]";

                    if (!isgetAll)
                    {
                        // Process each parameter in the dictionary
                        foreach (var param in parameters)
                        {
                            conditions.Add($"[{param.Key}] = @{param.Key}");

                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        if (conditions.Any())
                        {
                            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
                        }
                    }

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PhieuXuatKhoTPham pxktp = new();

                        List<Propertyy> rowItems = pxktp.GetPropertiesValues();

                        foreach (var item in rowItems)
                        {
                            string? columnName = item.DBName;

                            if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
                            {
                                object columnValue = reader[columnName];

                                item.Value = columnValue == DBNull.Value ? null : columnValue;
                            }
                        }

                        pxktp.ListKhoThungTPham = GetListThungTPs(new() { { ThungTPham.DBName.PXKTPID, pxktp.PXKTPID.Value } }, false).thungTPhams;

                        pxktp.IsDonePXK.Value = (pxktp.ListKhoThungTPham.All(ttp => ttp.DaXuatKho) && pxktp.ListKhoThungTPham.Count > 0) ? 1 : 0;

                        pxktp.isPXKDoneXuatKho = (int.TryParse(pxktp.IsDonePXK.Value?.ToString(), out int isdone) ? isdone : 0) == 1;

                        listphieuxuatkhos.Add(pxktp);
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: {ex.Message}";
                    listphieuxuatkhos.Clear(); // Clear the list in case of error
                }
            }
            return (listphieuxuatkhos, errorMessage);
        }

        // Delete
        public (bool, string) DeletePhieuXuatKhoTPham(object? pxktpid)
        {
            // Check for valid ID
            if (pxktpid == null)
            {
                return (false, "Error");
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = $"DELETE FROM [{PhieuXuatKhoTPham.DBName.Table_PhieuXuatKhoTPham}] WHERE [{PhieuXuatKhoTPham.DBName.PXKTPID}] = @PXKTPID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PXKTPID", pxktpid);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                return (rowsAffected > 0, string.Empty); // Return true if a row was deleted
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return false and the error message
            }
        }

        #endregion
    }
}




//// Get any columns of PXK by any paramaters
//public (List<object?> columnValues, string errorMessage) GetPXK_AnyColValuebyAnyParameters(Dictionary<string, object?> parameters, string? returnColumnName = null, bool isGetAll = false)
//{
//    List<object?> columnValues = new();
//    string errorMessage = string.Empty;

//    using (var connection = new SqlConnection(connectionString))
//    {
//        try
//        {
//            connection.Open();
//            var conditions = new List<string>();
//            var command = connection.CreateCommand();

//            // If a specific column is requested, select only that column
//            string selectClause = returnColumnName != null
//                ? $"SELECT [{returnColumnName}]"
//                : "SELECT *";

//            command.CommandText = $"{selectClause} FROM [{Common.Table_PhieuXuatKho}]";

//            if (!isGetAll)
//            {
//                // Process each parameter in the dictionary
//                foreach (var param in parameters)
//                {
//                    conditions.Add($"[{param.Key}] = @{param.Key}");
//                    command.Parameters.AddWithValue($"@{param.Key}", param.Value);
//                }
//                if (conditions.Any())
//                {
//                    command.CommandText += " WHERE " + string.Join(" AND ", conditions);
//                }
//            }

//            using var reader = command.ExecuteReader();
//            while (reader.Read())
//            {
//                // If no specific column is requested, read entire object
//                if (returnColumnName == null)
//                {
//                    PhieuXuatKho phieuxuatkho = new();
//                    List<Propertyy> rowItems = phieuxuatkho.GetPropertiesValues();
//                    foreach (var item in rowItems)
//                    {
//                        string? columnName = item.DBName;
//                        if (!string.IsNullOrEmpty(columnName) && reader.GetOrdinal(columnName) != -1)
//                        {
//                            object columnValue = reader[columnName];
//                            item.Value = columnValue == DBNull.Value ? null : columnValue;
//                        }
//                        columnValues.Add(phieuxuatkho);
//                    }
//                }
//                else
//                {
//                    // If a specific column is requested, read only that column
//                    object columnValue = reader[returnColumnName];
//                    columnValues.Add(columnValue == DBNull.Value ? null : columnValue);
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            errorMessage = $"Error: {ex.Message}";
//            columnValues.Clear(); // Clear the list in case of error
//        }
//    }
//    return (columnValues, errorMessage);
//}