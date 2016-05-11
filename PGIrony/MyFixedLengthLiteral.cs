#region License
////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012
////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {

  //A terminal for representing fixed-length lexemes coming up sometimes in programming language
  // (in Fortran for ex, every line starts with 5-char label, followed by a single continuation char)
  // It may be also used to create grammar/parser for reading data files with fixed length fields
  public class MyFixedLengthLiteral<T> : MyDataLiteralBase<T> {
    public int Length { get; private set; }

    public MyFixedLengthLiteral(string name, int length) : base(name) {
      Length = length;
    }

    protected override string ReadBody(ParsingContext context, ISourceStream source) {
      source.PreviewPosition = source.Location.Position + Length;
      var body = source.Text.Substring(source.Location.Position, Length);
      return body; 
    }
  }//class
}//namespace
