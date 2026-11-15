using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("PhongKham")]
public partial class PhongKham
{
    [Key]
    public int MaPhongKham { get; set; }

    [StringLength(200)]
    public string TenPhongKham { get; set; } = null!;

    [StringLength(300)]
    public string DiaChi { get; set; } = null!;

    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }

    public bool TrangThai { get; set; }

    [InverseProperty("MaPhongKhamNavigation")]
    public virtual ICollection<BacSi> BacSis { get; set; } = new List<BacSi>();

    [InverseProperty("MaPhongKhamNavigation")]
    public virtual ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();
}
