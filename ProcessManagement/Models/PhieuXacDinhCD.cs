namespace ProcessManagement.Models
{
    public class PhieuXacDinhCD
    {
        public string TenSanPham { get; set; } = string.Empty;
        public string MaSanPham { get; set; } = string.Empty;
        public string MaQuanLyLOT { get; set; } = string.Empty;
        public string MaKHSX { get; set; } = string.Empty;

        public List<NVLmoiNguyenCong> DSNVLofNguyenCongs { get; set; } = new();

    }
}
