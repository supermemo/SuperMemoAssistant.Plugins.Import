using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.Import
{
  class IpcService
  {
    public IpcService()
    {
      Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
    }

    public void Run()
    {

    }
  }
}
