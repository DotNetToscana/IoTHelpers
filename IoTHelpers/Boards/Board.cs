using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHelpers.Boards
{
	public abstract class Board : IDisposable
	{
		public virtual void Dispose()
		{ }
	}
}
