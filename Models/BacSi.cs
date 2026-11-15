using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("BacSi")]
[Index("MaPhongKham", Name = "IX_BacSi_PhongKham")]
[Index("MaTaiKhoan", Name = "UQ__BacSi__AD7C6528F201C054", IsUnique = true)]
public partial class BacSi
{
    [Key]
    public int MaBacSi { get; set; }

    public int MaTaiKhoan { get; set; }

    public int MaPhongKham { get; set; }

    [StringLength(100)]
    public string? HocVi { get; set; }

    public int? KinhNghiem { get; set; }

    [StringLength(1000)]
    public string? GioiThieu { get; set; }

    public bool TrangThai { get; set; }

    [InverseProperty("MaBacSiNavigation")]
    public virtual ICollection<BacSiChuyenKhoa> BacSiChuyenKhoas { get; set; } = new List<BacSiChuyenKhoa>();

    [InverseProperty("MaBacSiNavigation")]
    public virtual ICollection<CaLamViec> CaLamViecs { get; set; } = new List<CaLamViec>();

    [InverseProperty("MaBacSiNavigation")]
    public virtual ICollection<KetQuaKham> KetQuaKhams { get; set; } = new List<KetQuaKham>();

    [InverseProperty("MaBacSiNavigation")]
    public virtual ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();

    [ForeignKey("MaPhongKham")]
    [InverseProperty("BacSis")]
    public virtual PhongKham MaPhongKhamNavigation { get; set; } = null!;

    [ForeignKey("MaTaiKhoan")]
    [InverseProperty("BacSi")]
    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;

    [InverseProperty("MaBacSiNavigation")]
    public virtual ICollection<NgayNghi> NgayNghis { get; set; } = new List<NgayNghi>();
}
