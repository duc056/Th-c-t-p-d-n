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
    public class BenhNhanController : Controller
    {
        private readonly ClinicBookContext _context;

        public BenhNhanController(ClinicBookContext context)
        {
            _context = context;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BenhNhan")]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.BenhNhans
                .Include(b => b.MaTaiKhoanNavigation)
                .AsQueryable();

            if (User.IsInRole("BenhNhan"))
            {
                int maTaiKhoan = GetMaTaiKhoan();
                query = query.Where(b => b.MaTaiKhoan == maTaiKhoan);
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(b =>
                    b.MaTaiKhoanNavigation.HoTen.Contains(search) ||
                    b.MaTaiKhoanNavigation.Email.Contains(search) ||
                    b.MaTaiKhoanNavigation.TenDangNhap.Contains(search) ||
                    (b.MaTaiKhoanNavigation.SoDienThoai != null &&
                     b.MaTaiKhoanNavigation.SoDienThoai.Contains(search)));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var danhSach = await query
                .OrderBy(b => b.MaBenhNhan)
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

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BenhNhan")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var benhNhan = await _context.BenhNhans
                .Include(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == id);

            if (benhNhan == null)
                return NotFound();

            if (User.IsInRole("BenhNhan") &&
                benhNhan.MaTaiKhoan != GetMaTaiKhoan())
            {
                return Forbid();
            }

            return View(benhNhan);
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Create(
            [Bind("MaTaiKhoan,NgaySinh,GioiTinh,DiaChi")] BenhNhan benhNhan)
        {
            if (ModelState.IsValid)
            {
                bool daCoHoSo = await _context.BenhNhans
                    .AnyAsync(b => b.MaTaiKhoan == benhNhan.MaTaiKhoan);

                if (daCoHoSo)
                {
                    ModelState.AddModelError("MaTaiKhoan",
                        "Tai khoan nay da co ho so benh nhan.");
                }
                else
                {
                    benhNhan.NgayTao = DateTime.Now;
                    _context.BenhNhans.Add(benhNhan);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            LoadDropdowns(benhNhan.MaTaiKhoan);
            return View(benhNhan);
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var benhNhan = await _context.BenhNhans.FindAsync(id);

            if (benhNhan == null)
                return NotFound();

            LoadDropdowns(benhNhan.MaTaiKhoan);
            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaBenhNhan,MaTaiKhoan,NgaySinh,GioiTinh,DiaChi")] BenhNhan model)
        {
            if (id != model.MaBenhNhan)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.MaTaiKhoan);
                return View(model);
            }

            var benhNhan = await _context.BenhNhans.FindAsync(id);

            if (benhNhan == null)
                return NotFound();

            bool biTrung = await _context.BenhNhans
                .AnyAsync(b => b.MaTaiKhoan == model.MaTaiKhoan &&
                               b.MaBenhNhan != id);

            if (biTrung)
            {
                ModelState.AddModelError("MaTaiKhoan",
                    "Tai khoan da duoc gan cho ho so khac.");
                LoadDropdowns(model.MaTaiKhoan);
                return View(model);
            }

            benhNhan.MaTaiKhoan = model.MaTaiKhoan;
            benhNhan.NgaySinh = model.NgaySinh;
            benhNhan.GioiTinh = model.GioiTinh;
            benhNhan.DiaChi = model.DiaChi;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var benhNhan = await _context.BenhNhans
                .Include(b => b.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(b => b.MaBenhNhan == id);

            if (benhNhan == null)
                return NotFound();

            return View(benhNhan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhans.FindAsync(id);

            if (benhNhan == null)
                return NotFound();

            _context.BenhNhans.Remove(benhNhan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? maTaiKhoan = null)
        {
            ViewData["MaTaiKhoan"] = new SelectList(
                _context.TaiKhoans,
                "MaTaiKhoan",
                "MaTaiKhoan",
                maTaiKhoan);
        }
    }
}