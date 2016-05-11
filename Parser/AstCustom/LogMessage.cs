#region License - Microsoft Public License - from PG Software Solutions Inc.
/***********************************************************************************
 * This software is copyright © 2012 by PG Software Solutions Inc. and licensed under
 * the Microsoft Public License (http://pgirony.codeplex.com/license).
 * 
 * Author:			Pieter Geerkens
 * Organization:	PG Software Solutions Inc.
 * *********************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Irony;
using Irony.Parsing;

namespace PGSoftwareSolutions.PGIrony {
	/// <summary>Extension of Iorny.LogMessage that provides a SourceSpan, with the token length as well sa position.</summary>
	public class LogMessage : Irony.LogMessage {
	/// <summary>Extension of Iorny.LogMessage that provides a SourceSpan, with the token length as well sa position.</summary>
		 public LogMessage(ErrorLevel level, SourceSpan span, string message, ParserState parserState) 
		 : base(level,span.Location, message, parserState) {
			 Span = span;
		}
		/// <summary>The token location and length in the text source.</summary>
		public SourceSpan Span { get; private set; }
	}
}
