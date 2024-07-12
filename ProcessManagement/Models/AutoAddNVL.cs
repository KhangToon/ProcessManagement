using ProcessManagement.Services.SQLServer;

namespace ProcessManagement.Models
{
    public class AutoAddNVL
    {
        SQLServerServices SQLServerServices = new();

        public string LoaiNL { get; set; } = string.Empty;
        public string MaSP { get; set; } = string.Empty;
        public string FistMaQL { get; set; } = string.Empty;
        public int StartMaQLnum { get; set; } = 0;
        public int EndMaQLnum { get; set; } = 0;
        public int SoluongMoithung { get; set; } = 0;
        public int SoThung { get; set; } = 0;
        public string LotNL { get; set; } = string.Empty;
        public DateTime NgayNhap { get; set; }

        public List<NVL>? ListNVLs { get; set; }

        public List<string> GetLotNLList()
        {
            List<Lot> lots = SQLServerServices.GetlistLot();

            List<string> lotNLs = lots.Select(lot => lot.LotNVL?.Value?.ToString() ?? string.Empty).Where(lotnvl => (lotnvl != string.Empty)).ToList();

            return lotNLs;
        }

        public List<NVL> CreateListNVLs()
        {
            ListNVLs = new();

            for (int index = StartMaQLnum; index <= EndMaQLnum; index++)
            {
                NVL nVL = new();

                nVL.LoaiNVL.Value = LoaiNL;
                nVL.MaSP.Value = MaSP;
                nVL.MaQuanLy.Value = FistMaQL + index.ToString();
                nVL.SoLuong.Value = SoluongMoithung;
                nVL.NgayNhap.Value = DateTime.Now;
                nVL.LotNVL.Value = LotNL;

                ListNVLs.Add(nVL);
            }

            return ListNVLs;
        }

        public List<NVL>? CreateListNVLs_2(int startNum)
        {
            ListNVLs = new();

            int endNum = startNum + SoThung;

            for (int index = startNum; index < endNum; index++)
            {
                NVL nVL = new();

                nVL.LoaiNVL.Value = LoaiNL;
                nVL.MaSP.Value = MaSP;
                nVL.MaQuanLy.Value = FistMaQL + LoaiNL + "-" + index.ToString();
                nVL.SoLuong.Value = SoluongMoithung;
                nVL.NgayNhap.Value = NgayNhap;
                nVL.LotNVL.Value = LotNL;

                ListNVLs.Add(nVL);
            }

            return ListNVLs;
        }
    }
}
