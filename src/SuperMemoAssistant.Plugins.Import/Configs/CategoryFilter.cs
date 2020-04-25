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
// Created On:   2019/04/14 02:47
// Modified On:  2019/04/14 02:51
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using SuperMemoAssistant.Plugins.Import.Models;

namespace SuperMemoAssistant.Plugins.Import.Configs
{
  public class CategoryFilter
    : IEquatable<CategoryFilter>
  {
    #region Constants & Statics

    public static IEqualityComparer<CategoryFilter> CategoryComparer { get; } = new CategoryEqualityComparer();

    #endregion




    #region Constructors

    public CategoryFilter(string inlineFilter)
    {
      if (inlineFilter.StartsWith("+", StringComparison.InvariantCultureIgnoreCase))
      {
        Category = inlineFilter.Substring(1);
        Mode     = FilterMode.Include;
      }

      else if (inlineFilter.StartsWith("-", StringComparison.InvariantCultureIgnoreCase))
      {
        Category = inlineFilter.Substring(1);
        Mode     = FilterMode.Exclude;
      }

      else
      {
        Category = inlineFilter;
        Mode     = FilterMode.Exclude;
      }
    }

    #endregion




    #region Properties & Fields - Public

    public string     Category { get; }
    public FilterMode Mode     { get; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != GetType())
        return false;

      return Equals((CategoryFilter)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return Category != null ? Category.GetHashCode() : 0;
    }

    /// <inheritdoc />
    public bool Equals(CategoryFilter other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return string.Equals(Category, other.Category, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion




    #region Methods

    public static bool operator ==(CategoryFilter left, CategoryFilter right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(CategoryFilter left, CategoryFilter right)
    {
      return !Equals(left, right);
    }

    #endregion




    private sealed class CategoryEqualityComparer : IEqualityComparer<CategoryFilter>
    {
      #region Methods Impl

      public bool Equals(CategoryFilter x, CategoryFilter y)
      {
        if (ReferenceEquals(x, y))
          return true;
        if (ReferenceEquals(x, null))
          return false;
        if (ReferenceEquals(y, null))
          return false;
        if (x.GetType() != y.GetType())
          return false;

        return string.Equals(x.Category, y.Category, StringComparison.InvariantCultureIgnoreCase);
      }

      public int GetHashCode(CategoryFilter obj)
      {
        return obj.Category != null ? obj.Category.GetHashCode() : 0;
      }

      #endregion
    }
  }
}
