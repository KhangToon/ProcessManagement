using ProcessManagement.Commons;
using ProcessManagement.Models.MAYMOC;
using ProcessManagement.Models.NHANVIEN;
using System.Reflection;

namespace ProcessManagement.Models.TienDoGCs
{
    public class TienDoGC
    {
        public Propertyy TDGCID { get; set; } = new() { DBName = DBName.TDGCID, DisplayName = DispName.TDGCID, Type = typeof(int), AlowDatabase = false, AlowDisplay = false, DispDatagrid = false };
        public Propertyy KHSXID { get; set; } = new() { DBName = DBName.KHSXID, DisplayName = DispName.KHSXID, Type = typeof(int), AlowDatabase = true, AlowDisplay = false };
        public Propertyy SPID { get; set; } = new() { DBName = DBName.SPID, DisplayName = DispName.SPID, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NCID { get; set; } = new() { DBName = DBName.NCID, DisplayName = DispName.NCID, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy TonDau { get; set; } = new() { DBName = DBName.TonDau, DisplayName = DispName.TonDau, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy DonHang { get; set; } = new() { DBName = DBName.DonHang, DisplayName = DispName.DonHang, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy XuatNVL { get; set; } = new() { DBName = DBName.XuatNVL, DisplayName = DispName.XuatNVL, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NangLucMay { get; set; } = new() { DBName = DBName.NangLucMay, DisplayName = DispName.NangLucMay, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NhanCong { get; set; } = new() { DBName = DBName.NhanCong, DisplayName = DispName.NhanCong, Type = typeof(int), AlowDatabase = true, AlowDisplay = true };
        public Propertyy ThoiGianHanhChinh { get; set; } = new() { DBName = DBName.ThoiGianHanhChinh, DisplayName = DispName.ThoiGianHanhChinh, Type = typeof(double), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NgayXuatHang { get; set; } = new() { DBName = DBName.NgayXuatHang, DisplayName = DispName.NgayXuatHang, Type = typeof(DateTime), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NgayBatDau { get; set; } = new() { DBName = DBName.NgayBatDau, DisplayName = DispName.NgayBatDau, Type = typeof(DateTime), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NgayKetThuc { get; set; } = new() { DBName = DBName.NgayKetThuc, DisplayName = DispName.NgayKetThuc, Type = typeof(DateTime), AlowDatabase = true, AlowDisplay = true };
        public Propertyy NgayLap { get; set; } = new() { DBName = DBName.NgayLap, DisplayName = DispName.NgayLap, Type = typeof(DateTime), AlowDatabase = true, AlowDisplay = true };
        public Propertyy ThoiGianTangCa { get; set; } = new() { DBName = DBName.ThoiGianTangCa, DisplayName = DispName.ThoiGianTangCa, Type = typeof(double), AlowDatabase = true, AlowDisplay = true };
        public Propertyy TiLeNG_CD { get; set; } = new() { DBName = DBName.TiLeNG_CD, DisplayName = DispName.TiLeNG_CD, Type = typeof(double), AlowDatabase = true };
        public Propertyy TiLeNG_TT { get; set; } = new() { DBName = DBName.TiLeNG_TT, DisplayName = DispName.TiLeNG_TT, Type = typeof(double), AlowDatabase = true };

        public Propertyy MMIDs { get; set; } = new() { DBName = DBName.MMIDs, DisplayName = DispName.MMIDs, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy NVIDs { get; set; } = new() { DBName = DBName.NVIDs, DisplayName = DispName.NVIDs, Type = typeof(string), AlowDatabase = true, CheckErrors = new() { Propertyy.ErrType.NotEmptyValue } };
        public Propertyy CaLamViecs { get; set; } = new() { DBName = DBName.CaLamViecs, DisplayName = DispName.CaLamViecs, Type = typeof(string), AlowDatabase = true };

        // Selected item changed
        public IEnumerable<KeyValuePair<string, string>>? MayMocsSelected;
        public IEnumerable<KeyValuePair<string, string>>? NhanviensSelected;
        public IEnumerable<KeyValuePair<string, string>>? CalamviecsSelected;
        public IEnumerable<KeyValuePair<string, string>>? NgaysSelected;
        public object? ThoiGianGiaCong { get; set; } // Dung cho add-multi auto TienDoRow
        public object? ThoiGianLamViec { get; set; } // Dung cho add-multi auto TienDoRow
        public object? Kehoach { get; set; } // Dung cho add-multi auto TienDoRow

        public List<TienDoGCRow> DSachTienDoRows { get; set; } = new();

        public string MaSanPham { get; set; } = string.Empty;
        public string TenCongDoan { get; set; } = string.Empty;

        public int FooterSumKeHoach { get; set; }
        public int FooterSumThucTe { get; set; }
        public int FooterSumNG { get; set; }
        public int FooterSumTienDo { get; set; }
        public double TiLeNGCD { get; set; }
        public double TiLeNGTT { get; set; }

        // Get all property of this class
        public static List<Propertyy> GetClassProperties()
        {
            Type propertyType = typeof(TienDoGC);

            TienDoGC instance = new();

            FieldInfo[] fields = propertyType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            List<Propertyy> propertiesValue = new();

            foreach (FieldInfo field in fields)
            {
                Type ob = field.FieldType;

                if (ob == typeof(Propertyy))
                {
                    Propertyy? fieldValue = (Propertyy?)field.GetValue(instance);

                    if (fieldValue != null)
                    {
                        propertiesValue.Add(fieldValue);
                    }
                }
            }

            return propertiesValue;
        }

        public List<Propertyy> GetPropertiesValues()
        {
            Type propertyType = typeof(TienDoGC);

            FieldInfo[] fields = propertyType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            List<Propertyy> propertiesValue = new();

            foreach (FieldInfo field in fields)
            {
                Type ob = field.FieldType;

                if (ob == typeof(Propertyy))
                {
                    Propertyy? fieldValue = (Propertyy?)field.GetValue(this);

                    if (fieldValue != null) { propertiesValue.Add(fieldValue); }
                }
            }

            return propertiesValue;
        }

        public void SetPropertyValue(string propertyName, object newValue)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.DBName == propertyName);

            if (tagetProperty != null)
            {
                tagetProperty.Value = newValue;
            }
        }

        public object? GetPropertyValue(string propertyName)
        {
            List<Propertyy> propertiesValue = GetPropertiesValues();

            Propertyy? tagetProperty = propertiesValue.FirstOrDefault(f => f.DBName == propertyName);

            return tagetProperty?.Value;
        }

        public static class DBName
        {
            public const string Table_TienDoGC = "TDGC_TienDoGC";
            public const string TDGCID = "TDGCID";
            public const string SPID = "SPID";
            public const string NCID = "NCID";
            public const string KHSXID = "KHSXID";
            public const string TonDau = "tondau";
            public const string DonHang = "donhang";
            public const string XuatNVL = "xuatnvl";
            public const string NangLucMay = "nanglucmay";
            public const string NhanCong = "nhancong";
            public const string ThoiGianHanhChinh = "thoigianhanhchinh";
            public const string NgayXuatHang = "ngayxuathang";
            public const string NgayBatDau = "ngaybatdau";
            public const string NgayKetThuc = "ngayketthuc";
            public const string NgayLap = "ngaylap";
            public const string ThoiGianTangCa = "thoigiantangca";
            public const string TiLeNG_CD = "tile_ngcd";
            public const string TiLeNG_TT = "tile_ngtt";
            public const string MMIDs = "MMIDs";
            public const string NVIDs = "NVIDs";
            public const string CaLamViecs = "calamviecs";
            public const string Ngays = "ngays";
            public const string ExcellTitle = "exceltitile";
        }

        public static class DispName
        {
            public const string Table_KetQuaGC = "TDGC_TienDoGC";
            public const string TDGCID = "TDGCID";
            public const string SPID = "SPID";
            public const string NCID = "NCID";
            public const string KHSXID = "KHSXID";
            public const string TonDau = "Tồn đầu";
            public const string DonHang = "Đơn hàng";
            public const string XuatNVL = "Xuất NVL";
            public const string NangLucMay = "Năng lực máy";
            public const string NhanCong = "Nhân công";
            public const string ThoiGianHanhChinh = "Thời gian hành chính";
            public const string NgayXuatHang = "Ngày xuất hàng";
            public const string NgayBatDau = "Ngày bắt đầu";
            public const string NgayKetThuc = "Ngày kết thúc";
            public const string NgayLap = "Ngày lập";
            public const string ThoiGianTangCa = "Thời gian tăng ca";
            public const string TiLeNG_CD = "% NG CĐ";
            public const string TiLeNG_TT = "% NG TT";
            public const string MMIDs = "MMIDs";
            public const string NVIDs = "NVIDs";
            public const string CaLamViecs = "Ca làm việc";
            public const string Ngays = "ngays";
        }

        public static class ExcellAddress
        {
            public static Dictionary<string, string> ColumnAddress = new()
            {
                { DBName.ExcellTitle, "B" },
                { DBName.NgayXuatHang, "D" },
                { DBName.NgayBatDau, "D" },
                { DBName.NgayKetThuc, "D" },
                { DBName.NgayLap, "N" }
            };

            public static Dictionary<string, string> RowAddress = new()
            {
                { DBName.ExcellTitle, "1" },
                { DBName.NgayXuatHang, "3" },
                { DBName.NgayBatDau, "4" },
                { DBName.NgayKetThuc, "5" },
                { DBName.NgayLap, "3" }
            };
        }
    }
}
