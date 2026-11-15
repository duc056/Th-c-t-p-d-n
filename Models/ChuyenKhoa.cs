using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("ChuyenKhoa")]
[Index("TenChuyenKhoa", Name = "UQ__ChuyenKh__E7F9B9285CBEB9C1", IsUnique = true)]
public partial class ChuyenKhoa
{
    [Key]
    public int MaChuyenKhoa { get; set; }

    [StringLength(150)]
    public string TenChuyenKhoa { get; set; } = null!;

    [StringLength(500)]
    public string? MoTa { get; set; }

    public bool TrangThai { get; set; }

    [InverseProperty("MaChuyenKhoaNavigation")]
    public virtual ICollection<BacSiChuyenKhoa> BacSiChuyenKhoas { get; set; } = new List<BacSiChuyenKhoa>();
}
