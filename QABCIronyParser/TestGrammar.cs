using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Irony;
using Irony.Parsing;
using PGSoftwareSolutionsInc.MyIrony;

namespace Irony.Samples {
	[Language("Test", "0.0", "Testing only Parser")]
   public class TestGrammar :  Grammar<AstContext>, ICanRunSample
   {
      public TestGrammar()
      {
            GrammarComments = "Slick scripts interpreter.";

				#region Terminals
				var Number = new NumberLiteral("Number");
            Number.DefaultIntTypes = new TypeCode[] { TypeCode.Int32 };            
            Number.Options |= NumberOptions.IntOnly;

            var CharLiteral = new StringLiteral("Char", "'", StringOptions.AllowsAllEscapes);
            var StringLiteral = new StringLiteral("String", "\"", StringOptions.AllowsAllEscapes);
            var Identifier = new IdentifierTerminal("Identifier", "_-", "");
            var TextLiteral = new RegexBasedTerminal("identifier", @"[a-zA-Z_][a-zA-Z_0-9]*");

            var LineComment = new CommentTerminal("LineComment", "//", "\n", "\r");            
            NonGrammarTerminals.Add(LineComment);

            var BlockComment = new CommentTerminal("BlockComment", "/*", "*/");            
            NonGrammarTerminals.Add(BlockComment);
				#endregion

				#region Keywords
            var TrueKeyword = ToTerm("true", "true");
            var FalseKeyword = ToTerm("false", "false");
            var comma = ToTerm(",");
				#endregion

				#region Non terminals
            //Simple
            var SimpleValue = new NonTerminal("SimpleValue"); // , typeof(SimpleValue));SimpleValue));
            var StringValue = new NonTerminal("StringValue"); // , typeof(SimpleValue));StringValue));
            var NumberValue = new NonTerminal("NumberValue"); // , typeof(SimpleValue));NumberValue));
            var BoolValue = new NonTerminal("BoolValue"); // , typeof(SimpleValue));BoolValue));

            //Complex
            var Program = new NonTerminal("Program"); // , typeof(SimpleValue));Program));
            var Function = new NonTerminal("Function"); // , typeof(SimpleValue));Function));
            var FunctionArgs = new NonTerminal("FunctionArgs"); // , typeof(SimpleValue));FunctionArgs));
            var FunctionArg = new NonTerminal("FunctionArg"); // , typeof(SimpleValue));FunctionArg));
            var FunctionBody = new NonTerminal("FunctionBody"); // , typeof(SimpleValue));FunctionBody));
            var Expressions = new NonTerminal("Expressions"); // , typeof(SimpleValue));Expressions));
            var Expression = new NonTerminal("Expression"); // , typeof(SimpleValue));Expression));
            var FunctionCall = new NonTerminal("FunctionCall"); // , typeof(SimpleValue));FunctionCall));
            var FunctionCallArgs = new NonTerminal("FunctionCallArgs"); // , typeof(SimpleValue));FunctionCallArgs));
            var FunctionCallArg = new NonTerminal("FunctionCallArg"); // , typeof(SimpleValue));FunctionCallArg));
            var FunctionCallAgValue = new NonTerminal("FunctionCallArgValue"); // , typeof(SimpleValue));FunctionCallAgValue));
            var InlineFunction = new NonTerminal("InlineFunction"); // , typeof(SimpleValue));InlineFunction));
            var DefaultFunctionArg = new NonTerminal("DefaultFunctionArg"); // , typeof(SimpleValue));DefaultFunctionArg));
            var ValueExpression = new NonTerminal("ValueExpression"); // , typeof(SimpleValue));ValueExpression));
				#endregion

				#region Rules
            //Simple
            StringValue.Rule =              StringLiteral;
            NumberValue.Rule =              Number;
            BoolValue.Rule =                TrueKeyword | FalseKeyword;
            SimpleValue.Rule =              StringValue | NumberValue | BoolValue;

            //Complex
            Program.Rule =                  Function;
            Function.Rule =                 Identifier + "(" + FunctionArgs + ")" + FunctionBody;
            FunctionArgs.Rule =             MakeStarRule(FunctionArgs, comma, FunctionArg);
            FunctionArg.Rule =              DefaultFunctionArg | Identifier;
            DefaultFunctionArg.Rule =       Identifier + "=" + SimpleValue;
            FunctionBody.Rule =             "{" + Expressions + "}";
			
				//These are the "troublesome rules" that I'm having problems with
				Expressions.Rule =              MakeStarRule(Expressions, Expression);
            Expression.Rule =               ValueExpression | FunctionCall;
            ValueExpression.Rule			= "@" + Identifier;
            FunctionCall.Rule				= "$" + Identifier + "(" + FunctionCallArgs + ")"
													| "$" + Identifier + "(" + FunctionCallArgs + ")" + Identifier;             
				FunctionCallArgs.Rule =         MakeStarRule(FunctionCallArgs, comma, FunctionCallArg);
            FunctionCallArg.Rule =          FunctionCallAgValue | InlineFunction | Expression;
            FunctionCallAgValue.Rule =      Identifier | SimpleValue;
            InlineFunction.Rule =           FunctionBody;
				#endregion

				#region Punctuation, braces, transient terms, options
				RegisterOperators(1, "+", "-");
            RegisterOperators(2, "*", "/");
            RegisterOperators(3, Associativity.Right, "**");

            MarkPunctuation("(", ")");
            MarkPunctuation("{", "}");
            MarkPunctuation("=", ",", "&", "|", ";");

            RegisterBracePair("(", ")");
            RegisterBracePair("{", "}");

            MarkTransient(Expression, FunctionArg, FunctionCallArg, SimpleValue, BoolValue, NumberValue, 
					StringValue);
				#endregion

				Root = Program;
            LanguageFlags = LanguageFlags.CreateAst;
      }
		// Provide MyGrammar with appropriate AstContext
		protected override AstContext GetContext(LanguageData language) {
			return new AstContext(language);
		}
		#region interface to Irony
		// Enable AST Evaluation from Grammar Explorer.
		public virtual string RunSample(RunSampleArgs args){
			return "Started ....";
		}
		#endregion Interface to Irony
   }
}
