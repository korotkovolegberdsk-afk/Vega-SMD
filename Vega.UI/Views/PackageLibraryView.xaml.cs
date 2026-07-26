using System.Windows.Controls;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class PackageLibraryView : UserControl
{
    public PackageLibraryView()
    {
        InitializeComponent();

        DataContext = new PackageLibraryViewModel();
    }
}