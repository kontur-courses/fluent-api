using System;
using System.Collections.Generic;
using System.Threading;

namespace FluentTask
{
	public class Program
	{
		public static void Main()
		{
			var spectacle = new Spectacle()
				.Say("Привет мир!")
				.Delay(TimeSpan.FromSeconds(1))
				.UntilKeyPressed(s =>
					s.TypeText("тра-ля-ля")
					.TypeText("тру-лю-лю")
				)
				.Say("Пока-пока!");

			spectacle.Play();
		}
	}

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