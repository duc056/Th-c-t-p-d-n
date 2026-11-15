using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[PrimaryKey("MaBacSi", "MaChuyenKhoa")]
[Table("BacSiChuyenKhoa")]
[Index("MaChuyenKhoa", Name = "IX_BSCK_ChuyenKhoa")]
public partial class BacSiChuyenKhoa
{
    [Key]
    public int MaBacSi { get; set; }

    [Key]
    public int MaChuyenKhoa { get; set; }

    public bool LaChuyenKhoaChinh { get; set; }

    [ForeignKey("MaBacSi")]
    [InverseProperty("BacSiChuyenKhoas")]
    public virtual BacSi MaBacSiNavigation { get; set; } = null!;

    [ForeignKey("MaChuyenKhoa")]
    [InverseProperty("BacSiChuyenKhoas")]
    public virtual ChuyenKhoa MaChuyenKhoaNavigation { get; set; } = null!;
}
