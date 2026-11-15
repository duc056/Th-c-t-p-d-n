using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("NgayNghi")]
[Index("MaBacSi", "NgayNghi1", Name = "IX_NgayNghi_BacSi_Ngay")]
public partial class NgayNghi
{
    [Key]
    public int MaNgayNghi { get; set; }

    public int MaBacSi { get; set; }

    [Column("NgayNghi")]
    public DateOnly NgayNghi1 { get; set; }

    public TimeOnly? GioBatDau { get; set; }

    public TimeOnly? GioKetThuc { get; set; }

    [StringLength(300)]
    public string? LyDo { get; set; }

    public DateTime NgayTao { get; set; }

    [ForeignKey("MaBacSi")]
    [InverseProperty("NgayNghis")]
    public virtual BacSi MaBacSiNavigation { get; set; } = null!;
}
