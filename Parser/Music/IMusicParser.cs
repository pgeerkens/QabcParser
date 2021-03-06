﻿////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016-2016
////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Irony.Parsing;

namespace PGSoftwareSolutions.Music {
	/// <summary>TODO</summary>
	public interface IMusicParser<T>  {
		/// <summary>TODO</summary>
		T Parse(string music);
		/// <summary>TODO</summary>
		T Parse(TextReader reader);
	}
	/// <summary>Note types.</summary>
	/// <remarks>
	/// Note length can be calculated as Math.Pow(2,-Value).
	/// </remarks>
	public enum NoteType {
		/// <summary>A quadruple whole note.</summary>
		Longa			= -2,
		/// <summary>A double whole note.</summary>
		Breve			= -1,
		/// <summary>A whole note.</summary>
		SemiBreve,
		/// <summary>A half note.</summary>
		Minim,
		/// <summary>A quarter note.</summary>
		Crotchet,
		/// <summary>An Eighth note.</summary>
		Quaver,
		/// <summary>A 16th note.</summary>
		SemiQuaver,
		/// <summary>A 32nd Note.</summary>
		DemiSemiQuaver,
		/// <summary>A 64th note.</summary>
		HemiDemiSemiQuaver
	}
	/// <summary>TODO</summary>
	public static class NoteTypeExtensions {
		/// <summary>TODO</summary>
		public static NoteType GetNoteType (this NoteType type, Int16 length) {
			var i = (int)Math.Floor(Math.Log(length,2));
			var n = (NoteType)(i);
			if (!System.Enum.IsDefined(typeof(NoteType),n))
				throw new ArgumentOutOfRangeException("length",length,"Must be between 1 and 64.");
			return n;
		}
	}

	/// <summary>TODO</summary>
	public interface IPitch {
		/// <summary>TODO</summary>
		PianoKey		PianoKey		{ get; }
		/// <summary>TODO</summary>
		NoteLetter	NoteLetter	{ get; }
		/// <summary>TODO</summary>
		SharpFlat	SharpFlat	{ get; }
	}

	#pragma warning disable
	public enum Octave { _0, _1, _2, _3, _4, _5, _6, _7, _8 }
	#pragma warning restore

	/// <summary>Play styles</summary>
	public struct Style  {
		/// <summary>Legato</summary>
		public static Style L => new Style(_enum.L, "Legato",   0.950F);
		/// <summary>Normal</summary>
		public static Style N => new Style(_enum.N, "Normal",   0.875F);
		/// <summary>Staccato</summary>
		public static Style S => new Style(_enum.S, "Staccato", 0.750F);

		/// <summary>TODO</summary>
		public static Style ConvertValue(ParsingContext c, string s) =>
			_values[(int)(System.Enum.Parse(typeof(_enum), s.ToUpper()))];

		private enum _enum {L,N,S};
		private static IList<Style> _values = new List<Style> { L, N, S };

		private Style(_enum value, string name, double factor) :this() {
			_value  = (int)value;
			_name   = name;
			_factor = factor;
		}
		/// <summary>TODO</summary>
		public int    Value  { get { return _value;  } } readonly int    _value;
		/// <summary>TODO</summary>
		public string Name   { get { return _name;   } } readonly string _name;
		/// <summary>TODO</summary>
		public double Factor { get { return _factor; } } readonly double _factor;

		/// <summary>TODO</summary>
		public double Length(double seconds) => seconds * _factor;
	}

	/// <summary>TODO</summary>
    /// <remarks>
    /// Tone number (or alternatively, Piano-Key number) is calculated as
    /// base tome plus/minus enum Value.
    /// </remarks>
	public enum SharpFlat {
	    /// <summary>TODO</summary>
		FlatFlat	= -2,
	    /// <summary>TODO</summary>
		Flat		= -1,
	    /// <summary>TODO</summary>
		Natural		=  0,
	    /// <summary>TODO</summary>
		Sharp		= +1,
	    /// <summary>TODO</summary>
		SharpSharp	= +2		// "\uD834\uDD2A" or U+1D12A
	}

	/// <summary>TODO</summary>
	public enum OctaveShift {
		/// <summary>TODO</summary>
		Down	= -1,
		/// <summary>TODO</summary>
		Up		= +1
	}

	#pragma warning disable
	public enum Mode { maj,  min,	lyd,	ion,	mix,	dor,	aeo,	phr,	loc }

	public enum NoteLetter { C, D, E, F, G, A, B, c, d, e, f, g, a, b }
	#pragma warning restore

	#pragma warning disable
	public enum PianoKey {                     Rest = 0, A_0, Bf_0, B_0,	//  1 ->  3
		C_1, Cs_1, D_1, Ef_1, E_1, F_1, Fs_1, G_1, Af_1, A_1, Bf_1, B_1,	//  4 -> 15
		C_2, Cs_2, D_2, Ef_2, E_2, F_2, Fs_2, G_2, Af_2, A_2, Bf_2, B_2,	// 16 -> 27
		C_3, Cs_3, D_3, Ef_3, E_3, F_3, Fs_3, G_3, Af_3, A_3, Bf_3, B_3,	// 28 -> 39
		C_4, Cs_4, D_4, Ef_4, E_4, F_4, Fs_4, G_4, Af_4, A_4, Bf_4, B_4,	// 40 -> 51
		C_5, Cs_5, D_5, Ef_5, E_5, F_5, Fs_5, G_5, Af_5, A_5, Bf_5, B_5,	// 52 -> 63
		C_6, Cs_6, D_6, Ef_6, E_6, F_6, Fs_6, G_6, Af_6, A_6, Bf_6, B_6,	// 64 -> 75
		C_7, Cs_7, D_7, Ef_7, E_7, F_7, Fs_7, G_7, Af_7, A_7, Bf_7, B_7,	// 76 -> 87
		C_8,
        Bar = 89
    }
	#pragma warning restore

	/// <summary>TODO</summary>
	public static class EnumExtensions {
		#region SharpFlat extensions
	/// <summary>TODO</summary>
		public static SharpFlat Parse(this SharpFlat sharpFlat, string s) {
			return FromString(sharpFlat, s);
		}

	/// <summary>TODO</summary>
		public static SharpFlat FromString(this SharpFlat sharpFlat, string s) {
			switch (s ?? string.Empty) {
				case "-":
//				case "_":
//				case "♭":
					return SharpFlat.Flat;
				case "#":
				case "+":
//				case "^":
//				case "♯":
					return SharpFlat.Sharp;
				case "":		
				case " ":		
//				case "=":
//				case "♮":
					return SharpFlat.Natural;
				default:	
					throw new ArgumentOutOfRangeException("SharpFlat", s, 
					"Only these characters (and an empty string) are valid: -, #, +.");
						//, ♭, ♯, and ♮.");
			}
		}
		static string[] symbols = {"♭♭", "♭", "♮", "♯", "♯♯"};
	/// <summary>TODO</summary>
		public static string AsString(this SharpFlat sharpFlat) {
			return symbols [(int)sharpFlat + 2];
		}
		#endregion SharpFlat extensions

		#region OctaveShift extensions
		/// <summary>TODO</summary>
		public static OctaveShift FromString(this OctaveShift octaveShift, string value) {
			switch (value.Trim(new char[] {'o','O'})) {
				case "<":	return OctaveShift.Down;
				case ">":	return OctaveShift.Up;
				default:		throw new ArgumentOutOfRangeException("value", value,
									"input string must end in '<' or '>'.");
			}
		}
		/// <summary>TODO</summary>
		public static string AsString(this OctaveShift shift) { return shift.ToString(); }
		#endregion OctaveShift extensions

		#region NoteLettter extensions
		/// <summary>TODO</summary>
		public static NoteLetter Parse (this NoteLetter letter, string s) {
			return (NoteLetter)System.Enum.Parse(typeof(NoteLetter),s);
		}
		/// <summary>
		/// Returns the index for the base note according to:
		/// <b>F</b>ather <b>C</b>harles <b>G</b>oes <b>D</b>own <b>A</b>nd <b>E</b>nds <B>B</B>attle.
		/// </summary>
		/// <param name="baseNote"></param>
		/// <returns></returns>
		public static Int16 FCGDAEB(this NoteLetter baseNote) {
			return (Int16)"FCGDAEB".IndexOf(baseNote.ToString()[0]);	
		}
		/// <summary>TODO</summary>
		public static int SemitonesFromC (this NoteLetter note) {
			return (new int[] { 0, 2, 4, 5, 7, 9,11
							  ,12,14,16,17,19,21,23}) [(int)note];
		}
		/// <summary>TODO</summary>
		public static int NoteIndex (this NoteLetter note) {
			return "CDEFGAB".IndexOf(note.ToString().ToUpper());
		}
		#endregion

		#region Mode extensions
	/// <summary>TODO</summary>
		public static string AsString(this Mode mode) {
			switch (mode) {
				case Mode.maj: return "major";
				case Mode.min: return "minor";
				case Mode.aeo:	return "Aeolian";
				case Mode.dor: return "Dorian";
				case Mode.ion: return "Ionian";
				case Mode.loc: return "Locrian";
				case Mode.lyd: return "Lydian";
				case Mode.mix: return "Mixolydian";
				case Mode.phr: return "Phrygian";
				default: throw new ArgumentOutOfRangeException("Mode", mode, 
					"Invalid enumeration value.");
			}
		}
	/// <summary>TODO</summary>
		public static NoteLetter BaseNote(this Mode mode) {
			switch (mode) {
				case Mode.min: 
				case Mode.aeo:	return NoteLetter.A;
				case Mode.dor: return NoteLetter.D;
				case Mode.maj: 
				case Mode.ion: return NoteLetter.C;
				case Mode.loc: return NoteLetter.B;
				case Mode.lyd: return NoteLetter.F;
				case Mode.mix: return NoteLetter.G;
				case Mode.phr: return NoteLetter.E;
				default: throw new ArgumentOutOfRangeException("Mode", mode, 
					"Invalid enumeration value.");
			}
		}
		#endregion Mode extensions

		#region PianoKey extensions
		/// <summary>TODO</summary>
		public static int Octave(this PianoKey @this) {
			return ((int)@this + 8) / 12;
		}
		/// <summary>TODO</summary>
		public static NoteLetter Letter(this PianoKey @this, int countSharps = 0) {
			var noteNo	= ((new int[] {4,5,6,6,0,0,1,2,2,3,3,4}) [((int)@this) % 12]);
							//  C   D   E F   G   A   B	// favour G# in sharp keys; A_ otherwise
			if ((new int[] {7,0,0,4,0,6,0,0,1,0,5,0})[noteNo] >= countSharps && countSharps>0) noteNo--;
			if ((new int[] {0,4,0,0,7,0,5,0,0,0,0,6})[noteNo] >=-countSharps && countSharps<0) noteNo++;
			return (NoteLetter)noteNo;
		}
		/// <summary>TODO</summary>
		public static SharpFlat Accidental(this PianoKey key) {
			var index = ((int)key) % 12;
			return (SharpFlat)((new int[] {-1,0,-1,0,0,1,0,-1,0,0,1,0})[index]);
		}
		// Key    A_ A  B_ B  C  C# D  E_ E  F  F# G     A_  A  B_ B  C  C# D  E_ E  F  F# G
		// F#	{ 4, 5, 5, 6, 6, 0, 1, 1, 2, 2, 3, 4}	{ 0,-1, 0,-1, 0, 0,-1, 0,-1, 0, 0,-1}	B
		// C#	{ 4, 5, 5, 6, 0, 0, 1, 1, 2, 2, 3, 4}	{ 0,-1, 0, 0,-1, 0,-1, 0,-1, 0, 0,-1}	E
		// B	{ 4, 5, 5, 6, 0, 0, 1, 1, 2, 3, 3, 4}	{ 0,-1, 0, 0,-1, 0,-1, 0, 0,-1, 0,-1}	A
		// E	{ 4, 5, 6, 6, 0, 0, 1, 1, 2, 3, 3, 4}	{ 0, 0,-1, 0,-1, 0,-1, 0, 0,-1, 0,-1}	D
		// A	{ 4, 5, 6, 6, 0, 0, 1, 2, 2, 3, 3, 4}	{ 0, 0,-1, 0,-1, 0, 0,-1, 0,-1, 0,-1}	G

		// D	{ 5, 5, 6, 6, 0, 0, 1, 2, 2, 3, 3, 4}	{-1, 0,-1, 0,-1, 0, 0,-1, 0,-1, 0, 0}	C
		// G	{ 5, 5, 6, 6, 0, 0, 1, 2, 2, 3, 3, 4}	{-1, 0,-1, 0, 0,+1, 0,-1, 0,-1, 0, 0}	F
		// C	{ 5, 5, 6, 6, 0, 0, 1, 2, 2, 3, 3, 4}	{-1, 0,-1, 0, 0,+1, 0,-1, 0, 0,+1, 0}	-
		// F	{ 5, 5, 6, 6, 0, 0, 1, 2, 2, 3, 3, 4}	{-1, 0, 0,+1, 0,+1, 0,-1, 0, 0,+1, 0}	B
		// B_	{ 5, 5, 6, 6, 0, 0, 1, 2, 2, 3, 3, 4}	{-1, 0, 0,+1, 0,+1, 0, 0,+1, 0,+1, 0}	E
		// E_	{ 5, 5, 6, 6, 0, 0, 1, 2, 2, 3, 3, 4}	{ 0,+1, 0,+1, 0,+1, 0, 0,+1, 0,+1, 0}	A

		// A_	{ 5, 5, 6, 6, 0, 1, 1, 2, 2, 3, 3, 4}	{ 0,+1, 0,+1, 0, 0,+1, 0,+1, 0,+1, 0}	D
		// D_	{ 5, 5, 6, 6, 0, 1, 1, 2, 2, 3, 4, 4}	{ 0,+1, 0,+1, 0, 0,+1, 0,+1, 0, 0,+1}	G
		// G_	{ 5, 5, 6, 0, 0, 1, 1, 2, 2, 3, 4, 4}	{ 0,+1, 0, 0,+1, 0,+1, 0,+1, 0, 0,+1}	C
		// C_	{ 5, 5, 6, 0, 0, 1, 1, 2, 3, 3, 4, 4}	{ 0,+1, 0, 0,+1, 0,+1, 0, 0,+1, 0,+1}	F
		#endregion
	}
}
