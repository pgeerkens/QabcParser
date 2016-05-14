////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016-2016
////////////////////////////////////////////////////////////////////////
using System;

using Irony.Parsing;

namespace PGSoftwareSolutions.Qabc {
	class KeySpec {
	}
	public class FieldKeyNode			: QabcAstNode { }
	public class KeyNode					: QabcAstNode {
		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
			base.Init(context, treeNode);
		}
	}
	public class KeySpecNode			: QabcAstNode {
		public virtual	 string BaseNote		{ get; protected set; }
		public virtual	 string Accidental	{ get; protected set; }
		public virtual	 string Mode			{ get; protected set; }
		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
			base.Init(context, treeNode);
		}
	}
#if fred
//	public class KeyNoteNode			: QabcAstNode {
//		public virtual	 string BaseNote		{ get; protected set; }
//		public virtual	 string Accidental	{ get; protected set; }
//		public override string	Suffix		{ get { return BaseNote + Accidental; } } 
//		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
//			base.Init(context, treeNode);
//			BaseNote = Children[0].Token.Text;
//			if (Children.Count > 1)
//				Accidental = Children[1].Token.Text;
//			else 
//				Accidental = String.Empty;
//		}
//	}
//	public class ModeSpecNode			: QabcAstNode {
//		public virtual		string	Value		{ get; protected set; }
//		public override	string	Suffix	{ get { return Value.ToString(); } } 
//		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
//			base.Init(context, treeNode);
//			Value = ((ModeNode)Children[Children.Count-1].AstNode).Value;
//		}
//	}
//	public class ModeNode				: QabcAstNode {
//		public virtual		string	Value		{ get; protected set; }
//		public override	string	Suffix	{ get { return Value.ToString(); } } 
//		public override void Init(QabcAstContext context, ParseTreeNode treeNode) {
//			base.Init(context, treeNode);
//			Value = ((AbcList_String)Children[Children.Count-1].AstNode).Value;
//		}
//	}
//	public class GlobalAccidentalNode	: QabcAstNode { }
#endif
}
