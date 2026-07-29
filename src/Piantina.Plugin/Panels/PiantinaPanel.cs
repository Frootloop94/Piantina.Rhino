using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Views;
using System;
using System.Runtime.InteropServices;
using Piantina.Plugin.Controls; 

namespace Piantina.Plugin.Panels;

[Guid("F4D53D5E-52D6-4E6C-95B8-1F6D6F5E78A3")]
public class PiantinaPanel : Panel
{
    private readonly Panel _contentPanel;

    private void ShowView(Control view)
    {
        _contentPanel.Content = view;
    }

    
    public PiantinaPanel()
    {
        Padding = 10;

        Size = new Size(700, 500);

        _contentPanel = new Panel
        {
            Content = new DashboardView()
        };


        var sidebar = new Sidebar(ShowView);
           

        Content = new Splitter
        {
            Orientation = Orientation.Horizontal,

            Position = 180,

            FixedPanel = SplitterFixedPanel.Panel1,

            Panel1 = sidebar,

            Panel2 = _contentPanel
        };

    }
}