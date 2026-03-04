# Kế hoạch & Đặc tả Thiết kế UI/UX - BiBo's Grocery App

## 1. Tổng quan & Nguyên lý Thiết kế
**Mục tiêu**: Xây dựng ứng dụng quản lý tạp hóa tối ưu cho người không rành công nghệ.  
**Nền tảng**: Android (Mobile & Tablet).  
**Triết lý**:  
- **Mobile-First**: Tối ưu thao tác chạm 1 tay trên điện thoại.
- **Rõ ràng & Minh bạch**: Phản hồi tức thì, không ẩn thông tin quan trọng.
- **Tối giản**: Chỉ hiển thị những gì cần thiết tại thời điểm đó.

## 2. Hệ thống Design Tokens (Design System)

### 2.1 Màu sắc (Color Palette)
Hỗ trợ Light/Dark mode với độ tương phản cao.

| Token | Light Mode | Dark Mode | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| **Primary** | `#0069d2` (Xanh dương) | `#4da3ff` (Xanh sáng) | Màu thương hiệu, hành động chính |
| **On Primary** | `#ffffff` | `#003366` | Chữ trên nền primary |
| **Background** | `#f5f7fa` | `#000e23` (Xanh đen sâu) | Nền màn hình chính |
| **Surface** | `#ffffff` | `#0f1f38` | Nền thẻ, dialog, bottom sheet |
| **Text Primary** | `#1a1c1e` | `#e2e2e6` | Tiêu đề, nội dung chính |
| **Text Secondary**| `#444746` | `#c4c6d0` | Mô tả phụ, nhãn |
| **Error** | `#ba1a1a` | `#ffb4ab` | Báo lỗi, nút xóa, cảnh báo quan trọng |
| **Success** | `#1aa260` | `#6dd58c` | Thành công, tiền dương |
| **Warning** | `#d97706` | `#ffb74d` | Cảnh báo tồn thấp, sắp hết hạn |

### 2.2 Typography
Font chữ: **Inter** hoặc **Roboto** (Hỗ trợ tiếng Việt tốt).
- **Display**: Bold, 24sp - 32sp (Tổng tiền, Tiêu đề lớn)
- **Heading**: Medium, 18sp - 20sp (Tên màn hình, tên Section)
- **Body**: Regular, 16sp (Nội dung chính - Kích thước lớn dễ đọc)
- **Label**: Medium, 14sp (Nút bấm, Input label)
- **Caption**: Regular, 12sp (Ghi chú nhỏ)

### 2.3 Thành phần giao diện (Components)
- **Button**:
    - Chiều cao tối thiểu `48dp` (vùng chạm `56dp`).
    - Bo góc `8dp` hoặc `12dp` (thân thiện).
    - **Primary Button**: Nền xanh, chữ trắng, dùng cho "Thanh toán", "Lưu", "Xác nhận".
    - **Danger Button**: Nền đỏ nhạt (hoặc text đỏ), dùng cho "Xóa", "Hủy".
- **Product Card**:
    - Hiển thị: Ảnh (trái), Tên (đậm), Giá (màu nổi bật), Tồn kho.
    - Badge: "Sắp hết hạn", "Tồn thấp".
- **Product Variant Selector**:
    - Không dùng dropdown nhỏ.
    - Dùng **Bottom Sheet** hoặc **Chips** lớn để chọn biến thể (Ví dụ: [Chai 1.5L] [Lon 330ml]).
- **Input Field**:
    - Border rõ ràng, Label luôn hiển thị (không chỉ placeholder).
    - Bàn phím số tự bật cho trường giá/số lượng.
    - Input Barcode: Có nút icon Scan ngay bên cạnh.

## 3. Cấu trúc Màn hình & Navigation

### 3.1 Navigation (Điều hướng)
- **Phone**: Bottom Navigation Bar (Tổng quan, Bán hàng, Sản phẩm, Báo cáo, Thêm).
- **Tablet**: Navigation Rail (bên trái) hoặc Master-Detail layout (Danh sách bên trái, Chi tiết bên phải).

### 3.2 Sơ đồ màn hình chính (Sitemap)
1.  **Auth**: Đăng nhập (Pin code hoặc Password đơn giản).
2.  **Dashboard (Home)**:
    -   Doanh thu hôm nay (Số to).
    -   Đơn hàng gần đây.
    -   Cảnh báo quan trọng (Hết hàng, Hết hạn).
3.  **POS (Bán hàng)** - *Quan trọng nhất*:
    -   Layout: Danh sách sản phẩm (Grid/List) + Giỏ hàng (Panel trượt hoặc Split view trên Tablet).
    -   Flow: Scan/Chọn SP -> Chọn Variant (nếu có) -> Nhập số lượng -> Thanh toán -> In/Xuất hóa đơn.
4.  **Sản phẩm (Inventory)**:
    -   Danh sách Product (Cha).
    -   Chi tiết Product -> Danh sách Variants (Con).
    -   Màn hình Thêm/Sửa: Tách rõ "Thông tin chung" và "Các biến thể".
5.  **Báo cáo**:
    -   Chart đơn giản (Cột/Đường).
    -   Xuất PDF.
6.  **Setting**: Cấu hình VAT, Máy in, Backup.

## 4. Giải pháp UX cho Product & Variants
Đây là phần lõi dữ liệu dễ gây nhầm lẫn. Giải pháp thiết kế:

1.  **Hiển thị danh sách**:
    -   Luôn gom nhóm theo Product cha.
    -   Ví dụ: "Coca Cola" (Cha) -> Bấm vào sổ ra: "Lon 330ml", "Chai 1.5L" (Con).
    -   Trên POS: Khi bấm "Coca Cola", hiện Popup chọn loại ngay lập tức.
2.  **Tạo mới**:
    -   Bước 1: Nhập tên, hãng, danh mục (Dữ liệu chung).
    -   Bước 2: "Sản phẩm này có nhiều loại không?" (Switch).
    -   Nếu Có: Chuyển sang giao diện thêm từng dòng Variant (Mỗi dòng: Tên biến thể, Mã, Giá, Tồn).
    -   Nếu Không: Nhập giá và tồn ngay màn hình chính (Tự động tạo 1 variant ẩn hoặc default).

## 5. Công nghệ Frontend Đề xuất
- **Framework**: **Flutter**.
    -   Lý do: Hiệu năng tốt cho List dài, render UI nhất quán trên Android Tablet/Phone, dễ dàng custom component đẹp, cộng đồng plugin in ấn/POS mạnh (esc_pos_printer, qr_code_scanner).
-   **Cấu trúc thư mục (Clean Architecture)**:
    -   `lib/core`: Theme, Utils, Constants.
    -   `lib/features`: Chia theo nghiệp vụ (auth, pos, product, report).
        -   Mỗi feature có: `data`, `domain`, `presentation` (bloc/provider, widgets, pages).

## 6. Kế hoạch triển khai (Roadmap Design)
1.  Setup Base Project & Assets.
2.  Build UI Kit (Buttons, Inputs, Typos, Colors).
3.  Design màn hình Product & Variant Management (Khó nhất -> Làm trước).
4.  Design màn hình POS (Phức tạp về thao tác).
5.  Design Dashboard & Report.
6.  Review & Refine (Dark mode check, Tablet responsive check).
