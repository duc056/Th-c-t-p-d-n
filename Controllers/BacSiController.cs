
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize]
    public class BacSiController : Controller
    {
        private readonly ClinicBookContext _context;

        public BacSiController(ClinicBookContext context)
        {
            _context = context;
        }

        // =========================================
        // DANH SÁCH BÁC SĨ
        // =========================================
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.BacSis
                .Include(b => b.MaPhongKhamNavigation)
                .Include(b => b.MaTaiKhoanNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(b =>
                    (b.MaTaiKhoanNavigation.HoTen != null &&
                     b.MaTaiKhoanNavigation.HoTen.Contains(search)) ||
                    (b.MaTaiKhoanNavigation.Email != null &&
                     b.MaTaiKhoanNavigation.Email.Contains(search)) ||
                    (b.MaTaiKhoanNavigation.TenDangNhap != null &&
                     b.MaTaiKhoanNavigation.TenDangNhap.Contains(search)) ||
                    (b.MaTaiKhoanNavigation.SoDienThoai != null &&
                     b.MaTaiKhoanNavigation.SoDienThoai.Contains(search)) ||
                    (b.HocVi != null && b.HocVi.Contains(search))
                );
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);

            page = totalPages > 0
                ? Math.Clamp(page, 1, totalPages)
                : 1;

            var danhSach = await query
                .OrderBy(b => b.MaBacSi)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(danhSach);
        }

        // =========================================
        // CHI TIẾT BÁC SĨ
        // =========================================
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var bacSi = await _context.BacSis
                .Include(b => b.MaPhongKhamNavigation)
                .Include(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(b => b.MaBacSi == id);

            if (bacSi == null)
                return NotFound();

            return View(bacSi);
        }

        // =========================================
        // THÊM BÁC SĨ - GET
        // =========================================
        [Authorize(Roles = "QuanTriVien")]
        public IActionResult Create()
        {
            LoadDropdowns();

            return View(new BacSi
            {
                TrangThai = true
            });
        }

        // =========================================
        // THÊM BÁC SĨ - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Create(
            [Bind("MaTaiKhoan,MaPhongKham,HocVi,KinhNghiem,GioiThieu,TrangThai,NgaySinh")]
            BacSi bacSi)
        {
            // Kiểm tra tài khoản đã được gán cho bác sĩ khác chưa.
            bool taiKhoanDaGan = await _context.BacSis
                .AnyAsync(x => x.MaTaiKhoan == bacSi.MaTaiKhoan);

            if (taiKhoanDaGan)
            {
                ModelState.AddModelError(
                    "MaTaiKhoan",
                    "Tài khoản này đã được liên kết với một bác sĩ khác.");
            }

            // Kiểm tra kinh nghiệm.
            if (bacSi.KinhNghiem < 0)
            {
                ModelState.AddModelError(
                    "KinhNghiem",
                    "Số năm kinh nghiệm không được nhỏ hơn 0.");
            }
            ModelState.Remove("MaPhongKhamNavigation");
            ModelState.Remove("MaTaiKhoanNavigation");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.BacSis.Add(bacSi);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Thêm bác sĩ thành công.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể thêm bác sĩ. Vui lòng kiểm tra tài khoản, phòng khám và dữ liệu liên quan.");
                }
            }

            LoadDropdowns(bacSi.MaTaiKhoan, bacSi.MaPhongKham);

            return View(bacSi);
        }

        // =========================================
        // SỬA BÁC SĨ - GET
        // =========================================
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var bacSi = await _context.BacSis
                .FirstOrDefaultAsync(x => x.MaBacSi == id);

            if (bacSi == null)
                return NotFound();

            LoadDropdowns(
                bacSi.MaTaiKhoan,
                bacSi.MaPhongKham);

            return View(bacSi);
        }

        // =========================================
        // SỬA BÁC SĨ - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(
            [Bind("MaBacSi,MaTaiKhoan,MaPhongKham,HocVi,KinhNghiem,GioiThieu,TrangThai,NgaySinh")]
            BacSi model)
        {
            bool taiKhoanDaGan = await _context.BacSis
                .AnyAsync(x =>
                    x.MaTaiKhoan == model.MaTaiKhoan &&
                    x.MaBacSi != model.MaBacSi);

            if (taiKhoanDaGan)
            {
                ModelState.AddModelError(
                    "MaTaiKhoan",
                    "Tài khoản này đã được liên kết với bác sĩ khác.");
            }

            if (model.KinhNghiem < 0)
            {
                ModelState.AddModelError(
                    "KinhNghiem",
                    "Số năm kinh nghiệm không được nhỏ hơn 0.");
            }
            ModelState.Remove("MaPhongKhamNavigation");
            ModelState.Remove("MaTaiKhoanNavigation");
            if (!ModelState.IsValid)
            {
                LoadDropdowns(
                    model.MaTaiKhoan,
                    model.MaPhongKham);

                return View(model);
            }

            var bacSi = await _context.BacSis
                .FirstOrDefaultAsync(x => x.MaBacSi == model.MaBacSi);

            if (bacSi == null)
                return NotFound();

            try
            {
                bacSi.MaTaiKhoan = model.MaTaiKhoan;
                bacSi.MaPhongKham = model.MaPhongKham;
                bacSi.NgaySinh = model.NgaySinh;
                bacSi.HocVi = model.HocVi;
                bacSi.KinhNghiem = model.KinhNghiem;
                bacSi.GioiThieu = model.GioiThieu;
                bacSi.TrangThai = model.TrangThai;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin bác sĩ thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật bác sĩ. Vui lòng kiểm tra dữ liệu tài khoản và phòng khám.");

                LoadDropdowns(
                    model.MaTaiKhoan,
                    model.MaPhongKham);

                return View(model);
            }
        }

        // =========================================
        // XÓA BÁC SĨ - GET
        // =========================================
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var bacSi = await _context.BacSis
                .Include(b => b.MaPhongKhamNavigation)
                .Include(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(b => b.MaBacSi == id);

            if (bacSi == null)
                return NotFound();

            return View(bacSi);
        }

        // =========================================
        // XÓA BÁC SĨ - POST
        // =========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bacSi = await _context.BacSis
                .Include(x => x.BacSiChuyenKhoas)
                .Include(x => x.CaLamViecs)
                .Include(x => x.KetQuaKhams)
                .Include(x => x.LichHens)
                .Include(x => x.NgayNghis)
                .FirstOrDefaultAsync(x => x.MaBacSi == id);

            if (bacSi == null)
                return NotFound();

            bool coDuLieuLienQuan =
                bacSi.BacSiChuyenKhoas.Any() ||
                bacSi.CaLamViecs.Any() ||
                bacSi.KetQuaKhams.Any() ||
                bacSi.LichHens.Any() ||
                bacSi.NgayNghis.Any();

            if (coDuLieuLienQuan)
            {
                TempData["Error"] =
                    "Không thể xóa bác sĩ vì vẫn còn dữ liệu phân công, ca làm việc, lịch hẹn hoặc kết quả khám liên quan. Bạn nên chuyển trạng thái bác sĩ sang ngừng hoạt động.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.BacSis.Remove(bacSi);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Xóa bác sĩ thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa bác sĩ vì vẫn còn dữ liệu liên quan trong hệ thống.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // NẠP DROPDOWN TÀI KHOẢN VÀ PHÒNG KHÁM
        // =========================================
        private void LoadDropdowns(
            int? maTaiKhoan = null,
            int? maPhongKham = null)
        {
            var taiKhoans = _context.TaiKhoans
                .OrderBy(x => x.HoTen)
                .Select(x => new
                {
                    x.MaTaiKhoan,
                    TenHienThi = x.HoTen + " (" + x.TenDangNhap + ")"
                })
                .ToList();

            var phongKhams = _context.PhongKhams
                .OrderBy(x => x.TenPhongKham)
                .Select(x => new
                {
                    x.MaPhongKham,
                    x.TenPhongKham
                })
                .ToList();

            ViewData["MaTaiKhoan"] = new SelectList(
                taiKhoans,
                "MaTaiKhoan",
                "TenHienThi",
                maTaiKhoan);

            ViewData["MaPhongKham"] = new SelectList(
                phongKhams,
                "MaPhongKham",
                "TenPhongKham",
                maPhongKham);
        }
    }
}
