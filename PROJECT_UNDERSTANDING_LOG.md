# Nhật ký hiểu dự án ProcessManagement

_Cập nhật lần cuối: 2025-11-13_

## 1. Tổng quan giải pháp
- **Stack công nghệ**: ASP.NET Core 8.0, Blazor Server, Entity Framework Core (chỉ dùng cho Identity), ADO.NET thuần (SqlClient), bộ điều khiển UI Radzen, Blazor Bootstrap.
- **Mục tiêu chính**: Quản lý kế hoạch sản xuất (KHSX), tồn kho kho nguyên vật liệu và thành phẩm, nhân sự, máy móc cùng các tác vụ hỗ trợ. Hệ thống cung cấp dashboard, tích hợp thiết bị cầm tay, tiện ích QR/modbus.
- **Cấu trúc solution**: Một project duy nhất `ProcessManagement`. Thành phần trọng tâm: `Pages` (UI Blazor), `Models` (định nghĩa miền dữ liệu), `Services` (tầng truy cập dữ liệu/hạ tầng), `Controllers` (REST API), `Commons` (hằng số & helper), tài nguyên tĩnh trong `wwwroot`.

## 2. Khởi động ứng dụng & hạ tầng (`Program.cs`)
- Cấu hình `AppDbContext` kết nối SQL Server qua `DBConnectionString`. Identity scaffold với chính sách mật khẩu nới lỏng, seed sẵn role (`admin`, `client`).
- Đăng ký các dịch vụ Radzen, Blazor Bootstrap, Blazor Server, controllers và `HttpClient` đặt tên (`Common.ServerAPI`) trỏ về chính ứng dụng.
- Dịch vụ tuỳ chỉnh: `SQLServerServices` (singleton, truy vấn legacy), `SQLServerServicesV2` (scoped, tối ưu truy xuất), `ChartLoggingService`, socket server (`ServerSocketAsync`), `QRCodeServices`, `ModbusServices`, `ViTriofTPhamApiService`, Radzen UI services và `LoggingCircuitHandler`.
- Ứng dụng bật CORS (allow all) cho API, cấu hình authentication/authorization, map controllers + Blazor hub + fallback. Sử dụng `PortFinder` bind cổng động và tự mở trình duyệt. Đăng ký xử lý ngoại lệ toàn cục qua `AppDomain.CurrentDomain.UnhandledException` và `TaskScheduler.UnobservedTaskException`.

## 3. Chiến lược mô hình dữ liệu
- `AppDbContext` kế thừa `IdentityDbContext<AppUser>`; các bảng nghiệp vụ khác không dùng EF mà ánh xạ thủ công. Mỗi model trong `Models/**` bao bọc cột DB bằng kiểu `Propertyy` (tên, kiểu, giá trị, cờ) để map linh hoạt.
- `Commons/Common.cs` khai báo hằng số bảng/cột, sự kiện chia sẻ, helper (parse ngày, sinh mã, bỏ dấu tiếng Việt) và state tĩnh cho item đang chọn.
- Namespace `Models` phân theo miền: `KHSX` & các thư mục con (`KHSXs`, `TienDoGCs`, ...), `KHO_NVL` (nhập/xuất, tracking, vị trí), `KHO_TPHAM`, `SANPHAM`, `NHANVIEN`, `MAYMOC`, ... Quan hệ giữa thực thể quản lý qua `List<Propertyy>` và danh sách thuần; logic nghiệp vụ (ví dụ `KHSX.GetSLcongdoantruoc`) thao tác trên các list này thay vì navigation property của EF.

## 4. Tầng truy cập dữ liệu (`Services/SQLServer`)
- **`SQLServerServices`** (legacy): dịch vụ đơn khối bao trùm toàn bộ truy vấn ADO.NET/SQL thủ công cho mọi khu vực nghiệp vụ (kế hoạch sản xuất, kho, logging...). Sử dụng helper `PropertyMapper` để map `SqlDataReader` sang model.
- **`SQLServerServicesV2`**: lớp tối ưu phục vụ `PageDSachKHSXs_V2`. Cải tiến chính:
  - Batch load dữ liệu liên quan (KHSX, NVL, Công đoạn, LOT) tránh N+1 query.
  - Dùng `IMemoryCache` với cache ngắn hạn (vd `GetKHSXbyIDReduceTimeAsync`).
  - Thực hiện tổng hợp (SUM) trực tiếp trên SQL (vd kết quả KQGC) thay vì cộng dồn trong C#.
  - Rơi về dịch vụ legacy khi gặp tác vụ cập nhật phức tạp hoặc chưa migrate.
- Các helper (`Helpers/QueryBuilder`, `PropertyMapper`) xây dựng câu lệnh IN và map dữ liệu vào `Propertyy`. Dịch vụ ghi log thời gian thực thi SQL qua `Console` để profiling.

## 5. Kiến trúc UI (`Pages/`)
- Trang Blazor Server dùng Radzen mạnh mẽ (card, tab, grid, dialog). Phân chia theo nghiệp vụ:
  - `KehoachSX/`: dashboard kế hoạch sản xuất (`PageDSachKHSXs_V2.razor`) với nhiều tab (biểu đồ, chi tiết, lot NVL, theo dõi sản xuất). Tập trung hiển thị realtime, toggle thu gọn, tìm kiếm async, dialog sự kiện.
  - `Manager_NVL/`, `Kho_ThanhPham/`, `Manager_SanPham/`, `Manager_MayMoc/`, `Manager_NhanVien/`, `Manager_NguyenCong/`: module CRUD cho từng đối tượng, chủ yếu Radzen DataGrid + dialog.
  - `MobileDevice/`: giao diện thân thiện cho thiết bị cầm tay (quét mã).
  - Các component chung dưới `Pages/KehoachSX/*` (ví dụ `ChartTracking`, `DialogDetailKHSX`, `DanhSachLotNVL`, `TheoDoiSanXuat`) được import vào trang chính.
- Layout: `Shared/MainLayout.razor` áp dụng `[Authorize(Roles = "Admin, User")]`, dựng RadzenLayout, ẩn sidebar khi role là `User`/`Common.UserDongThung`. `NavigatorMenu.razor` định nghĩa menu điều hướng.
- Tài nguyên giao diện nằm ở `wwwroot/css`, scripts hỗ trợ trong `wwwroot/js`.

## 6. Tầng API (`Controllers/`)
- `UserController`, `RoleController`, `ViTriofTPhamController`, `ViTriTPhamController` cung cấp REST API. Tận dụng ASP.NET Identity (`UserManager`, `RoleManager`) hoặc dịch vụ thủ công để quản lý tài khoản, vị trí.
- Controllers được map qua `app.MapControllers()` và chạy chung host với ứng dụng Blazor.

## 7. Dịch vụ tích hợp
- **Socket service (`Services/SocketService/ServerSocketAsync`)**: TCP server phục vụ thiết bị cầm tay. Xử lý các lệnh:
  - Tải chi tiết phiếu nhập/xuất kho (`PNK_LOAD`, `PXK_LOAD`) và thực thi lệnh kho theo từng lot/vị trí.
  - Hỗ trợ quy trình nhập/xuất tự động và thủ công, đồng thời ghi log lịch sử (`HistoryXNKho`).
  - Kiểm tra tồn kho theo thời gian thực trả về thông tin NVL/vị trí.
  - Quản lý danh sách kết nối, vòng lặp nhận async, sự kiện cập nhật UI.
- **Modbus (`Services/Modbus`)**: bọc thư viện `EasyModbusTCP.NETCore` để giao tiếp máy móc.
- **QRCodeServices**: tạo mã QR qua `QRCoder`.
- **Xuất Excel**: dùng `NPOI` với template `.xlsx` lưu trong `wwwroot/ExportTemplate`.
- **Logging**: `LoggingCircuitHandler` theo dõi vòng đời circuit Blazor; `ChartLoggingService` phục vụ ghi nhận dữ liệu chart.

## 8. Identity & bảo mật
- Giao diện Identity mặc định nằm trong `Areas/Identity/Pages`.
- Role được seed ngay khi khởi động; quyền hiển thị layout và `[Authorize]` trên trang (vd `PageDSachKHSXs_V2` chỉ cho Admin) phụ thuộc role.
- Xác thực sử dụng cookie của Blazor Server; chính sách mật khẩu cố ý nới lỏng.

## 9. Triển khai & cấu hình
- Cấu hình lưu trong `appsettings.json` và các biến thể môi trường. Hồ sơ publish nằm ở `Properties/PublishProfiles`, bản build sẵn tại `Publish/win-x64`.
- Bind cổng động ⇒ triển khai production cần mở firewall hoặc cấu hình reverse proxy. Comment trong code cho biết bật/tắt auto-launch tuỳ môi trường.

## 10. Tài liệu/phụ trợ đáng chú ý
- `IMPLEMENTATION_GUIDE.md`, `SQLSERVER_IMPROVEMENT_PROPOSAL.md`: tài liệu nội bộ & kế hoạch tối ưu SQL.
- Thư mục `Task Note/`: ghi chú cú pháp, thay đổi DB, nhắc việc.
- `ProcessManagement.http`: bộ request mẫu dùng kiểm thử API.

## 11. Ghi chú & lưu ý tương lai
- **Mô hình ORM thủ công**: Dùng `Propertyy` rộng khắp ⇒ phải giữ đồng bộ tên cột/hằng số; đổi tên cột cần chỉnh cả hằng và logic map.
- **Hiệu năng**: Bản V2 nhấn mạnh batch load, cache, aggregation SQL – rất quan trọng với dữ liệu lớn (`PageDSachKHSXs_V2`). Khi thêm thao tác ghi dữ liệu phải nhớ làm mới cache.
- **UI hướng sự kiện**: `Commons.Common` cung cấp event tĩnh (vd `ReloadPXKEvent`, `ClickSaveEvent`) để các component cập nhật chéo; cần reset event đúng lúc để tránh rò rỉ.
- **Luồng socket**: Thiết bị cầm tay là trung tâm; thay đổi payload phải đảm bảo tương thích hoặc cập nhật firmware client.
- **Bảo mật**: CORS mở toàn bộ origin và một số controller chưa bật `[Authorize]`. Cần kiểm tra kỹ khi public.
- **Kiểm thử**: Chưa thấy test tự động; dựa vào kiểm thử thủ công bằng file HTTP và log.

## 12. Phím tắt điều hướng nhanh
- **Startup**: `Program.cs`
- **Hằng số/dịch vụ dùng chung**: `Commons/Common.cs`
- **Trang kế hoạch sản xuất**: `Pages/KehoachSX/PageDSachKHSXs_V2.razor`
- **Dịch vụ dữ liệu legacy**: `Services/SQLServer/SQLServerServices.cs`
- **Dịch vụ dữ liệu tối ưu**: `Services/SQLServer/SQLServerServicesV2.cs`
- **Socket server**: `Services/SocketService/ServerSocketAsync.cs`
- **Model Identity**: `Models/AppUser.cs`, `Areas/Identity/Pages/*`

Tài liệu này đủ để nắm nhanh kiến trúc dự án và biết nơi cần đi sâu khi mở rộng chức năng hoặc debug.