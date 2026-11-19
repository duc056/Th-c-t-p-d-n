
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize]
    public class BacSiChuyenKhoaController : Controller
    {
        private readonly ClinicBookContext _context;

        public BacSiChuyenKhoaController(ClinicBookContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.BacSiChuyenKhoas
                .Include(x => x.MaBacSiNavigation)
                    .ThenInclude(x => x.MaTaiKhoanNavigation)
                .Include(x => x.MaChuyenKhoaNavigation)
                .ToListAsync();

            return View(danhSach);
        }

        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Details(
            int? maBacSi,
            int? maChuyenKhoa)
        {
            if (maBacSi == null || maChuyenKhoa == null)
                return NotFound();

            var item = await _context.BacSiChuyenKhoas
                .Include(x => x.MaBacSiNavigation)
                    .ThenInclude(x => x.MaTaiKhoanNavigation)
                .Include(x => x.MaChuyenKhoaNavigation)
                .FirstOrDefaultAsync(x =>
                    x.MaBacSi == maBacSi &&
                    x.MaChuyenKhoa == maChuyenKhoa);

            if (item == null)
                return NotFound();

            return View(item);
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
            [Bind("MaBacSi,MaChuyenKhoa,LaChuyenKhoaChinh")]
            BacSiChuyenKhoa item)
        {
            bool daTonTai = await _context.BacSiChuyenKhoas.AnyAsync(x =>
                x.MaBacSi == item.MaBacSi &&
                x.MaChuyenKhoa == item.MaChuyenKhoa);

            if (daTonTai)
            {
                ModelState.AddModelError(
                    "",
                    "Bác sĩ đã được gán chuyên khoa này.");
            }

            bool bacSiTonTai = await _context.BacSis
                .AnyAsync(x => x.MaBacSi == item.MaBacSi);

            if (!bacSiTonTai)
            {
                ModelState.AddModelError(
                    "MaBacSi",
                    "Vui lòng chọn bác sĩ hợp lệ.");
            }

            bool chuyenKhoaTonTai = await _context.ChuyenKhoas
                .AnyAsync(x => x.MaChuyenKhoa == item.MaChuyenKhoa);

            if (!chuyenKhoaTonTai)
            {
                ModelState.AddModelError(
                    "MaChuyenKhoa",
                    "Vui lòng chọn chuyên khoa hợp lệ.");
            }
            ModelState.Remove("MaBacSiNavigation");
            ModelState.Remove("MaChuyenKhoaNavigation");
            if (ModelState.IsValid)
            {
                _context.BacSiChuyenKhoas.Add(item);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(item.MaBacSi, item.MaChuyenKhoa);
            return View(item);
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(
            int? maBacSi,
            int? maChuyenKhoa)
        {
            if (maBacSi == null || maChuyenKhoa == null)
                return NotFound();

            var item = await _context.BacSiChuyenKhoas.FindAsync(
                maBacSi.Value,
                maChuyenKhoa.Value);

            if (item == null)
                return NotFound();

            LoadDropdowns(item.MaBacSi, item.MaChuyenKhoa);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Edit(
            int maBacSi,
            int maChuyenKhoa,
            [Bind("MaBacSi,MaChuyenKhoa,LaChuyenKhoaChinh")]
            BacSiChuyenKhoa input)
        {
            if (maBacSi != input.MaBacSi ||
                maChuyenKhoa != input.MaChuyenKhoa)
                return NotFound();

            var item = await _context.BacSiChuyenKhoas.FindAsync(
                maBacSi,
                maChuyenKhoa);

            if (item == null)
                return NotFound();
            ModelState.Remove("MaBacSiNavigation");
            ModelState.Remove("MaChuyenKhoaNavigation");
            if (ModelState.IsValid)
            {
                item.LaChuyenKhoaChinh = input.LaChuyenKhoaChinh;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(item.MaBacSi, item.MaChuyenKhoa);
            return View(item);
        }

        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> Delete(
            int? maBacSi,
            int? maChuyenKhoa)
        {
            if (maBacSi == null || maChuyenKhoa == null)
                return NotFound();

            var item = await _context.BacSiChuyenKhoas
                .Include(x => x.MaBacSiNavigation)
                    .ThenInclude(x => x.MaTaiKhoanNavigation)
                .Include(x => x.MaChuyenKhoaNavigation)
                .FirstOrDefaultAsync(x =>
                    x.MaBacSi == maBacSi &&
                    x.MaChuyenKhoa == maChuyenKhoa);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTriVien")]
        public async Task<IActionResult> DeleteConfirmed(
            int maBacSi,
            int maChuyenKhoa)
        {
            var item = await _context.BacSiChuyenKhoas.FindAsync(
                maBacSi,
                maChuyenKhoa);

            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy phân công cần xóa.";
                return RedirectToAction(nameof(Index));
            }

            _context.BacSiChuyenKhoas.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa phân công chuyên khoa thành công.";

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(
            int? maBacSi = null,
            int? maChuyenKhoa = null)
        {
            var bacSis = _context.BacSis
                .Include(x => x.MaTaiKhoanNavigation)
                .OrderBy(x => x.MaTaiKhoanNavigation.HoTen)
                .Select(x => new
                {
                    x.MaBacSi,
                    TenHienThi = x.MaTaiKhoanNavigation.HoTen
                })
                .ToList();

            var chuyenKhoas = _context.ChuyenKhoas
                .OrderBy(x => x.TenChuyenKhoa)
                .Select(x => new
                {
                    x.MaChuyenKhoa,
                    x.TenChuyenKhoa
                })
                .ToList();

            ViewData["MaBacSi"] = new SelectList(
                bacSis,
                "MaBacSi",
                "TenHienThi",
                maBacSi);

            ViewData["MaChuyenKhoa"] = new SelectList(
                chuyenKhoas,
                "MaChuyenKhoa",
                "TenChuyenKhoa",
                maChuyenKhoa);
        }
    }
}
