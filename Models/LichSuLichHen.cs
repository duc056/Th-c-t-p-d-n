using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("LichSuLichHen")]
[Index("MaLichHen", "ThoiGianThayDoi", Name = "IX_LichSuLichHen_MaLichHen")]
public partial class LichSuLichHen
{
    [Key]
    public int MaLichSu { get; set; }

    public int MaLichHen { get; set; }

    [StringLength(30)]
    public string? TrangThaiCu { get; set; }

    [StringLength(30)]
    public string TrangThaiMoi { get; set; } = null!;

    public int? MaTaiKhoan { get; set; }

    public DateTime ThoiGianThayDoi { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaLichHen")]
    [InverseProperty("LichSuLichHens")]
    public virtual LichHen MaLichHenNavigation { get; set; } = null!;

    [ForeignKey("MaTaiKhoan")]
    [InverseProperty("LichSuLichHens")]
    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }
}
