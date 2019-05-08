#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2019/04/13 20:51
// Modified On:  2019/04/13 20:55
// Modified By:  Alexis

#endregion




using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace SuperMemoAssistant.Plugins.Import.Extensions
{
  public static class DynamicStringEx
  {
    #region Constants & Statics

    // ReSharper disable once InconsistentNaming
    private static readonly Regex RE_Interpolate = new Regex("{(?<exp>[^}]+)}", RegexOptions.Compiled);

    #endregion




    #region Methods

    public static string Interpolate(this string text, params (string name, object instance)[] parameters)
    {
      return RE_Interpolate.Replace(text, match =>
      {
        var expParams = parameters.Select(p => Expression.Parameter(p.instance.GetType(), p.name));

        var exp = System.Linq.Dynamic.DynamicExpression.ParseLambda(
          expParams.ToArray(),
          null,
          match.Groups["exp"].Value);
        var res = exp.Compile().DynamicInvoke(parameters.Select(p => p.instance).ToArray());

        return res?.ToString() ?? string.Empty;
      });
    }

    #endregion
  }
}
