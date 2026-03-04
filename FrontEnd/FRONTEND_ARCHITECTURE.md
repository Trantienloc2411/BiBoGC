# Kiến trúc Frontend & Đề xuất Kỹ thuật (Flutter)

Tài liệu này định nghĩa cấu trúc mã nguồn và công nghệ sẽ sử dụng để đảm bảo tính mở rộng, dễ bảo trì và hiệu năng cao cho ứng dụng Grocery Management.

## 1. Tech Stack
-   **Framework**: Flutter (Stable channel).
-   **Ngôn ngữ**: Dart.
-   **Min SDK**: Android 21 (Android 5.0) trở lên.

## 2. Thư viện cốt lõi (Dependencies)
| Nhóm | Thư viện đề xuất | Mục đích |
| :--- | :--- | :--- |
| **State Management** | `flutter_bloc` & `equatable` | Quản lý trạng thái chặt chẽ, dễ test, chuẩn doanh nghiệp. |
| **Dependency Injection** | `get_it` & `injectable` | Quản lý phụ thuộc, tách biệt các layer. |
| **Navigation** | `go_router` | Quản lý điều hướng (Deep link, Web support nếu cần). |
| **Local Database** | `isar` hoặc `hive` | NoSQL, siêu nhanh, hỗ trợ Offline-first, search text tốt. |
| **Networking** | `dio` & `retrofit` (nếu cần sync API) | Gọi API RESTful. |
| **Hardware** | `mobile_scanner` (Camera), `esc_pos_utils_plus` (In nhiệt), `printing` (In PDF) | Tương tác phần cứng POS. |
| **UI Kit** | `google_fonts`, `flutter_svg`, `intl` | Font, Icon vector, Định dạng tiền tệ/ngày tháng. |

## 3. Cấu trúc Thư mục (Clean Architecture)
Dự án sẽ được tổ chức theo Feature-based Clean Architecture để dễ dàng mở rộng và làm việc nhóm.

```text
lib/
├── main.dart                  # Điểm khởi chạy app.
├── app.dart                   # Cấu hình App (Theme, Router, Locale).
├── core/                      # Các thành phần dùng chung toàn app.
│   ├── config/                # Env vars, Flavor config.
│   ├── constants/             # AppColors, AppTextStyles, AssetPaths.
│   ├── error/                 # Failure classes, Exception handler.
│   ├── utils/                 # Validators, Formatters (Tiền tệ, Date).
│   └── widgets/               # Common Widgets (AppButton, AppInput, Loading).
│
└── features/                  # Chia theo nghiệp vụ logic.
    ├── auth/                  # Đăng nhập, Quản lý session.
    ├── product/               # Quản lý Sản phẩm & Biến thể.
    │   ├── data/              # Repositories impl, Data models, Local/Remote sources.
    │   ├── domain/            # Entities, Usecases, Repository interfaces.
    │   └── presentation/      # BLoCs, Pages (ui), Widgets (ui con).
    ├── sales/                 # POS, Giỏ hàng, Thanh toán.
    ├── inventory/             # Nhập kho, Kiểm kho.
    └── report/                # Báo cáo, Biểu đồ.
```

## 4. Quy ước Coding (Guidelines)
-   **Naming**: camelCase cho biến/hàm, PascalCase cho Class/Widget, snake_case cho file.
-   **Component hóa**: Tách nhỏ widget. File UI không quá 300 dòng.
-   **Responsive**: Sử dụng `LayoutBuilder` hoặc thư viện responsive để check kích thước màn hình (Phone vs Tablet).
    -   *Phone*: Dùng `ListView`, `Column`.
    -   *Tablet*: Dùng `GridView`, `Row` (Master-Detail).

## 5. Lưu đồ dữ liệu (Data Flow)
1.  **UI** gọi **BLoC** (Event).
2.  **BLoC** gọi **Usecase**.
3.  **Usecase** gọi **Repository**.
4.  **Repository** lấy data từ **Local DB** (ưu tiên hiển thị ngay) hoặc **Remote API**.
5.  Data trả về ngược lại và **UI** render lại (State).

Kế hoạch này đảm bảo ứng dụng chạy mượt, code dễ đọc cho dev sau này tiếp nhận.
