using ProcessManagement.Models.KHSXs;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class PalletThungTPham
    {
        public object? IDThung { get; set; }
        public int Total { get; set; }
        public string MaSanPham { get; set; } = string.Empty;
        public string PalletKey { get; set; } = string.Empty;
        public bool DaNhapKho { get; set; } = false;
        public List<PartOfThungTPham> ThungTPhams { get; set; } = new();

        public bool IsExpand { get; set; } = false;
    }
}
