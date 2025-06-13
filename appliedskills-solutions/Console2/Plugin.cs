using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Plugins;

public class ProductsPlugin
{
  // TODO 3.1: Add plugin code to handle requests for product length
  [KernelFunction, Description("Get the length in mm for a given product ID")]
  public static double Length([Description("The product ID")] string productID)
  {
    // look up the length for the given product ID

  }

  [KernelFunction, Description("Get the width in mm for a given product ID")]
  public static double Width([Description("The product ID")] string productID)
  {
    // look up the width for the given product ID
    if (_productDimensions.TryGetValue(productID, out var dimensions))
    {
      return dimensions.width;
    }
    return -1;
  }

  // look up table for length and width properties for a given product ID
  private static Dictionary<string, (double length, double width)> _productDimensions = new()
  {
    { "widget1", (27.0, 37.5) },
    { "widget2", (19.5, 43.0) },
    { "widget3", (12.9, 50.1) },
    { "M307000", (20.2, 58.4) },
    { "M307100", (27.5, 46.5) },
    { "M6432-A", (28.3, 66.3) },
    { "S486-LX", (29.4, 77.5) }
  };
}