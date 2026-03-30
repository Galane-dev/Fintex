using System.ComponentModel.DataAnnotations;

namespace Fintex.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}