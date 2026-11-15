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
    public class KetQuaKhamController : Controller
    {
        private readonly ClinicBookContext _context;

        public KetQuaKhamController(ClinicBookContext context)
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

        private async Task<int> GetMaBacSi()
        {
            return await _context.BacSis
                .Where(b => b.MaTaiKhoan == GetMaTaiKhoan())
                .Select(b => b.MaBacSi)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> CoQuyenXem(KetQuaKham ketQua)
        {
            if (User.IsInRole("QuanTriVien") ||
                User.IsInRole("NhanVienTiepNhan"))
                return true;

            if (User.IsInRole("BacSi"))
            {
                int maBacSi = await GetMaBacSi();
                return maBacSi > 0 && ketQua.MaBacSi == maBacSi;
            }

            if (User.IsInRole("BenhNhan"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                return await _context.BenhNhans.AnyAsync(b =>
                    b.MaTaiKhoan == maTaiKhoan &&
                    b.MaBenhNhan == ketQua.MaLichHenNavigation.MaBenhNhan
                );
            }

            return false;
        }

        private async Task LoadDropdowns(
            int? maBacSi = null,
            int? maLichHen = null)
        {
            var bacSiQuery = _context.BacSis.AsQueryable();
            var lichHenQuery = _context.LichHens.AsQueryable();

            if (User.IsInRole("BacSi"))
            {
                int idBacSi = await GetMaBacSi();

                bacSiQuery = bacSiQuery.Where(
                    b => b.MaBacSi == idBacSi
                );

                lichHenQuery = lichHenQuery.Where(
                    l => l.MaBacSi == idBacSi
                );
            }

            ViewData["MaBacSi"] = new SelectList(
                bacSiQuery,
                "MaBacSi",
                "MaBacSi",
                maBacSi
            );

            ViewData["MaLichHen"] = new SelectList(
                lichHenQuery,
                "MaLichHen",
                "MaLichHen",
                maLichHen
            );
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.KetQuaKhams
                .Include(k => k.MaBacSiNavigation)
                .Include(k => k.MaLichHenNavigation)
                .AsQueryable();

            if (User.IsInRole("BacSi"))
            {
                int maBacSi = await GetMaBacSi();

                if (maBacSi == 0)
                    return View(new List<KetQuaKham>());

                query = query.Where(k => k.MaBacSi == maBacSi);
            }
            else if (User.IsInRole("BenhNhan"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                query = query.Where(k =>
                    _context.BenhNhans.Any(b =>
                        b.MaTaiKhoan == maTaiKhoan &&
                        b.MaBenhNhan == k.MaLichHenNavigation.MaBenhNhan
                    )
                );
            }
            else if (!User.IsInRole("QuanTriVien") &&
                     !User.IsInRole("NhanVienTiepNhan"))
            {
                return Forbid();
            }

            return View(await query
                .OrderByDescending(k => k.NgayKham)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ketQua = await _context.KetQuaKhams
                .Include(k => k.MaBacSiNavigation)
                .Include(k => k.MaLichHenNavigation)
                .FirstOrDefaultAsync(k => k.MaKetQua == id);

            if (ketQua == null)
                return NotFound();

            if (!await CoQuyenXem(ketQua))
                return Forbid();

            return View(ketQua);
        }

        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("BacSi") &&
                await GetMaBacSi() == 0)
                return Forbid();

            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "QuanTriVien,BacSi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaLichHen,TrieuChung,KetQua,LoiDan,NgayKham,MaBacSi")]
            KetQuaKham ketQuaKham)
        {
            if (User.IsInRole("BacSi"))
            {
                int maBacSi = await GetMaBacSi();

                if (maBacSi == 0)
                    return Forbid();

                ketQuaKham.MaBacSi = maBacSi;
            }

            var lichHen = await _context.LichHens
                .FirstOrDefaultAsync(l =>
                    l.MaLichHen == ketQuaKham.MaLichHen
                );

            if (lichHen == null ||
                lichHen.MaBacSi != ketQuaKham.MaBacSi)
            {
                ModelState.AddModelError(
                    "MaLichHen",
                    "Lá»‹ch háº¹n khÃ´ng há»£p lá»‡ hoáº·c khÃ´ng thuá»™c bÃ¡c sÄ© nÃ y."
                );
            }

            if (ketQuaKham.NgayKham == default)
                ketQuaKham.NgayKham = DateTime.Now;

            bool daCoKetQua = await _context.KetQuaKhams.AnyAsync(
                k => k.MaLichHen == ketQuaKham.MaLichHen
            );

            if (daCoKetQua)
            {
                ModelState.AddModelError(
                    "MaLichHen",
                    "Lá»‹ch háº¹n nÃ y Ä‘Ã£ cÃ³ káº¿t quáº£ khÃ¡m."
                );
            }

            if (ModelState.IsValid)
            {
                _context.KetQuaKhams.Add(ketQuaKham);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(
                ketQuaKham.MaBacSi,
                ketQuaKham.MaLichHen
            );

            return View(ketQuaKham);
        }

        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var ketQua = await _context.KetQuaKhams
                .FirstOrDefaultAsync(k => k.MaKetQua == id);

            if (ketQua == null)
                return NotFound();

            if (User.IsInRole("BacSi") &&
                ketQua.MaBacSi != await GetMaBacSi())
                return Forbid();

            await LoadDropdowns(
                ketQua.MaBacSi,
                ketQua.MaLichHen
            );

            return View(ketQua);
        }

        [HttpPost]
        [Authorize(Roles = "QuanTriVien,BacSi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaKetQua,MaLichHen,TrieuChung,KetQua,LoiDan,NgayKham,MaBacSi")]
            KetQuaKham input)
        {
            if (id != input.MaKetQua)
                return NotFound();

            var ketQua = await _context.KetQuaKhams
                .FirstOrDefaultAsync(k => k.MaKetQua == id);

            if (ketQua == null)
                return NotFound();

            if (User.IsInRole("BacSi"))
            {
                int maBacSi = await GetMaBacSi();

                if (ketQua.MaBacSi != maBacSi)
                    return Forbid();

                input.MaBacSi = maBacSi;
            }

            var lichHen = await _context.LichHens
                .FirstOrDefaultAsync(l =>
                    l.MaLichHen == input.MaLichHen
                );

            if (lichHen == null ||
                lichHen.MaBacSi != input.MaBacSi)
            {
                ModelState.AddModelError(
                    "MaLichHen",
                    "Lá»‹ch háº¹n khÃ´ng thuá»™c bÃ¡c sÄ© Ä‘Ã£ chá»n."
                );
            }

            bool trungLichHen = await _context.KetQuaKhams.AnyAsync(
                k => k.MaLichHen == input.MaLichHen &&
                     k.MaKetQua != id
            );

            if (trungLichHen)
            {
                ModelState.AddModelError(
                    "MaLichHen",
                    "Lá»‹ch háº¹n nÃ y Ä‘Ã£ cÃ³ káº¿t quáº£ khÃ¡m khÃ¡c."
                );
            }

            if (ModelState.IsValid)
            {
                ketQua.MaLichHen = input.MaLichHen;
                ketQua.MaBacSi = input.MaBacSi;
                ketQua.TrieuChung = input.TrieuChung;
                ketQua.KetQua = input.KetQua;
                ketQua.LoiDan = input.LoiDan;
                ketQua.NgayKham = input.NgayKham;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(
                input.MaBacSi,
                input.MaLichHen
            );

            return View(input);
        }

        [Authorize(Roles = "QuanTriVien,BacSi")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var ketQua = await _context.KetQuaKhams
                .Include(k => k.MaBacSiNavigation)
                .Include(k => k.MaLichHenNavigation)
                .FirstOrDefaultAsync(k => k.MaKetQua == id);

            if (ketQua == null)
                return NotFound();

            if (User.IsInRole("BacSi") &&
                ketQua.MaBacSi != await GetMaBacSi())
                return Forbid();

            return View(ketQua);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "QuanTriVien,BacSi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ketQua = await _context.KetQuaKhams
                .FirstOrDefaultAsync(k => k.MaKetQua == id);

            if (ketQua == null)
                return NotFound();

            if (User.IsInRole("BacSi") &&
                ketQua.MaBacSi != await GetMaBacSi())
                return Forbid();

            _context.KetQuaKhams.Remove(ketQua);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}