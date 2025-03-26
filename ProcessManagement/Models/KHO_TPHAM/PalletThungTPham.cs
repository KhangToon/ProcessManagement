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
        public List<ThungTPham> ThungTPhams { get; set; } = new();

        public bool IsExpand { get; set; } = false;

        public bool IsCreatePXKTP()
        {
            bool iscreated = true;

            foreach (var ttp in ThungTPhams)
            {
                if (int.TryParse(ttp.InStock.Value?.ToString(), out int instock))
                {
                    if (instock == 1 && ttp.PXKTPID.Value == null)
                    {
                        iscreated = false; break;
                    }
                }
            }

            return iscreated;
        }
    }
}
