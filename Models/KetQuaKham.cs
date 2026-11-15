using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Models;

[Table("KetQuaKham")]
[Index("MaLichHen", Name = "UQ__KetQuaKh__150F264E8E924875", IsUnique = true)]
public partial class KetQuaKham
{
    [Key]
    public int MaKetQua { get; set; }

    public int MaLichHen { get; set; }

    [StringLength(1000)]
    public string? TrieuChung { get; set; }

    [StringLength(2000)]
    public string? KetQua { get; set; }

    [StringLength(1000)]
    public string? LoiDan { get; set; }

    public DateTime NgayKham { get; set; }

    public int MaBacSi { get; set; }

    [ForeignKey("MaBacSi")]
    [InverseProperty("KetQuaKhams")]
    public virtual BacSi MaBacSiNavigation { get; set; } = null!;

    [ForeignKey("MaLichHen")]
    [InverseProperty("KetQuaKham")]
    public virtual LichHen MaLichHenNavigation { get; set; } = null!;
}
