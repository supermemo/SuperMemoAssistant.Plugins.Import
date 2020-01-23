using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.Import.Models
{
  public interface IBrowserHostService
  {
    List<string> GetTabs();
  }
}
