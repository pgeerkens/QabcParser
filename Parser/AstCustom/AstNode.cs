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
using Irony.Ast;
using Irony.Parsing;

namespace PGSoftwareSolutions.PGIrony {
	#region interfaces: IAstWorkNode, IValueNode
	public interface IAstValueNode<TValue> {
		TValue Value { get; }
	}
	public interface IAstWorkNode<TContext> where TContext : PGIrony.AstContext {
		void EvaluateTree(TContext context);
	}
	#endregion interfaces: IAstWorkNode, IValueNode

	public abstract class AstNode<TAstContext>	: Irony.Ast.AbstractAstNode, IAstWorkNode<TAstContext>
	where TAstContext: PGIrony.AstContext {
		protected	LogMessageList		Messages	{ get; private set; }
		public AstNode() : base() {}

		/// <summary>
		/// Entry point, and error-handler, for all custom ASTNode creation invoked by the
		/// Irony framework.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="treeNode"></param>
		public override sealed void Init(Irony.Ast.AstContext context, ParseTreeNode treeNode) {
			try {
				this.Init((TAstContext)context, treeNode);
			} catch (InvalidCastException ex) {
				Messages.Add(new LogMessage(ErrorLevel.Error, Span, ex.Message, null));
			} catch (ArgumentOutOfRangeException ex) {
				Messages.Add(new LogMessage(ErrorLevel.Error, Span, ex.Message, null));
			} catch (Exception ex) {  // Add new specific cases as encountered
				Messages.Add(new PGIrony.LogMessage(ErrorLevel.Error, Span,
					"*** Add New handler in MyAstNodes. ***\n" 
					+ Environment.NewLine + ex.Message, null));
			}
		}

		/// <summary>
		/// Derived entry point for custom ASTNode creation.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="treeNode"></param>
		public virtual void Init(TAstContext context, ParseTreeNode treeNode) {
			base.Init(context as AstContext, treeNode);
			foreach (var child in treeNode.GetMappedChildNodes()) { AddChild(string.Empty, child); }
			Messages = context.Messages;	
			AsString	= GetType().Name.Replace("AstNode", string.Empty);

			if (ChildNodes.Count != 0) 
				ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
			Context = context;
		}
		protected TAstContext Context { get; private set; }

		public override string AsString {
			get { return (ChildNodes.Count == 1) ? ChildNodes[0].AsString : base.AsString; }
		}

		/// <summary>
		/// Stub, to be overridden by nodes that have evaluation work to do after their
		/// ChildNodes have evaluated.
		/// </summary>
		/// <param name="context"></param>
		protected virtual void DoMyWork(TAstContext context) { ; }

		/// <summary>
		/// Evaluation-time AST-tree walker.
		/// </summary>
		/// <param name="context"></param>
		public virtual void EvaluateTree(TAstContext context) {
			foreach (AbstractAstNode node in ChildNodes) { 
				if (node is IAstWorkNode<TAstContext>)
					((IAstWorkNode<TAstContext>)node).EvaluateTree(context); 
			}
			DoMyWork(context);
		}
		public override void DoSetValue(IScriptThread thread, object value) {
			throw new NotSupportedException("AstNode.DoSetValue() not supported.");
		}
	}

	public abstract class ListNode<TContext> : AstNode<TContext>, IAstWorkNode<TContext> 
	where TContext:AstContext {}

	public class LiteralValueNode<TValue>	: LiteralValueNode, IAstValueNode<TValue> {
		TValue IAstValueNode<TValue>.Value { get { return (TValue)base.Value; } }
	}

	#region CommandAstNode
	public class ValueNode<TAstContext,TValue>	: PGIrony.AstNode<TAstContext>, IAstValueNode<TValue> 
	where TAstContext:PGIrony.AstContext{
		public ValueNode() : base() {
			Name = GetType().Name.Replace("AstNode", string.Empty)+ " = ";
		}
		public ValueNode(string name) : base() {
			Name = name + " = ";
		}
		protected virtual	Func<string,TValue>	Parser	{ get; set; }
		public virtual		TValue					Value		{ get; protected set; }
		public virtual		string					Suffix	{ get { return Value.ToString(); } } 
		public virtual		string					Name		{ get; protected set; }
		public override	string					AsString	{ get { return Name + Suffix; } }
	}

	/// <summary>
	/// Generic AST node encapsulating a strongly-typed value-type derived from a single (non-transient),
	/// child node (usually a Terminal), and parameterizing an AST-Context environment setting.
	/// </summary>
	/// <typeparam name="TValue">The type of this command node</typeparam>
	public class CommandAstNode<TValue>	: ValueNode<PGIrony.AstContext,TValue>	where TValue:struct {
		public CommandAstNode(Action<TValue> fieldUpdater, string name) : base(name) { 
			FieldUpdater = fieldUpdater ?? (v => {}); // TO-DO Throw exception instead perhaps?
		}
		public override void		Init(PGIrony.AstContext context, ParseTreeNode treeNode) {
			base.Init(context, treeNode);
			FieldUpdater(Value);
		}
		protected virtual	Action<TValue>	FieldUpdater{ get; private set; }
		public	override		TValue		Value			{ get { return (((IAstValueNode<TValue>)ChildNodes[0]).Value); } }

		protected override void DoMyWork(AstContext context) {
			base.DoMyWork(context);
			FieldUpdater(Value); 
		}
	}
	#endregion
}
