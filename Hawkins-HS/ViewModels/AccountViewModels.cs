using System.ComponentModel.DataAnnotations;

namespace Hawkins_HS.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Beni Hatırla")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad Soyad gereklidir")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir")]
    [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
