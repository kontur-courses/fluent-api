using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class MusicAlbum
    {
        public string Name {  get;  }
        public string Artist { get; }
        public int Year { get; }
        public double Rating { get; }
        public Dictionary<string, string> Genres { get; } //genre and its' description
        public List<Song> Songs{ get; }

        public MusicAlbum(string name, string artist, Dictionary<string,string> genres, int year, double rating, List<Song> songs)
        {
            Name = name;
            Artist = artist;
            Year = year;
            Rating = rating;
            this.Genres = genres;
            this.Songs = songs;
        }
    }

    public class Song
    {
        public string Name { get;  }

        public Song(string name)
        {
            Name = name;
        }
    }
}