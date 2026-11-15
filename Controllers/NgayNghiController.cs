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
    public class NgayNghiController : Controller
    {
        private readonly ClinicBookContext _context;

        public NgayNghiController(ClinicBookContext context)
        {
            _context = context;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private bool LaBacSi()
        {
            return User.IsInRole("BacSi");
        }

        private async Task<int?> GetMaBacSiCuaTaiKhoan()
        {
            int maTaiKhoan = GetMaTaiKhoan();

            var bacSi = await _context.BacSis
                .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);

            return bacSi?.MaBacSi;
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi")]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.NgayNghis
                .Include(n => n.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .AsQueryable();

            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null)
                    return Forbid();

                query = query.Where(n => n.MaBacSi == maBacSi);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(n =>
                    (n.LyDo != null && n.LyDo.Contains(search)) ||
                    (n.MaBacSiNavigation.MaTaiKhoanNavigation != null &&
                     n.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen != null &&
                     n.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen.Contains(search)) ||
                    n.NgayNghi1.ToString().Contains(search)
                );
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = totalPages > 0
                ? Math.Clamp(page, 1, totalPages)
                : 1;

            var danhSach = await query
                .OrderBy(n => n.NgayNghi1)
                .ThenBy(n => n.GioBatDau)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(danhSach);
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ngayNghi = await _context.NgayNghis
                .Include(n => n.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(n => n.MaNgayNghi == id);

            if (ngayNghi == null)
                return NotFound();

            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null || ngayNghi.MaBacSi != maBacSi)
                    return Forbid();
            }

            return View(ngayNghi);
        }

        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null)
                    return Forbid();

                ViewData["MaBacSi"] = new SelectList(
                    await _context.BacSis.Where(b => b.MaBacSi == maBacSi).ToListAsync(),
                    "MaBacSi",
                    "MaBacSi",
                    maBacSi);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Create(
            [Bind("MaNgayNghi,MaBacSi,NgayNghi1,GioBatDau,GioKetThuc,LyDo,NgayTao")]
            NgayNghi ngayNghi)
        {
            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null)
                    return Forbid();

                ngayNghi.MaBacSi = maBacSi.Value;
            }

            if (ModelState.IsValid)
            {
                ngayNghi.NgayTao = DateTime.Now;

                _context.NgayNghis.Add(ngayNghi);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(ngayNghi.MaBacSi);
            return View(ngayNghi);
        }

        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var ngayNghi = await _context.NgayNghis.FindAsync(id);

            if (ngayNghi == null)
                return NotFound();

            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null || ngayNghi.MaBacSi != maBacSi)
                    return Forbid();
            }

            await LoadDropdowns(ngayNghi.MaBacSi);
            return View(ngayNghi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaNgayNghi,MaBacSi,NgayNghi1,GioBatDau,GioKetThuc,LyDo,NgayTao")]
            NgayNghi model)
        {
            if (id != model.MaNgayNghi)
                return NotFound();

            var ngayNghi = await _context.NgayNghis.FindAsync(id);

            if (ngayNghi == null)
                return NotFound();

            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null || ngayNghi.MaBacSi != maBacSi)
                    return Forbid();

                model.MaBacSi = maBacSi.Value;
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model.MaBacSi);
                return View(model);
            }

            ngayNghi.MaBacSi = model.MaBacSi;
            ngayNghi.NgayNghi1 = model.NgayNghi1;
            ngayNghi.GioBatDau = model.GioBatDau;
            ngayNghi.GioKetThuc = model.GioKetThuc;
            ngayNghi.LyDo = model.LyDo;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var ngayNghi = await _context.NgayNghis
                .Include(n => n.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(n => n.MaNgayNghi == id);

            if (ngayNghi == null)
                return NotFound();

            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null || ngayNghi.MaBacSi != maBacSi)
                    return Forbid();
            }

            return View(ngayNghi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ngayNghi = await _context.NgayNghis.FindAsync(id);

            if (ngayNghi == null)
                return NotFound();

            if (LaBacSi())
            {
                var maBacSi = await GetMaBacSiCuaTaiKhoan();

                if (maBacSi == null || ngayNghi.MaBacSi != maBacSi)
                    return Forbid();
            }

            _context.NgayNghis.Remove(ngayNghi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? maBacSi = null)
        {
            var danhSach = await _context.BacSis
                .Include(b => b.MaTaiKhoanNavigation)
                .Select(b => new
                {
                    b.MaBacSi,
                    TenHienThi = b.MaTaiKhoanNavigation != null
                        ? b.MaTaiKhoanNavigation.HoTen
                        : "Bac si " + b.MaBacSi
                })
                .ToListAsync();

            ViewData["MaBacSi"] = new SelectList(
                danhSach,
                "MaBacSi",
                "TenHienThi",
                maBacSi);
        }
    }
}