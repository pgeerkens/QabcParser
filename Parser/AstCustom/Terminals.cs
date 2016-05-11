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

using Irony.Parsing;

namespace PGSoftwareSolutions.PGIrony {
	/// <summary>A typed integer literal with the NumberOptions AllowLetterAfter and IntOnly set automatically.</summary>
	/// <typeparam name="TValue">The type of the literal; one of byte, sbyte, int16, etc.</typeparam>
	public class MusicIntegerLiteral<TValue> : NumberLiteral {
		/// <summary>A typed integer literal with the NumberOptions AllowLetterAfter and IntOnly set automatically.</summary>
		public MusicIntegerLiteral(string name) : this(name, NumberOptions.None) {}
		/// <summary>A typed integer literal with the NumberOptions <i>AllowLetterAfter</i> and <i>IntOnly</i> set automatically.</summary>
		public MusicIntegerLiteral(string name, NumberOptions options) 
		: base(name, options | NumberOptions.AllowLetterAfter | NumberOptions.IntOnly) {
			TypeCode typeCode		= Type.GetTypeCode(GetType().GetGenericArguments()[0].UnderlyingSystemType);
			DefaultIntTypes		= new TypeCode[] {typeCode};
			AstConfig.NodeType	= typeof(LiteralValueNode<TValue>);
		}
	}

	/// <summary> RegexBasedTerminal sub-class that parses the constants of enumeration <i>TValue</i> 
	/// in determining the Value of the Terminal.
	/// </summary>
	/// <typeparam name="TValue">Must be an <i>Enum</i> type, but only enforced at run-time
	/// as a constraint like <i>TValue : System.Enum</i> is forbidden.</typeparam>
	public class RegexEnumTerm<TValue> : RegexBasedTerminalX where TValue : struct, IComparable, IFormattable, IConvertible {
//	public class RegexEnumTerm<TValue> : RegexBasedTerminalX where TValue : UnconstrainedMelody.IEnumConstraint {
		/// <summary>RegexBasedTerminal sub-class that parses the constants of Enum <c>TValue</c>.</summary>
		/// <param name="pattern">A regular-expression pattern for this enumeration.</param>
		/// <exception cref="ArgumentException">ArgumentException</exception>
		public RegexEnumTerm(string pattern, Func<string,TValue> fromString) : this(pattern,(c,s) => fromString(s)) {}
		/// <summary>RegexBasedTerminal subclass that parses the constants of Enum <c>TValue</c>.</summary>
		/// <param name="pattern">A regular-expression pattern for this enumeration.</param>
		/// <param name="fromString">The parsing function to be used; defaults to: 
		/// <c>(c,s) => (TValue)(System.Enum.Parse(EnumType, s.ToUpper()))</c>.</param>
		/// <exception cref="ArgumentException">If TValue is not an enumeration</exception>
		public RegexEnumTerm(string pattern, Func<ParsingContext,string,TValue> fromString = null) : base(pattern) {
			EnumType					= GetType().GetGenericArguments()[0].UnderlyingSystemType;
			if (!EnumType.IsEnum) throw new ArgumentException("Type parameter must be an enumeration.","TValue");
			Name						= EnumType.Name;
			ConvertValue			= fromString ?? ((c,s) => (TValue)(System.Enum.Parse(EnumType, s.ToUpper())));
			AstConfig.NodeType	= typeof(LiteralValueNode<TValue>);
		}
		/// <summary>The <c>Type</c> of the enumeration that this Terminal parses.</summary>
		public Type						EnumType		{ get; private set; }
		/// <summary>The parsing function, returning an enumeration constant from Enum <c>TValue</c>.</returns>
		public Func<ParsingContext,string,TValue> ConvertValue { get; private set; }
		/// <summary>Returns a <c>Token</c> if a successful match made; else null.</summary>
		/// <param name="context"></param>
		/// <param name="source"></param>
		public override Token TryMatch(ParsingContext context, ISourceStream source) {
			Token token = base.TryMatch(context, source);
			if (token != null)
				try {	token.Value = ConvertValue(context,token.ValueString);}
				catch (ArgumentNullException) { token = null; }
				catch (ArgumentException)		{ token = null; }
				catch (OverflowException)		{ token = null; }
			return token;
		}
	}

	/// <summary>A <i>KeyTerm</i>(inal) implementation that automatically sets <i>AllowAlphaAfterKeyword</i>.</summary>
	public class MyKeyTerm : KeyTerm {
		/// <summary>A <i>KeyTerm</i>(inal) implementation that automatically sets <i>AllowAlphaAfterKeyword</i>.</summary>
		public MyKeyTerm(string text, string name, bool allowAlphaAfterKeyword = true) : base(text,name) { 
			AllowAlphaAfterKeyword = allowAlphaAfterKeyword; 
		}
	}
}
