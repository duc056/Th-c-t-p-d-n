using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize]
    public class ChuyenKhoaController : Controller
    {
        private readonly ClinicBookContext _context;

        public ChuyenKhoaController(ClinicBookContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.ChuyenKhoas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    (c.TenChuyenKhoa != null && c.TenChuyenKhoa.Contains(search)) ||
                    (c.MoTa != null && c.MoTa.Contains(search))
                );
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = totalPages > 0
                ? Math.Clamp(page, 1, totalPages)
                : 1;

            var danhSach = await query
                .OrderBy(c => c.MaChuyenKhoa)
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

            var chuyenKhoa = await _context.ChuyenKhoas
                .FirstOrDefaultAsync(c => c.MaChuyenKhoa == id);

            if (chuyenKhoa == null)
                return NotFound();

            return View(chuyenKhoa);
        }

        [Authorize(Roles = "QuanTriVien")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Create(
            [Bind("TenChuyenKhoa,MoTa,TrangThai")] ChuyenKhoa chuyenKhoa)
        {
            if (ModelState.IsValid)
            {
                _context.ChuyenKhoas.Add(chuyenKhoa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(chuyenKhoa);
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var chuyenKhoa = await _context.ChuyenKhoas.FindAsync(id);

            if (chuyenKhoa == null)
                return NotFound();

            return View(chuyenKhoa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaChuyenKhoa,TenChuyenKhoa,MoTa,TrangThai")] ChuyenKhoa model)
        {
            if (id != model.MaChuyenKhoa)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var chuyenKhoa = await _context.ChuyenKhoas.FindAsync(id);

            if (chuyenKhoa == null)
                return NotFound();

            chuyenKhoa.TenChuyenKhoa = model.TenChuyenKhoa;
            chuyenKhoa.MoTa = model.MoTa;
            chuyenKhoa.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var chuyenKhoa = await _context.ChuyenKhoas
                .FirstOrDefaultAsync(c => c.MaChuyenKhoa == id);

            if (chuyenKhoa == null)
                return NotFound();

            return View(chuyenKhoa);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chuyenKhoa = await _context.ChuyenKhoas.FindAsync(id);

            if (chuyenKhoa == null)
                return NotFound();

            _context.ChuyenKhoas.Remove(chuyenKhoa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}