# Hệ thống thông tin quản lý cửa hàng dụng cụ bóng rổ

Bài thuyết trình (trang web) và mã CSDL cho đề tài **Xây dựng hệ thống thông tin quản lý cửa hàng dụng cụ bóng rổ**.

## Cấu trúc
- `index.html` — slide trình bày (mở trực tiếp bằng trình duyệt)
- `docs/schema.sql` — script tạo CSDL SQL Server

## Cách xem slide
Mở `index.html`. Dùng phím `←` `→` hoặc `Space` để chuyển slide, `F` để toàn màn hình.

## Đẩy lên GitHub
```bash
cd basketball-store-mis
git init
git add .
git commit -m "Thêm slide và CSDL quản lý cửa hàng bóng rổ"
git branch -M main
git remote add origin https://github.com/<ten-tai-khoan>/basketball-store-mis.git
git push -u origin main
```
Bật web: GitHub → Settings → Pages → Branch `main` / thư mục `/ (root)` → Save.
Link sẽ có dạng `https://<ten-tai-khoan>.github.io/basketball-store-mis/`.
