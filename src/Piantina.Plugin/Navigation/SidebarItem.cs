using Eto.Forms;

namespace Piantina.Plugin.Navigation;

public class SidebarItem
{
    public string Title { get; }

    public Func<Control> CreateView { get; }

    public SidebarItem(string title, Func<Control> createView)
    {
        Title = title;
        CreateView = createView;
    }
}