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

using Irony.Parsing;

namespace PGSoftwareSolutionsInc.PGIrony {

	//DataLiteralBase is a base class for a set of specialized terminals with a primary purpose of building data readers
	// DsvLiteral is used for reading delimiter-separated values (DSV), comma-separated format is a specific case of DSV
	// FixedLengthLiteral may be used to read values of fixed length
	public abstract class DataLiteralBase<TValue> : Terminal where TValue : struct {
    //For date format strings see MSDN help for "Custom format strings", available through help for DateTime.ParseExact(...) method
		public string DateTimeFormat	= "d";	//standard format, identifies MM/dd/yyyy for invariant culture.
		public int IntRadix				= 10;		//Radix (base) for numeric numbers

		public Type			DataType { get { return GetType().GetGenericArguments()[0].GetType(); } }
		private TypeCode	TypeCode	{ get { return Type.GetTypeCode(DataType); } }
		public DataLiteralBase(string name) : base(name) {
			Parser = GetParser(TypeCode);
		}
		public DataLiteralBase(string name, Func<ParsingContext,string,TValue> parser) : base(name) {
			Parser = (c,v) => (object)(parser(c,v));
		}

		private Func<ParsingContext,string,object> GetParser(TypeCode typeCode) {
			switch(typeCode) {
				case TypeCode.String:	return (c,v) => v;
				case TypeCode.DateTime:	return (c,v) => DateTime.ParseExact(v, DateTimeFormat, c.Culture);
				case TypeCode.Single:	return (c,v) => Convert.ToSingle(v, c.Culture); 
				case TypeCode.Double:	return (c,v) => Convert.ToDouble(v, c.Culture);
				case TypeCode.Int64:		return (c,v) => (IntRadix == 10) ? Convert.ToInt64(v, c.Culture) 
																							: Convert.ToInt64(v, IntRadix);
				default:						return (c,v) => (IntRadix == 10) ? Convert.ToInt64(v, c.Culture) 
																							: Convert.ToInt64(v, IntRadix);
			}
		}

		public override Token TryMatch(ParsingContext context, ISourceStream source) {
			try {
				var textValue = ReadBody(context, source);
				if (textValue == null) return null; 
				var value = ConvertValue(context, textValue);
				return source.CreateToken(this.OutputTerminal, value);
			} catch(Exception ex) {
				//we throw exception in DsvLiteral when we cannot find a closing quote for quoted value
				return context.CreateErrorToken(ex.Message);
			}
		}//method

		protected abstract string ReadBody(ParsingContext context, ISourceStream source);

		protected Func<ParsingContext,string,object> Parser { get; set; }

		protected virtual TValue ConvertValue(ParsingContext context, string textValue) {
			return (TValue) Convert.ChangeType(Parser(context,textValue), typeof(TValue), context.Culture);
		}
	}//class
}//namespace
