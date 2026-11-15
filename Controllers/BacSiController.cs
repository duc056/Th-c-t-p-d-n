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
                    (b.MaTaiKhoanNavigation != null &&
                        (
                            (b.MaTaiKhoanNavigation.HoTen != null &&
                                b.MaTaiKhoanNavigation.HoTen.Contains(search)) ||
                            (b.MaTaiKhoanNavigation.Email != null &&
                                b.MaTaiKhoanNavigation.Email.Contains(search)) ||
                            (b.MaTaiKhoanNavigation.TenDangNhap != null &&
                                b.MaTaiKhoanNavigation.TenDangNhap.Contains(search)) ||
                            (b.MaTaiKhoanNavigation.SoDienThoai != null &&
                                b.MaTaiKhoanNavigation.SoDienThoai.Contains(search))
                        )) ||
                    (b.HocVi != null && b.HocVi.Contains(search))
                );
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (totalPages > 0)
                page = Math.Clamp(page, 1, totalPages);
            else
                page = 1;

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

        [Authorize(Roles = "QuanTriVien")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Create(
            [Bind("MaTaiKhoan,MaPhongKham,HocVi,KinhNghiem,GioiThieu,TrangThai")] BacSi bacSi)
        {
            if (ModelState.IsValid)
            {
                _context.BacSis.Add(bacSi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(bacSi.MaTaiKhoan, bacSi.MaPhongKham);
            return View(bacSi);
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var bacSi = await _context.BacSis.FindAsync(id);

            if (bacSi == null)
                return NotFound();

            LoadDropdowns(bacSi.MaTaiKhoan, bacSi.MaPhongKham);
            return View(bacSi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaBacSi,MaTaiKhoan,MaPhongKham,HocVi,KinhNghiem,GioiThieu,TrangThai")] BacSi model)
        {
            if (id != model.MaBacSi)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.MaTaiKhoan, model.MaPhongKham);
                return View(model);
            }

            var bacSi = await _context.BacSis.FindAsync(id);

            if (bacSi == null)
                return NotFound();

            bacSi.MaTaiKhoan = model.MaTaiKhoan;
            bacSi.MaPhongKham = model.MaPhongKham;
            bacSi.HocVi = model.HocVi;
            bacSi.KinhNghiem = model.KinhNghiem;
            bacSi.GioiThieu = model.GioiThieu;
            bacSi.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bacSi = await _context.BacSis.FindAsync(id);

            if (bacSi == null)
                return NotFound();

            _context.BacSis.Remove(bacSi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? maTaiKhoan = null, int? maPhongKham = null)
        {
            ViewData["MaPhongKham"] = new SelectList(
                _context.PhongKhams,
                "MaPhongKham",
                "MaPhongKham",
                maPhongKham);

            ViewData["MaTaiKhoan"] = new SelectList(
                _context.TaiKhoans,
                "MaTaiKhoan",
                "MaTaiKhoan",
                maTaiKhoan);
        }
    }
}