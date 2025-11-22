using CSharpMath.Atom;
using CSharpMath.Display.FrontEnd;
using System;
using System.Collections.Generic;
using System.Drawing;
using CSharpMath;
using System.Timers;
using CSharpMath.Atom;
using CSharpMath.Display;
using CSharpMath.Display.Displays;
using CSharpMath.Display.FrontEnd;
using CSharpMath.Structures;
using CSharpMath.Editor;
using CSharpMath.Rendering.BackEnd;
using CSharpMath.Rendering.FrontEnd;
using Atoms = CSharpMath.Atom.Atoms;
using Timer=System.Timers.Timer;
namespace CsGrafeq.CSharpMath.Editor
{

  public sealed class CgMathKeyboard: IDisposable {
    private Timer blinkTimer;
    public const double DefaultBlinkMilliseconds = 800;
    public CgMathKeyboard() {
      Context = TypesettingContext.Instance;
      Font = new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), PainterConstants.DefaultFontSize);
      blinkTimer = new Timer(DefaultBlinkMilliseconds);
      blinkTimer.Elapsed += (sender, e) => {
        if (!(MathList.AtomAt(_insertionIndex) is Atoms.Placeholder) || LaTeXSettings.PlaceholderBlinks)
          InsertionPositionHighlighted = !InsertionPositionHighlighted;
      };
      blinkTimer.Start();
    }
    public bool ShouldDrawCaret => InsertionPositionHighlighted && !(MathList.AtomAt(_insertionIndex) is Atoms.Placeholder);
    public void StartBlinking() => blinkTimer.Start();
    public void StopBlinking() => blinkTimer.Stop();
    private TypesettingContext<Fonts, Glyph> Context { get; }
    static void ResetPlaceholders(MathList mathList) {
      foreach (var mathAtom in mathList.Atoms) {
        ResetPlaceholders(mathAtom.Superscript);
        ResetPlaceholders(mathAtom.Subscript);
        switch (mathAtom) {
          case Atoms.Placeholder placeholder:
            placeholder.Color = LaTeXSettings.PlaceholderRestingColor;
            placeholder.Nucleus = LaTeXSettings.PlaceholderRestingNucleus;
            break;
          case IMathListContainer container:
            foreach (var list in container.InnerLists)
              ResetPlaceholders(list);
            break;
        }
      }
    }
    bool _insertionPositionHighlighted;
    public bool InsertionPositionHighlighted {
      get => _insertionPositionHighlighted;
      set {
        blinkTimer.Stop();
        blinkTimer.Start();
        _insertionPositionHighlighted = value;
        if (MathList.AtomAt(_insertionIndex) is Atoms.Placeholder placeholder) {
          (placeholder.Nucleus, placeholder.Color) =
            _insertionPositionHighlighted
            ? (LaTeXSettings.PlaceholderActiveNucleus, LaTeXSettings.PlaceholderActiveColor)
            : (LaTeXSettings.PlaceholderRestingNucleus, LaTeXSettings.PlaceholderRestingColor);
        }
        RecreateDisplayFromMathList();
        RedrawRequested?.Invoke(this, EventArgs.Empty);
      }
    }
    public ListDisplay<Fonts, Glyph>? Display { get; private set; }
    public MathList MathList { get; } = new MathList();
    public string LaTeX => LaTeXParser.MathListToLaTeX(MathList).ToString();
    private MathListIndex _insertionIndex = MathListIndex.Level0Index(0);
    public MathListIndex InsertionIndex {
      get => _insertionIndex;
      set {
        _insertionIndex = value;
        ResetPlaceholders(MathList);
        InsertionPositionHighlighted = true;
      }
    }
    public Fonts Font { get; set; }
    public LineStyle LineStyle { get; set; }
    public Color SelectColor { get; set; }
    public bool HasText => MathList?.Atoms?.Count > 0;
    public void RecreateDisplayFromMathList() {
      var position = Display?.Position ?? default;
      Display = Typesetter.CreateLine(MathList, Font, Context, LineStyle);
      Display.Position = position;
    }
    /// <summary>Keyboard should now be hidden and input be discarded.</summary>
    public event EventHandler? DismissPressed;
    /// <summary>Keyboard should now be hidden and input be saved.</summary>
    public event EventHandler? ReturnPressed;
    /// <summary><see cref="Display"/> should be redrawn.</summary>
    public event EventHandler? RedrawRequested;
    public PointF? ClosestPointToIndex(MathListIndex index) =>
      Display?.PointForIndex(Context, index);
    public MathListIndex? ClosestIndexToPoint(PointF point) =>
      Display?.IndexForPoint(Context, point);
    public void KeyPress(params CgMathKeyboardInput[] inputs) {
      foreach (var input in inputs) KeyPress(input);
    }
    public void KeyPress(CgMathKeyboardInput input) {
      void HandleScriptButton(bool isSuperScript) {
        var subIndexType = isSuperScript ? MathListSubIndexType.Superscript : MathListSubIndexType.Subscript;
        MathList GetScript(MathAtom atom) => isSuperScript ? atom.Superscript : atom.Subscript;
        void SetScript(MathAtom atom, MathList value) => GetScript(atom).Append(value);
        void CreateEmptyAtom() {
          // Create an empty atom and move the insertion index up.
          var emptyAtom = LaTeXSettings.Placeholder;
          SetScript(emptyAtom, LaTeXSettings.PlaceholderList);
          MathList.InsertAndAdvance(ref _insertionIndex, emptyAtom, subIndexType);
        }
        static bool IsFullPlaceholderRequired(MathAtom mathAtom) =>
          mathAtom switch
          {
            Atoms.BinaryOperator _ => true,
            Atoms.UnaryOperator _ => true,
            Atoms.Relation _ => true,
            Atoms.Open _ => true,
            Atoms.Punctuation _ => true,
            _ => false
          };
        if (!(_insertionIndex.Previous is MathListIndex previous)) {
          CreateEmptyAtom();
        } else {
          var isBetweenBaseAndScripts =
            _insertionIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts;
          var prevIndexCorrected =
            isBetweenBaseAndScripts
            ? _insertionIndex.LevelDown()
              ?? throw new InvalidCodePathException("BetweenBaseAndScripts index has null LevelDown")
            : previous;
          var prevAtom = MathList.AtomAt(prevIndexCorrected);
          if (prevAtom is null)
            throw new InvalidCodePathException("prevAtom is null");
          if (!isBetweenBaseAndScripts && IsFullPlaceholderRequired(prevAtom)) {
            CreateEmptyAtom();
          } else {
            var script = GetScript(prevAtom);
            if (script.IsEmpty()) {
              SetScript(prevAtom, LaTeXSettings.PlaceholderList);
            }
            _insertionIndex = prevIndexCorrected.LevelUpWithSubIndex
              (subIndexType, MathListIndex.Level0Index(0));
          }
        }
      }

      void HandleSlashButton() {
        // special / handling - makes the thing a fraction
        var numerator = new Stack<MathAtom>();
        var parenDepth = 0;
        if (_insertionIndex.FinalSubIndexType == MathListSubIndexType.BetweenBaseAndScripts)
          _insertionIndex = _insertionIndex.LevelDown()?.Next
              ?? throw new InvalidCodePathException("_insertionIndex.LevelDown() returned null");
        for (; _insertionIndex.Previous != null; _insertionIndex = _insertionIndex.Previous) {
          switch (MathList.AtomAt(_insertionIndex.Previous), parenDepth) {
            case (null, _): throw new InvalidCodePathException("Invalid _insertionIndex");
            // Stop looking behind upon encountering these atoms unparenthesized
            case (Atoms.Open _, _) when --parenDepth < 0: goto stop;
            case (Atoms.Close a, _): parenDepth++; numerator.Push(a); break;
            case (Atoms.UnaryOperator _, 0): goto stop;
            case (Atoms.BinaryOperator _, 0): goto stop;
            case (Atoms.Relation _, 0): goto stop;
            case (Atoms.Fraction _, 0): goto stop;
            case (Atoms.Open _, _) when parenDepth < 0: goto stop;
            // We don't put this atom on the fraction
            case (var a, _): numerator.Push(a); break;
          }
        }
        stop: MathList.RemoveAtoms(new MathListRange(_insertionIndex, numerator.Count));
        if (numerator.Count == 0)
          // so we didn't really find any numbers before this, so make the numerator 1
          numerator.Push(new Atoms.Number("1"));
        if (MathList.AtomAt(_insertionIndex.Previous) is Atoms.Fraction)
          // Add a times symbol
          MathList.InsertAndAdvance(ref _insertionIndex, LaTeXSettings.Times, MathListSubIndexType.None);
        MathList.InsertAndAdvance(ref _insertionIndex, new Atoms.Fraction(
          new MathList(numerator),
          LaTeXSettings.PlaceholderList
        ), MathListSubIndexType.Denominator);
      }
      void InsertInner(string left, string right) =>
        MathList.InsertAndAdvance(ref _insertionIndex,
          new Atoms.Inner(new Boundary(left), LaTeXSettings.PlaceholderList, new Boundary(right)),
          MathListSubIndexType.Inner);

      void MoveCursorLeft() {
        var prev = _insertionIndex.Previous;
        switch (MathList.AtomAt(prev)) {
          case var _ when prev is null:
          case null: // At beginning of line
            var levelDown = _insertionIndex.LevelDown();
            switch (_insertionIndex.FinalSubIndexType) {
              case MathListSubIndexType.None:
                goto default;
              case var _ when levelDown is null:
                throw new InvalidCodePathException("Null levelDown despite non-None FinalSubIndexType");
              case MathListSubIndexType.Superscript:
                var scriptAtom = MathList.AtomAt(levelDown);
                if (scriptAtom is null)
                  throw new InvalidCodePathException("Invalid levelDown");
                if (scriptAtom.Subscript.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Subscript,
                     MathListIndex.Level0Index(scriptAtom.Subscript.Count));
                else
                  goto case MathListSubIndexType.Subscript;
                break;
              case MathListSubIndexType.Subscript:
                _insertionIndex = levelDown.LevelUpWithSubIndex
                  (MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
                break;
              case MathListSubIndexType.BetweenBaseAndScripts:
                if (MathList.AtomAt(levelDown) is Atoms.Radical rad && rad.Radicand.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Radicand,
                     MathListIndex.Level0Index(rad.Radicand.Count));
                else if (MathList.AtomAt(levelDown) is Atoms.Fraction frac && frac.Denominator.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Denominator,
                     MathListIndex.Level0Index(frac.Denominator.Count));
                else if (MathList.AtomAt(levelDown) is Atoms.Inner inner && inner.InnerList.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Inner,
                     MathListIndex.Level0Index(inner.InnerList.Count));
                else goto case MathListSubIndexType.Radicand;
                break;
              case MathListSubIndexType.Radicand:
                if (MathList.AtomAt(levelDown) is Atoms.Radical radDeg && radDeg.Degree.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Degree, MathListIndex.Level0Index(radDeg.Degree.Count));
                else
                  goto case MathListSubIndexType.Denominator;
                break;
              case MathListSubIndexType.Denominator:
                if (MathList.AtomAt(levelDown) is Atoms.Fraction fracNum && fracNum.Numerator.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Numerator, MathListIndex.Level0Index(fracNum.Numerator.Count));
                else
                  goto default;
                break;
              case MathListSubIndexType.Degree:
              case MathListSubIndexType.Numerator:
              case MathListSubIndexType.Inner:
              default:
                _insertionIndex = levelDown ?? _insertionIndex;
                break;
            }
            break;
          case { Superscript: var s } when s.IsNonEmpty():
            _insertionIndex = prev.LevelUpWithSubIndex
              (MathListSubIndexType.Superscript, MathListIndex.Level0Index(s.Count));
            break;
          case { Subscript: var s } when s.IsNonEmpty():
            _insertionIndex = prev.LevelUpWithSubIndex
              (MathListSubIndexType.Subscript, MathListIndex.Level0Index(s.Count));
            break;
          case Atoms.Inner { InnerList: var l }:
            _insertionIndex = prev.LevelUpWithSubIndex
              (MathListSubIndexType.Inner, MathListIndex.Level0Index(l.Count));
            break;
          case Atoms.Radical { Radicand: var r }:
            _insertionIndex = prev.LevelUpWithSubIndex
              (MathListSubIndexType.Radicand, MathListIndex.Level0Index(r.Count));
            break;
          case Atoms.Fraction { Denominator: var d }:
            _insertionIndex = prev.LevelUpWithSubIndex
              (MathListSubIndexType.Denominator, MathListIndex.Level0Index(d.Count));
            break;
          default:
            _insertionIndex = prev;
            break;
        }
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        if (_insertionIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts) {
          var prevInd = _insertionIndex.LevelDown();
          if (prevInd != null && MathList.AtomAt(prevInd) is Atoms.Placeholder)
            _insertionIndex = prevInd;
        } else if (MathList.AtomAt(_insertionIndex) is null
                   && _insertionIndex?.Previous is MathListIndex previous) {
          if (MathList.AtomAt(previous) is Atoms.Placeholder p && p.Superscript.IsEmpty() && p.Subscript.IsEmpty())
            _insertionIndex = previous; // Skip right side of placeholders when end of line
        }
      }
      void MoveCursorRight() {
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        switch (MathList.AtomAt(_insertionIndex)) {
          case null: //After Count
            var levelDown = _insertionIndex.LevelDown();
            var levelDownAtom = MathList.AtomAt(levelDown);
            switch (_insertionIndex.FinalSubIndexType) {
              case MathListSubIndexType.None:
                goto default;
              case var _ when levelDown is null:
                throw new InvalidCodePathException("Null levelDown despite non-None FinalSubIndexType");
              case var _ when levelDownAtom is null:
                throw new InvalidCodePathException("Invalid levelDown");
              case MathListSubIndexType.Degree:
                if (levelDownAtom is Atoms.Radical)
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Radicand, MathListIndex.Level0Index(0));
                else
                  throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), levelDown);
                break;
              case MathListSubIndexType.Numerator:
                if (levelDownAtom is Atoms.Fraction)
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Denominator, MathListIndex.Level0Index(0));
                else
                  throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), levelDown);
                break;
              case MathListSubIndexType.Radicand:
              case MathListSubIndexType.Denominator:
              case MathListSubIndexType.Inner:
                if (levelDownAtom.Superscript.IsNonEmpty() || levelDownAtom.Subscript.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
                else
                  goto default;
                break;
              case MathListSubIndexType.BetweenBaseAndScripts:
                if (levelDownAtom.Subscript.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Subscript, MathListIndex.Level0Index(0));
                else
                  goto case MathListSubIndexType.Subscript;
                break;
              case MathListSubIndexType.Subscript:
                if (levelDownAtom.Superscript.IsNonEmpty())
                  _insertionIndex = levelDown.LevelUpWithSubIndex
                    (MathListSubIndexType.Superscript, MathListIndex.Level0Index(0));
                else
                  goto default;
                break;
              case MathListSubIndexType.Superscript:
              default:
                _insertionIndex = levelDown?.Next ?? _insertionIndex;
                break;
            }
            break;
          case var a when _insertionIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts:
            levelDown = _insertionIndex.LevelDown();
            if (levelDown is null)
              throw new InvalidCodePathException
                ("_insertionIndex.FinalSubIndexType is BetweenBaseAndScripts but levelDown is null");
            _insertionIndex = levelDown.LevelUpWithSubIndex(
              a.Subscript.IsNonEmpty() ? MathListSubIndexType.Subscript : MathListSubIndexType.Superscript,
              MathListIndex.Level0Index(0));
            break;
          case Atoms.Inner _:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListSubIndexType.Inner, MathListIndex.Level0Index(0));
            break;
          case Atoms.Fraction _:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex
              (MathListSubIndexType.Numerator, MathListIndex.Level0Index(0));
            break;
          case Atoms.Radical rad:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(
              rad.Degree.IsNonEmpty() ? MathListSubIndexType.Degree : MathListSubIndexType.Radicand,
              MathListIndex.Level0Index(0));
            break;
          case var a when a.Superscript.IsNonEmpty() || a.Subscript.IsNonEmpty():
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex
              (MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
            break;
          case Atoms.Placeholder _ when MathList.AtomAt(_insertionIndex.Next) is null:
            // Skip right side of placeholders when end of line
            goto case null;
          default:
            _insertionIndex = _insertionIndex.Next;
            break;
        }
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        if (_insertionIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts
            && MathList.AtomAt(_insertionIndex.LevelDown()) is Atoms.Placeholder)
          MoveCursorRight();
      }

      void DeleteBackwards() {
        // delete the last atom from the list
        if(!HasText) return;
        if (HasText && _insertionIndex.Previous is MathListIndex previous) {
          _insertionIndex = previous;
          MathList.RemoveAt(ref _insertionIndex);
        }
      }

      static bool IsPlaceholderList(MathList ml) => ml.Count == 1 && ml[0] is Atoms.Placeholder;
      void InsertAtom(MathAtom a) =>
        MathList.InsertAndAdvance(ref _insertionIndex, a,
          a switch
          {
            Atoms.Fraction _ => MathListSubIndexType.Numerator,
            Atoms.Radical { Degree: { } d } when IsPlaceholderList(d) => MathListSubIndexType.Degree,
            Atoms.Radical _ => MathListSubIndexType.Radicand,
            _ => MathListSubIndexType.None
          });
      void InsertSymbolName(string name, bool subscript = false, bool superscript = false) {
        var atom =
          LaTeXSettings.AtomForCommand(name) ??
            throw new InvalidCodePathException("Looks like someone mistyped a symbol name...");
        InsertAtom(atom);
        switch (subscript, superscript) {
          case (true, true):
            HandleScriptButton(true);
            _insertionIndex = _insertionIndex.LevelDown()?.Next
              ?? throw new InvalidCodePathException(
                "_insertionIndex.Previous returned null despite script button handling");
            HandleScriptButton(false);
            break;
          case (false, true):
            HandleScriptButton(true);
            break;
          case (true, false):
            HandleScriptButton(false);
            break;
          case (false, false):
            break;
        }
      }

      switch (input) {
      // TODO: Implement up/down buttons
        case CgMathKeyboardInput.Up:
          break;
        case CgMathKeyboardInput.Down:
          break;
        case CgMathKeyboardInput.Left:
          MoveCursorLeft();
          break;
        case CgMathKeyboardInput.Right:
          MoveCursorRight();
          break;
        case CgMathKeyboardInput.Backspace:
          DeleteBackwards();
          break;
        case CgMathKeyboardInput.Clear:
          MathList.Clear();
          InsertionIndex = MathListIndex.Level0Index(0);
          break;
        case CgMathKeyboardInput.Return:
          ReturnPressed?.Invoke(this, EventArgs.Empty);
          InsertionPositionHighlighted = false;
          StopBlinking();
          return;
        case CgMathKeyboardInput.Dismiss:
          DismissPressed?.Invoke(this, EventArgs.Empty);
          InsertionPositionHighlighted = false;
          StopBlinking();
          return;
        case CgMathKeyboardInput.Slash:
          HandleSlashButton();
          break;
        case CgMathKeyboardInput.Power:
          HandleScriptButton(true);
          break;
        case CgMathKeyboardInput.Subscript:
          HandleScriptButton(false);
          break;
        case CgMathKeyboardInput.Fraction:
          InsertAtom(new Atoms.Fraction(LaTeXSettings.PlaceholderList, LaTeXSettings.PlaceholderList));
          break;
        case CgMathKeyboardInput.SquareRoot:
          InsertAtom(new Atoms.Radical(new MathList(), LaTeXSettings.PlaceholderList));
          break;
        case CgMathKeyboardInput.CubeRoot:
          InsertAtom(new Atoms.Radical(new MathList(new Atoms.Number("3")), LaTeXSettings.PlaceholderList));
          break;
        case CgMathKeyboardInput.NthRoot:
          InsertAtom(new Atoms.Radical(LaTeXSettings.PlaceholderList, LaTeXSettings.PlaceholderList));
          break;
        case CgMathKeyboardInput.BothRoundBrackets:
          InsertInner("(", ")");
          break;
        case CgMathKeyboardInput.BothSquareBrackets:
          InsertInner("[", "]");
          break;
        case CgMathKeyboardInput.BothCurlyBrackets:
          InsertInner("{", "}");
          break;
        case CgMathKeyboardInput.Absolute:
          InsertInner("|", "|");
          break;
        case CgMathKeyboardInput.BaseEPower:
          InsertAtom(LaTeXSettings.AtomForCommand("e")
            ?? throw new InvalidCodePathException($"{nameof(LaTeXSettings.AtomForCommand)} returned null for e"));
          HandleScriptButton(true);
          break;
        case CgMathKeyboardInput.Logarithm:
          InsertSymbolName(@"\log");
          break;
        case CgMathKeyboardInput.NaturalLogarithm:
          InsertSymbolName(@"\ln");
          break;
        case CgMathKeyboardInput.LogarithmWithBase:
          InsertSymbolName(@"\log", subscript: true);
          break;
        case CgMathKeyboardInput.Sine:
          InsertSymbolName(@"\sin");
          break;
        case CgMathKeyboardInput.Cosine:
          InsertSymbolName(@"\cos");
          break;
        case CgMathKeyboardInput.Tangent:
          InsertSymbolName(@"\tan");
          break;
        case CgMathKeyboardInput.Cotangent:
          InsertSymbolName(@"\cot");
          break;
        case CgMathKeyboardInput.Secant:
          InsertSymbolName(@"\sec");
          break;
        case CgMathKeyboardInput.Cosecant:
          InsertSymbolName(@"\csc");
          break;
        case CgMathKeyboardInput.ArcSine:
          InsertSymbolName(@"\arcsin");
          break;
        case CgMathKeyboardInput.ArcCosine:
          InsertSymbolName(@"\arccos");
          break;
        case CgMathKeyboardInput.ArcTangent:
          InsertSymbolName(@"\arctan");
          break;
        case CgMathKeyboardInput.ArcCotangent:
          InsertSymbolName(@"\arccot");
          break;
        case CgMathKeyboardInput.ArcSecant:
          InsertSymbolName(@"\arcsec");
          break;
        case CgMathKeyboardInput.ArcCosecant:
          InsertSymbolName(@"\arccsc");
          break;
        case CgMathKeyboardInput.HyperbolicSine:
          InsertSymbolName(@"\sinh");
          break;
        case CgMathKeyboardInput.HyperbolicCosine:
          InsertSymbolName(@"\cosh");
          break;
        case CgMathKeyboardInput.HyperbolicTangent:
          InsertSymbolName(@"\tanh");
          break;
        case CgMathKeyboardInput.HyperbolicCotangent:
          InsertSymbolName(@"\coth");
          break;
        case CgMathKeyboardInput.HyperbolicSecant:
          InsertSymbolName(@"\sech");
          break;
        case CgMathKeyboardInput.HyperbolicCosecant:
          InsertSymbolName(@"\csch");
          break;
        case CgMathKeyboardInput.AreaHyperbolicSine:
          InsertSymbolName(@"\arsinh");
          break;
        case CgMathKeyboardInput.AreaHyperbolicCosine:
          InsertSymbolName(@"\arcosh");
          break;
        case CgMathKeyboardInput.AreaHyperbolicTangent:
          InsertSymbolName(@"\artanh");
          break;
        case CgMathKeyboardInput.AreaHyperbolicCotangent:
          InsertSymbolName(@"\arcoth");
          break;
        case CgMathKeyboardInput.AreaHyperbolicSecant:
          InsertSymbolName(@"\arsech");
          break;
        case CgMathKeyboardInput.AreaHyperbolicCosecant:
          InsertSymbolName(@"\arcsch");
          break;
        case CgMathKeyboardInput.LimitWithBase:
          InsertSymbolName(@"\lim", subscript: true);
          break;
        case CgMathKeyboardInput.Integral:
          InsertSymbolName(@"\int");
          break;
        case CgMathKeyboardInput.IntegralLowerLimit:
          InsertSymbolName(@"\int", subscript: true);
          break;
        case CgMathKeyboardInput.IntegralUpperLimit:
          InsertSymbolName(@"\int", superscript: true);
          break;
        case CgMathKeyboardInput.IntegralBothLimits:
          InsertSymbolName(@"\int", subscript: true, superscript: true);
          break;
        case CgMathKeyboardInput.Summation:
          InsertSymbolName(@"\sum");
          break;
        case CgMathKeyboardInput.SummationLowerLimit:
          InsertSymbolName(@"\sum", subscript: true);
          break;
        case CgMathKeyboardInput.SummationUpperLimit:
          InsertSymbolName(@"\sum", superscript: true);
          break;
        case CgMathKeyboardInput.SummationBothLimits:
          InsertSymbolName(@"\sum", subscript: true, superscript: true);
          break;
        case CgMathKeyboardInput.Product:
          InsertSymbolName(@"\prod");
          break;
        case CgMathKeyboardInput.ProductLowerLimit:
          InsertSymbolName(@"\prod", subscript: true);
          break;
        case CgMathKeyboardInput.ProductUpperLimit:
          InsertSymbolName(@"\prod", superscript: true);
          break;
        case CgMathKeyboardInput.ProductBothLimits:
          InsertSymbolName(@"\prod", subscript: true, superscript: true);
          break;
        case CgMathKeyboardInput.DoubleIntegral:
          InsertSymbolName(@"\iint");
          break;
        case CgMathKeyboardInput.TripleIntegral:
          InsertSymbolName(@"\iiint");
          break;
        case CgMathKeyboardInput.QuadrupleIntegral:
          InsertSymbolName(@"\iiiint");
          break;
        case CgMathKeyboardInput.ContourIntegral:
          InsertSymbolName(@"\oint");
          break;
        case CgMathKeyboardInput.DoubleContourIntegral:
          InsertSymbolName(@"\oiint");
          break;
        case CgMathKeyboardInput.TripleContourIntegral:
          InsertSymbolName(@"\oiiint");
          break;
        case CgMathKeyboardInput.ClockwiseIntegral:
          InsertSymbolName(@"\intclockwise");
          break;
        case CgMathKeyboardInput.ClockwiseContourIntegral:
          InsertSymbolName(@"\varointclockwise");
          break;
        case CgMathKeyboardInput.CounterClockwiseContourIntegral:
          InsertSymbolName(@"\ointctrclockwise");
          break;
        case CgMathKeyboardInput.LeftArrow:
          InsertSymbolName(@"\leftarrow");
          break;
        case CgMathKeyboardInput.UpArrow:
          InsertSymbolName(@"\uparrow");
          break;
        case CgMathKeyboardInput.RightArrow:
          InsertSymbolName(@"\rightarrow");
          break;
        case CgMathKeyboardInput.DownArrow:
          InsertSymbolName(@"\downarrow");
          break;
        case CgMathKeyboardInput.PartialDifferential:
          InsertSymbolName(@"\partial");
          break;
        case CgMathKeyboardInput.NotEquals:
          InsertSymbolName(@"\neq");
          break;
        case CgMathKeyboardInput.LessOrEquals:
          InsertSymbolName(@"\leq");
          break;
        case CgMathKeyboardInput.GreaterOrEquals:
          InsertSymbolName(@"\geq");
          break;
        case CgMathKeyboardInput.Multiply:
          InsertSymbolName(@"\times");
          break;
        case CgMathKeyboardInput.Divide:
          InsertSymbolName(@"\div");
          break;
        case CgMathKeyboardInput.Infinity:
          InsertSymbolName(@"\infty");
          break;
        case CgMathKeyboardInput.Degree:
          InsertSymbolName(@"\degree");
          break;
        case CgMathKeyboardInput.Angle:
          InsertSymbolName(@"\angle");
          break;
        case CgMathKeyboardInput.LeftCurlyBracket:
          InsertSymbolName(@"\{");
          break;
        case CgMathKeyboardInput.RightCurlyBracket:
          InsertSymbolName(@"\}");
          break;
        case CgMathKeyboardInput.Percentage:
          InsertSymbolName(@"\%");
          break;
        case CgMathKeyboardInput.Space:
          InsertSymbolName(@"\ ");
          break;
        case CgMathKeyboardInput.Prime:
          InsertAtom(new Atoms.Prime(1));
          break;
        case CgMathKeyboardInput.Modulo:
          InsertSymbolName(@"\%");
          break;
        case CgMathKeyboardInput.Cap:
          InsertSymbolName(@"\Cap");
          break;
        case CgMathKeyboardInput.Cup:
          InsertSymbolName(@"\cup");
          break;
        case CgMathKeyboardInput.LeftRoundBracket:
        case CgMathKeyboardInput.RightRoundBracket:
        case CgMathKeyboardInput.LeftSquareBracket:
        case CgMathKeyboardInput.RightSquareBracket:
        case CgMathKeyboardInput.D0:
        case CgMathKeyboardInput.D1:
        case CgMathKeyboardInput.D2:
        case CgMathKeyboardInput.D3:
        case CgMathKeyboardInput.D4:
        case CgMathKeyboardInput.D5:
        case CgMathKeyboardInput.D6:
        case CgMathKeyboardInput.D7:
        case CgMathKeyboardInput.D8:
        case CgMathKeyboardInput.D9:
        case CgMathKeyboardInput.Decimal:
        case CgMathKeyboardInput.Plus:
        case CgMathKeyboardInput.Minus:
        case CgMathKeyboardInput.Ratio:
        case CgMathKeyboardInput.Comma:
        case CgMathKeyboardInput.Semicolon:
        case CgMathKeyboardInput.Factorial:
        case CgMathKeyboardInput.VerticalBar:
        case CgMathKeyboardInput.LessThan:
        case CgMathKeyboardInput.GreaterThan:
          InsertAtom(LaTeXSettings.AtomForCommand(new string((char)input, 1))
                     ?? throw new InvalidCodePathException($"{nameof(LaTeXSettings.AtomForCommand)} returned null for {input}"));
          break;
        case CgMathKeyboardInput.Equals:
        {
          var currentIndex = InsertionIndex.AtomIndex-1;
          if (MathList[currentIndex] is Atoms.Relation rel)
          {
            if (rel.Nucleus == "<")
            {
              DeleteBackwards();
              InsertSymbolName(@"\leq");
              break;
            }
            if (rel.Nucleus == ">")
            {
              DeleteBackwards();
              InsertSymbolName(@"\geq");
              break;
            }
          }
          InsertAtom(LaTeXSettings.AtomForCommand("="));
        }
          break;
        case CgMathKeyboardInput.A:
        case CgMathKeyboardInput.B:
        case CgMathKeyboardInput.C:
        case CgMathKeyboardInput.D:
        case CgMathKeyboardInput.E:
        case CgMathKeyboardInput.F:
        case CgMathKeyboardInput.G:
        case CgMathKeyboardInput.H:
        case CgMathKeyboardInput.I:
        case CgMathKeyboardInput.J:
        case CgMathKeyboardInput.K:
        case CgMathKeyboardInput.L:
        case CgMathKeyboardInput.M:
        case CgMathKeyboardInput.N:
        case CgMathKeyboardInput.O:
        case CgMathKeyboardInput.P:
        case CgMathKeyboardInput.Q:
        case CgMathKeyboardInput.R:
        case CgMathKeyboardInput.S:
        case CgMathKeyboardInput.T:
        case CgMathKeyboardInput.U:
        case CgMathKeyboardInput.V:
        case CgMathKeyboardInput.W:
        case CgMathKeyboardInput.X:
        case CgMathKeyboardInput.Y:
        case CgMathKeyboardInput.Z:
        case CgMathKeyboardInput.SmallA:
        case CgMathKeyboardInput.SmallB:
        case CgMathKeyboardInput.SmallC:
        case CgMathKeyboardInput.SmallD:
        case CgMathKeyboardInput.SmallE:
        case CgMathKeyboardInput.SmallF:
        case CgMathKeyboardInput.SmallG:
        case CgMathKeyboardInput.SmallH:
        case CgMathKeyboardInput.SmallI:
        case CgMathKeyboardInput.SmallJ:
        case CgMathKeyboardInput.SmallK:
        case CgMathKeyboardInput.SmallL:
        case CgMathKeyboardInput.SmallM:
        case CgMathKeyboardInput.SmallN:
        case CgMathKeyboardInput.SmallO:
        case CgMathKeyboardInput.SmallP:
        case CgMathKeyboardInput.SmallQ:
        case CgMathKeyboardInput.SmallR:
        case CgMathKeyboardInput.SmallS:
        case CgMathKeyboardInput.SmallT:
        case CgMathKeyboardInput.SmallU:
        case CgMathKeyboardInput.SmallV:
        case CgMathKeyboardInput.SmallW:
        case CgMathKeyboardInput.SmallX:
        case CgMathKeyboardInput.SmallY:
        case CgMathKeyboardInput.SmallZ:
        {
          var atom = LaTeXSettings.AtomForCommand(new string((char)input, 1));
          InsertAtom( atom
                     ?? throw new InvalidCodePathException($"{nameof(LaTeXSettings.AtomForCommand)} returned null for {input}"));
          string funcName = "";
          var currentIndex = InsertionIndex.AtomIndex-1;
          var len = 0;
          var lastValidLen=0;
          Atoms.LargeOperator? lastValid = null;
          while (currentIndex>-1)
          {
            var currentAtom = MathList[currentIndex--];
            if (currentAtom is Atoms.Variable variable)
            {
              funcName =variable.Nucleus+funcName;
            }else if (currentAtom is Atoms.LargeOperator largeOperator && largeOperator.ForceNoLimits)
            {
              funcName =largeOperator.Nucleus+funcName;
            }
            else
              break;
            len++;
            var newAtom=LaTeXSettings.AtomForCommand('\\'+funcName) as Atoms.LargeOperator;
            if (newAtom != null)
            {
              lastValid = newAtom;
              lastValidLen=len;
            }
          }

          if (lastValid != null)
          {
              for(var i=0;i<lastValidLen;i++)
                DeleteBackwards();
              InsertAtom(lastValid);
          }
        }
          break;
        case CgMathKeyboardInput.Alpha:
        case CgMathKeyboardInput.Beta:
        case CgMathKeyboardInput.Gamma:
        case CgMathKeyboardInput.Delta:
        case CgMathKeyboardInput.Epsilon:
        case CgMathKeyboardInput.Zeta:
        case CgMathKeyboardInput.Eta:
        case CgMathKeyboardInput.Theta:
        case CgMathKeyboardInput.Iota:
        case CgMathKeyboardInput.Kappa:
        case CgMathKeyboardInput.Lambda:
        case CgMathKeyboardInput.Mu:
        case CgMathKeyboardInput.Nu:
        case CgMathKeyboardInput.Xi:
        case CgMathKeyboardInput.Omicron:
        case CgMathKeyboardInput.Pi:
        case CgMathKeyboardInput.Rho:
        case CgMathKeyboardInput.Sigma:
        case CgMathKeyboardInput.Tau:
        case CgMathKeyboardInput.Upsilon:
        case CgMathKeyboardInput.Phi:
        case CgMathKeyboardInput.Chi:
        case CgMathKeyboardInput.Psi:
        case CgMathKeyboardInput.Omega:
        case CgMathKeyboardInput.SmallAlpha:
        case CgMathKeyboardInput.SmallBeta:
        case CgMathKeyboardInput.SmallGamma:
        case CgMathKeyboardInput.SmallDelta:
        case CgMathKeyboardInput.SmallEpsilon:
        case CgMathKeyboardInput.SmallEpsilon2:
        case CgMathKeyboardInput.SmallZeta:
        case CgMathKeyboardInput.SmallEta:
        case CgMathKeyboardInput.SmallTheta:
        case CgMathKeyboardInput.SmallIota:
        case CgMathKeyboardInput.SmallKappa:
        case CgMathKeyboardInput.SmallKappa2:
        case CgMathKeyboardInput.SmallLambda:
        case CgMathKeyboardInput.SmallMu:
        case CgMathKeyboardInput.SmallNu:
        case CgMathKeyboardInput.SmallXi:
        case CgMathKeyboardInput.SmallOmicron:
        case CgMathKeyboardInput.SmallPi:
        case CgMathKeyboardInput.SmallPi2:
        case CgMathKeyboardInput.SmallRho:
        case CgMathKeyboardInput.SmallRho2:
        case CgMathKeyboardInput.SmallSigma:
        case CgMathKeyboardInput.SmallSigma2:
        case CgMathKeyboardInput.SmallTau:
        case CgMathKeyboardInput.SmallUpsilon:
        case CgMathKeyboardInput.SmallPhi:
        case CgMathKeyboardInput.SmallPhi2:
        case CgMathKeyboardInput.SmallChi:
        case CgMathKeyboardInput.SmallPsi:
        case CgMathKeyboardInput.SmallOmega:
          // All Greek letters are rendered as variables.
          InsertAtom(new Atoms.Variable(((char)input).ToStringInvariant()));
          break;
        default:
          break;
      }
      ResetPlaceholders(MathList);
      InsertionPositionHighlighted = true;
    }

    public void MoveCaretToPoint(PointF point) {
      point.Y *= -1; //inverted canvas, blah blah
      InsertionIndex = ClosestIndexToPoint(point) ?? MathListIndex.Level0Index(MathList.Atoms.Count);
    }

    public void Clear() {
      MathList.Clear();
      InsertionIndex = MathListIndex.Level0Index(0);
    }

    // Insert a list at a given point.
    public void InsertMathList(MathList list, PointF point) {
      var detailedIndex = ClosestIndexToPoint(point) ?? MathListIndex.Level0Index(0);
      // insert at the given index - but don't consider sublevels at this point
      var index = MathListIndex.Level0Index(detailedIndex.AtomIndex);
      foreach (var atom in list.Atoms) {
        MathList.InsertAndAdvance(ref index, atom, MathListSubIndexType.None);
      }
      InsertionIndex = index; // move the index to the end of the new list.
    }

    public void HighlightCharacterAt(MathListIndex index, Color color) {
      // setup highlights before drawing the MTLine
      Display?.HighlightCharacterAt(index, color);
      RedrawRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ClearHighlights() {
      RecreateDisplayFromMathList();
      RedrawRequested?.Invoke(this, EventArgs.Empty);
    }
    public void Dispose() {
      ((IDisposable)blinkTimer).Dispose();
    }
    public RectangleF Measure =>
      Display != null ? new RectangleF(0, -Display.Ascent, Display.Width, Display.Ascent + Display.Descent) : RectangleF.Empty;
    public void DrawCaret(ICanvas canvas, Color color, CaretShape shape) {
      if (Display == null)
        return;
      var cursorPosition = Display.PointForIndex(TypesettingContext.Instance, InsertionIndex) ?? Display.Position;
      cursorPosition.Y *= -1; //inverted canvas, blah blah
      using var path = canvas.StartNewPath();
      path.Foreground = color;
      path.MoveTo(cursorPosition.X, cursorPosition.Y);
      switch (shape) {
        case CaretShape.IBeam:
          ReadOnlySpan<PointF> s = stackalloc PointF[4] {
            new PointF(Font.PointSize / 2 / 16, 0),
            new PointF(Font.PointSize / 2 / 16, -Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 16, -Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 16, 0),
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
        case CaretShape.UpArrow:
          s = stackalloc PointF[4] {
            new PointF(Font.PointSize / 2 / 2, Font.PointSize * 2 / 3 / 4),
            new PointF(Font.PointSize / 2 / 2, Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 2, Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 2, Font.PointSize * 2 / 3 / 4)
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
      }
      path.CloseContour();
    }
    
  }
}
