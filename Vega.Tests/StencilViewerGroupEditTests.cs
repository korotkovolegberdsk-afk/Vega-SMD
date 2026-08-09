using Vega.Gerber.Models;
using Vega.StencilUI.ViewModels;
using Vega.StencilViewer.Models;
using Xunit;

namespace Vega.Tests;

public class StencilViewerGroupEditTests
{
    [Fact]
    public void GroupSelection_Should_Toggle_And_Keep_Three_Decimal_Edit_Values()
    {
        var first = new PastePrimitive { ApertureId = 10, X = 1, Y = 2, Width = 0.6, Height = 0.3 };
        var second = new PastePrimitive { ApertureId = 11, X = 3, Y = 4, Width = 0.8, Height = 0.4 };
        var viewModel = new StencilViewerWindowViewModel(new StencilViewDocument());

        viewModel.SelectSingle(first);
        viewModel.TogglePrimitive(second);
        viewModel.SelectedWidth = 1.234;
        viewModel.SelectedHeight = 0.567;
        viewModel.ApplyEditCommand.Execute(null);

        Assert.Equal(2, viewModel.SelectedCount);
        Assert.Equal(1.234, viewModel.SelectedWidth, 3);
        Assert.Equal(0.567, viewModel.SelectedHeight, 3);
        Assert.Matches(@"Width 1[,.]234", viewModel.SelectionInfo);

        viewModel.TogglePrimitive(first);
        Assert.Single(viewModel.SelectedPrimitives);
        Assert.Same(second, viewModel.SelectedPrimitives[0]);
    }
}