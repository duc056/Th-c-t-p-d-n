
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize(Roles = "QuanTriVien")]
    public class VaiTroController : Controller
    {
        private readonly ClinicBookContext _context;

        public VaiTroController(ClinicBookContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.VaiTros.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var vaiTro = await _context.VaiTros
                .FirstOrDefaultAsync(v => v.MaVaiTro == id);

            if (vaiTro == null)
                return NotFound();

            return View(vaiTro);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenVaiTro,MoTa")] VaiTro vaiTro)
        {
            bool daTonTai = await _context.VaiTros
                .AnyAsync(v => v.TenVaiTro == vaiTro.TenVaiTro);

            if (daTonTai)
                ModelState.AddModelError("TenVaiTro", "Tên vai trò đã tồn tại.");

            if (ModelState.IsValid)
            {
                _context.VaiTros.Add(vaiTro);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(vaiTro);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var vaiTro = await _context.VaiTros.FindAsync(id);

            if (vaiTro == null)
                return NotFound();

            return View(vaiTro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaVaiTro,TenVaiTro,MoTa")] VaiTro input)
        {
            if (id != input.MaVaiTro)
                return NotFound();

            var vaiTro = await _context.VaiTros.FindAsync(id);

            if (vaiTro == null)
                return NotFound();

            bool trungTen = await _context.VaiTros.AnyAsync(v =>
                v.MaVaiTro != id &&
                v.TenVaiTro == input.TenVaiTro);

            if (trungTen)
                ModelState.AddModelError("TenVaiTro", "Tên vai trò đã tồn tại.");

            if (ModelState.IsValid)
            {
                vaiTro.TenVaiTro = input.TenVaiTro;
                vaiTro.MoTa = input.MoTa;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(input);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var vaiTro = await _context.VaiTros
                .FirstOrDefaultAsync(v => v.MaVaiTro == id);

            if (vaiTro == null)
                return NotFound();

            return View(vaiTro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vaiTro = await _context.VaiTros.FindAsync(id);

            if (vaiTro == null)
                return NotFound();

            bool dangDuocSuDung = await _context.TaiKhoans
                .AnyAsync(t => t.MaVaiTro == id);

            if (dangDuocSuDung)
            {
                TempData["Error"] =
                    "Không thể xóa vai trò vì đang có tài khoản sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            _context.VaiTros.Remove(vaiTro);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
