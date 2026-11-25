using System.Security.Claims;
using ClinicBook.Data;
using ClinicBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ClinicBook.Controllers.Api;

[ApiController]
[Route("api/lich-hen")]
[Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
public class LichHenApiController : ControllerBase
{
    private readonly ClinicBookContext _context;

    public LichHenApiController(ClinicBookContext context)
    {
        _context = context;
    }

    // GET: api/lich-hen
    [HttpGet]
    public async Task<IActionResult> GetDanhSach(
        DateTime? from,
        DateTime? to,
        string? trangThai,
        int? maBacSi,
        string? tuKhoa,
        int page = 1,
        int pageSize = 20)
    {
        if (page < 1)
            return BadRequest(new { message = "Trang pháº£i lá»›n hÆ¡n hoáº·c báº±ng 1." });

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "pageSize pháº£i tá»« 1 Ä‘áº¿n 100." });

        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            return BadRequest(new { message = "NgÃ y báº¯t Ä‘áº§u khÃ´ng Ä‘Æ°á»£c sau ngÃ y káº¿t thÃºc." });

        var query = _context.LichHens
            .AsNoTracking()
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(l => l.ThoiGianBatDau >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(l => l.ThoiGianBatDau < to.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(l => l.TrangThai == trangThai.Trim());

        if (maBacSi.HasValue)
            query = query.Where(l => l.MaBacSi == maBacSi.Value);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();

            query = query.Where(l =>
                l.MaBenhNhanNavigation.MaTaiKhoanNavigation.HoTen.Contains(keyword) ||
                l.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen.Contains(keyword) ||
                (l.LyDoKham != null && l.LyDoKham.Contains(keyword)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.ThoiGianBatDau)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.MaLichHen,
                l.MaBenhNhan,
                TenBenhNhan = l.MaBenhNhanNavigation.MaTaiKhoanNavigation.HoTen,
                SoDienThoaiBenhNhan = l.MaBenhNhanNavigation.MaTaiKhoanNavigation.SoDienThoai,
                l.MaBacSi,
                TenBacSi = l.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen,
                l.MaPhongKham,
                TenPhongKham = l.MaPhongKhamNavigation.TenPhongKham,
                l.ThoiGianBatDau,
                l.ThoiGianKetThuc,
                l.LyDoKham,
                l.TrangThai,
                l.GhiChu,
                l.NgayDat,
                l.NgayCapNhat
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        });
    }

    // GET: api/lich-hen/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetChiTiet(int id)
    {
        var item = await _context.LichHens
            .AsNoTracking()
            .Where(l => l.MaLichHen == id)
            .Select(l => new
            {
                l.MaLichHen,
                l.MaBenhNhan,
                TenBenhNhan = l.MaBenhNhanNavigation.MaTaiKhoanNavigation.HoTen,
                EmailBenhNhan = l.MaBenhNhanNavigation.MaTaiKhoanNavigation.Email,
                SoDienThoaiBenhNhan = l.MaBenhNhanNavigation.MaTaiKhoanNavigation.SoDienThoai,
                l.MaBacSi,
                TenBacSi = l.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen,
                HocViBacSi = l.MaBacSiNavigation.HocVi,
                l.MaPhongKham,
                TenPhongKham = l.MaPhongKhamNavigation.TenPhongKham,
                l.ThoiGianBatDau,
                l.ThoiGianKetThuc,
                l.LyDoKham,
                l.TrangThai,
                l.GhiChu,
                l.NgayDat,
                l.NgayCapNhat
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y lá»‹ch háº¹n." });

        return Ok(item);
    }

    // PUT: api/lich-hen/5/trang-thai
    [HttpPut("{id:int}/trang-thai")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatTrangThai(
        int id,
        [FromBody] CapNhatTrangThaiRequest request)
    {
        var lichHen = await _context.LichHens
            .FirstOrDefaultAsync(l => l.MaLichHen == id);

        if (lichHen == null)
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y lá»‹ch háº¹n." });

        var trangThaiMoi = request.TrangThai.Trim();
        var trangThaiCu = lichHen.TrangThai;

        if (string.Equals(
            trangThaiCu,
            trangThaiMoi,
            StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new
            {
                message = "Lá»‹ch háº¹n Ä‘Ã£ á»Ÿ tráº¡ng thÃ¡i nÃ y.",
                maLichHen = id,
                trangThai = trangThaiCu
            });
        }

        var maTaiKhoanText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(maTaiKhoanText, out int maTaiKhoan))
            return Unauthorized(new { message = "KhÃ´ng xÃ¡c Ä‘á»‹nh Ä‘Æ°á»£c tÃ i khoáº£n." });

        lichHen.TrangThai = trangThaiMoi;
        lichHen.NgayCapNhat = DateTime.Now;

        _context.LichSuLichHens.Add(new LichSuLichHen
        {
            MaLichHen = lichHen.MaLichHen,
            TrangThaiCu = trangThaiCu,
            TrangThaiMoi = trangThaiMoi,
            MaTaiKhoan = maTaiKhoan,
            ThoiGianThayDoi = DateTime.Now,
            GhiChu = string.IsNullOrWhiteSpace(request.GhiChu)
                ? "Cáº­p nháº­t tráº¡ng thÃ¡i qua API"
                : request.GhiChu.Trim()
        });

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cáº­p nháº­t tráº¡ng thÃ¡i thÃ nh cÃ´ng.",
            maLichHen = id,
            trangThaiCu,
            trangThaiMoi
        });
    }
}

public class CapNhatTrangThaiRequest
{
    [Required(ErrorMessage = "Vui lÃ²ng nháº­p tráº¡ng thÃ¡i.")]
    [StringLength(30)]
    public string TrangThai { get; set; } = string.Empty;

    [StringLength(500)]
    public string? GhiChu { get; set; }
}