////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016-2016
////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;

namespace PGSoftwareSolutions.Music {
    /// <summary>TODO</summary>
	public interface INoteLength {
        /// <summary>TODO</summary>
		NoteType Type		{ get; }
        /// <summary>TODO</summary>
		Int16	 Numerator	{ get; }
        /// <summary>TODO</summary>
		Int16	 Denominator{ get; }
        /// <summary>TODO</summary>
		Double	 Length		{ get; }
        /// <summary>TODO</summary>
		Int16	 DotCount	{ get; }
        /// <summary>TODO</summary>
		Double	 Time(Int16 tempo);	
	}
    /// <summary>TODO</summary>
	public interface INote {
		/// <summary> Beat-length, or note value, as fraction/multiple of 1 whole note (semi-breve). </summary>
		INoteLength	Length			{ get; }
		/// <summary> Seconds that note is held. </summary>
		Double		Duration		{ get; }
		/// <summary> Seconds before start of next note. </summary>
		Double		LengthSeconds	{ get; }
		/// <summary>Hertz (Hz). </summary>
		Double		Frequency		{ get; }
		/// <summary> In decibels (dB) from -9 to 9 above or below the default of 0. </summary>
		Int16		Energy			{ get; }
		/// <summary> Grand-piano equivalent, from 1 (A_0) to 88 (C_8). </summary>
		PianoKey	PianoKey		{ get; }
        /// <summary>TODO</summary>
		NoteLetter	NoteLetter		{ get; }
        /// <summary>TODO</summary>
		SharpFlat	SharpFlat		{ get; }
		/// <summary> Staccato, Normal, or Legato </summary>
		Style		Style			{ get; }
	}
  /// <summary>TODO</summary>
	public class NoteLength : INoteLength {
        /// <summary>TODO</summary>
		public NoteType	Type		{ get; internal set; }
        /// <summary>TODO</summary>
		public Int16	Numerator	{ get; internal set; }
        /// <summary>TODO</summary>
		public Int16	Denominator	{ get; internal set; }
        /// <summary>TODO</summary>
		public Double	Length		{ get {return (Double)Numerator / (Double)Denominator; } }
        /// <summary>TODO</summary>
		public Int16	DotCount	{ get; internal set; }

        /// <summary>TODO</summary>
		public NoteLength(Int16 noteValue) : this(noteValue,0) {}
        /// <summary>TODO</summary>
		public NoteLength(Int16 noteValue, Int16 dotCount) {
			Type		= NoteType.Breve.GetNoteType(noteValue);
			Numerator	= (Int16)(Math.Pow(3, dotCount));
			Denominator	= (Int16)(Math.Pow(2, dotCount) * noteValue);
			DotCount    = dotCount;
		}
        /// <summary>TODO</summary>
		public NoteLength(NoteLength length, Int16 dotCount) {
			Type		= length.Type;
			Numerator	= (Int16)(length.Numerator	 * (Int16)(Math.Pow(3, dotCount)));
			Denominator = (Int16)(length.Denominator * (Int16)(Math.Pow(2, dotCount)));
			DotCount    = dotCount;
		}
        /// <summary>TODO</summary>
		public Double Time(Int16 tempo) { return this.Length * (60.0 * 4.0) / tempo; }
        /// <summary>TODO</summary>
		public static implicit operator NoteLength(Int16 v) { return new NoteLength(v,0); }
        /// <summary>TODO</summary>
		public static implicit operator NoteLength(Byte  v) { return new NoteLength((Int16)v,0); }

        /// <summary>TODO</summary>
		public override string ToString() {
				return (Numerator == 1) ? String.Format(" /{0}", Denominator)
												: String.Format(" {0}/{1}", Numerator, Denominator);
		}
	}

    /// <summary>TODO</summary>
	public class Note : INote {
		private PianoKey	_noteNum;
		private NoteLetter	_noteLetter;
		private SharpFlat	_sharpFlat;
		private INoteLength	_length;
		private Int16		_energy;		// dB (decibels) above/below standard
		private Style		_style;			// Staccato, Normal, Legato
		private Int16		_tempo;			// quarter-notes / minute
		private Double		_lengthSeconds;	// length in seconds of a whole note

    /// <summary>TODO</summary>
		public INoteLength	Length			{ get { return _length;							} }
    /// <summary>TODO</summary>
		public Double		Duration		{ get { return _style.Length(LengthSeconds);	} } 
    /// <summary>TODO</summary>
		public Double		LengthSeconds	{ get { return _length.Time(_tempo);			} } 
    /// <summary>TODO</summary>
		public Double		Frequency		{ get { return Freq(_noteNum);					} }
    /// <summary>TODO</summary>
		public Int16		Energy			{ get { return _energy;							} }
    /// <summary>TODO</summary>
		public PianoKey		PianoKey		{ get { return _noteNum;						} }
    /// <summary>TODO</summary>
		public NoteLetter	NoteLetter		{ get { return _noteLetter;						} }
    /// <summary>TODO</summary>
		public SharpFlat	SharpFlat		{ get { return _sharpFlat;						} }
    /// <summary>TODO</summary>
		public Style		Style			{ get { return _style;							} }

		/// <summary>TODO</summary>
		///// <param name="noteNum">1 = A0; 49 = A4; 88 = C8; 0 = rest</param>
		///// <param name="style"></param>
		///// <param name="tempo"></param>
		///// <param name="noteValue">Number of these notes that comprise a whole note (semibreve)(</param>
		///// <param name="dotCount">Multiplies noteValue as: 0 -=> 1; 1 => 3/2; 2 => 7/4; 3 => 15/8; etc.</param>
		protected Note(NoteLetter letter, PianoKey noteNum, SharpFlat accid, Style style, Int16 tempo, NoteLength length, Int16 energy = 0) {
			_noteLetter		= letter;
			_sharpFlat		= accid;
			_noteNum		= noteNum;
			_length			= length;
			_tempo			= tempo;
			_lengthSeconds	= (60.0 * 4.0) / _tempo;
			_style			= style;
			_energy			= energy;
		}

		private static Double Freq(PianoKey noteNum) {
			return ((int)noteNum == 0) ? 0 : 440.0D * Math.Pow(2, ((int)noteNum - (int)PianoKey.A_4) / 12.0D);
		}

    /// <summary>TODO</summary>
		public static PianoKey KeyNumber(NoteLetter noteLetter, Int16 octave, SharpFlat sharpFlat) {
			int keyNo = (4 + (octave-1) * 12 + (int)noteLetter.SemitonesFromC() + (int)sharpFlat);
			//if (!Enum.IsDefined(typeof(PianoKey),keyNo)) throw new ArgumentOutOfRangeException(
			//						"KeyNo",keyNo,"Must be between 1 and 88.");
			return checked((PianoKey)keyNo);
		}
    /// <inherited/>
		public override string ToString() {
			return string.Format("Note: {0} {1}", PianoKey.ToString().Replace("s","♯"), Length);
		}
	}
}
