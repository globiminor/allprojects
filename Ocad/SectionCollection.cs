using System;
using System.Collections.Generic;
using System.Text;

namespace Ocad
{
  public class SectionCollection : LinkedList<ISection>, ICloneable
  {
    public class EqualSectionsComparer : IEqualityComparer<SectionCollection>
    {
      public bool Equals(SectionCollection x, SectionCollection y)
      {
        if (x == y)
        { return true; }

        if (x.Count != y.Count)
        {
          return false;
        }
        LinkedListNode<ISection> nx = x.First;
        LinkedListNode<ISection> ny = x.First;
        while (nx != null)
        {
          if (EqualSection(nx.Value, ny.Value) == false)
          {
            return false;
          }

          nx = nx.Next;
          ny = ny.Next;
        }
        return true;
      }

      private bool EqualSection(ISection x, ISection y)
      {
        if (x == null && y == null)
        { return true; }

        if (x is Control cx)
        {
          if (!(y is Control cy))
          { return false; }

          bool equal = cx.Name == cy.Name;
          return equal;
        }
        else
        {
          throw new NotImplementedException();
        }
      }


      public int GetHashCode(SectionCollection obj)
      {
        int code = 0;
        foreach (var section in obj)
        {
          int add = GetHashCode(section);
          code = code * 29 + add;
        }
        return code;
      }
      public int GetHashCode(ISection section)
      {
        int code;
        if (section is Control)
        {
          code = ((Control)section).Name.GetHashCode();
        }
        else if (section is SplitSection split)
        {
          code = 41 * GetHashCode((ISection)split.Branches[0]);
        }
        else if (section is SplitSection.Branch splitBranch)
        {
          code = 61 * GetHashCode(splitBranch.First.Value);
        }
        else
        {
          throw new NotImplementedException();
        }
        return code;
      }
    }

    private LinkedListNode<ISection> _parent;

    public override string ToString()
    {
      string s = "";
      int iControl = 0;
      foreach (var section in this)
      {
        if (section is Control)
        {
          Control pControl = section as Control;
          s += string.Format("{0,3}. : {1}\n", iControl + 1, pControl.Name);
        }
        if (section is Fork)
        {
          s += "Start fork\n";
          Fork pFork = section as Fork;
          for (int iBranch = 0; iBranch < pFork.Branches.Count; iBranch++)
          {
            s += string.Format("branch {0}\n", iBranch + 1);
            s += pFork.Branches[iBranch].ToString();
          }
          s += "End fork\n";
        }
        if (section is Variation)
        {
          s += "Start variation\n";
          Variation pVariation = section as Variation;
          foreach (Variation.Branch pBranch in pVariation.Branches)
          {
            s += "variation for legs";
            for (int iLeg = 0; iLeg < pBranch.Legs.Count; iLeg++)
            {
              s += string.Format(" {0}", pBranch.Legs[iLeg]);
            }
            s += "\n";
            s += pBranch.ToString();
          }
          s += "End variation\n";
        }
      }
      return s;
    }
    public string ToShortString(int leg)
    {
//      List<string> combs = GetValidCombinationStrings(leg, First, leg.ToString());
      StringBuilder txt = new StringBuilder();
      foreach (var section in this)
      {
        if (txt.Length > 0)
        { txt.Append("-"); }
        if (section is Control cntr)
        {
          txt.Append(cntr.Name);
        }
        else if (section is Fork)
        {
          txt.Append("(");
          Fork fork = (Fork)section;
          bool start = true;
          foreach (var branch in fork.Branches)
          {
            if (start)
            { start = false; }
            else
            { txt.Append("/"); }
            txt.Append(branch.ToShortString(leg));
          }
          txt.Append(")");
        }
        else if (section is Variation)
        {
          txt.Append("[");
          Variation var = (Variation)section;
          txt.Append(var.GetBranch(leg).ToShortString(leg));
          txt.Append("]");
        }
      }
      return txt.ToString();
    }

    public SectionCollection()
    { }
    public SectionCollection(Control start, SectionCollection run, Control finish)
    {
      AddFirst(start);
      AddLast(run);
      AddLast(finish);
    }
    public void AddLast(SectionCollection list)
    {
      if (list == null)
      { return; }
      for (LinkedListNode<ISection> currentNode = list.First;
        currentNode != null; currentNode = currentNode.Next)
      {
        AddLast(currentNode.Value);
      }
    }

    object ICloneable.Clone()
    { return Clone(); }
    public SectionCollection Clone()
    {
      SectionCollection clone = CloneCore();
      return clone;
    }
    protected virtual SectionCollection CloneCore()
    {
      SectionCollection clone = (SectionCollection)Activator.CreateInstance(GetType(), true);
      foreach (var section in this)
      {
        clone.AddLast(section.Clone());
      }
      return clone;
    }

    public List<Control> GetCombination(string combination)
    {
      List<Control> comb = GetCombination(ref combination, null, null);
      return comb;
    }
    public List<Control> GetCombinationWithDummies(string combination,
      Dictionary<ISection, Control> dummies, string dummyPrefix)
    {
      List<Control> comb = GetCombination(ref combination, dummies, dummyPrefix);
      return comb;
    }

    private List<Control> GetCombination(ref string combination,
      Dictionary<ISection, Control> dummies, string dummyPrefix)
    {
      List<Control> comb = new List<Control>();
      int leg = 0;
      if (combination.Length > 0)
      {
        if (int.TryParse(combination.Substring(0, 1), out leg))
        {
          combination = combination.Substring(1);
        }
      }

      bool split = false;
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is Control)
        {
          comb.Add((Control)node.Value);
          split = false;
        }
        else if (node.Value is Fork)
        {
          if (dummies != null && split)
          {
            comb.Add(GetDummyControl(dummies, node.Value, dummyPrefix));
          }
          Fork fork = (Fork)node.Value;
          int iBranch = combination[0] - 'A';
          combination = combination.Substring(1);
          comb.AddRange(fork.Branches[iBranch].GetCombination(ref combination, dummies, dummyPrefix));

          split = true;
        }
        else if (node.Value is Variation)
        {
          if (dummies != null && split)
          {
            comb.Add(GetDummyControl(dummies, node.Value, dummyPrefix));
          }

          Variation var = (Variation)node.Value;
          Variation.Branch branch = var.GetBranch(leg);
          combination = leg + combination;
          comb.AddRange(branch.GetCombination(ref combination, dummies, dummyPrefix));

          split = true;
        }
        else
        { throw new ArgumentException("Unhandled type " + node.Value.GetType()); }
      }
      return comb;
    }

    private Control GetDummyControl(Dictionary<ISection, Control> dummies, ISection key,
      string prefix)
    {
      if (dummies.TryGetValue(key, out Control dummy) == false)
      {
        dummy = new Control();
        int iDummy = dummies.Count + 1;
        dummy.Name = prefix + iDummy;
        dummies.Add(key, dummy);
      }
      return dummy;
    }

    public List<List<Control>> GetExplicitCombinations()
    {
      int nLeg = GetLegCount();
      List<List<Control>> combs = new List<List<Control>>();
      for (int i = 0; i < nLeg; i++)
      {
        List<Control> list = new List<Control>();

        for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
        {
          if (node.Value is Control)
          {
            list.Add((Control)node.Value);
          }
          else
          {
            list.AddRange(((SplitSection)node.Value).GetExplicitCombination(i + 1));
          }
        }
        combs.Add(list);
      }
      return combs;
    }

    private int GetLegCount()
    {
      int nLeg = 1;
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection)
        {
          nLeg = ((SplitSection)node.Value).LegCount();
        }
      }
      return nLeg;
    }

    public List<SimpleSection> GetAllSections()
    {
      int nLegs = GetLegCount();
      List<SimpleSection> list = new List<SimpleSection>();
      for (int iLeg = 0; iLeg < nLegs; iLeg++)
      {
        Control pre = null;
        AppendSections(list, ref pre, First, iLeg, iLeg, null);
      }
      return list;
    }

    private void AppendSections(List<SimpleSection> list, ref Control pre,
      LinkedListNode<ISection> nextNode, int leg, int branch, string where)
    {
      for (LinkedListNode<ISection> node = nextNode; node != null; node = node.Next)
      {
        if (node.Value is Control next)
        {
          if (pre != null)
          {
            SimpleSection section = new SimpleSection(pre, next);
            section.AndWhere(where);
            list.Add(section);
          }
          pre = next;
        }
        else if (node.Value is Variation v)
        {
          int iBranch = v.GetBranchIndex(leg + 1);
          Variation.Branch b = (Variation.Branch)v.Branches[iBranch];
          int ix = b.Legs.IndexOf(leg + 1);
          string legsWhere = b.GetWhere();
          AppendSections(list, ref pre, b.First, leg, ix, legsWhere);
        }
        else if (node.Value is Fork f)
        {
          SplitSection.Branch b = f.Branches[branch];
          AppendSections(list, ref pre, b.First, leg, -1, where);
        }
        else
        {
          throw new NotImplementedException("Unhandled Section type " + node.Value.GetType());
        }
      }
    }

    public double ValidCombinationsFactor()
    {
      return 0.25;
    }

    public List<string> GetValidCombinationStrings()
    {
      int nLeg = 0;
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is Variation)
        {
          nLeg = ((Variation)node.Value).LegCount();
          break;
        }
      }
      if (nLeg == 0)
      { return GetValidCombinationStrings(0, First, ""); }

      List<string> combs = new List<string>();
      for (int i = 0; i < nLeg; i++)
      {
        int leg = i + 1;
        combs.AddRange(GetValidCombinationStrings(leg, First, leg.ToString()));
      }
      return combs;
    }

    protected List<string> GetValidCombinationStrings(int leg,
      LinkedListNode<ISection> start, string pre)
    {
      List<string> combs = new List<string>();
      for (LinkedListNode<ISection> node = start; node != null; node = node.Next)
      {
        if (node.Value is Control)
        { continue; }
        else if (node.Value is Fork fork)
        {

          #region fork

          if (node.Previous == null || node.Previous.Value is Control)
          {
            char add = 'A';
            foreach (var branch in fork.Branches)
            {
              combs.AddRange(GetValidCombinationStrings(leg, node.Next, pre + add));
              add = (char)(add + 1);
            }
          }
          else if (node.Previous.Value is Fork preFork)
          {

            #region fork - fork

            char lastComb = pre[pre.Length - 1];
            List<string> validCombs = new List<string>();
            int index;
            string fromName = null;
            string toName = null;

            for (index = 0; index < fork.Branches.Count; index++)
            {
              if (preFork.Branches[index].Count > 0 && fork.Branches[index].Count > 0)
              {
                Control from = (Control)preFork.Branches[index].Last.Value;
                Control to = (Control)fork.Branches[index].First.Value;
                validCombs.Add(from.Name + "-" + to.Name);
              }
            }
            index = lastComb - 'A';
            if (preFork.Branches[index].Count > 0)
            {
              Control from = (Control)preFork.Branches[index].Last.Value;
              fromName = from.Name;
            }
            if (preFork.Branches[index].Count > 0 && fork.Branches[index].Count > 0)
            {
              Control to = (Control)fork.Branches[index].First.Value;
              toName = to.Name;
            }


            char add = 'A';
            index = 0;
            foreach (var branch in fork.Branches)
            {
              bool cont = false;
              if (add == lastComb)
              {
                cont = true;
              }

              if (cont == false && branch.Count > 0 && fromName != null)
              {
                Control to = (Control)branch.First.Value;
                cont = (validCombs.IndexOf(fromName + "-" + to.Name) >= 0);
              }

              if (cont == false && branch.Count > 0)
              {
                Control to = (Control)branch.First.Value;
                cont = (toName == to.Name);
              }

              if (cont == false && preFork.Branches[index].Count > 0)
              {
                Control from = (Control)preFork.Branches[index].Last.Value;
                cont = (fromName == from.Name);
              }

              if (cont)
              {
                combs.AddRange(GetValidCombinationStrings(leg, node.Next, pre + add));
              }
              add = (char)(add + 1);
              index++;
            }
            #endregion fork - fork
          }
          else if (node.Previous.Value is Variation preVar)
          {
            #region variation-fork
            int preVarIndex = preVar.GetBranchIndex(leg);

            Variation.Branch preBranch = (Variation.Branch)preVar.GetExplicitBranch(preVarIndex);
            int minBranch = 0;
            foreach (Variation.Branch b in preVar.Branches)
            {
              if (b == preBranch)
              { break; }
              minBranch += b.Legs.Count;
            }
            int maxBranch = minBranch + preBranch.Legs.Count;

            Variation.Branch subBranch = preBranch;
            while (subBranch != null)
            {
              if (subBranch.Count == 0)
              { subBranch = null; }
              else if (subBranch.Last.Value is Control)
              { subBranch = null; }
              else if (subBranch.Last.Value is SplitSection)
              {
                int iFork = pre[pre.Length - 1] - 'A';
                minBranch += iFork;
                maxBranch = minBranch + 1;
                subBranch = null;
              }
              else if (subBranch.Last.Value is Variation)
              {
                preVar = (Variation)subBranch.Last.Value;
                preVarIndex = preVar.GetBranchIndex(leg);
                preBranch = (Variation.Branch)preVar.GetExplicitBranch(preVarIndex);

                foreach (Variation.Branch b in preVar.Branches)
                {
                  if (b == preBranch)
                  { break; }
                  minBranch += b.Legs.Count;
                }
                maxBranch = minBranch + preBranch.Legs.Count;

                subBranch = preBranch;
              }
            }

            List<string> validTos = new List<string>();
            for (int i = minBranch; i < maxBranch; i++)
            {
              Fork.Branch explicitBrach = (Fork.Branch)fork.GetExplicitBranch(i);
              validTos.Add(((Control)explicitBrach.First.Value).Name);
            }

            char add = 'A';
            int index = 0;
            foreach (var branch in fork.Branches)
            {
              Control to = (Control)branch.First.Value;

              if (validTos.Contains(to.Name))
              {
                combs.AddRange(GetValidCombinationStrings(leg, node.Next, pre + add));
              }
              add = (char)(add + 1);
              index++;
            }
            #endregion variation-fork
          }
          else
          {
            throw new ArgumentException("Unhandled type " + node.Previous.Value.GetType());
          }
          return combs;

          #endregion fork
        }
        else if (node.Value is Variation var)
        {
          #region variation
          Variation.Branch branch = var.GetBranch(leg);
          if (node.Previous == null || node.Previous.Value is Control
            || node.Previous.Value is Variation)
          {
            combs.AddRange(branch.GetValidLegCombinations(leg, node.Next, pre));
          }
          else if (node.Previous.Value is Fork)
          {
            #region fork-variation
            int varIndex = var.GetBranchIndex(leg);
            int preForkIndex = pre[pre.Length - 1] - 'A';

            bool doComb = false;
            if (varIndex == preForkIndex)
            { doComb = true; }

            if (doComb == false)
            {
              Fork preFork = (Fork)node.Previous.Value;
              List<string> controls = new List<string>();
              Variation.Branch subBranch = branch;

              while (subBranch != null)
              {
                if (subBranch.Count == 0)
                {
                  controls.Add(((Control)preFork.Branches[varIndex].Last.Value).Name);
                  subBranch = null;
                }
                else if (subBranch.First.Value is Control)
                {
                  foreach (var varLeg in subBranch.Legs)
                  {
                    int idx = var.GetBranchIndex(varLeg);
                    if (preFork.Branches[idx].Count > 0)
                    {
                      controls.Add(((Control)preFork.Branches[idx].Last.Value).Name);
                    }
                  }
                  subBranch = null;
                }
                else if (subBranch.First.Value is Fork)
                {
                  controls.Add(((Control)preFork.Branches[varIndex].Last.Value).Name);
                  subBranch = null;
                  throw new Exception("Not implemented");
                }
                else if (subBranch.First.Value is Variation)
                {
                  Variation subVar = (Variation)branch.First.Value;
                  subBranch = subVar.GetBranch(leg);
                }
                else
                { throw new ArgumentException("Unhandled type " + subBranch.First.Value.GetType()); }
              }

              if (preFork.Branches[preForkIndex].Count > 0 &&
                  controls.IndexOf(((Control)preFork.Branches[preForkIndex].Last.Value).Name) >= 0)
              {
                doComb = true;
              }
            }
            if (doComb)
            {
              combs.AddRange(branch.GetValidLegCombinations(leg, node.Next, pre));
            }

            #endregion fork-variation
          }
          else
          {
            throw new ArgumentException("Unhandled type " + node.Previous.Value.GetType());
          }
          return combs;
          #endregion variation
        }
        else
        { throw new ArgumentException("Unhandled type " + node.Value.GetType()); }
      }
      combs.Add(pre);
      return combs;
    }

    public int LegCount()
    {
      foreach (var section in this)
      {
        if (section is SplitSection split)
        {
          return split.LegCount();
        }
      }
      return 1;
    }

    public LinkedListNode<ISection> Parent
    {
      get { return _parent; }
    }
    public void SetParentRecursive(LinkedListNode<ISection> parent)
    {
      _parent = parent;
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection split)
        {
          foreach (var branch in split.Branches)
          {
            branch.SetParentRecursive(node);
          }
        }
      }
    }

    public void Analyse()
    {
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection split)
        {
          if (node.Previous != null && node.Previous.Value is Control)
          {
            foreach (var branch in split.Branches)
            {
              branch.SetPre((Control)node.Previous.Value);
            }
          }
          else if (node.Previous != null && node.Previous.Value is SplitSection)
          {
            SplitSection preSplit = (SplitSection)node.Previous.Value;
            int nBranch = preSplit.LegCount();
            for (int iBranch = 0; iBranch < nBranch; iBranch++)
            {
              SplitSection.Branch branch = split.GetExplicitBranch(iBranch);
              SplitSection.Branch preBranch = preSplit.GetExplicitBranch(iBranch);

              if (branch is Fork.Branch)
              { ((Fork.Branch)branch).SetPre(preBranch); }
              else
              { ((Variation.Branch)branch).SetPre(preBranch, iBranch); }

              if (preBranch is Fork.Branch)
              { ((Fork.Branch)preBranch).SetPost(branch); }
              else
              { ((Variation.Branch)preBranch).SetPost(branch, iBranch); }

              branch.Analyse();
            }
          }
          else if (this is Variation.Branch var)
          {
            for (int iBranch = 0; iBranch < var.Legs.Count; iBranch++)
            {
            }
          }

          if (node.Next != null && node.Next.Value is Control)
          {
            foreach (var branch in split.Branches)
            {
              branch.SetPost((Control)node.Next.Value);
            }
          }
        }
      }
    }

    public int BranchPosition(SplitSection.Branch branch)
    {
      // empty Branch, get control from next Section
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection split)
        {
          int iPosBranch = 0;
          for (int iBranch = 0; iBranch < split.Branches.Count; iBranch++)
          {
            if (split.Branches[iBranch] == branch)
            {
              return iPosBranch;
            }

            // Check Subbranches
            int subPosition = BranchPosition(split.Branches[iBranch]);
            if (subPosition >= 0)
            {
              return iPosBranch + subPosition;
            }

            if (split is Variation)
            { iPosBranch += ((Variation.Branch)split.Branches[iBranch]).Legs.Count; }
            else
            { iPosBranch++; }
          }
        }
      }
      return -1;
    }

    public Control GetExplicitFirstControl(SplitSection.Branch branch, int subBranch)
    {
      if (branch.Count > 0)
      {
        if (branch.First.Value is Control)
        {
          return (Control)branch.First.Value;
        }
        else
        {
          int iPos = BranchPosition(branch);
          return GetExplicitFirstControl((SplitSection.Branch)branch.First.Value, subBranch - iPos);
        }
      }

      // empty Branch, get control from next Section
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection split)
        {
          for (int iBranch = 0; iBranch < split.Branches.Count; iBranch++)
          {
            if (split.Branches[iBranch] == branch)
            {
              // Branch found, get next Control
              if (node.Next == null)
              { return null; }
              if (node.Next.Value is Control)
              { return (Control)node.Next.Value; }

              // no simple control, return next explicit connection
              SplitSection nextSplit = (SplitSection)node.Next.Value;
              Dlg d = new Dlg(GetExplicitFirstControl);
              return GetNeighbor(nextSplit, subBranch, d);
            }

            // Check Subbranches
            Control subControl = GetExplicitFirstControl(split.Branches[iBranch], subBranch);
            if (subControl != null)
            { return subControl; }
          }
        }
      }
      return null;
    }

    public Control GetExplicitLastControl(SplitSection.Branch branch, int subBranch)
    {
      if (branch.Count > 0)
      {
        if (branch.Last.Value is Control)
        {
          return (Control)branch.Last.Value;
        }
        else
        {
          int iPos = BranchPosition(branch);
          return GetExplicitLastControl((SplitSection.Branch)branch.Last.Value, subBranch - iPos);
        }
      }

      // empty Branch, get control from next Section
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection split)
        {
          for (int iBranch = 0; iBranch < split.Branches.Count; iBranch++)
          {
            if (split.Branches[iBranch] == branch)
            {
              // Branch found, get next Control
              if (node.Previous == null)
              { return null; }
              if (node.Previous.Value is Control)
              { return (Control)node.Previous.Value; }

              // no simple control, return next explicit connection
              SplitSection preSplit = (SplitSection)node.Previous.Value;
              Dlg d = new Dlg(GetExplicitLastControl);
              return GetNeighbor(preSplit, subBranch, d);
              //  delegate(SplitSection.Branch b, int i)
              //{ GetExplicitLastControl(b, i); });
            }

            // Check Subbranches
            Control subControl = GetExplicitFirstControl(split.Branches[iBranch], subBranch);
            if (subControl != null)
            { return subControl; }
          }
        }
      }
      return null;
    }

    private delegate Control Dlg(SplitSection.Branch s, int i);

    private Control GetNeighbor(SplitSection split, int subBranch, Dlg directionFct)
    {
      int iPosMax = 0;
      foreach (var branch in split.Branches)
      {
        int iPosMin = iPosMax;
        if (branch is Fork.Branch)
        { iPosMax++; }
        else if (branch is Variation.Branch)
        { iPosMax = iPosMin + ((Variation.Branch)branch).Legs.Count; }
        else { throw new ArgumentException("Unhandled type " + branch.GetType().Name); }

        if (iPosMin <= subBranch && subBranch < iPosMax)
        { return directionFct(branch, subBranch); }
      }
      throw new ArgumentException("Branch " + subBranch + " not found");
    }

    public IList<Control> PreviousControls(SectionCollection part)
    {
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection split)
        {
          foreach (var branch in split.Branches)
          {
            if (branch == part)
            { return LastControls(node.Previous); }

            IList<Control> list = branch.PreviousControls(part);
            if (list == null)
            { continue; }

            if (list.Count == 0)
            { return LastControls(node.Previous); }
            else
            { return list; }
          }
        }
      }
      return null;
    }
    public IList<Control> PreviousControls(ISection section)
    {
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value == section)
        {
          if (section is SplitSection || section is Control)
          { return LastControls(node.Previous); }
        }

        if (node.Value is SplitSection split)
        {
          foreach (var branch in split.Branches)
          {
            IList<Control> list = branch.PreviousControls(section);
            if (list == null)
            { continue; }

            if (list.Count == 0)
            { return LastControls(node.Previous); }
            else
            { return list; }
          }
        }
      }
      return null;
    }
    private IList<Control> LastControls(LinkedListNode<ISection> node)
    {
      if (node == null || node.Value == null)
      { return new List<Control>(); }

      if (node.Value is Control)
      { return new Control[] { (Control)node.Value }; }

      List<Control> list = new List<Control>();
      if (node.Value is SplitSection split)
      {
        foreach (var branch in split.Branches)
        {
          if (branch.Count > 0)
          { list.AddRange(LastControls(branch.Last)); }
          else
          { list.AddRange(LastControls(node.Previous)); }
        }
      }
      return list;
    }

    public IList<Control> NextControls(ISection section)
    {
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value == section)
        {
          if (section is SplitSection || section is Control)
          { return FirstControls(node.Next); }
        }

        if (node.Value is SplitSection split)
        {
          foreach (var branch in split.Branches)
          {
            IList<Control> list = branch.NextControls(section);
            if (list == null)
            { continue; }

            if (list.Count == 0)
            { return FirstControls(node.Next); }
            else
            { return list; }
          }
        }
      }
      return null;
    }
    public IList<Control> NextControls(SectionCollection part)
    {
      for (LinkedListNode<ISection> node = First; node != null; node = node.Next)
      {
        if (node.Value is SplitSection split)
        {
          foreach (var branch in split.Branches)
          {
            if (branch == part)
            { return FirstControls(node.Next); }

            IList<Control> list = branch.NextControls(part);
            if (list == null)
            { continue; }

            if (list.Count == 0)
            { return FirstControls(node.Next); }
            else
            { return list; }
          }
        }
      }
      return null;
    }

    private IList<Control> FirstControls(LinkedListNode<ISection> node)
    {
      if (node == null || node.Value == null)
      { return new List<Control>(); }

      if (node.Value is Control)
      { return new Control[] { (Control)node.Value }; }

      List<Control> list = new List<Control>();
      if (node.Value is SplitSection split)
      {
        foreach (var branch in split.Branches)
        {
          if (branch.Count > 0)
          { list.AddRange(FirstControls(branch.First)); }
          else
          { list.AddRange(FirstControls(node.Next)); }
        }
      }
      return list;
    }

  }
}
