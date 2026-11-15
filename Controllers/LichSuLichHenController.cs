using System.Security.Claims;
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize]
    public class LichSuLichHenController : Controller
    {
        private readonly ClinicBookContext _context;

        public LichSuLichHenController(ClinicBookContext context)
        {
            _context = context;
        }

        private int GetMaTaiKhoan()
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int id
            ) ? id : 0;
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.LichSuLichHens
                .Include(l => l.MaLichHenNavigation)
                .Include(l => l.MaTaiKhoanNavigation)
                .AsQueryable();

            if (User.IsInRole("BacSi"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                query = query.Where(l =>
                    _context.BacSis.Any(b =>
                        b.MaTaiKhoan == maTaiKhoan &&
                        b.MaBacSi == l.MaLichHenNavigation.MaBacSi
                    )
                );
            }
            else if (User.IsInRole("BenhNhan"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                query = query.Where(l =>
                    _context.BenhNhans.Any(b =>
                        b.MaTaiKhoan == maTaiKhoan &&
                        b.MaBenhNhan == l.MaLichHenNavigation.MaBenhNhan
                    )
                );
            }
            else if (!User.IsInRole("QuanTriVien") &&
                     !User.IsInRole("NhanVienTiepNhan"))
            {
                return Forbid();
            }

            return View(await query
                .OrderByDescending(l => l.ThoiGianThayDoi)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var lichSu = await _context.LichSuLichHens
                .Include(l => l.MaLichHenNavigation)
                .Include(l => l.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(l => l.MaLichSu == id);

            if (lichSu == null)
                return NotFound();

            if (User.IsInRole("BacSi"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                bool coQuyen = await _context.BacSis.AnyAsync(b =>
                    b.MaTaiKhoan == maTaiKhoan &&
                    b.MaBacSi == lichSu.MaLichHenNavigation.MaBacSi
                );

                if (!coQuyen)
                    return Forbid();
            }
            else if (User.IsInRole("BenhNhan"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                bool coQuyen = await _context.BenhNhans.AnyAsync(b =>
                    b.MaTaiKhoan == maTaiKhoan &&
                    b.MaBenhNhan == lichSu.MaLichHenNavigation.MaBenhNhan
                );

                if (!coQuyen)
                    return Forbid();
            }
            else if (!User.IsInRole("QuanTriVien") &&
                     !User.IsInRole("NhanVienTiepNhan"))
            {
                return Forbid();
            }

            return View(lichSu);
        }

        // Lá»‹ch sá»­ lÃ  dá»¯ liá»‡u kiá»ƒm toÃ¡n, khÃ´ng cho sá»­a hoáº·c xÃ³a trá»±c tiáº¿p.
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi,BenhNhan")]
        public IActionResult Create()
        {
            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi,BenhNhan")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LichSuLichHen input)
        {
            return Forbid();
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi,BenhNhan")]
        public IActionResult Edit(int? id)
        {
            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi,BenhNhan")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, LichSuLichHen input)
        {
            return Forbid();
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi,BenhNhan")]
        public IActionResult Delete(int? id)
        {
            return Forbid();
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan,BacSi,BenhNhan")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return Forbid();
        }
    }
}