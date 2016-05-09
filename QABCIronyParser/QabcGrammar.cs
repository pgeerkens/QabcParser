////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012
////////////////////////////////////////////////////////////////////////
//#define KeySpec
using System;
using System.Collections.Generic;

using PGSoftwareSolutionsInc.PGIrony;
using PGSoftwareSolutionsInc.Music;

namespace PGSoftwareSolutionsInc.Qabc {
    /// <summary>TODO</summary>
	[Irony.Parsing.Language("QABC", "1.0", "QBasic-ABC Parser")]
	public partial class QabcGrammar : PGIrony.Grammar<QabcAstContext> {
        private enum Dots { zero, one, two, three }
        internal QabcGrammar()	: base(false) {
            #region 1-Terminals
            #region Directions
            var bar             = new RegexBasedTerminalX("bar", @":?[|][1-9:\]]?");
            var mode			= new MyKeyTerm("M", "mode");
			var modeStyle		= new RegexEnumTerm<Style>(@"[NLS]");
			var length			= new MyKeyTerm("L", "length");
			var octave			= new MyKeyTerm("O", "octave");
			var tempo			= new MyKeyTerm("T", "tempo");
			var integer			= new MusicIntegerLiteral<Byte>("integer");
			var shift			= new RegexEnumTerm<OctaveShift>(@"O?[<>]", 
											s => OctaveShift.Up.FromString(s));
			var modePlay		= new RegexBasedTerminalX("modePlay",	@"M[BF]");
			#endregion Directions

			#region Notes
			var note			= new MyKeyTerm("N", "note");
			var rest			= new MyKeyTerm("P", "rest");
			var noteLetter		= new RegexEnumTerm<NoteLetter>(@"[CDEFGAB]");
			var dot				= ToTerm("."); 
			var sharpFlat		= new RegexEnumTerm<SharpFlat>(@"[-#+]", 
											s => SharpFlat.Natural.FromString(s));
			var bagpipes		= new RegexBasedTerminalX("bagpipes", @"H[pP]");
			#endregion Notes
			var whiteSpace		= new RegexBasedTerminalX("whiteSpace",	@"[ \t]+");
			#endregion 1-Terminals

			MarkPunctuation(modePlay);		// obsolete instruction - recognize & discard
			MarkPunctuation(NewLine, whiteSpace, length, mode, octave, tempo);

			#region 2-Nonterminals
			var Music			= new TransientNonTerminal("Music");
			var Direction		= new TransientNonTerminal("Direction");

			var Tempo			= new CommandNonTerminal <Notes,byte>			(Notes => Notes.Tempo);
			var Length			= new CommandNonTerminal <Notes,byte>			(Notes => Notes.Length);
			var Style			= new CommandNonTerminal <Notes,Style>			(Notes => Notes.Style);
			var OctaveNo		= new CommandNonTerminal <Notes,byte>			(Notes => Notes.Octave);
			var Shift			= new CommandNonTerminal <Notes,OctaveShift>	(Notes => Notes.Shift);

			var Beam			= new TypedNonTerminal<BeamAstNode>();
            var MusicBar        = new TypedNonTerminal<MusicBarAstNode>();
            var MusicLine		= new TypedNonTerminal<MusicLineAstNode>();
//			var Tune			= new TypedNonTerminal(typeof(TuneNode));

			var Note			= new TypedNonTerminal<NoteAstNode>();
			var Rest			= new TypedNonTerminal<RestAstNode>();
			var Pitch			= new TypedNonTerminal<PitchAstNode>();
			var NoteNumber		= new TypedNonTerminal<NoteNumberAstNode>();
			var NoteElement	    = new TypedNonTerminal<NoteElementAstNode>();
			var PitchOrRest	    = new TransientNonTerminal("PitchOrRest");

#if KeySpec
			var FieldK				= new MyKeyTerm(@"K:", "FieldK");
			MarkPunctuation(FieldK);

//			var Alpha				= new NonTerminal("alpha",				AlphaUpper | AlphaLower);

			var FieldKeyLine		= new NonTerminal("fieldKeyLine",		typeof(FieldKeyNode));
			var Key					= new NonTerminal("key",					typeof(KeyNode));
			var KeySpec				= new NonTerminal("keySpec",				typeof(PitchNode));
//			var keyNote				= new NonTerminal("keyNote",				typeof(KeyNoteNode));
//			var modeSpec			= new NonTerminal("modeSpec",				typeof(ModeSpecNode));
//			var Mode					= new NonTerminal("mode",					typeof(ModeNode));
//			var GlobalAccidental	= new NonTerminal("globalAccidental",	typeof(PitchNode));
#endif
			#endregion 2-Nonterminals

			#region 3-Rules
			Root				= MakePlusList<MusicListAstNode>(MusicLine);

			MusicLine.Rule		= MakePlusList<MusicLineAstNode>(Beam, whiteSpace, true) + (NewLine | Eof)
                                | whiteSpace + NewLine | NewLine;

            Beam.Rule			= MakePlusList<BeamAstNode>(Music);
			Music.Rule			= Direction | Note 
#if KeySpec
								| FieldKeyLine
#endif
								;
			Direction.Rule		= Length | OctaveNo | Shift | Style | Tempo | modePlay | MusicBar;
            MusicBar.Rule       = bar;
			Length.Rule			= length + integer;
			Style.Rule			= mode	+ modeStyle;
			Tempo.Rule			= tempo	+ integer;
			OctaveNo.Rule		= octave + integer;
			Shift.Rule			= shift;

			Note.Rule			= NoteNumber | NoteElement;
			NoteNumber.Rule	    = note + integer;
			NoteElement.Rule	= PitchOrRest
								| PitchOrRest + integer
								| NoteElement + MakePlusList<DotsAstNode>(dot);
			PitchOrRest.Rule	= Pitch | Rest;
			Rest.Rule			= rest;
			Pitch.Rule			= noteLetter
								| noteLetter + sharpFlat;

			Direction.ErrorRule	= SyntaxError + NewLine;
			Note.ErrorRule		= SyntaxError + whiteSpace;

			#region Key Specification
#if KeySpec
			FieldKeyLine.Rule	= FieldK + Key; // + NewLine;
			Key.Rule			= KeySpec | bagpipes;
			KeySpec.Rule		= Pitch 
//								| keyNote + modeSpec 
	;	//						| KeySpec + MakePlusList(GlobalAccidental,typeof(ListNode));
//			keyNote.Rule		= noteLetter | noteLetter + sharpFlat; //KeyAccidental;
//			modeSpec.Rule		= Mode
//								| PreferShiftHere() + whiteSpace + Mode;
//			Mode.Rule			= MakePlusList(Alpha, typeof(AbcList_String));	
										// only specified modes allowed: look-up in list
		//	GlobalAccidental.Rule= PreferShiftHere() + whiteSpace + noteLetter + sharpFlat;
			MarkTransient(FieldKeyLine);
#endif
			#endregion Key Specification
			#endregion 3-Rules

			#region 4-Color Highlighting
			SetHighlighting(
				new List<Irony.Parsing.Terminal>() {modePlay},
				new List<Irony.Parsing.Terminal>() {note},
				new List<Irony.Parsing.Terminal>() {rest},
				new List<Irony.Parsing.Terminal>() {mode, modeStyle},
				new List<Irony.Parsing.Terminal>() {noteLetter,sharpFlat,dot,integer},
				new List<Irony.Parsing.Terminal>() {tempo,length,octave,shift}
				);
			#endregion 7-Color Highlighting
		}

		/// <summary>Whitespace is part of this grammar, so we override the routine that skips it.</summary>
		public override void SkipWhitespace(Irony.Parsing.ISourceStream source) { ; }

		/// <summary>Provide MyGrammar with appropriate AstContext.</summary>
		/// <param name="language"></param>
		/// <returns></returns>
		protected override QabcAstContext GetContext(Irony.Parsing.LanguageData language) {
			return new QabcAstContext(language);
		}
	}
	/// <summary>Invoke <c>TypedNonTerminal&lt;TAstContext,TAstNode&gt;</c> against <c>QabcAstContext</c>.</summary>
	public class TypedNonTerminal<TAstNode> : TypedNonTerminal<QabcAstContext,TAstNode> 
	where TAstNode : AstNode<QabcAstContext>{}
}

