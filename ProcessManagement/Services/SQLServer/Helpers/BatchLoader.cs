using Microsoft.Data.SqlClient;
using ProcessManagement.Commons;
using ProcessManagement.Models;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.SANPHAM;
using ProcessManagement.Services.SQLServer.Helpers;
using System.Data;

namespace ProcessManagement.Services.SQLServer.Helpers
{
    /// <summary>
    /// Helper để batch load related data, tránh N+1 queries
    /// </summary>
    public static class BatchLoader
    {
        /// <summary>
        /// Batch load ThongTinSanPham cho nhiều SPIDs
        /// Thay vì N queries, chỉ cần 1 query
        /// </summary>
        public static async Task<List<ThongTinSanPham>> BatchLoadThongTinSanPhamAsync(
            string connectionString,
            List<object> spIds)
        {
            if (!spIds.Any()) return new List<ThongTinSanPham>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.Table_ThongTinSanPham,
                Common.SP_SPID,
                spIds);

            using var reader = await command.ExecuteReaderAsync();
            return PropertyMapper.MapEntitiesFromReader(reader, () => new ThongTinSanPham());
        }

        /// <summary>
        /// Batch load NVLwithSanPham cho nhiều SPIDs
        /// </summary>
        public static async Task<List<NVLwithSanPham>> BatchLoadNVLwithSanPhamAsync(
            string connectionString,
            List<object> spIds)
        {
            if (!spIds.Any()) return new List<NVLwithSanPham>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.Table_NVLwithSanPham,
                Common.SP_SPID,
                spIds);

            using var reader = await command.ExecuteReaderAsync();
            return PropertyMapper.MapEntitiesFromReader(reader, () => new NVLwithSanPham());
        }

        /// <summary>
        /// Batch load NguyenCong cho nhiều NCIDs
        /// </summary>
        public static async Task<List<NguyenCong>> BatchLoadNguyenCongsAsync(
            string connectionString,
            List<object> ncIds)
        {
            if (!ncIds.Any()) return new List<NguyenCong>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.Table_NguyenCong,
                Common.NCID,
                ncIds);

            using var reader = await command.ExecuteReaderAsync();
            return PropertyMapper.MapEntitiesFromReader(reader, () => new NguyenCong());
        }

        /// <summary>
        /// Batch load NguyenVatLieu cho nhiều NVLIDs
        /// </summary>
        public static async Task<List<NguyenVatLieu>> BatchLoadNguyenVatLieusAsync(
            string connectionString,
            List<object> nvlIds)
        {
            if (!nvlIds.Any()) return new List<NguyenVatLieu>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var command = QueryBuilder.BuildInQuery(
                connection,
                Common.Table_NguyenVatLieu,
                Common.NVLID,
                nvlIds);

            using var reader = await command.ExecuteReaderAsync();
            return PropertyMapper.MapEntitiesFromReader(reader, () => new NguyenVatLieu());
        }
    }
}

