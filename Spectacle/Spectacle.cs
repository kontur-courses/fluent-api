using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectacleSample
{
	public class Spectacle
	{
		private readonly List<Action> actions = new List<Action>();

		public void Schedule(Action action)
		{
			actions.Add(action);
		}

		public Spectacle Say(string message)
		{
			Schedule(() => Console.WriteLine(message));
			return this;
		}

		public Spectacle Delay(TimeSpan timeSpan)
		{
			Schedule(() => Thread.Sleep(timeSpan));
			return this;
		}

		public void Play()
		{
			foreach (var action in actions)
			{
				action();
			}
		}

		public Spectacle UntilKeyPressed(Func<Spectacle, Spectacle> inner)
		{
			var innerSpectacle = inner(new Spectacle());
			Schedule(() =>
			{
				while (!Console.KeyAvailable)
				{
					innerSpectacle.Play();
				}
				Console.ReadKey(true);
			});
			return this;
		}
	}
}