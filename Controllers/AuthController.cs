using System.Security.Claims;
using ClinicBook.Data;
using ClinicBook.Models;
using ClinicBook.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers;

public class AuthController : Controller
{
    private readonly ClinicBookContext _context;
    private readonly IPasswordHasher<TaiKhoan> _passwordHasher;

    public AuthController(
        ClinicBookContext context,
        IPasswordHasher<TaiKhoan> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var taiKhoan = await _context.TaiKhoans
            .Include(t => t.MaVaiTroNavigation)
            .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap);

        if (taiKhoan == null || !taiKhoan.TrangThai)
        {
            ModelState.AddModelError("", "Tai khoan khong ton tai hoac da bi khoa.");
            return View(model);
        }

        var result = _passwordHasher.VerifyHashedPassword(
            taiKhoan,
            taiKhoan.MatKhauHash,
            model.MatKhau
        );

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Ten dang nhap hoac mat khau khong dung.");
            return View(model);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            taiKhoan.MatKhauHash =
                _passwordHasher.HashPassword(taiKhoan, model.MatKhau);

            await _context.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTaiKhoan.ToString()),
            new Claim(ClaimTypes.Name, taiKhoan.HoTen),
            new Claim(ClaimTypes.Role, taiKhoan.MaVaiTroNavigation.TenVaiTro),
            new Claim("TenDangNhap", taiKhoan.TenDangNhap),
            new Claim("MaVaiTro", taiKhoan.MaVaiTro.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.GhiNhoDangNhap,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            }
        );

        if (!string.IsNullOrEmpty(model.ReturnUrl) &&
            Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToAction("Login", "Auth");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var tenDangNhap = model.TenDangNhap.Trim();
        var email = model.Email.Trim();

        if (await _context.TaiKhoans.AnyAsync(x => x.TenDangNhap == tenDangNhap))
            ModelState.AddModelError(nameof(model.TenDangNhap), "Ten dang nhap da duoc su dung.");

        if (await _context.TaiKhoans.AnyAsync(x => x.Email == email))
            ModelState.AddModelError(nameof(model.Email), "Email da duoc su dung.");

        if (!ModelState.IsValid)
            return View(model);

        var vaiTroBenhNhan = await _context.VaiTros
            .SingleOrDefaultAsync(x => x.TenVaiTro == "BenhNhan");

        if (vaiTroBenhNhan == null)
        {
            ModelState.AddModelError(string.Empty, "He thong chua cau hinh vai tro BenhNhan.");
            return View(model);
        }

        var taiKhoan = new TaiKhoan
        {
            MaVaiTro = vaiTroBenhNhan.MaVaiTro,
            TenDangNhap = tenDangNhap,
            Email = email,
            MatKhauHash = string.Empty,
            HoTen = model.HoTen.Trim(),
            SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                ? null : model.SoDienThoai.Trim(),
            TrangThai = true,
            NgayTao = DateTime.Now
        };

        taiKhoan.MatKhauHash = _passwordHasher.HashPassword(taiKhoan, model.MatKhau);

        var benhNhan = new BenhNhan
        {
            MaTaiKhoanNavigation = taiKhoan,
            NgaySinh = model.NgaySinh,
            GioiTinh = model.GioiTinh,
            DiaChi = model.DiaChi,
            NgayTao = DateTime.Now
        };

        _context.TaiKhoans.Add(taiKhoan);
        _context.BenhNhans.Add(benhNhan);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty,
                "Khong the tao tai khoan. Ten dang nhap hoac email co the da ton tai.");
            return View(model);
        }

        TempData["RegisterSuccess"] = "Dang ky tai khoan thanh cong. Vui long dang nhap.";
        return RedirectToAction(nameof(Login));
    }
}

