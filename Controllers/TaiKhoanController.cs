
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers
{
    [Authorize(Roles = "QuanTriVien")]
    public class TaiKhoanController : Controller
    {
        private readonly ClinicBookContext _context;
        private readonly IPasswordHasher<TaiKhoan> _passwordHasher;

        public TaiKhoanController(
            ClinicBookContext context,
            IPasswordHasher<TaiKhoan> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.TaiKhoans
                .Include(t => t.MaVaiTroNavigation)
                .ToListAsync();

            return View(danhSach);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.MaVaiTroNavigation)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            return View(taiKhoan);
        }

        public IActionResult Create()
        {
            LoadRoles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaVaiTro,TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai,AnhDaiDien,TrangThai")]
            TaiKhoan taiKhoan)
        {
            if (string.IsNullOrWhiteSpace(taiKhoan.MatKhauHash))
                ModelState.AddModelError("MatKhauHash", "Vui lòng nhập mật khẩu.");

            bool trungTen = await _context.TaiKhoans
                .AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap);

            if (trungTen)
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");

            bool trungEmail = await _context.TaiKhoans
                .AnyAsync(t => t.Email == taiKhoan.Email);

            if (trungEmail)
                ModelState.AddModelError("Email", "Email đã được sử dụng.");

            if (ModelState.IsValid)
            {
                string matKhauBanDau = taiKhoan.MatKhauHash;

                taiKhoan.MatKhauHash =
                    _passwordHasher.HashPassword(taiKhoan, matKhauBanDau);

                taiKhoan.NgayTao = DateTime.Now;

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadRoles(taiKhoan.MaVaiTro);
            return View(taiKhoan);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);

            if (taiKhoan == null)
                return NotFound();

            LoadRoles(taiKhoan.MaVaiTro);
            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaTaiKhoan,MaVaiTro,TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai,AnhDaiDien,TrangThai,NgayTao")]
            TaiKhoan input)
        {
            if (id != input.MaTaiKhoan)
                return NotFound();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);

            if (taiKhoan == null)
                return NotFound();

            bool trungTen = await _context.TaiKhoans.AnyAsync(t =>
                t.MaTaiKhoan != id &&
                t.TenDangNhap == input.TenDangNhap);

            if (trungTen)
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");

            bool trungEmail = await _context.TaiKhoans.AnyAsync(t =>
                t.MaTaiKhoan != id &&
                t.Email == input.Email);

            if (trungEmail)
                ModelState.AddModelError("Email", "Email đã được sử dụng.");

            if (ModelState.IsValid)
            {
                taiKhoan.MaVaiTro = input.MaVaiTro;
                taiKhoan.TenDangNhap = input.TenDangNhap;
                taiKhoan.Email = input.Email;
                taiKhoan.HoTen = input.HoTen;
                taiKhoan.SoDienThoai = input.SoDienThoai;
                taiKhoan.AnhDaiDien = input.AnhDaiDien;
                taiKhoan.TrangThai = input.TrangThai;

                // Không nhận mật khẩu mới từ form sửa thông tin.
                // Giữ nguyên MatKhauHash hiện tại.
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadRoles(input.MaVaiTro);
            return View(input);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.MaVaiTroNavigation)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null)
                return NotFound();

            if (User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                == id.Value.ToString())
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            return View(taiKhoan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maTaiKhoanDangNhap = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (maTaiKhoanDangNhap == id.ToString())
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);

            if (taiKhoan == null)
                return NotFound();

            _context.TaiKhoans.Remove(taiKhoan);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa tài khoản vì đang được sử dụng ở dữ liệu khác.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadRoles(int? selectedId = null)
        {
            ViewData["MaVaiTro"] = new SelectList(
                _context.VaiTros,
                "MaVaiTro",
                "TenVaiTro",
                selectedId);
        }
    }
}
