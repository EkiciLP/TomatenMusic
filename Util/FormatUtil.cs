using System;
using System.Collections.Generic;
using System.Text;

namespace TomatenMusic.Util
{
    class FormatUtil
    {
		public static string GetTimestamp(TimeSpan timeSpan)
		{
			if (timeSpan.Hours > 0)
				return String.Format("{0:hh\\:mm\\:ss}", timeSpan);
			else
				return String.Format("{0:mm\\:ss}", timeSpan);
		}

	}
}
