using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("CaLamViec")]
[Index("MaBacSi", "ThuTrongTuan", "TrangThai", Name = "IX_CaLamViec_BacSi_Thu")]
public partial class CaLamViec
{
    [Key]
    public int MaCaLamViec { get; set; }

    public int MaBacSi { get; set; }

    public byte ThuTrongTuan { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public int ThoiLuongKham { get; set; }

    public bool TrangThai { get; set; }

    [ForeignKey("MaBacSi")]
    [InverseProperty("CaLamViecs")]
    public virtual BacSi MaBacSiNavigation { get; set; } = null!;
}
