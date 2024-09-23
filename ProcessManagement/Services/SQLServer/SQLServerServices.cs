using Microsoft.Data.SqlClient;
using ProcessManagement.Commons;
using ProcessManagement.Models;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.KHO_NVL.NhapKho;
using ProcessManagement.Models.KHO_NVL.Tracking;
using ProcessManagement.Models.KHO_NVL.XuatKho;
using ProcessManagement.Models.MAYMOC;
using System.Data;
using System.Text.RegularExpressions;

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

        // Lay toan bo danh sach nguyen vat lieu
        public List<NVL> GetlistNVL()
        {
            List<NVL> listNVL = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVL}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVL rowNVL = new();

                    List<Propertyy> rowNVLitems = rowNVL.GetPropertiesValues();

                    foreach (var item in rowNVLitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listNVL.Add(rowNVL);
                }
            }

            return listNVL;
        }

        // Lay danh sach nguyen vat lieu theo Lot
        public List<NVL> GetlistNVLperLot(Lot? lot)
        {
            List<NVL> listNVL = new();

            if (lot == null) { return listNVL; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVL}] WHERE [{Common.LotNVL}] = '{lot.LotNVL.Value}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVL rowNVL = new();

                    List<Propertyy> rowNVLitems = rowNVL.GetPropertiesValues();

                    foreach (var item in rowNVLitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listNVL.Add(rowNVL);
                }
            }

            return listNVL;
        }

        // Lay danh sach NVL theo ma san pham
        public List<NVL> GetlistNVLperSanPham(SanPham? sanpham)
        {
            List<NVL> listNVL = new();

            if (sanpham == null || sanpham.MaSP.Value == null) { return listNVL; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVL}] WHERE [{Common.MaSP}] = '{sanpham.MaSP.Value}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVL rowNVL = new();

                    List<Propertyy> rowNVLitems = rowNVL.GetPropertiesValues();

                    foreach (var item in rowNVLitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listNVL.Add(rowNVL);
                }
            }

            return listNVL;
        }

        // Lay danh sach nguyen vat lieu
        public List<NVL> GetlistNVLperLot(string lotNVL)
        {
            List<NVL> listNVL = new();

            if (lotNVL == string.Empty) { return listNVL; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVL}] WHERE [{Common.LotNVL}] = '{lotNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVL rowNVL = new();

                    List<Propertyy> rowNVLitems = rowNVL.GetPropertiesValues();

                    foreach (var item in rowNVLitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listNVL.Add(rowNVL);
                }
            }

            return listNVL;
        }

        // Load danh sach NVL by loai nl
        public List<NVL> GetlistNVLbyLoaiNL(string loaiNL)
        {
            List<NVL> listNVL = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVL}] WHERE [{Common.LoaiNL}] = '{loaiNL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVL rowNVL = new();

                    List<Propertyy> rowNVLitems = rowNVL.GetPropertiesValues();

                    foreach (var item in rowNVLitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listNVL.Add(rowNVL);
                }
            }

            return listNVL;
        }

        // Them moi nguyen vat lieu
        public (int, string) InsertNewNguyenvatlieu(NVL newNVL)
        {
            List<Propertyy> newNVLItems = newNVL.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1;
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newNVLItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newNVLItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableNVL}] ({columnNames}) OUTPUT INSERTED.{Common.NVLID} VALUES ({parameterNames})";

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

        // Lay danh sach Lot
        public List<Lot> GetlistLot()
        {
            List<Lot> listLot = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableLot}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Lot rowLot = new();

                    List<Propertyy> rowLotitems = rowLot.GetPropertiesValues();

                    foreach (var item in rowLotitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listLot.Add(rowLot);
                }
            }

            Parallel.ForEach(listLot, lot =>
            {
                int lotNLcount = GetTotalNVLperLot(lot.LotNVL.Value?.ToString() ?? string.Empty).Item1;

                if (lotNLcount >= 0)
                {
                    lot.SoLuongNL.Value = lotNLcount;
                }
            });

            return listLot;
        }

        // Them moi Lot
        public (int, string) InsertNewLot(Lot newLot)
        {
            List<Propertyy> newLotItems = newLot.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1;
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newLotItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newLotItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableLot}] ({columnNames}) OUTPUT INSERTED.{Common.LotID} VALUES ({parameterNames})";

                foreach (var item in newLotItems)
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

        // Load so luong NVL moi lot
        public (int, string) GetTotalNVLperLot(string lotNL)
        {
            if (lotNL == string.Empty) { return (-1, "Lot NL is empty"); }

            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT COUNT(*) FROM [{Common.TableNVL}] WHERE [{Common.LotNVL}] = '{lotNL}'";

                int count = Convert.ToInt32(command.ExecuteScalar());

                return (count, errorMess);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }
        }

        // ------------------------------------------------------------------------------------- //
        #region Table_SanPham
        // Lay danh sach san pham
        public List<SanPham> GetlistSanphams()
        {
            List<SanPham> listSanphams = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableSanPham}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    SanPham rowSP = new();

                    List<Propertyy> rowSPitems = rowSP.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listSanphams.Add(rowSP);
                }
            }

            Parallel.ForEach(listSanphams, sp =>
            {
                sp.ChitietSanPhams = GetDSachChitietSanPham(sp);
                sp.DanhSachNVLs = GetDSachNVLofSanPham(sp.SPID.Value);
            });

            return listSanphams;
        }

        // Lay san pham by sp id
        public SanPham GetSanpham(int spid) // Lay danh sach san pham
        {
            SanPham sanpham = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableSanPham}] WHERE [{Common.SPID}] = '{spid}'";

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

            sanpham.ChitietSanPhams = GetDSachChitietSanPham(sanpham);
            sanpham.DanhSachNVLs = GetDSachNVLofSanPham(sanpham.SPID.Value);

            return sanpham;
        }

        // Load danh sach chi tiet san pham
        public List<SanPham.ChitietSanPham> GetDSachChitietSanPham(SanPham sanPham)
        {
            List<SanPham.ChitietSanPham> listChitietSanphams = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableChitietSanPham}] WHERE [{Common.SPID}] = '{sanPham.SPID.Value}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    SanPham.ChitietSanPham rowSP = new();

                    List<Propertyy> rowSPitems = rowSP.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    listChitietSanphams.Add(rowSP);
                }
            }

            return listChitietSanphams;
        }

        // Kiem tra newchitietsp da co hay chua 
        public (int, string) CheckNewChitietSanpham(SanPham.ChitietSanPham newChitietSP)
        {
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT COUNT(*) FROM [{Common.TableChitietSanPham}] WHERE [{Common.SPID}] = '{newChitietSP.SPID.Value}' AND [{Common.PropertyName}] = '{newChitietSP.PropertyName.Value}'";

                int count = Convert.ToInt32(command.ExecuteScalar());

                return (count, errorMess);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            // chua co return > 0
        }

        // Them moi chi tiet san pham
        public (int, string) InsertNewSanPham(SanPham newSanpham)
        {
            List<Propertyy> newspItems = newSanpham.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newspItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newspItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableSanPham}] ({columnNames}) OUTPUT INSERTED.{Common.SPID} VALUES ({parameterNames})";

                foreach (var item in newspItems)
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

        // Them moi chi tiet san pham
        public (int, string) InsertNewChitietSanPham(SanPham.ChitietSanPham newchitietSP)
        {
            List<Propertyy> newctspItems = newchitietSP.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newctspItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newctspItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableChitietSanPham}] ({columnNames}) OUTPUT INSERTED.{Common.CTSPID} VALUES ({parameterNames})";

                foreach (var item in newctspItems)
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

        // Update danh sach chi tiet san pham
        public (int, string) UpdateDanhSachChitietSanpham(List<SanPham.ChitietSanPham>? chitietSanphamList)
        {
            int result = -1; string errorMess = string.Empty;

            if (chitietSanphamList == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                foreach (var chitietSanpham in chitietSanphamList)
                {
                    List<Propertyy> chitietItems = chitietSanpham.GetPropertiesValues().Where(pro => pro.AlowDatabase == true).ToList();

                    var command = connection.CreateCommand();

                    string setClause = string.Join(",", chitietItems.Select(key => $"[{key.DBName}] = @{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                    command.CommandText = $"UPDATE [{Common.TableChitietSanPham}] SET {setClause} WHERE [{Common.CTSPID}] = '{chitietSanpham.CTSPID.Value}'";

                    foreach (var item in chitietItems)
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

        // Delete san pham
        public (int, string) DeleteSanpham(SanPham? removeSanpham)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeSanpham == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.TableSanPham} WHERE [{Common.SPID}] = '{removeSanpham.SPID.Value}'";

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

        // Delete chi tiet san pham
        public (int, string) DeleteChitietSanPham(SanPham.ChitietSanPham? removeChitiet)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeChitiet == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.TableChitietSanPham} WHERE [{Common.CTSPID}] = '{removeChitiet.CTSPID.Value}'";

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

        #endregion Table_SanPham

        // ------------------------------------------------------------------------------------- //
        #region Table_KHSX
        // Get last ke hoach san xuat
        public KHSX GetLastKHSX()
        {
            KHSX lastkhsx = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT TOP 1 * FROM [{Common.TableKHSX}] ORDER BY [{Common.KHSXID}] DESC";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowSPitems = lastkhsx.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            return lastkhsx;
        }

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

        // Load ke hoach san xuat by ID
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

                khsx.SanPham = GetSanpham(int.TryParse(khsx.SPID.Value?.ToString(), out int spid) ? spid : 0);

                khsx.LoaiNVL = GetLoaiNVLbyID(int.TryParse(khsx.LOAINVLID.Value?.ToString(), out int loainvlid) ? loainvlid : 0);

                khsx.DSachNVLs = GetListNVLofKHSXbyID(khsx.KHSXID.Value);

                khsx.DSachCongDoans = GetlistCongdoans(khsx.KHSXID.Value);
            }

            return khsx;
        }

        // Load danh sach ke hoach san xuat
        public List<KHSX> GetListKHSXs()
        {
            List<KHSX> listKHSXs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableKHSX}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    KHSX khsx = new();

                    List<Propertyy> rowSPitems = khsx.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    khsx.SanPham = GetSanpham(int.Parse(khsx.SPID.Value?.ToString() ?? "0"));

                    khsx.LoaiNVL = GetLoaiNVLbyID(int.TryParse(khsx.LOAINVLID.Value?.ToString(), out int loainvlid) ? loainvlid : 0);

                    khsx.DSachNVLs = GetListNVLofKHSXbyID(khsx.KHSXID.Value);

                    khsx.DSachCongDoans = GetlistCongdoans(khsx.KHSXID.Value);

                    listKHSXs.Add(khsx);
                }
            }

            return listKHSXs;
        }

        // Load danh sach ke hoach san xuat theo ngay
        public List<KHSX> GetListKHSXs(DateTime startDay, DateTime endDay)
        {
            List<KHSX> listKHSXs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableKHSX}] WHERE [{Common.NgayTao}] >= '{startDay}' AND [{Common.NgayTao}] < '{endDay}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    KHSX khsx = new();

                    List<Propertyy> rowSPitems = khsx.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    khsx.SanPham = GetSanpham(int.Parse(khsx.SPID.Value?.ToString() ?? "0"));

                    khsx.LoaiNVL = GetLoaiNVLbyID(int.TryParse(khsx.LOAINVLID.Value?.ToString(), out int loainvlid) ? loainvlid : 0);

                    khsx.DSachNVLs = GetListNVLofKHSXbyID(khsx.KHSXID.Value);

                    khsx.DSachCongDoans = GetlistCongdoans(khsx.KHSXID.Value);

                    listKHSXs.Add(khsx);
                }
            }

            return listKHSXs;
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

                command.CommandText = $"INSERT INTO [{Common.TableCongDoan}] ({columnNames}) OUTPUT INSERTED.{Common.CDID} VALUES ({parameterNames})";

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

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table_NguyenCongofKHSX
        // Load danh sach cong doan
        public List<NguyenCongofKHSX> GetlistCongdoans(object? khsxID)
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

                    listCongdoans.Add(rowSP);
                }
            }

            Parallel.ForEach(listCongdoans, congdoan =>
            {
                congdoan.IsUsing = true;

                congdoan.DSachNVLCongDoans = GetlistNVLmoiCongdoans(congdoan.CDID.Value ?? 0, khsxID);
            });

            return listCongdoans;
        }

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

                command.CommandText = $"SELECT * FROM [{Common.TableNVLmoiCongDoan}] WHERE [{Common.KHSXID}] = '{khsxID}' AND [{Common.CDID}] = '{congdoanID}'";

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

        // Get NVL moi cong doan by ID
        public NVLmoiNguyenCong? GetlistNVLmoiCongdoanbyID(object? nvlmcdID)
        {
            if (nvlmcdID == null) return null;

            NVLmoiNguyenCong nvlmoiCongdoan = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVLmoiCongDoan}] WHERE [{Common.NVLMCDID}] = '{nvlmcdID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowSPitems = nvlmoiCongdoan.GetPropertiesValues();

                    foreach (var item in rowSPitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }
                }
            }

            List<CaLamViec> listCaLamviec = GetlistNVLmoiCongdoanCalamviecs(nvlmoiCongdoan.NVLMCDID.Value);

            nvlmoiCongdoan.CaNgay = listCaLamviec.FirstOrDefault(ca => ca.Ca?.Value?.ToString() == Common.Cangay) ?? new CaLamViec();

            nvlmoiCongdoan.CaDem = listCaLamviec.FirstOrDefault(ca => ca.Ca?.Value?.ToString() == Common.Cadem) ?? new CaLamViec();

            return nvlmoiCongdoan;
        }

        // Update NG/OK nvlmoi cong doan
        public (int, string) UpdateNVLmoiCongdoan(int slOK, int slNG, string maquanly, object? nvlmcdid, object? cdID, object? khsxID)
        {
            int result = -1; string errorMess = string.Empty;

            if (slOK == 0 || maquanly == string.Empty || nvlmcdid == null || cdID == null || khsxID == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.TableNVLmoiCongDoan} SET [{Common.TongOK}] = '{slOK}', [{Common.TongNG}] = '{slNG}', [{Common.IsUpdated}] = '{1}' " +
                                    $"WHERE [{Common.KHSXID}] = '{khsxID}' AND [{Common.CDID}] = '{cdID}' AND [{Common.MaQuanLy}] = '{maquanly}'";

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

                    string sqlQuery = $"UPDATE {Common.TableNVLmoiCongDoan} SET [{Common.SLTruocGiaCong}] = '{sltruocgiacong}' WHERE [{Common.CDID}] = '{cdid}' AND [{Common.MaQuanLy}] = '{maquanly}'";

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

        #endregion Table_NguyenCongofKHSX

        // ------------------------------------------------------------------------------------- //
        #region Table_NguyenCong
        // Kiem tra nguyen cong da ton tai
        public (int, string) NguyencongDatontai(NguyenCong nguyenCong)
        {
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT COUNT(*) FROM [{Common.TableNguyenCong}] WHERE [{Common.NguyenCong}] = N'{nguyenCong.TenNguyenCong.Value}'";

                int count = Convert.ToInt32(command.ExecuteScalar());

                return (count, errorMess);
            }
            catch (Exception ex)
            {
                errorMess = ex.Message;

                return (-1, errorMess);
            }

            // chua co return > 0
        }

        // Them moi nguyen cong
        public (int, string) InsertNewNguyenCong(NguyenCong newNC)
        {
            List<Propertyy> newNCItems = newNC.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newNCItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newNCItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableNguyenCong}] ({columnNames}) OUTPUT INSERTED.{Common.NCID} VALUES ({parameterNames})";

                foreach (var item in newNCItems)
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

        // Get nguyen cong
        public NguyenCong GetNguyenCong(object ncid)
        {
            NguyenCong nguyencong = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenCong}] WHERE [{Common.NCID}] = '{ncid}'";

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

            return nguyencong;
        }

        // Load danh sach nguyen cong
        public List<NguyenCong> GetListNguyenCongs()
        {
            List<NguyenCong> lisNCs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenCong}]";

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

                    lisNCs.Add(nguyencong);
                }
            }

            return lisNCs;
        }
        #endregion Table_NguyenCong

        // ------------------------------------------------------------------------------------- //
        #region Table_NVLofSanPham
        // Get danh sach NVL cua san pham
        public List<NVLofSanPham> GetDSachNVLofSanPham(object? spID)
        {
            List<NVLofSanPham> listNVLofSanphams = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVLofSanPham}] WHERE [{Common.SPID}] = '{spID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLofSanPham nvlofsp = new();

                    List<Propertyy> rowitems = nvlofsp.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    // Load target nguyen vat lieu
                    nvlofsp.TargetNgLieu = GetNguyenVatLieuByID(nvlofsp.NVLID.Value);

                    listNVLofSanphams.Add(nvlofsp);
                }
            }

            return listNVLofSanphams;
        }

        // Get NVL of SanPham by ID // Table Table_NVLofSanPham //
        public NVLofSanPham GetNVLofSPbyID(int nvlspid)
        {
            NVLofSanPham nvlsp = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNVLofSanPham}] WHERE [{Common.NVLID}] = '{nvlspid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nvlsp.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            return nvlsp;
        }

        // Kiem tra da ton tai NVL trong ds NVL cua san pham // Table Table_NVLofSanPham //
        public bool IsNVLofSanPhamExisting(NVLofSanPham nVLofSanPham)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.TableNVLofSanPham}] WHERE [{Common.SPID}] = '{nVLofSanPham.SPID.Value}' AND [{Common.NVLID}] = '{nVLofSanPham.NVLID.Value}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Them loai NVL cho san pham // Table Table_NVLofSanPham //
        public (int, string) InsertNewNguyenVatLieuchoSanPham(NVLofSanPham newnvl)
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

                command.CommandText = $"INSERT INTO [{Common.TableNVLofSanPham}] ({columnNames}) OUTPUT INSERTED.{Common.NVLID} VALUES ({parameterNames})";

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

        // Xoa NVL cua SP // Table Table_NVLofSanPham //
        public (int, string) DeleteNVLofSanpham(NVLofSanPham? removeNVLofSP)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeNVLofSP == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.TableNVLofSanPham} WHERE [{Common.NVLID}] = '{removeNVLofSP.NVLSPID.Value}'";

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

        #endregion Table_NVLofSanPham

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

                    listnvls.Add(nvl);
                }
            }

            return listnvls;
        }

        #endregion

        // ------------------------------------------------------------------------------------- //
        #region Table KHO_NguyenVatLieu
        // Them loai NVL moi 
        public (int, string) InsertNewLoaiNguyenVatLieu(NguyenVatLieu? newnvl)
        {
            int result = -1; string errorMess = string.Empty;

            if (newnvl == null) return (result, "Error");

            List<Propertyy> newNVLItems = newnvl.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newNVLItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newNVLItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableNguyenVatLieu}] ({columnNames}) OUTPUT INSERTED.{Common.NVLID} VALUES ({parameterNames})";

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

        // Update so luong ton kho NVL 
        public (int, string) UpdateSLTonkhoNguyenVatLieu_XuatKho(object? nvlid, object? soluongLay)
        {
            int result = -1; string errorMess = string.Empty;

            if (soluongLay == null || nvlid == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.TableNguyenVatLieu} SET [{Common.NVLTonKho}] = [{Common.NVLTonKho}] - '{soluongLay}' WHERE [{Common.NVLID}] = '{nvlid}'";

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

        // Check Ten NVL da ton tai 
        public bool IsTenNVLExists(string? tenNVL)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.TenNVL}] = N'{tenNVL}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Get Nguyen Vat Lieu by ID 
        public NguyenVatLieu GetNguyenVatLieuByID(object? nvlID)
        {
            NguyenVatLieu nvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.NVLID}] = '{nvlID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
                    }

                }
            }

            // Load Details NVL 
            nvl.DSNguyenVatLieuDetails = GetNguyenVatLieuDetails(nvl.NVLID.Value);
            // Load Danh muc
            nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
            // Load Loai NVL
            nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
            // Load list vi tri 
            nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
            // Tinh so luong ton kho
            nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

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

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.TenNVL}] = '{tenNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
                    }

                }
            }

            // Load Details NVL 
            nvl.DSNguyenVatLieuDetails = GetNguyenVatLieuDetails(nvl.NVLID.Value);
            // Load Danh muc
            nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
            // Load Loai NVL
            nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
            // Load list vi tri 
            nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
            // Tinh so luong ton kho
            nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

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

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.LOAINVLID}] = '{loainvlID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenVatLieu nvl = new();

                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    // Load Details NVL 
                    nvl.DSNguyenVatLieuDetails = GetNguyenVatLieuDetails(nvl.NVLID.Value);
                    // Load Danh muc
                    nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
                    // Load Loai NVL
                    nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
                    // Load list vi tri 
                    nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
                    // Tinh so luong ton kho
                    nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

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

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.DMID}] = '{dmucID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenVatLieu nvl = new();

                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    // Load Details NVL 
                    nvl.DSNguyenVatLieuDetails = GetNguyenVatLieuDetails(nvl.NVLID.Value);
                    // Load Danh muc
                    nvl.DanhMuc = GetDanhMucbyID(nvl.DMID.Value);
                    // Load Loai NVL
                    nvl.LoaiNVL = GetLoaiNVLbyID(nvl.LOAINVLID.Value);
                    // Load list vi tri 
                    nvl.DSViTri = GetListViTriOfNgVatLieuByNVLid(nvl.NVLID.Value);
                    // Tinh so luong ton kho
                    nvl.TonKho = nvl.DSViTri.Sum(vitri => int.TryParse(vitri.VTNVLSoLuong.Value?.ToString(), out int slvt) ? slvt : 0);

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

                command.CommandText = $"SELECT [{Common.NVLID}] FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.TenNVL}] = '{tenNVL}'";

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

                command.CommandText = $"SELECT [{Common.NVLID}] FROM [{Common.TableNguyenVatLieu}]";

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

                command.CommandText = $"UPDATE [{Common.TableNguyenVatLieu}] SET {setClause} WHERE [{Common.NVLID}] = '{ngVatLieu.NVLID.Value}'";

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
        #region Table_KHO_NguyenLieuDetails

        // Kiểm tra thông tin đã tồn tại trong danh sách nvl details
        public bool IsDetailExistingInNVLDetailsList(object? tenttid, object? nvlid)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.Table_NguyenLieuDetails}] WHERE [{Common.TenTTID}] = N'{tenttid}' AND [{Common.NVLID}] = {nvlid}";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Load thong tin of nguyen vat lieu
        public List<NguyenVatLieuDetail> GetNguyenVatLieuDetails(object? nvlid)
        {
            List<NguyenVatLieuDetail> nvlDetails = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NguyenLieuDetails}] WHERE [{Common.NVLID}] = '{nvlid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenVatLieuDetail detail = new();

                    List<Propertyy> rowitems = detail.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    // Load ten thong tin 
                    detail.NVLDetailItems = GetNVLdetailsNamebyID(detail.TenTTID.Value);

                    nvlDetails.Add(detail);
                }
            }

            return nvlDetails;
        }

        // Thêm trường thông tin chi tiết mới cho nguyên vật liệu
        public (int, string) InsertNewNguyenVatLieuDetail(NguyenVatLieuDetail newnvldetail)
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

                command.CommandText = $"INSERT INTO [{Common.Table_NguyenLieuDetails}] ({columnNames}) OUTPUT INSERTED.{Common.TTNVLID} VALUES ({parameterNames})";

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

        // Xóa trường thông tin của nguyên vật liệu
        public (int, string) DeleteThongTinNgVatLieu(NguyenVatLieuDetail? removeNVLdetail)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeNVLdetail == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_NguyenLieuDetails} WHERE [{Common.TTNVLID}] = '{removeNVLdetail.TTNVLID.Value}'";

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

        // Xóa trường thông tin ở các nguyên vật liệu khác
        public (int, string) DeleteThongTinNgVatLieuByTenTTID(object? tenttID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_NguyenLieuDetails} WHERE [{Common.TenTTID}] = '{tenttID}'";

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
        public (int, string) UpdateListNguyenVatLieuDetails(List<NguyenVatLieuDetail> ngvatlieudetails)
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

                    command.CommandText = $"UPDATE [{Common.Table_NguyenLieuDetails}] SET {setClause} WHERE [{Common.TTNVLID}] = '{nvldetail.TTNVLID.Value}'";

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
        public List<NVLDetailsName> GetListNVLdetailsName()
        {
            List<NVLDetailsName> names = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NVLDetailsListName}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NVLDetailsName name = new();

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
        public NVLDetailsName GetNVLdetailsNamebyID(object? tenttID)
        {
            NVLDetailsName detailname = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.Table_NVLDetailsListName}] WHERE [{Common.TenTTID}] = '{tenttID}'";

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
                string query = $"SELECT COUNT(*) FROM [{Common.Table_NVLDetailsListName}] WHERE [{Common.TenThongTin}] = N'{tenNVLDetail}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Them NVL detail name moi
        public (int, string) InsertNewNVLDetailName(NVLDetailsName newnvldetailName)
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

                command.CommandText = $"INSERT INTO [{Common.Table_NVLDetailsListName}] ({columnNames}) OUTPUT INSERTED.{Common.TenTTID} VALUES ({parameterNames})";

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

        // Xóa loại thông tin trong bảng danh sách thông tin NVL
        public (int, string) DeleteNVLDetailName(object? tenttID)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.Table_NVLDetailsListName} WHERE [{Common.TenTTID}] = '{tenttID}'";

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

        // Thay đổi tên của trường thông tin nguyên vật liệu
        public (int, string) UpdateNVLDetailName(object? tenttid, string newName, string tentruyxuat)
        {
            int result = -1; string errorMess = string.Empty;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.Table_NVLDetailsListName} SET [{Common.TenThongTin}] = N'{newName}', [{Common.TenTruyXuat}] = '{tentruyxuat}' WHERE [{Common.TenTTID}] = '{tenttid}' ";

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

        // Tính số lượng trống còn lại của vị trí
        public (int, string) GetViTriLuuTruSoLuongTrong(object? vtltID)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = $"SELECT SUM(CAST([{Common.SoLuong}] AS INT)) AS TongSoLuong FROM [{Common.TableVitriOfNVL}] WHERE [{Common.VTID}] = '{vtltID}'";

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

                command.CommandText = $"SELECT [{Common.VTID}] FROM [{Common.TableViTriLuuTru}] WHERE [{Common.MaViTri}] = '{mavitri}'";

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

                command.CommandText = $"SELECT * FROM [{Common.TableViTriLuuTru}] WHERE [{Common.VTID}] = '{vtltID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
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

                command.CommandText = $"SELECT * FROM [{Common.TableViTriLuuTru}] WHERE [{Common.MaViTri}] = '{mavitri}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
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

                command.CommandText = $"SELECT * FROM [{Common.TableViTriLuuTru}]";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    VitriLuuTru vitri = new();

                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
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

                    string sqlQuery = $"UPDATE {Common.TableViTriLuuTru} SET [{Common.VTSLTrong}] = ([{Common.VTSLTrong}] - '{slnhapkho}') WHERE [{Common.VTID}] = '{vtid}'";

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
        #region Table_KHO_ViTriOfNVL

        // Lấy vitriofNVL by NVLid và VTid
        public ViTriofNVL GetViTriOfNgVatLieuByNVLid_VTid(object? nvlid, object? vtid)
        {
            ViTriofNVL vitriofnvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableVitriOfNVL}] WHERE [{Common.NVLID}] = '{nvlid}' AND [{Common.VTID}] = '{vtid}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = vitriofnvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
                    }
                }
            }

            // Get vitriluutru infor
            vitriofnvl.VitriInfor = GetViTriLuuTruByID(vitriofnvl.VTID.Value);
            vitriofnvl.NgLieuInfor = GetNguyenVatLieuByID(vitriofnvl.NVLID.Value);

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

                command.CommandText = $"SELECT * FROM [{Common.TableVitriOfNVL}] WHERE [{Common.NVLID}] = '{nvlID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ViTriofNVL vitri = new();

                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
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

                command.CommandText = $"SELECT * FROM [{Common.TableVitriOfNVL}] WHERE [{Common.VTID}] = '{vtID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ViTriofNVL vitri = new();

                    List<Propertyy> rowitems = vitri.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
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

                command.CommandText = $"INSERT INTO [{Common.TableVitriOfNVL}] ({columnNames}) OUTPUT INSERTED.{Common.VTofNVLID} VALUES ({parameterNames})";

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

                command.CommandText = $"UPDATE [{Common.TableVitriOfNVL}] SET {setClause} WHERE [{Common.VTofNVLID}] = '{vitriofNVL.VTofNVLID.Value}'";

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

                    string sqlQuery = $"UPDATE {Common.TableVitriOfNVL} SET [{Common.SoLuong}] = '{tonkho}' WHERE [{Common.VTofNVLID}] = '{vitriofnvlId}'";

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

                command.CommandText = $"DELETE FROM {Common.TableVitriOfNVL} WHERE [{Common.VTofNVLID}] = '{vtofnvllid}'";

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

                command.CommandText = $"SELECT * FROM [{Common.TableDanhMucNVL}]";

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

                command.CommandText = $"SELECT * FROM [{Common.TableDanhMucNVL}] WHERE [{Common.DMID}] = '{danhmucID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = dmnvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
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

                command.CommandText = $"INSERT INTO [{Common.TableDanhMucNVL}] ({columnNames}) OUTPUT INSERTED.{Common.DMID} VALUES ({parameterNames})";

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
                string query = $"SELECT COUNT(*) FROM [{Common.TableDanhMucNVL}] WHERE [{Common.TenDanhMuc}] = N'{tendmnvl}'";

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

                command.CommandText = $"SELECT * FROM [{Common.TableLoaiNVL}] WHERE [{Common.LOAINVLID}] = '{maloaiNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = loainvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue.ToString()?.Trim();
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

                command.CommandText = $"SELECT * FROM [{Common.TableLoaiNVL}]";

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

                command.CommandText = $"SELECT * FROM [{Common.TableLoaiNVL}] WHERE [{Common.DMID}] = '{dmid}'";

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
                string query = $"SELECT COUNT(*) FROM [{Common.TableLoaiNVL}] WHERE [{Common.TenLoaiNVL}] = N'{tenloainvl}'";

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

                command.CommandText = $"INSERT INTO [{Common.TableLoaiNVL}] ({columnNames}) OUTPUT INSERTED.{Common.LOAINVLID} VALUES ({parameterNames})";

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

                command.CommandText = $"DELETE FROM {Common.TableLoaiNVL} WHERE [{Common.LOAINVLID}] = '{removeloainvl.LOAINVLID.Value}'";

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

                        item.Value = columnValue;
                    }

                }
            }

            // Load danh sach nguyen vat lieu pnk
            phieunhapkho.DSNVLofPNKs = GetListNVLofPNKs(pnkid);

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

                        item.Value = columnValue.ToString()?.Trim();
                    }

                }
            }

            // Load danh sach nguyen vat lieu pnk
            phieunhapkho.DSNVLofPNKs = GetListNVLofPNKs(phieunhapkho.PNKID.Value);

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

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    // Load danh sach nguyen vat lieu pnk
                    phieunhapkho.DSNVLofPNKs = GetListNVLofPNKs(phieunhapkho.PNKID.Value);

                    // Check PNK isdone
                    foreach (var nvlofpnk in phieunhapkho.DSNVLofPNKs)
                    {
                        if (!nvlofpnk.IsNhapKhoDone)
                        {
                            phieunhapkho.IsPXKDoneNhapKho = false; break;
                        }
                        else if (nvlofpnk.IsNhapKhoDone)
                        {
                            phieunhapkho.IsPXKDoneNhapKho = true;
                        }
                    }

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

                        item.Value = columnValue.ToString()?.Trim();
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
                    nvlpnk.TargetNgLieu = GetNguyenVatLieuByID(nvlpnk.NVLID.Value);

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
            lnkho.TargetNgLieu = GetNguyenVatLieuByID(lnkho.NVLID.Value);

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

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    // Get vitriluutru infor
                    lenhnk.TargertVitri = GetViTriLuuTruByID(lenhnk.VTID.Value);

                    // Get ng vat lieu nhap kho
                    lenhnk.TargetNgLieu = GetNguyenVatLieuByID(lenhnk.NVLID.Value);

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

                        item.Value = columnValue.ToString()?.Trim();
                    }
                }
            }

            // Load danh sach nguyen vat lieu pxk
            phieuxuatkho.DSNVLofPXKs = GetListNVLofPXKs(pxkid);

            // Check PXK isdone
            foreach (var nvlofpxk in phieuxuatkho.DSNVLofPXKs)
            {
                if (!nvlofpxk.IsXuatKhoDone)
                {
                    phieuxuatkho.IsPXKDoneXuatKho = false; break;
                }
                else if (nvlofpxk.IsXuatKhoDone)
                {
                    phieuxuatkho.IsPXKDoneXuatKho = true;
                }
            }
            return phieuxuatkho;
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

                        item.Value = columnValue.ToString()?.Trim();
                    }
                }
            }
            // Load danh sach nguyen vat lieu pxk
            phieuxuatkho.DSNVLofPXKs = GetListNVLofPXKs(phieuxuatkho.PXKID.Value);

            // Check PXK isdone
            foreach (var nvlofpxk in phieuxuatkho.DSNVLofPXKs)
            {
                if (!nvlofpxk.IsXuatKhoDone)
                {
                    phieuxuatkho.IsPXKDoneXuatKho = false; break;
                }
                else if (nvlofpxk.IsXuatKhoDone)
                {
                    phieuxuatkho.IsPXKDoneXuatKho = true;
                }
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

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    // Load danh sach nguyen vat lieu pxk
                    phieuxuatkho.DSNVLofPXKs = GetListNVLofPXKs(phieuxuatkho.PXKID.Value);

                    // Check PXK isdone
                    foreach (var nvlofpxk in phieuxuatkho.DSNVLofPXKs)
                    {
                        if (!nvlofpxk.IsXuatKhoDone)
                        {
                            phieuxuatkho.IsPXKDoneXuatKho = false; break;
                        }
                        else if (nvlofpxk.IsXuatKhoDone)
                        {
                            phieuxuatkho.IsPXKDoneXuatKho = true;
                        }
                    }

                    dsPXKho.Add(phieuxuatkho);
                }
            }

            return dsPXKho;
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

                        item.Value = columnValue.ToString()?.Trim();
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
                    nvlpxk.TargetNgLieu = GetNguyenVatLieuByID(nvlpxk.NVLID.Value);

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

                command.CommandText = $"SELECT * FROM [{Common.Table_LenhXuatKho}] WHERE [{Common.PXKID}] = '{inputLXK.PXKID.Value}' AND [{Common.NVLID}] = '{inputLXK.NVLID.Value}' AND [{Common.VTID}] = '{inputLXK.VTID.Value}'";

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

                        item.Value = columnValue.ToString()?.Trim();
                    }

                    // Get vitriluutru infor
                    //lenhxk.TagertVitri = GetViTriLuuTruByID(lenhxk.VTID.Value);

                    // Get vitriofNVL infor
                    lenhxk.ViTriofNVL = GetViTriOfNgVatLieuByNVLid_VTid(lenhxk.NVLID.Value, lenhxk.VTID.Value);

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

                        item.Value = columnValue.ToString()?.Trim();
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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
                        }
                    }
                }
            }

            // Get danh sach thong tin may moc
            mayMoc.DSThongTin = GetDanhSachThongTinMayMoc(mayMoc.MMID.Value);

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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
                        }
                    }

                    danhSachLoaiThongTinMayMoc.Add(loaithongTinMayMoc);
                }
            }

            return danhSachLoaiThongTinMayMoc;
        }

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

                            item.Value = columnValue == DBNull.Value ? null : columnValue.ToString()?.Trim();
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
    }
}
