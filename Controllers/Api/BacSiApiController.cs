using ClinicBook.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers.Api;

[ApiController]
[Route("api/bac-si")]
[Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
public class BacSiApiController : ControllerBase
{
    private readonly ClinicBookContext _context;

    public BacSiApiController(ClinicBookContext context)
    {
        _context = context;
    }

    // GET: api/bac-si
    [HttpGet]
    public async Task<IActionResult> GetDanhSach(string? tuKhoa)
    {
        var query = _context.BacSis
            .AsNoTracking()
            .Where(b => b.TrangThai);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();

            query = query.Where(b =>
                b.MaTaiKhoanNavigation.HoTen.Contains(keyword) ||
                (b.HocVi != null && b.HocVi.Contains(keyword)));
        }

        var items = await query
            .OrderBy(b => b.MaTaiKhoanNavigation.HoTen)
            .Select(b => new
            {
                b.MaBacSi,
                b.MaTaiKhoan,
                TenBacSi = b.MaTaiKhoanNavigation.HoTen,
                Email = b.MaTaiKhoanNavigation.Email,
                SoDienThoai = b.MaTaiKhoanNavigation.SoDienThoai,
                b.HocVi,
                b.KinhNghiem,
                b.GioiThieu,
                b.MaPhongKham,
                TenPhongKham = b.MaPhongKhamNavigation.TenPhongKham,
                b.TrangThai
            })
            .ToListAsync();

        return Ok(new
        {
            total = items.Count,
            items
        });
    }

    // GET: api/bac-si/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetChiTiet(int id)
    {
        var item = await _context.BacSis
            .AsNoTracking()
            .Where(b => b.MaBacSi == id)
            .Select(b => new
            {
                b.MaBacSi,
                b.MaTaiKhoan,
                TenBacSi = b.MaTaiKhoanNavigation.HoTen,
                Email = b.MaTaiKhoanNavigation.Email,
                SoDienThoai = b.MaTaiKhoanNavigation.SoDienThoai,
                b.HocVi,
                b.KinhNghiem,
                b.GioiThieu,
                b.MaPhongKham,
                TenPhongKham = b.MaPhongKhamNavigation.TenPhongKham,
                b.TrangThai
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y bÃ¡c sÄ©." });

        return Ok(item);
    }
}