using ProcessManagement.Models.KHO_TPHAM;

namespace ProcessManagement.DTOs
{
    public class ViTriTPhamMapper
    {
        // This class is intended to map ViTriTPham objects to DTOs and vice versa.
        public object? VTTPID { get; set; }
        public object? MaViTri { get; set; }
        public object? ViTriKe { get; set; }
        public object? ViTriHang { get; set; }
        public object? ViTriCot { get; set; }
        public object? VTSucChua { get; set; }

        public static ViTriTPhamMapper ToMapper(ViTriTPham model)
        {
            return new ViTriTPhamMapper
            {
                VTTPID = model.VTTPID.Value,
                MaViTri = model.MaViTri.Value,
                ViTriKe = model.ViTriKe.Value,
                ViTriHang = model.ViTriHang.Value,
                ViTriCot = model.ViTriCot.Value,
                VTSucChua = model.VTSucChua.Value
            };
        }
    }
}
