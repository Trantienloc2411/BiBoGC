# Hướng dẫn Import Jira CSV

## Vấn đề

Jira CSV Import yêu cầu **Parent phải là Issue Key đã tồn tại** (ví dụ: PROJ-123), không thể dùng Summary text.

## Giải pháp: Import 3 bước

### Bước 1: Import Epics

1. Vào **Project Settings** → **External System Import** → **CSV**
2. Chọn file: `step1-epics.csv`
3. Map columns:
   - Summary → Summary
   - Issue Type → Issue Type  
   - Description → Description
   - Labels → Labels
4. **Import**
5. Ghi lại Issue Keys của các Epics đã tạo:
   - `[EPIC] Phase 1 - Inventory Management` → **PROJ-XXX**
   - `[EPIC] Phase 2 - Sales Management` → **PROJ-XXX**
   - `[EPIC] Phase 3 - Advanced Features` → **PROJ-XXX**

### Bước 2: Import Stories

1. Import file: `step2-stories.csv`
2. Map columns:
   - Summary → Summary
   - Issue Type → Issue Type
   - Description → Description
   - Labels → Labels
   - Story point estimate → Story point estimate
3. **Bỏ qua Parent** (không map)
4. **Import**

### Bước 3: Link Stories vào Epics

**Cách 1: Bulk Edit (Nhanh)**
1. Vào Board → Backlog
2. Filter theo label: `phase-1`
3. Chọn tất cả Stories
4. **Bulk Change** → **Edit Issues**
5. Set **Parent** = Epic Key của Phase 1
6. Lặp lại cho `phase-2`, `phase-3`

**Cách 2: Thủ công (Chính xác)**
- Mở từng Story
- Set Parent/Epic Link = Epic tương ứng

### Bước 4: Import Sub-tasks (Optional)

⚠️ **Sub-tasks trong Jira PHẢI có Parent là Story đã tồn tại**

**Cách tốt nhất:**
1. KHÔNG import sub-tasks qua CSV
2. Tạo sub-tasks thủ công trong từng Story
3. Hoặc dùng Jira API để tạo bulk

**Nếu vẫn muốn track sub-tasks:**
- Import file `step3-subtasks.csv` với Issue Type = **Task** (thay vì Sub-task)
- Dùng Labels để nhóm với Story (đã ghi PARENT trong Description)
- Sau đó link thủ công

---

## CSV Files

| File | Nội dung | Số records |
|------|----------|------------|
| `step1-epics.csv` | 4 Epics | 4 |
| `step2-stories.csv` | Tất cả Stories (Phase 1, 2, 3) | 27 |
| `step3-subtasks.csv` | Sub-tasks (import như Task) | 41 |

---

## Labels Structure

Dùng Labels để filter và nhóm:

| Label | Mô tả |
|-------|-------|
| `phase-1` | Phase 1 items |
| `phase-2` | Phase 2 items |
| `phase-3` | Phase 3 items |
| `priority-high` | High priority |
| `priority-medium` | Medium priority |
| `priority-low` | Low priority |
| `backend` | Backend development |
| `frontend` | Frontend development |
| `api` | API endpoint |
| `bug-fix` | Bug fix |
| `feature` | New feature |

---

## Sau khi Import

1. ✅ Set Sprint cho các items
2. ✅ Assign cho team members
3. ✅ Set Due dates
4. ✅ Move items sang đúng status (To Do, In Progress, Done)
5. ✅ Link Stories vào Epics

---

## Troubleshooting

### "doesn't have a valid Parent selection"
- **Nguyên nhân**: Parent chưa tồn tại trong Jira
- **Giải pháp**: Import mà không có Parent, sau đó link thủ công

### Sub-tasks không import được
- **Nguyên nhân**: Sub-task BẮT BUỘC phải có Parent là issue đã tồn tại
- **Giải pháp**: Import như Task, sau đó convert hoặc tạo thủ công

### Labels không hiện
- Kiểm tra Labels field đã được enable trong project
- Đảm bảo format: `label1;label2;label3` (dùng `;` phân cách)
