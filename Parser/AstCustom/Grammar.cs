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

using Irony.Ast;
using Irony.Parsing;

namespace PGSoftwareSolutions.PGIrony {
	/// <summary>A subclass of Irony.Grammar providien additional boilerplate to ease construction of
	/// AST's and List NonTerminals.</summary>
	/// <typeparam name="TContext">The AstContext to be used in construction and walking of the AST tree.</typeparam>
	public abstract class Grammar<TContext> : Grammar, ICanRunSample 
	where TContext : AstContext  {
		/// <summary>A subclass of Irony.Grammar providien additional boilerplate to ease construction of
		/// AST's and List NonTerminals.</summary>
		public Grammar(bool caseSensitive=true, LanguageFlags flags=LanguageFlags.CreateAst) 
			: base (caseSensitive){
			LanguageFlags |= flags;
		}

		// Sub-class overrides 'GetContext' below to instantiate an AST using TContext 
		// instead of a vanilla AstContext.
		public override void BuildAst(LanguageData language, ParseTree parseTree) {
			if (LanguageFlags.IsSet(LanguageFlags.CreateAst)) {
				(new AstBuilder(GetContext(language))).BuildAst(parseTree);
			}
		}
		/// <summary> Get sub-class' appropriate AstContext.</summary>
		protected abstract TContext GetContext(LanguageData language);
		
		/// <summary>Override to enable use of Test.Run button in Grammar Explorer.</summary>
		/// <param name="args"></param>
		/// <returns>Message to be displayed in <i>Output</i> window.</returns>
		public virtual string RunSample(RunSampleArgs args){ return "Not supported yet for this grammar.";	}

		/// <summary>Sets default colour-highlighting for six lists of <c>Terminals</c>.</summary>
		/// <param name="comments"></param>
		/// <param name="texts"></param>
		/// <param name="literals"></param>
		/// <param name="keywords"></param>
		/// <param name="identifiers"></param>
		/// <param name="strings"></param>
		protected virtual void SetHighlighting(
			IList<Terminal> comments,	IList<Terminal> texts,			IList<Terminal> literals,
			IList<Terminal> keywords,	IList<Terminal> identifiers,	IList<Terminal> strings)
	{
		foreach (Terminal terminal in comments) 
			{ terminal.EditorInfo = new TokenEditorInfo(TokenType.Comment,TokenColor.Comment,TokenTriggers.None); }
		foreach (Terminal terminal in texts) 
			{ terminal.EditorInfo = new TokenEditorInfo(TokenType.Text,TokenColor.Text,TokenTriggers.None); }
		foreach (Terminal terminal in literals) 
			{ terminal.EditorInfo = new TokenEditorInfo(TokenType.Literal,TokenColor.Number,TokenTriggers.None); }
		foreach (Terminal terminal in keywords) 
			{ terminal.EditorInfo = new TokenEditorInfo(TokenType.Keyword,TokenColor.Keyword,TokenTriggers.None); }
		foreach (Terminal terminal in identifiers) 
			{ terminal.EditorInfo = new TokenEditorInfo(TokenType.Identifier,TokenColor.Identifier,TokenTriggers.None); }
		foreach (Terminal terminal in strings) 
			{ terminal.EditorInfo = new TokenEditorInfo(TokenType.String,TokenColor.String,TokenTriggers.None); }
	}

		#region Changes for StarList & PlusList
		/// <summary>As MakeStarRule, except returns a constructed StarList NonTerminal.</summary>
		protected virtual NonTerminal MakeStarList<TNodeType>(BnfTerm listMember, BnfTerm delimiter=null, 
			bool AllowTrailingDelimiter=false) 
		where TNodeType : AstNode<TContext> {
			var options = TermListOptions.StarList;
			if (AllowTrailingDelimiter) options |= TermListOptions.AllowTrailingDelimiter ;
			return MakeList(new NonTerminal(listMember.Name + "*", typeof(TNodeType)),
											delimiter, listMember, options);
		}

		/// <summary>As MakePlusRule, except returns a constructed PlusList NonTerminal.</summary>
		protected virtual NonTerminal MakePlusList<TNodeType>(BnfTerm listMember, BnfTerm delimiter=null, 
			bool AllowTrailingDelimiter=false) 
		where TNodeType : AstNode<TContext> {
			var options = TermListOptions.PlusList;
			if (AllowTrailingDelimiter) options |= TermListOptions.AllowTrailingDelimiter ;
			return MakeList(new NonTerminal(listMember.Name + "+", typeof(TNodeType)),
											delimiter, listMember, options);
		}

		/// <summary>Preserves default behaviour of base class with a shared codebase.</summary>
		/// <returns>Returns the List-Rule constructed for the supplied List.</returns>
		protected new BnfExpression MakeListRule(NonTerminal list, BnfTerm delimiter, BnfTerm listMember, 
			TermListOptions options = TermListOptions.PlusList) {
			return MakeList(list, delimiter, listMember, options).Rule;
		}
		/// <summary>Implementation posted by Roman 2012-09-23, except returns <i>list</i> instead of <i>list.Rule</i>.</summary>
		/// <param name="list">A <code>NonTerminal</code> designating the <i>List</i> to be constructed.</param>
		/// <param name="delimiter">A <code>BnfTerm</code> for the list delimiter; specify null if not a delimited list, and specify 
		/// <code>TermOptions.AllowTrailingDelimiter</code> as an option as required.</param>
		/// <param name="listMember">A <code>BnfTerm</code> for the prescribed members of the list.</param>
		/// <param name="options">The <code>TermListOptions</code> desired for the list.</param>
		/// <returns>A <code>NonTerminal</code> for the list.</returns>
 		protected virtual NonTerminal MakeList(NonTerminal list, BnfTerm delimiter, BnfTerm listMember, TermListOptions options) {
			///If it is a star-list (allows empty), then we first build plus-list
			var isPlusList = !options.IsSet(TermListOptions.AllowEmpty);
			var allowTrailingDelim = options.IsSet(TermListOptions.AllowTrailingDelimiter) & delimiter != null;
			NonTerminal plusList = isPlusList ? list : new NonTerminal(listMember.Name + "+");
			//"list" is the real list for which we will construct expression - it is either extra plus-list or original listNonTerminal. 
			// In the latter case we will use it later to construct expression for listNonTerminal
			plusList.SetFlag(TermFlags.IsList);
			plusList.Rule = plusList;  // rule => list
			if (delimiter != null)
				plusList.Rule += delimiter;  // rule => list + delim
			if (options.IsSet(TermListOptions.AddPreferShiftHint))
				plusList.Rule += PreferShiftHere(); // rule => list + delim + PreferShiftHere()
			plusList.Rule += listMember;          // rule => list + delim + PreferShiftHere() + elem
			plusList.Rule |= listMember;        // rule => list + delim + PreferShiftHere() + elem | elem
			if (isPlusList) {
				// if we build plus list - we're almost done; plusList == list
				// add trailing delimiter if necessary; for star list we'll add it to final expression
				if (allowTrailingDelim)
					plusList.Rule |= list + delimiter; // rule => list + delim + PreferShiftHere() + elem | elem | list + delim
			} else {
				// Setup list.Rule using plus-list we just created
				list.Rule = Empty | plusList;
				if (allowTrailingDelim)
					list.Rule |= plusList + delimiter | delimiter;
				plusList.SetFlag(TermFlags.NoAstNode);
				list.SetFlag(TermFlags.IsListContainer); //indicates that real list is one level lower
			} 
			return list; 
		}//method
	#endregion
	}
}
