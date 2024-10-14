using System.Reflection;

namespace ProcessManagement.Models.KHSXs
{
    public class KetquaGC
    {
        public const string TableKetquaGC = "KHSX_KetquaGC";

        [IsAllowDatabase(false)]
        public int KQGCID { get; set; }
        public const string DBKQID = "KQGCID";

        [IsAllowDatabase(true)]
        public DateTime SubMitDay { get; set; }
        public const string DBSubMitDay = "SubMitDay";

        [IsAllowDatabase(true)]
        public string TenSanPham { get; set; } = string.Empty; public const string DBTenSanPham = "TenSanPham";

        [IsAllowDatabase(true)]
        public string TenNguyenCong { get; set; } = string.Empty; public const string DBTenNguyenCong = "TenNguyenCong";

        [IsAllowDatabase(true)]
        public string MaMay { get; set; } = string.Empty; public const string DBMaMay = "MaMay";

        [IsAllowDatabase(false)]
        public Calamviec CaLamViec { get; set; }
        public const string DBCaLamViec = "MaMay";

        [IsAllowDatabase(true)]
        public int ThoiGianGC { get; set; }
        public const string DBThoiGianGC = "ThoiGianGC";

        [IsAllowDatabase(true)]
        public string MaQuanLyLOT { get; set; } = string.Empty; public const string DBMaQuanLyLOT = "MaQuanLyLOT";

        [IsAllowDatabase(true)]
        public int SLOK { get; set; } = 0; public const string DBSLOK = "SLOK";

        [IsAllowDatabase(true)]
        public int SLNG { get; set; } = 0; public const string DBSLNG = "SLNG";

        [IsAllowDatabase(true)]
        public int SLperLOT { get; set; }
        public const string DBSLperLOT = "SLperLOT";

        [IsAllowDatabase(true)]
        public string LoaiNG { get; set; } = string.Empty; public const string DBLoaiNG = "LoaiNG";

        [IsAllowDatabase(true)]
        public int ThoiGianLamViec { get; set; }
        public const string DBThoiGianLamViec = "ThoiGianLamViec";
        [IsAllowDatabase(true)]
        public string TenNhanVien { get; set; } = string.Empty; public const string DBTenNhanVien = "TenNhanVien";

        [IsAllowDatabase(true)]
        public string GhiChu { get; set; } = string.Empty; public const string DBGhiChu = "GhiChu";

        [IsAllowDatabase(false)]
        public DongThung DongThung { get; set; } = new();

        public class IsAllowDatabaseAttribute : Attribute
        {
            public bool Value { get; set; }

            public IsAllowDatabaseAttribute(bool value)
            {
                Value = value;
            }
        }
    }

    public enum Calamviec
    {
        Ngay = 0,
        Dem = 1,
    }

    public class DongThung
    {
        //public int DTID { get; set; }
        public int KQGCID { get; set; }
        public string SoIDThung { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public DateTime NgayDongThung { get; set; }
    }
}
