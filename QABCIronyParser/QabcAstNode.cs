////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012
////////////////////////////////////////////////////////////////////////
using System;

using Irony.Parsing;

using PGSoftwareSolutionsInc.PGIrony;
using PGSoftwareSolutionsInc.Music;

namespace PGSoftwareSolutionsInc.Qabc {
    /// <summary>TODO</summary>
	public interface INoteNode {
        /// <summary>TODO</summary>
        AwareNote Note			{ get; }
        /// <summary>TODO</summary>
        NoteLetter NoteLetter	{ get; }
	}

    /// <summary>TODO</summary>
	public class QabcAstNode : AstNode<QabcAstContext>, IAstWorkNode<QabcAstContext> {}

    /// <summary>TODO</summary>
	public class MusicListAstNode	: QabcAstNode {
		public Tune<INote> Tune() { 
			EvaluateTree(Context); 	
			return Context.Tune;	
		}
	}

    #region Note Nodes
    /// <summary>TODO</summary>
    public class NoteAstNode			: QabcAstNode {
        /// <summary>TODO</summary>
        protected override void DoMyWork(QabcAstContext context) {
			context.Tune.Add(((INoteNode)(ChildNodes[0])).Note);
		}
	}
    /// <summary>TODO</summary>
	public class NoteNumberAstNode	: QabcAstNode, INoteNode, IPitch {
		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
			base.Init(context, treeNode);
			Length	= new NoteLength(Notes.Length, 0);
			Style		= Notes.Style;
			Tempo		= Notes.Tempo;
		}
		public  AwareNote   Note        { 
			get { return AwareNote.GetAwareNote(NoteLetter, PianoKey, SharpFlat, Length, Style, Tempo, 0, Span); }
		}
		private	NoteLength	Length		{ get; set; }
		private	Style		Style		{ get; set; }
		private	Int16		Tempo		{ get; set; }
		public	PianoKey	PianoKey	{ get { return (PianoKey)(((IAstValueNode<byte>)ChildNodes[1]).Value); } }
		public	NoteLetter	NoteLetter	{ get { return PianoKey.Letter(); } }
		public	SharpFlat	SharpFlat	{ get { return PianoKey.Accidental(); } }
		public override string AsString	{ get { return string.Format("{0} {1}",PianoKey, Notes.Length); }}
	}
	public class NoteElementAstNode	: QabcAstNode, INoteNode, IPitch {
		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
			base.Init(context, treeNode);
			Style		= Notes.Style;
			_length	= Notes.Length;
			Tempo		= Notes.Tempo;
		}
		public Int16		DotCount	{
			get { return (ChildNodes[0] is NoteElementAstNode && ChildNodes.Count == 2)
							? (Int16) ((DotsAstNode)(ChildNodes[1])).Value
							: (Int16) 0;
			}
		}
		public Int16		LengthDenominator {
			get { return ( !(ChildNodes[0] is NoteElementAstNode)  && ChildNodes.Count == 2)
							? (((IAstValueNode<Byte>)ChildNodes[1]).Value)
							: Notes.Length;
			}
		}
		public NoteLength   Length      { get { 
			if ( ChildNodes.Count == 1)
				return _length;
			else if (ChildNodes[0] is NoteElementAstNode )
				return new NoteLength(((NoteElementAstNode)ChildNodes[0]).Length, DotCount);
			else
				return new NoteLength(((IAstValueNode<Byte>)ChildNodes[1]).Value, 0); } 
		}
		public AwareNote	Note        { 
			get { return AwareNote.GetAwareNote(NoteLetter, PianoKey, SharpFlat, Length, Style, Tempo, 0, Span); }
		}
		private NoteLength _length;
		public Style		Style		{ get; private set; }
		public Int16		Tempo		{ get; private set; }
		public PianoKey	    PianoKey	{ get { return ((IPitch)ChildNodes[0]).PianoKey; } }
		public NoteLetter	NoteLetter	{ get { return ((IPitch)ChildNodes[0]).NoteLetter; } }
		public SharpFlat	SharpFlat	{ get { return ((IPitch)ChildNodes[0]).SharpFlat; } }
		public override string AsString	{ get { return string.Format("{0} {1}", PianoKey,Length); } }
	}
	public class PitchAstNode			: QabcAstNode, IPitch {
		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
			base.Init(context, treeNode);
			Octave	= Notes.Octave;
		}
		public virtual		PianoKey	PianoKey    { 
			get{ return (PianoKey)(Note.KeyNumber(NoteLetter, Octave, SharpFlat)); }
		}
		public virtual		NoteLetter	NoteLetter	{
			get { return ((IAstValueNode<NoteLetter>)(ChildNodes[0])).Value; } 
		}
		public virtual		SharpFlat	SharpFlat	{
			get { return ChildNodes.Count == 2	? ((IAstValueNode<SharpFlat>)ChildNodes[1]).Value 
															: SharpFlat.Natural; }
		}
		private             Int16		Octave	    { get; set; }
	}
    public class MusicBarAstNode : PitchAstNode {
        public override PianoKey   PianoKey     { get { return PianoKey.Bar; } }
        public override NoteLetter NoteLetter   { get { return NoteLetter.C; } }
        public override SharpFlat  SharpFlat    { get { return SharpFlat.Natural; } }
    }
    public class RestAstNode			: PitchAstNode {
		public override	PianoKey	PianoKey	{ get { return PianoKey.Rest; } }
		public override	NoteLetter	NoteLetter	{ get { return NoteLetter.C; } }
		public override	SharpFlat	SharpFlat	{ get { return SharpFlat.Natural; } }
	}
	public class DotsAstNode		: ValueNode<QabcAstContext,byte> {
		public override byte Value { get { return (byte)ChildNodes.Count; } }
	}
	#endregion Note Nodes

	public class BeamAstNode : QabcAstNode {}
    public class MusicLineAstNode : QabcAstNode {}
}
