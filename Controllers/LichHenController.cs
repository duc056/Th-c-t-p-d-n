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
    public class LichHenController : Controller
    {
        private readonly ClinicBookContext _context;

        public LichHenController(ClinicBookContext context)
        {
            _context = context;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private async Task<bool> CoQuyenXem(LichHen lichHen)
        {
            if (User.IsInRole("QuanTriVien") ||
                User.IsInRole("NhanVienTiepNhan"))
                return true;

            int maTaiKhoan = GetMaTaiKhoan();

            if (User.IsInRole("BenhNhan"))
            {
                return await _context.BenhNhans.AnyAsync(
                    b => b.MaTaiKhoan == maTaiKhoan &&
                         b.MaBenhNhan == lichHen.MaBenhNhan);
            }

            if (User.IsInRole("BacSi"))
            {
                return await _context.BacSis.AnyAsync(
                    b => b.MaTaiKhoan == maTaiKhoan &&
                         b.MaBacSi == lichHen.MaBacSi);
            }

            return false;
        }

        // =========================
        // DANH SACH LICH HEN
        // =========================
        public async Task<IActionResult> Index()
        {
            var query = _context.LichHens
                .Include(l => l.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .Include(l => l.MaBenhNhanNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .Include(l => l.MaPhongKhamNavigation)
                .AsQueryable();

            if (User.IsInRole("BenhNhan"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                var benhNhan = await _context.BenhNhans
                    .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);

                if (benhNhan == null)
                    return View(new List<LichHen>());

                query = query.Where(l => l.MaBenhNhan == benhNhan.MaBenhNhan);
            }
            else if (User.IsInRole("BacSi"))
            {
                int maTaiKhoan = GetMaTaiKhoan();

                var bacSi = await _context.BacSis
                    .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);

                if (bacSi == null)
                    return View(new List<LichHen>());

                query = query.Where(l => l.MaBacSi == bacSi.MaBacSi);
            }
            else if (!User.IsInRole("QuanTriVien") &&
                     !User.IsInRole("NhanVienTiepNhan"))
            {
                return Forbid();
            }

            return View(await query
                .OrderByDescending(l => l.ThoiGianBatDau)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var lichHen = await _context.LichHens
                .Include(l => l.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .Include(l => l.MaBenhNhanNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .Include(l => l.MaPhongKhamNavigation)
                .FirstOrDefaultAsync(l => l.MaLichHen == id);

            if (lichHen == null)
                return NotFound();

            if (!await CoQuyenXem(lichHen))
                return Forbid();

            return View(lichHen);
        }

        // =========================
        // BENH NHAN TU DAT LICH
        // =========================
        [Authorize(Roles = "BenhNhan")]
        public async Task<IActionResult> DatLich()
        {
            await LoadBacSiDatLich();
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "BenhNhan")]
        public async Task<IActionResult> GetKhungGioTrong(
            int maBacSi,
            DateTime ngay)
        {
            if (ngay.Date < DateTime.Today)
                return Json(new List<object>());

            var bacSi = await _context.BacSis
                .FirstOrDefaultAsync(b =>
                    b.MaBacSi == maBacSi &&
                    b.TrangThai);

            if (bacSi == null)
                return Json(new List<object>());

            var khungGio = await LayKhungGioTrong(maBacSi, ngay.Date);

            return Json(khungGio.Select(k => new
            {
                start = k.BatDau.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = k.KetThuc.ToString("yyyy-MM-ddTHH:mm:ss"),
                label = k.BatDau.ToString("HH:mm") + " - " +
                        k.KetThuc.ToString("HH:mm")
            }));
        }

        [HttpPost]
        [Authorize(Roles = "BenhNhan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLich(
            int maBacSi,
            DateTime thoiGianBatDau,
            string? lyDoKham,
            string? ghiChu)
        {
            int maTaiKhoan = GetMaTaiKhoan();

            var benhNhan = await _context.BenhNhans
                .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);

            if (benhNhan == null)
            {
                TempData["ErrorMessage"] =
                    "TÃ i khoáº£n chÆ°a cÃ³ há»“ sÆ¡ bá»‡nh nhÃ¢n. Vui lÃ²ng liÃªn há»‡ nhÃ¢n viÃªn tiáº¿p nháº­n.";
                return RedirectToAction(nameof(DatLich));
            }

            var bacSi = await _context.BacSis
                .FirstOrDefaultAsync(b =>
                    b.MaBacSi == maBacSi &&
                    b.TrangThai);

            if (bacSi == null)
            {
                TempData["ErrorMessage"] = "BÃ¡c sÄ© khÃ´ng tá»“n táº¡i hoáº·c Ä‘ang ngá»«ng hoáº¡t Ä‘á»™ng.";
                return RedirectToAction(nameof(DatLich));
            }

            if (thoiGianBatDau <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Vui lÃ²ng chá»n thá»i gian trong tÆ°Æ¡ng lai.";
                return RedirectToAction(nameof(DatLich));
            }

            // Kiá»ƒm tra láº¡i khung giá» á»Ÿ mÃ¡y chá»§, khÃ´ng tin dá»¯ liá»‡u tá»« trÃ¬nh duyá»‡t.
            var khungGio = await LayKhungGioTrong(
                maBacSi,
                thoiGianBatDau.Date);

            var khungDuocChon = khungGio.FirstOrDefault(k =>
                k.BatDau == thoiGianBatDau);

            if (khungDuocChon == null)
            {
                TempData["ErrorMessage"] =
                    "Khung giá» vá»«a chá»n khÃ´ng cÃ²n trá»‘ng. Vui lÃ²ng chá»n giá» khÃ¡c.";
                return RedirectToAction(nameof(DatLich));
            }

            if (bacSi.MaPhongKham <= 0)
            {
                TempData["ErrorMessage"] = "BÃ¡c sÄ© chÆ°a Ä‘Æ°á»£c phÃ¢n cÃ´ng phÃ²ng khÃ¡m.";
                return RedirectToAction(nameof(DatLich));
            }

            var phongKham = await _context.PhongKhams
                .FirstOrDefaultAsync(p =>
                    p.MaPhongKham == bacSi.MaPhongKham &&
                    p.TrangThai);

            if (phongKham == null)
            {
                TempData["ErrorMessage"] = "PhÃ²ng khÃ¡m cá»§a bÃ¡c sÄ© hiá»‡n khÃ´ng hoáº¡t Ä‘á»™ng.";
                return RedirectToAction(nameof(DatLich));
            }

            var lichHen = new LichHen
            {
                MaBenhNhan = benhNhan.MaBenhNhan,
                MaBacSi = bacSi.MaBacSi,
                MaPhongKham = bacSi.MaPhongKham,
                ThoiGianBatDau = khungDuocChon.BatDau,
                ThoiGianKetThuc = khungDuocChon.KetThuc,
                LyDoKham = lyDoKham?.Trim(),
                GhiChu = ghiChu?.Trim(),
                TrangThai = "ChoXacNhan",
                NgayDat = DateTime.Now,
                NgayCapNhat = null
            };

            _context.LichHens.Add(lichHen);

            _context.LichSuLichHens.Add(new LichSuLichHen
            {
                MaLichHenNavigation = lichHen,
                TrangThaiCu = null,
                TrangThaiMoi = "ChoXacNhan",
                MaTaiKhoan = maTaiKhoan,
                ThoiGianThayDoi = DateTime.Now,
                GhiChu = "Bá»‡nh nhÃ¢n tá»± Ä‘áº·t lá»‹ch khÃ¡m"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Äáº·t lá»‹ch khÃ¡m thÃ nh cÃ´ng. Vui lÃ²ng chá» xÃ¡c nháº­n.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBacSiDatLich()
        {
            var bacSis = await _context.BacSis
                .Include(b => b.MaTaiKhoanNavigation)
                .Where(b => b.TrangThai)
                .OrderBy(b => b.MaTaiKhoanNavigation.HoTen)
                .ToListAsync();

            ViewBag.BacSis = bacSis.Select(b => new SelectListItem
            {
                Value = b.MaBacSi.ToString(),
                Text = "BS. " + b.MaTaiKhoanNavigation.HoTen +
                       (string.IsNullOrWhiteSpace(b.HocVi) ? "" : " - " + b.HocVi)
            }).ToList();
        }

        // =========================
        // SINH KHUNG GIO TRONG
        // =========================
        private class KhungGio
        {
            public DateTime BatDau { get; set; }
            public DateTime KetThuc { get; set; }
        }

        private async Task<List<KhungGio>> LayKhungGioTrong(
            int maBacSi,
            DateTime ngay)
        {
            var ketQua = new List<KhungGio>();

            // .NET: Monday = 1, Tuesday = 2, ..., Sunday = 7.
            int thu = ngay.DayOfWeek == DayOfWeek.Sunday
                ? 7
                : (int)ngay.DayOfWeek;

            var caLamViecs = await _context.CaLamViecs
                .Where(c =>
                    c.MaBacSi == maBacSi &&
                    c.ThuTrongTuan == thu &&
                    c.TrangThai)
                .OrderBy(c => c.GioBatDau)
                .ToListAsync();

            if (!caLamViecs.Any())
                return ketQua;

            DateOnly ngayDateOnly = DateOnly.FromDateTime(ngay);

            var ngayNghis = await _context.NgayNghis
                .Where(n =>
                    n.MaBacSi == maBacSi &&
                    n.NgayNghi1 == ngayDateOnly)
                .ToListAsync();

            // Láº¥y cÃ¡c lá»‹ch Ä‘Ã£ tá»“n táº¡i trong ngÃ y.
            var lichHenTrongNgay = await _context.LichHens
                .Where(l =>
                    l.MaBacSi == maBacSi &&
                    l.ThoiGianBatDau < ngay.Date.AddDays(1) &&
                    l.ThoiGianKetThuc > ngay.Date)
                .ToListAsync();

            // Nhá»¯ng tráº¡ng thÃ¡i há»§y/tá»« chá»‘i khÃ´ng chiáº¿m khung giá».
            var trangThaiKhongChiemCho = new[]
            {
                "DaHuy", "DaTuChoi", "TuChoi", "Huy"
            };

            lichHenTrongNgay = lichHenTrongNgay
                .Where(l => !trangThaiKhongChiemCho.Contains(
                    l.TrangThai,
                    StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var ca in caLamViecs)
            {
                if (ca.ThoiLuongKham <= 0 ||
                    ca.GioKetThuc <= ca.GioBatDau)
                    continue;

                DateTime gioBatDauCa = ngay.Date.Add(ca.GioBatDau.ToTimeSpan());
                DateTime gioKetThucCa = ngay.Date.Add(ca.GioKetThuc.ToTimeSpan());

                DateTime gioHienTai = gioBatDauCa;

                while (gioHienTai.AddMinutes(ca.ThoiLuongKham) <= gioKetThucCa)
                {
                    DateTime gioKetThuc = gioHienTai.AddMinutes(ca.ThoiLuongKham);

                    bool biNghi = ngayNghis.Any(n =>
                    {
                        // KhÃ´ng cÃ³ giá» báº¯t Ä‘áº§u/káº¿t thÃºc: nghá»‰ cáº£ ngÃ y.
                        if (!n.GioBatDau.HasValue || !n.GioKetThuc.HasValue)
                            return true;

                        DateTime nghiBatDau = ngay.Date.Add(
                            n.GioBatDau.Value.ToTimeSpan());

                        DateTime nghiKetThuc = ngay.Date.Add(
                            n.GioKetThuc.Value.ToTimeSpan());

                        return gioHienTai < nghiKetThuc &&
                               gioKetThuc > nghiBatDau;
                    });

                    bool biTrungLich = lichHenTrongNgay.Any(l =>
                        gioHienTai < l.ThoiGianKetThuc &&
                        gioKetThuc > l.ThoiGianBatDau);

                    if (!biNghi &&
                        !biTrungLich &&
                        gioHienTai > DateTime.Now)
                    {
                        ketQua.Add(new KhungGio
                        {
                            BatDau = gioHienTai,
                            KetThuc = gioKetThuc
                        });
                    }

                    gioHienTai = gioKetThuc;
                }
            }

            return ketQua
                .OrderBy(k => k.BatDau)
                .ToList();
        }

        // =========================
        // NHAN VIEN / QUAN TRI TAO LICH
        // =========================
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public IActionResult Create()
        {
            LoadDropdowns();

            return View(new LichHen
            {
                TrangThai = "ChoXacNhan",
                ThoiGianBatDau = DateTime.Now.AddHours(1),
                ThoiGianKetThuc = DateTime.Now.AddHours(2)
            });
        }

        [HttpPost]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaBenhNhan,MaBacSi,MaPhongKham,ThoiGianBatDau,ThoiGianKetThuc,LyDoKham,GhiChu")]
            LichHen lichHen)
        {
            ModelState.Remove(nameof(LichHen.TrangThai));
            ModelState.Remove(nameof(LichHen.NgayDat));
            ModelState.Remove(nameof(LichHen.NgayCapNhat));
            ModelState.Remove(nameof(LichHen.MaBacSiNavigation));
            ModelState.Remove(nameof(LichHen.MaBenhNhanNavigation));
            ModelState.Remove(nameof(LichHen.MaPhongKhamNavigation));

            lichHen.TrangThai = "ChoXacNhan";
            lichHen.NgayDat = DateTime.Now;
            lichHen.NgayCapNhat = null;

            if (lichHen.ThoiGianBatDau <= DateTime.Now)
                ModelState.AddModelError(
                    nameof(LichHen.ThoiGianBatDau),
                    "Thá»i gian báº¯t Ä‘áº§u pháº£i á»Ÿ tÆ°Æ¡ng lai.");

            if (lichHen.ThoiGianKetThuc <= lichHen.ThoiGianBatDau)
                ModelState.AddModelError(
                    nameof(LichHen.ThoiGianKetThuc),
                    "Thá»i gian káº¿t thÃºc pháº£i sau thá»i gian báº¯t Ä‘áº§u.");

            if (ModelState.IsValid)
            {
                _context.LichHens.Add(lichHen);

                _context.LichSuLichHens.Add(new LichSuLichHen
                {
                    MaLichHenNavigation = lichHen,
                    TrangThaiCu = null,
                    TrangThaiMoi = lichHen.TrangThai,
                    MaTaiKhoan = GetMaTaiKhoan(),
                    ThoiGianThayDoi = DateTime.Now,
                    GhiChu = "NhÃ¢n viÃªn/quáº£n trá»‹ viÃªn táº¡o lá»‹ch háº¹n"
                });

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Táº¡o lá»‹ch háº¹n thÃ nh cÃ´ng.";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(lichHen);
            return View(lichHen);
        }

        // =========================
        // SUA LICH
        // =========================
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var lichHen = await _context.LichHens.FindAsync(id);

            if (lichHen == null)
                return NotFound();

            LoadDropdowns(lichHen);
            return View(lichHen);
        }

        [HttpPost]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaLichHen,MaBenhNhan,MaBacSi,MaPhongKham,ThoiGianBatDau,ThoiGianKetThuc,LyDoKham,TrangThai,GhiChu")]
            LichHen lichHen)
        {
            if (id != lichHen.MaLichHen)
                return NotFound();

            ModelState.Remove(nameof(LichHen.MaBacSiNavigation));
            ModelState.Remove(nameof(LichHen.MaBenhNhanNavigation));
            ModelState.Remove(nameof(LichHen.MaPhongKhamNavigation));

            if (lichHen.ThoiGianKetThuc <= lichHen.ThoiGianBatDau)
                ModelState.AddModelError(
                    nameof(LichHen.ThoiGianKetThuc),
                    "Thá»i gian káº¿t thÃºc pháº£i sau thá»i gian báº¯t Ä‘áº§u.");

            var lichHenCu = await _context.LichHens.FindAsync(id);

            if (lichHenCu == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                string trangThaiCu = lichHenCu.TrangThai;
                string trangThaiMoi = lichHen.TrangThai;

                lichHenCu.MaBenhNhan = lichHen.MaBenhNhan;
                lichHenCu.MaBacSi = lichHen.MaBacSi;
                lichHenCu.MaPhongKham = lichHen.MaPhongKham;
                lichHenCu.ThoiGianBatDau = lichHen.ThoiGianBatDau;
                lichHenCu.ThoiGianKetThuc = lichHen.ThoiGianKetThuc;
                lichHenCu.LyDoKham = lichHen.LyDoKham;
                lichHenCu.GhiChu = lichHen.GhiChu;
                lichHenCu.NgayCapNhat = DateTime.Now;

                if (!string.Equals(
                    trangThaiCu,
                    trangThaiMoi,
                    StringComparison.OrdinalIgnoreCase))
                {
                    lichHenCu.TrangThai = trangThaiMoi;

                    _context.LichSuLichHens.Add(new LichSuLichHen
                    {
                        MaLichHen = lichHenCu.MaLichHen,
                        TrangThaiCu = trangThaiCu,
                        TrangThaiMoi = trangThaiMoi,
                        MaTaiKhoan = GetMaTaiKhoan(),
                        ThoiGianThayDoi = DateTime.Now,
                        GhiChu = "Thay Ä‘á»•i tráº¡ng thÃ¡i lá»‹ch háº¹n"
                    });
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cáº­p nháº­t lá»‹ch háº¹n thÃ nh cÃ´ng.";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(lichHen);
            return View(lichHen);
        }

        // =========================
        // XOA LICH
        // =========================
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var lichHen = await _context.LichHens
                .Include(l => l.MaBacSiNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .Include(l => l.MaBenhNhanNavigation)
                    .ThenInclude(b => b.MaTaiKhoanNavigation)
                .Include(l => l.MaPhongKhamNavigation)
                .FirstOrDefaultAsync(l => l.MaLichHen == id);

            if (lichHen == null)
                return NotFound();

            return View(lichHen);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichHen = await _context.LichHens.FindAsync(id);

            if (lichHen == null)
                return NotFound();

            // KhÃ´ng xÃ³a lá»‹ch Ä‘Ã£ cÃ³ lá»‹ch sá»­; chuyá»ƒn tráº¡ng thÃ¡i há»§y Ä‘á»ƒ giá»¯ dáº¥u váº¿t.
            lichHen.TrangThai = "DaHuy";
            lichHen.NgayCapNhat = DateTime.Now;

            _context.LichSuLichHens.Add(new LichSuLichHen
            {
                MaLichHen = lichHen.MaLichHen,
                TrangThaiCu = "TrÆ°á»›c khi há»§y",
                TrangThaiMoi = "DaHuy",
                MaTaiKhoan = GetMaTaiKhoan(),
                ThoiGianThayDoi = DateTime.Now,
                GhiChu = "Há»§y lá»‹ch háº¹n"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "ÄÃ£ há»§y lá»‹ch háº¹n.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(LichHen? lichHen = null)
        {
            var benhNhans = _context.BenhNhans
                .Include(b => b.MaTaiKhoanNavigation)
                .AsNoTracking()
                .AsEnumerable()
                .Select(b => new
                {
                    b.MaBenhNhan,
                    TenHienThi = $"{b.MaTaiKhoanNavigation.HoTen} - {b.MaTaiKhoanNavigation.SoDienThoai ?? "ChÆ°a cÃ³ SÄT"}"
                })
                .OrderBy(b => b.TenHienThi)
                .ToList();

            ViewData["MaBenhNhan"] = new SelectList(
                benhNhans, "MaBenhNhan", "TenHienThi", lichHen?.MaBenhNhan);

            var bacSis = _context.BacSis
                .Include(b => b.MaTaiKhoanNavigation)
                .AsNoTracking()
                .AsEnumerable()
                .Select(b => new
                {
                    b.MaBacSi,
                    TenHienThi = $"BS. {b.MaTaiKhoanNavigation.HoTen} - {b.HocVi ?? "BÃ¡c sÄ©"} - {b.MaTaiKhoanNavigation.SoDienThoai ?? "ChÆ°a cÃ³ SÄT"}"
                })
                .OrderBy(b => b.TenHienThi)
                .ToList();

            ViewData["MaBacSi"] = new SelectList(
                bacSis, "MaBacSi", "TenHienThi", lichHen?.MaBacSi);

            ViewData["MaPhongKham"] = new SelectList(
                _context.PhongKhams.AsNoTracking().OrderBy(p => p.TenPhongKham),
                "MaPhongKham", "TenPhongKham", lichHen?.MaPhongKham);
        }
    }
}
