using ProcessManagement.Services.SQLServer;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.KHO_NVL.NhapKho;
using ProcessManagement.Models.KHO_NVL.XuatKho;
using ProcessManagement.Models.KHO_NVL.Tracking;
using Radzen;
using ProcessManagement.Commons;
using ProcessManagement.Models.KHO_NVL.KiemKe;
using Microsoft.IdentityModel.Tokens;

namespace ProcessManagement.Services
{
    public static class KHOServices
    {
        private static readonly SQLServerServices SQLServerServices = new();

        // Handle lenh nhap kho
        public static async Task<(int, string)> HandleLenhNhapKho(PhieuNhapKho PNK, LenhNhapKho LNK, string maVitri, string qridlot, bool modeTraKho = false)
        {
            return await Task.Run(() =>
            {
                int soluongnhapfromClient = int.TryParse(LNK.LNKSoLuong.Value?.ToString(), out int lnksl) ? lnksl : 0;
                object? lnkid = LNK.LenhNKID.Value?.ToString();

                // Load lenh nhap kho by ID
                LenhNhapKho savedLNK = SQLServerServices.GetLenhNhapKhoByID(lnkid);

                // Checking LNK is "Trakho"
                modeTraKho = !string.IsNullOrEmpty(savedLNK.NgayNhapKho.Value?.ToString()?.Trim());

                string maPNK = PNK.MaPhieuNK.Value?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(maPNK)) { return (-1, $"Mã phiếu {((modeTraKho) ? "trả" : "nhập")} không tồn tại"); }
                if (string.IsNullOrWhiteSpace(maVitri) || !string.Equals(maVitri, savedLNK.TargertVitri.MaViTri.Value?.ToString()?.Trim())) { return (-1, "Mã vị trí không tồn tại"); }
                if (string.IsNullOrWhiteSpace(qridlot) || !string.Equals(qridlot, savedLNK.QRIDLOT.Value?.ToString()?.Trim())) { return (-1, "Mã quản lý LOT NVL không tồn tại"); }
                if (soluongnhapfromClient == 0) { return (-1, $"Số lượng {((modeTraKho) ? "trả" : "nhập")} kho phải lớn hơn 0"); }

                if (savedLNK.LenhNKID.Value == null)
                {
                    return (-1, $"Không tồn tại lệnh {((modeTraKho) ? "trả" : "nhập")} kho!");
                }
                else
                {
                    // Handle lenh nhap kho

                    // Check trang thai lenh (da hoan thanh hay chua)
                    _ = int.TryParse(savedLNK.LNKIsDone.Value?.ToString(), out int scanlnkIsdone) ? scanlnkIsdone : -1;
                    if (scanlnkIsdone != 0)
                    {
                        return (-1, $"Không thể {((modeTraKho) ? "trả" : "nhập")} (lệnh đã hoàn thành trước đó)!");
                    }

                    // Kiem tra so luong them vao
                    int soluongThemvao = int.TryParse(savedLNK.LNKSoLuong.Value?.ToString(), out int slthem) ? slthem : 0;
                    if (soluongThemvao == 0)
                    {
                        return (-1, "Số lượng không hợp lệ!");
                    }

                    // Kiem tra vitri of NVL con trong hay khong / hoac cung ngay nhap kho
                    var viTriofNVL = SQLServerServices.GetViTriOfNgVatLieuBy_VTid_LotVitri(vtid: savedLNK.VTID.Value, lotvitri: savedLNK.LotVitri.Value);

                    // Handle lenh nhap kho voi LOT Vitri
                    if (viTriofNVL.VTofNVLID.Value != null) // vtofnvl co chua NVL
                    {
                        int slhienco = int.TryParse(viTriofNVL?.VTNVLSoLuong.Value?.ToString(), out int slhc) ? slhc : 0;

                        // so luong sau khi nhap cho vi tri da luu
                        int newtonkhotaivitri = slhienco + soluongThemvao;

                        // Tao moi vitriofNVL
                        ViTriofNVL newviTriofNVL = new()
                        {
                            VTofNVLID = { Value = viTriofNVL?.VTofNVLID.Value },
                            VTID = { Value = viTriofNVL?.VTID.Value },
                            NVLID = { Value = savedLNK.NVLID.Value },
                            VTNVLSoLuong = { Value = newtonkhotaivitri },
                            //NgayNhapKho = { Value = DateTime.Now.Date.ToShortDateString() },
                            NgayNhapKho = { Value = (modeTraKho) ? savedLNK.NgayNhapKho.Value : DateTime.Now.Date.ToString("dd/MM/yyyy") },
                            LotVitri = { Value = viTriofNVL?.LotVitri.Value },
                            QRIDLOT = { Value = savedLNK.QRIDLOT.Value }
                        };

                        int updateVTofNVLresult = -1; string updateVTofNVLerror;

                        if (slhienco > 0) // cung NVL cung NgayNhapKho
                        {
                            bool isnvlsame = object.Equals(viTriofNVL?.NVLID.Value, savedLNK.NVLID.Value);
                            //bool isngaynksame = viTriofNVL?.NgayNhapKho.Value?.ToString()?.Trim() == DateTime.Now.Date.ToShortDateString();
                            bool isngaynksame = viTriofNVL?.NgayNhapKho.Value?.ToString()?.Trim() == ((modeTraKho) ? savedLNK.NgayNhapKho.Value?.ToString()?.Trim() : DateTime.Now.Date.ToShortDateString());

                            if (isnvlsame && isngaynksame)
                            {
                                // Trung ngay va trung NVL --> ++ so luong ton kho tai LOT
                                (updateVTofNVLresult, updateVTofNVLerror) = SQLServerServices.UpdateViTriOfNgVatLieu(newviTriofNVL);

                                if (updateVTofNVLresult == -1)
                                {
                                    return (-1, $"LNK Lỗi: {updateVTofNVLerror}!");
                                }
                            }
                            else
                            {
                                // Khong cho nhap kho vao LOT // chi duoc nhap vao lot khac cung vitri
                                return (-1, $"LOT đã chứa NVL, không thể {((modeTraKho) ? "trả" : "nhập")} kho!");
                            }
                        }
                        else
                        {
                            // Update so luong ton kho tai LOT va NVLID
                            (updateVTofNVLresult, updateVTofNVLerror) = SQLServerServices.UpdateViTriOfNgVatLieu(newviTriofNVL);

                            if (updateVTofNVLresult == -1)
                            {
                                return (-1, $"LNK Lỗi: {updateVTofNVLerror}!");
                            }
                        }

                        // Update success
                        if (updateVTofNVLresult > 0)
                        {
                            // Update lenh nhap kho status
                            savedLNK.LNKIsDone.Value = 1;
                            savedLNK.NgayNhapKho.Value = (modeTraKho) ? savedLNK.NgayNhapKho.Value : DateTime.Now.Date.ToShortDateString();
                            //savedLNK.NgayNhapKho.Value = DateTime.Now.Date.ToShortDateString();

                            (int updatelnkResult, string updatelnkError) = SQLServerServices.UpdateLenhNhapKho(savedLNK);

                            if (updatelnkResult == -1)
                            {
                                return (-1, $"LNK Lỗi: {updatelnkError}!");
                            }

                            // Update isDonePNK status
                            // Get if this LNK is last LNK
                            int totalLNK = PNK.DSNVLofPNKs.Sum(nvlpNK => nvlpNK.DSLenhNKs.Count);
                            int totalLNKdone = PNK.DSNVLofPNKs.Sum(nvlpNK => nvlpNK.DSLenhNKs.Sum(lNK => (((int.TryParse(lNK.LNKIsDone.Value?.ToString(), out int isdonelNK) ? isdonelNK : 0) == 1) ? 1 : 0)));
                            bool islastLNK = (totalLNK - totalLNKdone) == 1;
                            //
                            // Set bit done PNK is 1 if isLastLNK
                            if (islastLNK)
                            {
                                PNK.IsDonePNK.Value = 1;
                                // Update isDonePNK
                                SQLServerServices.UpdatePhieuNhapKhoInfor(PNK);
                            }

                            // update status to UI
                            LNK.LNKIsDone.Value = 1;

                            // Get nguoi nhap kho
                            string nguoiNhapkho = SQLServerServices.GetNguoiTaoPhieuNhapKhoByID(savedLNK.PNKID.Value);

                            // Logging nhap kho
                            HistoryXNKho logNhapKho = new()
                            {
                                LogLoaiPhieu = { Value = (modeTraKho) ? Common.LogTypeTraKho : Common.LogTypePNK },
                                LogMaPhieu = { Value = maPNK },
                                LogMaViTri = { Value = maVitri },
                                LogNgThucHien = { Value = nguoiNhapkho },
                                LogSoLuong = { Value = savedLNK.LNKSoLuong.Value },
                                LogTonKhoTruoc = { Value = savedLNK.TargetNgLieu.TonKho },
                                LogTonKhoSau = { Value = savedLNK.TargetNgLieu.TonKho + soluongThemvao },
                                LogTenNVL = { Value = savedLNK.TargetNgLieu.MaNVL.Value },
                                LogThoiDiem = { Value = DateTime.Now },
                                LotVitri = { Value = savedLNK.LotVitri.Value },
                                NVLID = { Value = savedLNK.NVLID.Value },
                                VTID = { Value = savedLNK.VTID.Value },
                                QRIDLOT = { Value = savedLNK.QRIDLOT.Value }
                            };
                            // Insert logging to Database
                            (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logNhapKho);

                            if (logId == -1)
                            {

                            }

                            if (string.IsNullOrEmpty(savedLNK.NgayNhapKho.Value?.ToString()?.Trim()))
                            {
                                return (-1, $"Đã {((modeTraKho) ? "trả" : "nhập")} kho nguyên liệu: \n {savedLNK.TargetNgLieu.MaNVL.Value?.ToString()} \n Số lượng : {soluongnhapfromClient} (pcs), \n \n Lỗi trống ngày {((modeTraKho) ? "trả" : "nhập")} kho!");
                            }

                            return (1, $"Đã {((modeTraKho) ? "trả" : "nhập")} kho nguyên liệu: \n {savedLNK.TargetNgLieu.MaNVL.Value?.ToString()} \n Số lượng : {soluongnhapfromClient} (pcs)");
                        }
                    }
                }

                return (-1, $"Không thể {((modeTraKho) ? "trả" : "nhập")} kho!");
            });
        }

        // Handle lenh xuat kho
        public static async Task<(int, string)> HandleLenhXuatKho(PhieuXuatKho PXK, LenhXuatKho LXK, string maViTri, string qridlot)
        {
            return await Task.Run(() =>
            {
                int soluongxuatfromClient = int.TryParse(LXK.LXKSoLuong.Value?.ToString(), out int lxksl) ? lxksl : 0;
                object? lxkid = LXK.LenhXKID.Value;

                // Load lenh xuat kho by ID
                LenhXuatKho savedLXK = SQLServerServices.GetLenhXuatKhoByID(lxkid);


                if (string.IsNullOrWhiteSpace(PXK.MaPhieuXK.Value?.ToString())) { return (-1, "Mã phiếu nhập không tồn tại"); }
                if (string.IsNullOrWhiteSpace(maViTri) || !string.Equals(maViTri, savedLXK.ViTriofNVL.VitriInfor.MaViTri.Value?.ToString()?.Trim())) { return (-1, "Mã vị trí không tồn tại"); }
                if (string.IsNullOrWhiteSpace(qridlot) || !string.Equals(qridlot, savedLXK.QRIDLOT.Value?.ToString()?.Trim())) { return (-1, "Mã quản lý LOT NVL không tồn tại"); }
                if (soluongxuatfromClient == 0) { return (-1, "Số lượng nhập kho phải lớn hơn 0"); }


                if (savedLXK.LenhXKID.Value == null)
                {
                    return (-1, "Không tồn tại lệnh xuất kho!");
                }
                else
                {
                    // Handle lenh xuat kho

                    // Check trang thai lenh (da hoan thanh hay chua)
                    _ = int.TryParse(savedLXK.LXKIsDone.Value?.ToString(), out int scanlxkIsdone) ? scanlxkIsdone : -1;
                    if (scanlxkIsdone != 0)
                    {
                        return (-1, "Không thể xuất (lệnh đã hoàn thành trước đó)!");
                    }

                    // Kiem tra so luong xuat ra
                    int soluongXuatra = int.TryParse(savedLXK.LXKSoLuong.Value?.ToString(), out int slxuat) ? slxuat : 0;
                    if (soluongXuatra == 0)
                    {
                        return (-1, "Số lượng không hợp lệ!");
                    }

                    // Kiem tra trang thai vi tri (vi tri co QRIDLOT trung voi lenh xuat kho)
                    ViTriofNVL viTriofNVL = SQLServerServices.GetViTriOfNgVatLieuByAnyParameters(vtid: savedLXK.VTID.Value, qridlot: savedLXK.QRIDLOT.Value).FirstOrDefault() ?? new();

                    if (viTriofNVL == null || viTriofNVL.VTofNVLID.Value == null)
                    {
                        return (-1, "Vị trí không tồn tại nguyên liệu này!");
                    }

                    int soluongHiencotaivitri = int.TryParse(viTriofNVL?.VTNVLSoLuong.Value?.ToString(), out int slhc) ? slhc : 0;

                    // Kiem tra so luong xuat co vuot qua so luong hien tai khong
                    if (soluongXuatra > soluongHiencotaivitri)
                    {
                        return (-1, "Số lượng xuất quá số lượng tồn kho tại vị trí!");
                    }

                    // Tinh so luong con lai sau khi xuat
                    int newtonkhotaivitri = soluongHiencotaivitri - soluongXuatra;

                    // Tao moi vitriofNVL
                    ViTriofNVL viTriofNVLupdate = new()
                    {
                        VTofNVLID = { Value = viTriofNVL?.VTofNVLID.Value },
                        VTID = { Value = viTriofNVL?.VTID.Value },
                        NVLID = { Value = (newtonkhotaivitri > 0) ? savedLXK.NVLID.Value : 0 },
                        VTNVLSoLuong = { Value = newtonkhotaivitri },
                        NgayNhapKho = { Value = (newtonkhotaivitri > 0) ? viTriofNVL?.NgayNhapKho.Value : string.Empty },
                        LotVitri = { Value = viTriofNVL?.LotVitri.Value },
                        QRIDLOT = { Value = (newtonkhotaivitri > 0) ? viTriofNVL?.QRIDLOT.Value : string.Empty }
                    };

                    // Update so luong vi tri cua nvl
                    (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateViTriOfNgVatLieu(viTriofNVLupdate);

                    if (updateVTofNVLresult == -1)
                    {
                        return (-1, "Số lượng xuất quá số lượng tồn kho tại vị trí!");
                    }

                    // Update lenh xuat kho status
                    savedLXK.LXKIsDone.Value = 1;
                    savedLXK.NgayXuatKho.Value = DateTime.Now.Date.ToShortDateString();
                    (int updatelxkResult, string updatelxkError) = SQLServerServices.UpdateLenhXuatKho(savedLXK);

                    if (updatelxkResult == -1)
                    {
                        return (-1, "Không thể thực hiện lệnh xuất kho!");
                    }


                    // Update isDonePXK status
                    // Get if this LXK is last LXK
                    int totalLXK = PXK.DSNVLofPXKs.Sum(nvlpxk => nvlpxk.DSLenhXKs.Count);
                    int totalLXKdone = PXK.DSNVLofPXKs.Sum(nvlpxk => nvlpxk.DSLenhXKs.Sum(lxk => (((int.TryParse(lxk.LXKIsDone.Value?.ToString(), out int isdonelxk) ? isdonelxk : 0) == 1) ? 1 : 0)));
                    bool islastLXK = (totalLXK - totalLXKdone) == 1;
                    //
                    // Set bit done PXK is 1 if isLastLXK
                    if (islastLXK)
                    {
                        PXK.IsDonePXK.Value = 1;
                        // Update isDonePXK
                        SQLServerServices.UpdatePhieuXuatKhoInfor(PXK);
                    }

                    // Update ngaynhap/xuatkho of KHSX
                    _ = int.TryParse(PXK.KHSXID.Value?.ToString(), out int khsxid) ? khsxid : 0;
                    //  // Update ngaynhap/xuatkho of KHSX (use with create KHSX)
                    if (khsxid > 0)
                    {
                        //// update IsDonePXKofKHSX is done (da xuat kho cho KHSX)
                        //SQLServerServices.UpdateKHSXProperty(khsxid, Common.IsDonePXK, PXK.IsDonePXK.Value);

                        (int updatelotstatus, string errorlot) = UpdateNgayNhapXuatKho_dsLOTofKHSX(khsxid, savedLXK);
                    }

                    // update status to UI
                    LXK.LXKIsDone.Value = 1;

                    // Get nguoi xuat kho
                    string nguoiXuatkho = SQLServerServices.GetNguoiTaoPhieuXuatKhoByID(savedLXK.PXKID.Value);

                    // Logging xuat kho
                    HistoryXNKho logXuatKho = new()
                    {
                        LogLoaiPhieu = { Value = Common.LogTypePXK },
                        LogMaPhieu = { Value = PXK.MaPhieuXK.Value?.ToString() },
                        LogMaViTri = { Value = maViTri },
                        LogNgThucHien = { Value = nguoiXuatkho },
                        LogSoLuong = { Value = savedLXK.LXKSoLuong.Value },
                        LogTonKhoTruoc = { Value = savedLXK.ViTriofNVL.NgLieuInfor.TonKho },
                        LogTonKhoSau = { Value = savedLXK.ViTriofNVL.NgLieuInfor.TonKho - soluongXuatra },
                        LogTenNVL = { Value = savedLXK.ViTriofNVL.NgLieuInfor.MaNVL.Value },
                        LogThoiDiem = { Value = DateTime.Now },
                        LotVitri = { Value = savedLXK.LotVitri.Value },
                        NVLID = { Value = savedLXK.NVLID.Value },
                        VTID = { Value = savedLXK.VTID.Value },
                        QRIDLOT = { Value = savedLXK.QRIDLOT.Value }
                    };
                    // Insert logging to Database
                    (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logXuatKho);

                    if (logId == -1)
                    {
                        return (-1, $"Đã xuất kho nguyên liệu: \n {savedLXK.ViTriofNVL.NgLieuInfor.MaNVL.Value?.ToString()} \n Số lượng : {soluongxuatfromClient} (pcs) \n Lỗi insert loging!");
                    }

                    // Feedback xuat kho thanh cong
                    return (1, $"Đã xuất kho nguyên liệu: \n {savedLXK.ViTriofNVL.NgLieuInfor.MaNVL.Value?.ToString()} \n Số lượng : {soluongxuatfromClient} (pcs)");
                }
            });
        }

        // Update ngaynhapkho/ngayxuatkho cua dsLOTofKHSX
        private static (int, string) UpdateNgayNhapXuatKho_dsLOTofKHSX(object? khsxid, LenhXuatKho targetLXK)
        {
            Dictionary<string, object?> parameters = new() { { Models.KHSXs.KHSX_LOT.DBName.KHSXID, khsxid }, { Models.KHSXs.KHSX_LOT.DBName.NgayNhapKho, targetLXK.NgayNhapKho.Value } };
            // ---> chi load nhung lotKHSX theo ngay nhap kho
            (var resultdslots, string getError) = SQLServerServices.GetListLOT_khsx(parameters);

            if (resultdslots != null && resultdslots.Any())
            {
                if (targetLXK != null)
                {
                    foreach (var lot in resultdslots)
                    {
                        //lot.NgayNhapKho.Value = targetLXK.ViTriofNVL.NgayNhapKho.Value; --> Da get ngaynhapkho luc tao KHSX
                        lot.NgayXuatKho.Value = targetLXK.NgayXuatKho.Value;

                        (int updatelot, string updateloterr) = SQLServerServices.UpdateLOT_khsx(lot);

                        if (updatelot == -1)
                        {
                            return (-1, updateloterr);
                        }
                    }

                    return (1, "Success!");
                }

                return (-1, "Error!");

            }
            else return (-1, "Danh sách LOT của KHSX không tồn tại!");
        }

        // Handle kiem ke
        public static async Task<(int, string)> OnHandleLenhKiemKe(ViTriofNVL viTriofNVL)
        {
            return await Task.Run(() =>
            {
                int slhientai = int.Parse(viTriofNVL.VTNVLSoLuong.Value?.ToString() ?? "0");
                int slafteruUpdated = int.Parse(viTriofNVL.tempVTNVLSoLuong.ToString() ?? "0");

                viTriofNVL.VTNVLSoLuong.Value = viTriofNVL.tempVTNVLSoLuong;

                // Update so luong vi tri cua nvl
                (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateViTriOfNgVatLieu(viTriofNVL);

                if (updateVTofNVLresult == -1)
                {
                    return (-1, "Không thể cập nhật số lượng");
                }
                else
                {
                    // Logging update kho
                    HistoryXNKho logUpdate = new HistoryXNKho()
                    {
                        LogLoaiPhieu = { Value = Common.LogTypeKiemKe },
                        LogMaPhieu = { Value = string.Empty },
                        LogMaViTri = { Value = viTriofNVL.VitriInfor.MaViTri.Value?.ToString() },
                        LogNgThucHien = { Value = "Admin" },
                        LogTonKhoTruoc = { Value = viTriofNVL.NgLieuInfor.TonKho },
                        LogSoLuong = { Value = Math.Abs(slafteruUpdated - slhientai) },
                        LogTonKhoSau = { Value = viTriofNVL.NgLieuInfor.TonKho + (slafteruUpdated - slhientai) },
                        LogTenNVL = { Value = viTriofNVL.NgLieuInfor.MaNVL.Value?.ToString() },
                        LogThoiDiem = { Value = DateTime.Now },
                        LotVitri = { Value = viTriofNVL.LotVitri.Value?.ToString() },
                        NVLID = { Value = viTriofNVL.NVLID.Value?.ToString() },
                        VTID = { Value = viTriofNVL.VTID.Value?.ToString() },
                        QRIDLOT = { Value = viTriofNVL.QRIDLOT.Value?.ToString() }
                    };
                    // Insert loggingupdate to Database
                    (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logUpdate);
                    if (logId == -1) { }


                    // Log kiem ke
                    LogKiemKe logKiemKe = new()
                    {
                        VTofNVLID = { Value = viTriofNVL.VTofNVLID.Value },
                        SLTruoc = { Value = slhientai },
                        SLSau = { Value = slafteruUpdated },
                        NgayKiemKe = { Value = logUpdate.LogThoiDiem.Value }
                    };
                    // Insert loggingkiemke to Database
                    (int logkkid, string logkkErr) = SQLServerServices.InsertLogKiemKe(logKiemKe);
                    if (logkkid == -1) { }

                    return (1, "Cập nhật thành công!");
                }
            });
        }
    }
}
