using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("LichHen")]
[Index("MaBacSi", "ThoiGianBatDau", "ThoiGianKetThuc", Name = "IX_LichHen_BacSi_ThoiGian")]
[Index("MaBenhNhan", "ThoiGianBatDau", Name = "IX_LichHen_BenhNhan")]
[Index("TrangThai", Name = "IX_LichHen_TrangThai")]
public partial class LichHen
{
    [Key]
    public int MaLichHen { get; set; }

    public int MaBenhNhan { get; set; }

    public int MaBacSi { get; set; }

    public int MaPhongKham { get; set; }

    public DateTime ThoiGianBatDau { get; set; }

    public DateTime ThoiGianKetThuc { get; set; }

    [StringLength(500)]
    public string? LyDoKham { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    public DateTime NgayDat { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("MaLichHenNavigation")]
    public virtual KetQuaKham? KetQuaKham { get; set; }

    [InverseProperty("MaLichHenNavigation")]
    public virtual ICollection<LichSuLichHen> LichSuLichHens { get; set; } = new List<LichSuLichHen>();

    [ForeignKey("MaBacSi")]
    [InverseProperty("LichHens")]
    public virtual BacSi MaBacSiNavigation { get; set; } = null!;

    [ForeignKey("MaBenhNhan")]
    [InverseProperty("LichHens")]
    public virtual BenhNhan MaBenhNhanNavigation { get; set; } = null!;

    [ForeignKey("MaPhongKham")]
    [InverseProperty("LichHens")]
    public virtual PhongKham MaPhongKhamNavigation { get; set; } = null!;
}
