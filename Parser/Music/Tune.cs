#region License
////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012-2016-2016
////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PGSoftwareSolutions.Music {
    /// <summary>TODO</summary>
	public class Tune<TNote>: List<TNote> where TNote:INote{
        /// <summary>TODO</summary>
		public static Tune<TNote> operator + (Tune<TNote> lhs, TNote rhs) { lhs.Add(rhs);		return lhs; }
        /// <summary>TODO</summary>
		public static Tune<TNote> operator - (Tune<TNote> lhs, TNote rhs) { lhs.Remove(rhs);	return lhs; }
	}
}
