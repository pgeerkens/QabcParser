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
using System.Linq.Expressions;
using System.Text;
using System.CodeDom;
using System.Xml;
using System.IO;

using Irony.Ast;
using Irony.Parsing;

// Additional bits and pieces needed in Irony.Ast
namespace Irony.Ast {
	[Flags]
	public enum AstNodeFlags {
		None = 0x0,
		IsTail = 0x01,			//the node is in tail position
		//IsScope = 0x02,    //node defines scope for local variables
	}

	[Flags]
	public enum NodeUseType {
		Unknown,
		Name, //identifier used as a Name container - system would not use it's Evaluate method directly
		CallTarget,
		ValueRead,
		ValueWrite,
		ValueReadWrite,
		Parameter,
		Keyword,
		SpecialSymbol,
	}

	public interface IScriptThread{
		Irony.Ast.AbstractAstNode CurrentNode { get; set; }
		void ThrowScriptError(string error, params object[] args);
	}
	public interface IScopeInfo{	}

	public delegate object EvaluateMethod(IScriptThread thread);
	public delegate void ValueSetterMethod(IScriptThread thread, object value);

	public interface IAstVisitor {
		void BeginVisit(IVisitableNode node);
		void EndVisit(IVisitableNode node);
	}
	public interface IVisitableNode {
		void AcceptVisitor(IAstVisitor visitor);
	}
	//A stub to use when AST node was not created (type not specified on NonTerminal, or error on creation)
	// The purpose of the stub is to throw a meaningful message when interpreter tries to evaluate null node.
	public class NullAstNode : AbstractAstNode {
//		public override void Init(AstContext context, ParseTreeNode treeNode) {
//			base.Init(context, treeNode); 
//		}
		protected override object DoEvaluate(IScriptThread thread) {
		thread.CurrentNode = this;  //standard prolog
		thread.ThrowScriptError(Resources.ErrNullNodeEval, this.Term);
		return null; //never happens
		}
		public override void DoSetValue(IScriptThread thread, object value) {
			throw new NotSupportedException("NullNode.DoSetValue() not supported.");
		}
	}//class

	public class LiteralValueNode : AbstractAstNode {
		public object Value;

		public override void Init(AstContext context, ParseTreeNode treeNode) {
		base.Init(context, treeNode); 
		Value = treeNode.Token.Value;
		AsString = Value == null ? "null" : Value.ToString();
		if (Value is string)
			AsString = "\"" + AsString + "\"";
		}

		protected override object DoEvaluate(IScriptThread thread) { return Value; }

		public override bool IsConstant() { return true; }
		public override void DoSetValue(IScriptThread thread, object value) {
			throw new NotSupportedException("LiteralValueNode.DoSetValue() not supported.");
		}
	}//class

  public static class CustomExpressionTypes {
    public const ExpressionType NotAnExpression =(ExpressionType) (-1);
  }

  public class AstNodeList<TAstNode> : List<TAstNode> where TAstNode : AbstractAstNode { }

  /// <summary>Abstract base class for all AstNodes. Contains mucho boilerplate to support 
  /// tree-walking, mostly adapted from Irony.Interpreter.Ast. </summary>
  public abstract partial class AbstractAstNode : IAstNodeInit, IBrowsableAstNode
  {
    public AbstractAstNode Parent { get; protected set; }
	 public AstNodeFlags Flags { get; set; }
    public BnfTerm Term { get; protected set; }
    public SourceSpan Span { get; protected set; }
    protected ExpressionType ExpressionType { get; set; } //= CustomExpressionTypes.NotAnExpression;
    protected object LockObject { get; set; } //= new object();

    //Used for pointing to error location. For most nodes it would be the location of the node itself.
    // One exception is BinExprNode: when we get "Division by zero" error evaluating 
    //  x = (5 + 3) / (2 - 2)
    // it is better to point to "/" as error location, rather than the first "(" - which is the start 
    // location of binary expression. 
    public SourceLocation ErrorAnchor { get; protected set; }
    // Role is a free-form string used as prefix in ToString() representation of the node. 
    // Node's parent can set it to "property name" or role of the child node in parent's node currentFrame.Context. 
    public string Role { get; set; }
    // Default AstNode.ToString() returns 'Role: AsString', which is used for showing node in AST tree. 
    public virtual string AsString { get; protected set; }
    public /*readonly*/ AstNodeList<AbstractAstNode> ChildNodes { get; protected set; } // = new AstNodeList();  //List of child nodes

    //Reference to Evaluate method implementation. Initially set to DoEvaluate virtual method. 
    public EvaluateMethod Evaluate { get; protected set; }
    public ValueSetterMethod SetValue { get; protected set; }

    // Public default constructor
    public AbstractAstNode() {
      this.Evaluate = DoEvaluate;
      this.SetValue = DoSetValue;

		// added 2012-10-12 by pgeerkens while converting fields to properties
		ExpressionType = CustomExpressionTypes.NotAnExpression;
		LockObject = new object();
		ChildNodes = new AstNodeList<AbstractAstNode>();

#if MoveToImplementation
		 UseType = NodeUseType.Unknown;
#endif
    }
    public SourceLocation Location { get { return Span.Location; } }	// TO-DO Use ErrorAnchor?

    #region IAstNodeInit Members
    public virtual void Init(AstContext context, ParseTreeNode treeNode) {
      this.Term = treeNode.Term;
      Span = treeNode.Span;
      ErrorAnchor = this.Location;
      treeNode.AstNode = this;
      AsString = (Term == null ? this.GetType().Name : Term.Name);
    }
    #endregion

    //ModuleNode - computed on demand
    public AbstractAstNode ModuleNode {
      get {
        if (_moduleNode == null) {
          _moduleNode = (Parent == null) ? this : Parent.ModuleNode;
        }
        return _moduleNode;
      }
      set { _moduleNode = value; }
    } AbstractAstNode _moduleNode;

    #region virtual methods: DoEvaluate, SetValue, IsConstant, SetIsTail, GetDependentScopeInfo
    public virtual void Reset() {
      _moduleNode = null;
      Evaluate = DoEvaluate;
      foreach (var child in ChildNodes)
        child.Reset();
    }

		//By default the Evaluate field points to this method.
		protected virtual object DoEvaluate(IScriptThread thread) {
			//These 2 lines are standard prolog/epilog statements. Place them in every Evaluate and SetValue implementations.
			thread.CurrentNode = this;  //standard prolog
			thread.CurrentNode = Parent; //standard epilog
			return null; 
		}

      //Place the prolog/epilog lines in every implementation of SetValue method (see DoEvaluate above)
#if MoveToImplementation
	  public virtual void DoSetValue(IScriptThread thread, object value) { }
#else
	 public abstract void DoSetValue(IScriptThread thread, object value);
#endif

    public virtual bool IsConstant() {
      return false; 
    }

    /// <summary>
    /// Sets a flag indicating that the node is in tail position. The value is propagated from parent to children. 
    /// Should propagate this call to appropriate children.
    /// </summary>
    public virtual void SetIsTail() {
      Flags |= AstNodeFlags.IsTail;
    }

#if MoveToImplementation
    /// <summary>
    /// Dependent scope is a scope produced by the node. For ex, FunctionDefNode defines a scope
    /// </summary>
    public virtual IScopeInfo DependentScopeInfo {
      get {return _dependentScope; }
      set { _dependentScope = value; }
    } IScopeInfo _dependentScope;
#endif
    #endregion

    #region IBrowsableAstNode Members
    public virtual System.Collections.IEnumerable GetChildNodes() {
      return ChildNodes;
    }
    public int Position { 
      get { return Span.Location.Position; } 
    }
    #endregion

    #region Visitors, Iterators
#if MoveToImplementation
    //the first primitive Visitor facility
    public virtual void AcceptVisitor(IAstVisitor visitor) {
      visitor.BeginVisit(this);
      if (ChildNodes.Count > 0)
        foreach(AstNode node in ChildNodes)
          node.AcceptVisitor(visitor);
      visitor.EndVisit(this);
    }
#endif

    //Node traversal 
    public IEnumerable<AbstractAstNode> GetAll() {
      AstNodeList<AbstractAstNode> result = new AstNodeList<AbstractAstNode>();
      AddAll(result);
      return result; 
    }
    private void AddAll(AstNodeList<AbstractAstNode> list) {
      list.Add(this);
      foreach (AbstractAstNode child in this.ChildNodes)
        if (child != null) 
          child.AddAll(list);
    }
    #endregion

    #region overrides: ToString
    public override string ToString() {
      return string.IsNullOrEmpty(Role) ? AsString : Role + ": " + AsString;
    }
    #endregion

    #region Utility methods: AddChild, HandleError

    protected AbstractAstNode AddChild(string role, ParseTreeNode childParseNode) {
      return AddChild(NodeUseType.Unknown, role, childParseNode);
    }

    protected AbstractAstNode AddChild(NodeUseType useType, string role, ParseTreeNode childParseNode) {
      var child = (AbstractAstNode)childParseNode.AstNode;
      if (child == null) {
        child = new NullAstNode(); //put a stub to throw an exception with clear message on attempt to evaluate. 
        child.Init(null,childParseNode); 
		}
      child.Role = role;
      child.Parent = this;
      ChildNodes.Add(child);
      return child;
    }

    #endregion
  }//class
}//namespace