using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize]
    public class PhongKhamController : Controller
    {
        private readonly ClinicBookContext _context;

        public PhongKhamController(ClinicBookContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.PhongKhams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    (p.TenPhongKham != null && p.TenPhongKham.Contains(search)) ||
                    (p.DiaChi != null && p.DiaChi.Contains(search)) ||
                    (p.SoDienThoai != null && p.SoDienThoai.Contains(search)) ||
                    (p.Email != null && p.Email.Contains(search))
                );
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = totalPages > 0
                ? Math.Clamp(page, 1, totalPages)
                : 1;

            var danhSach = await query
                .OrderBy(p => p.MaPhongKham)
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

            var phongKham = await _context.PhongKhams
                .FirstOrDefaultAsync(p => p.MaPhongKham == id);

            if (phongKham == null)
                return NotFound();

            return View(phongKham);
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
            [Bind("TenPhongKham,DiaChi,SoDienThoai,Email,MoTa,TrangThai")] PhongKham phongKham)
        {
            if (ModelState.IsValid)
            {
                _context.PhongKhams.Add(phongKham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(phongKham);
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var phongKham = await _context.PhongKhams.FindAsync(id);

            if (phongKham == null)
                return NotFound();

            return View(phongKham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaPhongKham,TenPhongKham,DiaChi,SoDienThoai,Email,MoTa,TrangThai")] PhongKham model)
        {
            if (id != model.MaPhongKham)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var phongKham = await _context.PhongKhams.FindAsync(id);

            if (phongKham == null)
                return NotFound();

            phongKham.TenPhongKham = model.TenPhongKham;
            phongKham.DiaChi = model.DiaChi;
            phongKham.SoDienThoai = model.SoDienThoai;
            phongKham.Email = model.Email;
            phongKham.MoTa = model.MoTa;
            phongKham.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var phongKham = await _context.PhongKhams
                .FirstOrDefaultAsync(p => p.MaPhongKham == id);

            if (phongKham == null)
                return NotFound();

            return View(phongKham);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phongKham = await _context.PhongKhams.FindAsync(id);

            if (phongKham == null)
                return NotFound();

            _context.PhongKhams.Remove(phongKham);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}