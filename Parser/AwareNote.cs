////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016-2016
////////////////////////////////////////////////////////////////////////
using System;

using Irony.Parsing;

using PGSoftwareSolutions.Music;

namespace PGSoftwareSolutions.Qabc {
	public interface IAwareNote : INote {
		int SpanPosition	{ get; }
        int SpanLength	    { get; }
        int SpanEndPos	    { get; }
	}

	public class AwareNote : Note, IAwareNote {
		public int SpanPosition	{ get; private set; }
		public int SpanLength   { get; private set; }
		public int SpanEndPos   { get; private set; }

		public static AwareNote GetAwareNote(NoteLetter letter, PianoKey noteNum, SharpFlat accid, 
								NoteLength length, Style style, Int16 tempo, Int16 energy, SourceSpan span) {
			return new AwareNote(letter, noteNum, accid, length, style, tempo, energy, span);
		}
		private AwareNote(NoteLetter letter, PianoKey noteNum, SharpFlat accid, NoteLength length,  
								Style style, Int16 tempo, Int16 energy, SourceSpan span) 
		: base(letter, noteNum, accid, style, tempo, length, energy) {
			SpanPosition	= span.Location.Position;
			SpanLength		= span.Length;
			SpanEndPos		= span.EndPosition;
		}
	}
}
