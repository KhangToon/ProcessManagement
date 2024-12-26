using ProcessManagement.Models.KHSXs;

namespace ProcessManagement.Models.KHO_TPHAM
{
    public class ThungTPhamExtend
    {
        public object? IDThung { get; set; }

        public List<ThungTPham> ThungTPhams { get; set; } = new();
    }
}
