using System;
using System.Threading;

namespace SpectacleSample
{
	public static class SpectacleExtensions
	{
		public static Spectacle TypeText(this Spectacle spectacle, string message)
		{
			spectacle.Schedule(() =>
			{
				foreach (var ch in message)
				{
					Console.Write(ch);
					Thread.Sleep(50);
				}
				Console.WriteLine();
			});
			return spectacle;
		}
	}
}