using System;
using System.Collections.Generic;

namespace SpectacleSample
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
}