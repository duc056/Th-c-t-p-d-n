using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("TaiKhoan")]
[Index("TenDangNhap", Name = "UQ__TaiKhoan__55F68FC0802C7E08", IsUnique = true)]
[Index("Email", Name = "UQ__TaiKhoan__A9D10534341FAD3A", IsUnique = true)]
public partial class TaiKhoan
{
    [Key]
    public int MaTaiKhoan { get; set; }

    public int MaVaiTro { get; set; }

    [Required(ErrorMessage = "Vui lÃ²ng nháº­p tÃªn Ä‘Äƒng nháº­p.")]
    [StringLength(100)]
    public string TenDangNhap { get; set; } = null!;

    [Required(ErrorMessage = "Vui lÃ²ng nháº­p email.")]
    [EmailAddress(ErrorMessage = "Email khÃ´ng Ä‘Ãºng Ä‘á»‹nh dáº¡ng.")]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    public string MatKhauHash { get; set; } = null!;

    [Required(ErrorMessage = "Vui lÃ²ng nháº­p há» tÃªn.")]
    [StringLength(150)]
    public string HoTen { get; set; } = null!;

    [Phone(ErrorMessage = "Sá»‘ Ä‘iá»‡n thoáº¡i khÃ´ng há»£p lá»‡.")]
    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [StringLength(255)]
    public string? AnhDaiDien { get; set; }

    public bool TrangThai { get; set; }

    public DateTime NgayTao { get; set; }

    [InverseProperty("MaTaiKhoanNavigation")]
    public virtual BacSi? BacSi { get; set; }

    [InverseProperty("MaTaiKhoanNavigation")]
    public virtual BenhNhan? BenhNhan { get; set; }

    [InverseProperty("MaTaiKhoanNavigation")]
    public virtual ICollection<LichSuLichHen> LichSuLichHens { get; set; } = new List<LichSuLichHen>();

    [ForeignKey("MaVaiTro")]
    [InverseProperty("TaiKhoans")]
    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;
}
