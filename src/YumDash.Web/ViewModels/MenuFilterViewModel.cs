using YumDash.Web.Models;

namespace YumDash.Web.ViewModels;

public class MenuFilterViewModel
{
    public IReadOnlyList<MenuCategory> Categories { get; init; } = Enum.GetValues<MenuCategory>();
}
