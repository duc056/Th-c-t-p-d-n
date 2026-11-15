using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("BenhNhan")]
[Index("MaTaiKhoan", Name = "UQ__BenhNhan__AD7C6528E7A6BB08", IsUnique = true)]
public partial class BenhNhan
{
    [Key]
    public int MaBenhNhan { get; set; }

    public int MaTaiKhoan { get; set; }

    public DateOnly? NgaySinh { get; set; }

    [StringLength(10)]
    public string? GioiTinh { get; set; }

    [StringLength(300)]
    public string? DiaChi { get; set; }

    public DateTime NgayTao { get; set; }

    [InverseProperty("MaBenhNhanNavigation")]
    public virtual ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();

    [ForeignKey("MaTaiKhoan")]
    [InverseProperty("BenhNhan")]
    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;
}
