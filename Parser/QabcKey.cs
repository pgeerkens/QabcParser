////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016-2016
////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;

using PGSoftwareSolutions.Music;

namespace PGSoftwareSolutions.Qabc {
	public enum KeyAccidental {
		Flat		= -1,
		Natural	= 0,
		Sharp		= +1
	}

	public enum HighlandPipesKey { HP, Hp }
	internal static class Extensions {
		#region HighlandPipes options
		public static KeyOptions Options(this HighlandPipesKey pipes) {
			KeyOptions options = KeyOptions.HighlandPipes;
			if (pipes == HighlandPipesKey.Hp)
				options |= KeyOptions.HideKeySig;
			return options;
		}
		#endregion HighlandPipes options
	}

	[FlagsAttribute]
	public enum KeyOptions {
		None				= 0x00,
		HighlandPipes	= 0x01,
		HideKeySig		= 0x02
	}

	public struct Pitch {
		public NoteLetter		BaseNote;
		public SharpFlat		Accidental;

		public Pitch(NoteLetter baseNote) : this(baseNote,SharpFlat.Natural){}
		public Pitch(NoteLetter baseNote, SharpFlat accidental) {
			BaseNote = baseNote; 	Accidental = accidental;
		}
		public override string ToString() {
			return BaseNote.ToString() + Accidental.ToString();//"b #".Substring((int)Accidental+1,1);
		}
	}

	public class QabcKey {
		#region private fields & properties
		private static readonly	int[] Sharps	= new int[] { 3, 0, 4, 1, 5, 2, 6};
		private static readonly	int[]	Flats		= new int[] { 6, 2, 5, 1, 4, 0, 3};
		#endregion

		#region constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseNote">Valid values are an upper case character between A and G</param>
		/// <param name="accidental">Valid values are '#', 'b' and ' '</param>
		/// <param name="mode">Mode must be exactly 'm'; or its first three characters must be one of min, maj,
		/// Lyd, Ion, Mix, Dor, Aeo, Phr, or Loc. The case of the string is ignored.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public QabcKey(HighlandPipesKey keyOptions) 
		: this(NoteLetter.D, SharpFlat.Natural, Mode.maj) { Options = (KeyOptions)keyOptions; }
		public QabcKey(NoteLetter baseNote, SharpFlat accidental = SharpFlat.Natural, Mode mode = Mode.maj )
		: this(new Pitch(baseNote,accidental), mode) {}
		public QabcKey(Pitch pitch, Mode mode = Mode.maj) {
			KeyNote	= pitch; 
			KeyMode	= mode;
			KeySig	= MakeKeySignature();
		}
		#endregion constructors

		private int[] MakeKeySignature() {
			int[] keySignature = new int[] {0,0,0,0,0,0,0};

			int offsetMode		 = KeyMode.BaseNote().FCGDAEB()	- 1;	// -1 to 5
			int offsetKeyNote	 = KeyNote.BaseNote.FCGDAEB()		- 1;	// -1 to 5
			offsetKeyNote		+= 7 * (int)KeyNote.Accidental		- offsetMode;

			if (offsetKeyNote < -7 || 7 < offsetKeyNote) 
				throw new ArgumentOutOfRangeException("keyNote", KeyNote, 
					"Invalid key for mode: " + KeyMode);
			for (int i=0; i<offsetKeyNote; i++) keySignature[Sharps[i]]++;
			for (int i=0; i>offsetKeyNote; i--) keySignature[Flats[-i]] --;
			return keySignature;
		}

		#region public properties: Options, KeySignature, GetPianoKeyNo
		public Pitch		KeyNote	{ get; private set; }
		public Mode			KeyMode	{ get; private set; }
		public KeyOptions	Options	{ get; private set; }
		public int[]		KeySig	{ get; private set; }
		public PianoKey GetPianoKeyNo(NoteLetter baseNote, string accidentals, string octaves) {
			const PianoKey pianoKey	= PianoKey.C_4; //40;	// C4, Middle C

			int octave		=  0;
			for (int i=0; i< octaves.Count(); i++)
				switch(octaves[i]) {
					case '\'':	octave++; break;
					case ',':	octave--; break;
					default:		throw new ArgumentOutOfRangeException("octaves["+i.ToString()+"]",
										octaves, "Only comma (,) and apostrophe (') acceptable.");
				}

			int noteIndex	= baseNote.NoteIndex();
			int accidental	=  0;
			for (int i=0; i<accidentals.Count(); i++) {
				switch(accidentals[i]) {
					case '_':	accidental--;							break;
					case '=':	accidental -= KeySig[noteIndex];	break;
					case '^':	accidental++;							break;
					default:		throw new ArgumentOutOfRangeException("accidentals["+i.ToString()+"]",
										accidentals, "Only underbar (_), caret (^) and equals (=) acceptable.");
				}
			}
			return pianoKey + octave*8 + baseNote.SemitonesFromC() + KeySig[noteIndex] + accidental;
		}
		#endregion public properties
	}
}
