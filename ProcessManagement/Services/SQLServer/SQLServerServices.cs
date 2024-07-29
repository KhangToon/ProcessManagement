using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using ProcessManagement.Commons;
using ProcessManagement.Models;
using ProcessManagement.Models.KHO_NVL;
using System.Collections.Generic;
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

        // Lay danh sach NVL theo ma san pham
        public List<NguyenLieu> GetlistNguyenLieuperSanPham(SanPham? sanpham)
        {
            List<NguyenLieu> listNVL = new();

            if (sanpham == null || sanpham.MaSP.Value == null) { return listNVL; }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenLieu}] WHERE [{Common.MaSP}] = '{sanpham.MaSP.Value}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenLieu rowNVL = new();

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

        // Them moi Lot
        public (int, string) InsertNewNLforSanpham(NguyenLieu newnl)
        {
            List<Propertyy> newLotItems = newnl.GetPropertiesValues().Where(po => po.AlowDatabase == true && po.Value != null).ToList();

            int result = -1;
            string errorMess = string.Empty;

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                string columnNames = string.Join(",", newLotItems.Select(key => $"[{key.DBName}]"));

                string parameterNames = string.Join(",", newLotItems.Select(key => $"@{Regex.Replace(key.DBName ?? string.Empty, @"[^\w]+", "")}"));

                command.CommandText = $"INSERT INTO [{Common.TableNguyenLieu}] ({columnNames}) OUTPUT INSERTED.{Common.NLID} VALUES ({parameterNames})";

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

        // Insert new Ke hoach san xuat
        public (int, string) InsertNewKehoachSanxuat(KeHoachSX newKHSX)
        {
            int result = -1;
            string errorMess = string.Empty;

            object? spid = newKHSX.SanPham?.SPID.Value;
            int slsanxuat = newKHSX.SoluongSX;
            object? masp = newKHSX.SanPham?.MaSP.Value;

            if (spid == null && slsanxuat == 0) return (result, "Sản phẩm ID null, SL sản xuất bằng 0");

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"INSERT INTO [{Common.TableKHSX}] ([{Common.SPID}], [{Common.SLSanXuat}], [{Common.MaSP}]) OUTPUT INSERTED.{Common.KHSXID} VALUES ('{spid}', '{slsanxuat}', '{masp}')";

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

                khsx.LoaiNVL = GetLoaiNVLbyID(int.TryParse(khsx.MaLoaiNVL.Value?.ToString(), out int loainvlid) ? loainvlid : 0);

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
        public (int, string) UpdateCalamviec2(NVLmoiNguyenCong? nVLmoiCong, string calamviec, int slOK)
        {
            int result = -1; string errorMess = string.Empty;

            if (nVLmoiCong == null) { return (result, errorMess); }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = $"UPDATE {Common.TableCalamviec} SET [{Common.OK}] = '{slOK}', [{Common.NgayGiaCong}] = '{DateTime.Now}' " +
                                    $"WHERE [{Common.NVLMCDID}] = '{nVLmoiCong.NVLMCDID.Value}' AND [{Common.Ca}] = '{calamviec}'";

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

        // Delete Ke hoach san xuat
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

        // Nguyen cong ///////////
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

        // Table Table_NVLofSanPham //
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

                    listNVLofSanphams.Add(nvlofsp);
                }
            }

            return listNVLofSanphams;
        }

        // Kiem tra da ton tai NVL trong ds NVL cua san pham
        public bool IsNVLofSanPhamExisting(NVLofSanPham nVLofSanPham)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT COUNT(*) FROM [{Common.TableNVLofSanPham}] WHERE [{Common.SPID}] = '{nVLofSanPham.SPID.Value}' AND [{Common.MaNVL}] = '{nVLofSanPham.MaNVL.Value}'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // Them loai NVL cho san pham
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

                command.CommandText = $"INSERT INTO [{Common.TableNVLofSanPham}] ({columnNames}) OUTPUT INSERTED.{Common.NVLSPID} VALUES ({parameterNames})";

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

        // Xoa NVL cua SP
        public (int, string) DeleteNVLofSanpham(NVLofSanPham? removeNVLofSP)
        {
            int result = -1; string errorMess = string.Empty;

            if (removeNVLofSP == null) return (result, errorMess);

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"DELETE FROM {Common.TableNVLofSanPham} WHERE [{Common.NVLSPID}] = '{removeNVLofSP.NVLSPID.Value}'";

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

        // Management KHO_NGUYENVATLIEU //

        // Table KHO_NguyenVatLieu //
        // Them loai NVL moi 
        public (int, string) InsertNewLoaiNguyenVatLieu(NguyenVatLieu newnvl)
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

                command.CommandText = $"INSERT INTO [{Common.TableNguyenVatLieu}] ({columnNames}) OUTPUT INSERTED.{Common.MaNVL} VALUES ({parameterNames})";

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
        public NguyenVatLieu GetNguyenVatLieubyID(int maNVL)
        {
            NguyenVatLieu nvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.MaNVL}] = '{maNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            return nvl;
        }

        // Get list NguyenVatLieu
        public List<NguyenVatLieu> GetListNguyenVatLieu(int loainvlID)
        {
            List<NguyenVatLieu> nguyenvatlieus = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableNguyenVatLieu}] WHERE [{Common.MaLoaiNVL}] = '{loainvlID}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    NguyenVatLieu nvl = new();

                    List<Propertyy> rowitems = nvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                    nguyenvatlieus.Add(nvl);
                }
            }

            return nguyenvatlieus;
        }

        // Table KHO_DanhMucNguyenVatLieu //
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

        // Them new danh muc NVL // KHO_DanhMucNguyenVatLieu
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

                command.CommandText = $"INSERT INTO [{Common.TableDanhMucNVL}] ({columnNames}) OUTPUT INSERTED.{Common.MaDanhMuc} VALUES ({parameterNames})";

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

        // Check ten danh muc da ton tai // KHO_DanhMucNguyenVatLieu
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

        // Table KHO_LoaiNguyenVatLieu //

        // Get loai NVL by maloai NVL // KHO_LoaiNguyenVatLieu
        public LoaiNVL GetLoaiNVLbyID(int maloaiNVL)
        {
            LoaiNVL loainvl = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableLoaiNVL}] WHERE [{Common.MaLoaiNVL}] = '{maloaiNVL}'";

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<Propertyy> rowitems = loainvl.GetPropertiesValues();

                    foreach (var item in rowitems)
                    {
                        string? columnName = item.DBName;

                        object columnValue = reader[columnName];

                        item.Value = columnValue;
                    }

                }
            }

            return loainvl;
        }

        // Get list loai nguyen vat lieu (get all) // KHO_LoaiNguyenVatLieu
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

        // Get list loai nguyen vat lieu (order by madanhmuc) // KHO_LoaiNguyenVatLieu
        public List<LoaiNVL> GetListLoaiNVLs(int madanhmuc)
        {
            List<LoaiNVL> listloaiNVLs = new();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $"SELECT * FROM [{Common.TableLoaiNVL}] WHERE [{Common.MaDanhMuc}] = '{madanhmuc}'";

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

        // Check ten loai nvl da ton tai // KHO_LoaiNguyenVatLieu
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

        // Them new loai NVL // KHO_LoaiNguyenVatLieu
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

                command.CommandText = $"INSERT INTO [{Common.TableLoaiNVL}] ({columnNames}) OUTPUT INSERTED.{Common.MaLoaiNVL} VALUES ({parameterNames})";

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
    }
}
