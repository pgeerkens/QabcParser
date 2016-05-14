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
	public class RegexEnumTerm<TValue> : RegexTerm<TValue> where TValue : struct {
        //	public class RegexEnumTerm<TValue> : RegexBasedTerminalX where TValue : UnconstrainedMelody.IEnumConstraint {
        /// <summary>RegexBasedTerminal sub-class that parses the constants of Enum <c>TValue</c>.</summary>
        /// <param name="pattern">A regular-expression pattern for this enumeration.</param>
        /// <param name="fromString">The parsing function to be used; defaults to: 
		/// <c>(c,s) => (TValue)(System.Enum.Parse(EnumType, s.ToUpper()))</c>.</param>
        /// <exception cref="ArgumentException">ArgumentException</exception>
        public RegexEnumTerm(string pattern, Func<string,TValue> fromString) : this(pattern,(c,s) => fromString(s)) {}
		/// <summary>RegexBasedTerminal subclass that parses the constants of Enum <c>TValue</c>.</summary>
		/// <param name="pattern">A regular-expression pattern for this enumeration.</param>
		/// <param name="fromString">The parsing function to be used; defaults to: 
		/// <c>(c,s) => (TValue)(System.Enum.Parse(EnumType, s.ToUpper()))</c>.</param>
		/// <exception cref="ArgumentException">If TValue is not an enumeration</exception>
		public RegexEnumTerm(string pattern, Func<ParsingContext,string,TValue> fromString) : base(pattern, fromString) {
			_enumType = GetType().GetGenericArguments()[0].UnderlyingSystemType;
			if (!_enumType.IsEnum) throw new ArgumentException("Type parameter must be an enumeration.","TValue");
		}
        readonly Type _enumType;
	}

    /// <summary> RegexBasedTerminal sub-class that parses the constants of enumeration <i>TValue</i> 
    /// in determining the Value of the Terminal.
    /// </summary>
    /// <typeparam name="TValue">Must be an <i>Enum</i> type, but only enforced at run-time
    /// as a constraint like <i>TValue : System.Enum</i> is forbidden.</typeparam>
    public class RegexTerm<TValue> : RegexBasedTerminalX {
        /// <summary>RegexBasedTerminal subclass that parses the constants of Enum <c>TValue</c>.</summary>
        /// <param name="pattern">A regular-expression pattern for this enumeration.</param>
        /// <param name="fromString">The parsing function to be used; defaults to: 
        /// <c>(c,s) => (TValue)(System.Enum.Parse(EnumType, s.ToUpper()))</c>.</param>
        /// <exception cref="ArgumentException">If TValue is not an enumeration</exception>
        public RegexTerm(string pattern, Func<ParsingContext, string, TValue> fromString) : base(pattern) {
            Name               = GetType().GetGenericArguments()[0].UnderlyingSystemType.Name;
            _parser            = fromString;
            AstConfig.NodeType = typeof(LiteralValueNode<TValue>);
        }
        /// <summary>The parsing function, returning an enumeration constant from Enum <c>TValue</c>.</returns>
        private Func<ParsingContext, string, TValue> _parser { get; set; }
        /// <summary>Returns a <c>Token</c> if a successful match made; else null.</summary>
        /// <param name="context"></param>
        /// <param name="source"></param>
        public override Token TryMatch(ParsingContext context, ISourceStream source) {
            Token token = base.TryMatch(context, source);
            if (token != null)
                try { token.Value = _parser(context, token.ValueString); }
                catch (ArgumentNullException) { token = null; }
                catch (ArgumentException) { token = null; }
                catch (OverflowException) { token = null; }
            return token;
        }
    }

    /// <summary>A <i>KeyTerm</i>(inal) implementation that automatically sets <i>AllowAlphaAfterKeyword</i>.</summary>
    public class KeyTermX : KeyTerm {
		/// <summary>A <i>KeyTerm</i>(inal) implementation that automatically sets <i>AllowAlphaAfterKeyword</i>.</summary>
		public KeyTermX(string text, string name, bool allowAlphaAfterKeyword = true) : base(text,name) { 
			AllowAlphaAfterKeyword = allowAlphaAfterKeyword; 
		}
	}
}
