////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012
////////////////////////////////////////////////////////////////////////
//#define KeySpec
using System;
using System.Collections.Generic;
using Irony.Parsing;

using PGSoftwareSolutionsInc.PGIrony;
using PGSoftwareSolutionsInc.Music;

namespace PGSoftwareSolutionsInc.Qabc {
    /// <summary>TODO</summary>
    [Irony.Parsing.Language("QABC", "1.0", "QBasic-ABC Parser")]
    public partial class QabcGrammar : PGIrony.Grammar<QabcAstContext> {
        private enum Dots { zero, one, two, three }

        /// <summary>TODO</summary>
        public QabcGrammar() : base(false) {
            #region 1-Terminals
            #region Directions
            var Bar             = new RegexBasedTerminalX("Bar", @":?[|][1-9:\]]?");
            var Mode            = new MyKeyTerm("M", "Mode");
            var ModeStyle       = new RegexEnumTerm<Style>(@"[NLS]");
            var Length          = new MyKeyTerm("L", "Length");
            var Octave          = new MyKeyTerm("O", "Octave");
            var Tempo           = new MyKeyTerm("T", "Tempo");
            var Integer         = new MusicIntegerLiteral<Byte>("Integer");
            var Shift           = new RegexEnumTerm<OctaveShift>(@"O?[<>]",
                                            s => Music.OctaveShift.Up.FromString(s));
            var ModePlay        = new RegexBasedTerminalX("ModePlay", @"M[BF]");
            #endregion Directions

            #region Notes
            var Note            = new MyKeyTerm("N", "Note");
            var Rest            = new MyKeyTerm("P", "Rest");
            var NoteLetter      = new RegexEnumTerm<NoteLetter>(@"[CDEFGABcdefgab]",
                                            s => Music.NoteLetter.C.Parse(s));
            var Dot             = ToTerm(".");
            var SharpFlat       = new RegexEnumTerm<SharpFlat>(@"[-#+]",
                                            s => Music.SharpFlat.Natural.FromString(s));
            var Bagpipes        = new RegexBasedTerminalX("Bagpipes", @"H[pP]");
            #endregion Notes

            var WhiteSpace      = new RegexBasedTerminalX("WhiteSpace", @"[ \t]+");
            #endregion 1-Terminals

            MarkPunctuation(ModePlay);        // obsolete instruction - recognize & discard
            MarkPunctuation(NewLine, WhiteSpace, Length, Mode, Octave, Tempo);

            #region 2-Nonterminals
            var tunes           = new TransientNonTerminal("tunes");
            var tune            = new TypedNonTerminal<MusicListAstNode>();

            var musicLine       = new TransientNonTerminal("musicLine");
            var musicBar        = new TypedNonTerminal<MusicBarAstNode>();
            var beam            = new TypedNonTerminal<BeamAstNode>();

            var music           = new TransientNonTerminal("music");
            var direction       = new TransientNonTerminal("direction");

            var tempo           = new CommandNonTerminal<Notes, byte>(Notes => Notes.Tempo);
            var length          = new CommandNonTerminal<Notes, byte>(Notes => Notes.Length);
            var style           = new CommandNonTerminal<Notes, Style>(Notes => Notes.Style);
            var octave          = new CommandNonTerminal<Notes, byte>(Notes => Notes.Octave);
            var shift           = new CommandNonTerminal<Notes, OctaveShift>(Notes => Notes.Shift);

            var note            = new TypedNonTerminal<NoteAstNode>();
            var rest            = new TypedNonTerminal<RestAstNode>();
            var pitch           = new TypedNonTerminal<PitchAstNode>();
            var noteNumber      = new TypedNonTerminal<NoteNumberAstNode>();
            var noteElement     = new TypedNonTerminal<NoteElementAstNode>();
            var pitchOrRest     = new TransientNonTerminal("pitchOrRest");

#if KeySpec
            var FieldK           = new MyKeyTerm(@"K:", "FieldK");
            MarkPunctuation(FieldK);

//            var Alpha            = new NonTerminal("alpha",               AlphaUpper | AlphaLower);

            var FieldKeyLine     = new NonTerminal("fieldKeyLine",        typeof(FieldKeyNode));
            var Key              = new NonTerminal("key",                 typeof(KeyNode));
            var KeySpec          = new NonTerminal("keySpec",             typeof(PitchNode));
//            var keyNote          = new NonTerminal("keyNote",             typeof(KeyNoteNode));
//            var modeSpec         = new NonTerminal("modeSpec",            typeof(ModeSpecNode));
//            var Mode             = new NonTerminal("mode",                typeof(ModeNode));
//            var GlobalAccidental = new NonTerminal("globalAccidental",    typeof(PitchNode));
#endif
            #endregion 2-Nonterminals

            #region 3-Rules
            Root                = tunes;

            tunes.Rule          = MakePlusList<MusicListAstNode>(tune, NewLine, true)
                                + Eof;

            tune.Rule           = MakePlusList<QabcAstNode>(musicLine);

            musicLine.Rule      = MakePlusList<QabcAstNode>(musicBar)
                                | WhiteSpace + musicLine;

            musicBar.Rule       = MakePlusList<MusicBarAstNode>(music) + (Bar|NewLine);

            music.Rule          = direction
                                | beam
                                | music + WhiteSpace
#if KeySpec
                                | FieldKeyLine
#endif
                                ;
            direction.Rule      = length | octave | style | tempo | shift | ModePlay;
            length.Rule         = Length + Integer;
            style.Rule          = Mode   + ModeStyle;
            tempo.Rule          = Tempo  + Integer;
            octave.Rule         = Octave + Integer;
            shift.Rule          = Shift;

            beam.Rule           = MakePlusList<BeamAstNode>(note);

            note.Rule           = noteNumber | noteElement;
            noteNumber.Rule     = Note + Integer;
            noteElement.Rule    = pitchOrRest
                                | pitchOrRest + Integer
                                | noteElement + MakePlusList<DotsAstNode>(Dot);
            pitchOrRest.Rule    = pitch | rest;
            rest.Rule           = Rest;
            pitch.Rule          = NoteLetter
                                | NoteLetter + SharpFlat;

            tunes.ErrorRule     = SyntaxError + Eof;
            musicLine.ErrorRule = SyntaxError + NewLine;
            music.ErrorRule     = SyntaxError + WhiteSpace;

            #region Key Specification
#if KeySpec
            FieldKeyLine.Rule   = FieldK + Key; // + NewLine;
            Key.Rule            = KeySpec | bagpipes;
            KeySpec.Rule        = Pitch 
//                                | keyNote + modeSpec 
    ;    //                        | KeySpec + MakePlusList(GlobalAccidental,typeof(ListNode));
//            keyNote.Rule        = noteLetter | noteLetter + sharpFlat; //KeyAccidental;
//            modeSpec.Rule       = Mode
//                                | PreferShiftHere() + whiteSpace + Mode;
//            Mode.Rule           = MakePlusList(Alpha, typeof(AbcList_String));    
                                        // only specified modes allowed: look-up in list
        //    GlobalAccidental.Rule= PreferShiftHere() + whiteSpace + noteLetter + sharpFlat;
            MarkTransient(FieldKeyLine);
#endif
            #endregion Key Specification
            #endregion 3-Rules

            #region 4-Color Highlighting
            SetHighlighting(
                new List<Irony.Parsing.Terminal>() {ModePlay},
                new List<Irony.Parsing.Terminal>() {Note},
                new List<Irony.Parsing.Terminal>() {Rest},
                new List<Irony.Parsing.Terminal>() {Mode, ModeStyle},
                new List<Irony.Parsing.Terminal>() {NoteLetter,SharpFlat,Dot,Integer},
                new List<Irony.Parsing.Terminal>() {Tempo,Length,Octave,Shift}
                );
            #endregion 4-Color Highlighting

            #region Clear shift-reduce conflicts
            #if !ClearConflict
            RegisterOperators(10, Associativity.Right, Length);
            RegisterOperators(10, Associativity.Right, Mode);
            RegisterOperators(10, Associativity.Right, Tempo);
            RegisterOperators(10, Associativity.Right, Octave);
            RegisterOperators(10, Associativity.Right, Shift);
            RegisterOperators(10, Associativity.Right, ModePlay);

            RegisterOperators(10, Associativity.Right, Note);
            RegisterOperators(10, Associativity.Right, NoteLetter);
            RegisterOperators(10, Associativity.Right, Rest);
            #endif
            #endregion

            LanguageFlags = LanguageFlags.CreateAst | LanguageFlags.NewLineBeforeEOF;
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
