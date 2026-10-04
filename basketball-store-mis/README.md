# Hệ thống thông tin quản lý cửa hàng dụng cụ bóng rổ

Đồ án gồm: **ứng dụng C# Windows Forms + SQL Server** (kiến trúc 3 lớp), slide thuyết trình và bản demo web.

## Cấu trúc
```
BasketballStore.sln            Mở bằng Visual Studio 2012 trở lên
BasketballStore/
  Models/   Thực thể (SanPham, NhanVien, DongHang...)
  DAL/      Truy cập dữ liệu: Db.cs (tham số hóa, giao dịch), Repos.cs
  BLL/      Nghiệp vụ: băm mật khẩu PBKDF2, đăng nhập, phân quyền, kiểm tra dữ liệu, thanh toán
  GUI/      Giao diện: đăng nhập, form MDI, sản phẩm, bán hàng, nhập hàng, báo cáo
docs/schema.sql                Tạo CSDL, ràng buộc, trigger, dữ liệu mẫu
index.html                     Slide thuyết trình (mở bằng trình duyệt)
app.html                       Bản demo web chạy trên trình duyệt (không cần CSDL)
```

## Cách chạy ứng dụng WinForms
1. Mở SQL Server Management Studio, mở `docs/schema.sql`, bấm **Execute** (chạy một lần).
2. Mở `BasketballStore/App.config`, sửa `Data Source` cho đúng máy
   (ví dụ `.`, `.\SQLEXPRESS` hoặc `(localdb)\v11.0`).
3. Mở `BasketballStore.sln` bằng Visual Studio, bấm **F5**.
4. Đăng nhập (tài khoản mẫu được tạo tự động ở lần chạy đầu):

| Tài khoản | Mật khẩu | Vai trò | Quyền |
|---|---|---|---|
| admin | admin123 | Admin | Tất cả chức năng, báo cáo |
| thungan | thungan123 | ThuNgan | Bán hàng, xem sản phẩm |
| thukho | thukho123 | ThuKho | Nhập hàng, quản lý sản phẩm |

Hãy đổi mật khẩu mẫu trước khi dùng thật.

## Điểm kỹ thuật chính
- Kiến trúc 3 lớp GUI / BLL / DAL, tham số hóa mọi truy vấn (chống SQL Injection).
- Mật khẩu băm PBKDF2 + salt ngẫu nhiên, so sánh thời gian cố định.
- Phân quyền theo vai trò: menu chỉ hiện chức năng được phép.
- Bán hàng và nhập hàng chạy trong giao dịch; trigger tự cập nhật tồn kho,
  ràng buộc CHECK chặn bán vượt tồn.
- Lưu giá vốn tại thời điểm bán để báo cáo lợi nhuận chính xác.
- Không xóa cứng sản phẩm (đổi trạng thái ngừng bán) để giữ lịch sử hóa đơn.
- In hóa đơn (PrintPreview), tích điểm khách hàng theo SĐT, xuất báo cáo CSV.

## Đẩy lên GitHub
```bash
git init
git add .
git commit -m "Them ung dung WinForms, CSDL, slide va demo web"
git branch -M main
git remote add origin https://github.com/thaiphong261205-dev/basketball-store-mis.git
git push -u origin main
```
Bật xem web: Settings → Pages → Branch `main` / root → Save. Slide: `/index.html`, demo: `/app.html`.
