using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Lavalink;

namespace TomatenMusic.Music.Entitites
{
    class MultiTrack
    {

        public static List<MultiTrack> ToMultiTrackList(IEnumerable<LavalinkTrack> tracks)
        {
            List<MultiTrack> newTracks = new List<MultiTrack>();

            foreach (var track in tracks)
            {
                newTracks.Add(new MultiTrack(track));
            }

            return newTracks;
        }

        public MultiTrack(LavalinkTrack track)
        {
            LavalinkTrack = track;
            TrackString = track.TrackString;
            YoutubeIdentifier = track.Identifier;
            IsSeekable = track.IsSeekable;
            Author = track.Author;
            Length = track.Length;
            IsStream = track.IsStream;
            Position = track.Position;
            Title = track.Title;
            Uri = track.Uri;
        }

        //
        // Summary:
        //     Gets or sets the ID of the track to play.
        public string TrackString { get; set;}
        //
        // Summary:
        //     Gets the identifier of the track.
        public string YoutubeIdentifier { get; private set; }
        //
        // Summary:
        //     Gets whether the track is seekable.
        public bool IsSeekable { get; private set; }
        //
        // Summary:
        //     Gets the author of the track.
        public string Author { get; private set; }
        //
        // Summary:
        //     Gets the track's duration.
        public TimeSpan Length { get; private set; }
        //
        // Summary:
        //     Gets whether the track is a stream.
        public bool IsStream { get; private set; }
        //
        // Summary:
        //     Gets the starting position of the track.
        public TimeSpan Position { get; private set; }
        //
        // Summary:
        //     Gets the title of the track.
        public string Title { get; private set; }
        //
        // Summary:
        //     Gets the source Uri of this track.
        public Uri Uri { get; private set; }

        public int YoutubeViews { get; private set; }

        public LavalinkTrack LavalinkTrack { get; private set; }

        public bool IsQueueLoopItem { get; set; }

        public bool IsFile { get; set; }
    }
}
