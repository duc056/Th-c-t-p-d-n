using ClinicBook.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Controllers.Api;

[ApiController]
[Route("api/thong-ke")]
[Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
public class ThongKeApiController : ControllerBase
{
    private readonly ClinicBookContext _context;

    public ThongKeApiController(ClinicBookContext context)
    {
        _context = context;
    }

    // GET: api/thong-ke/lich-hen?from=2026-11-01&to=2026-11-30
    [HttpGet("lich-hen")]
    public async Task<IActionResult> ThongKeLichHen(
        DateTime? from,
        DateTime? to)
    {
        var ngayBatDau = (from ??
            new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;

        var ngayKetThuc = (to ?? DateTime.Today).Date;

        if (ngayBatDau > ngayKetThuc)
        {
            return BadRequest(new
            {
                message = "NgÃ y báº¯t Ä‘áº§u khÃ´ng Ä‘Æ°á»£c sau ngÃ y káº¿t thÃºc."
            });
        }

        var ketThucExclusive = ngayKetThuc.AddDays(1);

        var query = _context.LichHens
            .AsNoTracking()
            .Where(l =>
                l.ThoiGianBatDau >= ngayBatDau &&
                l.ThoiGianBatDau < ketThucExclusive);

        var tongLichHen = await query.CountAsync();

        var theoTrangThai = await query
            .GroupBy(l => l.TrangThai)
            .Select(g => new
            {
                trangThai = g.Key,
                soLuong = g.Count()
            })
            .OrderByDescending(x => x.soLuong)
            .ToListAsync();

        var theoNgay = await query
            .GroupBy(l => l.ThoiGianBatDau.Date)
            .Select(g => new
            {
                ngay = g.Key,
                soLuong = g.Count()
            })
            .OrderBy(x => x.ngay)
            .ToListAsync();

        var theoBacSi = await query
            .GroupBy(l => new
            {
                l.MaBacSi,
                TenBacSi = l.MaBacSiNavigation.MaTaiKhoanNavigation.HoTen
            })
            .Select(g => new
            {
                maBacSi = g.Key.MaBacSi,
                tenBacSi = g.Key.TenBacSi,
                soLuong = g.Count()
            })
            .OrderByDescending(x => x.soLuong)
            .ToListAsync();

        return Ok(new
        {
            tuNgay = ngayBatDau,
            denNgay = ngayKetThuc,
            tongLichHen,
            theoTrangThai,
            theoNgay,
            theoBacSi
        });
    }
}