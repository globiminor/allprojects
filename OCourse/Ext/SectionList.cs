using Ocad;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCourse.Ext
{
	internal interface ICloneable<T>
	{
		T Clone();
	}
	internal interface ISectionList
	{
		IEnumerable<Control> Controls { get; }
		IEnumerable<NextControl> NextControls { get; }
		int ControlsCount { get; }
		Control GetControl(int index);
		NextControl GetNextControl(int index);
		int CountExisting(Control from, Control to);

		NextControl Add(NextControl current);
		VariationBuilder.WhereInfo.State GetState();
		VariationBuilder.WhereInfo Add(VariationBuilder.WhereInfo where);
		VariationBuilder.SplitInfo GetSplitInfo(Control split);
	}

	public class SectionList : ISectionList, ICloneable<SectionList>
	{
		public class NameComparer : IComparer<SectionList>, IComparer<object>
		{
			int IComparer<object>.Compare(object x, object y)
			{
				SectionList sx = x as SectionList;
				SectionList sy = y as SectionList;
				if (sx == sy) return 0;
				if (sx == null) return 1;
				if (sy == null) return -1;

				return Compare(sx, sy);
			}
			public int Compare(SectionList x, SectionList y)
			{
				return x.GetName().CompareTo(y.GetName());
			}
		}

		public class EqualControlNamesComparer : IEqualityComparer<SectionList>
		{
			public bool Equals(SectionList x, SectionList y)
			{
				int n = x._nextControls.Count;
				if (n != y._nextControls.Count)
				{
					return false;
				}
				for (int i = 0; i < n; i++)
				{
					if (x._nextControls[i].Control.Name != y._nextControls[i].Control.Name)
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(SectionList obj)
			{
				return obj._nextControls[0].Control.Name.GetHashCode();
			}
		}

		private readonly List<VariationBuilder.WhereInfo> _wheres = new List<VariationBuilder.WhereInfo>();
		private readonly List<NextControl> _nextControls = new List<NextControl>();

		private string _preName;

		private static Control _legStart;
		public static Control LegStart => _legStart ?? (_legStart = new Control(string.Empty, ControlCode.Start));

		public SectionList(NextControl start)
		{
			_nextControls.Add(start);
		}

		public int ControlsCount
		{
			get { return _nextControls.Count; }
		}

		public IEnumerable<Control> Controls
		{
			get
			{
				foreach (var next in _nextControls)
				{
					int nInter = next.Inter.Count - 1;
					for (int i = 0; i < nInter; i++)
					{ yield return next.Inter[i]; }
					yield return next.Control;
				}
			}
		}

		IEnumerable<NextControl> ISectionList.NextControls => NextControls;
		public IReadOnlyList<NextControl> NextControls => _nextControls;

		public string PreName
		{
			get { return _preName; }
			set { _preName = value; }
		}

		public IList<Course> Parts
		{
			get { throw new NotImplementedException(); }
		}

		VariationBuilder.SplitInfo ISectionList.GetSplitInfo(Control split)
		{
			int iSplit = 0;
			int iPos = 0;
			int pos = 0;
			string pre = null;
			foreach (var control in Controls)
			{
				if (pre == split.Name)
				{
					iSplit++;
					iPos = pos - 1;
				}
				pre = control.Name;
				pos++;
			}
			return new VariationBuilder.SplitInfo(this, iPos, iSplit);
		}

		public string GetName()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0}", _preName);
			foreach (var next in _nextControls)
			{
				if (next.Code != 0)
				{ sb.Append(next.Code); }
			}
			return sb.ToString();
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var control in Controls)
			{
				sb.AppendFormat("{0} ", control.Name);
			}
			string full = sb.ToString();
			if (full.Length > 200)
			{
				full = full.Substring(0, 100) + "..." + full.Substring(full.Length - 100);
			}
			return full;
		}

		public List<SectionList> GetParts(bool byLegs = false)
		{
			List<SectionList> parts = new List<SectionList>();
			SectionList current = null;
			foreach (var next in NextControls)
			{
				if (next.Control.Code == ControlCode.Start)
				{
					if (current == null
						|| (byLegs && next.Control == LegStart)
						|| (!byLegs))
					{
						if (current != null)
						{
							parts.Add(current);
						}
						if (next.Control == LegStart)
						{ current = null; }
						else
						{ current = new SectionList(next); }
						continue;
					}
				}

				if (current != null)
				{
					current.Add(next);
				}
				else
				{ // funny course !?!
					current = new SectionList(next);
				}
			}
			parts.Add(current);
			return parts;
		}

		public SectionList Clone()
		{
			SectionList clone = new SectionList(_nextControls[0]);
			clone._nextControls.Clear();

			clone._nextControls.AddRange(_nextControls);
			clone._wheres.AddRange(_wheres);
			return clone;
		}

		VariationBuilder.WhereInfo.State ISectionList.GetState()
		{ return GetState(); }
		internal VariationBuilder.WhereInfo.State GetState()
		{
			if (_wheres.Count == 0)
			{ return VariationBuilder.WhereInfo.State.Fulfilled; }

			List<VariationBuilder.WhereInfo> remove = new List<VariationBuilder.WhereInfo>();
			foreach (VariationBuilder.WhereInfo where in _wheres)
			{
				VariationBuilder.WhereInfo.State state = where.GetState(this);
				if (state == VariationBuilder.WhereInfo.State.Failed)
				{ return VariationBuilder.WhereInfo.State.Failed; }

				if (state == VariationBuilder.WhereInfo.State.Unknown)
				{ continue; }

				if (state == VariationBuilder.WhereInfo.State.Fulfilled)
				{ remove.Add(where); }
			}
			foreach (VariationBuilder.WhereInfo rem in remove)
			{ _wheres.Remove(rem); }

			if (_wheres.Count == 0)
			{ return VariationBuilder.WhereInfo.State.Fulfilled; }

			return VariationBuilder.WhereInfo.State.Unknown;
		}

		public Course ToSimpleCourse()
		{
			Course course = new Course(GetName());
			foreach (var control in Controls)
			{
				Control add;
				IList<string> nameParts = control.Name.Split('.');
				if (nameParts.Count == 1)
				{ add = control; }
				else
				{
					add = control.Clone();
					add.Name = nameParts[0];
				}
				if (course.Last == null || ((Control)course.Last.Value).Name != add.Name)
				{
					course.AddLast(add);
				}
			}
			return course;
		}

		public NextControl GetNextControl(int i)
		{
			if (i < 0)
			{
				i = _nextControls.Count - (-i % _nextControls.Count) - 1;
			}
			return _nextControls[i];
		}

		public Control GetControl(int i)
		{
			return GetNextControl(i).Control;
		}
		public NextControl Add(NextControl nextControl)
		{
			_nextControls.Add(nextControl);
			return nextControl;
		}

		VariationBuilder.WhereInfo ISectionList.Add(VariationBuilder.WhereInfo where)
		{ return Add(where); }
		internal VariationBuilder.WhereInfo Add(VariationBuilder.WhereInfo where)
		{
			if (where == null)
			{ return null; }
			_wheres.Add(where);
			return where;
		}

		public int IndexOf(NextControl next)
		{
			return _nextControls.IndexOf(next);
		}

		public int CountExisting(Control from, Control to)
		{
			int count = 0;
			Control c1 = null;
			foreach (var nextControl in _nextControls)
			{
				Control c0 = c1;
				c1 = nextControl.Control;
				if (c0 == from && c1 == to)
				{
					count++;
				}
			}
			return count;
		}

		public void SetLeg(int leg)
		{
			_preName = leg.ToString();
		}

		public static IList<SectionList> GetSimpleParts(IList<SectionList> combined)
		{
			EqualControlNamesComparer cmp = new EqualControlNamesComparer();
			Dictionary<SectionList, SectionList> partsDict = new Dictionary<SectionList, SectionList>(cmp);

			foreach (var list in combined)
			{
				IList<SectionList> parts = list.GetParts();
				foreach (var part in parts)
				{
					TryAdd(partsDict, part);
				}
			}

			List<SectionList> allParts = new List<SectionList>(partsDict.Keys);
			allParts.Sort(new NameComparer());

			return allParts;
		}

		private static void TryAdd(Dictionary<SectionList, SectionList> parts, SectionList part)
		{
			if (!parts.ContainsKey(part))
			{
				parts.Add(part, part);
			}
		}
	}
	public class MultiSection : SimpleSection
	{
		public readonly List<Control> Inter = new List<Control>();
		public MultiSection(Control from, Control to)
			: base(from, to)
		{ }
	}
}