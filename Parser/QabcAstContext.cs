////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016-2016
////////////////////////////////////////////////////////////////////////
using System;

using Irony.Ast;
using Irony.Parsing;

using PGSoftwareSolutions.Music;

namespace PGSoftwareSolutions.Qabc {
    /// <summary>TODO</summary>
	public class QabcAstContext : PGIrony.AstContext {
        /// <summary>TODO</summary>
        public QabcAstContext(LanguageData language) : base(language) { 
			DefaultNodeType = typeof(QabcAstNode); 
			Tune = new Tune<INote>(); 
		}
        /// <summary>TODO</summary>
        public Tune<INote> Tune	 { get; protected set; }

        /// <summary>TODO</summary>
        public static AstContext NewInstance(LanguageData language) {
			return new QabcAstContext(language); 
		}
	}
}
