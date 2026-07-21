using Eto.Drawing;
using Eto.Forms;
using Piantina.Plugin.Views;
using System;
using System.Runtime.InteropServices;

namespace Piantina.Plugin.Panels;

[Guid("F4D53D5E-52D6-4E6C-95B8-1F6D6F5E78A3")]
public class PiantinaPanel : Panel
{
    private readonly Panel _contentPanel;

    public PiantinaPanel()
    {
        Padding = 10;

        _contentPanel = new Panel
        {
            Content = new DashboardView()
        };

        var dashboardButton = new Button { Text = "Dashboard" };
        dashboardButton.Click += (_, _) => _contentPanel.Content = new DashboardView();

        var materialsButton = new Button { Text = "Materials" };
        var gemstonesButton = new Button { Text = "Gemstones" };
        var manufacturingButton = new Button { Text = "Manufacturing" };
        var calculatorButton = new Button { Text = "Calculator" };
        var settingsButton = new Button { Text = "Settings" };

        Content = new TableLayout
        {
            Spacing = new Size(10, 0),

            Rows =
            {
                new TableRow(
                    new StackLayout
                    {
                        Width = 140,
                        Spacing = 6,
                        Items =
                        {
                            dashboardButton,
                            materialsButton,
                            gemstonesButton,
                            manufacturingButton,
                            calculatorButton,
                            settingsButton
                        }
                    },
                    _contentPanel
                )
            }
        };
    }
}