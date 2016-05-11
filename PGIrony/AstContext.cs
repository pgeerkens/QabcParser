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

using Irony.Ast;
using Irony.Parsing;

namespace PGSoftwareSolutionsInc.PGIrony {
	/// <summary>Ensures that the default AST node-types can all be created.</summary>
	public class AstContext : Irony.Ast.AstContext {
	/// <summary>Sets the default Node builders used by BuildAst.</summary>
		public AstContext(LanguageData language)
		: base(language) {
			base.DefaultIdentifierNodeType	= typeof(NullAstNode); //typeof(IdentifierNode);
			base.DefaultLiteralNodeType		= typeof(LiteralValueNode);
			base.DefaultNodeType					= typeof(NullAstNode); //null; 
		}
	}
}
