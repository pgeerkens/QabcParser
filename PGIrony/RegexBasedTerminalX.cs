#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Irony;
using Irony.Parsing;

namespace PGSoftwareSolutionsInc.PGIrony {

    /// <summary>Identical to <see cref="Irony.Parsing.RegexBasedTerminal"/> (as of 2012-09-29) except options 
    /// Compiled and ExplicitCapture are automatically set.</summary>
    public class RegexBasedTerminalX : Terminal {
        /// <summary>Identical to <see cref="Irony.Parsing.RegexBasedTerminal"/> (as of 2012-09-29) except options 
        /// Compiled and ExplicitCapture are automatically set.</summary>
        public RegexBasedTerminalX(string pattern, params string[] prefixes) : base("name") {
      Pattern = pattern;
      if (prefixes != null)
        Prefixes.AddRange(prefixes);
		Count++;
    }
        /// <summary>Identical to <see cref="Irony.Parsing.RegexBasedTerminal"/> (as of 2012-09-29) except options 
        /// Compiled and ExplicitCapture are automatically set.</summary>
        public RegexBasedTerminalX(string name, string pattern, params string[] prefixes) : base(name) {
      Pattern = pattern;
      if (prefixes != null)
        Prefixes.AddRange(prefixes);
		Count++;
    }

	 public static int Count { get; private set; }
    #region public properties
    public readonly string Pattern;
    public readonly StringList Prefixes = new StringList();

    public Regex Expression {
      get { return _expression; }
    } Regex  _expression;
    #endregion

    public override void Init(GrammarData grammarData) {
      base.Init(grammarData);
      string workPattern = @"\G(" + Pattern + ")";
      RegexOptions options = (Grammar.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)
			// 2012-11-08 - P.Geerkens - added these two options for better performance & safety.
									| RegexOptions.Compiled | RegexOptions.ExplicitCapture;
      _expression = new Regex(workPattern, options);
      if (this.EditorInfo == null) 
        this.EditorInfo = new TokenEditorInfo(TokenType.Unknown, TokenColor.Text, TokenTriggers.None);
    }

    public override IList<string> GetFirsts() {
      return Prefixes;
    }

    public override Token TryMatch(ParsingContext context, ISourceStream source) {
      Match m = _expression.Match(source.Text, source.PreviewPosition);
      if (!m.Success || m.Index != source.PreviewPosition) 
        return null;
      source.PreviewPosition += m.Length;
      return source.CreateToken(this.OutputTerminal); 
    }
  }//class
}//namespace
