using System.Windows.Controls;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class ComponentLibraryView : UserControl
{
    public ComponentLibraryView()
    {
        InitializeComponent();

        DataContext =
            new ComponentLibraryViewModel();
    }
}