using System.Security.Claims;
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize]
    public class CaLamViecController : Controller
    {
        private readonly ClinicBookContext _context;

        public CaLamViecController(ClinicBookContext context)
        {
            _context = context;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi")]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.CaLamViecs
                .Include(c => c.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .AsQueryable();

            // Bac si chi xem ca lam viec cua chinh minh
            if (User.IsInRole("BacSi"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                query = query.Where(c =>
                    c.MaBacSiNavigation.MaTaiKhoan == maTaiKhoan);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.ThuTrongTuan.ToString().Contains(search) ||
                    (c.MaBacSiNavigation.MaTaiKhoanNavigation != null &&
                     c.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen != null &&
                     c.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen.Contains(search))
                );
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = totalPages > 0
                ? Math.Clamp(page, 1, totalPages)
                : 1;

            var danhSach = await query
                .OrderBy(c => c.ThuTrongTuan)
                .ThenBy(c => c.GioBatDau)
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

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var caLamViec = await _context.CaLamViecs
                .Include(c => c.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(c => c.MaCaLamViec == id);

            if (caLamViec == null)
                return NotFound();

            if (User.IsInRole("BacSi") &&
                caLamViec.MaBacSiNavigation.MaTaiKhoan != GetMaTaiKhoan())
            {
                return Forbid();
            }

            return View(caLamViec);
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
            [Bind("MaBacSi,ThuTrongTuan,GioBatDau,GioKetThuc,ThoiLuongKham,TrangThai")]
            CaLamViec caLamViec)
        {
            if (ModelState.IsValid)
            {
                _context.CaLamViecs.Add(caLamViec);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(caLamViec.MaBacSi);
            return View(caLamViec);
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var caLamViec = await _context.CaLamViecs.FindAsync(id);

            if (caLamViec == null)
                return NotFound();

            LoadDropdowns(caLamViec.MaBacSi);
            return View(caLamViec);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaCaLamViec,MaBacSi,ThuTrongTuan,GioBatDau,GioKetThuc,ThoiLuongKham,TrangThai")]
            CaLamViec model)
        {
            if (id != model.MaCaLamViec)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.MaBacSi);
                return View(model);
            }

            var caLamViec = await _context.CaLamViecs.FindAsync(id);

            if (caLamViec == null)
                return NotFound();

            caLamViec.MaBacSi = model.MaBacSi;
            caLamViec.ThuTrongTuan = model.ThuTrongTuan;
            caLamViec.GioBatDau = model.GioBatDau;
            caLamViec.GioKetThuc = model.GioKetThuc;
            caLamViec.ThoiLuongKham = model.ThoiLuongKham;
            caLamViec.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var caLamViec = await _context.CaLamViecs
                .Include(c => c.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(c => c.MaCaLamViec == id);

            if (caLamViec == null)
                return NotFound();

            return View(caLamViec);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caLamViec = await _context.CaLamViecs.FindAsync(id);

            if (caLamViec == null)
                return NotFound();

            _context.CaLamViecs.Remove(caLamViec);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? maBacSi = null)
        {
            var danhSachBacSi = _context.BacSis
                .Include(b => b.MaTaiKhoanNavigation)
                .Select(b => new
                {
                    b.MaBacSi,
                    TenHienThi = b.MaTaiKhoanNavigation != null
                        ? b.MaTaiKhoanNavigation.HoTen
                        : "Bac si " + b.MaBacSi
                })
                .ToList();

            ViewData["MaBacSi"] = new SelectList(
                danhSachBacSi,
                "MaBacSi",
                "TenHienThi",
                maBacSi);
        }
    }
}