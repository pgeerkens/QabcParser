////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016
////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PGSoftwareSolutions.Music {
  /// <summary>TODO</summary>
	public struct Notes { //: List<INote>, INotes{
    /// <summary>TODO</summary>
		public static void Init() {
			Tempo		= 120;			// range: [32,255] quarter notes per minute
			Length	= 4;				// range: [ 1, 64] semi-breve to hemi-demi-semi-quaver
			Octave	= 4;				// range: [ 0,  6]
			Style		= Style.N;		// Normal, Legato, and Staccato supported
		} 
		private static void ValidateField(string field, byte lower, byte upper, byte value) {
			if (value < lower || value > upper) 
				throw new ArgumentOutOfRangeException(field,value,
					string.Format("{0} must be between {1} and {2}.",field,lower, upper));
		}

		#region Tempo, Length & Style
		private static byte _tempo;
    /// <summary>TODO</summary>
		public static byte Tempo {
			get { return _tempo; }
			set {	ValidateField("Tempo",32,255,value); _tempo = value; 	}
		}

    /// <summary>TODO</summary>
		public static NoteLength GetNoteLength { get { return new NoteLength(Length, 0); } }

		private static byte _length;
    /// <summary>TODO</summary>
		public static byte Length {
			get { return _length; }
			set { ValidateField("Length",1,64,value);	_length = value;	}
		}

    /// <summary>TODO</summary>
		public static Style	Style		{ get; set; }
    /// <summary>TODO</summary>
		public static string	SetStyle	{ set { Style = (Style)Enum.Parse(typeof(Style),value,true); } }
		#endregion Tempo, Value & Style

		#region Octave & KeyNumber
		private static byte _octave;
    /// <summary>TODO</summary>
		public static byte Octave { 
			get { return _octave; } 
			set { ValidateField("Octave",0,8,value); 	_octave = value;	}
		}
        /// <summary>TODO</summary>
        public static OctaveShift Shift { get {return 0;} set { Octave = (byte)(Octave + value); } }

    /// <summary>TODO</summary>
		public static PianoKey KeyNumber(NoteLetter noteLetter, SharpFlat sharpFlat) {
			return Note.KeyNumber(noteLetter, Octave, sharpFlat);
		}
		#endregion Octave & KeyNumber
	}

    /// <summary>TODO</summary>
	public struct Notes2 {
		private byte		_tempo;		// range: [32,255] quarter notes per minute
		private byte		_length;		// range: [ 1, 64] semi-breve to hemi-demi-semi-quaver
		private byte		_octave;		// range: [ 0,  6]
		private Style		_style;		// Normal, Legato, and Staccato supported

    /// <summary>TODO</summary>
		public void Init() {
			_tempo	= 120;
			_length	= 4;
			_octave	= 4;
			_style	= Style.N;
		}

    /// <summary>TODO</summary>
		public Notes2(Notes2 notes2) {
			_tempo	= notes2.Tempo;
			_length	= notes2.Length;
			_octave	= notes2.Octave;
			_style	= notes2.Style;
		}

    /// <summary>TODO</summary>
		public NoteLength GetNoteLength { get { return new NoteLength(Length, 0); } }

		#region Length, Octave, Style & Tempo
    /// <summary>TODO</summary>
		public byte Length {
			get { return _length; }
			set { ValidateField("Length",1,64,value);	_length = value;	}
		}

    /// <summary>TODO</summary>
		public byte Octave { 
			get { return _octave; } 
			set { ValidateField("Octave",0,8,value); 	_octave = value;	}
		}
    /// <summary>TODO</summary>
		public OctaveShift ShiftOctave { set { Octave = (byte)(Octave + value); } }

    /// <summary>TODO</summary>
		public Style	Style		{ get { return _style;} set { _style = value;} }
        /// <summary>TODO</summary>
        public string	SetStyle	{ set { Style = (Style)Enum.Parse(typeof(Style),value,true); } }

    /// <summary>TODO</summary>
		public byte Tempo {
			get { return _tempo; }
			set {	ValidateField("Tempo",32,255,value); _tempo = value; 	}
		}
		#endregion Length, Octave, Style & Tempo

		private static void ValidateField(string field, byte lower, byte upper, byte value) {
			if (value < lower || value > upper) 
				throw new ArgumentOutOfRangeException(field,value,
					string.Format("{0} must be between {1} and {2}.",field,lower, upper));
		}
		private static void ValidateEnum<T>(string field, Func<T,byte,bool> isDefined, byte value) {

		}
	}
}
