using ProcessManagement.Models.KHO_TPHAM;

namespace ProcessManagement.Mappers
{
    // DTOs - Data Transfer Objects 
    public class ViTriofTPhamMapper
    {
        public object? VTofTPID { get; set; }
        public object? VTTPID { get; set; }
        public object? VTTPSoLuong { get; set; }
        public object? NgayNhapKho { get; set; }
        public object? LotVitri { get; set; }

        public static ViTriofTPhamMapper ToMapper(ViTriofTPham entity)
        {
            return new ViTriofTPhamMapper
            {
                VTofTPID = entity.VTofTPID.Value,
                VTTPID = entity.VTTPID.Value,
                VTTPSoLuong = entity.VTTPSoLuong.Value,
                NgayNhapKho = entity.NgayNhapKho.Value,
                LotVitri = entity.LotVitri.Value
            };
        }
    }
}
