////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016
////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

using Irony;
using Irony.Parsing;

using PGSoftwareSolutions.Music;

namespace PGSoftwareSolutions.Qabc {
    /// <summary>TODO</summary>
	public class QabcIronyParser : Parser, IMusicParser<Tune<INote>> {
        /// <summary>TODO</summary>
        public static QabcIronyParser Instance { get { return _first; } } static QabcIronyParser _first = new QabcIronyParser();

        /// <summary>TODO</summary>
        public QabcIronyParser() : base(new QabcGrammar()) {}
        /// <summary>TODO</summary>
        public Tune<INote> Parse(TextReader reader) { return Parse(reader.ReadToEnd()); }
        /// <summary>TODO</summary>
        public new	Tune<INote> Parse(string music){
			ParseTree	_parseTree;
			if (GCFirst) {	_parseTree	= null;	GC.Collect(); }

			Parse(music, "<music>");
			_parseTree = Context.CurrentParseTree;
			Errors = _parseTree.ParserMessages;

			if (_parseTree == null  || _parseTree.Root == null  ||  _parseTree.Root.AstNode == null)
				return null;
			else if (Errors.Count == 0)
				return (Tune<INote>) ((MusicListAstNode)_parseTree.Root.AstNode).Tune();
			else 
				return null;
		}
		/// <summary>Force Garbage Collection prior to parse attempt.</summary>
		public bool					GCFirst	{ get; set; }
		/// <summary>Collection of parsing errors.</summary>
		public LogMessageList	Errors	{ get; private set; }
	}
}
