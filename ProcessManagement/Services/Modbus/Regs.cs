namespace ProcessManagement.Services.Modbus
{
    public static class Regs
    {
        public enum AlarmCode
        {
            UpdateSuccess = 1, // Cap nhat so luong gia cong thanh cong
            MQLNotexist = 2, // Ma quan ly khong ton tai
            NCNotexist = 3, // Nguyen cong khong ton tai trong LSX
            ErrorSLOKNG = 4, // So luong OK/NG khong hop le
            NotEnoughInfor = 5, // Chua nhap day du thong tin
            LSXNotexist = 6, // LSX khong ton tai
            IsUpdated = 7, // MQL da duoc update truoc do
            UpdateFailed = 8, // Update SQL calamviec error
            MMayNotexist = 10, // Ma may khong ton tai
            MNVNotexist = 11, // Ma nhan vien khong ton tai
            NotAllowUpdate = 12, // Chua duoc phep gia cong
        }

        public class NguyenCongID // Nguyen cong code
        {
            public static Dictionary<int, int> IDs = new()
            {      
              //{nguyencongkey, ncID in database}
                {1, 2}, // Tiện phi
                {2, 3}, // Tiện ren
                {3, 4}, // Khoan lỗ
                {4, 13}, // Bavia + Rữa
                {5, 14}, // Kiểm Pin,M,Ren
                {6, 15}, // Ngoại quan + đóng thùng 
            };
        }

        public class RegTypes
        {
            public const string RegCoilOutputs = "Coil Outputs"; // 0x
            public const string RegDigitalInputs = "Digital Inputs"; // 2x
            public const string RegAnalogueInputs = "Analogue Inputs"; // 3x
            public const string RegHoldingRegisters = "Holding Registers"; // 4x
            public const string RegWriteString = "Write String";
        }

        // Dia chi thanh ghi ben Modbus Inovance Client hon dia chi ben Server 1 don vi

        public static class Server // server common register
        {
            // Server datetime
            public const int ServerDatetime = 59;   // 4x 58
            // Ma LSX
            public const int LSXCode = 17;          // 4x 16
            // Ma san pham
            public const int MaSanPham = 81;        // 4x 80
            // Loai NVL
            public const int LoaiNVL01 = 29;        // 4x 28
            // DinhMuc san xuat
            public const int DinhMucSX = 47;        // 4x 46
            // SL da san xuat
            public const int SLDaSanXuat = 48;      // 4x 47
            // SL lot hoan thanh
            public const int SLLotDone = 50;        // 4x 49
            // SL lot
            public const int SLLot = 52;            // 4x 51
        }

        public static class Device01 // device 01 register
        {
            // bit enable device 
            public const int EnableDevice01 = 1;    // 0x 0
            // bit load LSX details
            public const int LSXIsLoading01 = 3;         // 0x 2
            // bit submit results
            public const int SubmitBit01 = 4;       // 0x 3

            // SL loi cho phep
            public const int SLLoichophep01 = 49;     // 4x 48
            // SL lot hien tai
            public const int SLLoihientai01 = 56;   // 4x 55

            // nguyen cong Required 
            public const int NgCongRequiredLoadLSX01 = 54;     // 4x 53
            // nguyen cong ID
            public const int NgCongIDCode01 = 72;     // 4x 71
            // ma may
            public const int MaMay01 = 41;          // 4x 40
            // ma quan ly lot
            public const int MaQuanLyLot01 = 1;     // 4x 0
            // ma nhan vien 
            public const int MaNhanVien01 = 35;     // 4x 34
            // ca lam viec
            public const int Calamviec01 = 2;       // 0x 1
            // sl OK
            public const int SLOK01 = 51;           // 4x 50
            // sl NG
            public const int SLNG01 = 53;           // 4x 52
            // alarm loging
            public const int AlarmLog01 = 58;       // 4x 57
        }
    }
}
