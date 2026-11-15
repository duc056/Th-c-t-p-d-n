using System.Security.Claims;
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ClinicBookContext _context;

    public DashboardController(ClinicBookContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (User.IsInRole("QuanTriVien"))
            return RedirectToAction(nameof(Admin));

        if (User.IsInRole("NhanVienTiepNhan"))
            return RedirectToAction(nameof(NhanVien));

        if (User.IsInRole("BacSi"))
            return RedirectToAction(nameof(BacSi));

        if (User.IsInRole("BenhNhan"))
            return RedirectToAction(nameof(BenhNhan));

        return Forbid();
    }

    [Authorize(Roles = "QuanTriVien")]
    public async Task<IActionResult> Admin()
    {
        ViewBag.TongTaiKhoan = await _context.TaiKhoans.CountAsync();
        ViewBag.TongBenhNhan = await _context.BenhNhans.CountAsync();
        ViewBag.TongBacSi = await _context.BacSis.CountAsync();
        ViewBag.TongChuyenKhoa = await _context.ChuyenKhoas.CountAsync();
        ViewBag.TongPhongKham = await _context.PhongKhams.CountAsync();
        ViewBag.TongLichHen = await _context.LichHens.CountAsync();
        ViewBag.TongCaLamViec = await _context.CaLamViecs.CountAsync();
        ViewBag.TongNgayNghi = await _context.NgayNghis.CountAsync();

        return View();
    }

    [Authorize(Roles = "NhanVienTiepNhan")]
    public async Task<IActionResult> NhanVien()
    {
        var homNay = DateTime.Today;
        var ngayMai = homNay.AddDays(1);

        ViewBag.TongBenhNhan = await _context.BenhNhans.CountAsync();
        ViewBag.TongBacSi = await _context.BacSis.CountAsync();
        ViewBag.TongLichHen = await _context.LichHens.CountAsync();

        ViewBag.LichHenHomNay = await _context.LichHens
            .CountAsync(x => x.ThoiGianBatDau >= homNay &&
                             x.ThoiGianBatDau < ngayMai);

        ViewBag.LichHenSapToi = await _context.LichHens
            .Where(x => x.ThoiGianBatDau >= DateTime.Now)
            .Include(x => x.MaBenhNhanNavigation)
                .ThenInclude(x => x.MaTaiKhoanNavigation)
            .Include(x => x.MaBacSiNavigation)
                .ThenInclude(x => x.MaTaiKhoanNavigation)
            .OrderBy(x => x.ThoiGianBatDau)
            .Take(10)
            .ToListAsync();

        return View();
    }

    [Authorize(Roles = "BacSi")]
    public async Task<IActionResult> BacSi()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var maTaiKhoan))
            return Forbid();

        var hoSoBacSi = await _context.BacSis
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);

        if (hoSoBacSi == null)
        {
            ViewBag.ChuaCoHoSo = true;
            ViewBag.TongLichHen = 0;
            ViewBag.LichHenHomNay = 0;
            ViewBag.LichHenSapToi = new List<LichHen>();
            return View();
        }

        var homNay = DateTime.Today;
        var ngayMai = homNay.AddDays(1);

        var lichHenCuaToi = _context.LichHens
            .Where(x => x.MaBacSi == hoSoBacSi.MaBacSi);

        ViewBag.ChuaCoHoSo = false;
        ViewBag.TongLichHen = await lichHenCuaToi.CountAsync();

        ViewBag.LichHenHomNay = await lichHenCuaToi
            .CountAsync(x => x.ThoiGianBatDau >= homNay &&
                             x.ThoiGianBatDau < ngayMai);

        ViewBag.LichHenSapToi = await lichHenCuaToi
            .Where(x => x.ThoiGianBatDau >= DateTime.Now)
            .Include(x => x.MaBenhNhanNavigation)
                .ThenInclude(x => x.MaTaiKhoanNavigation)
            .OrderBy(x => x.ThoiGianBatDau)
            .Take(10)
            .ToListAsync();

        return View();
    }

    [Authorize(Roles = "BenhNhan")]
    public async Task<IActionResult> BenhNhan()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var maTaiKhoan))
            return Forbid();

        var hoSoBenhNhan = await _context.BenhNhans
            .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);

        if (hoSoBenhNhan == null)
        {
            ViewBag.ChuaCoHoSo = true;
            ViewBag.TongLichHen = 0;
            ViewBag.LichHenSapToi = new List<LichHen>();
            return View();
        }

        var lichHenCuaToi = _context.LichHens
            .Where(x => x.MaBenhNhan == hoSoBenhNhan.MaBenhNhan);

        ViewBag.ChuaCoHoSo = false;
        ViewBag.TongLichHen = await lichHenCuaToi.CountAsync();

        ViewBag.LichHenSapToi = await lichHenCuaToi
            .Where(x => x.ThoiGianBatDau >= DateTime.Now)
            .Include(x => x.MaBacSiNavigation)
                .ThenInclude(x => x.MaTaiKhoanNavigation)
            .OrderBy(x => x.ThoiGianBatDau)
            .Take(10)
            .ToListAsync();

        return View();
    }
}
