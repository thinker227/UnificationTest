using System;
using System.Linq;

namespace UnificationTest;

public sealed class Unification
{
    public static void Unify(IType a, IType b)
    {
        if (a is TypeVariable ta) a = ta.Substitution;
        if (b is TypeVariable tb) b = tb.Substitution;

        switch ((a, b))
        {
            case (TypeVariable va, TypeVariable vb):
                if (va.Equals(vb))
                {
                    throw new InvalidOperationException($"Cannot unify type '{va}' with itself");
                }

                va.Substitute(vb);

                break;

            case (TypeVariable v, _):
                v.Substitute(b);
                break;

            case (_, TypeVariable v):
                v.Substitute(a);
                break;

            case (NameType na, NameType nb):
                if (!na.Equals(nb))
                {
                    throw new InvalidOperationException($"Cannot unify types '{na}' and '{nb}'");
                }

                break;

            case (FunctionType fa, FunctionType fb):
                if (fa.Params.Count != fb.Params.Count)
                {
                    throw new InvalidOperationException($"Cannot unify types '{fa}' and '{fb}' because of differing arity");
                }

                foreach (var (pa, pb) in fa.Params.Zip(fb.Params))
                {
                    Unify(pa, pb);
                }

                Unify(fa.Return, fb.Return);

                break;

            default: throw new InvalidOperationException();
        }
    }
}
