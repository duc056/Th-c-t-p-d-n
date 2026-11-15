using System.ComponentModel.DataAnnotations;

namespace ClinicBook.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui long nhap ten dang nhap")]
    [Display(Name = "Ten dang nhap")]
    public string TenDangNhap { get; set; } = "";

    [Required(ErrorMessage = "Vui long nhap mat khau")]
    [DataType(DataType.Password)]
    [Display(Name = "Mat khau")]
    public string MatKhau { get; set; } = "";

    [Display(Name = "Ghi nho dang nhap")]
    public bool GhiNhoDangNhap { get; set; }

    public string? ReturnUrl { get; set; }
}
