using System.ComponentModel.DataAnnotations;

namespace ClinicBook.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui long nhap ten dang nhap.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Ten dang nhap phai tu 4 den 100 ky tu.")]
    [Display(Name = "Ten dang nhap")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long nhap email.")]
    [EmailAddress(ErrorMessage = "Email khong dung dinh dang.")]
    [StringLength(150)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long nhap mat khau.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mat khau phai co it nhat 8 ky tu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mat khau")]
    public string MatKhau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long xac nhan mat khau.")]
    [DataType(DataType.Password)]
    [Compare(nameof(MatKhau), ErrorMessage = "Mat khau xac nhan khong khop.")]
    [Display(Name = "Xac nhan mat khau")]
    public string XacNhanMatKhau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long nhap ho ten.")]
    [StringLength(150)]
    [Display(Name = "Ho va ten")]
    public string HoTen { get; set; } = string.Empty;

    [Phone(ErrorMessage = "So dien thoai khong hop le.")]
    [StringLength(20)]
    [Display(Name = "So dien thoai")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Ngay sinh")]
    public DateOnly? NgaySinh { get; set; }

    [StringLength(10)]
    [Display(Name = "Gioi tinh")]
    public string? GioiTinh { get; set; }

    [StringLength(300)]
    [Display(Name = "Dia chi")]
    public string? DiaChi { get; set; }
}
