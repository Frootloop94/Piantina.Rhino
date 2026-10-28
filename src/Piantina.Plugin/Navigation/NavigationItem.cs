using Eto.Forms;

namespace Piantina.Plugin.Navigation;

public class NavigationItem
{
    public string Title { get; }

    public Func<Control> CreateView { get; }

    public NavigationItem(string title, Func<Control> createView)
    {
        Title = title;
        CreateView = createView;
    }
}
