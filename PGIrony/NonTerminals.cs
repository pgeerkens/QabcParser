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
using System.Linq.Expressions;
using System.Text;

using Irony.Ast;
using Irony.Parsing;

namespace PGSoftwareSolutionsInc.PGIrony {
	/// <summary>
	/// Automatically set NonTerminal name as Type Name stripped of "Node"
	/// </summary>
 	public class TypedNonTerminal : NonTerminal {
	/// <summary>
	/// Automatically set NonTerminal name as Type Name stripped of "Node"
	/// </summary>
		public TypedNonTerminal(Type type) : base(type.Name.Replace("AstNode",string.Empty), type) {}
	}
	/// <summary>A TypedNonTerminal of type <c>TValue</c>.</summary>
	public class TypedNonTerminal<TAstNodeType> : TypedNonTerminal 
		where TAstNodeType : AstNode<AstContext> {
	/// <summary>A TypedNonTerminal of type <c>TValue</c>.</summary>
		public TypedNonTerminal() : base(typeof(TAstNodeType)) {}
	}
	/// <summary>A TypedNonTerminal of type <c>TValue</c> in a custom <c>AstContext</c>.</summary>
	/// <typeparam name="TAstContext"></typeparam>
	/// <typeparam name="TAstNodeType"></typeparam>
	public class TypedNonTerminal<TAstContext,TAstNodeType> : TypedNonTerminal  
		where TAstNodeType : AstNode<TAstContext> 
		where TAstContext : AstContext
	{
		/// <summary>A TypedNonTerminal of type <c>TValue</c> in a custom <c>AstContext</c>.</summary>
		public TypedNonTerminal() : base(typeof(TAstNodeType)) {}
	}

	/// <summary>A NonTerminal that <i>mirrors</i> its value to a specified 'setter' property of a <c>TClass</c> instance.</summary>
	/// <typeparam name="TClass">Type of the object containing the mirror property.</typeparam>
	/// <typeparam name="TValue">A 'getter' for the mirror property, from which a 'setter' will be inferred.</typeparam>
	public class CommandNonTerminal<TClass,TValue> : NonTerminal
	where TClass:new() 
	where TValue:struct {
		/// <summary>Returns a NonTerminal publishing an internal AstNodeCreator using as its fieldUpdater
		///  a <i>setter</i> for the given  class constructed from the supplied <i>getter</i>.</summary>
		public CommandNonTerminal(Expression<Func<TClass,TValue>> getter) 
			: base(((MemberExpression)getter.Body).Member.Name) {
			base.AstConfig.NodeCreator = CommandNodeCreator(getter);
		}
		/// <summary>
		///  Returns an AstNodeCreator using a <i>setter</i> for the given class constructed from 
		///  the supplied <i>getter</i> as its fieldUpdater.
		/// </summary>
		/// <typeparam name="Func<TClass,TValue>">The <i>getter</i> from which a <i>setter</i> will be inferred.</typeparam>
		/// <typeparam name="TClass">The class for which a <i>setter</i> is desired.</typeparam>
		/// <typeparam name="TValue">The Type for both the <i>setter</i> and <i>getter</i>.</typeparam>
		/// <param name="getter">A <i>getter</i> in class <b>TClass</b></param>
		/// <returns>AstNodeCreator for the specified flavour of Command Node, using the constructed 
		/// <i>setter</i> as its <i>fieldUpdater</i>.</returns>
		private AstNodeCreator CommandNodeCreator(Expression<Func<TClass,TValue>> getter) 
		 {
			var member	= (MemberExpression)getter.Body;
			var action = SetterFromGetter(getter);

			Action<TValue> fieldUpdater = (t) => action(new TClass(),t);
			return (c,s) => (new CommandAstNode<TValue>(fieldUpdater, member.Member.Name)).Init(c,s);
		}
		/// <summary>Returns a 'setter' for the class <c>TClass</c>, from the provided 'getter'.</summary>
		/// <typeparam name="TClass"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="getter"></param>
		/// <returns></returns>
		protected static Action<TClass,TValue> SetterFromGetter(
			Expression<Func<TClass,TValue>> getter) 
		{
			var member	= (MemberExpression)getter.Body;
			var param	= Expression.Parameter(typeof(TValue), "value");
			var setter	= Expression.Lambda<Action<TClass,TValue>>(
							  Expression.Assign(member,param), getter.Parameters[0], param);
			return setter.Compile();
		}
	}
	/// <summary>A NonTerminal automatically <i>marked</i>as transient.</summary>
	public class TransientNonTerminal : NonTerminal {
		public TransientNonTerminal(string name) : base(name) {
			Flags |= TermFlags.IsTransient | TermFlags.NoAstNode;
		}
	}
}
